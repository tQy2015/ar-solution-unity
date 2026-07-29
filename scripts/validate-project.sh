#!/bin/bash
# Unity プロジェクト検証スクリプト
# コンパイルエラー・依存関係の事前チェック

set -e

PROJECT_PATH="$HOME/repo/ar-solution-unity"
UNITY_CLI="/Applications/Unity/Hub/Editors/2022.3.62f3/Unity.app/Contents/MacOS/Unity"
LOG_FILE="/tmp/unity-validate-$(date +%s).log"

echo "🔍 Validating Unity project at: $PROJECT_PATH"
echo ""

# Unity CLI 実行
"$UNITY_CLI" \
  -projectPath "$PROJECT_PATH" \
  -batchmode \
  -nographics \
  -logFile "$LOG_FILE" \
  -quit

# ログを出力
echo "📋 Build Log:"
cat "$LOG_FILE"
echo ""

# エラーチェック
if grep -qi "error\|failed" "$LOG_FILE"; then
  echo "❌ Project validation failed - errors detected"
  exit 1
else
  echo "✅ Project validation passed"
  exit 0
fi
