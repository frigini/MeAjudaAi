#!/bin/bash
#
# Shared utility functions for generating Coverlet runsettings files.
# This script is sourced by GitHub Actions workflows to avoid code duplication.
#

# Strict error handling:
# -e: exit on error
# -u: exit on unset variable
# -o pipefail: catch errors in piped commands
set -euo pipefail

# Escape XML special characters to prevent malformed XML output
escape_xml() {
    local input="$1"
    # Use sed for compatibility - escape &, <, >, ", '
    echo "$input" | sed 's/&/\&amp;/g; s/</\&lt;/g; s/>/\&gt;/g; s/"/\&quot;/g; s/'\''/\&apos;/g'
}

# Generate a Coverlet runsettings file with the specified filters.
#
# Parameters:
#   $1 - Output file path (REQUIRED)
#   $2 - Assemblies to exclude from coverage (e.g., "[*]*Tests*")
#   $3 - Files to exclude by path (glob patterns)
#   $4 - Attributes to exclude (e.g., "Obsolete,GeneratedCode")
#   $5 - (Optional) Assemblies to include in coverage
#
# Returns 0 on success, 1 on failure.
generate_runsettings() {
    local file="${1:-}"
    local exclude_filter="${2:-}"
    local exclude_by_file="${3:-}"
    local exclude_by_attr="${4:-}"
    local include_filter="${5:-}"

    # Validate required parameters
    if [ -z "$file" ]; then
        echo "❌ ERROR: Output file path is required as first parameter." >&2
        return 1
    fi

    echo "📝 Generating runsettings: $file" >&2

    # Escape XML special characters
    exclude_filter=$(escape_xml "$exclude_filter")
    exclude_by_file=$(escape_xml "$exclude_by_file")
    exclude_by_attr=$(escape_xml "$exclude_by_attr")

    # Use a temporary file to avoid partial writes if something fails
    local temp_file
    temp_file=$(mktemp)

    {
        echo '<?xml version="1.0" encoding="utf-8"?>'
        echo '<RunSettings>'
        echo '  <DataCollectionRunSettings>'
        echo '    <DataCollectors>'
        echo '      <DataCollector friendlyName="XPlat Code Coverage">'
        echo '        <Configuration>'
        echo '          <Format>cobertura</Format>'
        if [ -n "$include_filter" ]; then
            include_filter=$(escape_xml "$include_filter")
            echo "          <Include>${include_filter}</Include>"
        fi
        echo "          <Exclude>${exclude_filter}</Exclude>"
        echo "          <ExcludeByFile>${exclude_by_file}</ExcludeByFile>"
        echo "          <ExcludeByAttribute>${exclude_by_attr}</ExcludeByAttribute>"
        echo '        </Configuration>'
        echo '      </DataCollector>'
        echo '    </DataCollectors>'
        echo '  </DataCollectionRunSettings>'
        echo '</RunSettings>'
    } > "$temp_file"

    # Verify write and move to final destination
    if [ -s "$temp_file" ]; then
        mv "$temp_file" "$file"
        echo "✅ Generated: $file" >&2
        return 0
    else
        echo "❌ ERROR: Failed to generate runsettings file (empty or write failed)." >&2
        rm -f "$temp_file"
        return 1
    fi
}
