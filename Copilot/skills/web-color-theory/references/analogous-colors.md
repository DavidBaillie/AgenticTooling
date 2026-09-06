# Analogous Colors

## Definition

Analogous colors sit next to one another on the color wheel. A practical palette uses a base hue and nearby hues, often within roughly 15 to 45 degrees on either side. The exact range depends on the color wheel and the desired level of variation.

Analogous describes a hue relationship. Lighter and darker versions of one hue are monochromatic, not automatically analogous.

## Formula

For a base hue `H` in HSL or another hue-based model, candidate hues can be selected near:

- `H - 30`
- `H`
- `H + 30`

Wrap values below 0 or above 360 back into the 0 to 360 range.

## Character

- Harmonious, cohesive, and easy to keep visually consistent.
- Often feels calm, natural, or atmospheric.
- Works well when the interface needs unity more than visual tension.
- Can become muddy or monotonous when every color has the same lightness and saturation.

## Web Use

1. Choose one dominant base hue.
2. Use one neighboring hue for support and another for a restrained accent.
3. Vary lightness and chroma to separate surfaces, text, borders, and actions.
4. Use neutrals for long-form text and large surfaces when the palette becomes tiring.
5. Check that neighboring hues do not collapse into one another at small sizes or low contrast.

## Example Brief

> Create a five-color analogous palette centered on teal, spanning nearby blue-green and green hues. Keep the palette moderately muted, include light and dark surface roles, and reserve the darkest neutral for readable text.

## Common Mistake

Do not treat adjacency as an accessibility guarantee. Similar hues can have poor contrast, especially for text, icons, borders, and status indicators. Test every foreground and background pairing in the rendered interface.
