FROM python:3.13-slim
ENV PYTHONUNBUFFERED=1
WORKDIR /app

COPY ai/requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY iot/*.py /simulator/
COPY ai/*.py ./

RUN python /simulator/generate_dataset.py --output /data/raw/training_readings.csv \
 && python train.py --data /data/raw/training_readings.csv

EXPOSE 8000
CMD ["python", "serve.py"]