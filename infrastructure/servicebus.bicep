@description('Service Bus Namespace name')
param serviceBusNamespaceName string = 'sb-MeAjudaAi-${uniqueString(resourceGroup().id)}'

@description('Location for all resources')
param location string = resourceGroup().location

@description('Service Bus SKU')
@allowed(['Basic', 'Standard', 'Premium'])
param skuName string = 'Standard'

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: skuName
    tier: skuName
  }
  properties: {
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
  }
}

// Shared Access Policy para gerenciamento (criação de tópicos)
resource managementPolicy 'Microsoft.ServiceBus/namespaces/authorizationRules@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: 'ManagementPolicy'
  properties: {
    rights: [
      'Manage'
      'Send' 
      'Listen'
    ]
  }
}

// Shared Access Policy apenas para envio/recebimento (para produção)
resource applicationPolicy 'Microsoft.ServiceBus/namespaces/authorizationRules@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: 'ApplicationPolicy'
  properties: {
    rights: [
      'Send'
      'Listen'
    ]
  }
}

output serviceBusNamespaceName string = serviceBusNamespace.name
output managementPolicyName string = managementPolicy.name
output applicationPolicyName string = applicationPolicy.name
output serviceBusEndpoint string = serviceBusNamespace.properties.serviceBusEndpoint
