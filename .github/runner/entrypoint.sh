#!/bin/bash
set -euo pipefail

# Required environment variables
: "${GITHUB_REPOSITORY:?GITHUB_REPOSITORY is required (owner/repo)}"
: "${GITHUB_PAT:?GITHUB_PAT is required (PAT with repo scope)}"

RUNNER_NAME="${RUNNER_NAME:-bookstore-runner}"
RUNNER_LABELS="${RUNNER_LABELS:-self-hosted,linux,docker}"
RUNNER_WORKDIR="${RUNNER_WORKDIR:-_work}"

# Acquire a short-lived registration token from the GitHub API
echo "Requesting registration token..."
REG_TOKEN=$(curl -fsSL \
  -X POST \
  -H "Authorization: token ${GITHUB_PAT}" \
  -H "Accept: application/vnd.github.v3+json" \
  "https://api.github.com/repos/${GITHUB_REPOSITORY}/actions/runners/registration-token" \
  | jq -r '.token')

if [ -z "$REG_TOKEN" ] || [ "$REG_TOKEN" = "null" ]; then
  echo "ERROR: Failed to obtain registration token." >&2
  exit 1
fi

# Remove stale config from a previous run (container restart)
if [ -f ".runner" ]; then
  echo "Removing stale runner config..."
  ./config.sh remove --token "$REG_TOKEN" || true
fi

# Register the runner
echo "Registering runner '${RUNNER_NAME}'..."
./config.sh \
  --url "https://github.com/${GITHUB_REPOSITORY}" \
  --token "$REG_TOKEN" \
  --name "$RUNNER_NAME" \
  --labels "$RUNNER_LABELS" \
  --work "$RUNNER_WORKDIR" \
  --replace \
  --unattended \
  --disableupdate \
  --ephemeral

# Deregister on shutdown so the runner slot is freed
cleanup() {
  echo "Caught signal — removing runner..."
  ./config.sh remove --token "$REG_TOKEN" || true
  exit 0
}
trap cleanup SIGTERM SIGINT

# Start the runner in the background so trap can fire
./run.sh &
wait $!
