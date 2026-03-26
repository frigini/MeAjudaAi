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

@description('The SKU of the PostgreSQL server')
param postgresSkuName string = 'Standard_B1ms'

@description('The tier of the PostgreSQL server')
param postgresSkuTier string = 'Burstable'

@description('The storage size of the PostgreSQL server in GB')
param postgresStorageSizeGB int = 32

@description('The backup retention days for the PostgreSQL server')
param postgresBackupRetentionDays int = 7

@description('The SKU of the Redis cache')
param redisSkuName string = 'Balanced_B1'

// PostgreSQL Server
resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: postgresServerName
  location: location
  sku: {
    name: postgresSkuName
    tier: postgresSkuTier
  }
  properties: {
    version: '16'
    administratorLogin: postgresAdminUsername
    administratorLoginPassword: postgresAdminPassword
    storage: {
      storageSizeGB: postgresStorageSizeGB
    }
    backup: {
      backupRetentionDays: postgresBackupRetentionDays
      geoRedundantBackup: 'Disabled'
    }
  }
}

// PostgreSQL Database
resource postgresDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-03-01-preview' = {
  parent: postgresServer
  name: postgresDatabaseName
}

// Redis Cache (Azure Managed Redis / Redis Enterprise)
resource redisCache 'Microsoft.Cache/redisEnterprise@2024-02-01' = {
  name: redisCacheName
  location: location
  sku: {
    name: redisSkuName
  }
  properties: {
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
