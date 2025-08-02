targetScope = 'resourceGroup'

@description('Environment name')
param environmentName string = 'dev'

@description('Location for all resources')
param location string = resourceGroup().location

// Service Bus
module serviceBus 'servicebus.bicep' = {
  name: 'servicebus-deployment'
  params: {
    serviceBusNamespaceName: 'sb-meajudaai-${environmentName}'
    location: location
    skuName: 'Standard'
  }
}

output serviceBusConnectionString string = serviceBus.outputs.managementConnectionString
output serviceBusNamespace string = serviceBus.outputs.serviceBusNamespaceName