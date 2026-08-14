# Deploy configurators

Illustrative Terraform for running the Api on either cloud. These are
starting points, not a turnkey production stack — read the inline comments
before applying (they call out what's simplified: public subnets instead of
a NAT'd private setup, Basic-tier SKUs, `skip_final_snapshot`, etc.).

Both stacks are validated with `terraform validate` and formatted with
`terraform fmt`, but **never applied** as part of this repo's CI or by any
agent — applying either one creates real, billable cloud resources and
requires your own cloud credentials. Run `terraform plan`/`apply` yourself,
locally, after reviewing the plan.

- [`aws/`](aws/) — ECS Fargate behind an ALB, RDS (Postgres/MySQL/SQL
  Server Express selectable via `db_engine`), ECR, secrets in SSM
  Parameter Store.
- [`azure/`](azure/) — Linux App Service for Containers, Azure Database for
  PostgreSQL Flexible Server, Azure Container Registry.

## Usage (either directory)

```bash
cd deploy/terraform/aws   # or azure

terraform init
terraform plan -out=tfplan   # requires TF_VAR_db_password (aws) or
                              # TF_VAR_db_admin_password + TF_VAR_jwt_key (azure)
terraform apply tfplan
```

Never put real passwords in a committed `.tfvars` file — pass them as
`TF_VAR_*` environment variables or an untracked `*.tfvars` (the directory's
`.gitignore` already excludes `*.tfvars`).
