# Fetch Data

An F# Cli app to grab OHLC data from TwelveData's REST API and push to Postgres
DB. If there is OHLC data in the database, then data will be fetched from the
latest timestamp entry. Otherwise, data will be fetched for the past 24 hours,
and collect new records since the latest timestamp on each subsequent execution.

The environment variables are

- CONNECTION_STRING - Postgres db connection string
- API_KEY - TwelveData API key

The env vars can be set using a `--env-file .env` or `-e` flags when running the
container.

# Containerization

## When making code changes

### Build

```
docker build -t username/fetch_data:latest .
```

## Push

```
docker push username/fetch_data:latest
```

## Pulling and using the image on target environment

### Pull

```
docker pull username/fetch_data:latest
```

### Run

```
docker run --rm --env-file .env username/fetch_data:latest ETHUSD > ~/fetch_eth.log
```
