#!/usr/bin/env python3
"""Portable worker/process and extension-host IPC contract audit for ImpactLab.

This gate intentionally does not execute the .NET worker or extension host. It checks
canonical source/sample contracts that can be verified deterministically with Python's
standard library while keeping runtime/security validation as an explicit downstream gate.
"""
from __future__ import annotations

import argparse
import json
import re
import sys
from dataclasses import asdict, dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


@dataclass
class CheckResult:
    name: str
    status: str
    detail: str


class Audit:
    def __init__(self) -> None:
        self.results: list[CheckResult] = []

    def pass_(self, name: str, detail: str) -> None:
        self.results.append(CheckResult(name, "passed", detail))

    def fail(self, name: str, detail: str) -> None:
        self.results.append(CheckResult(name, "failed", detail))

    @property
    def passed(self) -> bool:
        return all(item.status == "passed" for item in self.results)


def read(root: Path, rel: str) -> str:
    return (root / rel).read_text(encoding="utf-8")


def require_fragments(text: str, fragments: list[str]) -> list[str]:
    compact_text = re.sub(r"\s+", "", text)
    return [
        fragment
        for fragment in fragments
        if fragment not in text and re.sub(r"\s+", "", fragment) not in compact_text
    ]


def parse_const_int(text: str, name: str) -> int | None:
    match = re.search(rf"\bconst\s+int\s+{re.escape(name)}\s*=\s*(\d+)\s*;", text)
    return int(match.group(1)) if match else None


def parse_enum_names(text: str, enum_name: str) -> list[str]:
    match = re.search(rf"\benum\s+{re.escape(enum_name)}\s*\{{(?P<body>.*?)\}}", text, re.DOTALL)
    if not match:
        return []
    names: list[str] = []
    for part in match.group("body").split(","):
        token = part.strip()
        if not token:
            continue
        token = token.split("=", 1)[0].strip()
        m = re.match(r"[A-Za-z_]\w*", token)
        if m:
            names.append(m.group(0))
    return names


def check_worker_protocol(root: Path, audit: Audit) -> dict[str, Any]:
    rel = "src/ImpactLab.Core/Experiments/Workers/ExperimentWorkerProtocol.cs"
    failures: list[str] = []
    try:
        text = read(root, rel)
        version = parse_const_int(text, "Version")
        if version != 2:
            failures.append(f"worker protocol Version={version!r}, expected 2")
        for fragment in [
            'RequestType = "impactlab.worker.request"',
            'ResponseType = "impactlab.worker.response"',
            'HelloType = "impactlab.worker.hello"',
            'HeartbeatType = "impactlab.worker.heartbeat"',
            "if (version != Version)",
            "throw new WorkerProtocolException",
        ]:
            if fragment not in text:
                failures.append(f"missing protocol fragment {fragment!r}")
        kinds = parse_enum_names(read(root, "src/ImpactLab.Core/Experiments/Workers/WorkerMessageKind.cs"), "WorkerMessageKind")
        expected = ["Hello", "Heartbeat", "Response", "Diagnostic"]
        if kinds != expected:
            failures.append(f"WorkerMessageKind={kinds}, expected {expected}")
        envelope = read(root, "src/ImpactLab.Core/Experiments/Workers/WorkerProtocolEnvelope.cs")
        for field in ["int ProtocolVersion", "WorkerMessageKind Kind", "string CorrelationId", "JsonElement Payload", "DateTimeOffset TimestampUtc"]:
            if field not in envelope:
                failures.append(f"WorkerProtocolEnvelope missing {field}")
    except Exception as exc:  # noqa: BLE001
        failures.append(str(exc))
        version = None
        kinds = []
    if failures:
        audit.fail("worker-protocol-contract", "; ".join(failures[:20]))
    else:
        audit.pass_("worker-protocol-contract", "Worker protocol v2, message-kind and envelope identity/correlation contracts are intact.")
    return {"workerProtocolVersion": version, "workerMessageKinds": kinds}


def check_worker_process_boundary(root: Path, audit: Audit) -> None:
    failures: list[str] = []
    try:
        client = read(root, "src/ImpactLab.Core/Experiments/Workers/ProcessExperimentWorker.cs")
        host = read(root, "src/ImpactLab.Worker/WorkerHost.cs")
        program = read(root, "src/ImpactLab.Worker/Program.cs")
        quota = read(root, "src/ImpactLab.Core/Experiments/Workers/WorkerResourceQuota.cs")
        json_protocol = read(root, "src/ImpactLab.Core/Experiments/Workers/WorkerJsonProtocol.cs")
        missing = require_fragments(client, [
            "UseShellExecute=false", "RedirectStandardInput=true", "RedirectStandardOutput=true", "RedirectStandardError=true",
            'psi.ArgumentList.Add("worker")', "request.Validate();", "timeout.CancelAfter(request.Quota.WallTime)",
            "process.StandardInput.WriteLineAsync(WorkerJsonProtocol.Serialize(request))", "ExperimentWorkerProtocol.ValidateVersion(envelope.ProtocolVersion)",
            "envelope.CorrelationId!=request.EffectiveRequestId", "envelope.Kind==WorkerMessageKind.Heartbeat",
            "envelope.Kind==WorkerMessageKind.Hello", "envelope.Kind==WorkerMessageKind.Response", "process.Kill(true)",
        ])
        failures += [f"ProcessExperimentWorker missing {x!r}" for x in missing]
        missing = require_fragments(host, [
            "ReadLineAsync(ct)", "request=WorkerJsonProtocol.Deserialize<ExperimentWorkerRequest>(line);request.Validate();",
            "WorkerMessageKind.Hello", "heartbeat.Start();", "WorkerMessageKind.Response", "_writeGate.WaitAsync(ct)", "FlushAsync(ct)",
        ])
        failures += [f"WorkerHost missing {x!r}" for x in missing]
        missing = require_fragments(program, ['args[0],"worker"', "WorkerExitCodes.InvalidRequest", "new WorkerHost(Console.In,Console.Out)"])
        failures += [f"Worker Program missing {x!r}" for x in missing]
        missing = require_fragments(quota, ["WallTime <= TimeSpan.Zero", "MaxWorkingSetMb < 128", "MaxThreads < 0"])
        failures += [f"WorkerResourceQuota missing {x!r}" for x in missing]
        missing = require_fragments(json_protocol, ["new JsonStringEnumConverter()", "ExperimentWorkerProtocol.Version", "ExperimentWorkerProtocol.ValidateVersion(envelope.ProtocolVersion)"])
        failures += [f"WorkerJsonProtocol missing {x!r}" for x in missing]
    except Exception as exc:  # noqa: BLE001
        failures.append(str(exc))
    if failures:
        audit.fail("worker-process-boundary", "; ".join(failures[:24]))
    else:
        audit.pass_("worker-process-boundary", "Worker source preserves redirected stdio, request validation, protocol/correlation checks, heartbeat/response handling and wall-time kill behavior.")


def check_extension_policy_sample(root: Path, audit: Audit) -> dict[str, Any]:
    path = root / "samples/v16/extension-host-policy.json"
    failures: list[str] = []
    payload: dict[str, Any] = {}
    try:
        raw = json.loads(path.read_text(encoding="utf-8"))
        if not isinstance(raw, dict):
            failures.append("policy root must be an object")
        else:
            payload = raw
            expected_scalars = {
                "schemaVersion": 1,
                "minimumTrust": "Untrusted",
                "authenticatedIpc": True,
                "replayProtection": True,
                "windowsIsolation": "JobObject",
                "portableIsolation": "ProcessOnly",
            }
            for key, expected in expected_scalars.items():
                if raw.get(key) != expected:
                    failures.append(f"{key}={raw.get(key)!r}, expected {expected!r}")
            perms = raw.get("maximumPermissions")
            if perms != ["ReadProject", "ReadFiles"]:
                failures.append(f"maximumPermissions={perms!r}, expected ['ReadProject', 'ReadFiles']")
            enum_text = read(root, "src/ImpactLab.Core/Extensions/Security/ExtensionPermission.cs")
            enum_names = set(parse_enum_names(enum_text, "ExtensionPermission"))
            if isinstance(perms, list):
                unknown = [p for p in perms if p not in enum_names]
                if unknown:
                    failures.append(f"policy permissions not present in ExtensionPermission: {unknown}")
    except Exception as exc:  # noqa: BLE001
        failures.append(str(exc))
    if failures:
        audit.fail("extension-policy-sample", "; ".join(failures[:20]))
    else:
        audit.pass_("extension-policy-sample", "v16 policy sample requires authenticated/replay-protected IPC, JobObject on Windows and explicit process-only portable isolation.")
    return {"extensionPolicySchemaVersion": payload.get("schemaVersion")}


def check_extension_authentication(root: Path, audit: Audit) -> dict[str, Any]:
    failures: list[str] = []
    version: int | None = None
    try:
        envelope = read(root, "src/ImpactLab.Core/Extensions/Security/AuthenticatedIpcEnvelope.cs")
        version = parse_const_int(envelope, "CurrentProtocolVersion")
        if version != 1:
            failures.append(f"extension IPC protocol version={version!r}, expected 1")
        auth = read(root, "src/ImpactLab.Core/Extensions/Security/ExtensionIpcAuthenticator.cs")
        for fragment in require_fragments(auth, [
            "if(key.Length<32)", "HMACSHA256", "SHA256.HashData", "CryptographicOperations.FixedTimeEquals",
            "Math.Abs((utc-e.TimestampUtc).TotalSeconds)>_maxClockSkew.TotalSeconds", "policy.Allows(e.Capability)",
            "replay.TryAccept(e.Nonce,utc)", "e.ProtocolVersion!=AuthenticatedIpcEnvelope.CurrentProtocolVersion",
            "e.ExtensionId,", "e.SessionId,", "e.CorrelationId,", "e.Nonce,", "payloadHash",
        ]):
            failures.append(f"ExtensionIpcAuthenticator missing {fragment!r}")
        replay = read(root, "src/ImpactLab.Core/Extensions/Security/IpcReplayGuard.cs")
        for fragment in require_fragments(replay, ["_seen.ContainsKey(nonce)", "_seen[nonce]=now", "string.IsNullOrWhiteSpace(nonce)"]):
            failures.append(f"IpcReplayGuard missing {fragment!r}")
        policy = read(root, "src/ImpactLab.Core/Extensions/Security/IpcAuthorizationPolicy.cs")
        if re.sub(r"\s+", "", "(Allowed & c) == c") not in re.sub(r"\s+", "", policy):
            failures.append("IpcAuthorizationPolicy no longer enforces requested capability subset")
    except Exception as exc:  # noqa: BLE001
        failures.append(str(exc))
    if failures:
        audit.fail("extension-authenticated-ipc", "; ".join(failures[:24]))
    else:
        audit.pass_("extension-authenticated-ipc", "Extension IPC v1 retains >=256-bit keys, HMAC-SHA256 payload binding, fixed-time MAC verification, clock-skew, capability and replay checks.")
    return {"extensionIpcProtocolVersion": version}


def check_extension_host_boundary(root: Path, audit: Audit) -> None:
    failures: list[str] = []
    try:
        bootstrap = read(root, "src/ImpactLab.Core/Extensions/Security/ExtensionHostBootstrap.cs")
        launcher = read(root, "src/ImpactLab.Core/Extensions/Hosting/ExtensionProcessLauncher.cs")
        session = read(root, "src/ImpactLab.Core/Extensions/Hosting/ExtensionHostProcessSession.cs")
        runtime = read(root, "src/ImpactLab.ExtensionHost/ExtensionHostRuntime.cs")
        portable = read(root, "src/ImpactLab.Core/Extensions/Sandbox/PortableExtensionIsolationProvider.cs")
        windows = read(root, "src/ImpactLab.Core/Extensions/Sandbox/WindowsJobObjectIsolationProvider.cs")
        architecture = read(root, "docs/architecture/v16-extension-host-security.md")
        for fragment in require_fragments(bootstrap, [
            "ProtocolVersion!=AuthenticatedIpcEnvelope.CurrentProtocolVersion", "Entry assembly escapes package root", "DecodeKey().Length<32",
        ]):
            failures.append(f"ExtensionHostBootstrap missing {fragment!r}")
        for fragment in require_fragments(launcher, [
            "UseShellExecute=false", "RedirectStandardInput=true", "RedirectStandardOutput=true", "RedirectStandardError=true",
            "RandomNumberGenerator.GetBytes(32)", "ExtensionHostProtocol.WriteLineAsync(p.StandardInput,bootstrap,ct)",
            "hello.Kind!=ExtensionIpcMessageKind.Hello", "auth.Verify(hello", "p.Kill(true)",
        ]):
            failures.append(f"ExtensionProcessLauncher missing {fragment!r}")
        for fragment in require_fragments(session, [
            "linked.CancelAfter(_timeout)", "_auth.Sign(ExtensionId,SessionId,correlation,ExtensionIpcMessageKind.Request",
            "reply.CorrelationId!=correlation", "ExtensionIpcMessageKind.Response or ExtensionIpcMessageKind.Error", "_auth.Verify(reply",
        ]):
            failures.append(f"ExtensionHostProcessSession missing {fragment!r}")
        for fragment in require_fragments(runtime, [
            "bootstrap.Validate()", "new ExtensionIpcAuthenticator(bootstrap.DecodeKey())", "new IpcReplayGuard()",
            "envelope.Kind!=ExtensionIpcMessageKind.Request", "!auth.Verify(envelope", 'req.Operation.Equals("shutdown"',
            'req.Operation.Equals("ping"', 'req.Operation.Equals("describe"', 'req.Operation.Equals("invoke"', "auth.Sign(bootstrap.ExtensionId",
        ]):
            failures.append(f"ExtensionHostRuntime missing {fragment!r}")
        if "No platform restricted-token/container primitive is claimed by the portable provider." not in portable:
            failures.append("Portable isolation provider no longer disclaims restricted-token/container isolation")
        for fragment in ["JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE", "AssignProcessToJobObject", "Restricted token/AppContainer is not claimed"]:
            if fragment not in windows:
                failures.append(f"Windows isolation provider missing {fragment!r}")
        for fragment in ["HMAC-SHA256", "fixed-time MAC comparison", "nonce replay rejection", "Job Object", "does **not** claim a restricted token/container sandbox"]:
            if fragment not in architecture:
                failures.append(f"v16 extension-security architecture doc missing {fragment!r}")
    except Exception as exc:  # noqa: BLE001
        failures.append(str(exc))
    if failures:
        audit.fail("extension-host-process-boundary", "; ".join(failures[:28]))
    else:
        audit.pass_("extension-host-process-boundary", "Bootstrap, signed hello/request/response, correlation/timeout, process separation and explicit OS-isolation claim boundaries remain wired.")


def check_canonical_tests(root: Path, audit: Audit) -> None:
    failures: list[str] = []
    try:
        worker = read(root, "tests/ImpactLab.Core.Tests/WorkerProtocolTests.cs")
        auth = read(root, "tests/ImpactLab.Core.Tests/ExtensionIpcAuthenticatorTests.cs")
        for fragment in ["EnvelopeRoundTrips", "WorkerMessageKind.Heartbeat", "WorkerJsonProtocol.Payload<WorkerHeartbeat>"]:
            if fragment not in worker:
                failures.append(f"WorkerProtocolTests missing {fragment!r}")
        for fragment in ["SignedEnvelopeVerifiesOnce", "Assert.True(a.Verify", "Assert.False(a.Verify", "TamperedPayloadFails"]:
            if fragment not in auth:
                failures.append(f"ExtensionIpcAuthenticatorTests missing {fragment!r}")
    except Exception as exc:  # noqa: BLE001
        failures.append(str(exc))
    if failures:
        audit.fail("canonical-test-source-contract", "; ".join(failures[:20]))
    else:
        audit.pass_("canonical-test-source-contract", "Canonical worker round-trip and extension replay/tamper test sources remain present for the first SDK-capable execution.")


def write_evidence(path: Path, audit: Audit, summary: dict[str, Any]) -> None:
    payload = {
        "schemaVersion": 1,
        "generatedUtc": datetime.now(timezone.utc).isoformat().replace("+00:00", "Z"),
        "status": "passed" if audit.passed else "failed",
        "summary": summary,
        "checks": [asdict(item) for item in audit.results],
    }
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(payload, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--output", type=Path, default=Path("artifacts/verification/process-ipc-integrity.json"))
    args = parser.parse_args()
    root = args.root.resolve()
    output = args.output if args.output.is_absolute() else root / args.output

    audit = Audit()
    summary: dict[str, Any] = {}
    summary.update(check_worker_protocol(root, audit))
    check_worker_process_boundary(root, audit)
    summary.update(check_extension_policy_sample(root, audit))
    summary.update(check_extension_authentication(root, audit))
    check_extension_host_boundary(root, audit)
    check_canonical_tests(root, audit)
    write_evidence(output, audit, summary)

    for result in audit.results:
        print(f"[{result.status.upper()}] {result.name}: {result.detail}")
    print(f"[{'PASS' if audit.passed else 'FAIL'}] process-ipc-integrity -> {output}")
    return 0 if audit.passed else 2


if __name__ == "__main__":
    sys.exit(main())
