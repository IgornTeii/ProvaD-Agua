"""Transforma uma janela de leituras em características (features) que o modelo entende.

O modelo nunca olha uma leitura isolada. Ele olha o COMPORTAMENTO da janela:
como o nível evolui, quanto tempo fica alto e se a chuva acompanha.
"""
import numpy as np

SUSPICION_THRESHOLD_CM = 15.0

FEATURE_NAMES = [
    "max_level_cm",
    "mean_level_cm",
    "level_std_cm",
    "level_slope_cm_per_step",
    "max_jump_cm",
    "fraction_above_threshold",
    "longest_run_above_threshold",
    "mean_rainfall_mm_per_hour",
]


def extract_features(levels: list[float], rainfalls: list[float]) -> list[float]:
    level = np.asarray(levels, dtype=float)
    rain = np.asarray(rainfalls, dtype=float)

    steps = np.arange(len(level))
    slope = float(np.polyfit(steps, level, 1)[0]) if len(level) > 1 else 0.0
    max_jump = float(np.max(np.abs(np.diff(level)))) if len(level) > 1 else 0.0

    above = level >= SUSPICION_THRESHOLD_CM
    longest_run = current = 0
    for is_above in above:
        current = current + 1 if is_above else 0
        longest_run = max(longest_run, current)

    return [
        float(level.max()),
        float(level.mean()),
        float(level.std()),
        slope,
        max_jump,
        float(above.mean()),
        float(longest_run),
        float(rain.mean()),
    ]