# Palette Relationships

Use harmony geometry to propose hue families, not to decide semantic meaning or certify accessibility. A local contrast correction rarely needs a new harmony scheme.

## Name the Model

State the model used for hue calculations. HSL, LCH, and OKLCH assign different angles to the same visible color; their offsets are not interchangeable. OKLCH is a useful starting point for a new perceptual palette, while an existing model may remain appropriate.

For computed candidates, normalize degrees with `((H % 360) + 360) % 360`. Hue is undefined or unstable near zero chroma; do not derive meaningful relationships from a neutral's hue.

| Relationship | Candidate hues in the chosen model | Useful application | Main risk |
| --- | --- | --- | --- |
| Monochromatic | One hue; vary lightness and chroma | A focused interface with one accent family | Similar steps fail to distinguish roles or states |
| Analogous | Base plus nearby hues, for example `H - 30`, `H`, `H + 30` | Related supporting surfaces or illustrations | Neighbors become indistinguishable |
| Complementary | `H`, `H + 180` | A dominant family with a contrasting accent | Equally prominent vivid hues compete |
| Split-complementary | `H`, `H + 150`, `H + 210` | One dominant family with two supporting accents | The accents acquire overlapping or confusing jobs |
| Triadic | `H`, `H + 120`, `H + 240` | Several genuinely distinct color roles | Geometry substitutes for hierarchy |

Offsets are illustrative within the named model, not perceptually equal spacing or guaranteed pleasant combinations. Mood associations depend on context and audience.

## Turn Candidates into a System

- Give every added family a job before generating values. Not every hue needs a full ramp.
- Establish hierarchy through area, lightness, chroma, typography, and spacing, not hue count alone.
- For a monochromatic scheme, generate distinct surface, text, border, and action roles. Permit semantic exceptions when required.
- Preserve product and locale meanings for feedback. Do not assign success or danger merely to complete a geometric scheme.
- Judge candidates in representative components, including small indicators and supported themes, rather than only as swatches.
- Use [data-visualization rules](./data-visualization.md) when hues encode categories or numeric values.

Source: [MDN hue models and normalization](https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Values/hue).
