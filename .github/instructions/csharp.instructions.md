---
applyTo: "src/**/*.cs"
---

# C# source instructions

## Style and compatibility

- Follow `.editorconfig`: tabs, nullable annotations, analyzer rules, using order, and existing naming conventions.
- Match the AGPL header and current style in adjacent source files.
- Preserve every target framework and avoid modern-only APIs without a compatible implementation.
- Preserve `ValueTask`-based APIs and required `ConfigureAwait(false)` calls.

## Architecture

- Maintain sync/async and argument-arity symmetry across registration and resolution APIs.
- Use existing `TypeKey`, `InstanceScope`, `SharedWithin`, and `Option` models.
- Preserve decorator ordering, module idempotency, async initialization, ownership, and disposal behavior.
- Keep completed-`ValueTask` fast paths equivalent to awaited fallbacks.

## Generated and dependency files

- Edit `Properties/Resources.resx`, not `Resources.Designer.cs`.
- Keep dependency versions in `Directory.Packages.props` and omit versions from `PackageReference` items.
- Do not edit generated build-property files or build output.

## Verification

- Cover affected sync/async variants, argument arities, scopes, ownership, and disposal.
- Build a modern target and relevant compatibility targets.
