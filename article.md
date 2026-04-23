Claude Code

---
  1. CV Bullet Point (250 chars max)

  ▎ Designed and built an AI-native CI/CD pipeline where AI agents autonomously implement GitHub issues, review PRs, address feedback, and back-review fixes — orchestrated via GitHub Actions, self-hosted runners, and Claude    
  ▎ Code — reducing human intervention to final approval.
  (249 characters)

  ---
  2. LinkedIn About Section (STAR Story, max 1000 chars)

  ▎ Situation: Modern software teams spend significant time on repetitive SDLC tasks — implementing well-scoped tickets, reviewing PRs, and addressing review feedback — creating a bottleneck that limits throughput.
  ▎
  ▎ Task: I set out to build a fully automated, multi-agent development pipeline where AI handles the entire cycle from issue to merge-ready PR, and the human only makes the final call.
  ▎
  ▎ Action: I designed an agentic CI/CD system using Claude Code, GitHub Actions, and a Dockerized self-hosted runner. The pipeline works in stages: I describe a feature → an AI agent drafts issues to the project board →       
  ▎ another agent implements the code, writes tests, and opens a PR → a "Grumpy Reviewer" AI reviews the PR → an addresser agent fixes valid concerns and argues back on non-issues → a back-reviewer validates the fixes and can  
  ▎ trigger re-addressing loops. The entire conversation happens autonomously on the PR.
  ▎
  ▎ Result: The human re-enters the loop only at the final review stage, with a fully tested, reviewed, and iterated PR. This shifted my role from writing code to designing systems and making judgment calls — a fundamentally   
  (993 characters)to build software.

  ---
  3. Medium Article — Content Skeleton

  Title: "Building an Agentic CI/CD Pipeline: How I Automated My Entire SDLC with AI Agents"

  1. HOOK
     - The moment I realized I was only merging PRs I never wrote
     - What "human-in-the-loop" actually looks like when AI handles everything else

  2. THE PROBLEM
     - Repetitive SDLC work: implement ticket → PR → review → address feedback → re-review
     - Human bottleneck: context switching, review fatigue, feedback cycles
     - Why traditional CI/CD automates builds/tests but not the cognitive work

  3. THE ARCHITECTURE (high-level diagram)
     - Stage 0: Human ideation → AI drafts issues to project board
     - Stage 1: AI implementation agent (/work-issue)
     - Stage 2: AI code review (Grumpy Reviewer via GitHub Agentic Workflows)
     - Stage 3: AI addresser (self-hosted runner + Claude Code)
     - Stage 4: AI back-reviewer (validates fixes, triggers re-addressing if needed)
     - Stage 5: Human final review and merge
     - Supporting infra: CI pipeline, project board automation, daily/weekly status reports

  4. DEEP DIVE: THE IMPLEMENTATION AGENT
     - Custom Claude Code skills (/work-issue, /commit, /pr, /test-and-fix)
     - How CLAUDE.md and .claude/rules/ encode project conventions as AI-readable specs
     - The importance of explicit coding standards for deterministic AI output

  5. DEEP DIVE: THE REVIEW LOOP
     - Grumpy Reviewer: opinionated AI reviewer triggered on PR open
     - Addresser: evaluates each comment (fix / discuss / skip), makes changes, replies
     - Back-reviewer: validates fixes, can trigger re-addressing loops
     - Loop prevention: how to avoid infinite AI-arguing-with-AI cycles

  6. THE SELF-HOSTED RUNNER
     - Why a Dockerized runner with Claude Code pre-installed
     - Auth via subscription (not API keys)
     - Volume-mounting Claude credentials into the container

  7. KEY DESIGN DECISIONS
     - Why the human only enters at final review (trust boundary)
     - Why AI agents argue with each other (adversarial quality assurance)
     - Why coding standards must be machine-readable (CLAUDE.md as system prompt)
     - Event-driven chaining vs. monolithic workflow

  8. WHAT I LEARNED
     - AI is better at following conventions than inventing them
     - The bottleneck shifted from writing code to writing good issue descriptions
     - Adversarial multi-agent review catches more issues than single-agent
     - You need loop prevention mechanisms
     - The role of the developer changes: architect + judge, not typist

  9. LIMITATIONS & HONEST ASSESSMENT
     - Where it breaks down (ambiguous requirements, novel architecture decisions)
     - Cost considerations
     - When human-in-the-loop should be earlier

  10. WHAT'S NEXT
      - Scaling to multi-repo / team workflows
      - Metrics: cycle time, fix quality, human override rate
      - The broader trend: Continuous AI as the next evolution of CI/CD

  11. CLOSING
      - This isn't about replacing developers — it's about redefining what a developer does
      - The best code I wrote this year was the pipeline that writes the code

  ---

  Copilot

  Built an end-to-end agentic CI/CD pipeline where AI agents autonomously write issues, implement features, review PRs, address feedback, and back-review fixes — human involvement limited to idea input and final approval.

---

  Situation: I wanted to explore how far AI could be pushed inside a real software development lifecycle — not just as a copilot, but as the entire engineering team.

Task: Design a fully automated, self-correcting development pipeline where AI agents handle every phase from issue creation to code review, with the human acting only as a product owner.

Action: I built an agentic CI/CD workflow on a .NET Clean Architecture API project. Claude Code drafts GitHub issues and implements them. On PR open, two AI reviewers fire in parallel — a thorough architectural reviewer and a "grumpy senior dev" persona. A self-hosted Claude Code runner then addresses every review comment autonomously, commits fixes, and responds to each remark. A final back-review agent validates the fixes and only then requests human approval.

Result: A fully autonomous review–fix–verify loop running on GitHub Actions, where the human is the idea source and the final gate — nothing else.


---

Title: "I Replaced My Entire Code Review Process With AI Agents — Here's What Happened"

1. Hook / Intro
   - The premise: what if you removed yourself from the dev loop almost entirely?
   - The experiment: a real .NET project, real code, real review standards

2. The Problem With Existing "AI-Assisted" Dev
   - Copilot autocomplete is still human-driven
   - AI code review tools are one-shot, passive, non-acting
   - The gap: no agent acts on its own findings

3. Architecture of the Pipeline
   - Stage 1: Idea → Issue (Claude Code in VS Code drafts structured GitHub issues)
   - Stage 2: Issue → Implementation (Claude Code implements, commits, pushes, opens PR, updates project board)
   - Stage 3: PR Open → Dual AI Review (GitHub Actions triggers two reviewers in parallel)
     - The Architectural Reviewer: builds, tests, checks conventions, posts structured verdict
     - The Grumpy Reviewer: inline comments with a senior-dev persona, persistent memory
   - Stage 4: Review → Address (self-hosted runner, Claude Code reads comments, fixes code, pushes)
   - Stage 5: Addressing → Back-Review (validates fixes, argues or accepts, always comments)
   - Stage 6: Back-Review → Human (PR review request sent only now)

4. Technical Deep-Dive
   - GitHub Actions as the orchestration layer
   - Self-hosted runners: Claude Code running in Docker on local machine, subscription auth
   - GitHub Copilot Coding Agent workflows (.md prompt files as workflow definitions)
   - Triggering chains: PR open → review → review submitted → address → back-review → human
   - Custom Claude Code slash commands (/address-review-ci, /review-back-ci)
   - Project board automation (status field transitions via GraphQL)

5. What the AI Does Well
   - Consistent convention enforcement
   - Never forgets to check test coverage, XML summaries, naming
   - Faster than a human reviewer on mechanical rules
   - The "grumpy" persona actually surfaces real issues with good specificity

6. Where It Still Needs a Human
   - Product/domain judgment calls
   - Architecture decisions that require intent context
   - Final trust and merge authority
   - Designing the prompts and rules themselves

7. Lessons & Surprises
   - Prompt engineering IS software engineering
   - Persona matters: the grumpy reviewer found different issues than the structured one
   - Self-hosted runner with persistent auth changes the cost model entirely
   - The loop catches real bugs before any human sees the code

8. What to Call This Pattern
   - "Agentic CI/CD" or "Continuous AI" — definition and distinction from plain CI
   - The human as product owner, not task executor

9. Conclusion & What's Next
   - The human role is shifting: from implementer to curator
   - Next frontier: agents that refactor, not just fix
   - Open question: where should the human boundary permanently sit?


   ---


   I Replaced My Entire Code Review Process With AI Agents — Here's What Happened
There's a version of AI-assisted development where you tab-complete your way through a feature. And then there's this.

Over the past few weeks, I built a pipeline where an AI agent writes the code, another reviews it, a third fixes the review comments, and a fourth checks whether the fixes were good enough — before I ever look at the pull request. My job is to have the idea and press merge.

This is an account of how I built it, what works surprisingly well, and where the seam between human and machine still matters.

The Problem With "AI-Assisted" Development
Most AI tooling in the dev workflow is reactive. You write, it suggests. You ask, it answers. The human is still the actor; AI is the instrument.

Even the best AI code review tools — the ones that post comments on PRs — are fundamentally passive. They observe and annotate. Nobody acts on the annotations except you.

The gap I wanted to close: what if the agent that finds an issue also fixes it, and another agent verifies the fix?

The Pipeline
The project is a .NET 10 REST API built on Clean Architecture — CQRS, DDD, FluentValidation, EF Core. Real code, real conventions, real complexity. Here's the full lifecycle, stage by stage.

Stage 1: Idea → Issue
In VS Code, I describe what I want built. Claude Code reads the project conventions (CLAUDE.md, architecture docs, coding standards) and drafts a structured GitHub Issue — acceptance criteria, implementation hints, labels — directly onto the project board in Todo status.

No ticket writing. No Jira grooming session. Just a sentence and a well-formed issue.

Stage 2: Issue → Pull Request
Claude Code picks up the issue, creates a feature branch following the issue-{number}-{short-kebab-description} convention, implements the feature against the project's architecture and test standards, writes unit tests, commits with Conventional Commits messages, pushes, opens the PR, and moves the board card to Ready for Review.

The entire implementation loop — from reading the issue to opening the PR — runs autonomously.

Stage 3: PR Open → Dual AI Review
The moment the PR is opened, two independent review workflows fire in parallel via GitHub Actions.

Reviewer 1 — The Architectural Reviewer: Builds the solution, runs the test suite, fetches the full source of changed files, reads the convention documents, and produces a structured report: summary, implementation analysis, code quality assessment, test coverage evaluation, required changes, and suggestions. It posts a verdict in a defined format (APPROVE, REQUEST CHANGES, etc.) as a single PR comment.

Reviewer 2 — The Grumpy Reviewer: A different AI, different model, different persona. A sarcastic senior developer with 40 years of experience who has been reluctantly asked to look at your code. It posts inline review comments directly on the diff — up to five, prioritised by severity — with a persistent memory cache so it doesn't repeat itself across reviews. It remembers patterns it's seen before.

Two reviewers, two perspectives, zero human involvement so far.

Stage 4: Review → Address
When the Grumpy Reviewer submits, a self-hosted GitHub Actions runner wakes up. This runner is a Docker container on my local machine with Claude Code installed and authenticated via subscription (no API key needed). It:

Checks out the PR branch
Runs the /address-review-ci Claude Code slash command, which reads all open review comments and fixes them — editing files, adding tests, updating logic
Pushes the fixes to the PR branch
Runs /address-respond-ci, which posts a reply to each review comment: either confirming the fix was applied, or arguing why the comment is a non-issue (with reasoning)
Every comment gets a response. Every actionable comment gets a code fix. Automatically.

Stage 5: Addressing → Back-Review
Immediately after addressing, the same self-hosted runner fires the back-review job. Another Claude Code invocation (/review-back-ci) reads the original comments and the new diffs and determines for each: was the fix adequate? It posts its verdict as a PR comment — accepting the fix or arguing that the issue wasn't actually resolved. It always comments, never stays silent.

Only after this step does the workflow add the original PR author as a reviewer — triggering a GitHub notification that says: "Your code is ready for your eyes."

Stage 6: Human Review
You open the PR. You read a full architectural review, inline grumpy comments with fix confirmations, and a back-review summary. You decide whether to merge.

That's the only moment you're in the loop.

Technical Architecture
GitHub Actions as the orchestration layer. Each stage is a workflow triggered by the previous one: pull_request: [opened] → pull_request_review: [submitted] → chained jobs within the same workflow run.

Self-hosted runners for the agentic steps. The address and back-review jobs run on runs-on: self-hosted. The runner is a Docker container on my development machine with Claude Code installed and logged in via claude auth login (subscription-based, not API-keyed). This matters economically: no per-token cost for the agentic steps.

Copilot-powered review workflows via .md prompt files. The two reviewer workflows are defined as Markdown files with YAML front matter in .github/workflows/. The engine is copilot with model configuration. Tools, permissions, and safe outputs (the things the agent is allowed to do, like post comments or submit reviews) are all declared declaratively. The prompt body is the actual instruction set.

Custom Claude Code slash commands. The address and back-review steps call Claude Code with project-specific slash commands (/address-review-ci, /address-respond-ci, /review-back-ci) that are defined in the repo's .claude/commands/ directory. They encapsulate the full instruction context for each task.

Project board automation. GraphQL mutations keep the Kanban board in sync throughout the lifecycle — issue moves from Todo → In Progress → Ready for Review → Done without manual intervention.

What the AI Does Surprisingly Well
Consistent convention enforcement. The architectural reviewer never forgets to check for XML summaries on public members. It never skips the test naming convention check. It never misses the "no commented-out code" rule. Humans do all of these things constantly.

The grumpy persona surfaces real issues. I was sceptical about a personality-driven reviewer. In practice, the "grumpy senior dev" framing produces more pointed, specific inline comments than a neutral reviewer. The constraint of five maximum comments forces prioritisation. It's a genuinely useful second opinion.

Build and test feedback in the review. Because the architectural reviewer actually runs dotnet build and dotnet test as part of its analysis, it catches compilation errors and test failures in the review comment — before a CI failure triggers separately. The review is CI feedback, not just a style opinion.

The address-respond loop closes the conversation. Every review comment gets a reply. Not "LGTM" or silence — an actual explanation of what was changed and why. This is the part that most surprised me: the quality of the argumentation when the agent disagrees with a comment is often stronger than what a developer would bother to write in a code review reply.

Where It Still Needs a Human
Domain and product judgment. No agent knows that "actually, this feature should work differently" because the business logic doesn't match the mental model. That's always a human call.

Architecture decisions that require intent. An agent can flag that a design is unconventional. It cannot know whether the unconventionality is a deliberate trade-off or a mistake without context that only the author has.

Designing the rules themselves. The entire system's quality ceiling is set by the prompt files and convention documents. Writing CLAUDE.md, the coding standards, the reviewer instructions — this is genuinely hard, consequential work. It's software engineering applied to AI behaviour.

The final merge gate. Trust, accountability, and the "this ships to production" decision remain human. That's intentional.

Lessons
Prompt engineering is software architecture. The convention documents and slash command definitions are not documentation — they are the system's behaviour specification. Bad prompts produce bad agents. Modular, well-structured prompts produce agents you can reason about and maintain.

Two reviewers beat one. The architectural reviewer and the grumpy reviewer consistently find different things. One is optimised for coverage and structure; one is optimised for specific, actionable inline critique. The combination is better than either alone.

Persistent memory changes the reviewer's character. The grumpy reviewer writes its findings to a cache between runs. Across multiple reviews on the same PR, it doesn't repeat itself. Over multiple PRs, it notices patterns. This is a small feature that has a disproportionate effect on the quality of the reviews.

Self-hosted runner economics matter. Running Claude Code on a local Docker container via subscription auth means the most computationally intensive steps — implement, address, back-review — have no incremental cost. This changes what's economically viable to automate.

What to Call This
I've been calling it an agentic CI/CD pipeline or Continuous AI workflow. The distinction from ordinary CI is important:

CI/CD automates the verification and delivery of human-written changes.
Agentic CI/CD automates the creation, review, correction, and verification of changes — with humans defining intent and holding final authority, not executing steps.
The human role shifts from implementer to curator. You're no longer in the flow of work; you're above it.

What's Next
The current pipeline handles implementation and review well. The next frontier is more interesting: agents that refactor proactively — not just fix what a reviewer flags, but improve code that no one complained about. Agents that manage technical debt as a first-class concern.

The open question I keep returning to: where should the human boundary permanently sit? Not where it sits today out of habit, and not at "AI does everything" either. The right answer is somewhere specific to context, trust level, and consequence — and finding that line is itself a design problem worth taking seriously.

The full pipeline runs on GitHub Actions with self-hosted runners. The project is a private .NET 10 Bookstore API built as a personal learning and experimentation platform.

A few notes on the naming and framing for your own version:

"Agentic CI/CD" and "Continuous AI" are both reasonable terms. "Continuous AI" has a better ring for a CV or LinkedIn; "Agentic CI/CD" is more technically precise for an article audience.
The grumpy reviewer's actual name in your workflows is "Grumpy Code Reviewer" — feel free to keep that detail as a human touch in the article; it's memorable.
The back-review step is the most novel part of the architecture from a systems design perspective — it's worth emphasising that most AI review tools have no equivalent of a "did the fix actually work?" verification step.