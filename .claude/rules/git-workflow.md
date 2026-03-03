# Git Workflow

## Commits
- Use [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/)
- Subject: imperative mood, max 72 chars, no trailing period
- Body: explain WHY, not WHAT (the diff shows what)
- One logical change per commit

## Branches
- `main` is the production branch -- always deployable
- Feature branches: `feat/short-description`
- Bug fixes: `fix/short-description`
- Branch from main, merge back to main

## Pull Requests
- One concern per PR (don't mix features with refactors)
- Title matches the main commit's conventional commit message
- Description explains motivation, not implementation details
- All tests must pass before merge

## Safety
- Never force push to main
- Never skip pre-commit hooks (--no-verify)
- Never amend published commits
- Always pull before push to avoid conflicts
