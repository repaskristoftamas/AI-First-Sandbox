# Review back on addressed grumpy review (CI — autonomous)

Autonomous re-evaluation of how the addresser handled grumpy review feedback.

## Input

PR number and PR author login, space-separated.

> $ARGUMENTS

## Workflow

### Phase 1: Resolve context

1. Parse PR number (first token) and author login (second token) from `$ARGUMENTS`.
2. Determine owner/repo: `git remote get-url origin`.
3. Fetch all review comments and their reply threads:
   ```bash
   gh api repos/<OWNER>/<REPO>/pulls/<NUMBER>/comments \
     --paginate --jq '.[] | {id, path, line, body, user: .user.login, in_reply_to_id, created_at}'
   ```
4. Fetch all reviews to identify the grumpy reviewer's review:
   ```bash
   gh api repos/<OWNER>/<REPO>/pulls/<NUMBER>/reviews \
     --jq '.[] | {id, state, body, user: .user.login}'
   ```
5. Build a conversation map: group comments by thread (using `in_reply_to_id`).
6. Keep only conversations where:
   - The root comment belongs to a grumpy reviewer review (the review body contains `gh-aw-agentic-workflow`).
   - `github-actions[bot]` posted a reply (the addresser).

### Phase 2: Classify each addresser response

7. For each addresser reply, determine the action taken (reply format is defined in `.claude/commands/address-review-ci.md` Phase 4):
   - **Fixed**: Reply contains "Fixed in" followed by a commit SHA.
   - **Discussed**: Reply explains reasoning without referencing a fix commit.
   - **Skipped**: Reply explains why no change was made (factual correction or convention reference).

### Phase 3: Evaluate fixes

8. For each **fixed** conversation:
   - Extract the commit SHA from the addresser's reply.
   - View the commit diff: `git show <SHA> --stat` and `git show <SHA> -- <FILE>`.
   - Read the current state of the file at the lines the grumpy reviewer commented on.
   - Assess: Does the fix actually address the grumpy reviewer's concern? Is it correct? Did it introduce any new issues?
   - Verdict: `adequate`, `incomplete`, or `incorrect`.

### Phase 4: Evaluate discussions and skips

9. For each **discussed** or **skipped** conversation:
   - Read the original grumpy review comment carefully.
   - Read the addresser's reply carefully.
   - Read the relevant source code and project conventions (CLAUDE.md, .claude/rules/).
   - Assess: Is the addresser's reasoning sound? Was the grumpy reviewer right?
   - Verdict: `agree-with-addresser`, `agree-with-reviewer`, or `needs-human-judgment`.

### Phase 5: Post replies

10. For each conversation, post a reply tagging the PR author. Reply to the **last comment** in the thread (the addresser's reply) so the response appears at the end:
    ```bash
    gh api repos/<OWNER>/<REPO>/pulls/<NUMBER>/comments/<COMMENT_ID>/replies \
      -X POST -f body="<response>"
    ```

    Reply format by verdict:

    **Fixed + adequate:**
    > @author The fix in `<SHA>` addresses this concern. The change looks correct.

    **Fixed + incomplete:**
    Post the evaluation as a **new top-level issue comment** (not an inline reply) so it triggers the `/address` workflow:
    ```bash
    gh api repos/<OWNER>/<REPO>/issues/<NUMBER>/comments \
      -X POST -f body="/address [Concise description of the specific gap that remains after <SHA>]"
    ```
    Also reply inline to the thread for visibility:
    > @author The fix in `<SHA>` partially addresses this, but [specific gap]. Triggered a follow-up `/address` to resolve the remaining concern.

    **Fixed + incorrect:**
    Post the evaluation as a **new top-level issue comment** to trigger the `/address` workflow:
    ```bash
    gh api repos/<OWNER>/<REPO>/issues/<NUMBER>/comments \
      -X POST -f body="/address [Concise description of what's still wrong after <SHA>]"
    ```
    Also reply inline to the thread for visibility:
    > @author The fix in `<SHA>` doesn't fully resolve this. [Explanation]. Triggered a follow-up `/address` to fix the remaining issue.

    **Discussed + agree-with-addresser:**
    > @author The addresser's reasoning holds here. [Brief supporting rationale].

    **Discussed + agree-with-reviewer:**
    Post as a **new top-level issue comment** to trigger the `/address` workflow:
    ```bash
    gh api repos/<OWNER>/<REPO>/issues/<NUMBER>/comments \
      -X POST -f body="/address [Concise description of the reviewer's valid concern]"
    ```
    Also reply inline:
    > @author The original reviewer has a point on this one. [Brief explanation]. Triggered a follow-up `/address`.

    **Discussed/Skipped + needs-human-judgment:**
    > @author This is a judgment call — both sides have valid points. [Summarize the trade-off]. Your call.

## Rules

- NEVER resolve any conversation thread.
- NEVER approve or dismiss the review.
- NEVER make code changes or push commits.
- Always tag the PR author with `@<author>` at the start of every reply.
- Be concise and neutral — this is an objective arbiter, not a grumpy personality.
- When evaluating fixes, look at the actual code diff, not just the commit message.
- The `COMMENT_ID` for inline replies should be the last comment in the thread (the addresser's reply).
- If a conversation has no addresser reply, skip it silently.
- **Loop prevention**: Before posting a `/address` comment, check existing issue comments for prior `/address` comments on the same concern. If a `/address` has already been posted for the same thread/topic and the addresser has already attempted a fix for it, do NOT post another `/address`. Instead, reply inline tagging the author: `@author This concern has been addressed twice but remains unresolved. Needs manual attention.`
- The `/address` comment body must be a self-contained description of the concern — the addresser will use only this text to scope its work, so include enough context (file, concept, what's wrong) without being verbose.
