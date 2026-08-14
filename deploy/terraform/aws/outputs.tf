output "api_url" {
  description = "Public URL of the load-balanced Api."
  value       = "http://${aws_lb.this.dns_name}"
}

output "ecr_repository_url" {
  description = "Push images here, then update container_image and re-apply."
  value       = aws_ecr_repository.api.repository_url
}

output "rds_endpoint" {
  description = "RDS instance endpoint (host:port)."
  value       = aws_db_instance.this.endpoint
}

output "ecs_cluster_name" {
  value = aws_ecs_cluster.this.name
}
