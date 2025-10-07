# RabbitMQ Configuration

This directory contains security configurations for RabbitMQ message broker.

## Security Features

### `rabbitmq.conf`
- **Disables guest user** remote access (localhost-only)
- **Enforces authentication** for all connections
- **Logs authentication attempts** for security monitoring
- **Sets reasonable connection limits**
- **Requires secure credentials** via environment variables
- **Includes TLS hardening options** (commented out, ready for production use)

## TLS/SSL Security (Optional)

For production deployments, uncomment and configure the TLS options in `rabbitmq.conf`:

```properties
# Bind management interface to localhost only
management.listener.ip = 127.0.0.1

# Enable SSL/TLS listener on port 5671
listeners.ssl.default = 5671

# SSL certificate configuration
ssl_options.cacertfile = /etc/rabbitmq/certs/ca.pem
ssl_options.certfile   = /etc/rabbitmq/certs/server.pem
ssl_options.keyfile    = /etc/rabbitmq/certs/server_key.pem
ssl_options.verify     = verify_peer
ssl_options.fail_if_no_peer_cert = true
```

**Note**: TLS configuration requires valid SSL certificates to be mounted in the container.

## Environment Variables

The RabbitMQ service requires secure credentials:

```bash
# Required for all environments
RABBITMQ_USER=meajudaai               # Default non-guest username
RABBITMQ_PASS=your-secure-password    # Generate with: openssl rand -base64 32
```

## Security Improvements

1. **No Default Guest Access**: Guest user is restricted to localhost only
2. **Required Authentication**: All remote connections must authenticate
3. **Secure Defaults**: No anonymous or default credential access
4. **Monitoring**: Connection and authentication events are logged
5. **Environment-Driven**: Credentials come from secure environment variables

## Usage

The configuration is automatically mounted when using Docker Compose:

```bash
# Mount point in container: /etc/rabbitmq/rabbitmq.conf
- ../../rabbitmq/rabbitmq.conf:/etc/rabbitmq/rabbitmq.conf:ro
```

## Management Interface

- **URL**: `http://localhost:15672`
- **Username**: Value from `RABBITMQ_USER` (default: `meajudaai`)
- **Password**: Value from `RABBITMQ_PASS` (must be set securely)

## Security Notes

⚠️ **Never use default guest/guest credentials in any deployed environment**
✅ **Always generate strong passwords**: `openssl rand -base64 32`
✅ **Use environment variables**: Never hardcode credentials in compose files
✅ **Monitor logs**: Check authentication failures regularly