---
applyTo: "test/Xtate.IoC.Test/Tests/**/*.cs"
---

# Test suite instructions

## Purpose

Behavioral and regression tests for IoC internals and public APIs.

## Follow existing patterns

- Keep MSTest style: `[TestClass]` / `[TestMethod]`, explicit Arrange/Act/Assert sections, and focused assertions.
- Prefer assertion styles already used here (`Assert.ThrowsExactly`, `Assert.AreSame`, `Assert.AreNotSame`, `Assert.IsInstanceOfType`).
- Keep helper test types nested/private within the test class when they are scenario-specific.

## Implementation rules

- Cover both async and sync API variants when changing shared behavior.
- For lifetime/scoping changes, include disposal and ownership assertions, not only type checks.

## Testing rules

- Keep test method names scenario-driven (`Action_State_ExpectedResult` style used in this folder).
- Keep tests independent and parallel-safe under method-level parallelization.

## Avoid

- Do not rely on execution order or shared mutable static state across tests.
