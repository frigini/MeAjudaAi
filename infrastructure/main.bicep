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

@description('The VNet subnet ID for private endpoint')
param vnetSubnetId string = ''

@description('The VNet ID for DNS zone link')
param vnetId string = ''

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
    publicNetworkAccess: 'Disabled'
  }
}

// PostgreSQL Database
resource postgresDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2025-08-01' = {
  parent: postgresServer
  name: postgresDatabaseName
}

// Private Endpoint for PostgreSQL (only if VNet is provided)
resource postgresPrivateEndpoint 'Microsoft.Network/privateEndpoints@2024-01-01' = if (vnetSubnetId != '') {
  name: '${postgresServerName}-pe'
  location: location
  properties: {
    privateLinkServiceConnections: [
      {
        name: '${postgresServerName}-pe-connection'
        properties: {
          privateLinkServiceId: postgresServer.id
          groupIds: ['postgresqlServer']
        }
      }
    ]
    subnet: {
      id: vnetSubnetId
    }
  }
}

// DNS Zone for PostgreSQL Private Link (only if VNet is provided)
resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = if (vnetId != '') {
  name: 'privatelink.postgres.database.azure.com'
  location: 'global'
  properties: {}
}

resource privateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = if (vnetId != '') {
  name: '${privateDnsZone.name}-link'
  parent: privateDnsZone
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vnetId
    }
  }
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

resource redisDatabase 'Microsoft.Cache/redisEnterprise/databases@2024-02-01' = {
  parent: redisCache
  name: 'default'
  properties: {
    clientProtocol: 'Encrypted'
    clusteringPolicy: 'OSSCluster'
  }
}

// Service Bus Namespace (as RabbitMQ alternative in Azure)
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
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
output redisHost string = redisDatabase.properties.endpoint
output serviceBusNamespace string = serviceBusNamespaceName
output postgresPrivateEndpointIp string = (vnetSubnetId != '') ? '${postgresPrivateEndpoint.properties.customNetworkInterfaceName}' : ''
output privateDnsZoneName string = (vnetId != '') ? privateDnsZone.name : ''
