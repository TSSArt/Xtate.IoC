---
applyTo: ".github/workflows/**/*.yml"
---

# GitHub workflows instructions

## Purpose

CI/CD automation for build, test, security scanning, packaging, and publishing.

## Follow existing patterns

- Keep jobs on Ubuntu with explicit .NET setup and Mono installation where multi-target builds require `net462` support.
- Preserve reusable environment variables (for example `Configuration`) and current restore/build/test command flow.
- Keep action major versions consistent with existing workflows unless intentionally upgrading.

## Implementation rules

- Maintain `--no-restore` / `--no-build` chaining between restore/build/test steps when appropriate.
- Keep secret usage scoped to workflow steps that need package feeds or publishing.
- Preserve trigger intent (`push` branches/tags, `pull_request`, schedule, `workflow_dispatch`) when editing workflow files.

## Testing rules

- Ensure workflow changes keep both `publish.yml` and `codeql.yml` aligned with supported SDK/runtime assumptions.

## Avoid

- Do not add plaintext credentials or environment-specific host assumptions.
