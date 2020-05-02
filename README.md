# Games Microservice

Required: CG_FEED environment variable (for building container). Otherwise setup nuget source.

## Building Docker images
```bash
make docker-build
```

## Running infrastructure locally to work against infrastructure
```bash
make docker-infra
```

## Running Integration Tests in Docker Compose
```bash
make docker-test
```

## Kill / Re-run Tests:
```bash
make docker-clean
make docker-test
```
