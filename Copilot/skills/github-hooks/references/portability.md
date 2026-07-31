# Portability Notes

## GitHub Copilot: CLI, VS Code, and Cloud Agent

The same hook system works across all GitHub Copilot environments:

| Environment | Support | Notes |
|-------------|---------|-------|
| **Copilot CLI** | ✅ Full support | Reads `.github/hooks/*.json` from working directory |
| **Copilot in VS Code** | ✅ Full support | Reads `.github/hooks/*.json` from workspace root |
| **Copilot Cloud Agent** | ✅ Full support | Reads `.github/hooks/*.json` from **default branch only** |

### Key Compatibility Points

1. **Same config format**: `.github/hooks/*.json` with same schema
2. **Same payload shapes**: Event JSON fields are identical
3. **Same script contract**: stdin → process → stdout/stderr/exit code
4. **Same event names**: Accept both camelCase (`preToolUse`) and PascalCase (`PreToolUse`)
5. **Same field name**: Tool arguments field is `toolArgs` (a JSON string)

### Cloud Agent Differences

The cloud agent has one important difference:

**Hooks are loaded from the default branch only**. If your `.github/hooks/*.json` file is only on a feature branch, the cloud agent won't see it.

**Workaround**: Merge your hooks config to the default branch (main/master), or test locally using CLI/VS Code first.

## Claude Code

Claude Code uses a **different hook system** with its own conventions:

| Aspect | GitHub Copilot | Claude Code |
|--------|----------------|-------------|
| **Config location** | `.github/hooks/*.json` | `~/.claude/settings.json` or `.claude/settings.json` |
| **Event names** | `preToolUse`, `postToolUse`, etc. | `FileChanged`, `CwdChanged`, `ToolUse`, etc. (29+ events) |
| **Exit code 2** | Not special | Exit 2 = block (exit 1 = non-blocking error) |
| **Matchers** | `"matcher": "bash"` | Regex and `if` conditions |
| **Hook types** | `command` only (in repo hooks) | `command`, `http`, `mcp_tool`, `prompt`, `agent` |
| **Tool args field** | `toolArgs` (JSON string) | `input` (varies by event) |

Claude Code hooks are **not portable** to GitHub Copilot. The config formats and event schemas are incompatible.

### When to Use Which System

| Use Case | GitHub Copilot Hooks | Claude Code Hooks |
|----------|----------------------|-------------------|
| Portable across GitHub Copilot environments | ✅ Yes | ❌ No |
| Repository-scoped hooks | ✅ Yes | Limited |
| User-global hooks | ❌ No | ✅ Yes |
| Rich event types (file changes, cwd changes) | ❌ No | ✅ Yes |
| HTTP webhooks | ❌ No | ✅ Yes |
| MCP tool wrapping | ❌ No | ✅ Yes |

## Platform Compatibility: Bash vs PowerShell

Hooks can provide `bash`, `powershell`, or both entry points.

### When to Provide Both

| Scenario | Provide |
|----------|---------|
| Private repo, one known platform | Only that platform's entry |
| Published hook, claiming cross-platform support | **Both** entries |
| Single cross-platform runtime (Python, Node, pwsh) | Expose same script through both |
| Bash-only dependency (grep, sed, awk) | `bash` only |
| Windows-only dependency | `powershell` only |

### Cross-Platform Script Example

Use Python through both entry points:

**Config**:
```json
{
  "type": "command",
  "bash": "python3 ./.github/hooks/scripts/validate.py",
  "powershell": "python .\\.github\\hooks\\scripts\\validate.py"
}
```

**Script** (`validate.py`):
```python
#!/usr/bin/env python3
import json
import sys

payload = json.load(sys.stdin)
tool_name = payload.get('toolName')

if tool_name != 'bash':
    sys.exit(0)

# Validation logic here
```

### Path Separator Differences

| Platform | Path separator | Example |
|----------|----------------|---------|
| Bash | `/` | `./.github/hooks/scripts/hook.sh` |
| PowerShell | `\` or `/` | `.\\.github\\hooks\\scripts\\hook.ps1` (escaped) or `./.github/hooks/scripts/hook.ps1` |

PowerShell accepts both. Use forward slashes in config to maximize compatibility:

```json
{
  "bash": "./.github/hooks/scripts/hook.sh",
  "powershell": "./.github/hooks/scripts/hook.ps1"
}
```

## Dependency Portability

| Tool | Bash | PowerShell | Notes |
|------|------|------------|-------|
| `jq` | ✅ Usually available | ❌ Not built-in | Install separately on Windows or use PowerShell's `ConvertFrom-Json` |
| `grep`, `sed`, `awk` | ✅ Built-in | ❌ Not built-in | Use WSL, Git Bash, or PowerShell equivalents |
| `git` | ✅ Usually available | ✅ Usually available | Widely installed |
| `python3` | ✅ Common | ✅ Available as `python` | Check with `command -v python3` (Bash) or `Get-Command python` (PowerShell) |
| `node` | ✅ Common | ✅ Common | Check with `command -v node` or `Get-Command node` |

### Checking Dependencies

**Bash**:
```bash
if ! command -v jq >/dev/null 2>&1; then
  echo "Error: jq is required" >&2
  exit 1
fi
```

**PowerShell**:
```powershell
if (-not (Get-Command jq -ErrorAction SilentlyContinue)) {
  Write-Error "jq is required"
  exit 1
}
```

## JSON Parsing: jq vs ConvertFrom-Json

**Bash** (requires jq):
```bash
tool_name="$(printf '%s' "$payload" | jq -r '.toolName')"
```

**PowerShell** (built-in):
```powershell
$payload = [Console]::In.ReadToEnd() | ConvertFrom-Json
$toolName = $payload.toolName
```

## Testing Across Platforms

Test your hook on all target platforms:

**Bash (Unix/macOS/WSL)**:
```bash
echo '{"toolName":"bash","toolArgs":"{}"}' | ./.github/hooks/scripts/hook.sh
```

**PowerShell (Windows)**:
```powershell
'{"toolName":"bash","toolArgs":"{}"}' | .\.github\hooks\scripts\hook.ps1
```

## Matchers (Feature Status)

The `matcher` field filters hooks by tool name at the host level:

```json
{
  "matcher": "bash",
  "type": "command",
  "bash": "./.github/hooks/scripts/hook.sh"
}
```

**Status**: Locally verified working in Copilot CLI v1.0.36. Not yet documented in official reference. May not work in all environments.

**Fallback**: Always include in-script filtering as backup:

```bash
tool_name="$(printf '%s' "$payload" | jq -r '.toolName')"
[[ "$tool_name" != "bash" ]] && exit 0
```

This ensures compatibility even if matcher support is incomplete.

## Summary

| Feature | GitHub Copilot | Claude Code |
|---------|----------------|-------------|
| Config location | `.github/hooks/*.json` | `~/.claude/settings.json` |
| Portable across Copilot environments | ✅ Yes | ❌ No |
| Cross-platform scripts | Provide both `bash` and `powershell` | Varies |
| Event schemas | Same across CLI/VS Code/Cloud | Different system |
| Default branch requirement | Cloud agent only | N/A |

For maximum portability within GitHub Copilot:
1. ✅ Use forward slashes in paths
2. ✅ Provide both `bash` and `powershell` if claiming cross-platform support
3. ✅ Check dependencies early with clear error messages
4. ✅ Use cross-platform runtimes (Python, Node, pwsh) when possible
5. ✅ Test on all target platforms
6. ✅ Merge hooks config to default branch for cloud agent support
