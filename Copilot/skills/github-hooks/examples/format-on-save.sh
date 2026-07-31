#!/usr/bin/env bash
set -euo pipefail

# Format-on-Save Hook
# Automatically formats files after the agent creates or edits them.
# The agent's next read sees the formatted version.

payload="$(cat)"
tool_name="$(printf '%s' "$payload" | jq -r '.toolName')"
result_type="$(printf '%s' "$payload" | jq -r '.toolResult.resultType // ""')"

# Only format after successful file writes
case "$tool_name" in
  edit|create) ;;
  *) exit 0 ;;
esac
[[ "$result_type" != "success" ]] && exit 0

file_path="$(printf '%s' "$payload" | jq -r '.toolArgs' | jq -r '.path // ""')"
[[ -z "$file_path" || ! -f "$file_path" ]] && exit 0

# Run the project's formatter — adapt to your stack
if command -v npx >/dev/null 2>&1 && [[ -f "package.json" ]]; then
  npx prettier --write "$file_path" 2>/dev/null || true
elif command -v dotnet >/dev/null 2>&1 && [[ "$file_path" == *.cs ]]; then
  dotnet format --include "$file_path" 2>/dev/null || true
fi
exit 0
