import json
import threading
import time
import urllib.error
import urllib.request
from http.server import ThreadingHTTPServer

import pytest

import server


@pytest.fixture(autouse=True)
def fake_api(monkeypatch):
    sent = []
    monkeypatch.setattr(server, "INTERVAL_SECONDS", 0)
    monkeypatch.setattr(server, "send_reading", lambda *args: sent.append(args) or "202")
    server.simulations.clear()
    return sent


@pytest.fixture
def base_url():
    httpd = ThreadingHTTPServer(("127.0.0.1", 0), server.Handler)
    threading.Thread(target=httpd.serve_forever, daemon=True).start()
    yield f"http://127.0.0.1:{httpd.server_port}"
    httpd.shutdown()


def call(url: str, body: dict | None = None) -> tuple[int, dict]:
    data = json.dumps(body).encode() if body is not None else None
    request = urllib.request.Request(url, data=data, headers={"Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(request, timeout=5) as response:
            return response.status, json.loads(response.read())
    except urllib.error.HTTPError as error:
        return error.code, json.loads(error.read())


def wait_until_done(simulation_id: str) -> server.Simulation:
    for _ in range(200):
        simulation = server.simulations[simulation_id]
        if simulation.status != "running":
            return simulation
        time.sleep(0.01)
    raise AssertionError("a simulação não terminou")


def test_rejects_unknown_scenario():
    status, body = server.start("tsunami", "sensor-01")

    assert status == 400
    assert "Cenário" in body["error"]


@pytest.mark.parametrize("sensor_id", ["sensor-1", "sensor-001", "../sensor-01", ""])
def test_rejects_invalid_sensor_id(sensor_id):
    status, _ = server.start("flood", sensor_id)

    assert status == 400


def test_runs_scenario_and_sends_every_reading(fake_api):
    status, body = server.start("splash", "sensor-02")

    simulation = wait_until_done(body["id"])

    assert status == 202
    assert simulation.status == "finished"
    assert simulation.sent == simulation.total == len(fake_api)
    assert fake_api[0][1] == "dev-key-sensor-02"


def test_blocks_two_simulations_on_the_same_sensor(monkeypatch):
    release = threading.Event()
    monkeypatch.setattr(server, "send_reading", lambda *args: release.wait(2) and "202")

    first, _ = server.start("flood", "sensor-03")
    second, body = server.start("splash", "sensor-03")
    other_sensor, _ = server.start("splash", "sensor-04")
    release.set()

    assert (first, second, other_sensor) == (202, 409, 202)
    assert "andamento" in body["error"]


def test_marks_simulation_failed_when_api_rejects_everything(monkeypatch):
    monkeypatch.setattr(server, "send_reading", lambda *args: "401 chave inválida")

    _, body = server.start("splash", "sensor-05")
    simulation = wait_until_done(body["id"])

    assert simulation.status == "failed"
    assert simulation.last_error == "401 chave inválida"


def test_http_health_and_scenarios(base_url):
    assert call(f"{base_url}/health") == (200, {"status": "ok"})

    status, scenarios = call(f"{base_url}/scenarios")
    assert status == 200
    assert {s["id"] for s in scenarios} >= {"flood", "splash", "light_rain"}


def test_http_start_and_follow_simulation(base_url):
    status, created = call(f"{base_url}/simulations", {"scenarioId": "light_rain", "sensorId": "sensor-06"})
    assert status == 202

    wait_until_done(created["id"])
    status, simulation = call(f"{base_url}/simulations/{created['id']}")

    assert status == 200
    assert simulation["status"] == "finished"
    assert len(simulation["readings"]) == simulation["total"]


def test_http_unknown_routes_return_404(base_url):
    assert call(f"{base_url}/nada")[0] == 404
    assert call(f"{base_url}/simulations/inexistente")[0] == 404
