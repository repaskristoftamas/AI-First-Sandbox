#!/usr/bin/env bash
# Usage:
#   ./ef.sh add <MigrationName>    — create a new migration
#   ./ef.sh update                 — apply pending migrations
#   ./ef.sh remove                 — remove the last migration
#   ./ef.sh script                 — generate a SQL script

set -euo pipefail

export DatabaseProvider=SqlServer
export ConnectionStrings__DefaultConnection="Server=localhost,1435;Database=BookstoreDb;User Id=sa;Password=passWORD123;TrustServerCertificate=True"

EF_ARGS=(
  --project src/backend/Bookstore.Infrastructure
  --startup-project src/backend/Bookstore.WebApi
)

case "${1:-}" in
  add)    dotnet ef migrations add "$2" "${EF_ARGS[@]}" --output-dir Data/Migrations ;;
  update) dotnet ef database update "${EF_ARGS[@]}" ;;
  remove) dotnet ef migrations remove "${EF_ARGS[@]}" ;;
  script) dotnet ef migrations script "${EF_ARGS[@]}" "${@:2}" ;;
  drop)   dotnet ef database drop --force "${EF_ARGS[@]}" ;;
  *)      echo "Usage: ./ef.sh {add|update|remove|drop|script} [args]"; exit 1 ;;
esac
