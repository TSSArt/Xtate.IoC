---
applyTo: "src/Xtate.IoC/ImplementationEntry/**/*.cs"
---

# Implementation entry instructions

## Purpose

Lifetime-specific runtime entries (`Transient`, `Scoped`, `Singleton`, `Forwarding`) and ownership/disposal behavior.

## Follow existing patterns

- Keep ownership semantics strict: `*OwnerImplementationEntry` variants track disposable objects in bins; external/forwarding variants do not claim ownership.
- Preserve chain behavior and cloning contracts via `CreateNew(...)` overloads.
- Use fast-path + await fallback flow (`IsCompletedSuccessfully` checks) for `ValueTask`-based execution.

## Implementation rules

- Keep disposal/state checks centralized through existing provider tokens and object bins.
- Preserve async initialization invocation semantics before returning resolved instances.

## Testing rules

- Reflect lifetime/ownership changes in `ImplementationEntryTest`, `ObjectsBinTest`, and disposal-related tests.

## Avoid

- Do not mix owner and external disposal behavior.
- Do not change chain ordering semantics unless corresponding enumeration/decorator behavior is intentionally updated.
