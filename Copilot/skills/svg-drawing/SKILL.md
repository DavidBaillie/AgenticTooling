---
name: svg-drawing
description: 'Manually recreate simple raster logos and icons as standalone SVG. Use when asked to redraw, vectorize, or convert a PNG or raster reference into editable vector shapes, sample colors, and verify geometry and visual fidelity without automatic tracing.'
---

# SVG Drawing from Raster References

Recreate simple, flat-color artwork with the fewest vector shapes that meet
the requested fidelity. Use the source and current request as requirements,
not the geometry or palette from a previous example.

Keep the redraw manual and local; do not embed the raster or upload it to an
external conversion service. Detailed photographs, textures, and incompatible
fidelity/shape constraints require clarification rather than silent simplification.

## When to Use

- Recreate or simplify a geometric raster logo or icon.
- Rebuild a mark with an explicit shape count or arrangement.
- Simplify raster artifacts while preserving the original visual identity.

## Capabilities and Fallbacks

Discover available tools first. The names below are examples, not required
interfaces. Reuse installed tools; do not add dependencies just to run a recipe.

| Capability | Example tools |
| --- | --- |
| Inspect files and references | `glob`, `view`, `rg`; the client's image viewer. |
| Read dimensions, pixels, and alpha | PowerShell with `System.Drawing` on Windows; an existing local image inspector elsewhere. |
| Save the standalone SVG | `apply_patch` or the client's file editor. |
| Check XML and geometry | Python's standard-library `xml.etree.ElementTree`; existing XML tooling. |
| Render and compare | `openBrowserPage`, `screenshotPage`; an existing local preview or renderer. |
| Inspect browser geometry | `runPlaywrightCode` with `page.evaluate`; equivalent browser evaluation. |
| Track existing changes | `git --no-pager status --short`; source hashes or file comparison. |

If the source cannot be inspected, request an accessible image rather than
inventing its appearance. Disclose estimated colors when exact sampling is
unavailable. If no local rendering option is available, report
`rendering unverified`; do not claim visual fidelity from XML checks alone.

## Workflow

1. **Inspect the source and define success.**
   - Open the image; inspect orientation, layering, lettering, and alpha.
   - Record source/output paths, existing SVG conventions, and the source's
     pre-run state. Preserve unrelated work and pre-existing changes.
   - Establish faithful reconstruction versus recognizable approximation,
     allowed simplification, shape budget/types, color policy, canvas/cropping,
     background/transparency, and intended display sizes.
   - Unless requested otherwise, preserve the full canvas, transparency,
     distinct colors, and meaningful contours. Do not normalize colors or
     simplify letterforms without permission when fidelity matters.
   - Ask only about unresolved choices that materially affect the result.
     Flag constraints that cannot preserve the mark; do not silently trade
     fidelity for a shape limit.

2. **Measure landmarks and sample colors.**
   - Read raster dimensions and map them to the intended SVG coordinate system.
   - Locate subject bounds, vertices, alignment axes, curve extrema, gaps,
     letter counters, and other negative spaces before choosing coordinates.
   - Preserve deliberate symmetry and proportions, not antialiasing artifacts.
   - Prefer a supplied approved palette. Otherwise sample multiple opaque
     interior pixels, away from antialiased edges and compression artifacts.
   - Preserve distinct shades and alpha unless simplification is permitted.
     If pixels are translucent, do not infer their visible color without
     considering the comparison background.

3. **Draw geometry that meets the contract.**
   - Choose primitives and curves from the source, not a previous example.
     Use triangles or a fixed number of shapes only when appropriate.
   - Preserve paint order and negative space. Outline lettering when required;
     use compound paths and an appropriate fill rule to retain counters.
   - Set a finite `viewBox` with positive dimensions; retain the measured
     canvas unless cropping was requested. Preserve aspect ratio when scaling.
   - Keep transparency or an intentional background as specified. Do not
     embed bitmap images or depend on external assets.
   - Include a descriptive title for a standalone informative SVG. When
     integrating it, verify inline SVG naming or provide the host `<img alt>`
     as appropriate; handle decorative usage separately.
   - Save at the requested destination, or beside the source when unspecified.
     Do not overwrite unrelated work.

4. **Validate the saved structure against the current requirements.**
   - Parse the actual file; check the SVG namespace and numeric `viewBox`.
     Reject non-finite coordinates, invalid dimensions, and raster embedding.
   - Verify required shape types, fills, counts, and geometric constraints.
     Define what a fixed shape budget counts; groups and metadata are not
     painted primitives.
   - Use scale-relative tolerances for floating-point geometry. Reject
     zero-length edges and degenerate areas before accepting approximate
     right angles or alignment.
   - Require exact element structure only when the request requires it.
     Preserve meaningful paint order without imposing arbitrary metadata order.
   - Check bounds and scaling with an available renderer. XML validity does
     not establish valid path geometry, unclipped artwork, or visual fidelity.

5. **Compare, revise, and report.**
   - Render the saved SVG locally. Use an absolute `file:` URL when supported;
     otherwise use an existing local preview or renderer.
   - Align source/output canvases, cropping, scale, and comparison backgrounds.
     Compare side-by-side or with an overlay: silhouette, landmarks, spacing,
     counters, layering, and colors.
   - Inspect the smallest intended display size, normal use, and an enlarged
     view. If target sizes are unknown, choose representative sizes and report
     them. For transparent output, also preview on light and dark backgrounds
     or a checkerboard without adding those backgrounds to the artifact.
   - Inspect clipping, stroke extents, nonzero bounds, and aspect ratio.
     Revise mismatches, then rerun structure checks and visual comparison.
   - Confirm the source matches its pre-run state. Report the output path,
     intentional simplifications, checks performed, tested sizes/backgrounds,
     remaining differences, and unavailable verification. Never inherit a
     previous example's claimed pass.

## Gotchas

- **White is not automatically transparent.** Inspect alpha before removing
  or inventing a background.
- **Element count is not always painted-shape count.** Definitions, reuse,
  masks, hidden elements, inherited styles, and transforms need renderer-aware
  inspection; do not generalize a simple descendant count to arbitrary SVG.
- **`getBBox()` is not a clipping proof.** Local geometry bounds alone do not
  account for all transforms, strokes, filters, or viewport clipping.
