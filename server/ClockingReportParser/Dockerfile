# Build context is this directory (see docker-compose.yml).
FROM python:3.12-slim

# qpdf is required by pikepdf (used to repair malformed source PDFs).
RUN apt-get update && apt-get install -y --no-install-recommends qpdf \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

EXPOSE 8010
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8010"]
