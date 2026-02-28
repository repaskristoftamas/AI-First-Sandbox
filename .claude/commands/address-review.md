# Address PR review feedback

## Input

Optional PR number. If provided, work on that PR. If omitted, discover the PR for the current branch or list all PRs with requested changes and prompt the user to pick one.

> $ARGUMENTS

## Workflow

### Phase 1: Identify the PR

1. **Resolve the repository** — Determine the GitHub owner/repo from the current git remote origin (`git remote get-url origin`). Parse out the `owner/repo` pair.

2. **Find the target PR**:
   - If `$ARGUMENTS` is a number, use that PR directly.
   - Otherwise, check if the current branch has an open PR: `gh pr view --json number,title,headRefName,reviewDecision 2>/dev/null`.
   - If still unresolved, list all open PRs with changes requested:
     ```bash
     gh pr list --state open --json number,title,headRefName,reviewDecision \
       --jq '.[] | select(.reviewDecision == "CHANGES_REQUESTED") | "\(.number)\t\(.title)\t(\(.headRefName))"'
     ```
   - If multiple PRs are found, present the list and ask the user which one to address.

3. **Verify the PR has changes requested** — If `reviewDecision` is not `CHANGES_REQUESTED`, warn the user that there are no requested changes and ask whether to proceed anyway (there may still be inline comments without a formal review decision).

### Phase 2: Gather feedback

4. **Fetch all review comments** (inline code comments):
   ```bash
   gh pr view <NUMBER> --json reviewRequests,reviews,comments
   gh api repos/<OWNER>/<REPO>/pulls/<NUMBER>/reviews \
     --jq '.[] | {id, state, body, user: .user.login}'
   gh api repos/<OWNER>/<REPO>/pulls/<NUMBER>/comments \
     --jq '.[] | {id, path, line, body, user: .user.login, in_reply_to_id}'
   ```

5. **Fetch the PR diff** for context:
   ```bash
   gh pr diff <NUMBER>
   ```

6. **Read the affected files** — For each file referenced in review comments, read the relevant sections with the Read tool to understand the current code in full context.

### Phase 3: Evaluate feedback

7. **Assess each comment** — For every inline comment and review summary, judge:
   - **Correctness**: Is the reviewer factually right? Does the concern hold given the actual code?
   - **Relevance**: Is it within the PR's scope or a tangential observation?
   - **Severity**: Is it a blocking concern (logic bug, security issue, violation of project conventions) or a subjective style preference that follows the project's existing patterns?
   - **Actionability**: Is there a clear, specific change that addresses it?

8. **Classify each comment** as one of:
   - `fix` — Valid concern that requires a code change.
   - `discuss` — Debatable or subjective; worth a reply but no code change needed.
   - `skip` — Factually incorrect, out of scope, or contradicts established project patterns. Note the reason.

9. **Present your assessment** to the user as a summary table:

   | # | File | Comment (truncated) | Classification | Rationale |
   |---|------|----------------------|----------------|-----------|
   | 1 | `Foo.cs:42` | "This should be..." | fix | ... |
   | 2 | `Bar.cs:10` | "Consider renaming..." | discuss | ... |

   Ask the user to confirm before proceeding with changes, or allow them to reclassify items.

### Phase 4: Fix

10. **Check out the PR branch** if not already on it:
    ```bash
    gh pr checkout <NUMBER>
    ```

11. **Apply all `fix` changes** — Make the necessary code edits. Follow the existing project conventions (Clean Architecture, DDD, coding standards). Do not make unrelated changes.

12. **Build** — Run `dotnet build` and fix any compilation errors.

13. **Test** — Run `dotnet test` and ensure all tests pass. If a fix required logic changes, check whether existing tests cover the new behavior; add tests if needed.

14. **Validate** — Review each fix against the PR diff and the original comment to confirm the concern is fully addressed.

### Phase 5: Commit and push

15. **Commit** — Use the `/commit` skill to stage and commit. The commit message should reflect the nature of the fixes, e.g.:
    - `fix: address PR review feedback` (for multiple fixes)
    - `fix: <specific concern>` (for a single targeted fix)

16. **Push** — Push to the remote branch:
    ```bash
    git push
    ```

### Phase 6: Respond

17. **Reply to each addressed comment** via the GitHub API to close the loop with the reviewer:
    ```bash
    gh api repos/<OWNER>/<REPO>/pulls/<NUMBER>/comments/<COMMENT_ID>/replies \
      -X POST -f body="Fixed in <COMMIT_SHA> — <brief explanation>"
    ```
    For `discuss` items, post a reply explaining your reasoning without making a code change.
    For `skip` items, post a reply explaining why the change was not made (factual correction or project convention reference).

18. **Re-request review** (optional — ask the user):
    ```bash
    gh pr edit <NUMBER> --add-reviewer <ORIGINAL_REVIEWER>
    ```

### Phase 7: Update project board

19. **Move to "Ready for Review"** — Update the project board status:
    ```bash
    gh project item-list 2 --owner repaskristoftamas --format json \
      --jq '.items[] | select(.content.number == <PR_NUMBER>) | .id'
    ```
    Then:
    ```bash
    gh project item-edit --id <ITEM_ID> --project-id PVT_kwHOApxqws4BP2Z2 \
      --field-id PVTSSF_lAHOApxqws4BP2Z2zg-I7Tg \
      --single-select-option-id d9d9aaf5
    ```
    If no linked issue is found, skip this step.

20. **Report back** — Summarize what was fixed, what was discussed, and what was skipped. Provide the PR URL.

## Rules

- Do NOT merge the PR — leave it for human re-review.
- Do NOT make changes beyond what review comments ask for.
- Do NOT skip the evaluation phase — never blindly apply all suggestions; assess correctness first.
- Do NOT reply to comments until after the fixes are committed and pushed.
- If a reviewer's suggestion contradicts the project's established patterns (coding-standards, architecture, etc.), classify it as `skip` and explain why in the reply.
- If a fix introduces ambiguity or requires a design decision, ask the user before implementing.
- Build and test must pass before pushing.
