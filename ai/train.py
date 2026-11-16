"""Treina o classificador de alagamento e compara com a regra fixa (antes x depois)."""
import argparse
import csv
import json
from collections import defaultdict
from pathlib import Path

import joblib
from sklearn.ensemble import RandomForestClassifier
from sklearn.metrics import accuracy_score, confusion_matrix, f1_score, precision_score, recall_score
from sklearn.model_selection import train_test_split

from features import FEATURE_NAMES, extract_features

BASELINE_THRESHOLD_CM = 15.0
BASELINE_MIN_CONSECUTIVE = 3


def baseline_rule(levels: list[float]) -> str:
    run = 0
    for level in levels:
        run = run + 1 if level >= BASELINE_THRESHOLD_CM else 0
        if run >= BASELINE_MIN_CONSECUTIVE:
            return "flood"
    return "noise"


def load_windows(path: Path) -> tuple[list[list[float]], list[str], list[list[float]]]:
    levels, rains, labels = defaultdict(list), defaultdict(list), {}
    with path.open() as file:
        for row in csv.DictReader(file):
            window_id = int(row["window_id"])
            levels[window_id].append(float(row["water_level_cm"]))
            rains[window_id].append(float(row["rainfall_mm_per_hour"]))
            labels[window_id] = row["label"]

    features, targets, raw_levels = [], [], []
    for window_id in sorted(labels):
        features.append(extract_features(levels[window_id], rains[window_id]))
        targets.append(labels[window_id])
        raw_levels.append(levels[window_id])
    return features, targets, raw_levels


def metrics(y_true: list[str], y_pred: list[str]) -> dict:
    return {
        "accuracy": round(accuracy_score(y_true, y_pred), 4),
        "precision_flood": round(precision_score(y_true, y_pred, pos_label="flood"), 4),
        "recall_flood": round(recall_score(y_true, y_pred, pos_label="flood"), 4),
        "f1_flood": round(f1_score(y_true, y_pred, pos_label="flood"), 4),
        "confusion_matrix [flood, noise]": confusion_matrix(y_true, y_pred, labels=["flood", "noise"]).tolist(),
    }


def main() -> None:
    parser = argparse.ArgumentParser(description="Treina a IA do Prova d'Água")
    parser.add_argument("--data", default="../data/raw/training_readings.csv")
    parser.add_argument("--model-output", default="model.joblib")
    parser.add_argument("--metrics-output", default="metrics.json")
    args = parser.parse_args()

    features, targets, raw_levels = load_windows(Path(args.data))
    indices = list(range(len(targets)))

    train_idx, test_idx = train_test_split(indices, test_size=0.25, random_state=42, stratify=targets)

    model = RandomForestClassifier(n_estimators=200, max_depth=8, random_state=42)
    model.fit([features[i] for i in train_idx], [targets[i] for i in train_idx])

    y_test = [targets[i] for i in test_idx]
    y_model = list(model.predict([features[i] for i in test_idx]))
    y_baseline = [baseline_rule(raw_levels[i]) for i in test_idx]

    report = {
        "train_windows": len(train_idx),
        "test_windows": len(test_idx),
        "baseline_rule": (f"nível >= {BASELINE_THRESHOLD_CM} cm por "
                          f"{BASELINE_MIN_CONSECUTIVE}+ leituras seguidas é alagamento"),
        "baseline": metrics(y_test, y_baseline),
        "model": metrics(y_test, y_model),
        "feature_importance": dict(sorted(
            zip(FEATURE_NAMES, (round(float(v), 4) for v in model.feature_importances_)),
            key=lambda item: item[1], reverse=True)),
    }

    joblib.dump(model, args.model_output)
    Path(args.metrics_output).write_text(json.dumps(report, indent=2, ensure_ascii=False))
    print(json.dumps(report, indent=2, ensure_ascii=False))


if __name__ == "__main__":
    main()