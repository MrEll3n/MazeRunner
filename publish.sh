#!/bin/bash

# Exit immediately if a command fails
set -e

# App name (replace with your actual project name if needed)
APP_NAME="YourApp"

# Target runtime identifiers
RUNTIMES=("osx-x64" "osx-arm64" "win-x64" "linux-x64")

for RID in "${RUNTIMES[@]}"; do
  echo "Publishing for $RID..."

  OUTPUT_DIR="./publish/$RID"

  dotnet publish -c Release -r "$RID" \
    --self-contained true \
    -o "$OUTPUT_DIR" \
    /p:PublishSingleFile=true \
    /p:PublishTrimmed=true \
    /p:DebugType=None \
    /p:DebugSymbols=false

  echo "✔️  Done: $RID → $OUTPUT_DIR"
done

echo ""
echo "✅ All targets published successfully!"