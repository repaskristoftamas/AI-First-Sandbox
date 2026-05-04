# AI-First Sandbox Repository

This repository is an experimental environment focused on AI-driven development rather than the creation of an actual product. It demonstrates an end-to-end agentic CI/CD pipeline where AI agents autonomously implement features, review code, address review feedback, and manage the project board — with human involvement limited to idea input and final approval.

The development workflow is built on [Claude Code](https://claude.ai/claude-code) with custom agents, skills, and commands, combined with [GitHub Agentic Workflows](https://github.github.com/gh-aw/) running as GitHub Actions. Project conventions (architecture, coding standards, testing, security, git workflow) are codified as structured agent instructions, enabling AI agents to produce standards-compliant code that follows Clean Architecture, CQRS, and DDD patterns out of the box.

For further details, see [this article](https://kristofrepas.medium.com/how-i-automated-my-entire-sdlc-with-ai-agents-e32cf6dc8c97).

# Bookstore-API

## Database Provider

The API supports both **SQL Server** and **PostgreSQL**. The active provider is controlled by the `DatabaseProvider` configuration setting.

| Value        | Provider   | Connection String Key |
|--------------|------------|-----------------------|
| `SqlServer`  | SQL Server | `DefaultConnection`   |
| `PostgreSQL` | PostgreSQL | `PostgreSQL`          |

### Switching Providers

Set the `DatabaseProvider` value in `appsettings.json`, an environment variable, or user-secrets:

```bash
# Via environment variable
export DatabaseProvider=PostgreSQL

# Via dotnet user-secrets
dotnet user-secrets set "DatabaseProvider" "PostgreSQL"
```

### Running with Docker Compose

**SQL Server (default):**

```bash
docker compose up
```

**PostgreSQL:**

```bash
docker compose -f docker-compose.yml -f docker-compose.postgres.yml up
```

### EF Core Migrations

Migrations are provider-specific. Set the `DatabaseProvider` environment variable before running EF tooling:

```bash
# SQL Server migrations (default)
export ConnectionStrings__DefaultConnection="Server=localhost,1435;Database=BookstoreDb;User Id=sa;Password=passWORD123;TrustServerCertificate=True"
dotnet ef migrations add <MigrationName> --project src/backend/Bookstore.Infrastructure --startup-project src/backend/Bookstore.WebApi

# PostgreSQL migrations
export DatabaseProvider=PostgreSQL
export ConnectionStrings__PostgreSQL="Host=localhost;Port=5433;Database=BookstoreDb;Username=bookstore;Password=passWORD123"
dotnet ef migrations add <MigrationName> --project src/backend/Bookstore.Infrastructure --startup-project src/backend/Bookstore.WebApi
```

> **Note:** The existing migrations were generated for SQL Server. If the schemas diverge between providers, consider separate migration assemblies per provider.

## AI-Assisted Development Workflow

This project uses [Claude Code](https://claude.ai/claude-code) commands and [GitHub Agentic Workflows](https://github.github.com/gh-aw/) to automate the issue-to-PR lifecycle. Below is a step-by-step walkthrough of how a feature goes from idea to reviewed pull request.

### 1. Draft an Issue

**Command:** `/draft-issue <description in free text>`

Describe what you want in plain language. Claude Code will:

1. Resolve the current GitHub repo from `git remote origin`
2. Explore the codebase to understand relevant architecture, patterns, and affected files
3. Check for duplicate issues on GitHub
4. Draft a structured issue (title, summary, scope, acceptance criteria, out-of-scope, technical notes)
5. Flag risks, gaps, missing edge cases, or suggest splitting into multiple issues
6. Present the draft for review

You iterate on the draft until you're satisfied, then approve it. Claude creates the issue on GitHub via the `gh` CLI.

### 2. Implement the Issue

**Command:** `/work-issue <issue-number>`

Point Claude Code at a GitHub issue number. It will:

1. Resolve the repo from `git remote origin`
2. Fetch the issue details from GitHub (`gh issue view`)
3. Explore the codebase to identify all files that need changes
4. Create a feature branch (`issue-{number}-{short-description}`)
5. Implement the changes following existing conventions
6. Build (`dotnet build`) and fix any compilation errors
7. Run tests (`dotnet test`) and fix any failures
8. Validate changes against the project's architecture and coding standards
9. Commit with a message referencing the issue (e.g., `fix #14: remove RabbitMQ code`)
10. Push and create a pull request that references `Closes #N`

The commit and PR steps use Claude Code's built-in `/commit` and `/pr` skills. Skills are prompt templates that enforce consistent formatting (conventional commits, co-author line, structured PR description) on top of the same underlying tools (git, `gh` CLI). They don't add new capabilities -- they standardize *how* the tools are used.

The PR is left open for human review -- Claude never merges it.

### 3. Code Review by Grumpy Reviewer

**Trigger:** Automatic — fires when a PR is opened.

[`.github/workflows/auto-grumpy.yml`](.github/workflows/auto-grumpy.yml) posts a `/grumpy` comment on the PR automatically, which triggers the reviewer workflow ([`.github/workflows/grumpy-reviewer.lock.yml`](.github/workflows/grumpy-reviewer.lock.yml)):

1. **Activation** -- Validates the commenter has write access and the comment is on a PR
2. **Agent** -- Spins up a sandboxed Claude Code instance in CI with read-only access to the repo and PR diff
3. **Analysis** -- The agent reviews changed files for code smells, performance issues, security concerns, naming, error handling, over/under-engineering
4. **Review comments** -- Posts up to 5 inline review comments in a grumpy-but-constructive tone
5. **Verdict** -- Submits a formal review (`APPROVE`, `REQUEST_CHANGES`, or `COMMENT`)
6. **Threat detection** -- A second pass verifies the agent's output for safety before publishing
7. **Safe outputs** -- Review comments are posted to the PR only after passing all checks

The reviewer persona is a sarcastic senior developer with 40+ years of experience. It has persistent memory across reviews to track recurring patterns and avoid repeating itself.

### 4. Address Review (Automatic)

**Trigger:** Automatic — fires when the Grumpy Reviewer submits its review.

[`.github/workflows/address-grumpy-review.yml`](.github/workflows/address-grumpy-review.yml) detects the review via the `pull_request_review` event, filtering on the unique signature the reviewer embeds in every review body. A Claude Code instance runs on the CI runner with full project context (coding standards, architecture rules, testing conventions, agent definitions) and executes the `/address-review-ci` command:

1. Fetches all inline review comments and the overall review summary
2. Reads the diff and each affected file for full context
3. Classifies every comment as `fix`, `discuss`, or `skip` against the project's established conventions
4. Applies all `fix` changes to the codebase
5. Builds (`dotnet build`) and runs the full test suite (`dotnet test`), fixing any failures
6. Commits the fixes using the `/commit` skill
7. Replies to every review comment on GitHub explaining the outcome
8. Moves the issue to **Ready for Review** on the project board
9. Pushes the commit back to the PR branch

The PR is then ready for a final human review. Claude never merges it.

### End-to-End Flow

```
You describe a feature
        |
        v
/draft-issue "add pagination to books endpoint"
        |
        v
Claude explores codebase, drafts issue, suggests improvements
        |
        v
You review, iterate, approve → Issue created on GitHub
        |
        v
/work-issue 42
        |
        v
Claude branches, implements, builds, tests, commits, opens PR
        |
        v [PR opened — automatic from here]
        |
        v
auto-grumpy.yml posts /grumpy comment
        |
        v
Grumpy Reviewer analyzes diff, posts inline comments, submits review
        |
        v
address-grumpy-review.yml detects the review signature
        |
        v
Claude evaluates each comment (fix / discuss / skip),
applies fixes, builds, tests, commits, replies to comments,
updates project board → pushes to PR branch
        |
        v
You do a final review, merge when satisfied
```
