variable "aws_region" {
  description = "AWS region to deploy into."
  type        = string
  default     = "us-east-1"
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
  description = "Full image reference the ECS task runs (e.g. the ECR repo URL this stack creates, tagged). Leave the default only for a first `terraform apply` — push a real image and update it before the service can start healthy tasks."
  type        = string
  default     = "public.ecr.aws/docker/library/hello-world:latest"
}

variable "container_port" {
  description = "Port the Api listens on inside the container (matches ASPNETCORE_HTTP_PORTS in the Dockerfile)."
  type        = number
  default     = 8080
}

variable "desired_count" {
  description = "Number of ECS tasks to run."
  type        = number
  default     = 1
}

variable "task_cpu" {
  description = "Fargate task vCPU units (256 = 0.25 vCPU)."
  type        = number
  default     = 256
}

variable "task_memory" {
  description = "Fargate task memory in MiB."
  type        = number
  default     = 512
}

variable "db_engine" {
  description = "RDS engine: postgres, mysql, or sqlserver-ex (matches Database:Provider in appsettings)."
  type        = string
  default     = "postgres"

  validation {
    condition     = contains(["postgres", "mysql", "sqlserver-ex"], var.db_engine)
    error_message = "db_engine must be one of: postgres, mysql, sqlserver-ex."
  }
}

variable "db_instance_class" {
  description = "RDS instance class."
  type        = string
  default     = "db.t4g.micro"
}

variable "db_allocated_storage" {
  description = "RDS allocated storage in GiB."
  type        = number
  default     = 20
}

variable "db_name" {
  description = "Database name created on the RDS instance."
  type        = string
  default     = "smartapiforge"
}

variable "db_username" {
  description = "Master username for the RDS instance."
  type        = string
  default     = "smartapiforge_admin"
}

variable "db_password" {
  description = "Master password for the RDS instance. Pass via TF_VAR_db_password or a .tfvars file that is gitignored — never commit it."
  type        = string
  sensitive   = true
}
