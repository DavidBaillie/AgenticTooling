---
name: wcag-22-aa
description: Maintain WCAG 2.2 Level AA accessibility during coding, review, testing, and release.
---

# WCAG 2.2 AA Coding Skill

Use this checklist when building, changing, reviewing, or releasing a web interface. Treat accessibility as a functional requirement, not as a final visual polish step.

This checklist supports WCAG 2.2 Level A and AA review. It does not replace the WCAG 2.2 specification, a qualified accessibility review, user testing, or legal advice.

## Conformance Rules

- Test every applicable Level A and Level AA success criterion.
- Evaluate complete pages, states, workflows, and responsive presentations, not isolated screenshots.
- Include content, components, client-side behavior, third-party widgets, generated markup, and error states.
- Do not claim conformance from an automated score, a static source scan, or a single browser.
- Use accessibility-supported technologies. Provide an accessible alternative when a feature depends on unsupported technology.
- Preserve meaning and operation when JavaScript, CSS, images, audio, or color are unavailable where the feature permits progressive enhancement.
- Do not hide, remove, disable, or replace content merely to make a check pass.
- Record each result as `pass`, `fail`, `not applicable`, `not tested`, or `pending`, with scope and reproducible evidence. A checkbox alone does not establish a pass.
- Document exceptions and residual risk. Do not silently downgrade a requirement.

## Assessment Record

Use one record for each applicable criterion and each material finding:

```markdown
- Status: `pass` | `fail` | `not applicable` | `not tested` | `pending`
- Scope: route, component, state, input method, browser, viewport, and assistive technology
- Evidence: reproduction steps, selector, screenshot, recording, or test output
- Residual risk: none, documented exception, or follow-up owner
```

Use checkboxes below only as review-progress markers. Do not use them as the result of an assessment.

## Workflow

### 1. Inspect Before Coding

- Identify routes, templates, layouts, components, states, forms, dialogs, overlays, media, embedded content, and external services.
- Read product, content, design, localization, and legal requirements that affect accessibility.
- Identify the users, input methods, browsers, assistive technologies, zoom levels, languages, and devices in scope.
- List accessibility assumptions, known exceptions, and content still awaiting approval.
- Find existing semantic components, focus utilities, live-region utilities, validation patterns, and automated checks. Reuse them.

### 2. Build an Accessible Baseline

- Use semantic HTML elements before ARIA.
- Give every interactive control an accessible name, role, value, state, and description where needed.
- Keep DOM order aligned with the intended reading and focus order.
- Make the primary journey usable with keyboard and touch.
- Add visible focus styles, skip navigation, page titles, language metadata, and meaningful document structure.
- Define text, component, focus, and non-text contrast tokens before styling individual components.

### 3. Implement and Review States

For each component, inspect at least:

- default, hover, focus, focus-visible, active, visited, selected, checked, expanded, pressed, disabled, loading, success, warning, and error states where applicable;
- empty, populated, long-content, validation-error, network-error, permission, and timeout states where applicable;
- keyboard, pointer, touch, zoomed, high-contrast or forced-colors, reduced-motion, and narrow-width presentations.

### 4. Validate Before Release

- Run the smallest relevant build, lint, typecheck, unit test, and accessibility test commands.
- Test the primary flows manually with keyboard only and with a screen reader where available.
- Test text-only resizing to 200% for 1.4.4.
- Test 400% browser zoom, or the equivalent 320 CSS-pixel presentation for content designed for 1280 CSS pixels, for 1.4.10.
- Test touch targets, pointer cancellation, focus visibility, overlays, sticky content, and dragging alternatives.
- Test with text spacing overrides, increased text size, reduced motion, and forced colors. Test light or dark user-agent settings when the product supports them; this is additional robustness coverage, not a standalone WCAG 2.2 AA requirement.
- Re-test after fixing defects. Report the exact scope of the final check.

## WCAG 2.2 Level A and AA Checklist

### Principle 1: Perceivable

#### 1.1 Text Alternatives

- [ ] **1.1.1 Non-text Content (A):** Provide a text alternative for every meaningful image, icon, chart, canvas, control, CAPTCHA, and non-text object. Use empty alt text for purely decorative images. Do not duplicate adjacent visible text. Provide a complete text or data alternative for complex graphics.

#### 1.2 Time-based Media

- [ ] **1.2.1 Audio-only and Video-only (Prerecorded) (A):** Provide a text transcript or equivalent for prerecorded audio-only content. Provide a transcript or audio description for prerecorded video-only content, as applicable.
- [ ] **1.2.2 Captions (Prerecorded) (A):** Provide synchronized captions for prerecorded synchronized media. Captions include speech, speaker identification when needed, and meaningful sounds.
- [ ] **1.2.3 Audio Description or Media Alternative (Prerecorded) (A):** Provide audio description or a complete text alternative for prerecorded video with important visual information.
- [ ] **1.2.4 Captions (Live) (AA):** Provide synchronized captions for live synchronized media.
- [ ] **1.2.5 Audio Description (Prerecorded) (AA):** Provide synchronized audio description for prerecorded video when visual information is needed to understand the content.

#### 1.3 Adaptable

- [ ] **1.3.1 Info and Relationships (A):** Express headings, lists, tables, groups, labels, required state, errors, relationships, and structure in the DOM or programmatically determinable markup. Do not convey structure by visual styling alone.
- [ ] **1.3.2 Meaningful Sequence (A):** Keep reading and interaction order meaningful when content is linearized. CSS positioning must not create a misleading order.
- [ ] **1.3.3 Sensory Characteristics (A):** Do not rely only on shape, size, position, orientation, sound, or color to identify or operate content. Use labels such as `Close`, not only `the button on the right`.
- [ ] **1.3.4 Orientation (AA):** Support portrait and landscape orientation unless a specific orientation is essential to the function.
- [ ] **1.3.5 Identify Input Purpose (AA):** Mark common personal-data fields with the correct `autocomplete` token and programmatically determinable purpose. Use the token on the actual field, not only on a visual label.

#### 1.4 Distinguishable

- [ ] **1.4.1 Use of Color (A):** Do not use color as the only way to convey information, indicate an action, distinguish a control, or identify a response. Add text, shape, pattern, icon, or programmatic state.
- [ ] **1.4.2 Audio Control (A):** For audio that plays automatically for more than 3 seconds, provide a mechanism to pause or stop it and an independent volume control. Do not let autoplay audio interfere with screen-reader output.
- [ ] **1.4.3 Contrast (Minimum) (AA):** Maintain at least 4.5:1 contrast for normal text and 3:1 for large text. Large text is at least 18 point, or 14 point bold. Check text against its actual background, including overlays and gradients.
- [ ] **1.4.4 Resize Text (AA):** Support text resize to 200% without loss of content or functionality, except captions and images of text. Use responsive units and allow content to reflow.
- [ ] **1.4.5 Images of Text (AA):** Use real text instead of an image of text when the same visual presentation can be achieved with technology. Logos are an exception.
- [ ] **1.4.10 Reflow (AA):** At 400% browser zoom, or a viewport equivalent to 320 CSS pixels for content designed for a 1280 CSS-pixel width, content must work without two-dimensional scrolling. Apply only the exceptions defined by the criterion, including content that requires two-dimensional layout such as maps, diagrams, and media controls.
- [ ] **1.4.11 Non-text Contrast (AA):** Maintain at least 3:1 contrast for meaningful user-interface components, states, focus indicators, and graphics required to understand content against adjacent colors. Decorative or inactive parts may be exempt where the criterion permits.
- [ ] **1.4.12 Text Spacing (AA):** Do not clip, overlap, or lose content when users set line height to at least 1.5 times font size, paragraph spacing to at least 2 times font size, letter spacing to at least 0.12 times font size, and word spacing to at least 0.16 times font size.
- [ ] **1.4.13 Content on Hover or Focus (AA):** Make additional hover or focus content dismissible without moving the pointer or focus, hoverable so it can be read, and persistent until dismissed, no longer relevant, or no longer hovered or focused. Do not use content that disappears when the pointer moves toward it.

### Principle 2: Operable

#### 2.1 Keyboard Accessible

- [ ] **2.1.1 Keyboard (A):** Make every operation available through a keyboard interface. Do not require a mouse, touch gesture, hover, or timing-dependent pointer action.
- [ ] **2.1.2 No Keyboard Trap (A):** Let keyboard users move focus into and out of every component. For dialogs, menus, embeds, and custom widgets, provide an expected exit such as `Escape` or a logical next control.
- [ ] **2.1.4 Character Key Shortcuts (A):** Let users turn off or remap single-character keyboard shortcuts, or activate them only when the relevant component has focus. Do not hijack keys used by assistive technology.

#### 2.2 Enough Time

- [ ] **2.2.1 Timing Adjustable (A):** Let users turn off, adjust, or extend time limits, with the allowed exceptions. Warn before expiration and preserve entered data when a session expires.
- [ ] **2.2.2 Pause, Stop, Hide (A):** Provide controls to pause, stop, or hide moving, blinking, or scrolling content that starts automatically, lasts more than 5 seconds, and is presented with other content. Provide a mechanism for auto-updating content where required.

#### 2.3 Seizures and Physical Reactions

- [ ] **2.3.1 Three Flashes or Below Threshold (A):** Do not include content that flashes more than three times in one second or exceeds the flash and red-flash thresholds.

#### 2.4 Navigable

- [ ] **2.4.1 Bypass Blocks (A):** Provide a mechanism to skip repeated blocks, normally a visible-on-focus skip link and meaningful landmarks.
- [ ] **2.4.2 Page Titled (A):** Give every page a descriptive, unique title that identifies its topic or purpose.
- [ ] **2.4.3 Focus Order (A):** Keep focus order logical and meaningful. Do not use positive `tabindex` values or DOM tricks that create a confusing sequence.
- [ ] **2.4.4 Link Purpose (In Context) (A):** Make each link's purpose determinable from its text or programmatic context. Avoid repeated ambiguous labels such as `Read more` without an accessible context.
- [ ] **2.4.5 Multiple Ways (AA):** Provide at least two ways to locate pages within a set of pages, such as navigation, search, sitemap, table of contents, or relevant links. A process step or result page may be exempt where applicable.
- [ ] **2.4.6 Headings and Labels (AA):** Use descriptive headings and labels that identify topic, purpose, or expected input. Keep heading hierarchy logical and do not use headings only for visual styling.
- [ ] **2.4.7 Focus Visible (AA):** Make keyboard focus visibly apparent. Do not remove the browser focus indicator without providing a stronger replacement.
- [ ] **2.4.11 Focus Not Obscured (Minimum) (AA):** When a component receives keyboard focus, ensure no part of the focused component is entirely hidden by author-created content. Account for sticky headers, footers, banners, cookie notices, chat controls, and scroll containers.
- [ ] **2.4.13 Focus Appearance (AA):** Provide a focus indicator that satisfies the criterion's area and contrast requirements: an area at least equivalent to a 2 CSS-pixel perimeter around the unfocused component, and at least 3:1 contrast against adjacent colors and the component's unfocused state, subject to the criterion's exceptions.

#### 2.5 Input Modalities

- [ ] **2.5.1 Pointer Gestures (A):** Provide a single-pointer alternative for multipoint or path-based gestures unless the gesture is essential or the user agent supplies the alternative.
- [ ] **2.5.2 Pointer Cancellation (A):** Start activation on `down` only when necessary. Allow cancellation by moving away, aborting, undoing, or completing the action on `up`. Do not trigger an irreversible action merely on pointer down.
- [ ] **2.5.3 Label in Name (A):** Include the visible label text in the accessible name of the control, in the same order where practical. This supports speech input.
- [ ] **2.5.4 Motion Actuation (A):** Provide a conventional control for functions triggered by device motion or user motion, and allow motion-triggered operation to be disabled unless essential.
- [ ] **2.5.7 Dragging Movements (AA):** Provide a single-pointer alternative for every drag operation unless dragging is essential or the user agent supplies the alternative. Support buttons, fields, keyboard commands, or click-to-place equivalents.
- [ ] **2.5.8 Target Size (Minimum) (AA):** Make pointer targets at least 24 by 24 CSS pixels, with the exceptions defined by the criterion. Do not rely on an exception when spacing or an equivalent control can make the target larger.

### Principle 3: Understandable

#### 3.1 Readable

- [ ] **3.1.1 Language of Page (A):** Declare the page's human language with a valid `lang` attribute on the root HTML element.
- [ ] **3.1.2 Language of Parts (AA):** Mark changes in human language with `lang` on the smallest appropriate element, except for proper names, technical terms, vernacular, and other permitted cases.

#### 3.2 Predictable

- [ ] **3.2.1 On Focus (A):** Do not initiate a context change merely because a component receives focus.
- [ ] **3.2.2 On Input (A):** Do not initiate an unexpected context change merely because a user changes a field value. Provide an explicit submit or confirmation control when needed.
- [ ] **3.2.3 Consistent Navigation (AA):** Keep repeated navigation mechanisms in the same relative order across pages unless the user initiates a change.
- [ ] **3.2.4 Consistent Identification (AA):** Identify components with the same function consistently across a set of pages. Keep accessible names, icons, and visible labels consistent.
- [ ] **3.2.6 Consistent Help (AA):** Present repeated help mechanisms, such as contact details, self-help, or human support, in the same relative order across pages. Do not require users to hunt for a help method.

#### 3.3 Input Assistance

- [ ] **3.3.1 Error Identification (A):** Identify and describe input errors in text. Associate each error with its field and expose the invalid state programmatically.
- [ ] **3.3.2 Labels or Instructions (A):** Provide labels or instructions when input is required, constrained, formatted, or otherwise needs explanation. Do not rely on placeholder text as the only label.
- [ ] **3.3.3 Error Suggestion (AA):** When a correction is known, suggest it in text. Do not discard valid user input while reporting the error.
- [ ] **3.3.4 Error Prevention (Legal, Financial, Data) (AA):** For submissions that create legal commitments, financial transactions, or modify or delete user-controlled data, provide at least one of review before submission, a way to correct input, or a confirmation and reversal mechanism.
- [ ] **3.3.7 Redundant Entry (A):** Do not require users to enter the same information more than once in the same process. Autofill, reuse, selection, or a previous value may satisfy the requirement. Exceptions include security, essential repetition, and information that is no longer valid.
- [ ] **3.3.8 Accessible Authentication (Minimum) (AA):** Do not require a cognitive function test for authentication unless an allowed exception applies. Support password managers, paste, passkeys, device-based authentication, or an accessible alternative. Do not make users solve memory, transcription, or visual puzzles as the only path.

### Principle 4: Robust

#### 4.1 Compatible

- [ ] **4.1.2 Name, Role, Value (A):** Expose the accessible name, role, value, state, and property of every user-interface component. Ensure changes are available to assistive technology. Prefer native controls; implement the complete keyboard and ARIA behavior for custom widgets.
- [ ] **4.1.3 Status Messages (AA):** Expose status, success, progress, validation, and result messages programmatically without moving focus unless the user must act. Use an appropriate live-region role such as `status`, `alert`, or `log`, and avoid interruptive announcements for routine updates.

## Non-normative Hygiene

4.1.1 Parsing was removed from WCAG 2.2. Still produce valid, well-formed HTML and avoid duplicate IDs because assistive technologies and browser behavior depend on robust markup. Do not record this as a WCAG 2.2 success-criterion result.

## Optional AAA Checks

These checks are outside a Level AA release gate unless the project explicitly includes AAA requirements:

- [ ] **2.3.2 Three Flashes (AAA):** As a stronger safeguard, do not include any content that flashes more than three times in one second, regardless of size or brightness.

## Implementation Rules

### HTML and Semantics

- Use `button` for actions and `a` with a valid destination for navigation.
- Use native `input`, `select`, `textarea`, `fieldset`, `legend`, `details`, `summary`, table, list, heading, and landmark elements when they match the behavior.
- Keep one clear `h1` for the page topic where the content model permits it. Do not skip heading levels for visual effect.
- Use landmarks such as `header`, `nav`, `main`, `aside`, and `footer` with useful accessible names when multiple landmarks share a role.
- Keep IDs unique. Associate labels, descriptions, errors, table headers, captions, and grouped controls programmatically.
- Do not use `div`, `span`, CSS pseudo-elements, or click handlers as a replacement for a native control.
- Ensure disabled controls have a justified behavior. Do not disable submit or navigation controls without exposing why and preserving an accessible path forward.

### Names, Labels, and ARIA

- Compute the accessible name from the visible label whenever possible.
- Make icon-only buttons have a concise accessible name and a tooltip when the icon is unfamiliar.
- Do not put essential instructions only in `title`, placeholder, tooltip, hover, or color.
- Use `aria-describedby` for helpful instructions and field errors. Use `aria-errormessage` only with a correctly exposed invalid state and a supported implementation.
- Use `aria-expanded`, `aria-controls`, `aria-selected`, `aria-checked`, `aria-pressed`, `aria-current`, and `aria-invalid` only when their state is accurate.
- Never add a conflicting ARIA role or state to native semantics. Do not use `aria-hidden="true"` on focusable content or on an ancestor of focusable content.
- Keep the accessibility tree synchronized with visual and DOM state.

### Keyboard and Focus

- Make the complete user journey usable with `Tab`, `Shift+Tab`, `Enter`, `Space`, arrow keys where the widget pattern requires them, and `Escape` where expected.
- Keep focus in the document and in a logical order. Do not use positive `tabindex` values.
- Move focus deliberately after navigation, opening a dialog, submitting a form, or displaying a blocking error. Do not move focus for routine status messages.
- For dialogs, provide an accessible name, move focus inside on open, keep focus inside while modal, close on `Escape` when appropriate, and return focus to the invoking control when closed.
- Ensure visible focus is clear of sticky or fixed content.

### Forms and Errors

- Give every form control a persistent visible label.
- Identify required fields in text and programmatic state. Do not rely on an asterisk alone.
- Provide format, unit, length, and input-purpose instructions before the user needs them.
- Validate both client and server side. Return errors without losing valid input.
- Associate summary errors with fields and make the summary discoverable. Focus the first actionable error only when that behavior is predictable and useful.
- Announce dynamic errors and success messages without creating an announcement storm.
- For authentication and high-impact submissions, support review, correction, confirmation, reversal, and accessible alternatives.

### Images, Icons, and Media

- Write alt text based on purpose and context, not visual inventory.
- Mark decorative images as decorative and prevent redundant announcements.
- Provide transcripts, captions, audio descriptions, and controls appropriate to each media type.
- Give charts and data visualizations a text or data alternative. Do not make a canvas or SVG graphic the only representation of data.
- Ensure media controls are keyboard accessible, named, visible, and sufficiently large.

### Color, Layout, and Typography

- Use semantic color tokens and verify actual rendered contrast, including disabled, selected, error, hover, focus, gradients, images, and translucent surfaces.
- Do not communicate state by color alone.
- Let content reflow. Avoid fixed heights, clipped text, `overflow: hidden` used to conceal content, and interactions that require horizontal scrolling for ordinary content.
- Test long words, translated text, user-generated text, large text settings, increased spacing, and browser zoom.
- Keep text readable with sufficient line height and avoid justified text for ordinary reading.
- Do not use CSS order, transforms, absolute positioning, or visual hiding to contradict meaningful source order.
- Respect user preferences such as `prefers-reduced-motion` and forced-colors. Keep essential content and controls available when animation is disabled.

### Pointer, Touch, and Motion

- Provide click or keyboard alternatives for hover, drag, swipe, pinch, shake, tilt, and other complex gestures.
- Do not make destructive or irreversible actions fire on pointer-down. Support cancellation or undo.
- Keep targets separated and at least 24 by 24 CSS pixels unless a documented criterion exception applies.
- Do not require precise timing, simultaneous actions, or device motion for an essential operation.
- Avoid autoplay, parallax, rapid transitions, and flashing. Pause or stop nonessential motion.

### Dynamic Content and Single-page Applications

- Update the document title and announce meaningful route changes.
- Use real links for navigation and restore focus after route changes.
- Expose loading, progress, success, and error states to assistive technology.
- Keep DOM updates from unexpectedly destroying focus or entered data.
- Make overlays, popovers, drawers, and dialogs dismissible, keyboard reachable, and correctly layered.
- Ensure virtualized or lazy-loaded content remains discoverable and operable with keyboard and assistive technology.

## Test Matrix

### Automated Checks

Use automated checks to find likely defects, not to prove conformance.

- Validate HTML, duplicate IDs, missing labels, invalid ARIA, missing document language, missing titles, heading structure, and image alternatives.
- Run an accessibility engine against representative routes and states.
- Check computed contrast for text, controls, focus, and meaningful graphics.
- Test tab order and focusability in integration or end-to-end tests.
- Test keyboard activation, dialog behavior, live regions, form errors, route changes, and reduced-motion behavior.
- Fail CI for deterministic regressions, but keep manual review requirements visible.

### Manual Checks

- Complete the primary journey with keyboard only.
- Complete the primary journey with a screen reader and no mouse where feasible.
- Verify accessible names and state announcements for custom controls.
- Verify skip links, landmarks, heading navigation, form navigation, dialog focus, and status messages.
- Test text-only resizing to 200%.
- Test 400% browser zoom or the equivalent 320 CSS-pixel presentation for content designed for 1280 CSS pixels. A 400 CSS-pixel viewport is useful additional narrow-layout coverage.
- Apply the WCAG text-spacing values and confirm no clipping or overlap.
- Test forced colors or high-contrast mode, light and dark user-agent settings, and reduced motion.
- Test touch and pointer cancellation, target size, dragging alternatives, and hover/focus content.
- Test captions, transcripts, descriptions, controls, and keyboard operation for media.

### Browser and Assistive Technology Coverage

Choose a representative supported matrix and record it. At minimum, include:

- one Chromium-based browser, one Firefox or WebKit-based browser, and a mobile browser;
- keyboard-only interaction;
- one screen reader on the primary desktop platform and one mobile screen reader when mobile is in scope;
- browser zoom, text spacing, forced colors, reduced motion, touch, and narrow viewport checks.

## Defect Reporting

For every issue, record:

- success criterion and severity;
- page, route, component, and state;
- exact steps to reproduce;
- expected and actual behavior;
- affected input method, browser, viewport, and assistive technology;
- source location, selector, screenshot, recording, or test output;
- user impact and workaround, if any;
- owner, status, retest result, and residual risk.

Use severity consistently:

- `blocker`: prevents a primary task, access to essential content, or safe or legal completion;
- `high`: creates a serious barrier or breaks a major journey;
- `medium`: materially reduces clarity, operability, or independence;
- `low`: limited impact or a minor inconsistency;
- `pending`: requires content, design, product, vendor, or deployment input.

## Release and Conformance Gates

Keep release approval separate from a WCAG conformance claim.

### Conformance Claim

Do not call the product WCAG 2.2 AA conformant when any applicable Level A or AA criterion is `fail`, `not tested`, or `pending`, or when a claimed exception lacks a documented conformance basis. Confirm:

- [ ] All applicable criteria have a recorded result.
- [ ] Every applicable criterion is `pass` or `not applicable`.
- [ ] Critical keyboard and screen-reader journeys pass.
- [ ] Responsive, text-resize, browser-zoom, text-spacing, reduced-motion, forced-colors, and touch checks are complete.
- [ ] Media alternatives, labels, errors, live messages, and authentication paths are reviewed.
- [ ] Third-party and embedded content is included in the assessment.
- [ ] The report states tested scope, tools, browsers, assistive technologies, exceptions, and residual risk.

### Release Approval

A product may be released with formally accepted accessibility risk, but that decision must not be reported as WCAG conformance. Record the owner, rationale, affected users, workaround, deadline, and retest plan for every accepted failure.

## References

- [WCAG 2.2 Recommendation](https://www.w3.org/TR/WCAG22/)
- [WCAG 2.2 Understanding Documents](https://www.w3.org/WAI/WCAG22/Understanding/)
- [WAI-ARIA Authoring Practices Guide](https://www.w3.org/WAI/ARIA/apg/)
- [Web Content Accessibility Guidelines Overview](https://www.w3.org/WAI/standards-guidelines/wcag/)
