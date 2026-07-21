---
applyTo: "src/Xtate.IoC/IoC/**/*.cs"
---

# IoC core instructions

## Purpose

Core container APIs, service registration DSL, resolution APIs, scopes, and type-key plumbing.

## Follow existing patterns

- Keep API symmetry across `ServiceCollectionArg0..4Extensions` and `ServiceProviderArg0..4Extensions`; when adding a new capability, update the matching arg-arity and sync/async variants.
- Preserve the zero-arg bridge pattern: arg0 APIs delegate to `TArg = Empty` implementations.
- Use `TypeKey.ServiceKey*` / `TypeKey.ImplementationKey*` and existing `InstanceScope`/`SharedWithin` mappings instead of introducing new key or lifetime models.

## Implementation rules

- Keep module registration idempotent (`IsModuleRegistered` + `Option.IfNotRegistered` behavior).
- Maintain existing decorator semantics (registration order controls wrapping; last registered is outermost).
- Keep `Container.Create(...)` overloads as lightweight composition entry points over `ServiceCollection`.

## Testing rules

- Mirror API changes in `test/Xtate.IoC.Test/Tests` and add or update usage coverage in `test/Xtate.IoC.Test/Examples`.

## Avoid

- Do not add reflection-based auto-scanning or attribute-driven registration paths in this area.
- Do not add Task-based public APIs where the existing surface uses `ValueTask`.
