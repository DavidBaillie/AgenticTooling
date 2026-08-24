---
name: web-wcag-compliance
description: 'Use when checking, fixing, reviewing, or validating a web page or frontend workflow for WCAG 2.2 Level AA. Inspect semantic HTML, keyboard and pointer interaction, focus, forms, contrast, responsive reflow, dynamic states, media alternatives, and assistive-technology behavior; make focused fixes and report tested scope, evidence, exceptions, and residual risk.'
---

# WCAG 2.2 AA Compliance

Check and improve complete web pages and workflows against WCAG 2.2 Level AA. Treat accessibility as functional behavior, not visual polish. Use the [full checklist](references/wcag-22-aa-checklist.md) when a criterion applies, and record evidence for every applicable criterion.

## When to Use This Skill

- Audit or review HTML, CSS, JavaScript, React, or other web UI code
- Fix accessibility defects or regressions
- Validate a page, route, component, form, dialog, overlay, or responsive state
- Assess keyboard, touch, screen-reader, zoom, contrast, or reduced-motion behavior
- Prepare a WCAG 2.2 AA test report or release gate

## Operating Rules

- Inspect source, rendered output, routes, states, dialogs, forms, media, third-party content, and error paths before changing code.
- Identify users, input methods, supported browsers, assistive technologies, viewport sizes, zoom settings, languages, and known exceptions in scope.
- Prefer semantic HTML and native controls. Add ARIA only when native semantics cannot express the behavior, and keep names and states synchronized with the DOM.
- Preserve reading order, focus order, entered data, content meaning, and operation when CSS, images, JavaScript, or color is unavailable where practical.
- Fix the controlling cause, not only the visible symptom. Reuse existing semantic components, focus utilities, validation patterns, live regions, and test helpers.
- Record each applicable result as `pass`, `fail`, `not applicable`, `not tested`, or `pending`, with scope and evidence. Do not claim AA conformance unless every applicable Level A and AA criterion is `pass` or `not applicable`, critical flows pass, and any claimed exception has a documented conformance basis.
- Treat release approval and conformance as separate decisions. Accepted residual risk may support release, but it does not turn a failed or untested criterion into a conforming result.

## Workflow

### 1. Define Scope

List the page or route, components, workflows, states, content, embedded services, and technologies under review. Include default, populated, empty, loading, success, warning, validation-error, network-error, permission, timeout, selected, expanded, disabled, and modal states where applicable.

### 2. Inspect Before Editing

Check:

- Document language, unique title, one useful page heading, landmarks, skip navigation, and heading hierarchy
- Labels, required state, instructions, autocomplete purpose, error association, and preservation of valid input
- Native controls, accessible names, roles, values, states, descriptions, and duplicate IDs
- DOM order versus visual order, responsive layout, sticky content, overlays, and focus visibility
- Images, icons, charts, canvas, SVG, audio, video, captions, transcripts, and descriptions
- Keyboard and touch paths, pointer cancellation, target sizes, drag alternatives, motion, flashing, timing, and hover/focus content
- Text and non-text contrast, 200% text-only resize, 400% browser zoom or equivalent reflow, text spacing, forced colors, and reduced motion
- Dynamic status, progress, validation, result, and route-change announcements

### 3. Update the Page

Make the smallest focused changes that restore the relevant criteria:

- Use real links for navigation and buttons for actions.
- Give every control a visible label or concise accessible name. Include visible label text in the accessible name.
- Associate help and errors with fields using `for`/`id`, `aria-describedby`, and accurate `aria-invalid` or native validity.
- Implement complete keyboard behavior, visible focus, logical focus movement, escape behavior, dialog focus management, and focus return.
- Add text alternatives and media alternatives based on purpose. Mark decorative media as decorative.
- Replace color-only communication with text, icons, patterns, or programmatic state.
- Allow content to reflow without clipping, overlap, or ordinary horizontal scrolling at narrow widths and high zoom.
- Provide non-drag, non-gesture, non-motion, and non-timing alternatives where the criterion requires them.
- Announce meaningful asynchronous changes with an appropriate non-interruptive live region; move focus only when user action is required.
- Respect `prefers-reduced-motion` and forced-colors modes without removing essential content or controls.

### 4. Validate

Run the smallest relevant checks available, then test the complete primary journey manually:

- Build, lint, typecheck, unit tests, HTML validation, duplicate-ID checks, and an accessibility engine on representative routes and states
- Keyboard-only operation with `Tab`, `Shift+Tab`, `Enter`, `Space`, arrow keys, and `Escape` as appropriate
- Accessible names, roles, values, state changes, live messages, form errors, dialogs, route changes, and focus restoration
- A representative supported browser and assistive-technology pair, plus a second browser engine and mobile coverage when in scope
- 200% text-only resize; 400% browser zoom or a 320 CSS-pixel viewport for a 1280 CSS-pixel design; text-spacing overrides, forced colors, reduced motion, touch, and long content
- Contrast for text, controls, focus indicators, selected and error states, and meaningful graphics

Use automation to find likely defects, not to prove conformance. Re-test after every fix that changes behavior or structure.

## Reporting

For every finding, record:

- WCAG success criterion and severity: `blocker`, `high`, `medium`, `low`, or `pending`
- Page, route, component, state, input method, browser, viewport, and assistive technology
- Reproduction steps, expected behavior, actual behavior, source location, and evidence
- User impact, workaround, owner, status, retest result, and residual risk

Use this minimum record for each finding and criterion result:

```markdown
- Status: `pass` | `fail` | `not applicable` | `not tested` | `pending`
- Scope: route, component, state, input method, browser, viewport, and assistive technology
- Evidence: reproduction steps, selector, screenshot, recording, or test output
- Residual risk: none, documented exception, or follow-up owner
```

End with tested scope, tools and versions, browsers and assistive technologies, exceptions, untested areas, and separate release and conformance decisions. Use `pending` for content, product, vendor, legal, or deployment decisions that are unresolved.

## Gotchas

- **Do not call a page WCAG 2.2 AA compliant from an automated score or static scan.** Automation cannot establish complete keyboard, screen-reader, zoom, pointer, content, or workflow behavior.
- **Do not hide content or disable controls to make a check pass.** Preserve an accessible path and explain why a control is unavailable.
- **Do not use placeholder text as the only label.** It disappears during entry and does not reliably provide an accessible name.
- **Do not use color as the only state signal.** Add text, an icon, a pattern, or a programmatic state.
- **Do not add positive `tabindex` values.** They create brittle focus order and often separate visual and DOM order.
- **Do not put essential instructions only in tooltips, `title`, hover, or focus content.** Keyboard and touch users must be able to discover them.
- **Do not use `aria-hidden="true"` on focusable content or an ancestor of focusable content.** This removes operable content from the accessibility tree.
- **Do not trigger irreversible actions on pointer-down.** Allow cancellation, undo, or confirmation as appropriate.
- **Do not claim full coverage when a criterion is untested or an exception is unresolved.** Report the exact boundary and residual risk.

## References

- [WCAG 2.2 AA checklist](references/wcag-22-aa-checklist.md)
- [WCAG 2.2 Recommendation](https://www.w3.org/TR/WCAG22/)
- [WAI-ARIA Authoring Practices Guide](https://www.w3.org/WAI/ARIA/apg/)
