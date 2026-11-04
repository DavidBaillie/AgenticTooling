# Contrast

## WCAG Thresholds and Focus

Apply the project's required conformance level. These are WCAG 2.2 requirements, not interchangeable with APCA scores.

| Content | AA | AAA |
| --- | --- | --- |
| Normal text | 4.5:1 | 7:1 |
| Large text: at least `18pt` regular (`24` CSS px) or `14pt` bold | 3:1 | 4.5:1 |
| Visual information required to identify controls, states, or meaningful graphics | 3:1 against adjacent colors | No separate enhanced ratio |

Use computed font size and weight. `14pt` equals `18 2/3` CSS px; do not round a smaller font into the large-text category.

- Apply non-text contrast to the information needed for identification, not every decorative border or gridline.
- A hover, selected, or focused state must retain required contrast against adjacent colors. WCAG 1.4.11 does not generally require 3:1 between the default and changed states.
- Inactive controls, logotypes, and incidental text have relevant exemptions. Identify the applicable exception rather than inventing a required pass. Placeholder text is not a blanket exemption.
- Brand guidelines do not exempt ordinary body or control text.

### Focus

- **AA:** keyboard focus must be visible (2.4.7). Author-styled focus information is subject to applicable non-text contrast (1.4.11). Focus must not be entirely hidden by author-created content (2.4.11).
- **AAA:** Focus Appearance (2.4.13) additionally requires an indicator area at least equivalent to a `2` CSS px perimeter and at least 3:1 change between the same pixels in focused and unfocused states, subject to its exceptions.
- Test indicators on the surfaces they actually touch. A two-color outline can help across variable backgrounds; a shadow-only ring may disappear in forced colors.
- A color inspection alone does not establish keyboard operability or complete focus compliance.

## Measurement Contract

1. Resolve the foreground and the surface it actually renders on, including inheritance and theme aliases. The page background is not automatically the correct background.
2. Resolve alpha, element opacity, `color-mix()`, relative colors, overlays, and gamut mapping. For images or gradients, inspect the worst region behind the foreground, not an average or just the endpoints.
3. Use foreground/background colors obtained from computed styles or resolved source values. Do not sample antialiased glyph-edge pixels to calculate WCAG conformance; inspect thin text separately for usability.
4. Compare the unrounded result with the applicable threshold. `4.499:1` fails `4.5:1`, even if a rounded display says `4.50`. Round only for reporting.
5. After a change, remeasure the final serialized values and affected consumers, including relevant themes, states, fallbacks, and wider-gamut enhancements.

Record the standard/criterion and tool/version once per report unless they differ by pair. Each pair needs:

| Role / location | Theme / state | Resolved foreground / background | Metric / result | Threshold / exception | Verification |
| --- | --- | --- | --- | --- | --- |

Use `Pass`, `Fail`, `Exempt` with a reason, or `Not verified`. Identify declared-value versus rendered checks and any display-gamut assumptions. Never present an unresolved token or a guessed background as a measurement.

Without a browser, resolved solid pairs can still be computed. Layout, actual background selection, image content, uninspected states, and uncertain compositing remain `Not verified`. For variable surfaces, inspect their possible range or provide a stable surface that guarantees the pairing.

## Correct a Failing Pair

Within authorized scope:

1. Try a lightness adjustment while preserving intended hue and role. OKLCH `L` is a perceptual coordinate, not WCAG relative luminance or a contrast ratio.
2. Compare candidate foregrounds against the actual background. If needed, change the surface or treatment; a darker step or white label is not automatically safe.
3. Check gamut, serialize to the project's notation, then repeat the measurement contract.

For opaque sRGB examples, white on `#3b82f6` measures about 3.68:1; white on `#2563eb` about 5.17:1. Only the latter meets AA for normal text. These are results for those literals, not guarantees about a framework's `500` or `600` token.

## Optional APCA Evaluation

- Use APCA for supplementary text evaluation when requested or established by the project. It is not a replacement for WCAG 2.2 and must not be presented as a finalized WCAG 3 requirement.
- Record the implementation/version, signed Lc result, target source, and applicable font-size/weight guidance. Positive Lc denotes dark-on-light; negative denotes light-on-dark.
- Do not apply a blanket Lc target to every label or transfer text targets to controls and borders. Keep project-specific non-text heuristics separate from conformance checks.
- If required APCA tooling is unavailable, report `Not verified`, not an estimate.

## Sources

- [WCAG 2.2 normative requirements](https://www.w3.org/TR/WCAG22/)
- [Text contrast, exceptions, and precision](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html)
- [Non-text contrast and state comparisons](https://www.w3.org/WAI/WCAG22/Understanding/non-text-contrast.html)
- [Focus Appearance, Level AAA](https://www.w3.org/WAI/WCAG22/Understanding/focus-appearance.html)
- [APCA guidance](https://git.apcacontrast.com/documentation/APCA_in_a_Nutshell)
