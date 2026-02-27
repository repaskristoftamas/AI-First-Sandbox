---
name: Weekly Project Status
description: |
  Generates a lightweight weekly summary of the BookStore project board activity,
  covering completed, in-progress, and planned work. Posts the result as a GitHub issue.

on:
  # 9 AM UTC = 11 AM CEST / 10 AM CET — ready well before noon
  schedule: "0 9 * * 1"
  workflow_dispatch:

permissions:
  contents: read
  issues: read
  pull-requests: read

tools:
  github:
    lockdown: false

safe-outputs:
  create-issue:
    title-prefix: "[weekly-status] "
    labels: [report, weekly-status]

engine: claude
---

# Weekly Project Status

Generate a concise weekly status report for the **BookStore** project board.

## Step 1: Fetch Project Board Data

```bash
gh project item-list 2 --owner repaskristoftamas --format json -L 100
```

Filter by status:

```bash
# Done
gh project item-list 2 --owner repaskristoftamas --format json -L 100 \
  --jq '.items[] | select(.status == "Done") | {title: .title, number: .content.number}'

# In Progress
gh project item-list 2 --owner repaskristoftamas --format json -L 100 \
  --jq '.items[] | select(.status == "In Progress") | {title: .title, number: .content.number}'

# Ready for Review
gh project item-list 2 --owner repaskristoftamas --format json -L 100 \
  --jq '.items[] | select(.status == "Ready for Review") | {title: .title, number: .content.number, prs: .["linked pull requests"]}'

# Todo
gh project item-list 2 --owner repaskristoftamas --format json -L 100 \
  --jq '.items[] | select(.status == "Todo") | {title: .title, number: .content.number}'
```

## Step 2: Fetch Recent Repository Activity

Use the GitHub tools to gather the last 7 days of activity:

- Merged pull requests
- Closed issues
- Newly opened issues or PRs

## Step 3: Create the Issue

Create a GitHub issue using the `create-issue` safe output:

- **Title**: `Weekly status – <YYYY-MM-DD>` (today's date)
- **Body**: report following the structure below

Keep it short — bullet points only, no filler text.

## Report Structure

```markdown
## Done this week
- #N Title

## In progress
- #N Title

## In review
- #N Title (PR: #M)

## Planned (Todo)
- #N Title
```

Omit any section that has no items.
