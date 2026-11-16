"""Gera o conjunto de treino da IA: janelas de leituras rotuladas como 'flood' ou 'noise'.

Cada janela simula o que a API enviaria à IA quando um evento Suspeito existe:
as últimas leituras de um sensor depois que o nível passou da régua de suspeita.
O resultado vai para a camada BRUTA de dados (data/raw).
"""
import argparse
import csv
import random
from pathlib import Path

import scenarios
from main import MonitoringStation
from sensors import RainGauge, UltrasonicLevelSensor

SUSPICION_THRESHOLD_CM = 15.0
WINDOW_SIZE = 30


def _measure(station: MonitoringStation, conditions) -> list[tuple[float, float]]:
    readings = []
    for street in conditions:
        m = station.measure(street)
        readings.append((m.water_level_cm, m.rainfall_mm_per_hour))
    return readings


def _window_ending_at(readings: list[tuple[float, float]], end: int) -> list[tuple[float, float]]:
    return readings[max(0, end - WINDOW_SIZE):end]


def flood_window(station: MonitoringStation) -> list[tuple[float, float]]:
    """Um alagamento real, com intensidade, velocidade e chuva variadas.

    Inclui casos difíceis: enxurrada que sobe em poucos minutos e
    alagamento sem chuva local (a água vem de outro bairro, rio acima).
    """
    kind = random.choices(["heavy_rain", "moderate_rain", "upstream", "flash"],
                          weights=[50, 20, 15, 15])[0]
    rainfall_range = {"heavy_rain": (35, 60), "moderate_rain": (12, 30),
                      "upstream": (0, 5), "flash": (40, 80)}[kind]
    rise_steps = random.randint(2, 8) if kind == "flash" else random.randint(20, 90)

    conditions = scenarios.flood(
        rise_steps=rise_steps,
        peak_steps=random.randint(10, 40),
        recede_steps=random.randint(30, 90),
        peak_cm=random.uniform(18, 80),
        rainfall_range=rainfall_range,
    )
    readings = _measure(station, conditions)
    crossed = next(i for i, (level, _) in enumerate(readings) if level >= SUSPICION_THRESHOLD_CM)
    # A janela termina em algum momento depois que o evento ficou Suspeito
    end = random.randint(crossed + 3, min(len(readings), crossed + 60))
    return _window_ending_at(readings, end)


def noise_window(station: MonitoringStation) -> list[tuple[float, float]]:
    """Algo entre o sensor e a rua que NÃO é alagamento.

    Inclui casos difíceis: obstrução que dura minutos (lixo, galhos, um carro
    parado embaixo do sensor) e splash durante chuva moderada.
    """
    kind = random.choices(["splash_dry", "splash_rain", "obstruction"], weights=[40, 30, 30])[0]
    splash_steps = random.randint(5, 18) if kind == "obstruction" else random.randint(1, 3)
    rainfall_range = (0, 1) if kind == "splash_dry" else (2, 30)

    splash_at = 40
    conditions = scenarios.splash(
        steps=80,
        splash_at=splash_at,
        splash_steps=splash_steps,
        rainfall_range=rainfall_range,
    )
    readings = _measure(station, conditions)
    end = splash_at + random.randint(3, 30)
    return _window_ending_at(readings, end)


def main() -> None:
    parser = argparse.ArgumentParser(description="Gera dados de treino para a IA")
    parser.add_argument("--samples", type=int, default=2000, help="quantidade de janelas por classe")
    parser.add_argument("--output", default="../data/raw/training_readings.csv")
    parser.add_argument("--seed", type=int, default=42)
    args = parser.parse_args()

    random.seed(args.seed)
    station = MonitoringStation(UltrasonicLevelSensor(), RainGauge())
    output = Path(args.output)
    output.parent.mkdir(parents=True, exist_ok=True)

    with output.open("w", newline="") as file:
        writer = csv.writer(file)
        writer.writerow(["window_id", "step", "water_level_cm", "rainfall_mm_per_hour", "label"])
        window_id = 0
        for label, build in (("flood", flood_window), ("noise", noise_window)):
            for _ in range(args.samples):
                for step, (level, rain) in enumerate(build(station)):
                    writer.writerow([window_id, step, level, rain, label])
                window_id += 1

    print(f"{window_id} janelas gravadas em {output}")


if __name__ == "__main__":
    main()