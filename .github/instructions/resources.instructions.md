---
applyTo: "src/Xtate.IoC/Properties/Resources*"
---

# Resource files instructions

## Purpose

Localized exception/messages source (`Resources.resx`) and generated strongly-typed accessor class.

## Follow existing patterns

- Treat `Resources.Designer.cs` as generated output; source-of-truth edits belong in `Resources.resx`.
- Keep message keys stable and descriptive so existing `Resources.Format_*` and `Resources.Exception_*` call sites remain valid.

## Implementation rules

- When adding/changing messages, update only `.resx` content and allow regeneration to update designer accessors.
- Preserve placeholder ordering/meaning (`{0}`, `{1}`, ...) used by existing exception formatting calls.

## Testing rules

- Update localization/resource tests (`ResourcesLocalizationTest`, `ResourcesThreadCultureTest`) when keys or message formats change.

## Avoid

- Do not hand-edit generated code blocks in `Resources.Designer.cs`.
