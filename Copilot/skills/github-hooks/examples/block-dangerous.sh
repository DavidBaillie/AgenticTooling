#!/usr/bin/env bash
set -euo pipefail

# Block Dangerous Commands Hook
# Prevents destructive shell commands before execution.
# Set BLOCK_MODE=deny in config to block, or BLOCK_MODE=log to only log.

payload="$(cat)"
block_mode="${BLOCK_MODE:-log}"
tool_name="$(printf '%s' "$payload" | jq -r '.toolName')"

[[ "$tool_name" != "bash" ]] && exit 0

command="$(printf '%s' "$payload" | jq -r '.toolArgs' | jq -r '.command // ""')"

if printf '%s' "$command" | grep -qE 'rm -rf /|git reset --hard|git clean -fd|git push.*--force'; then
  # Truncate command to avoid leaking secrets in deny reason or logs
  short_cmd="$(printf '%.80s' "$command")"
  if [[ "$block_mode" == "deny" ]]; then
    jq -cn --arg reason "Destructive command blocked: ${short_cmd}..." \
      '{permissionDecision:"deny",permissionDecisionReason:$reason}'
  else
    echo "Would block: ${short_cmd}..." >&2
  fi
fi
exit 0
