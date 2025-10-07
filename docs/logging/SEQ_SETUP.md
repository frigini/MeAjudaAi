# 📊 Seq - Logging Estruturado com Serilog

## 🚀 Setup Rápido para Desenvolvimento

### Docker Compose (Recomendado)

Adicione ao seu `docker-compose.development.yml`:

```yaml
services:
  seq:
    image: datalust/seq:latest
    container_name: meajudaai-seq
    environment:
      - ACCEPT_EULA=Y
    ports:
      - "5341:80"
    volumes:
      - seq_data:/data
    restart: unless-stopped

volumes:
  seq_data:
```

### Docker Run (Simples)

```bash
docker run -d \
  --name seq \
  -e ACCEPT_EULA=Y \
  -p 5341:80 \
  -v seq_data:/data \
  datalust/seq:latest
```

## 🎯 Configuração por Ambiente

### Development
- **URL**: `http://localhost:5341`
- **Interface**: `http://localhost:5341`
- **Custo**: 🆓 Gratuito
- **Limite**: Ilimitado

### Production
- **URL**: Configure `${SEQ_SERVER_URL}`
- **API Key**: Configure `${SEQ_API_KEY}`
- **Custo**: 🆓 Gratuito até 32MB/dia
- **Escalabilidade**: $390/ano para 1GB/dia

## 📱 Interface Web

Acesse `http://localhost:5341` para:
- ✅ **Busca estruturada** com sintaxe SQL-like
- ✅ **Filtros por propriedades** (UserId, CorrelationId, etc.)
- ✅ **Dashboards** personalizados
- ✅ **Alertas** por email/webhook
- ✅ **Análise de trends** e performance

## 🔍 Exemplos de Queries

```sql
-- Buscar por usuário específico
UserId = "123" and @Level = "Error"

-- Buscar por correlation ID
CorrelationId = "abc-123-def"

-- Performance lenta
@Message like "%responded%" and Elapsed > 1000

-- Erros de autenticação
@Message like "%authentication%" and @Level = "Error"
```

## 💰 Custos por Volume

| Volume/Dia | Eventos/Dia | Custo/Ano | Cenário |
|------------|-------------|-----------|---------|
| < 32MB | ~100k | 🆓 $0 | MVP/Startup |
| < 1GB | ~3M | $390 | Crescimento |
| < 10GB | ~30M | $990 | Empresa |

## 🛠️ Comandos Úteis

```bash
# Iniciar Seq
docker start seq

# Ver logs do Seq
docker logs seq

# Backup dos dados
docker exec seq cat /data/Documents/seq.db > backup.db

# Verificar saúde
curl http://localhost:5341/api/diagnostics/status
```

## 🎯 Próximos Passos

1. **Desenvolvimento**: Execute `docker run` e acesse `localhost:5341`
2. **CI/CD**: Adicione Seq ao pipeline de desenvolvimento
3. **Produção**: Configure servidor Seq dedicado
4. **Monitoramento**: Configure alertas para erros críticos

## 🔗 Links Úteis

- [Documentação Seq](https://docs.datalust.co/docs)
- [Serilog + Seq](https://docs.datalust.co/docs/using-serilog)
- [Pricing](https://datalust.co/pricing)