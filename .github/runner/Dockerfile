# Self-hosted GitHub Actions runner with Claude Code, .NET 10, and gh CLI
# Build: docker compose build
# Run:   docker compose up -d

FROM node:20-bookworm

ARG RUNNER_VERSION=2.332.0
ARG CLAUDE_CODE_VERSION=latest
ARG DOTNET_VERSION=10.0

# System packages
RUN apt-get update && apt-get install -y --no-install-recommends \
    git \
    curl \
    jq \
    sudo \
    ca-certificates \
    gnupg2 \
    libicu-dev \
    && rm -rf /var/lib/apt/lists/*

# gh CLI (official apt repository)
RUN curl -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg \
      | dd of=/usr/share/keyrings/githubcli-archive-keyring.gpg \
    && chmod go+r /usr/share/keyrings/githubcli-archive-keyring.gpg \
    && echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main" \
      | tee /etc/apt/sources.list.d/github-cli.list > /dev/null \
    && apt-get update && apt-get install -y --no-install-recommends gh \
    && rm -rf /var/lib/apt/lists/*

# .NET 10 SDK (needed for dotnet build/test in CI)
RUN curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh \
    && chmod +x /tmp/dotnet-install.sh \
    && /tmp/dotnet-install.sh --channel $DOTNET_VERSION --install-dir /usr/share/dotnet \
    && ln -s /usr/share/dotnet/dotnet /usr/local/bin/dotnet \
    && rm /tmp/dotnet-install.sh

ENV DOTNET_ROOT=/usr/share/dotnet
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Claude Code CLI
RUN npm install -g @anthropic-ai/claude-code@${CLAUDE_CODE_VERSION}

# Non-root runner user with limited sudo
RUN useradd -m -s /bin/bash runner \
    && echo "runner ALL=(ALL) NOPASSWD: /usr/bin/apt-get" >> /etc/sudoers.d/runner

# GitHub Actions runner agent
WORKDIR /home/runner/actions-runner
RUN curl -fsSL "https://github.com/actions/runner/releases/download/v${RUNNER_VERSION}/actions-runner-linux-x64-${RUNNER_VERSION}.tar.gz" \
      -o runner.tar.gz \
    && tar xzf runner.tar.gz \
    && rm runner.tar.gz \
    && ./bin/installdependencies.sh \
    && chown -R runner:runner /home/runner/actions-runner

COPY entrypoint.sh /home/runner/entrypoint.sh
RUN sed -i 's/\r$//' /home/runner/entrypoint.sh && chmod +x /home/runner/entrypoint.sh

USER runner
WORKDIR /home/runner/actions-runner

ENTRYPOINT ["/home/runner/entrypoint.sh"]
