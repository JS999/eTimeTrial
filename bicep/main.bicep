param location string = 'westeurope'
@secure()
param frontendClientSecret string=''

@secure()
param cosmosKey string=''

param cosmosEndpoint string=''
param applicationInsightsConnectionString string=''

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
resource webAppSettings 'Microsoft.Web/sites/config@2023-12-01' = {
  parent: webApp
  name: 'appsettings'

  properties: {

    ASPNETCORE_ENVIRONMENT: 'Production'

    WEBSITES_PORT: '8080'

    AzureAd__TenantId: '8f3c88c1-8477-49a2-a4a3-388052b55f75'

    AzureAd__FrontendClientId: '2c84ea02-7314-4bd8-aa9e-20d37c53388e'

    AzureAd__FrontendClientSecret: frontendClientSecret

    AzureAd__CallbackPath: '/signin-oidc'

    CosmosDb__Endpoint: cosmosEndpoint

    CosmosDb__Key: cosmosKey

    CosmosDb__DatabaseName: 'RaceDb'

    CosmosDb__ContainerName: 'Races'

    ApplicationInsights__ConnectionString: applicationInsightsConnectionString
  }
}
