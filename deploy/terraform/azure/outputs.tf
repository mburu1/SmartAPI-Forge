output "api_url" {
  description = "Public URL of the Api."
  value       = "https://${azurerm_linux_web_app.api.default_hostname}"
}

output "container_registry_login_server" {
  description = "Push images here, then update container_image and re-apply."
  value       = azurerm_container_registry.this.login_server
}

output "postgres_fqdn" {
  value = azurerm_postgresql_flexible_server.this.fqdn
}
