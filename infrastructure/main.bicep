targetScope = 'resourceGroup'

@description('Environment name')
param environmentName string = 'dev'

@description('Location for all resources')
param location string = resourceGroup().location

// Service Bus
module serviceBus 'servicebus.bicep' = {
  name: 'servicebus-deployment'
  params: {
    serviceBusNamespaceName: 'sb-MeAjudaAi-${environmentName}'
    location: location
    skuName: 'Standard'
  }
}

output serviceBusNamespace string = serviceBus.outputs.serviceBusNamespaceName
output managementPolicyName string = serviceBus.outputs.managementPolicyName
output applicationPolicyName string = serviceBus.outputs.applicationPolicyName
output serviceBusEndpoint string = serviceBus.outputs.serviceBusEndpoint
