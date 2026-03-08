---
name: commit
description: Creates a well-formatted conventional commit. Analyzes staged/unstaged changes and drafts a commit message.
user-invocable: true
---

## Instructions

1. Run `git status` and `git diff` (staged + unstaged) to understand all changes.
2. Run `git log --oneline -5` to see recent commit style.
3. Analyze the changes:
   - What was added, changed, or removed?
   - What is the purpose/motivation?
   - Which type fits: feat, fix, refactor, test, docs, chore, ci, perf?
4. Stage the appropriate files (prefer specific files over `git add -A`).
5. Draft a commit message following Conventional Commits:
   - Format: `type(scope): short description`
   - Subject: imperative, max 72 chars, no period
   - Body (if needed): explain "why", not "what"
6. Create the commit.
7. Show the result with `git log --oneline -3`.

## Rules
- Never commit .env, credentials, or secrets
- Never use --no-verify
- Never amend unless explicitly asked
- If pre-commit hook fails, fix the issue and create a NEW commit
- Never use `cd <path> && git ...` compound commands — they trigger security hooks. Run git commands directly (the working directory is already the repo root) or use `git -C <path>` if needed
