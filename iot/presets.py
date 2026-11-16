from dataclasses import dataclass
from typing import Callable, Iterator

import scenarios
from scenarios import StreetConditions


@dataclass(frozen=True)
class Preset:
    id: str
    name: str
    description: str
    expected: str
    build: Callable[[], Iterator[StreetConditions]]


PRESETS = [
    Preset(
        "flood",
        "Alagamento de verdade",
        "Chuva forte. A água sobe aos poucos, fica alta por um tempo e depois baixa.",
        "A IA confirma, o alagamento é encerrado e as comprovações são emitidas.",
        lambda: scenarios.flood(),
    ),
    Preset(
        "flash_flood",
        "Enxurrada rápida",
        "Um temporal faz a água subir de repente.",
        "Mesmo com a subida brusca, a IA confirma e as comprovações são emitidas.",
        lambda: scenarios.flood(rise_steps=6, peak_steps=25, recede_steps=40, peak_cm=45, rainfall_range=(50, 80)),
    ),
    Preset(
        "upstream_flood",
        "Alagamento sem chuva no bairro",
        "A água vem de outro bairro, rio acima. Aqui o céu está quase seco.",
        "A IA reconhece o alagamento pelo comportamento da água, mesmo sem chuva.",
        lambda: scenarios.flood(rise_steps=40, peak_steps=25, recede_steps=40, peak_cm=35, rainfall_range=(0, 4)),
    ),
    Preset(
        "splash",
        "Carro jogando água no sensor",
        "Um carro passa e espirra água na frente do sensor por alguns segundos.",
        "Abre uma suspeita, e a IA descarta: não era alagamento.",
        lambda: scenarios.splash(),
    ),
    Preset(
        "obstruction",
        "Lixo parado embaixo do sensor",
        "Um saco de lixo fica alguns minutos embaixo do sensor, com chuva fraca.",
        "Abre uma suspeita, e a IA descarta: não era alagamento.",
        lambda: scenarios.splash(steps=70, splash_at=20, splash_steps=12, rainfall_range=(3, 12)),
    ),
    Preset(
        "light_rain",
        "Chuva fraca",
        "Chove um pouco e a água sobe alguns centímetros, sem alagar.",
        "Nada acontece: a água nunca chega ao nível de suspeita.",
        lambda: scenarios.light_rain(),
    ),
]

PRESETS_BY_ID = {preset.id: preset for preset in PRESETS}
