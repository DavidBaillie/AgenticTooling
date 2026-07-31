---
description: 'Author safe, fast, and portable GitHub hooks for code quality gates, auto-formatting, and guardrails'
name: github-hooks
---

# GitHub Copilot Hooks

Hooks are **small, deterministic scripts** that run at specific lifecycle events in GitHub Copilot sessions. A great hook does one clear job, runs quickly, and makes side effects explicit.

## When to Use Hooks

Hooks excel at:
- **Pre-flight validation**: Block commits until lint/tests pass
- **Auto-formatting**: Format files immediately after edits
- **Guardrails**: Prevent dangerous commands before execution
- **Context injection**: Add environment-specific context to sessions
- **Audit logging**: Track tool usage and decisions

## When NOT to Use Hooks

Avoid hooks for:
- Open-ended reasoning or style guidance (use instructions/prompts)
- Long multi-step workflows with memory or retries (use agents)
- Background daemons or async jobs (use dedicated automation)
- Heavy repository-wide scans (use CI)

## Quick Start

### 1. Folder Structure

```text
.github/
└── hooks/
    ├── my-hook.json           ← config (events, scripts, options)
    └── scripts/
        ├── my-hook.sh         ← Bash implementation
        └── my-hook.ps1        ← PowerShell (optional)
```

### 2. Config File

```json
{
  "version": 1,
  "hooks": {
    "preToolUse": [
      {
        "matcher": "bash",
        "type": "command",
        "bash": "./.github/hooks/scripts/my-hook.sh",
        "powershell": "./.github/hooks/scripts/my-hook.ps1",
        "cwd": ".",
        "timeoutSec": 5,
        "env": {
          "BLOCK_MODE": "deny"
        }
      }
    ]
  }
}
```

### 3. Script Contract

Every hook script:
- Reads JSON payload from `stdin`
- Responds via exit code and `stdout`
- Uses `stderr` for diagnostics

**Important**: `toolArgs` is a **JSON string** — parse it twice to access fields.

## Key Config Fields

| Field | Purpose |
|-------|---------|
| `matcher` | Filter by tool name (e.g., `"bash"`, `"edit"`) — hook only fires when matched |
| `type` | Always `"command"` for scripts |
| `bash` / `powershell` | Platform-specific script paths (provide one or both) |
| `env` | Static config variables passed to script as environment variables |
| `timeoutSec` | Max execution time (default 30) |

## Exit Codes and Deny Mechanism

| Event Type | Allow | Deny/Block |
|------------|-------|------------|
| `preToolUse` | exit `0` + empty stdout OR `{"permissionDecision":"allow"}` | **Preferred**: exit `0` + `{"permissionDecision":"deny","permissionDecisionReason":"..."}` on stdout. **Also works**: non-zero exit |
| `userPromptSubmitted` | exit `0` | non-zero exit |
| Other events | exit `0` | non-zero exit signals failure |

## Common Event Types

| Event | When It Fires | stdout Parsed? | Typical Use |
|-------|---------------|----------------|-------------|
| `sessionStart` | Session begins | Yes: `additionalContext` | Setup, context injection |
| `preToolUse` | Before tool execution | Yes: `permissionDecision`, `modifiedArgs` | Guardrails, validation, argument modification |
| `postToolUse` | After tool execution | No | Logging, auto-formatting |
| `userPromptSubmitted` | User sends prompt | No | Auditing, prompt blocking |
| `sessionEnd` | Session ends | No | Cleanup, summaries |
| `errorOccurred` | Error happens | No | Diagnostics, alerts |

## Universal Design Rules

1. **One hook, one responsibility** — small hooks are easier to trust
2. **Default to observe first** — blocking should be explicit
3. **Keep synchronous and bounded** — hooks run in critical path
4. **Make deterministic and idempotent** — re-runs should not drift
5. **Never mutate Git state by default** — high-risk operations need opt-in
6. **Treat all input as untrusted** — validate, sanitize, redact
7. **Redact secrets from logs** — logs outlive hook runs

## Script Authoring Checklist

- Use strict mode: `set -euo pipefail` (Bash) or `Set-StrictMode -Version Latest` (PowerShell)
- Quote shell variables: `"$var"` not `$var`
- Parse `toolArgs` twice (it's a JSON string)
- Keep stdout clean unless outputting structured data
- Check dependencies early with clear error messages
- Test by piping sample JSON into script manually
- Avoid prompts, installs, or environment mutation

## Reading stdin — Quick Reference

**Bash:**
```bash
#!/usr/bin/env bash
set -euo pipefail
payload="$(cat)"
tool_name="$(printf '%s' "$payload" | jq -r '.toolName')"
tool_args="$(printf '%s' "$payload" | jq -r '.toolArgs')"
command="$(printf '%s' "$tool_args" | jq -r '.command // ""')"
```

**PowerShell:**
```powershell
Set-StrictMode -Version Latest
$payload = [Console]::In.ReadToEnd() | ConvertFrom-Json
$toolArgs = $payload.toolArgs | ConvertFrom-Json
$command = $toolArgs.command
```

## Deny with Reason — Quick Reference

**Bash:**
```bash
jq -cn --arg reason "Blocked: reason here" \
  '{permissionDecision:"deny",permissionDecisionReason:$reason}'
exit 0
```

**PowerShell:**
```powershell
@{ permissionDecision = 'deny'; permissionDecisionReason = 'Blocked: reason here' } |
    ConvertTo-Json -Compress
exit 0
```

## Examples

See the `examples/` folder for complete, working hooks:
- **commit-gate**: Block commits until lint, types, and tests pass
- **format-on-save**: Auto-format files after edits
- **block-dangerous**: Prevent destructive shell commands

## References

See the `references/` folder for:
- **event-schemas.md**: Full payload shapes for all events
- **script-contract.md**: Detailed stdin/stdout contract
- **anti-patterns.md**: Common mistakes to avoid
- **portability.md**: Cross-platform and cross-tool notes

## Official Documentation

- [Hooks configuration reference](https://docs.github.com/en/copilot/reference/hooks-configuration)
- [About hooks](https://docs.github.com/en/copilot/concepts/agents/cloud-agent/about-hooks)
