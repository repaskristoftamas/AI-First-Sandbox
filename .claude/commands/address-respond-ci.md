# Post reply comments for addressed PR review feedback (CI — autonomous)

Runs as a separate invocation after `/address-review-ci` has fixed, committed, and pushed code changes. Posts reply comments to each review thread and updates the project board.

## Input

PR number, optionally followed by `--focus <description>` to scope replies to a specific concern.

> $ARGUMENTS

## Workflow

### Phase 1: Resolve context

1. Parse `$ARGUMENTS`:
   - First token: PR number.
   - If `--focus` is present, everything after it is the **focus description**. Enter **focused mode**.
   - If `--focus` is absent, enter **full scan mode**.
2. Determine owner/repo: `git remote get-url origin`.
3. Fetch all review comments with thread info:
   ```bash
   gh api repos/<OWNER>/<REPO>/pulls/<NUMBER>/comments \
     --paginate --jq '.[] | {id, path, line, body, user: .user.login, in_reply_to_id}'
   ```
4. Fetch reviews to identify grumpy reviewer comments:
   ```bash
   gh api repos/<OWNER>/<REPO>/pulls/<NUMBER>/reviews \
     --jq '.[] | {id, state, body, user: .user.login}'
   ```
5. Get the most recent fix commit SHA (the commit pushed by the addresser):
   ```bash
   git log origin/main..HEAD --oneline -1
   ```
6. Get the diff of that commit to understand what was changed:
   ```bash
   git show <SHA> --stat
   git show <SHA>
   ```
7. Read each file referenced in the grumpy reviewer's comments.

### Phase 2: Classify each comment

8. Build a list of grumpy reviewer comments (root comments from reviews whose body contains `gh-aw-agentic-workflow`).
9. **Skip** any comment that already has a `github-actions[bot]` reply — it was addressed in a previous round.
10. **Focused mode only**: Skip comments not related to the focus description.
11. For each remaining comment, classify by comparing the comment against the commit diff and project rules:
    - `fix` — The commit changed code at or near the file/lines the comment references, addressing the concern.
    - `discuss` — The comment raises a debatable or subjective point; the code was intentionally left unchanged.
    - `skip` — The comment is factually wrong, out of scope, or contradicts established project patterns.

    Apply the project's coding standards, architecture rules, and testing standards when evaluating.

### Phase 3: Post replies

12. For each classified comment, post a reply:
    ```bash
    gh api repos/<OWNER>/<REPO>/pulls/<NUMBER>/comments/<COMMENT_ID>/replies \
      -X POST -f body="<response>"
    ```
    - `fix`: "Fixed in <SHA> — <one-line explanation>"
    - `discuss`: Explain reasoning without making a code change.
    - `skip`: Explain why (factual correction or project convention reference).

### Phase 4: Update project board

13. Move the linked issue to "Ready for Review":
    ```bash
    ITEM_ID=$(gh project item-list 2 --owner repaskristoftamas --format json \
      --jq '.items[] | select(.content.number == <NUMBER>) | .id')
    gh project item-edit --id "$ITEM_ID" --project-id PVT_kwHOApxqws4BP2Z2 \
      --field-id PVTSSF_lAHOApxqws4BP2Z2zg-I7Tg \
      --single-select-option-id d9d9aaf5
    ```
    Skip if no linked issue exists.

## Rules

- Never make code changes or push commits — this command only posts comments and updates the board.
- Never merge the PR.
- Skip comments that already have a `github-actions[bot]` reply.
- Post exactly one reply per comment — never duplicate.
- Every comment must get a reply. Do not stop after posting some replies.
