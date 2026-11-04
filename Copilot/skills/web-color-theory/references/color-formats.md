# Color Formats and Gamut

Match the project's notation. Perceptual generation and measurement can happen internally without changing the format written to source files.

## Choose a Notation

| Notation | Useful for | Limitation |
| --- | --- | --- |
| Hex | Compact, widely supported values and design-tool exchange | Channels do not express perceptual adjustments |
| `rgb()` | Familiar channels and explicit alpha | Channels do not correspond to perceptual lightness or hue |
| `hsl()` | Hue-based authoring | Equal lightness steps are not perceptually equal |
| `oklch()` | Perceptual lightness, hue, and chroma adjustments | Older browser matrices may need fallbacks |

A consistent hex system is better than an unrequested mixture of notations. For a genuinely new system, OKLCH is a useful default when the browser matrix supports it.

In `oklch(L C H / alpha)`, `L` is lightness from `0` to `1` or a percentage, `C` is chroma, and `H` is a hue angle in degrees. Chroma has no single safe maximum: available gamut depends on hue and lightness.

## Convert Only When in Scope

Convert when requested or when an approved migration standardizes the system. Change color values without altering unrelated behavior:

- Preserve keywords such as `currentColor`, `inherit`, `transparent`, `initial`, and `unset`.
- Convert gradient stops without changing the interpolation space.
- Respect formats required by third-party configurations.
- Preserve comments and formatting.
- Use tooling and check rounding, alpha, and gamut; do not estimate equivalents.

For example, this conversion preserves the sRGB blue within rounding precision:

```css
/* Before */
color: #3b82f6;
border: 1px solid rgba(0, 0, 0, 0.1);

/* After */
color: oklch(0.623083 0.188015 259.814527);
border: 1px solid oklch(0 0 0 / 0.1);
```

Bulk conversion is a migration. It can introduce rounding differences and touches otherwise unrelated surfaces, so do not make it incidental cleanup.

## Gamut

Display P3 contains colors outside sRGB. Maximum chroma varies by hue and lightness, so one chroma value copied across a ramp can be renderable at some steps and outside the target gamut at others.

Browser gamut mapping may reduce or flatten distinctions. Check the target gamut with tooling instead of inferring it from appearance or notation.

For an sRGB baseline:

- Generate and check values in sRGB.
- Reduce chroma first when keeping lightness and hue is important.
- Add wider-gamut values as an intentional enhancement, not an accidental dependency.
- Recheck contrast after any mapping or enhancement.

```css
:root {
  --green-9: #11ad32;
  --color-success-solid: var(--green-9);
}

@media (color-gamut: p3) {
  :root {
    --green-9: oklch(0.65 0.27 145);
  }
}
```

The second value is a deliberately more vivid enhancement, not an equivalent format conversion. Declare the sRGB baseline first so a narrower-gamut display gets a chosen value. A documented display-restricted product may use a different baseline.

Syntax support is a separate concern. If the browser matrix predates `oklch()`, layer a supported fallback:

```css
:root {
  --blue-600: #2563eb;
}

@supports (color: oklch(0 0 0)) {
  :root {
    --blue-600: oklch(0.546150 0.215208 262.880917);
  }
}
```

Do not add obsolete-browser fallbacks when the target matrix does not need them.

## Derived CSS Colors

- **`color-mix()`** can derive a tint, for example `color-mix(in oklab, var(--color-accent-solid) 15%, white)`. Use it when project tooling supports the result; prefer resolved primitive values when export or inspection requires them.
- **Relative color syntax**, such as `oklch(from var(--color-accent-solid) calc(l - 0.1) c h)`, adjusts an existing color. Keep derivations shallow and check gamut after channel changes.
- **`light-dark()`** selects values through `color-scheme`. See [theme switching](./palette-generation.md#choose-one-coherent-switching-strategy).

Resolve these values with suitable tooling or measure their rendered output before reporting contrast. Inspect the project's support matrix rather than assuming all modern color functions have identical support.
