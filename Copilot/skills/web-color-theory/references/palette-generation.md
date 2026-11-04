# Palette Generation

Generate values after deciding [palette structure](./palette-structure.md). Compute in a perceptual space and emit the project's notation.

## Start from the Brand Color

Determine the brand color's intended role before building a ramp.

- If it supports the intended label contrast, it may occupy the solid-fill step, often Tailwind `600` or Radix `9`.
- Pin a contractually fixed value exactly and build around it. Uneven spacing is preferable to silently changing the brand.
- Snap a flexible seed to a regular ramp only when adjustment is allowed.
- If the seed fails the intended text pairing, retain it for another role and select a suitable fill or label treatment.

For example, `#3b82f6` is Tailwind blue `500`, not a safe fill for normal white text: the pair measures 3.68:1. White on the existing blue `600`, `#2563eb`, measures 5.17:1. Do not generalize that result to every `600`.

## Useful Ramp Properties

These are starting criteria, not conformance thresholds:

- **Lightness moves steadily.** Measure OKLCH `L`, from `0` to `1`, rather than treating HSL lightness as perceptual.
- **Hue stays coherent within a ramp.** Start with a constant hue. Deliberate hue shifts may suit an established scale; inspect them rather than rejecting them automatically.
- **Chroma usually peaks near the middle.** Pale and dark extremes typically need less chroma to remain usable and in gamut.
- **Light-end steps are often finer.** As a starting point, `50` to `200` might differ by `0.04` to `0.05` in `L`, compared with `0.07` to `0.10` near the middle. Roles and measured pairs decide the final spacing.
- **Adjacent steps need a reason to differ.** Steps less than about `0.03` apart in `L` deserve inspection, but small surface distinctions may be intentional.
- **Tinted endpoints often stop short of black and white.** Pure black or white remain valid when the design or contrast requirements call for them.

## Use Color Tooling

Use an available color library or verified tool rather than inventing values by eye. `culori`, `colorjs.io`, and `chroma.js` support conversion and perceptual interpolation. Do not add a dependency merely because this reference names it.

With `culori` already available:

```js
import { formatHex, interpolate, samples } from 'culori'

const ramp = interpolate(['#eff6ff', '#3b82f6', '#172554'], 'oklab')
const steps = samples(11).map((t) => formatHex(ramp(t)))
```

This is a candidate ramp, not a validated one. Linear sampling does not enforce constant OKLCH hue, role-specific spacing, or gamut limits. Inspect the result, adjust samples, and regenerate before using it.

For comparison, a familiar Tailwind blue scale is:

```css
:root {
  --blue-50: #eff6ff;
  --blue-100: #dbeafe;
  --blue-200: #bfdbfe;
  --blue-300: #93c5fd;
  --blue-400: #60a5fa;
  --blue-500: #3b82f6;
  --blue-600: #2563eb;
  --blue-700: #1d4ed8;
  --blue-800: #1e40af;
  --blue-900: #1e3a8a;
  --blue-950: #172554;
}
```

Keep only the steps the project needs. A reference scale is not a requirement to generate every step.

Ramp interpolation should support usable perceptual steps. Decorative [gradients](./color-usage.md#gradients) are a different case: their interpolation space is a deliberate visual choice.

## Several Hues at Once

Compare ramps at equivalent roles so status and accent treatments have intentional visual weight.

- Similar perceived lightness is a useful starting point across hue families, not a reason to sacrifice required contrast.
- Match vividness relative to each hue's available gamut, not by copying one chroma value everywhere.
- Maximum available chroma varies with both hue and lightness. Recheck each step in the target gamut rather than assuming a shared saturation or chroma is safe.

## Dark Mode

A reversed light ramp is a starting point, not a finished dark theme. Repoint semantic roles first:

```css
:root {
  --color-bg-page: var(--neutral-50);
  --color-text-primary: var(--neutral-950);
}

.dark {
  --color-bg-page: var(--neutral-950);
  --color-text-primary: var(--neutral-50);
}
```

Using the project's existing neutral primitives, inspect:

- **Chroma:** an accent may look excessively vivid on a near-black surface. Reduce it when needed, not automatically.
- **Surface separation:** dark surface roles may need wider spacing than a mirrored light scale provides.
- **Contrast:** remeasure every affected pair. Passing in one appearance does not establish a pass in another.

### Choose One Coherent Switching Strategy

- Use `prefers-color-scheme` alone when the interface follows the system and has no override.
- Use the established class or attribute strategy when users can choose an appearance. System preference may initialize the choice, but must not fight the explicit override.
- Use `light-dark()` when supported and appropriate. It follows the element's `color-scheme`, not a class name; a manual toggle must update that property too.

```css
:root {
  color-scheme: light dark;
  --color-bg-page: light-dark(#ffffff, #172554);
}
```

Do not let independent media-query and class-based token sets produce a half-themed interface. Check system, explicit light, explicit dark, and any increased-contrast variants that the product supports.
