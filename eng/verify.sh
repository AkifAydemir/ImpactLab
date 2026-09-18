#!/usr/bin/env bash
set -uo pipefail
repo="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo"

evidence="${1:-artifacts/verification/verify-evidence.json}"
mkdir -p "$(dirname "$evidence")" artifacts/verification/test-results

overall_status="not-run"
dotnet_available=false
dotnet_version=""
stages=""
error_message=""

json_escape() {
    local value=${1-}
    value=${value//\\/\\\\}
    value=${value//\"/\\\"}
    value=${value//$'\n'/\\n}
    value=${value//$'\r'/\\r}
    value=${value//$'\t'/\\t}
    printf '%s' "$value"
}

add_stage() {
    local name=$1 status=$2 exit_code=$3 summary=$4
    local item
    item="{\"name\":\"$(json_escape "$name")\",\"status\":\"$(json_escape "$status")\",\"exitCode\":$exit_code,\"summary\":\"$(json_escape "$summary")\"}"
    if [[ -n "$stages" ]]; then
        stages+=","
    fi
    stages+="$item"
}

write_evidence() {
    cat > "$evidence" <<JSON
{
  "schemaVersion": 1,
  "generatedUtc": "$(date -u +'%Y-%m-%dT%H:%M:%SZ')",
  "overallStatus": "$(json_escape "$overall_status")",
  "dotnet": {
    "available": $dotnet_available,
    "version": $(if [[ -n "$dotnet_version" ]]; then printf '"%s"' "$(json_escape "$dotnet_version")"; else printf 'null'; fi)
  },
  "stages": [$stages],
  "error": $(if [[ -n "$error_message" ]]; then printf '"%s"' "$(json_escape "$error_message")"; else printf 'null'; fi)
}
JSON
}
trap write_evidence EXIT

if ! command -v dotnet >/dev/null 2>&1; then
    overall_status="environment-blocked"
    add_stage "sdk" "environment-blocked" 127 ".NET SDK executable was not found on PATH."
    exit 4
fi

dotnet_version="$(dotnet --version)"
code=$?
if [[ $code -ne 0 ]]; then
    overall_status="environment-blocked"
    error_message="dotnet --version could not execute successfully (exit code $code)."
    add_stage "sdk" "environment-blocked" "$code" "dotnet executable was found but the SDK could not be queried."
    exit 4
fi
if [[ -z "${dotnet_version//[[:space:]]/}" ]]; then
    overall_status="environment-blocked"
    error_message="dotnet --version returned no SDK version."
    add_stage "sdk" "environment-blocked" 126 "dotnet executable returned an empty SDK version."
    exit 4
fi
dotnet_available=true
add_stage "sdk" "passed" 0 ".NET SDK $dotnet_version is available."

run_stage() {
    local name=$1
    shift
    dotnet "$@"
    local code=$?
    if [[ $code -eq 0 ]]; then
        add_stage "$name" "passed" 0 "dotnet $* passed."
        return 0
    fi

    add_stage "$name" "failed" "$code" "dotnet $* failed."
    overall_status="failed"
    error_message="$name failed with exit code $code."
    exit 1
}

run_stage "restore" restore ./ImpactLab.sln
run_stage "build" build ./ImpactLab.sln -c Release --no-restore
run_stage "tests" test ./tests/ImpactLab.Core.Tests/ImpactLab.Core.Tests.csproj -c Release --no-build --results-directory ./artifacts/verification/test-results --logger "trx;LogFileName=ImpactLab.Core.Tests.trx"

overall_status="passed"
