# RabbitMQ Configuration

RabbitMQ is used as the message broker for async communication between modules.

## Security

RabbitMQ security is handled by environment variables, not config files:

```bash
# Required for all environments
RABBITMQ_USER=meajudaai
RABBITMQ_PASS=<secure-password>    # Generate with: openssl rand -base64 32
```

- Guest user is restricted to localhost by default (RabbitMQ built-in behavior)
- Credentials are injected via `RABBITMQ_DEFAULT_USER` / `RABBITMQ_DEFAULT_PASS` env vars
- RabbitMQ ports are only exposed on internal networks (not publicly accessible)

## TLS/SSL (Production)

For production deployments, create a `rabbitmq.conf` with TLS options:

```properties
listeners.ssl.default = 5671
ssl_options.cacertfile = /etc/rabbitmq/certs/ca.pem
ssl_options.certfile   = /etc/rabbitmq/certs/server.pem
ssl_options.keyfile    = /etc/rabbitmq/certs/server_key.pem
ssl_options.verify     = verify_peer
ssl_options.fail_if_no_peer_cert = true
```

Mount it via Docker Compose or Aspire `WithBindMount`.

## Management Interface

- **URL**: `http://localhost:15672`
- **Username**: Value from `RABBITMQ_USER`
- **Password**: Value from `RABBITMQ_PASS`
