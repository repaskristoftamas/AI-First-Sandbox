---
description: "Comprehensive PR code review on open using Copilot CLI"
engine:
  id: copilot
  model: gpt-5.3-codex

on:
  pull_request:
    types: [opened]

permissions:
  contents: read
  pull-requests: read
  issues: read

tools:
  github:
    mode: remote
    toolsets: [default]
    read-only: true
    github-token: ${{ secrets.GH_AW_GITHUB_MCP_SERVER_TOKEN || secrets.GH_AW_GITHUB_TOKEN || secrets.GITHUB_TOKEN }}
  bash:
    - "gh:*"
    - "git:*"
    - "dotnet:*"
    - "ls"
    - "find:*"
    - "grep:*"
    - "cat"
    - "head"
    - "tail"
    - "wc"
    - "echo"
    - "pwd"

safe-outputs:
  add-comment:
    max: 1
    target: "triggering"

---

# Pull Request Review On Open

Perform a complete PR review for PR #${{ github.event.pull_request.number }} in `${{ github.repository }}`.

The triggering PR number must be used as the review target: #${{ github.event.pull_request.number }}.

Follow these convention documents exactly (read them from the repository before reviewing):
- `CLAUDE.md` (project architecture, code style, testing, and conventions)
- `.claude/rules/coding-standards.md` (C#/.NET coding standards)
- `.claude/rules/security.md` (security rules)
- `.claude/rules/testing.md` (testing standards)
- `.claude/rules/git-workflow.md` (git and commit conventions)

## Review requirements

### 1) Initial information gathering
- Fetch PR title, author, status, source branch, target branch, latest commit SHA.
- Resolve all `#NUMBER` issue references from PR title/body and review linked issue requirements.
- Collect commit list and changed files with patches.
- Read complete contents of all changed source files (excluding generated artifacts such as migrations), and inspect related files when needed.
- Read the convention documents listed above so you can evaluate the code against them.

### 2) Code analysis
- Build the solution:
  ```
  dotnet build Bookstore.slnx --configuration Release
  ```
- Run tests relevant to the PR changes:
  ```
  dotnet test Bookstore.slnx --configuration Release --logger "console;verbosity=normal"
  ```
- Verify implementation against PR description and linked issue requirements.
- Check code quality against the conventions in `CLAUDE.md` and `.claude/rules/coding-standards.md`:
  - File-scoped namespaces, primary constructors, `sealed` classes, records for DTOs
  - Result pattern over exceptions for expected failures
  - Clean Architecture layering (dependencies flow inward, Domain has zero external dependencies)
  - CQRS handlers follow the established pattern (ICommand/IQuery via Mediator)
  - FluentValidation validators are present for new commands
  - XML summaries on public members and private methods
  - No dead code, no commented-out code, no regions
  - Meaningful naming, no unnecessary abbreviations
- Check security concerns against `.claude/rules/security.md`:
  - No secrets or connection strings committed
  - Input validation at API boundaries
  - Parameterized queries / EF Core usage (no string concatenation for SQL)
- Check test quality against `.claude/rules/testing.md`:
  - xUnit + Shouldly conventions
  - Arrange-Act-Assert pattern
  - Test naming: `MethodName_Scenario_ExpectedResult`
  - InMemory EF Core DbContext for handler tests
  - Coverage of new/modified behavior
- Check git conventions against `.claude/rules/git-workflow.md`:
  - Conventional Commits format
  - Feature branch naming: `issue-{number}-{short-kebab-description}`
- Check for regressions, edge cases, async safety, and error paths.
- Check dependency/security/license impact when dependencies changed.

### 3) Review report

Produce a comprehensive report including:
- **Summary of changes**: What the PR does in 2-3 sentences
- **Implementation analysis**: How it was implemented, architectural decisions
- **Code quality assessment**: Adherence to project conventions and coding standards
- **Test coverage evaluation**: Quality and coverage of tests for modified/new behavior
- **Concerns and potential issues**: Bugs, edge cases, security, performance
- **Requirements checklist**: List of required changes from PR description/linked issues and whether each is addressed
- **Suggested file review order**: For human reviewers

Include this exact structured verdict format and mark one option in bold:

```
[ ] APPROVE — Ready to merge as is
[ ] APPROVE WITH MINOR COMMENTS — Can be merged, but author should consider the minor suggestions
[ ] REQUEST CHANGES — Requires changes before merging
[ ] NEEDS MORE INFORMATION — Cannot make a determination without additional context

Required Changes:
1.
2.
or "None"

Suggestions:
1.
2.
or "None"
```

### 4) Final output behavior
- Post the full review report as a single comment on the triggering PR #${{ github.event.pull_request.number }}.
- Do not modify code or open fix PRs.

## Tooling preference

- Use `gh` CLI commands for PR/issue metadata and file diffs.
