---
name: test-and-fix
description: Runs the full test suite, diagnoses any failures, and fixes them. Use after making changes.
user-invocable: true
---

## Instructions

1. Build the project first: `dotnet build --nologo -v q`
2. If build fails, fix build errors first (delegate to debugger agent if complex).
3. Run all tests: `dotnet test --nologo -v q`
4. If tests pass, report success and stop.
5. If tests fail:
   a. Parse the failure output -- identify which tests failed and why.
   b. Read the failing test code to understand what it expects.
   c. Read the code under test to understand current behavior.
   d. Determine if the test or the code is wrong:
      - If the code is wrong: fix the code.
      - If the test needs updating due to intentional changes: update the test.
   e. Run tests again to verify the fix.
   f. Repeat until all tests pass.

## Rules
- Never delete or skip failing tests to make the suite pass
- Never weaken assertions to make tests pass
- If a test is genuinely obsolete, explain why before removing it
- Always run the full suite at the end, not just the previously failing tests
- Never use `cd <path> && dotnet ...` compound commands — they trigger security hooks. Run dotnet commands directly (the working directory is already the repo root) or pass the solution/project path as an argument if needed
