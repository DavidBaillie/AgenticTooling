# Color Formats and Gamut

Perceptual computation does not require changing source notation. Match the project's format and browser matrix.

## Choose a Notation

| Notation | Useful for | Limitation |
| --- | --- | --- |
| Hex | Compact, widely supported values and design-tool exchange | Channels do not express perceptual adjustments |
| `rgb()` | Familiar channels and explicit alpha | Channels do not correspond to perceptual lightness or hue |
| `hsl()` | Hue-based authoring | Equal lightness steps are not perceptually equal |
| `oklch()` | Perceptual lightness, hue, and chroma adjustments | Older browser matrices may need fallbacks |

For a new system, OKLCH is a useful default when supported. Consistent hex remains valid.

In `oklch(L C H / alpha)`, `L` is lightness from `0` to `1` or a percentage, `C` is chroma, and `H` is a hue angle in degrees. Chroma has no single safe maximum: available gamut depends on hue and lightness.

## Convert Only When in Scope

Convert when requested or when an approved migration standardizes the system. Change color values without altering unrelated behavior:

- Preserve keywords such as `currentColor`, `inherit`, `transparent`, `initial`, and `unset`.
- Convert gradient stops without changing the interpolation space.
- Respect formats required by third-party configurations.
- Preserve comments and formatting.
- Use tooling and check rounding, alpha, and gamut; do not estimate equivalents.

Conversions can introduce rounding differences. Check the final serialized values, not just high-precision intermediates.

## Gamut and Fallbacks

Display P3 contains colors outside sRGB. Notation does not establish gamut: an `oklch()` value can be inside or outside either target.

Inspect candidates before serialization or clamping. Mapping can flatten distinctions, so inspect role separation afterward as well as contrast.

For an sRGB baseline:

- Generate candidates in the chosen space and verify sRGB renderability.
- Reduce chroma first when preserving lightness and hue matters; document another mapping when needed.
- Supply a chosen sRGB baseline before intentional wider-gamut enhancements.
- Distinguish syntax support (`@supports`) from display gamut (`color-gamut`). One does not establish the other.
- Remeasure the final baseline and enhancement independently.

```css
:root {
  --green-9: #11ad32;
  --color-success-solid: var(--green-9);
}

@supports (color: color(display-p3 0 1 0)) {
  @media (color-gamut: p3) {
    :root {
      --green-9: color(display-p3 0.1 0.7 0.2);
    }
  }
}
```

The second value intentionally changes the color; it is not an equivalent conversion or a certified text pairing. A documented display-restricted product may use another baseline.

For unsupported syntax, guard modern custom-property assignments with `@supports`; an unrecognized color stored in a custom property can fail only when consumed, so repeated assignments alone are not a reliable fallback. Add compatibility layers only when the target matrix needs them.

## Derived CSS Colors

- **`color-mix()`** can derive a tint, for example `color-mix(in oklab, var(--color-accent-solid) 15%, white)`. Use it when project tooling supports the result; prefer resolved primitive values when export or inspection requires them.
- **Relative color syntax**, such as `oklch(from var(--color-accent-solid) calc(l - 0.1) c h)`, adjusts an existing color. Keep derivations shallow and check gamut after channel changes.
- **`light-dark()`** selects values through `color-scheme`. See [theme switching](./palette-generation.md#theme-switching).

Resolve these values with suitable tooling or measure their rendered output before reporting contrast. Inspect the project's support matrix rather than assuming all modern color functions have identical support.

Sources: [CSS Color 4](https://www.w3.org/TR/css-color-4/), [CSS Color 5 derived colors](https://www.w3.org/TR/css-color-5/), and [MDN color-gamut](https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/At-rules/@media/color-gamut). CSS Color 5 is evolving; check feature-specific support rather than treating the whole draft as universally available.
