import argparse
import json
import sys
import time
import urllib.error
import urllib.request

SENSOR_COUNT = 8


def call(url: str, body: dict | None = None) -> tuple[int, object]:
    data = json.dumps(body).encode() if body is not None else None
    request = urllib.request.Request(url, data=data, headers={"Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(request, timeout=10) as response:
            raw = response.read()
            return response.status, json.loads(raw) if raw and "json" in response.headers.get("Content-Type", "") else raw
    except urllib.error.HTTPError as error:
        return error.code, None
    except (urllib.error.URLError, ConnectionError, TimeoutError):
        return 0, None


def wait_for(description: str, check, timeout: int, every: float = 2):
    deadline = time.monotonic() + timeout
    while time.monotonic() < deadline:
        result = check()
        if result:
            print(f"  ok  {description}")
            return result
        time.sleep(every)
    print(f"  FALHOU  {description} (esperou {timeout}s)")
    sys.exit(1)


def events_of(api: str, sensor_id: str) -> list[dict]:
    status, events = call(f"{api}/flood-events?sensorId={sensor_id}")
    return events if status == 200 else []


def new_event(api: str, sensor_id: str, known: set[str], *expected: str):
    for event in events_of(api, sensor_id):
        if event["id"] not in known and event["status"] in expected:
            return event
    return None


def check_services(api: str, simulator: str, site: str | None, timeout: int) -> None:
    print("Serviços")
    if site:
        wait_for("site responde", lambda: call(site)[0] == 200, timeout)
    sensors = wait_for("API responde com os sensores", lambda: (lambda r: r[0] == 200 and r[1])(call(f"{api}/sensors")), timeout)
    if len(sensors) != SENSOR_COUNT:
        print(f"  FALHOU  esperava {SENSOR_COUNT} sensores, vieram {len(sensors)}")
        sys.exit(1)
    wait_for("simulador lista os cenários", lambda: call(f"{simulator}/scenarios")[0] == 200, timeout)


def run_full_flow(api: str, simulator: str, timeout: int) -> None:
    known = {e["id"] for sensor in ("sensor-01", "sensor-02") for e in events_of(api, sensor)}

    print("Alagamento de verdade no sensor-01")
    status, _ = call(f"{simulator}/simulations", {"scenarioId": "flood", "sensorId": "sensor-01"})
    assert status == 202, f"simulador recusou o alagamento ({status})"

    print("Carro jogando água no sensor-02")
    status, _ = call(f"{simulator}/simulations", {"scenarioId": "splash", "sensorId": "sensor-02"})
    assert status == 202, f"simulador recusou o splash ({status})"

    wait_for("IA descartou o splash", lambda: new_event(api, "sensor-02", known, "Dismissed"), timeout)
    wait_for("IA confirmou o alagamento", lambda: new_event(api, "sensor-01", known, "Confirmed", "Closed"), timeout)
    event = wait_for("alagamento encerrado quando a água baixou", lambda: new_event(api, "sensor-01", known, "Closed"), timeout)

    certificates = wait_for(
        "comprovações emitidas para os endereços",
        lambda: (lambda r: r[0] == 200 and r[1])(call(f"{api}/certificates?floodEventId={event['id']}")),
        timeout,
    )
    code = certificates[0]["verificationCode"]
    status, verified = call(f"{api}/certificates/{code}/verify")
    if status != 200 or verified["verificationCode"] != code:
        print(f"  FALHOU  código {code} não foi verificado")
        sys.exit(1)
    print(f"  ok  código {code} verificado ({len(certificates)} comprovações)")

    if call(f"{api}/certificates/PDA-0000-0000/verify")[0] != 404:
        print("  FALHOU  código inexistente deveria dar 404")
        sys.exit(1)
    print("  ok  código falso é recusado")


def main() -> None:
    parser = argparse.ArgumentParser(description="Teste de fumaça do Prova d'Água de ponta a ponta")
    parser.add_argument("--api", default="http://localhost:5016")
    parser.add_argument("--simulator", default="http://localhost:8100")
    parser.add_argument("--site")
    parser.add_argument("--quick", action="store_true", help="só confere se os serviços estão de pé")
    parser.add_argument("--timeout", type=int, default=240)
    args = parser.parse_args()

    check_services(args.api, args.simulator, args.site, args.timeout)
    if not args.quick:
        run_full_flow(args.api, args.simulator, args.timeout)
    print("Tudo certo.")


if __name__ == "__main__":
    main()
