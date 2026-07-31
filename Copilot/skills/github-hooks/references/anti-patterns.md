# Anti-Patterns

Common mistakes when writing hooks and how to avoid them.

## Long-Running Hooks

**Problem**: Hooks that run for minutes or start background daemons.

**Why it's bad**: Hooks run in the critical path of every tool call or session event. Long-running hooks create user-visible latency and may time out.

**Instead**:
- Keep hooks synchronous and bounded (< 30 seconds)
- Use dedicated automation, CI, or services for long-running work
- Use `timeoutSec` to enforce time limits

## Heavy Scans on Every Event

**Problem**: Scanning entire repository or running expensive analysis on every `preToolUse` or `postToolUse`.

**Why it's bad**: Creates performance bottleneck; user experiences lag.

**Instead**:
- Use `matcher` field to filter by tool name
- Filter early in script (check `toolName` and exit if not relevant)
- Scope validation to changed files only
- Move expensive checks to CI or scheduled jobs

## Hidden Network Calls

**Problem**: Making HTTP requests, uploading logs, or calling external APIs without user knowledge.

**Why it's bad**: Violates user trust; creates security and privacy risks; adds latency; may fail silently.

**Instead**:
- Document any network activity clearly
- Make network calls opt-in via config
- Use async background jobs for telemetry
- Keep hooks local and deterministic by default

## Silent Git Mutation

**Problem**: Automatically running `git checkout`, `git reset --hard`, `git clean -fd`, `git stash`, `git commit`, or `git push` without explicit user consent.

**Why it's bad**: Can destroy work, change history, or push unintended code. Users don't expect hooks to mutate repository state.

**Instead**:
- Default to read-only or observation mode
- Make destructive operations opt-in via config
- Block destructive commands unless explicitly allowed
- Log what would happen instead of doing it

## Interactive Prompts

**Problem**: Using `read`, `Get-Confirmation`, or other interactive input during hook execution.

**Why it's bad**: Hooks run non-interactively; prompts block indefinitely and time out.

**Instead**:
- Use config variables (`env` field) for all settings
- Fail early with clear error if required config is missing
- Use structured deny reasons to communicate with agent

## Noisy stdout

**Problem**: Writing logs, debug output, progress messages, or mixed content to stdout.

**Why it's bad**: Host parses stdout as structured JSON. Non-JSON output causes parsing failures.

**Instead**:
- Use `stderr` for all human-readable diagnostics
- Use `stdout` only for structured JSON responses
- Keep stdout empty unless event requires structured output

## Mixed Machine/Human Output

**Problem**: Writing both JSON and plain text to stdout.

**Why it's bad**: Host expects either all-JSON or empty stdout. Mixed output breaks parsing.

**Instead**:
- One format per channel: JSON on stdout, text on stderr
- Use `jq -nc` to ensure clean JSON-only output

## Logging Raw Secrets

**Problem**: Logging full command text, prompts, tool arguments, or environment variables without redaction.

**Why it's bad**: Logs may contain API keys, tokens, passwords, or private data. Logs often outlive the hook run and may be shared.

**Instead**:
- Truncate command strings before logging
- Redact patterns like `token=`, `password=`, `key=`
- Never log full environment or tool arguments
- Use structured deny reasons that don't leak secrets

## Monolithic Hooks

**Problem**: One hook that validates linting, runs tests, formats code, audits security, and uploads logs.

**Why it's bad**: Hard to debug, test, disable, or understand. Timeout applies to entire monolith.

**Instead**:
- One hook, one responsibility
- Separate configs for separate concerns
- Each hook has clear purpose and can be disabled independently

## Implicit Installs

**Problem**: Running `npm install`, `pip install`, or package manager commands during hook execution.

**Why it's bad**: Mutates environment, adds latency, may fail due to network or permissions, introduces non-determinism.

**Instead**:
- Check for dependencies early and fail with clear message
- Document required dependencies in README
- Expect dependencies to be pre-installed
- Use CI or setup scripts for installation

## Building Commands from Raw Input

**Problem**: Directly interpolating `toolArgs` or `prompt` into shell commands.

```bash
# NEVER do this
eval "$command"
bash -c "$user_prompt"
```

**Why it's bad**: Command injection vulnerability. User or agent input may contain shell metacharacters.

**Instead**:
- Treat all input as untrusted
- Validate input against allowlist patterns
- Use structured argument passing (arrays, not strings)
- Quote all variables: `"$var"` not `$var`

## Assuming jq is Available

**Problem**: Running `jq` commands without checking if jq is installed.

**Why it's bad**: Hook fails silently or with cryptic error.

**Instead**:
```bash
if ! command -v jq >/dev/null 2>&1; then
  echo "Error: jq is required but not installed" >&2
  exit 1
fi
```

## Ignoring Exit Codes

**Problem**: Running commands without checking if they succeeded.

```bash
# Bad — continues even if command fails
npm test
do_something_else
```

**Why it's bad**: Errors are hidden; hook may report success when work failed.

**Instead**:
```bash
set -e  # Exit on error
npm test || {
  echo "Tests failed" >&2
  exit 1
}
```

## Non-Idempotent Hooks

**Problem**: Hook behavior changes based on previous runs or external state.

**Why it's bad**: Re-running the same event produces different results; hard to debug.

**Instead**:
- Make hooks deterministic (same input = same output)
- Avoid state files or counters
- Use event payload as single source of truth

## Platform-Specific Dependencies

**Problem**: Hook requires Windows-only or Unix-only tools without fallback.

**Why it's bad**: Fails silently on other platforms; not portable.

**Instead**:
- Provide both `bash` and `powershell` entries if claiming cross-platform support
- Document platform requirements clearly
- Check for required tools early and fail with clear message
- Use cross-platform tools (Python, Node, pwsh) when possible

## Timeout Too Short or Too Long

**Problem**: `timeoutSec: 1` for expensive validation, or `timeoutSec: 300` for simple check.

**Why it's bad**: Short timeout causes false failures; long timeout delays feedback.

**Instead**:
- Match timeout to actual workload
- Simple checks: 5-15 seconds
- Lint/tests: 30-120 seconds
- If work takes > 120 seconds, move it to CI

## No Early Exit

**Problem**: Hook does expensive work even when event is not relevant.

```bash
# Bad — runs for every tool
payload="$(cat)"
run_expensive_validation
```

**Why it's bad**: Adds latency to unrelated tool calls.

**Instead**:
```bash
# Filter early
tool_name="$(printf '%s' "$payload" | jq -r '.toolName')"
[[ "$tool_name" != "bash" ]] && exit 0

# Or use matcher in config
```

## Summary

| Anti-Pattern | Solution |
|--------------|----------|
| Long-running work | Keep < 30s, use CI for heavy work |
| Heavy scans | Filter early, scope to changed files |
| Hidden network calls | Document, make opt-in, keep local by default |
| Silent Git mutation | Read-only by default, destructive ops opt-in |
| Interactive prompts | Use config vars, fail early if missing |
| Noisy stdout | JSON on stdout, text on stderr |
| Logging secrets | Truncate, redact, never log raw input |
| Monolithic hooks | One hook, one responsibility |
| Implicit installs | Check deps early, document requirements |
| Command injection | Quote vars, validate input, no `eval` |
| Missing dependencies | Check early, fail with clear message |
| Non-idempotent | Same input = same output, no state files |
| Wrong timeout | Match to workload (5-120s typical) |
| No early exit | Filter by toolName early, use matcher |
