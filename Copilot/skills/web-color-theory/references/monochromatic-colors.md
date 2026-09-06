# Monochromatic Colors

## Definition

A monochromatic palette uses one hue family and varies lightness, saturation, or chroma. It may include tints, shades, and tones of the same base hue.

Unlike analogous palettes, monochromatic palettes do not depend on multiple neighboring hues.

## Formula

For a base hue `H`, keep the hue stable and vary the other dimensions:

- Light tint: increase lightness and reduce chroma or saturation.
- Mid tone: keep a moderate lightness and chroma.
- Dark shade: reduce lightness while preserving enough chroma for the role.

The exact values should be tested visually. Equal numeric steps do not always appear equally spaced, especially in HSL.

## Character

- Cohesive, quiet, and easy to brand.
- Useful for focused products, editorial layouts, and interfaces where content should lead.
- Can lack hierarchy or state distinction when every role differs only slightly.

## Web Use

1. Choose a base hue that supports the brand and content.
2. Create distinct lightness steps for background, surface, border, text, and action roles.
3. Use chroma changes to separate interactive elements from passive surfaces.
4. Add neutrals or a small semantic exception when success, warning, error, or focus states need clearer meaning.
5. Check that borders, disabled controls, and focus indicators remain visible without relying on saturation alone.

## Example Brief

> Create a monochromatic indigo interface with a warm near-white background, four surface levels, a readable dark text color, and a high-contrast action color. Keep hue consistent but vary lightness and chroma enough to separate states.

## Common Mistake

Do not use several nearly identical lightness values and call the result a usable scale. If users cannot distinguish surfaces, borders, controls, or states, increase the lightness or chroma separation and validate the actual contrast ratios.
