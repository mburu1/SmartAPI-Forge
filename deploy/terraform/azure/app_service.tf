resource "azurerm_service_plan" "this" {
  name                = "asp-${var.project_name}-${var.environment}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  os_type             = "Linux"
  sku_name            = var.app_service_sku
}

resource "azurerm_linux_web_app" "api" {
  name                = "app-${var.project_name}-${var.environment}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  service_plan_id     = azurerm_service_plan.this.id

  site_config {
    application_stack {
      docker_image_name   = var.container_image
      docker_registry_url = "https://${azurerm_container_registry.this.login_server}"
    }
  }

  app_settings = {
    WEBSITES_PORT                   = tostring(var.container_port)
    ASPNETCORE_ENVIRONMENT          = "Production"
    ASPNETCORE_HTTP_PORTS           = tostring(var.container_port)
    Database__Provider              = "Postgres"
    "ConnectionStrings__Postgres"   = local.db_connection_string
    Jwt__Key                        = var.jwt_key
    DOCKER_REGISTRY_SERVER_URL      = "https://${azurerm_container_registry.this.login_server}"
    DOCKER_REGISTRY_SERVER_USERNAME = azurerm_container_registry.this.admin_username
    DOCKER_REGISTRY_SERVER_PASSWORD = azurerm_container_registry.this.admin_password
  }

  tags = {
    project     = var.project_name
    environment = var.environment
  }
}

locals {
  db_connection_string = "Host=${azurerm_postgresql_flexible_server.this.fqdn};Port=5432;Username=${var.db_admin_username};Password=${var.db_admin_password};Database=${var.db_name}"
}
