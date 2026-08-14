resource "aws_db_subnet_group" "this" {
  name       = "${var.project_name}-${var.environment}"
  subnet_ids = data.aws_subnets.default.ids

  tags = {
    Project     = var.project_name
    Environment = var.environment
  }
}

resource "aws_db_instance" "this" {
  identifier     = "${var.project_name}-${var.environment}"
  engine         = var.db_engine
  engine_version = null # use the provider's current default for the chosen engine

  instance_class    = var.db_instance_class
  allocated_storage = var.db_allocated_storage
  storage_encrypted = true

  db_name  = var.db_engine == "sqlserver-ex" ? null : var.db_name # SQL Server Express doesn't support db_name at creation
  username = var.db_username
  password = var.db_password

  db_subnet_group_name   = aws_db_subnet_group.this.name
  vpc_security_group_ids = [aws_security_group.rds.id]
  publicly_accessible    = false

  backup_retention_period = 7
  skip_final_snapshot     = true # set false and add final_snapshot_identifier for production

  tags = {
    Project     = var.project_name
    Environment = var.environment
  }
}
