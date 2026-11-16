import random

import pytest

import scenarios
from presets import PRESETS, PRESETS_BY_ID

SUSPICION_CM = 15.0


@pytest.fixture(autouse=True)
def fixed_seed():
    random.seed(42)


def levels(preset_id: str) -> list[float]:
    return [c.surface_height_cm for c in PRESETS_BY_ID[preset_id].build()]


def test_every_preset_has_a_unique_id_and_texts():
    assert len(PRESETS_BY_ID) == len(PRESETS)
    for preset in PRESETS:
        assert preset.name and preset.description and preset.expected


@pytest.mark.parametrize("preset_id", ["flood", "flash_flood", "upstream_flood"])
def test_floods_rise_above_suspicion_and_end_dry(preset_id):
    readings = levels(preset_id)

    assert max(readings) >= SUSPICION_CM
    assert all(level <= 1 for level in readings[-18:])


@pytest.mark.parametrize("preset_id", ["splash", "obstruction"])
def test_noise_crosses_suspicion_only_for_a_short_time(preset_id):
    readings = levels(preset_id)
    above = sum(level >= SUSPICION_CM for level in readings)

    assert 0 < above <= 12


def test_light_rain_never_reaches_suspicion():
    assert max(levels("light_rain")) < SUSPICION_CM


def test_presets_build_a_new_sequence_each_time():
    preset = PRESETS_BY_ID["flood"]

    assert list(preset.build()) != list(preset.build())


def test_flood_keeps_dry_tail_for_the_api_to_close_the_event():
    tail = list(scenarios.flood(dry_steps=30))[-30:]

    assert all(c.water_level_cm == scenarios.DRY_STREET_CM for c in tail)
