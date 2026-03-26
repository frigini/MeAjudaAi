@description('The name of the environment (e.g. dev, prod)')
param environmentName string

@description('The location for all resources')
param location string = resourceGroup().location

@description('The name of the PostgreSQL server')
param postgresServerName string = 'psql-${environmentName}-${uniqueString(resourceGroup().id)}'

@description('The name of the PostgreSQL database')
param postgresDatabaseName string = 'meajudaai'

@description('The administrator username for the PostgreSQL server')
param postgresAdminUsername string = 'psqladmin'

@description('The administrator password for the PostgreSQL server')
@secure()
param postgresAdminPassword string

@description('The name of the Redis cache')
param redisCacheName string = 'redis-${environmentName}-${uniqueString(resourceGroup().id)}'

@description('The name of the Service Bus namespace')
param serviceBusNamespaceName string = 'sb-${environmentName}-${uniqueString(resourceGroup().id)}'

// PostgreSQL Server
resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-03-01-preview' = {
  name: postgresServerName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    administratorLogin: postgresAdminUsername
    administratorLoginPassword: postgresAdminPassword
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
  }
}

// PostgreSQL Database
resource postgresDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-03-01-preview' = {
  parent: postgresServer
  name: postgresDatabaseName
}

// Redis Cache
resource redisCache 'Microsoft.Cache/redis@2023-08-01' = {
  name: redisCacheName
  location: location
  properties: {
    sku: {
      name: 'Basic'
      family: 'C'
      capacity: 0
    }
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
  }
}

// Service Bus Namespace (as RabbitMQ alternative in Azure)
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
}

// Outputs
output postgresHost string = postgresServer.properties.fullyQualifiedDomainName
output postgresDatabase string = postgresDatabaseName
output redisHost string = redisCache.properties.hostName
output serviceBusNamespace string = serviceBusNamespaceName
