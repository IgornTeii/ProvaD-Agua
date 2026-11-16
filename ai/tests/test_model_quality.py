MIN_ACCURACY = 0.95
MIN_PRECISION = 0.95
MIN_RECALL = 0.95


def test_model_is_accurate(trained):
    assert trained["metrics"]["model"]["accuracy"] >= MIN_ACCURACY


def test_model_rarely_raises_false_alarms(trained):
    assert trained["metrics"]["model"]["precision_flood"] >= MIN_PRECISION


def test_model_rarely_misses_a_real_flood(trained):
    assert trained["metrics"]["model"]["recall_flood"] >= MIN_RECALL


def test_model_beats_the_fixed_rule(trained):
    metrics = trained["metrics"]

    assert metrics["model"]["f1_flood"] > metrics["baseline"]["f1_flood"]
    assert metrics["model"]["confusion_matrix [flood, noise]"][1][0] < metrics["baseline"]["confusion_matrix [flood, noise]"][1][0]
