# Bookstore-API

## AI-Assisted Development Workflow

This project uses [Claude Code](https://claude.ai/claude-code) commands and [GitHub Agentic Workflows](https://github.github.com/gh-aw/) to automate the issue-to-PR lifecycle. Below is a step-by-step walkthrough of how a feature goes from idea to reviewed pull request.

### 1. Draft an Issue

**Command:** `/user:draft-issue <description in free text>`

Describe what you want in plain language. Claude Code will:

1. Resolve the current GitHub repo from `git remote origin`
2. Explore the codebase to understand relevant architecture, patterns, and affected files
3. Check for duplicate issues on GitHub
4. Draft a structured issue (title, summary, scope, acceptance criteria, out-of-scope, technical notes)
5. Flag risks, gaps, missing edge cases, or suggest splitting into multiple issues
6. Present the draft for review

You iterate on the draft until you're satisfied, then approve it. Claude creates the issue on GitHub via the MCP GitHub server.

### 2. Implement the Issue

**Command:** `/user:work-issue <issue-number>`

Point Claude Code at a GitHub issue number. It will:

1. Resolve the repo from `git remote origin`
2. Fetch the issue details from GitHub (MCP `get_issue`)
3. Explore the codebase to identify all files that need changes
4. Create a feature branch (`issue-{number}-{short-description}`)
5. Implement the changes following existing conventions
6. Build (`dotnet build`) and fix any compilation errors
7. Run tests (`dotnet test`) and fix any failures
8. Validate changes against the project's architecture and coding standards
9. Commit with a message referencing the issue (e.g., `fix #14: remove RabbitMQ code`)
10. Push and create a pull request that references `Closes #N`

The commit and PR steps use Claude Code's built-in `/commit` and `/pr` skills. Skills are prompt templates that enforce consistent formatting (conventional commits, co-author line, structured PR description) on top of the same underlying tools (git, GitHub MCP). They don't add new capabilities -- they standardize *how* the tools are used.

The PR is left open for human review -- Claude never merges it.

### 3. Code Review by Grumpy Reviewer

**Trigger:** Comment `/grumpy` on any open pull request.

A GitHub Actions workflow ([`.github/workflows/grumpy-reviewer.lock.yml`](.github/workflows/grumpy-reviewer.lock.yml)) kicks in:

1. **Activation** -- Validates the commenter has write access and the comment is on a PR
2. **Agent** -- Spins up a sandboxed Claude Code instance in CI with read-only access to the repo and PR diff
3. **Analysis** -- The agent reviews changed files for code smells, performance issues, security concerns, naming, error handling, over/under-engineering
4. **Review comments** -- Posts up to 5 inline review comments in a grumpy-but-constructive tone
5. **Verdict** -- Submits a formal review (`APPROVE`, `REQUEST_CHANGES`, or `COMMENT`)
6. **Threat detection** -- A second pass verifies the agent's output for safety before publishing
7. **Safe outputs** -- Review comments are posted to the PR only after passing all checks

The reviewer persona is a sarcastic senior developer with 40+ years of experience. It has persistent memory across reviews to track recurring patterns and avoid repeating itself.

### End-to-End Flow

```
You describe a feature
        |
        v
/user:draft-issue "add pagination to books endpoint"
        |
        v
Claude explores codebase, drafts issue, suggests improvements
        |
        v
You review, iterate, approve --> Issue created on GitHub
        |
        v
/user:work-issue 42
        |
        v
Claude branches, implements, builds, tests, commits, opens PR
        |
        v
Comment "/grumpy" on the PR
        |
        v
Grumpy Reviewer analyzes diff, posts inline comments, submits review
        |
        v
You address feedback, merge when ready
```
