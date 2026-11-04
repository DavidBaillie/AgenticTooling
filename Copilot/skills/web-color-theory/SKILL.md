---
name: web-color-theory
description: 'Use when creating, improving, or auditing web color systems: palette harmony, color ramps, semantic tokens, CSS color formats, light and dark themes, gamut, gradients, contrast, and component states.'
---

# Web Color Theory

Choose color as a system: establish palette relationships and hierarchy, build the ramps each role needs, then validate their use. Hue relationships do not guarantee accessible contrast.

Never report a contrast value you did not measure or estimate a color you could compute. A notation, tinted neutral, or gradient interpolation space is a project choice, not a finding. Broken role mappings, failing required pairs, and unintended gamut clipping are findings.

## When to Use This Skill

- Create or refine a website, web app, or design-system palette.
- Derive supporting colors from a brand or base color.
- Generate color ramps, map semantic tokens, or implement supported themes.
- Review color harmony, hierarchy, contrast, gamut, gradients, or component states.
- Convert color notation when a conversion or migration is explicitly in scope.

## Scope and Permissions

- Preserve the project's notation, token vocabulary, brand constraints, browser matrix, and established interaction hierarchy.
- During an audit or review, report findings without changing colors. A request to improve or fix colors authorizes relevant changes; remeasure every affected pair afterward.
- Inventory an existing palette before restructuring it. Palette consolidation and bulk notation conversion need an explicit request or approval; do not make them side effects of a local fix.
- Treat palette sizes, hue spacing, ramp spacing, and emphasis patterns as contextual defaults, not automatic defects. Applicable accessibility requirements and honest measurement are not optional.

## Core Workflow

### Create or Improve

1. Identify the product, audience, primary action, existing color system, brand constraints, supported themes, and required contrast standard.
2. Choose the hue families the product needs. Read only the matching relationship reference: [analogous](./references/analogous-colors.md), [complementary](./references/complementary-colors.md), [triadic](./references/triadic-colors.md), [split-complementary](./references/split-complementary-colors.md), or [monochromatic](./references/monochromatic-colors.md).
3. Assign jobs to ramp steps before computing values. Follow [palette structure](./references/palette-structure.md) and [palette generation](./references/palette-generation.md); emit the project's notation.
4. Map values to semantic roles using [token naming](./references/token-naming.md). Tune lightness and chroma until surfaces, text, borders, actions, and feedback states remain distinct.
5. Apply the system consistently and validate affected pairs in every supported theme. Use [color usage](./references/color-usage.md), [color formats and gamut](./references/color-formats.md), and [contrast](./references/contrast.md) as needed.

### Audit an Existing System

1. Inventory tokens, literals, ramp families, and theme mappings within the requested scope, including relevant SVGs, charts, and email templates.
2. Inspect role usage, naming, duplication, hierarchy, gamut, and theme behavior. Use the [audit checklist](./references/color-usage.md#audit-checklist); distinguish design preferences from defects.
3. Measure foregrounds against their actual backgrounds. Check states and appearances, and separate declared-value calculations from rendered verification.
4. Report root causes, affected locations, measured evidence, proposed fixes, and unverified checks. Apply changes only when authorized by the task, then remeasure.

## System Rules

### Palette and Tokens

- Start with few hue families: usually one neutral, one accent, and only the status colors the product renders. Related supporting hues need a role. Hue-family count is not a limit on ramp steps.
- Let one color or neutral dominate. Use neutrals or near-neutrals for large surfaces and long-form text, and reserve strong accents for actions and meaningful states.
- Generate only consumed ramp steps. For a new system, prefer hue-named primitives and role-named semantic tokens; components consume the semantic tier. Preserve an established architecture unless its migration is in scope.
- Keep decisions centralized in CSS custom properties. Never borrow a border token for text merely because its current value happens to work.
- Preserve fixed brand values. If a brand color cannot support the intended text, use another ramp step or treatment rather than silently changing the brand.

### Generation and Use

- Use available color tooling, not guesses, to generate ramps, convert notation, and measure colors. Examples include `culori`, `colorjs.io`, and `chroma.js`; loading this skill does not justify adding a dependency.
- Perceived lightness here means OKLCH `L`, from `0` to `1`. A useful ramp generally keeps a stable hue, peaks in chroma near its middle, and has finer light-end spacing. Validate exceptions in context.
- Match existing notation. For a new system, prefer OKLCH when browser support allows it; provide sRGB or syntax fallbacks where the target matrix needs them.
- Keep semantic color meanings consistent, including locale-specific conventions. Nearby hues are not reliable substitutes for distinct state cues; `15` degrees of hue separation is a starting heuristic, not an accessibility test.
- When filled color communicates primary emphasis, usually give it to one primary action per view or task group. Preserve established alternative hierarchies; colored states and categories need not follow this pattern.
- For a new gradient, `in oklab` is a useful default. Use `in oklch` when a vivid hue-wheel transition is wanted. Existing interpolation choices are not defects.
- Derive dark themes deliberately, not by mechanically reversing a light ramp. Use one coherent switching strategy and recheck increased-contrast variants in each appearance.
- Never communicate meaning through color alone; pair it with text, icons, labels, patterns, position, or shape.

## Contrast and Verification

- Use the project's required standard; otherwise use WCAG 2.2 AA as the baseline. Normal text needs at least 4.5:1, large text 3:1, and required non-text UI or state indicators 3:1 against adjacent colors. Apply the exceptions and sizing rules in [contrast](./references/contrast.md).
- APCA can supplement text evaluation, but it does not replace WCAG conformance checks. Identify the metric used and apply its own font-size and weight guidance.
- Measure the foreground against the background it actually renders on, not automatically the page background. Resolve alpha, mixed colors, overlays, gradients, and images before claiming rendered contrast.
- Check body text, controls, focus, hover, disabled presentation, error, and success states in every supported theme. Do not apply active-control contrast requirements to exempt inactive controls.
- Without a browser, check declared tokens, their gamut, theme mappings, and computable pairs. Rendered backgrounds, compositing, and uninspected states remain `Not verified`.
- After an authorized contrast fix, adjust lightness first, preserve the intended hue where possible, keep the result in gamut, and remeasure. Do not assume a darker step or white label passes.

## Required Output

For palette creation or improvement:

- Color values, consumed ramp steps, and their intended roles.
- Semantic token mapping or the CSS custom properties to add or revise.
- Foreground/background pairs for body text, controls, focus, and meaningful states, with measured results and applicable thresholds.
- Validation performed, themes and states covered, and any pairing still `Not verified`.

For an audit:

- Group findings under the violated principle, ordered by severity. Use one row per root cause and list every affected location.
- `HIGH`: body or control text fails its required contrast, meaning relies on color alone, or semantic color contradicts its meaning.
- `MEDIUM`: a noticeable theme, token, or gamut failure. `LOW`: isolated polish.

| Severity | Location | Before | After | Why |
| --- | --- | --- | --- | --- |

`Location` is `path/to/file:line`. `After` is a proposed correction unless changes were authorized and verified. `Why` names the principle and user impact; contrast findings include the pair, measured value, and missed threshold.

End with `Block` when any `HIGH` remains, or `Approve` for verified inspected scope otherwise. Never approve uninspected coverage; required checks that could not run remain `Not verified`. With no findings, state "No actionable color findings" and report verification.

## References

Load only the references needed for the current task.

- [Analogous Colors](./references/analogous-colors.md)
- [Complementary Colors](./references/complementary-colors.md)
- [Triadic Colors](./references/triadic-colors.md)
- [Split-Complementary Colors](./references/split-complementary-colors.md)
- [Monochromatic Colors](./references/monochromatic-colors.md)
- [Palette Structure](./references/palette-structure.md)
- [Palette Generation](./references/palette-generation.md)
- [Token Naming](./references/token-naming.md)
- [Color Formats and Gamut](./references/color-formats.md)
- [Color Usage and Audit Checklist](./references/color-usage.md)
- [Contrast](./references/contrast.md)
