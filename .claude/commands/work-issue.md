# Implement GitHub issue #$ARGUMENTS

## Workflow

1. **Resolve the repository** — Determine the GitHub owner/repo from the current git remote origin (`git remote get-url origin`). Parse out the `owner/repo` pair. All subsequent GitHub operations must use this resolved owner and repo.

2. **Read the issue** — Fetch issue #$ARGUMENTS details using `gh issue view $ARGUMENTS --json number,title,body,labels`. Understand the summary, scope, target implementation, and acceptance criteria.

3. **Explore the codebase** — Before writing any code, use the Explore subagent or Glob/Grep/Read tools to understand the current state of the files you'll be modifying. Identify all files that need changes.

4. **Update project board** — Move the issue to "In Progress":
   ```bash
   ITEM_ID=$(gh project item-list 2 --owner repaskristoftamas --format json \
     --jq '.items[] | select(.content.number == $ARGUMENTS) | .id')
   gh project item-edit --id "$ITEM_ID" --project-id PVT_kwHOApxqws4BP2Z2 \
     --field-id PVTSSF_lAHOApxqws4BP2Z2zg-I7Tg \
     --single-select-option-id 47fc9ee4
   ```

5. **Create a feature branch** — Branch from `main` with the naming convention: `issue-{number}-{short-kebab-description}` (e.g., `issue-14-remove-rabbitmq`).

6. **Implement** — Make all changes described in the issue. Follow existing code conventions. Do not add unrelated changes, extra comments, or over-engineer beyond what the issue asks for.

7. **Build** — Run `dotnet build` on the entire solution (not just the changed project) and fix any compilation errors, including in test projects.

8. **Test** — Before running tests, assess coverage:
   - If the issue adds new behavior, write tests that cover it.
   - If your changes alter a public API (e.g., constructor signatures, method return types), update all affected tests.
   - Use the `/test-and-fix` skill to run the full test suite and fix any failures.

9. **Validate** — Review your changes against Clean Architecture and the patterns and coding conventions used in the project before committing.

10. **Commit** — Use the `/commit` skill to stage and commit. The commit message must reference the issue number (e.g., `fix #14: remove all RabbitMQ-related code`).

11. **Push and create PR** — Push the branch, then use the `/pr` skill to create the pull request. Ensure the PR body references `Closes #$ARGUMENTS` and the target branch is `main`.

## Rules

- Do NOT merge the PR — leave it for human review.
- Do NOT modify files unrelated to the issue.
- If the issue body references a dependency on another issue, check if that issue is closed. If it's still open, warn and stop.
- If you encounter ambiguity in the issue requirements, ask the user before guessing.
