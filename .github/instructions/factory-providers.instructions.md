---
applyTo: "src/Xtate.IoC/FactoryProviders/**/*.cs"
---

# Factory providers instructions

## Purpose

Factory delegate construction for service, decorator, forwarding, sync, and async resolution paths.

## Follow existing patterns

- Keep delegate-shape compatibility with the existing `Func<IServiceProvider,...>` signatures used across provider creators.
- Reuse the `Infra.TypeInitHandle` and nested-static-field caching pattern for one-time delegate construction.
- Keep sync and async provider families behaviorally aligned when introducing new delegate creators.

## Implementation rules

- Validate assignability before building decorator delegates and throw `DependencyInjectionException` using localized `Resources` messages.
- Prefer extending existing provider/creator classes over adding parallel abstractions.

## Testing rules

- Update factory/delegate coverage in `FactoryProviderTest` when changing delegate creation behavior.

## Avoid

- Do not bypass centralized factory provider helpers with ad-hoc reflection or runtime codegen in call sites.
