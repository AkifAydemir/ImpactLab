# v19 portable source-integrity verification

ImpactLab v19 makes the source-level/cross-file audit reproducible inside the canonical repository instead of relying on one-off chat/runtime inspection. `eng/source_integrity.py` is a Python-standard-library audit that deliberately does **not** compile C#, execute xUnit, run WPF, or claim numerical/runtime/security/accessibility validation.

The audit checks only contracts that can be established honestly without the .NET SDK: case-insensitive canonical path uniqueness, exclusion of generated/runtime artifact directories, JSON parseability, project/props/targets/XAML XML parseability, solution project completeness, `ProjectReference` target resolution, verification-workflow indentation/wiring, explicit `ReleaseGateKind` numeric uniqueness/monotonicity, and restore -> build -> tests ordering/evidence support in both toolchain entrypoints.

The Windows GitHub Actions workflow runs the portable audit before .NET setup/build/test and uploads `artifacts/verification/source-integrity.json` beside the existing toolchain evidence. Runtime evidence under `artifacts/` remains noncanonical unless intentionally summarized into the canonical document.

Executed source-integrity evidence in the v19 development runtime: PASS. The final canonical candidate observed 1,220 source files, parsed 26 JSON documents and 10 project/props/targets/XAML XML documents, found all 7 solution projects exactly once, resolved all 6 `ProjectReference` edges, and passed workflow/release-gate/verification-entrypoint checks.

Fail-closed harness audit: three deliberately corrupted temporary repositories were rejected with non-zero status: (1) the known YAML indentation defect (`push:` moved from two spaces to column zero), (2) malformed JSON in the extension-host policy sample, and (3) a `ProjectReference` redirected to a missing target. These negative fixtures are runtime audit evidence only and are not canonical source files.

Canonical extraction-mode note: section-7/DOCX reconstruction preserves text payloads but does not encode POSIX executable permission bits. A freshly reconstructed `eng/verify.sh` therefore must be invoked portably as `bash eng/verify.sh ...`; direct `./eng/verify.sh` execution is not a canonical extraction guarantee. This is an extraction metadata boundary, not a C# build failure.

Actual toolchain state remains environment-blocked. On 2026-09-06T16:17:09Z, `bash eng/verify.sh artifacts/verification/verify-evidence-v19.json` executed and exited 4 because no `dotnet` executable was available. The emitted evidence contains only the sdk stage with `environment-blocked` / exitCode 127. Restore, compilation and tests did not run, so no build/test PASS or FAIL is inferred.

v19 therefore closes only the portable source-integrity reproducibility milestone. All compiler/analyzer/xUnit, numerical, runtime-integration, persistence, security, performance and accessibility release gates remain open.
