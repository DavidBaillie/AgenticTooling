# Palette Structure

Decide families and consumed roles before [generating values](./palette-generation.md). A hue-family count is not a ramp-step count.

## What a System Needs

Most interfaces can start with these families:

| Ramp | Default count | Purpose |
| --- | --- | --- |
| Neutral | 1 | Large surfaces, borders, and body text |
| Accent | 1 | Brand emphasis, interaction, and selected states |
| Status | Only those rendered | Danger, warning, success, or information |

These are starting points, not limits. Additional families need a distinguishable job, such as a separate product identity or [chart encoding](./data-visualization.md).

## Every Step Has a Job

Generate steps for consumed roles, not an arbitrary number of swatches.

| Role | Radix role-based step |
| --- | --- |
| Page / subtle background | `1` / `2` |
| Component background / hover / active | `3` / `4` / `5` |
| Subtle / default / hovered border | `6` / `7` / `8` |
| Solid fill / solid hover | `9` / `10` |
| Secondary / primary text | `11` / `12` |

Match the project's convention:

- **Radix** assigns its 12 steps to roles. Step `9` remains the solid-fill role in separate light and dark scales.
- **Tailwind** orders its 11 steps, `50` through `950`, from light to dark. Put role and theme mappings in the semantic tier; a step number alone does not determine its job.

The table is a role inventory, not a requirement to generate every step or a contrast certificate. Check actual library versions, hue families, and component pairs.

Different roles may share a value today. Keep their [semantic mappings](./token-naming.md) independently adjustable when their jobs differ.

## Neutrals

A pure gray ramp is valid; tinting is optional. For tinted neutrals, use restrained chroma and inspect the light and dark extremes in gamut. Let actual surface, border, and text needs determine the number of steps; neutrals often serve more roles than accents.

## Status Colors

Red for danger, amber for warning, and green for success are common conventions, but product and [locale expectations](./color-usage.md#meaning-and-hierarchy) take precedence.

- If feedback resembles the accent, inspect semantic clarity in actual components. Do not shift an established hue merely to satisfy geometric separation.
- Brand and danger may share red when labels, icons, shape, or a distinct destructive treatment communicate their jobs.
- A status family may need a subtle background, border, solid fill, and text. Build only the treatments consumed by shipped components.

## Auditing an Existing Palette

Inventory before proposing a restructure:

1. Collect literals and tokens within scope: hex, `rgb()`, `hsl()`, `oklch()`, other project formats, and utility classes. Include relevant SVG `fill` and `stroke`, charts, and email templates.
2. Group by hue family and sort by perceived lightness. Resolve aliases and computed values where tooling allows.
3. Inspect near-duplicates with suitable perceptual tooling. Record its metric and scale; proximity alone does not authorize merging roles, fixed brand colors, or state distinctions.
4. Assign each value its actual roles. A value with no clear role may be unused, a missing token, or an intentional exception; establish which before reporting it.
5. Report family and value counts, duplication, role gaps, and a proposed mapping before consolidation. More families than the defaults warrant questions, not an automatic failure.

When consolidation is authorized, prefer an existing suitable value over averaging neighbors into a new one. Usage count is evidence, not a reason to discard a required semantic distinction.

Sources: [Radix scale roles](https://www.radix-ui.com/colors/docs/palette-composition/understanding-the-scale) and [Tailwind colors](https://tailwindcss.com/docs/colors).
