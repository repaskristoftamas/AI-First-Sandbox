---
description: "Automatic PR review on PR open using .aikb process"
engine:
  id: copilot
  model: gpt-5.3-codex

on:
  pull_request:
    types: [opened, reopened, synchronize]

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
    - "npm:*"
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

Perform a complete PR review for PR #${{ github.event.pull_request.number }}.

The triggering PR number must be used as the review target: #${{ github.event.pull_request.number }}.

Follow these process documents exactly:
- `.aikb/PR_review_process.md`
- `.aikb/CodingConventionsAndMethodologies/DotNet.md` (when relevant)
- `.aikb/localization.md` (for frontend changes)

## Review requirements

1) Initial information gathering
- Fetch PR title, author, status, source branch, target branch, latest commit SHA.
- Resolve all `#NUMBER` issue references from PR title/body and review linked issue requirements.
- Collect commit list and changed files with patches.
- Read complete contents of all changed source files (excluding generated artifacts such as migrations/CoreApiClient), and inspect related files when needed.

2) Code analysis
- Compile code relevant to the PR changes.
- Run tests relevant to the PR changes.
- Integration tests must be skipped because required environment is unavailable.
  - For .NET test commands, always exclude integration tests with:
    `--filter 'FullyQualifiedName!~AEye.IntegrationTests'`
- Verify implementation against PR description and linked issue requirements.
- Check code quality, standards, readability, error handling, consistency, and removal of debug/commented artifacts.
- Check tests for coverage and quality of modified/new behavior.
- Check regressions, edge cases, async safety, and error paths.
- Check dependency/security/license/version impact when dependencies changed.
- Validate UX consistency for user-facing changes.
- For frontend changes, perform localization checks from `.aikb/localization.md`.

3) Review report
- Produce a comprehensive report including:
  - Summary of changes
  - Implementation analysis
  - Code quality assessment
  - Test coverage evaluation
  - Concerns and potential issues
  - Checklist of required changes from PR/issues and whether each is addressed
  - Suggested file review order for humans
- Include this exact structured verdict format and mark one option in bold:

[ ] APPROVE - Ready to merge as is
[ ] APPROVE WITH MINOR COMMENTS - Can be merged, but author should consider the minor suggestions
[ ] REQUEST CHANGES - Requires changes before merging
[ ] NEEDS MORE INFORMATION - Cannot make a determination without additional context

Required Changes:
1.
2.
or "None"

Suggestions:
1.
2.
or "None"

4) Final output behavior
- Post the full review report as a comment on the triggering PR #${{ github.event.pull_request.number }}.
- Do not modify code or open fix PRs.

## Tooling preference

- Prefer GitHub MCP tools for PR/issue metadata and file diffs.
- If needed, use `gh` CLI commands for equivalent read operations.
