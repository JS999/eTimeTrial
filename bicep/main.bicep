param location string = 'westeurope'

param acrName string
param webAppName string

resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: acrName
  location: location
  sku: {
    name: 'Basic'
  }
}

resource appPlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${webAppName}-plan'
  location: location

  kind: 'linux'

  sku: {
    name: 'B1'
    tier: 'Basic'
  }

  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: webAppName
  location: location

  properties: {
    serverFarmId: appPlan.id

    siteConfig: {
      linuxFxVersion: 'DOCKER|${acr.properties.loginServer}/etimetrial-web:latest'
    }
  }
}
