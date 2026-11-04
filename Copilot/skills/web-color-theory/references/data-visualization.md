# Data Visualization

Choose the palette from the data's meaning before selecting hues. Interface status ramps and decorative harmony schemes are not automatically suitable chart palettes.

## Match the Encoding

| Data | Palette | Rules |
| --- | --- | --- |
| Unordered categories | Categorical / qualitative | Use distinguishable families without implying magnitude. Keep category mappings stable across related views. |
| Ordered or increasing magnitude | Sequential | Use a perceptually ordered progression, usually with monotonic lightness. Make the direction and range explicit. |
| Values around a meaningful reference | Diverging | Use two ordered arms around a labeled midpoint such as zero or a target. Preserve comparable visual weight for comparable deviations. |

- Do not use a diverging palette when the data has no meaningful midpoint.
- Avoid rainbow defaults for ordered data; uneven perceptual changes can create false boundaries or hide trends.
- Do not recycle category colors when doing so makes distinct categories appear identical. Reduce simultaneous categories, label directly, or use small multiples when the palette is exhausted.
- Give missing data a distinct treatment; do not make it look like zero, an average, or an unselected category.

## Verify the Chart, Not Just the Legend

- Check marks at their actual size and density, including overlapping areas, opacity, selection, and muted states.
- Verify required graphical boundaries or marks against adjacent colors under [non-text contrast](./contrast.md#wcag-thresholds-and-focus). Not every decorative gridline needs 3:1.
- Supply visible non-hue cues where needed: direct labels, marker shapes, line styles, patterns, or boundaries. A legend of otherwise indistinguishable swatches is insufficient.
- Check supported themes and forced colors. SVG and canvas rendering may need different treatments; do not assume one renderer's result establishes another's.
- Use color-vision simulations and grayscale inspection to expose problems, not to certify accessibility or replace contrast checks.
- Retain accessible names, descriptions, or data alternatives as appropriate; visible non-hue cues do not replace programmatic access.

Sources: [ColorBrewer palette guidance](https://colorbrewer2.org/), [W3C use of color](https://www.w3.org/WAI/WCAG22/Understanding/use-of-color.html), and [W3C non-text contrast](https://www.w3.org/WAI/WCAG22/Understanding/non-text-contrast.html).
