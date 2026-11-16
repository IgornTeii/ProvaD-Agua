import json
import os
import subprocess
import sys
from pathlib import Path

import pytest

AI_DIR = Path(__file__).resolve().parents[1]
IOT_DIR = AI_DIR.parent / "iot"


@pytest.fixture(scope="session")
def trained(tmp_path_factory):
    folder = tmp_path_factory.mktemp("modelo")
    dataset, model, metrics = folder / "readings.csv", folder / "model.joblib", folder / "metrics.json"

    subprocess.run([sys.executable, "generate_dataset.py", "--output", str(dataset)], cwd=IOT_DIR, check=True)
    subprocess.run(
        [sys.executable, "train.py", "--data", str(dataset), "--model-output", str(model), "--metrics-output", str(metrics)],
        cwd=AI_DIR, check=True, capture_output=True,
    )

    return {"model": model, "metrics": json.loads(metrics.read_text())}


@pytest.fixture(scope="session")
def client(trained):
    os.environ["MODEL_PATH"] = str(trained["model"])
    import serve

    return serve.app.test_client()
