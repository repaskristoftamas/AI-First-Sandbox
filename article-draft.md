# Building an Agentic CI/CD Pipeline: How I Automated My Entire SDLC with AI Agents

Last month, I merged a pull request that I never wrote a single line of code for. I didn't review the initial implementation either — an AI agent did that. I didn't address the review feedback — another agent handled that. I didn't even verify that the fixes were correct — a third agent validated them. My only job was to read the final PR, confirm the result, and click merge.

This isn't a thought experiment. This is my actual workflow, running in production on a .NET Clean Architecture project. And building it taught me more about the future of software engineering than any conference talk ever could.

## The Problem: Cognitive Bottlenecks in the SDLC

CI/CD solved the mechanical bottleneck years ago. We don't manually compile, package, or deploy anymore. But the *cognitive* bottleneck — the part where a human reads a ticket, understands the codebase, writes the code, reviews someone else's code, addresses feedback, and iterates — that part hasn't changed.

For a solo developer or a small team, this is especially painful. You're the implementer, the reviewer, and the feedback-addresser. Context switching between these roles kills velocity. And let's be honest: reviewing your own code is about as effective as proofreading your own writing.

I wanted to build a system where AI agents handle the entire cycle from well-scoped issue to merge-ready PR, and the human only makes the final judgment call.

## The Architecture

Here's the full pipeline, stage by stage:

### Stage 0: Ideation and Issue Drafting

This is where the human starts — and it's the most important stage. I describe a feature, a bug fix, or a refactor in natural language. Claude Code (running locally in VS Code) drafts well-structured GitHub issues with clear acceptance criteria and adds them to the project board as "Todo."

The quality of the issue description directly determines the quality of everything downstream. This is the new bottleneck — and it's a good one, because it forces you to *think* before you build.

### Stage 1: AI Implementation

I invoke `/work-issue <number>` in Claude Code. This triggers a custom skill that:

1. Reads the issue from GitHub
2. Explores the codebase to understand the current state
3. Moves the issue to "In Progress" on the project board
4. Creates a feature branch following naming conventions
5. Implements the changes following the project's architecture and coding standards
6. Builds the entire solution and fixes any compilation errors
7. Writes or updates tests as needed
8. Runs the full test suite and fixes failures
9. Commits with a conventional commit message referencing the issue
10. Pushes the branch and creates a PR with a structured description

All of this happens autonomously. The agent has access to the full codebase, the project's coding standards (encoded in `CLAUDE.md` and `.claude/rules/`), and the test suite. It doesn't guess — it reads, plans, implements, validates, and ships.

### Stage 2: AI Code Review

When the PR opens, two things happen automatically via GitHub Actions:

First, a **Grumpy Code Reviewer** (powered by GitHub's Agentic Workflows / `gh-aw`) performs a critical code review. This isn't a gentle "looks good to me" bot — it's specifically designed to find edge cases, potential bugs, and code quality issues. It submits a full review with inline comments.

### Stage 3: AI Feedback Addresser

This is where it gets interesting. The grumpy reviewer's submission triggers a **self-hosted GitHub Actions runner** — a Docker container running on my local machine with Claude Code pre-installed and authenticated.

The addresser agent:
1. Reads every review comment
2. Classifies each one as `fix` (valid concern, make a code change), `discuss` (debatable point, reply with reasoning), or `skip` (factually wrong or contradicts project conventions)
3. Makes the code changes for `fix` items
4. Builds and runs the full test suite
5. Commits and pushes the fixes
6. Posts a reply to every single review comment explaining what it did and why

This is critical: the agent doesn't blindly apply every suggestion. It evaluates each comment against the project's actual coding standards, architecture rules, and testing conventions. If the reviewer asks for something that contradicts an established pattern, the agent pushes back — with a citation.

### Stage 4: AI Back-Review

After the addresser finishes, a **back-reviewer** agent runs automatically. Its job is adversarial quality assurance:

- For each **fixed** comment, it reads the actual commit diff and evaluates whether the fix is adequate, incomplete, or incorrect.
- For each **discussed** or **skipped** comment, it reads both sides of the argument and the relevant source code to determine who's right.
- If a fix is incomplete or incorrect, it posts a `/address` command as a new comment, which triggers another round of addressing — creating a self-correcting loop.
- It tags the PR author (me) on every reply, so I can see the full AI-to-AI conversation when I arrive.

There's a built-in loop prevention mechanism: if the same concern has been addressed twice and is still unresolved, the back-reviewer escalates to the human instead of triggering another cycle.

### Stage 5: Human Final Review

Only now do I enter the picture. I arrive to a PR that has been:
- Implemented against a well-scoped issue
- Reviewed by an opinionated AI reviewer
- Iterated on by an addresser that fixed valid concerns and argued back on non-issues
- Validated by a back-reviewer that verified the fixes

My job is to read the conversation, check the final state, and make the judgment call. More often than not, the PR is ready to merge.

## The Infrastructure

### Custom Claude Code Skills

The pipeline is driven by custom Claude Code skills — markdown files in `.claude/commands/` that define structured workflows:

- **`/work-issue`**: The full implementation pipeline from issue to PR
- **`/address-review-ci`**: Autonomous review feedback addressing (the CI variant that runs without user confirmation)
- **`/address-respond-ci`**: Posts reply comments after the addresser has pushed code
- **`/review-back-ci`**: The back-reviewer that validates addresser fixes

Each skill is a detailed, step-by-step protocol that the AI follows. Think of them as runbooks — but the runner is an LLM, not a human.

### Machine-Readable Coding Standards

The single most important enabler of this pipeline is `CLAUDE.md` and the `.claude/rules/` directory. These files encode the project's architecture (Clean Architecture, CQRS, DDD), coding standards (sealed classes, file-scoped namespaces, primary constructors), testing conventions (xUnit, Shouldly, AAA pattern), and even anti-patterns to avoid.

When an AI agent implements code or reviews a PR, it reads these files as its system prompt. This means the agent isn't following generic best practices — it's following *your project's* conventions. The result is remarkably consistent code that looks like a human on the team wrote it.

### The Self-Hosted Runner

The addresser and back-reviewer run on a self-hosted GitHub Actions runner inside a Docker container. The Dockerfile installs:
- GitHub Actions runner agent
- Claude Code CLI (via npm)
- .NET SDK (for building and testing)
- gh CLI (for GitHub API operations)

Claude Code authenticates via a mounted subscription credential — no API keys in CI secrets. The container is defined in a `docker-compose.yml` for easy spin-up and tear-down.

Why self-hosted? Because the addresser needs Claude Code's full skill system (custom commands, project rules, persistent memory), which isn't available through a simple API call. The runner is Claude Code running in headless mode with `--permission-mode bypassPermissions`.

### Project Board Automation

GitHub Actions workflows keep the project board in sync:
- Issue moves to "In Progress" when the agent starts working
- Issue moves to "Ready for Review" when the PR opens
- Daily and weekly status reports are generated automatically

## Key Design Decisions

### Why the human only enters at final review

This is a trust boundary decision. The AI agents operate within well-defined constraints: they can't merge PRs, they can't modify unrelated files, and they escalate ambiguity. The human's job isn't to check every line — it's to verify that the system produced the right outcome.

### Why AI agents argue with each other

Single-agent review misses things for the same reason self-review misses things: confirmation bias. The grumpy reviewer and the addresser have different objectives (find problems vs. solve problems), which creates a natural adversarial dynamic. The back-reviewer acts as an arbiter. This three-agent architecture catches more issues than any single agent could.

### Why coding standards must be machine-readable

If your coding standards live in a Confluence page that developers sometimes read, AI agents will produce inconsistent code. If your standards are in a `CLAUDE.md` file that gets loaded into every agent's context, consistency becomes automatic. This is perhaps the most transferable insight from this project: **invest in machine-readable engineering standards**.

### Event-driven chaining over monolithic workflows

Each stage is a separate workflow triggered by GitHub events (PR opened, review submitted, comment created). This makes the system modular, debuggable, and resilient. If the addresser fails, you can re-trigger it without re-running the review. If you want to add a new stage, you add a new workflow file.

## What I Learned

**The bottleneck shifted.** I used to spend most of my time writing and reviewing code. Now I spend most of my time writing precise issue descriptions and making architectural decisions. The actual typing — the part we traditionally call "coding" — is handled by agents.

**AI follows conventions better than it invents them.** AI agents are excellent at implementing well-defined patterns consistently. They're less good at deciding *which* pattern to use in a novel situation. Encode your decisions; let AI execute them.

**Adversarial multi-agent review works.** The grumpy reviewer finds real issues. The addresser fixes most of them correctly. The back-reviewer catches the cases where the addresser got it wrong. The system is genuinely better than single-pass review.

**You need loop prevention.** Without explicit guards, the back-reviewer can trigger infinite address cycles when agents disagree on a subjective point. The two-attempt limit with human escalation is essential.

**The role of the developer changes.** I'm no longer primarily a code writer. I'm a system designer, a quality arbiter, and a decision-maker. The code is a byproduct of well-articulated intent. This is a fundamental shift in what it means to be a software engineer.

## Limitations

This isn't a silver bullet. Here's where the system breaks down:

- **Ambiguous requirements**: If the issue description is vague, the implementation agent produces vague code. Garbage in, garbage out — but faster.
- **Novel architecture**: When the task requires a pattern the codebase hasn't seen before, the agent falls back to generic approaches. These are the cases where I step in earlier.
- **Cost**: Running Claude Opus on every PR review and address cycle isn't free. For a personal project this is fine; for a team at scale, you'd need to be strategic about which PRs get the full pipeline.
- **Debugging the pipeline**: When an agent makes a mistake three layers deep, tracing the root cause through AI-generated PR comments requires a new kind of debugging skill.

## What's Next

This pipeline runs on a single repository today. The natural evolution is:

- **Metrics**: Tracking cycle time, fix quality rate, and human override frequency to quantify the pipeline's value
- **Multi-repo**: Generalizing the skills and runner setup so any repository can opt in
- **Team workflows**: Adapting the trust boundary for teams where multiple humans need to approve

## Closing Thoughts

The best code I wrote this year wasn't application logic — it was the pipeline that writes the application logic. The markdown files defining agent skills. The Dockerfile for the self-hosted runner. The workflow files that chain everything together.

We're entering an era where "Continuous Integration" and "Continuous Deployment" are joined by something new: **Continuous AI** — AI agents embedded directly into the development lifecycle, not as tools you invoke, but as autonomous participants that implement, review, argue, and iterate.

The developer's job isn't disappearing. It's evolving. From writing code to designing systems that write code. From reviewing diffs to adjudicating AI debates. From addressing feedback to building the agents that address feedback.

The question isn't whether this is the future. It's whether you're building the pipeline or waiting for someone else to build it for you.

---

*This article describes an agentic CI/CD pipeline built with Claude Code, GitHub Actions, GitHub Agentic Workflows, and a Dockerized self-hosted runner. The project is a .NET Clean Architecture API — but the patterns are stack-agnostic.*
