---
applyTo: "test/Xtate.IoC.Test/Examples/**/*.cs"
---

# Examples instructions

## Purpose

Executable documentation examples that show intended public API usage patterns.

## Follow existing patterns

- Keep examples minimal and readable: one feature focus per example class.
- Preserve `Xtate.IoC.Examples` namespace and narrative comments that explain the scenario.
- Use real assertions so examples remain executable tests, not pseudo-code.

## Implementation rules

- Prefer public API calls from `Container`/`IServiceProvider` extensions rather than internal helpers.
- Keep registration snippets representative of recommended usage (modules, decorators, arg-aware resolution, scopes).

## Testing rules

- Ensure each example still runs under MSTest and demonstrates a verifiable outcome.

## Avoid

- Do not add implementation-only details that distract from API usage guidance.
