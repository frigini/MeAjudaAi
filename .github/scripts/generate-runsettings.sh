#!/bin/bash
#
# Shared utility functions for generating Coverlet runsettings files.
# This script is sourced by GitHub Actions workflows to avoid code duplication.
#

# Escape XML special characters to prevent malformed XML output
escape_xml() {
  local input="$1"
  # Use sed for compatibility - escape &, <, >, ", '
  echo "$input" | sed 's/&/\&amp;/g; s/</\&lt;/g; s/>/\&gt;/g; s/"/\&quot;/g; s/'\''/\&apos;/g'
}

# Generate a Coverlet runsettings file with the specified filters.
#
# Parameters:
#   $1 - Output file path
#   $2 - Assemblies to exclude from coverage (e.g., "[*]*Tests*")
#   $3 - Files to exclude by path (glob patterns)
#   $4 - Attributes to exclude (e.g., "Obsolete,GeneratedCode")
#   $5 - (Optional) Assemblies to include in coverage
#
# Example:
#   generate_runsettings "/tmp/unit.runsettings" \
#     "[*]*Tests*;[*]*.Migrations.*" \
#     "**/Migrations/**" \
#     "Obsolete,GeneratedCode,CompilerGenerated" \
#     "[MeAjudaAi.*]*"
#
generate_runsettings() {
  local file="$1"
  local exclude_filter="$2"
  local exclude_by_file="$3"
  local exclude_by_attr="$4"
  local include_filter="${5:-}"  # Optional parameter

  # Escape XML special characters
  exclude_filter=$(escape_xml "$exclude_filter")
  exclude_by_file=$(escape_xml "$exclude_by_file")
  exclude_by_attr=$(escape_xml "$exclude_by_attr")

  {
    echo '<?xml version="1.0" encoding="utf-8"?>'
    echo '<RunSettings>'
    echo '  <DataCollectionRunSettings>'
    echo '    <DataCollectors>'
    echo '      <DataCollector friendlyName="XPlat Code Coverage">'
    echo '        <Configuration>'
    echo '          <Format>opencover</Format>'
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
  } > "$file"
}
