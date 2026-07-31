# Script Contract

Every hook script follows the same basic contract: read JSON from stdin, do work, and respond through exit code, stdout, and stderr.

## What the Script Receives

| Input | Contents |
|-------|----------|
| `stdin` | One JSON payload describing the current event |
| Process environment | Normal env vars plus any defined in `env` config field |
| Working directory | `cwd` from config, or host default |

## How the Script Responds

| Channel | Purpose |
|---------|---------|
| exit `0` | Script succeeded — host continues unless stdout had structured deny |
| non-zero exit | **Blocks the triggering action** and signals hook failure |
| `stdout` | Structured machine-readable output — only for events with documented stdout schema |
| `stderr` | Human-readable diagnostics for logs |

## Critical: `toolArgs` is a JSON String

The `toolArgs` field in event payloads is **always a JSON string**, not a nested object. You must parse it a second time to access its fields.

**Wrong**:
```bash
# This won't work — toolArgs is a string, not an object
command="$(printf '%s' "$payload" | jq -r '.toolArgs.command')"
```

**Correct**:
```bash
# Parse toolArgs first, then extract command
tool_args="$(printf '%s' "$payload" | jq -r '.toolArgs')"
command="$(printf '%s' "$tool_args" | jq -r '.command // ""')"
```

## Reading stdin — Bash

```bash
#!/usr/bin/env bash
set -euo pipefail

payload="$(cat)"
tool_name="$(printf '%s' "$payload" | jq -r '.toolName')"
tool_args="$(printf '%s' "$payload" | jq -r '.toolArgs')"
command="$(printf '%s' "$tool_args" | jq -r '.command // ""')"
cwd="$(printf '%s' "$payload" | jq -r '.cwd')"
```

## Reading stdin — PowerShell

```powershell
Set-StrictMode -Version Latest

$payload = [Console]::In.ReadToEnd() | ConvertFrom-Json
$toolArgs = $payload.toolArgs | ConvertFrom-Json
$command = $toolArgs.command
$cwd = $payload.cwd
```

## Responding: Allow (Default)

Just exit 0:

```bash
exit 0
```

Or explicitly allow (for `preToolUse`):

```bash
jq -cn '{permissionDecision:"allow"}'
exit 0
```

## Responding: Deny with Reason

**Bash**:
```bash
jq -cn --arg reason "Blocked: destructive command" \
  '{permissionDecision:"deny",permissionDecisionReason:$reason}'
exit 0
```

**PowerShell**:
```powershell
@{
  permissionDecision = 'deny'
  permissionDecisionReason = 'Blocked: destructive command'
} | ConvertTo-Json -Compress
exit 0
```

## Responding: Modified Arguments

Replace tool arguments with safer or corrected values:

```bash
jq -cn --arg new_cmd "git push origin main" \
  '{modifiedArgs:$new_cmd}'
exit 0
```

## Responding: Additional Context

Inject context into the agent's turn (works in `preToolUse` and `sessionStart`):

```bash
jq -cn --arg ctx "Current environment: production. Proceed with caution." \
  '{additionalContext:$ctx}'
exit 0
```

## Exit Codes: The Full Picture

| Event Type | Allow | Deny/Block |
|------------|-------|------------|
| `preToolUse` | exit `0` (empty stdout or `{"permissionDecision":"allow"}`) | **Preferred**: exit `0` + `{"permissionDecision":"deny","permissionDecisionReason":"..."}` — gives host a reason. **Also works**: non-zero exit blocks without reason. |
| `userPromptSubmitted` | exit `0` | non-zero exit |
| `agentStop` | exit `0` | non-zero exit |
| Other events | exit `0` | non-zero exit signals failure |

## Strict Mode

Always use strict mode to catch errors early:

**Bash**:
```bash
#!/usr/bin/env bash
set -euo pipefail
```

- `-e`: Exit on error
- `-u`: Exit on undefined variable
- `-o pipefail`: Exit on pipeline failure

**PowerShell**:
```powershell
Set-StrictMode -Version Latest
```

## Accessing Config Variables

Variables defined in the `env` field of your config arrive as **process environment variables**:

**Config**:
```json
{
  "env": {
    "BLOCK_MODE": "deny",
    "MAX_FILES": "20"
  }
}
```

**Script**:
```bash
block_mode="${BLOCK_MODE:-log}"
max_files="${MAX_FILES:-10}"
```

They do **not** appear in the stdin JSON payload.

## Testing Scripts Manually

Test your hook script by piping sample JSON into it:

```bash
echo '{"toolName":"bash","toolArgs":"{\"command\":\"rm -rf dist\"}","cwd":"/tmp"}' | \
  ./.github/hooks/scripts/my-hook.sh
echo "Exit code: $?"
```

Check:
- Exit code (0 = success, non-zero = failure/block)
- stdout content (should be valid JSON if outputting structured data)
- stderr content (should have clear diagnostics)

## Validation Checklist

Before deploying a hook script:

- ✅ Uses strict mode
- ✅ Parses `toolArgs` twice (it's a JSON string)
- ✅ Quotes all shell variables
- ✅ Checks dependencies early (jq, git, npm, etc.)
- ✅ Keeps stdout clean (only structured output)
- ✅ Writes diagnostics to stderr
- ✅ Handles missing/malformed JSON gracefully
- ✅ Avoids prompts, installs, or env mutation
- ✅ Tested with sample payloads
- ✅ Timeout is appropriate for workload
