#!/bin/bash
# iOS ビルドスクリプト（Xcode プロジェクト生成）
# 実行後、Builds/iOS/Unity-iPhone.xcodeproj を Xcode で開く

set -e

PROJECT_PATH="$HOME/repo/ar-solution-unity"
BUILD_OUTPUT="$PROJECT_PATH/Builds/iOS"
UNITY_CLI="/Applications/Unity/Hub/Editors/2022.3.62f3/Unity.app/Contents/MacOS/Unity"
LOG_FILE="/tmp/unity-build-ios-$(date +%s).log"

echo "🔨 Building iOS project..."
echo "   Project: $PROJECT_PATH"
echo "   Output: $BUILD_OUTPUT"
echo ""

# 既存ビルドをクリア
if [ -d "$BUILD_OUTPUT" ]; then
  echo "🗑️  Removing old build..."
  rm -rf "$BUILD_OUTPUT"
fi

# ビルド実行
"$UNITY_CLI" \
  -projectPath "$PROJECT_PATH" \
  -buildTarget iOS \
  -build "$BUILD_OUTPUT" \
  -batchmode \
  -nographics \
  -logFile "$LOG_FILE" \
  -quit

# ログ確認
echo ""
echo "📋 Build Log (last 30 lines):"
tail -30 "$LOG_FILE"
echo ""

# 結果確認
if [ -d "$BUILD_OUTPUT" ] && [ -f "$BUILD_OUTPUT/Unity-iPhone.xcodeproj/project.pbxproj" ]; then
  echo "✅ iOS build succeeded"
  echo ""
  echo "📱 Next steps:"
  echo "   1. Open Xcode project: open '$BUILD_OUTPUT/Unity-iPhone.xcodeproj'"
  echo "   2. Connect iPad via USB"
  echo "   3. Select Personal Team in 'Signing & Capabilities'"
  echo "   4. Click 'Build' (Cmd+B) to deploy to iPad"
  exit 0
else
  echo "❌ iOS build failed"
  echo "   Please check: $LOG_FILE"
  exit 1
fi
