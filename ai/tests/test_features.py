import pytest

from features import FEATURE_NAMES, extract_features


def as_dict(levels, rains):
    return dict(zip(FEATURE_NAMES, extract_features(levels, rains)))


def test_returns_one_value_per_feature_name():
    assert len(extract_features([1, 2, 3], [0, 0, 0])) == len(FEATURE_NAMES)


def test_dry_street_has_nothing_above_threshold():
    features = as_dict([0.5] * 30, [0] * 30)

    assert features["fraction_above_threshold"] == 0
    assert features["longest_run_above_threshold"] == 0
    assert features["max_level_cm"] == pytest.approx(0.5)


def test_rising_water_has_positive_slope_and_long_run():
    levels = [float(i) for i in range(30)]
    features = as_dict(levels, [40] * 30)

    assert features["level_slope_cm_per_step"] == pytest.approx(1.0)
    assert features["longest_run_above_threshold"] == 15
    assert features["mean_rainfall_mm_per_hour"] == 40


def test_splash_shows_as_a_big_jump():
    levels = [0.5] * 10 + [40.0] + [0.5] * 10

    assert as_dict(levels, [0] * 21)["max_jump_cm"] == pytest.approx(39.5)


def test_single_reading_does_not_break():
    features = as_dict([20.0], [10.0])

    assert features["level_slope_cm_per_step"] == 0
    assert features["max_jump_cm"] == 0
