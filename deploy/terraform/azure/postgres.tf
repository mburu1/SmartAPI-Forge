resource "azurerm_postgresql_flexible_server" "this" {
  name                = "psql-${var.project_name}-${var.environment}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location

  version    = "16"
  sku_name   = var.db_sku_name
  storage_mb = var.db_storage_mb

  administrator_login    = var.db_admin_username
  administrator_password = var.db_admin_password

  backup_retention_days        = 7
  geo_redundant_backup_enabled = false

  tags = {
    project     = var.project_name
    environment = var.environment
  }
}

resource "azurerm_postgresql_flexible_server_database" "this" {
  name      = var.db_name
  server_id = azurerm_postgresql_flexible_server.this.id
  charset   = "UTF8"
  collation = "en_US.utf8"
}

# Lets App Service (which has no fixed outbound IP on the Basic tier) reach
# the database. Tighten this to a VNet integration + private endpoint for
# production.
resource "azurerm_postgresql_flexible_server_firewall_rule" "allow_azure_services" {
  name             = "allow-azure-services"
  server_id        = azurerm_postgresql_flexible_server.this.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}
