# Consistency audit coverage — pre-remediation baseline

Completed generation 2 on 2026-10-30 against convention revision 52. All 50 criteria are reviewed across the 205-file inventory. The audit was completed before implementation. All five resulting tasks were subsequently implemented and verified; see [implementation evidence](IMPLEMENTATION.audit-fixes.json). The matrix and fingerprints below intentionally retain the pre-fix baseline.

Exact per-file dispositions, hashes, evidence and task IDs: [coverage enumeration](AUDIT.coverage.json). Review methods and selectors: [summary](AUDIT.summary.json). Inputs: [inventory](AUDIT.inventory.json), [syntax declarations and bodies](AUDIT.structure.json), [checks](AUDIT.checks.json), [direct member ordering](AUDIT.members.json), and [evaluated projects](AUDIT.projects.json). The single remediation list is [TODO.md](TODO.md); completion is recorded in [TODO.progress.json](TODO.progress.json).

The comparison unit is each file and its relevant declarations/operations, or a configuration/project boundary. Each criterion partitions the same 205-file universe. Applicable = compliant + deviation + exception + unreviewed; N/A is separate. Criteria overlap: summing rows does not count unique files. Generated accessors/build inputs are configuration evidence, not style peers. Build output, vendored files, IDE metadata and this audit workspace are excluded from source comparison.

| Criterion | Applicable | Compliant | Deviation | Exception | Unreviewed | N/A | Tasks |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| [C01](CONVENTIONS.md#c01) | 98 | 98 | 0 | 0 | 0 | 107 |  |
| [C02](CONVENTIONS.md#c02) | 96 | 96 | 0 | 0 | 0 | 109 |  |
| [C03](CONVENTIONS.md#c03) | 96 | 96 | 0 | 0 | 0 | 109 |  |
| [C04](CONVENTIONS.md#c04) | 96 | 96 | 0 | 0 | 0 | 109 |  |
| [C05](CONVENTIONS.md#c05) | 96 | 96 | 0 | 0 | 0 | 109 |  |
| [C06](CONVENTIONS.md#c06) | 171 | 167 | 0 | 4 | 0 | 34 |  |
| [C07](CONVENTIONS.md#c07) | 205 | 205 | 0 | 0 | 0 | 0 |  |
| [C08](CONVENTIONS.md#c08) | 168 | 168 | 0 | 0 | 0 | 37 |  |
| [C09](CONVENTIONS.md#c09) | 46 | 46 | 0 | 0 | 0 | 159 |  |
| [C10](CONVENTIONS.md#c10) | 70 | 70 | 0 | 0 | 0 | 135 |  |
| [C11](CONVENTIONS.md#c11) | 88 | 88 | 0 | 0 | 0 | 117 |  |
| [C12](CONVENTIONS.md#c12) | 8 | 8 | 0 | 0 | 0 | 197 |  |
| [C13](CONVENTIONS.md#c13) | 22 | 19 | 1 | 2 | 0 | 183 | [CONS-018](TODO.md#cons-018) |
| [C14](CONVENTIONS.md#c14) | 24 | 24 | 0 | 0 | 0 | 181 |  |
| [C15](CONVENTIONS.md#c15) | 19 | 19 | 0 | 0 | 0 | 186 |  |
| [C16](CONVENTIONS.md#c16) | 9 | 8 | 1 | 0 | 0 | 196 | [CONS-019](TODO.md#cons-019) |
| [C17](CONVENTIONS.md#c17) | 96 | 96 | 0 | 0 | 0 | 109 |  |
| [C18](CONVENTIONS.md#c18) | 11 | 11 | 0 | 0 | 0 | 194 |  |
| [C19](CONVENTIONS.md#c19) | 51 | 51 | 0 | 0 | 0 | 154 |  |
| [C20](CONVENTIONS.md#c20) | 60 | 59 | 0 | 1 | 0 | 145 |  |
| [C21](CONVENTIONS.md#c21) | 18 | 18 | 0 | 0 | 0 | 187 |  |
| [C22](CONVENTIONS.md#c22) | 96 | 96 | 0 | 0 | 0 | 109 |  |
| [C23](CONVENTIONS.md#c23) | 168 | 168 | 0 | 0 | 0 | 37 |  |
| [C24](CONVENTIONS.md#c24) | 96 | 96 | 0 | 0 | 0 | 109 |  |
| [C25](CONVENTIONS.md#c25) | 22 | 22 | 0 | 0 | 0 | 183 |  |
| [C26](CONVENTIONS.md#c26) | 97 | 97 | 0 | 0 | 0 | 108 |  |
| [C27](CONVENTIONS.md#c27) | 42 | 42 | 0 | 0 | 0 | 163 |  |
| [C28](CONVENTIONS.md#c28) | 36 | 36 | 0 | 0 | 0 | 169 |  |
| [C29](CONVENTIONS.md#c29) | 38 | 37 | 1 | 0 | 0 | 167 | [CONS-019](TODO.md#cons-019) |
| [C30](CONVENTIONS.md#c30) | 96 | 96 | 0 | 0 | 0 | 109 |  |
| [C31](CONVENTIONS.md#c31) | 96 | 96 | 0 | 0 | 0 | 109 |  |
| [C32](CONVENTIONS.md#c32) | 73 | 73 | 0 | 0 | 0 | 132 |  |
| [C33](CONVENTIONS.md#c33) | 70 | 70 | 0 | 0 | 0 | 135 |  |
| [C34](CONVENTIONS.md#c34) | 70 | 70 | 0 | 0 | 0 | 135 |  |
| [C35](CONVENTIONS.md#c35) | 70 | 69 | 1 | 0 | 0 | 135 | [CONS-019](TODO.md#cons-019) |
| [C36](CONVENTIONS.md#c36) | 98 | 97 | 1 | 0 | 0 | 107 | [CONS-020](TODO.md#cons-020) |
| [C37](CONVENTIONS.md#c37) | 98 | 97 | 1 | 0 | 0 | 107 | [CONS-020](TODO.md#cons-020) |
| [C38](CONVENTIONS.md#c38) | 83 | 83 | 0 | 0 | 0 | 122 |  |
| [C39](CONVENTIONS.md#c39) | 170 | 170 | 0 | 0 | 0 | 35 |  |
| [C40](CONVENTIONS.md#c40) | 168 | 166 | 2 | 0 | 0 | 37 | [CONS-021](TODO.md#cons-021) |
| [C41](CONVENTIONS.md#c41) | 169 | 169 | 0 | 0 | 0 | 36 |  |
| [C42](CONVENTIONS.md#c42) | 11 | 11 | 0 | 0 | 0 | 194 |  |
| [C43](CONVENTIONS.md#c43) | 175 | 173 | 2 | 0 | 0 | 30 | [CONS-017](TODO.md#cons-017) |
| [C44](CONVENTIONS.md#c44) | 11 | 11 | 0 | 0 | 0 | 194 |  |
| [C45](CONVENTIONS.md#c45) | 96 | 96 | 0 | 0 | 0 | 109 |  |
| [C46](CONVENTIONS.md#c46) | 12 | 11 | 1 | 0 | 0 | 193 | [CONS-018](TODO.md#cons-018) |
| [C47](CONVENTIONS.md#c47) | 33 | 30 | 1 | 2 | 0 | 172 | [CONS-019](TODO.md#cons-019) |
| [C48](CONVENTIONS.md#c48) | 13 | 13 | 0 | 0 | 0 | 192 |  |
| [C49](CONVENTIONS.md#c49) | 74 | 74 | 0 | 0 | 0 | 131 |  |
| [C50](CONVENTIONS.md#c50) | 98 | 96 | 1 | 1 | 0 | 107 | [CONS-019](TODO.md#cons-019) |

## Verification and limits

Current-source verification: all six library targets built; 739 tests passed on each of five test frameworks (3,695 executions). [Verification evidence](AUDIT.verification.json) preserves full console output because .log files are ignored by Git. The two changed documentation examples were reread and structural evidence refreshed before completion. No production/test source or conventions were edited by the audit.

The isolated ValueTask probe reproduces CONS-019 in the unchanged runtime. The native-handle race is supported by a concrete source interleaving and the selected lifecycle policy; this test run did not reproduce the intermittent stress failure.

Earlier read-only formatter verification exited 2. Its report predates the latest two example edits. IMPORTS identifies the two still-unsorted global lists; IDE0130 conflicts with selected namespace-folder exceptions. WHITESPACE includes unselected line-ending defaults and JetBrains alignment/unary-spacing differences. Other SDK suggestions are not automatically selected convention violations. Generated/vendor diagnostics are excluded. No clean formatter result is claimed. Initial generic-owner member-order candidates were corrected by direct syntax-span inspection in AUDIT.members.json.

At audit time ReSharper CLI inspection was unperformed. Subsequent implementation verification completed it successfully; see IMPLEMENTATION.audit-fixes.json. Suppression scope/rationale and analyzer configuration were reviewed from source; no successful ReSharper run is claimed. Default-symbol structural extraction is supplemented by branch review and all-target builds. No performance measurements were taken. This audit assesses the documented conventions, not exhaustive correctness.

All source hashes, file membership, convention revision/digest and documenting tracker were revalidated. Historical implementation evidence and owner decisions remain preserved; checked historical decisions do not imply that current defects are fixed.
