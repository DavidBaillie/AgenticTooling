---
name: web-general-design
description: 'Use when designing or redesigning frontend UI, landing pages, websites, React components, or visual application screens. Focuses on distinctive, accessible, production-quality interfaces.'
---

# Web Design

Create frontend interfaces with a clear visual point of view, working code, and restrained implementation complexity.

## When to Use

Use this skill when the task involves:

- Designing a new page, component, or app screen
- Improving visual quality, layout, polish, or interaction design
- Creating a landing page, marketing site, dashboard, or product UI
- Choosing typography, color, motion, spacing, or visual hierarchy

Do not use this skill for purely functional frontend bug fixes unless visual design is part of the request.

## Core Workflow

1. Identify the user, purpose, and primary action.
2. Choose one clear aesthetic direction.
3. Implement the smallest working interface that expresses that direction.
4. Refine typography, spacing, color, states, and responsiveness.
5. Verify the result on desktop and mobile.

## Design Direction

Pick a specific direction before coding. Use references as a loose vocabulary, not a template.

| Direction | Useful References |
|-----------|-------------------|
| Brutally minimal | Swiss posters, Dieter Rams products, monochrome museum labels |
| Editorial | Magazine openers, art books, newspaper feature layouts |
| Industrial | Factory signage, technical manuals, machined metal controls |
| Refined luxury | Watch catalogs, gallery invitations, premium hospitality sites |
| Playful | Toy packaging, game menus, colorful learning apps |
| Organic | Botanical diagrams, natural materials, garden maps |
| Retro-futuristic | Space-age interiors, analog synth panels, 1970s sci-fi interfaces |
| Geometric | Bauhaus posters, modular grids, architectural wayfinding |
| Quiet utility | Trading desks, airline operations tools, medical dashboards |

Use these as starting points, not templates. The design should fit the product, audience, and task.

## Implementation Rules

- Prefer working UI over decorative mockups.
- Use distinctive typography; avoid default stacks like Arial, Roboto, Inter, or system fonts unless the existing project requires them.
- Define colors with CSS variables.
- Avoid default purple gradients and generic SaaS layouts.
- Use motion only where it improves orientation, feedback, or delight.
- Check installed dependencies before using animation libraries.
- Keep complexity proportional to the visual idea.
- Preserve accessibility: contrast, focus states, keyboard use, readable text, and reduced-motion support.
- Make responsive behavior deliberate, not incidental.

## Visual Quality Checklist

Before finishing, check:

- The first screen has a strong visual hierarchy.
- The primary action is obvious.
- Text does not overflow or overlap.
- Components have complete hover, focus, active, empty, and loading states where relevant.
- The design feels specific to the requested domain.
- The implementation avoids unnecessary dependencies and abstractions.
