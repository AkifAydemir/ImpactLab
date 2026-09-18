# v15 source/sample catalog
This catalog records source-level fixtures that must remain in the Current Canonical Source Snapshot.
- `samples/v15/external-mesher/mesher-request.json`: neutral CAD/tetra request fixture for the sample process adapter.
- `samples/v15/external-mesher/mesher-result.json`: deterministic one-tetra result artifact for protocol decoding/validation.
- `tools/ImpactLab.MeshAdapter.Sample/`: executable wire-contract sample; intentionally not a production tetrahedralizer.
- `samples/v15/report-golden/report.json`: deterministic engineering report document fixture.
- `samples/v15/report-golden/report.md`: expected Markdown rendering.
- `samples/v15/report-golden/report.html`: expected HTML rendering.
- `samples/v15/report-golden/metrics.csv`: expected metric-table CSV rendering.
- `samples/v15/report-golden/manifest.json`: deterministic package integrity manifest fixture.
- Historical migration fixtures under `tests/ImpactLab.Core.Tests/Fixtures/` remain compatibility evidence and must not be replaced by narrative-only documentation.
The catalog is mirrored by `SourceCoverageCatalog.CreateV15()` so source completeness can be reviewed independently from build/test execution.