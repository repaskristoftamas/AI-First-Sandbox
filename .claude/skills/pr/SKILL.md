---
name: pr
description: Creates a GitHub pull request with a well-structured title and description.
user-invocable: true
---

## Instructions

1. Run `git status`, `git log main..HEAD --oneline`, and `git diff main...HEAD --stat` to understand all changes on this branch.
2. Check if the branch is pushed to remote (`git rev-parse --abbrev-ref --symbolic-full-name @{u}`).
3. Analyze ALL commits on this branch (not just the latest):
   - What features were added?
   - What bugs were fixed?
   - What was refactored?
4. Draft the PR:
   - **Title**: Short, under 70 chars, describes the outcome
   - **Body**: Use the template below
5. Push to remote if needed (`git push -u origin <branch>`).
6. Create the PR with `gh pr create`.

## PR Body Template

```
## Summary
<1-3 bullet points describing what this PR does and why>

## Changes
<Bulleted list of notable changes>

## Test plan
- [ ] <How to verify this works>
```

## Rules
- Never create PRs to branches other than main unless asked
- Include the full commit range in your analysis, not just the tip
- Never use `cd <path> && git ...` compound commands — they trigger security hooks. Run git/gh commands directly (the working directory is already the repo root) or use `git -C <path>` if needed
