# Palette Structure

Decide which hue families and roles exist before generating values. Use [palette generation](./palette-generation.md) to compute them and [token naming](./token-naming.md) to expose them.

## What a System Needs

Most interfaces can start with these families:

| Ramp | Default count | Purpose |
| --- | --- | --- |
| Neutral | 1 | Large surfaces, borders, and body text |
| Accent | 1 | Brand emphasis, interaction, and selected states |
| Status | Only those rendered | Danger, warning, success, or information |

These are defaults, not a ceiling. A second accent needs a distinguishable role, such as a chart category or a separate product identity, rather than simply adding variety.

## Every Step Has a Job

Generate steps for consumed roles, not an arbitrary number of swatches.

| Role | Tailwind light-mode starting point | Radix role |
| --- | --- | --- |
| Page background | `50` | `1` |
| Subtle background | `50` | `2` |
| Component background | `100` | `3` |
| Component hover | `200` | `4` |
| Component active or selected | `200` | `5` |
| Subtle border or separator | `200` | `6` |
| Border or focus ring | `300` | `7` |
| Hovered border | `400` | `8` |
| Solid fill | `600` | `9` |
| Solid fill hover | `700` | `10` |
| Low-contrast text | `700` | `11` |
| High-contrast text | `900` | `12` |

The conventions differ in meaning:

- **Radix** defines its 12 steps by role. Step `9` remains the solid-fill role in separate light and dark scales.
- **Tailwind** defines its 11 steps, `50` through `950`, by lightness. Semantic mappings therefore change by theme.

Match the project's convention. For a new role-based system, Radix's model is a useful starting point. On Tailwind, keep its scale and put theme changes in the semantic tier.

The table does not certify contrast. White on Tailwind's `#3b82f6` blue `500` measures 3.68:1; white on blue `600`, `#2563eb`, measures 5.17:1. Other hues and versions need their own measurements.

Repeated steps mean those roles currently share a value. If hover and subtle border need different treatment, add a distinct value without conflating their semantic tokens.

## Neutrals

A pure gray ramp is a valid default. Tinting neutrals is a stylistic option, not a correction.

For a subtle tint, OKLCH chroma around `0.005` to `0.02` is a starting range, subject to lightness and gamut. Warm neutrals can support an editorial tone; cool ones can support a technical tone. Keep the chosen direction coherent unless the design intentionally contrasts surfaces.

Neutrals often need more steps than accents because they serve more roles. Let actual surface, border, and text needs determine the count.

## Status Colors

Red for danger, amber for warning, and green for success are common conventions, but product and locale expectations take precedence. See [color usage](./color-usage.md#color-across-cultures).

- Consider hue separation from the accent when statuses could be mistaken for primary emphasis. About `15` degrees of OKLCH hue is a starting heuristic, not proof of distinguishability.
- Do not move an established semantic hue solely to satisfy that heuristic. If brand and danger share red, use labels, icons, shape, or a distinct destructive treatment.
- Most status families need only a subtle background, border, solid fill, and text. Build more steps only when components consume them.

## Auditing an Existing Palette

Inventory before proposing a restructure:

1. Collect literals and tokens within scope: hex, `rgb()`, `hsl()`, `oklch()`, other project formats, and utility classes. Include relevant SVG `fill` and `stroke`, charts, and email templates.
2. Group by hue family and sort by perceived lightness. Resolve aliases and computed values where tooling allows.
3. Identify near-duplicates. An OKLab delta E below about `0.02` is a candidate for inspection, not automatic permission to merge roles, fixed brand colors, or state distinctions.
4. Assign each value its actual roles. A value with no clear role may be unused, a missing token, or an intentional exception; establish which before reporting it.
5. Report family and value counts, duplication, role gaps, and a proposed mapping before consolidation. More families than the defaults warrant questions, not an automatic failure.

When consolidation is authorized, prefer an existing suitable value over averaging neighbors into a new one. Usage count is evidence, not a reason to discard a required semantic distinction.
