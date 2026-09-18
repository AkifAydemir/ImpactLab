# v22 portable worker / extension-host IPC integrity

Vol22 adds a Python-standard-library verification gate for the process boundaries that remain impossible to execute in the current SDK-blocked runtime. It is intentionally a **source/sample contract integrity gate**, not a runtime security certification.

## Canonical gate

`eng/process_ipc_integrity.py` verifies the current worker protocol and extension-host IPC contracts before the .NET toolchain stage. The worker side checks protocol version 2, message kinds and envelope identity/correlation fields, redirected stdio process launch, request validation, heartbeat/hello/terminal-response routing, wall-time cancellation/kill behavior and the canonical test-source hooks that must execute once an SDK-capable environment is available.

The extension-host side checks protocol version 1, the v16 policy sample, >=256-bit per-session keys, HMAC-SHA256 with payload hashing, fixed-time MAC comparison, bounded clock skew, nonce replay rejection, capability authorization, authenticated hello/request/response flow, request correlation/timeout boundaries, Windows Job Object ownership and the explicit portable-process limitation that does not claim a restricted token/container sandbox.

The audit runs before `actions/setup-dotnet` in `.github/workflows/verify.yml` and emits `artifacts/verification/process-ipc-integrity.json`. Runtime evidence remains noncanonical unless its conclusions are summarized back into the canonical DOCX.

## Fail-closed evidence

Three temporary negative repositories are used during the milestone and are not canonical source: worker protocol version drift, `authenticatedIpc=false` in the extension policy sample, and removal of fixed-time MAC verification. Each must be rejected with audit exit code 2.

## Scope boundary

PASS means the current text/source/sample contracts remain mutually consistent and security-sensitive source boundaries have not silently drifted. PASS does **not** mean the worker executable, extension host, HMAC implementation, OS isolation, process lifetime, filesystem boundary, quotas or extension code have been runtime-validated. The first real .NET 8 restore/build/test run remains mandatory, followed by executed worker/extension-host security and isolation tests on supported operating systems.
