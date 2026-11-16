import random

import pytest

from main import MonitoringStation
from scenarios import StreetConditions
from sensors import RainGauge, UltrasonicLevelSensor


@pytest.fixture(autouse=True)
def fixed_seed():
    random.seed(7)


def test_ultrasonic_converts_distance_back_to_water_level():
    sensor = UltrasonicLevelSensor(noise_cm=0, echo_loss_probability=0)

    distance = sensor.measure_distance_cm(30)

    assert distance == 170
    assert sensor.to_water_level_cm(distance) == 30


def test_water_level_is_never_negative():
    sensor = UltrasonicLevelSensor()

    assert sensor.to_water_level_cm(250) == 0


def test_lost_echo_reads_as_dry_street():
    sensor = UltrasonicLevelSensor(noise_cm=0, echo_loss_probability=1)

    level = sensor.to_water_level_cm(sensor.measure_distance_cm(40))

    assert level < 3


def test_rain_gauge_stays_close_to_real_rain_and_never_negative():
    gauge = RainGauge()

    readings = [gauge.measure_mm_per_hour(50) for _ in range(200)]

    assert all(45 <= r <= 55 for r in readings)
    assert gauge.measure_mm_per_hour(0) == 0


def test_obstruction_is_seen_as_water_by_the_station():
    station = MonitoringStation(UltrasonicLevelSensor(noise_cm=0, echo_loss_probability=0), RainGauge())

    measurement = station.measure(StreetConditions(0.5, 0, obstruction_height_cm=35))

    assert measurement.water_level_cm == 35
