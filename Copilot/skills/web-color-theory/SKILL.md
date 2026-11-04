---
name: web-color-theory
description: 'Use when creating, fixing, or auditing web color systems: palette harmony, color ramps, semantic tokens, themes, contrast, CSS color formats, gamut, gradients, and chart colors.'
---

# Web Color Theory

Choose roles before values. Build only the palette the interface uses, then verify actual foreground/background pairs.

## Non-Negotiable Rules

- Preserve established notation, token architecture, fixed brand values, browser support, and interaction hierarchy unless their change is in scope.
- An audit reports without editing. A fix request authorizes relevant corrections, not incidental palette consolidation or bulk notation migration.
- Use available tooling to compute colors and contrast; never invent measurements. Do not install a dependency just because this skill names it.
- Components consume appropriate semantic roles when that layer exists. Do not borrow border tokens for text merely because their values currently work.
- Never rely on hue alone to convey meaning. Provide a visible distinguishing cue such as a label, icon, pattern, or shape.
- Use the project's required standard; otherwise use WCAG 2.2 AA. APCA may supplement text evaluation, not replace WCAG conformance checks.
- Remeasure affected pairs after changes, including supported themes and states. Unresolved or uninspected checks remain `Not verified`.
- Treat harmony, ramp spacing, tinted neutrals, and gradient interpolation as design choices, not automatic findings.

## Choose the Task

Establish scope, existing roles, brand constraints, supported appearances, and the contrast standard. Ask only when missing information would materially change the solution; otherwise state assumptions.

| Task | Procedure |
| --- | --- |
| New system or authorized redesign | Assign roles, choose needed hue families, generate consumed steps, map semantic tokens, then validate them in real components. Use harmony geometry only when useful to the brief. |
| Local correction | Trace the failing role and its consumers. Preserve unrelated hues and tokens, change the smallest appropriate mapping or value, then recheck affected states and themes. |
| Audit | Inventory scoped tokens, literals, aliases, and theme mappings, including relevant SVGs, charts, and email templates. Measure pairs and report root causes without editing. |

## Load Only Relevant References

| Need | Reference |
| --- | --- |
| Choose supporting hue families | [Palette relationships](./references/palette-relationships.md) |
| Decide families, roles, and step counts | [Palette structure](./references/palette-structure.md) |
| Generate ramps or implement theme switching | [Palette generation](./references/palette-generation.md) |
| Name roles or integrate Tailwind tokens | [Token naming](./references/token-naming.md) |
| Convert notation, resolve derived colors, or handle gamut | [Color formats and gamut](./references/color-formats.md) |
| Apply hierarchy, gradients, appearances, or audit checks | [Color usage](./references/color-usage.md) |
| Measure pairs, fix contrast, or evaluate focus | [Contrast](./references/contrast.md) |
| Choose categorical, sequential, or diverging colors | [Data visualization](./references/data-visualization.md) |

## Required Output

Scale output to the task. A local fix does not require a complete palette or token migration.

- **Creation or correction:** show added or changed values, their roles and mappings, and affected-pair verification. Include the consumed ramp only for system-level work.
- **Audit:** report one finding per root cause, with affected locations, user impact, measured evidence, and a proposed correction. Keep optional design suggestions separate.
- **Both:** identify tools/versions, standard, themes, states, and coverage limits. Distinguish declared-value calculations from rendered verification; use the evidence fields in [contrast](./references/contrast.md#measurement-contract).

### Audit Severity

| Severity | User impact |
| --- | --- |
| HIGH | Required text, focus, control, or meaningful-graphic contrast fails; meaning depends on hue alone; or a semantic mismatch misleads users about a consequential action or state. |
| MEDIUM | A theme, role mapping, or gamut defect disrupts use without the impact above. |
| LOW | A verified, isolated inconsistency with limited impact. Stylistic alternatives are suggestions, not defects. |

Order findings by severity. Link affected locations; group repeated occurrences instead of duplicating explanations.

| Severity | Location(s) | Evidence and impact | Proposed correction |
| --- | --- | --- | --- |

Corrections remain proposals unless implemented and verified. With no findings, state "No actionable color findings" and report coverage.

### Audit Outcome

Choose the first applicable outcome:

1. **Block:** a verified required accessibility failure or HIGH-impact defect remains. List missing checks too.
2. **Incomplete:** no blocking defect was verified, but a required in-scope check could not run.
3. **Verified within scope:** all required in-scope checks completed without a blocking defect. Disclose remaining non-blocking findings.

These are color-review outcomes, not certification of full WCAG conformance. For future revisions, use the [maintenance checks](./references/color-usage.md#maintenance-checks).
