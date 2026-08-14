# Secrets are stored as SecureString SSM parameters and injected into the
# task as environment variables at launch (ECS resolves `secrets[].valueFrom`
# itself — the value never appears in the task definition or CloudWatch Logs
# unless the app logs it).

resource "aws_ssm_parameter" "db_connection_string" {
  name  = "/${var.project_name}/${var.environment}/db-connection-string"
  type  = "SecureString"
  value = local.db_connection_string

  tags = {
    Project     = var.project_name
    Environment = var.environment
  }
}

resource "aws_ssm_parameter" "jwt_key" {
  name  = "/${var.project_name}/${var.environment}/jwt-key"
  type  = "SecureString"
  value = "REPLACE_WITH_BASE64_256BIT_SECRET" # override via -var or a gitignored .tfvars before applying

  lifecycle {
    ignore_changes = [value] # rotate out-of-band; don't let a stale default clobber a real secret
  }

  tags = {
    Project     = var.project_name
    Environment = var.environment
  }
}

resource "aws_iam_role_policy" "ecs_execution_ssm" {
  name = "${var.project_name}-${var.environment}-ecs-execution-ssm"
  role = aws_iam_role.ecs_task_execution.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect = "Allow"
      Action = ["ssm:GetParameters", "kms:Decrypt"]
      Resource = [
        aws_ssm_parameter.db_connection_string.arn,
        aws_ssm_parameter.jwt_key.arn,
      ]
    }]
  })
}

locals {
  db_connection_string = (
    var.db_engine == "postgres" ?
    "Host=${aws_db_instance.this.address};Port=${aws_db_instance.this.port};Username=${var.db_username};Password=${var.db_password};Database=${var.db_name}" :
    var.db_engine == "mysql" ?
    "Server=${aws_db_instance.this.address};Port=${aws_db_instance.this.port};Database=${var.db_name};User=${var.db_username};Password=${var.db_password}" :
    "Server=${aws_db_instance.this.address},${aws_db_instance.this.port};Database=${var.db_name};User Id=${var.db_username};Password=${var.db_password};TrustServerCertificate=True"
  )
}
