# Event Payload Schemas

These are the payload shapes from the [official hooks reference](https://docs.github.com/en/copilot/reference/hooks-configuration). Always verify against the official documentation for the latest fields.

## `sessionStart`

**When**: Session begins (new conversation, resume, or startup)

**Payload**:
```json
{
  "timestamp": 1704614400000,
  "cwd": "/path/to/project",
  "source": "new",
  "initialPrompt": "Create a new feature"
}
```

- `source`: `"new"`, `"resume"`, or `"startup"`
- `initialPrompt`: User's first prompt (if provided)

**Parsed stdout fields**:
```json
{
  "additionalContext": "Current branch: main. Deploy target: staging."
}
```

`additionalContext` is injected directly into the session conversation, letting hooks provide environment-specific context dynamically.

---

## `sessionEnd`

**When**: Session ends

**Payload**:
```json
{
  "timestamp": 1704618000000,
  "cwd": "/path/to/project",
  "reason": "complete"
}
```

- `reason`: `"complete"`, `"error"`, `"abort"`, `"timeout"`, or `"user_exit"`

**stdout**: Ignored

---

## `userPromptSubmitted`

**When**: User submits a prompt

**Payload**:
```json
{
  "timestamp": 1704614500000,
  "cwd": "/path/to/project",
  "prompt": "Fix the authentication bug"
}
```

- `prompt`: Exact text the user submitted

**stdout**: Ignored  
**Deny mechanism**: Non-zero exit blocks the prompt

---

## `preToolUse`

**When**: Before any tool execution

**Payload**:
```json
{
  "timestamp": 1704614600000,
  "cwd": "/path/to/project",
  "toolName": "bash",
  "toolArgs": "{\"command\":\"rm -rf dist\",\"description\":\"Clean build directory\"}"
}
```

**Critical**: `toolArgs` is a **JSON string** — parse it a second time to access its fields.

**Parsed stdout fields**:
```json
{
  "permissionDecision": "deny",
  "permissionDecisionReason": "Destructive command blocked",
  "modifiedArgs": "{\"command\":\"echo 'safe command'\"}",
  "additionalContext": "Environment: production"
}
```

| Field | Purpose |
|-------|---------|
| `permissionDecision` | `"deny"` blocks the tool. `"allow"` and `"ask"` also accepted; only `"deny"` is processed. |
| `permissionDecisionReason` | Human-readable reason shown to user |
| `modifiedArgs` or `updatedInput` | Replacement tool arguments (used instead of originals) |
| `additionalContext` | Text injected into agent's context for this turn |

---

## `postToolUse`

**When**: After tool execution

**Payload**:
```json
{
  "timestamp": 1704614700000,
  "cwd": "/path/to/project",
  "toolName": "bash",
  "toolArgs": "{\"command\":\"npm test\"}",
  "toolResult": {
    "resultType": "success",
    "textResultForLlm": "All tests passed (15/15)"
  }
}
```

- `resultType`: `"success"`, `"failure"`, or `"denied"`

**stdout**: Ignored

---

## `postToolUseFailure`

**When**: After a tool execution fails

**Payload**:
```json
{
  "timestamp": 1704614750000,
  "cwd": "/path/to/project",
  "toolName": "bash",
  "toolArgs": "{\"command\":\"npm test\"}",
  "toolResult": {
    "resultType": "failure",
    "textResultForLlm": "Tests failed: 3/15 passing"
  }
}
```

**stdout**: Ignored

---

## `agentStop`

**When**: Agent completes its turn

**Payload**:
```json
{
  "timestamp": 1704618000000,
  "cwd": "/path/to/project"
}
```

Minimal payload — use for end-of-turn actions like `git diff --stat` or final validation.

**stdout**: Ignored

---

## `subagentStart`

**When**: A subagent begins execution

**Payload**:
```json
{
  "timestamp": 1704615000000,
  "cwd": "/path/to/project",
  "agentName": "Explore",
  "prompt": "Find all authentication handlers"
}
```

**stdout**: Ignored

---

## `subagentStop`

**When**: A subagent completes

**Payload**:
```json
{
  "timestamp": 1704615100000,
  "cwd": "/path/to/project",
  "agentName": "Explore"
}
```

**stdout**: Ignored

---

## `errorOccurred`

**When**: An error occurs during execution

**Payload**:
```json
{
  "timestamp": 1704614800000,
  "cwd": "/path/to/project",
  "error": {
    "message": "Network timeout",
    "name": "TimeoutError",
    "stack": "TimeoutError: Network timeout\n    at ..."
  }
}
```

**stdout**: Ignored

---

## `preCompact`

**When**: Before context compaction occurs

**Payload**:
```json
{
  "timestamp": 1704615500000,
  "cwd": "/path/to/project"
}
```

**stdout**: Ignored

---

## `permissionRequest`

**When**: Agent requests permission for an action

**Payload**:
```json
{
  "timestamp": 1704615600000,
  "cwd": "/path/to/project",
  "requestType": "fileEdit",
  "details": {
    "path": "src/app.ts"
  }
}
```

**stdout**: May be parsed depending on implementation

---

## Summary Table

| Event | stdout Parsed? | Deny Mechanism | Common Use |
|-------|----------------|----------------|------------|
| `sessionStart` | ✅ `additionalContext` | non-zero exit | Context injection |
| `sessionEnd` | ❌ | non-zero exit | Cleanup |
| `userPromptSubmitted` | ❌ | non-zero exit | Audit, block prompt |
| `preToolUse` | ✅ `permissionDecision`, `modifiedArgs`, `additionalContext` | exit 0 + deny JSON (preferred) or non-zero exit | Guardrails, validation |
| `postToolUse` | ❌ | non-zero exit | Logging, formatting |
| `postToolUseFailure` | ❌ | non-zero exit | Recovery |
| `agentStop` | ❌ | non-zero exit | Final validation |
| `subagentStart` | ❌ | non-zero exit | Subagent audit |
| `subagentStop` | ❌ | non-zero exit | Output validation |
| `errorOccurred` | ❌ | non-zero exit | Diagnostics |
| `preCompact` | ❌ | non-zero exit | Pre-compaction work |
| `permissionRequest` | Varies | Varies | Approval workflow |
