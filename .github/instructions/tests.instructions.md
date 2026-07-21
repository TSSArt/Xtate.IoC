---
applyTo: "test/Xtate.IoC.Test/**/*.cs"
---

# Test source instructions

## Test style

- Use MSTest attributes and the assertion style already used by nearby tests.
- Keep tests focused, deterministic, independent, and safe under parallel execution.
- Keep scenario-specific helper types nested or local to the test area when practical.
- Do not rely on execution order, shared mutable state, timing, or console inspection.

## Coverage

- Cover sync and async variants when shared behavior changes.
- Cover every affected argument arity or prove the common underlying path with targeted tests.
- For lifetime changes, assert instance identity, scope boundaries, initialization, ownership, and disposal.
- For decorator or factory changes, assert chain order, arguments, missing-service behavior, and exceptions.
- Keep examples executable and focused on recommended public API usage.

## Verification

- Run the narrowest matching test on one modern framework first.
- Run broader solution tests and legacy targets when shared or compatibility-sensitive behavior changes.
