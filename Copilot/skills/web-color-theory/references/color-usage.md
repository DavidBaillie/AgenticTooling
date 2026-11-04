# Color Usage

## Meaning and Hierarchy

- Keep interaction, selection, and feedback meanings consistent. If an accent signals interaction, using it indiscriminately for decoration can weaken that signal.
- Let area, lightness, and chroma establish emphasis. Low-chroma surfaces often support long-form content, but pure grays and strongly branded surfaces are valid when usable.
- When filled color encodes primary emphasis, usually reserve it for one primary action per view or task group. Preserve an established alternative hierarchy.
- Selected tabs, categories, and statuses are not necessarily competing primary actions.
- Verify locale-specific conventions. For example, Chinese financial interfaces commonly use red for gains and green for losses. Use locale-aware gain/loss roles where needed, plus visible labels or icons.

## Gradients

Interpolation space is a visual choice. Preserve an established gradient unless its change is in scope.

| Space | Useful behavior | Check |
| --- | --- | --- |
| `in srgb` | Familiar legacy interpolation | Opposing hues may produce a muted midpoint |
| `in oklab` | Useful default for a new perceptual transition | A path near the neutral axis may still become gray |
| `in oklch` | A polar hue path with potentially vivid intermediate colors | Hue direction can introduce unwanted colors or leave gamut |

Choose the hue path intentionally:

```css
background: linear-gradient(in oklch shorter hue, #3b82f6, #ec4899);
background: linear-gradient(in oklch longer hue, #3b82f6, #ec4899);
```

- Add an intermediate stop when it produces the intended transition without changing an established interpolation model.
- Check intermediate colors and gamut, not only endpoints. Verify syntax support for the project's browser matrix.
- For banding, first reconsider the stop range or area. Add restrained noise only when the treatment fits the project.
- Text over a gradient needs worst-region measurement or a guaranteed surface; see the [measurement contract](./contrast.md#measurement-contract).

## Supported Appearances

Theme switching belongs in [palette generation](./palette-generation.md#theme-switching). For each supported appearance:

- Map every consumed semantic role deliberately. Primitive values can be shared when remapping already supplies the intended result.
- Cover body text, links, controls, focus, hover, active/pressed, selected, disabled, error, and success states that the product renders.
- Adapt `prefers-contrast: more` per light/dark appearance and explicit theme override. One shared adjustment can improve one pairing while worsening another.
- Inspect on-accent text, surface separation, and meaningful state distinctions. A fixed lightness delta does not prove improvement.

### Forced Colors

`forced-colors: active` lets the user agent replace colors with a restricted system palette. It is not equivalent to `prefers-contrast: more` and should not become a separate decorative theme.

- Prefer native semantics and system colors such as `CanvasText`, `ButtonText`, and `Highlight`. Added ARIA roles do not necessarily receive native elements' forced-color styling.
- Check controls, focus, selection, and meaningful SVGs when gradients and shadows disappear. Canvas content needs separate inspection.
- Add targeted borders or outlines when shadow-only affordances vanish. Do not blanket-disable user adjustments with `forced-color-adjust: none`.
- A necessary exception, such as a color swatch, needs a narrow scope and an independent visible cue.

For a control otherwise identified only by its shadow, a targeted fallback could be:

```css
@media (forced-colors: active) {
  .shadow-only-button {
    border: 1px solid ButtonText;
  }
}
```

## Audit Checklist

Investigate actual behavior before reporting a defect. The core skill defines scope, permissions, and severity.

| Signal | Inspect |
| --- | --- |
| A literal or primitive bypasses an existing semantic layer | Whether an appropriate role exists and theme mapping is lost |
| A token serves unrelated jobs or has an ambiguous name | Whether consumers need independent evolution |
| Near-duplicate values or many unused steps | Actual consumers and role distinctions before consolidation |
| Equal HSL steps or shared chroma across hues | Perceptual spacing, required pairs, and target gamut |
| Accent and feedback hues look similar | Visible non-hue cues and semantic clarity |
| A dark ramp is merely reversed | Surface separation, accent vividness, and pair measurements |
| Theme selectors and media queries overlap | System initialization versus explicit user choice |
| Alpha, gradients, images, or derived CSS colors | Resolved values and worst-case backgrounds |
| A color leaves the target gamut | Intentional mapping, fallbacks, and preserved distinctions |
| A focus or selection cue relies on shadows | Forced-color behavior and required contrast |

## Maintenance Checks

When revising this skill, check local links/anchors, frontmatter discovery terms, and affected examples. Keep one authoritative explanation per rule; add a reference only for a distinct recurring task.

Use these regression briefs to check that revised guidance still produces the intended decisions. They are review scenarios, not claims of automated or rendered verification.

| Brief | Expected decision |
| --- | --- |
| A computed ratio is `4.499:1` against `4.5:1` | Fail before reporting rounded output |
| A translucent label sits on variable content | Resolve the range or report the rendered check as `Not verified` |
| A fixed brand fill fails normal white text | Preserve the fixed value; propose another role, label, or treatment |
| System preference is dark but the user chose light | The explicit choice controls all related tokens |
| A wide-gamut enhancement is unavailable | The chosen supported baseline remains usable and measured |
| A focus ring or selected state disappears in forced colors | Report the lost cue and propose a targeted system-color treatment |
| An audit lacks a required browser check | Report `Incomplete`, unless a verified blocking defect already requires `Block` |
| A sequential chart uses a decorative triadic palette | Inspect ordering and non-hue cues rather than accepting harmony as evidence |

For framework and color-tool examples, identify the relevant version or literal inputs and recheck when dependencies change. Check current browser support when adopting a feature; do not turn today's support status or experimental metrics into timeless rules.

Sources: [MDN forced colors](https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/At-rules/@media/forced-colors), [CSS Color 4 interpolation](https://www.w3.org/TR/css-color-4/#interpolation), and [W3C use of color](https://www.w3.org/WAI/WCAG22/Understanding/use-of-color.html).
