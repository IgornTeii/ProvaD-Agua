"""Estação de monitoramento simulada: mede a rua com 2 sensores e envia para a API."""
import argparse
import json
import os
import time
import urllib.error
import urllib.request
from dataclasses import dataclass
from datetime import datetime, timedelta, timezone

from scenarios import SCENARIOS, StreetConditions
from sensors import RainGauge, UltrasonicLevelSensor

SIMULATED_STEP = timedelta(seconds=10)


@dataclass(frozen=True)
class Measurement:
    distance_cm: float
    water_level_cm: float
    rainfall_mm_per_hour: float


@dataclass(frozen=True)
class MonitoringStation:
    """Junta os dois sensores, como o microcontrolador (ex.: ESP32) faria no poste."""

    level_sensor: UltrasonicLevelSensor
    rain_gauge: RainGauge

    def measure(self, street: StreetConditions) -> Measurement:
        distance = self.level_sensor.measure_distance_cm(street.surface_height_cm)
        return Measurement(
            distance_cm=distance,
            water_level_cm=self.level_sensor.to_water_level_cm(distance),
            rainfall_mm_per_hour=self.rain_gauge.measure_mm_per_hour(street.rainfall_mm_per_hour),
        )


def send_reading(api_url: str, sensor_key: str, measurement: Measurement, measured_at: datetime) -> str:
    body = json.dumps({
        "waterLevelCm": measurement.water_level_cm,
        "rainfallMmPerHour": measurement.rainfall_mm_per_hour,
        "measuredAt": measured_at.isoformat(),
    }).encode()

    request = urllib.request.Request(
        f"{api_url}/readings",
        data=body,
        method="POST",
        headers={"Content-Type": "application/json", "X-Sensor-Key": sensor_key},
    )

    try:
        with urllib.request.urlopen(request, timeout=5) as response:
            return str(response.status)
    except urllib.error.HTTPError as error:
        return f"{error.code} {error.read().decode()[:120]}"
    except urllib.error.URLError as error:
        return f"API indisponível ({error.reason})"


def main() -> None:
    parser = argparse.ArgumentParser(description="Estação de monitoramento simulada do Prova d'Água")
    parser.add_argument("scenario", choices=SCENARIOS.keys())
    parser.add_argument("--api-url", default=os.getenv("API_URL", "http://localhost:5016"))
    parser.add_argument("--sensor-key", default=os.getenv("SENSOR_KEY", "dev-key-sensor-01"))
    parser.add_argument("--mount-height", type=float, default=200.0,
                        help="altura de instalação do sensor ultrassônico, em cm")
    parser.add_argument("--interval", type=float, default=0.5,
                        help="segundos reais entre um envio e outro")
    parser.add_argument("--dry-run", action="store_true",
                        help="só mostra as leituras, sem enviar para a API")
    args = parser.parse_args()

    station = MonitoringStation(UltrasonicLevelSensor(mount_height_cm=args.mount_height), RainGauge())
    conditions = list(SCENARIOS[args.scenario]())

    # Replay: o cenário termina "agora", então todas as leituras ficam no passado
    measured_at = datetime.now(timezone.utc) - SIMULATED_STEP * len(conditions)

    print(f"Cenário '{args.scenario}': {len(conditions)} leituras, "
          f"{len(conditions) * SIMULATED_STEP.seconds // 60} min simulados")

    for step, street in enumerate(conditions, start=1):
        measurement = station.measure(street)
        status = "dry-run" if args.dry_run else send_reading(
            args.api_url, args.sensor_key, measurement, measured_at)

        print(f"[{step:03}] {measured_at:%H:%M:%S}  "
              f"distância={measurement.distance_cm:6.1f} cm  "
              f"nível={measurement.water_level_cm:6.2f} cm  "
              f"chuva={measurement.rainfall_mm_per_hour:5.1f} mm/h  ->  {status}")

        measured_at += SIMULATED_STEP
        if not args.dry_run:
            time.sleep(args.interval)


if __name__ == "__main__":
    main()