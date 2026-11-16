"""Serviço HTTP da IA: recebe uma janela de leituras e responde se é alagamento.

A IA só OPINA. Quem decide (confirmar ou descartar o evento) é a API.
"""
import os

import joblib
from flask import Flask, jsonify, request

from features import extract_features

MIN_READINGS = 6
CONFIDENCE_THRESHOLD = 0.8

app = Flask(__name__)
model = joblib.load(os.getenv("MODEL_PATH", "model.joblib"))


@app.get("/health")
def health():
    return jsonify(status="ok")


@app.post("/classify")
def classify():
    payload = request.get_json(silent=True) or {}
    readings = payload.get("readings")

    if not isinstance(readings, list):
        return jsonify(error="O corpo deve ter uma lista 'readings'."), 400

    try:
        levels = [float(r["waterLevelCm"]) for r in readings]
        rainfalls = [float(r["rainfallMmPerHour"]) for r in readings]
    except (KeyError, TypeError, ValueError):
        return jsonify(error="Cada leitura precisa de 'waterLevelCm' e 'rainfallMmPerHour' numéricos."), 400

    if len(readings) < MIN_READINGS:
        return jsonify(label="inconclusive", confidence=None,
                       reason=f"São necessárias pelo menos {MIN_READINGS} leituras.")

    probabilities = model.predict_proba([extract_features(levels, rainfalls)])[0]
    best = int(probabilities.argmax())
    label, confidence = str(model.classes_[best]), round(float(probabilities[best]), 4)

    if confidence < CONFIDENCE_THRESHOLD:
        return jsonify(label="inconclusive", confidence=confidence,
                       reason="Confiança abaixo do limite.")

    return jsonify(label=label, confidence=confidence)


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=int(os.getenv("PORT", "8000")))