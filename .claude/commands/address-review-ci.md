# Address PR review feedback (CI — autonomous)

Autonomous variant of address-review for use in CI. No user confirmation steps.

## Input

PR number to address.

> $ARGUMENTS

## Workflow

### Phase 1: Resolve context

1. Parse PR number from `$ARGUMENTS`.
2. Determine owner/repo: `git remote get-url origin`.
3. Fetch all review comments:
   ```bash
   gh api repos/<OWNER>/<REPO>/pulls/<NUMBER>/reviews \
     --jq '.[] | {id, state, body, user: .user.login}'
   gh api repos/<OWNER>/<REPO>/pulls/<NUMBER>/comments \
     --jq '.[] | {id, path, line, body, user: .user.login}'
   ```
4. Fetch the PR diff for context: `gh pr diff <NUMBER>`
5. Read each file referenced in the comments.

### Phase 2: Evaluate

6. For each comment, classify as:
   - `fix` — Factually valid concern with a clear code change.
   - `discuss` — Debatable or subjective; reply with reasoning, no code change.
   - `skip` — Factually wrong, out of scope, or contradicts established project patterns.

   Apply the project's coding standards, architecture rules, and testing standards when evaluating.

### Phase 3: Fix

7. Apply all `fix` changes to the codebase.
8. Build: `dotnet build --nologo -v q`
9. Test: `dotnet test --nologo -v q`
   - If tests fail, fix them. If a fix introduced new behavior with no coverage, add tests.
10. Use the `/commit` skill to commit. Message: `fix: address PR review feedback`

### Phase 4: Respond

11. Reply to every comment via the GitHub API:
    ```bash
    gh api repos/<OWNER>/<REPO>/pulls/<NUMBER>/comments/<COMMENT_ID>/replies \
      -X POST -f body="<response>"
    ```
    - `fix`: "Fixed in <SHA> — <one-line explanation>"
    - `discuss`: Explain reasoning without making a code change.
    - `skip`: Explain why (factual correction or project convention reference).

### Phase 5: Update project board

12. Move the linked issue to "Ready for Review":
    ```bash
    ITEM_ID=$(gh project item-list 2 --owner repaskristoftamas --format json \
      --jq '.items[] | select(.content.number == <NUMBER>) | .id')
    gh project item-edit --id "$ITEM_ID" --project-id PVT_kwHOApxqws4BP2Z2 \
      --field-id PVTSSF_lAHOApxqws4BP2Z2zg-I7Tg \
      --single-select-option-id d9d9aaf5
    ```
    Skip if no linked issue exists.

## Rules

- Never merge the PR.
- Never make changes beyond what review comments ask for.
- Never blindly apply all suggestions — evaluate correctness first.
- Build and tests must pass before committing.
- Do not push — the CI workflow handles the push step.
