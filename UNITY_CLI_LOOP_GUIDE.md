# Unity CLI Loop ガイド（Mac側 Claude Code 自動化）

**対象環境**: Mac + Unity 2022.3.62f3 LTS + ar-solution-unity  
**用途**: Claude Code から Unity の自動ビルド・テスト・デプロイを制御

---

## 概要

このガイドは、Mac側の Claude Code セッションで **Unity コマンドラインインターフェース（CLI）** を使った自動化タスクを実装する方法を説明します。

### 使用シーン

| フェーズ | タスク | コマンド | 実行条件 |
|---|---|---|---|
| **P2** | AR Foundation 設定確認 | `unity-validate` | コンパイルエラーの事前チェック |
| **P2** | モデル統合テスト | `unity-test-compile` | GLB インポート後の動作確認 |
| **P3** | iOS ビルド | `unity-build-ios` | Xcode 配備前の最終ビルド |
| **P3** | iPad デプロイテスト | `unity-deploy-test` | iPad 実機での形状認識精度テスト |

---

## セットアップ

### 1. 実行ファイルの確認

Mac で以下を実行してください：

```bash
# Unity CLI が利用可能か確認
/Applications/Unity/Hub/Editors/2022.3.62f3/Unity.app/Contents/MacOS/Unity -version
```

**期待される出力:**
```
Unity 2022.3.62f3 (xxxxxxxx)
```

### 2. プロジェクトパスの確認

```bash
cd ~/repo/ar-solution-unity
pwd
# → /Users/tqy/repo/ar-solution-unity

# ProjectSettings が存在するか確認
ls -la ProjectSettings/ProjectSettings.asset
```

---

## CLI コマンド一覧

### ① プロジェクト検証（コンパイルチェック）

```bash
/Applications/Unity/Hub/Editors/2022.3.62f3/Unity.app/Contents/MacOS/Unity \
  -projectPath ~/repo/ar-solution-unity \
  -batchmode \
  -logFile - \
  -quit
```

**動作**: Unity Editor なしで、C# スクリプトのコンパイル・依存関係チェック  
**結果**: エラーがあれば `stderr` に出力

### ② ユニットテスト実行

```bash
/Applications/Unity/Hub/Editors/2022.3.62f3/Unity.app/Contents/MacOS/Unity \
  -projectPath ~/repo/ar-solution-unity \
  -runTests \
  -testPlatform playmode \
  --testCategory "AR" \
  -logFile - \
  -batchmode \
  -quit
```

**動作**: AR Foundation・ARKit 関連の Unit テストを実行  
**結果**: テスト成功 / 失敗を JSON 形式で出力

### ③ iOS ビルド（Xcode プロジェクト生成）

```bash
/Applications/Unity/Hub/Editors/2022.3.62f3/Unity.app/Contents/MacOS/Unity \
  -projectPath ~/repo/ar-solution-unity \
  -buildTarget iOS \
  -build ~/repo/ar-solution-unity/Builds/iOS \
  -logFile - \
  -batchmode \
  -quit
```

**動作**: Xcode プロジェクトを生成（`Builds/iOS/` に出力）  
**使用**: Xcode から `Builds/iOS/Unity-iPhone.xcodeproj` を開いて、iPad へデプロイ

---

## 使用例（Claude Code スクリプト）

### パターン1: コンパイルテストのみ（高速）

```bash
#!/bin/bash
# ファイル: scripts/validate-project.sh

PROJECT_PATH="$HOME/repo/ar-solution-unity"
UNITY_CLI="/Applications/Unity/Hub/Editors/2022.3.62f3/Unity.app/Contents/MacOS/Unity"

echo "🔍 Validating Unity project..."
"$UNITY_CLI" \
  -projectPath "$PROJECT_PATH" \
  -batchmode \
  -logFile - \
  -quit

if [ $? -eq 0 ]; then
  echo "✅ Project validation passed"
  exit 0
else
  echo "❌ Project validation failed"
  exit 1
fi
```

**実行:**
```bash
bash scripts/validate-project.sh
```

### パターン2: コンパイル + ビルド（本格的）

```bash
#!/bin/bash
# ファイル: scripts/build-ios.sh

PROJECT_PATH="$HOME/repo/ar-solution-unity"
BUILD_OUTPUT="$PROJECT_PATH/Builds/iOS"
UNITY_CLI="/Applications/Unity/Hub/Editors/2022.3.62f3/Unity.app/Contents/MacOS/Unity"

# 既存ビルドをクリア
rm -rf "$BUILD_OUTPUT"

echo "🔨 Building iOS project..."
"$UNITY_CLI" \
  -projectPath "$PROJECT_PATH" \
  -buildTarget iOS \
  -build "$BUILD_OUTPUT" \
  -logFile - \
  -batchmode \
  -quit

if [ $? -eq 0 ]; then
  echo "✅ iOS build succeeded"
  echo "📱 Xcode project: $BUILD_OUTPUT/Unity-iPhone.xcodeproj"
  exit 0
else
  echo "❌ iOS build failed"
  exit 1
fi
```

**実行:**
```bash
bash scripts/build-ios.sh
```

---

## Claude Code `/loop` での自動実行

### 定期的なコンパイルテスト（10分ごと）

```
/loop 10m bash ~/repo/ar-solution-unity/scripts/validate-project.sh
```

### モデル統合後の自動ビルド

```bash
# コマンド例（Claude Code で実行）
bash ~/repo/ar-solution-unity/scripts/build-ios.sh
```

---

## トラブルシューティング

### エラー: `Unity CLI not found`

**原因**: Unity がインストールされていない、またはバージョンが異なる  
**対処**:
```bash
# インストール済みの Unity バージョンを確認
ls /Applications/Unity/Hub/Editors/
```

### エラー: `License activation required`

**原因**: Unity ライセンスが認証されていない  
**対処**:
```bash
# Mac でワンタイム Unity Editor を起動してライセンス認証（一度のみ）
/Applications/Unity/Hub/Editors/2022.3.62f3/Unity.app/Contents/MacOS/Unity
```

### ビルド失敗: `Scene xxx not found in build settings`

**原因**: テストシーン（`Assets/Scenes/Test0728.unity`）が Build Settings に登録されていない  
**対処**: Unity Editor で Build Settings に シーンを追加

---

## 参考資料

- **Unity CLI 公式ドキュメント**: https://docs.unity3d.com/Manual/CommandLineArguments.html
- **AR Foundation セットアップ**: `~/repo/melon-active/ar-solution/docs/AR_CALIBRATION_ARCHITECTURE.md`
- **iOS ビルド設定**: `~/repo/melon-active/ar-solution/docs/IPAD_DEPLOYMENT.md`

---

**最終更新**: 2026-07-29  
**作成者**: Claude Code (Haiku 4.5)
