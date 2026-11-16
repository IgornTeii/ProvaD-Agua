"""Cenários: o que REALMENTE acontece na rua, momento a momento.

Aqui não há ruído de sensor. Isso é responsabilidade dos sensores (sensors.py).
Cada cenário descreve só o mundo físico: altura da água, chuva e obstruções.
"""
import random
from dataclasses import dataclass
from typing import Callable, Iterator


@dataclass(frozen=True)
class StreetConditions:
    water_level_cm: float
    rainfall_mm_per_hour: float
    obstruction_height_cm: float = 0.0

    @property
    def surface_height_cm(self) -> float:
        """A superfície que o ultrassônico "enxerga": a água ou o que estiver acima dela."""
        return max(self.water_level_cm, self.obstruction_height_cm)


DRY_STREET_CM = 0.5


def normal(steps: int = 60) -> Iterator[StreetConditions]:
    """Dia comum: rua seca, pouca ou nenhuma chuva."""
    for _ in range(steps):
        yield StreetConditions(DRY_STREET_CM, random.uniform(0, 1))


def light_rain(steps: int = 90) -> Iterator[StreetConditions]:
    """Chuva fraca: o nível sobe devagar, mas não chega a alagar."""
    level = DRY_STREET_CM
    for _ in range(steps):
        level = min(10.0, level + random.uniform(0, 0.2))
        yield StreetConditions(level, random.uniform(5, 15))


def flood(rise_steps: int = 60, peak_steps: int = 30, recede_steps: int = 60,
          peak_cm: float = 55.0,
          rainfall_range: tuple[float, float] = (35, 55),
          dry_steps: int = 30) -> Iterator[StreetConditions]:
    """Alagamento real: chuva forte, a água sobe aos poucos, fica no pico e baixa."""
    for i in range(rise_steps):
        progress = (i + 1) / rise_steps
        level = DRY_STREET_CM + (peak_cm - DRY_STREET_CM) * progress
        yield StreetConditions(level, random.uniform(*rainfall_range))

    for _ in range(peak_steps):
        yield StreetConditions(peak_cm + random.uniform(-1.5, 1.5), random.uniform(*rainfall_range))

    for i in range(recede_steps):
        progress = (i + 1) / recede_steps
        level = peak_cm - (peak_cm - DRY_STREET_CM) * progress
        yield StreetConditions(level, max(0.0, 20 * (1 - progress)))

    for _ in range(dry_steps):
        yield StreetConditions(DRY_STREET_CM, random.uniform(0, 2))


def splash(steps: int = 60, splash_at: int = 30, splash_steps: int = 2,
           rainfall_range: tuple[float, float] = (0, 1)) -> Iterator[StreetConditions]:
    """Ruído: um carro joga água na frente do sensor por poucas leituras."""
    for i in range(steps):
        is_splash = splash_at <= i < splash_at + splash_steps
        obstruction = random.uniform(18, 50) if is_splash else 0.0
        yield StreetConditions(DRY_STREET_CM, random.uniform(*rainfall_range), obstruction)


SCENARIOS: dict[str, Callable[[], Iterator[StreetConditions]]] = {
    "normal": normal,
    "light_rain": light_rain,
    "flood": flood,
    "splash": splash,
}