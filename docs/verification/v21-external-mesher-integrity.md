# v21 external-mesher wire-contract integrity

Vol21 adds a portable, Python-standard-library audit for the canonical v15 external-mesher fixture and its current source-level protocol boundaries. `eng/external_mesher_integrity.py` checks schema-version alignment, the four-vertex neutral request, deterministic request-to-result node mapping, one-tetra connectivity, positive signed volume, the current mean-ratio admission threshold, fixture identity hashes, sample-adapter non-production semantics, and process-adapter request/result hashing/timeout/validation boundaries.

Final portable audit evidence: PASS. The canonical request fixture SHA-256 is `959cbb164c9e373c89dd92b5a5068f690bd250ca5c67045149f387db43336b84`; the result fixture SHA-256 is `d85d2996acf62116dfb6efc1ba93b13519e8b06bedbece4ebe4dfae414a18e70`. The result maps the sorted four request vertices to node ids 0..3, references all four nodes exactly once, has positive signed volume and exceeds the canonical `1e-4` minimum mean-ratio admission threshold.

Fail-closed evidence: three temporary negative repositories were rejected with exit 2 when the request schemaVersion drifted from the current protocol version, the tetrahedron referenced a missing node id, or the sample-adapter source no longer enforced its exact four-vertex fixture contract. These temporary repositories/evidence files are runtime evidence only and are not canonical source.

The audit is intentionally not an executable .NET adapter test and is not production-mesher validation. It cannot prove C# compilation, process invocation, serializer behavior, CAD fidelity or production tetrahedralization quality. CI runs it before .NET setup and uploads its structured evidence beside the existing source/artifact/toolchain records.

Actual toolchain evidence remains environment-blocked. `bash eng/verify.sh` executed at `2026-09-06T17:14:32Z`, found no `dotnet` executable, emitted an sdk-stage environment-blocked record and exited 4. Restore/build/tests and the compiled sample adapter did not execute; no build/test/runtime PASS or FAIL is inferred from the portable wire-contract audit.
