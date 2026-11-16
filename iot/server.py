import json
import os
import re
import threading
import time
import uuid
from dataclasses import dataclass, field
from datetime import datetime, timezone
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

from main import MonitoringStation, send_reading
from presets import PRESETS, PRESETS_BY_ID
from sensors import RainGauge, UltrasonicLevelSensor

API_URL = os.getenv("API_URL", "http://localhost:5016")
PORT = int(os.getenv("PORT", "8100"))
INTERVAL_SECONDS = float(os.getenv("INTERVAL_SECONDS", "0.3"))

SENSOR_ID = re.compile(r"^sensor-\d{2}$")


@dataclass
class Simulation:
    id: str
    scenario_id: str
    sensor_id: str
    total: int
    started_at: str
    status: str = "running"
    sent: int = 0
    rejected: int = 0
    last_error: str | None = None
    readings: list[dict] = field(default_factory=list)

    def to_json(self) -> dict:
        return {
            "id": self.id,
            "scenarioId": self.scenario_id,
            "sensorId": self.sensor_id,
            "total": self.total,
            "startedAt": self.started_at,
            "status": self.status,
            "sent": self.sent,
            "rejected": self.rejected,
            "lastError": self.last_error,
            "readings": self.readings,
        }


simulations: dict[str, Simulation] = {}
lock = threading.Lock()


def running_on(sensor_id: str) -> Simulation | None:
    return next((s for s in simulations.values() if s.sensor_id == sensor_id and s.status == "running"), None)


def run(simulation: Simulation, conditions: list) -> None:
    station = MonitoringStation(UltrasonicLevelSensor(), RainGauge())
    sensor_key = f"dev-key-{simulation.sensor_id}"

    for street in conditions:
        measurement = station.measure(street)
        result = send_reading(API_URL, sensor_key, measurement, datetime.now(timezone.utc))

        with lock:
            simulation.sent += 1
            simulation.readings.append({
                "waterLevelCm": measurement.water_level_cm,
                "rainfallMmPerHour": measurement.rainfall_mm_per_hour,
            })
            if result != "202":
                simulation.rejected += 1
                simulation.last_error = result

        time.sleep(INTERVAL_SECONDS)

    with lock:
        simulation.status = "failed" if simulation.rejected == simulation.total else "finished"


def start(scenario_id: str, sensor_id: str) -> tuple[int, dict]:
    preset = PRESETS_BY_ID.get(scenario_id)
    if preset is None:
        return 400, {"error": "Cenário desconhecido."}
    if not SENSOR_ID.match(sensor_id):
        return 400, {"error": "Sensor desconhecido."}

    with lock:
        if running_on(sensor_id):
            return 409, {"error": "Já existe uma simulação em andamento neste sensor. Aguarde ela terminar."}

        conditions = list(preset.build())
        simulation = Simulation(
            id=uuid.uuid4().hex[:8],
            scenario_id=scenario_id,
            sensor_id=sensor_id,
            total=len(conditions),
            started_at=datetime.now(timezone.utc).isoformat(),
        )
        simulations[simulation.id] = simulation

    threading.Thread(target=run, args=(simulation, conditions), daemon=True).start()
    return 202, simulation.to_json()


class Handler(BaseHTTPRequestHandler):
    def log_message(self, *args) -> None:
        pass

    def send_json(self, status: int, body) -> None:
        data = json.dumps(body, ensure_ascii=False).encode()
        self.send_response(status)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Access-Control-Allow-Origin", "*")
        self.end_headers()
        self.wfile.write(data)

    def do_OPTIONS(self) -> None:
        self.send_response(204)
        self.send_header("Access-Control-Allow-Origin", "*")
        self.send_header("Access-Control-Allow-Methods", "GET, POST, OPTIONS")
        self.send_header("Access-Control-Allow-Headers", "Content-Type")
        self.end_headers()

    def do_GET(self) -> None:
        if self.path == "/health":
            return self.send_json(200, {"status": "ok"})

        if self.path == "/scenarios":
            return self.send_json(200, [
                {"id": p.id, "name": p.name, "description": p.description, "expected": p.expected}
                for p in PRESETS
            ])

        if self.path.startswith("/simulations/"):
            simulation = simulations.get(self.path.removeprefix("/simulations/"))
            if simulation is None:
                return self.send_json(404, {"error": "Simulação não encontrada."})
            with lock:
                return self.send_json(200, simulation.to_json())

        self.send_json(404, {"error": "Rota não encontrada."})

    def do_POST(self) -> None:
        if self.path != "/simulations":
            return self.send_json(404, {"error": "Rota não encontrada."})

        try:
            length = int(self.headers.get("Content-Length", 0))
            body = json.loads(self.rfile.read(length) or b"{}")
        except (ValueError, json.JSONDecodeError):
            return self.send_json(400, {"error": "Corpo da requisição inválido."})

        status, response = start(str(body.get("scenarioId", "")), str(body.get("sensorId", "")))
        self.send_json(status, response)


if __name__ == "__main__":
    print(f"Simulador ouvindo na porta {PORT}, enviando leituras para {API_URL}")
    ThreadingHTTPServer(("0.0.0.0", PORT), Handler).serve_forever()
