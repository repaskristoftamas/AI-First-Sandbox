# Address PR review feedback (CI — autonomous)

Autonomous variant of address-review for use in CI. No user confirmation steps.

## Input

PR number to address, optionally followed by `--focus <description>` to scope work to a specific concern raised by the review-back bot.

> $ARGUMENTS

## Workflow

### Phase 1: Resolve context

1. Parse `$ARGUMENTS`:
   - First token: PR number.
   - If `--focus` is present, everything after it is the **focus description** — a specific concern to address. Enter **focused mode**.
   - If `--focus` is absent, enter **full scan mode** (original behavior).
2. Determine owner/repo: `git remote get-url origin`.
3. **Full scan mode** — Fetch all review comments:
   ```bash
   gh api repos/<OWNER>/<REPO>/pulls/<NUMBER>/reviews \
     --jq '.[] | {id, state, body, user: .user.login}'
   gh api repos/<OWNER>/<REPO>/pulls/<NUMBER>/comments \
     --jq '.[] | {id, path, line, body, user: .user.login}'
   ```
4. **Focused mode** — Fetch all review comments and issue comments to find the thread related to the focus description:
   ```bash
   gh api repos/<OWNER>/<REPO>/pulls/<NUMBER>/comments \
     --paginate --jq '.[] | {id, path, line, body, user: .user.login, in_reply_to_id}'
   gh api repos/<OWNER>/<REPO>/issues/<NUMBER>/comments \
     --jq '.[] | {id, body, user: .user.login}'
   ```
   Match the focus description against comment threads to identify which conversation(s) to address. Only process those — ignore all other threads.
5. Fetch the PR diff for context: `gh pr diff <NUMBER>`
6. Read each file referenced in the matched comments.

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

Reply posting and project board updates are handled by a separate `/address-respond-ci` invocation in the CI workflow. Do NOT post replies or update the project board here.

## Rules

- Never merge the PR.
- Never make changes beyond what review comments ask for.
- Never blindly apply all suggestions — evaluate correctness first.
- Build and tests must pass before committing.
- Do NOT post reply comments to review threads — that is handled by `/address-respond-ci`.
