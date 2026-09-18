# v18 release-evidence hardening

The release model must distinguish an executed failure from an execution that could not start because the required toolchain or runtime environment was unavailable. `ReleaseGateStatus` therefore represents `NotRun`, `Passed`, `Failed`, and `EnvironmentBlocked` explicitly; a boolean pass/fail field is not sufficient evidence for canonical release decisions.

`ReleaseReadinessReport` is strict: release readiness requires exactly one result for every `ReleaseGateKind`, and every result must be `Passed`. The gate set explicitly includes numerical verification, persistence compatibility, and runtime integration so a clean compiler/test run cannot be mistaken for full release completion. Empty, partial, duplicated, failed, not-run, or environment-blocked gate sets are never release-ready. `MissingGates` and `DuplicateGates` expose structural incompleteness explicitly instead of leaving `Ready=false` unexplained.

The repository verification entrypoints now emit `artifacts/verification/verify-evidence.json` even when the .NET SDK is missing or a restore/build/test stage fails. PowerShell checks every native `dotnet` exit code explicitly, while the bash entrypoint records the same stage/status vocabulary. CI uploads the JSON evidence and TRX test results on every run.

This evidence hardening does not make any v18 build/test claim by itself. A release gate changes state only when the corresponding command actually executes and its generated evidence is synchronized back into the canonical DOCX.

Compatibility note: the ten v17 `ReleaseGateKind` numeric values are preserved explicitly (0-9). The three new release gates are appended as values 10-12 rather than inserted into the previous numeric range, preventing accidental enum-value drift for any persisted or external numeric representation.

Toolchain evidence ingestion is also explicit. `ToolchainVerificationEvidenceStore` reads the JSON emitted by `eng/verify.ps1` / `eng/verify.sh`, while `ToolchainReleaseGateMapper` converts only toolchain evidence into Build and Tests gate states. Numerical benchmark evidence remains under `ImpactLab.Core.Verification.Benchmarks` and is not conflated with compiler/test evidence.

Evidence ingestion is fail-closed. `ToolchainVerificationEvidenceStore.Validate` rejects unsupported schema versions, unknown statuses, duplicate stages, invalid pass/fail exit-code combinations, missing SDK descriptors, impossible stage ordering, and contradictions between stage outcomes and `overallStatus`. `ToolchainReleaseGateMapper` invokes this validation even when callers bypass JSON loading and construct the evidence object directly; malformed evidence therefore cannot silently become release state.

Source-feature bookkeeping is fail-closed as well. `FeatureCompletenessMatrix` now requires exactly one entry for every `FeatureArea` before either source completeness or feature completeness can be true, eliminating the empty/partial-matrix `Enumerable.All` false-positive. `MissingAreas` makes incomplete matrices diagnosable. The WPF release-readiness view continues to show source-feature readiness separately but derives actual release completion only from a complete passed `ReleaseReadinessReport`.

SDK discovery is fail-closed as well. The verification scripts do not set `dotnet.available=true` until `dotnet --version` both exits successfully and returns a non-empty version string. A discovered executable whose SDK query crashes, fails, or returns no version is recorded as `environment-blocked` with `available=false`; this keeps script output consistent with the C# evidence validator and prevents malformed toolchain discovery from masquerading as an installed SDK.

Toolchain stage structure is canonical rather than free-form. Evidence may contain only the sequential prefix `sdk -> restore -> build -> tests`; a failed or environment-blocked stage terminates the sequence, unknown/out-of-order stages are rejected, and every non-passed stage must carry a non-zero native exit code. Native exit codes remain signed `int` values because Windows process crashes can surface negative codes; negative non-zero values are valid failure/block evidence rather than parser errors.

Malformed stage arrays are also fail-closed. JSON evidence containing a `null` stage element is rejected with `InvalidDataException` before any stage property is dereferenced; a dedicated regression test protects this deserialization edge case.
