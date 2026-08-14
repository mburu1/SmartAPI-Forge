variable "location" {
  description = "Azure region."
  type        = string
  default     = "eastus"
}

variable "project_name" {
  description = "Short name used to prefix/tag every resource."
  type        = string
  default     = "smartapiforge"
}

variable "environment" {
  description = "Deployment environment (dev, staging, production)."
  type        = string
  default     = "dev"
}

variable "container_image" {
  description = "Full image reference the Web App runs, e.g. '<registry>.azurecr.io/smartapiforge-api:latest'. Push a real image to the registry this stack creates and update it before the app can start healthy."
  type        = string
  default     = "mcr.microsoft.com/appsvc/staticsite:latest" # Microsoft's placeholder "it works" image
}

variable "container_port" {
  description = "Port the Api listens on inside the container."
  type        = number
  default     = 8080
}

variable "app_service_sku" {
  description = "App Service Plan SKU (Linux)."
  type        = string
  default     = "B1"
}

variable "db_sku_name" {
  description = "Azure Database for PostgreSQL Flexible Server SKU."
  type        = string
  default     = "B_Standard_B1ms"
}

variable "db_storage_mb" {
  description = "Flexible Server storage size in MB."
  type        = number
  default     = 32768
}

variable "db_name" {
  description = "Database name created on the Flexible Server."
  type        = string
  default     = "smartapiforge"
}

variable "db_admin_username" {
  description = "Administrator username for the Flexible Server."
  type        = string
  default     = "smartapiforge_admin"
}

variable "db_admin_password" {
  description = "Administrator password for the Flexible Server. Pass via TF_VAR_db_admin_password or a gitignored .tfvars — never commit it."
  type        = string
  sensitive   = true
}

variable "jwt_key" {
  description = "Base64-encoded JWT signing key. Pass via TF_VAR_jwt_key or a gitignored .tfvars — never commit it. For production, prefer an Azure Key Vault reference instead of a plain App Setting."
  type        = string
  sensitive   = true
}
