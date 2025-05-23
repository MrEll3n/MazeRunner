#!/bin/bash

set -e

APP_NAME="MazeRunner"
PROJECT_PATH="./src/MazeRunner.csproj"

RUNTIMES=("osx-x64" "osx-arm64" "win-x64" "linux-x64")

for RID in "${RUNTIMES[@]}"; do
  echo "🚀 Publishing for $RID..."
  OUTPUT_DIR="./publish/$RID"

  dotnet publish "$PROJECT_PATH" \
    -c Release -r "$RID" \
    --self-contained true \
    -o "$OUTPUT_DIR" \
    /p:PublishSingleFile=true \
    /p:PublishTrimmed=false \
    /p:DebugType=None \
    /p:DebugSymbols=false

  if [[ "$RID" != win-* ]]; then
    chmod +x "$OUTPUT_DIR/$APP_NAME"
  fi

  echo "✔️  Done: $RID → $OUTPUT_DIR"
done

echo ""
echo "✅ All targets published successfully!"