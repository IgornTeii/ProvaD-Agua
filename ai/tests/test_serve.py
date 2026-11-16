import random

import pytest

from generate_dataset import flood_window, noise_window
from main import MonitoringStation
from sensors import RainGauge, UltrasonicLevelSensor


def payload(window):
    return {"readings": [{"waterLevelCm": level, "rainfallMmPerHour": rain} for level, rain in window]}


def windows(build, count=40):
    random.seed(123)
    station = MonitoringStation(UltrasonicLevelSensor(), RainGauge())
    return [build(station) for _ in range(count)]


def labels(client, build):
    return [client.post("/classify", json=payload(w)).get_json()["label"] for w in windows(build)]


def test_health(client):
    assert client.get("/health").get_json() == {"status": "ok"}


@pytest.mark.parametrize("body", [None, {}, {"readings": "x"}, {"readings": [{"waterLevelCm": 1}]}, {"readings": [{"waterLevelCm": "a", "rainfallMmPerHour": 1}]}])
def test_rejects_invalid_body(client, body):
    response = client.post("/classify", json=body)

    assert response.status_code == 400


def test_few_readings_are_inconclusive(client):
    body = client.post("/classify", json=payload([(30, 40)] * 5)).get_json()

    assert body["label"] == "inconclusive"
    assert body["confidence"] is None


def test_confirms_real_floods(client):
    result = labels(client, flood_window)

    assert result.count("flood") >= 36
    assert "noise" not in result


def test_dismisses_splashes_and_obstructions(client):
    result = labels(client, noise_window)

    assert result.count("noise") >= 36
    assert "flood" not in result


def test_confident_answers_respect_the_threshold(client):
    for window in windows(flood_window, 10):
        body = client.post("/classify", json=payload(window)).get_json()
        if body["label"] != "inconclusive":
            assert body["confidence"] >= 0.8
