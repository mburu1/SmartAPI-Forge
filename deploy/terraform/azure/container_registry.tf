resource "azurerm_container_registry" "this" {
  name                = "acr${var.project_name}${var.environment}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  sku                 = "Basic"
  admin_enabled       = true # simplest auth path for App Service; use a managed identity + AcrPull role for production

  tags = {
    project     = var.project_name
    environment = var.environment
  }
}
