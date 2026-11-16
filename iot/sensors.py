"""Modelos dos dois sensores físicos da estação de monitoramento."""
import random
from dataclasses import dataclass


@dataclass(frozen=True)
class UltrasonicLevelSensor:
    """Sensor ultrassônico (tipo HC-SR04) instalado acima da rua, apontado para o chão.

    Ele NÃO mede o nível da água: mede a DISTÂNCIA até a superfície mais próxima.
    O nível é calculado assim: altura de instalação - distância medida.
    """

    mount_height_cm: float = 200.0
    noise_cm: float = 0.5
    min_range_cm: float = 2.0
    max_range_cm: float = 400.0
    echo_loss_probability: float = 0.03  

    def measure_distance_cm(self, surface_height_cm: float) -> float:

        if surface_height_cm > 5 and random.random() < self.echo_loss_probability:
            return round(self.mount_height_cm + random.uniform(-2, 2), 1)
        
        distance = self.mount_height_cm - surface_height_cm + random.gauss(0, self.noise_cm)
        
        return round(min(max(distance, self.min_range_cm), self.max_range_cm), 1)

    def to_water_level_cm(self, distance_cm: float) -> float:
        return round(max(0.0, self.mount_height_cm - distance_cm), 2)


@dataclass(frozen=True)
class RainGauge:
    """Pluviômetro: mede a intensidade da chuva em mm/h."""

    relative_noise: float = 0.1

    def measure_mm_per_hour(self, true_rainfall_mm_per_hour: float) -> float:
        variation = random.uniform(-self.relative_noise, self.relative_noise)
        return round(max(0.0, true_rainfall_mm_per_hour * (1 + variation)), 1)