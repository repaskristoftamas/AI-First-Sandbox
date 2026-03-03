# Security Rules

## Secrets
- NEVER commit secrets, API keys, connection strings, or tokens to git
- Use environment variables or user-secrets for local dev
- Check for .env, appsettings.*.json, and credentials files before staging

## Input Validation
- Validate ALL external input at API boundaries (controllers/endpoints)
- Use FluentValidation or DataAnnotations for request models
- Reject invalid input early -- don't let it propagate

## SQL / Data
- Use parameterized queries or EF Core -- never string concatenation for SQL
- Apply principle of least privilege for DB access
- Sanitize any user input that appears in logs

## API Security
- Require authentication on all endpoints unless explicitly public
- Use authorization policies, not role checks in controllers
- Return 404 (not 403) when a resource exists but user lacks access (prevent enumeration)
- Rate limit public endpoints

## Dependencies
- Keep NuGet packages updated (watch for security advisories)
- Don't add packages for trivial functionality
- Verify package publisher/reputation before adding
