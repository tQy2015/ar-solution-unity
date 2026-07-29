# AR Solution — iPad 配備ガイド

**目的**: Mac で iOS ビルドを生成し、iPad に Xcode 個人チーム署名でデプロイ  
**前提**: `MAC_SETUP_CHECKLIST.md` Phase 1-4 が完了していること

---

## 環境メモ

```
Mac (開発環境)
  ↓ Unity 2022 LTS + AR Foundation
  ↓ iOS Build 生成
  ↓ Xcode でビルド
  ↓
iPad (展示・検証端末)
  ↑ 個人チーム署名 (7日有効)
  ↑ USB 接続 / Wi-Fi 無線配備
```

---

## Step 1: Bundle ID・署名設定

### 1.1 Unity Player Settings で Bundle ID 設定

**Unity Editor で:**

1. **Edit → Project Settings → Player**
2. **iOS** タブを選択
3. **Other Settings → Bundle Identifier**
4. 入力例:
   ```
   com.arsolution.graduation2026
   ```
   または
   ```
   com.tqy.arsolution
   ```

### 1.2 Minimum iOS Version 確認

**Project Settings → Player → iOS → Other Settings**:
- **Minimum iOS Version**: `14.0` 以上推奨
- AR Foundation 要件: iOS 14.0+

---

## Step 2: iOS Build 生成

### 2.1 Build Settings を開く

**Unity Editor で:**

1. **File → Build Settings**
2. **Scenes in Build** セクション
3. 使用する Scene（例: `ARTestScene`）をリストに追加

### 2.2 Platform を iOS に切り替え

1. **Platform** リスト → **iOS** を選択
2. **Switch Platform** ボタンをクリック
3. ⏳ **初回は時間がかかります**（5-20分）

### 2.3 Build を実行

1. **Build** ボタン → ビルド先フォルダを指定
   ```
   Builds/iOS/
   ```
2. ⏳ ビルド中（初回: 10-20分、以降: 2-5分）

### 2.4 ビルド完了

```
Builds/iOS/Unity-iPhone.xcodeproj
├── Unity-iPhone.xcodeproj/
├── Libraries/
├── Classes/
└── ...
```

---

## Step 3: Xcode で署名設定

### 3.1 Xcode でプロジェクトを開く

```bash
open Builds/iOS/Unity-iPhone.xcodeproj
```

### 3.2 Signing & Capabilities タブ

Xcode UI:

1. **Project Navigator** → **Unity-iPhone** (プロジェクト名)
2. **Targets** → **Unity-iPhone**
3. **Signing & Capabilities** タブ

### 3.3 Team 選択

**Signing & Capabilities → Team** ドロップダウン:

- **Automatically manage signing** ☑（チェック）
- **Team**: `[Your Name] (Personal Team)` を選択
- 表示例: `TQY Kobayashi (Personal Team)`

### 3.4 Bundle Identifier 確認

**General** タブ → **Bundle Identifier**:
```
com.arsolution.graduation2026
```

---

## Step 4: iPad へのデプロイ

### 4.1 iPad を Mac に接続

**物理接続**:

1. iPad を USB-C / Lightning ケーブルで Mac に接続
2. iPad 画面: **「コンピュータを信頼しますか？」 → 「信頼」**
3. Mac 側: パスワード入力（必要に応じて）

**確認**:

```bash
# 接続状況確認
system_profiler SPUSBDataType | grep -A 5 "iPad"
```

### 4.2 Xcode でデバイス認識

**Xcode UI** → **Schemes** ドロップダウン:

- `Any iOS Device` → **iPad** を選択
  - 例: `iPad (7th generation) (OS: iPadOS 16.x)`

### 4.3 Build & Run

**Xcode UI** → **▶ (Play) ボタン** をクリック:

1. コンパイル開始
2. iPad へのインストール
3. アプリ自動起動

⏳ **初回: 3-5分** / **以降: 30秒-2分**

### 4.4 iPad 上で確認

**iPad 画面**:

```
AR Test App
└─ Cube が表示されれば成功 ✓
```

---

## Step 5: Wi-Fi 無線配備（オプション）

### 5.1 Mac と iPad を同一 Wi-Fi ネットワークに接続

### 5.2 iPad デバイスを登録

Xcode → **Window → Devices & Simulators**:

1. iPad を USB 接続
2. **Connect via Network** ☑ にチェック
3. USB ケーブル抜去 → Wi-Fi 接続に切り替わり

### 5.3 以降は Wi-Fi ビルドが可能

---

## トラブルシューティング

### Q: "Failed to prepare device for development"

**原因**: iPad の信頼設定がされていない

**対処**:
1. iPad を抜く
2. iPad → **設定 → 一般 → リセット → 信頼済みコンピュータをリセット**
3. Mac に再接続 → **「信頼」**

### Q: "Code Sign error: Provisioning profile ... not found"

**原因**: Personal Team が正しく選択されていない

**対処**:
1. Xcode → **Preferences → Accounts**
2. Apple ID が表示されているか確認
3. **Manage Certificates** → **Apple Development** が存在するか確認
4. なければ **+ ボタン** → **Apple Development** 作成

### Q: "iPad (OS version X.X) is not available"

**原因**: Xcode が古い、または iPad OS が古すぎる

**対処**:
1. **Xcode → App Store で最新版を確認**
2. iPad → **設定 → 一般 → ソフトウェア・アップデート**

### Q: ビルド時間が長い（初回）

**原因**: 正常（初回は完全コンパイル）

**期待値**: 10-20分

**高速化**: 
- 増分ビルド（2回目以降）: 30秒-2分
- **Xcode → Product → Clean Build Folder** で全削除後は初回扱い

---

## 配備チェックリスト

| # | 項目 | 完了 | 日付 | メモ |
|---|-----|------|------|------|
| 1 | Bundle ID を Unity で設定 | ☐ | | `com.arsolution...` |
| 2 | iOS Build を Unity で生成 | ☐ | | `Builds/iOS/` |
| 3 | Xcode で Bundle ID 確認 | ☐ | | |
| 4 | Xcode で Team を Personal Team に設定 | ☐ | | |
| 5 | iPad を Mac に USB 接続 | ☐ | | |
| 6 | iPad で「コンピュータを信頼」をタップ | ☐ | | |
| 7 | Xcode Schemes で iPad デバイス選択 | ☐ | | |
| 8 | Build & Run 実行 | ☐ | | ▶ ボタン |
| 9 | iPad 上でアプリが起動・Cube 表示 | ☐ | | テスト成功 |
| 10 | Wi-Fi 無線配備設定（オプション） | ☐ | | |

---

## 7日有効期限の更新

Personal Team 署名は **7日有効**。更新方法:

### A. アプリを再実行（推奨・簡単）

```
iPad でアプリを開く
→ 署名が有効期限内に自動更新される
```

### B. Xcode で再ビルド（オプション）

```
Mac で Build & Run を再実行
→ 新しい署名が付与される
```

---

## 複数 iPad への配備

1. **iPad A** を接続 → Build & Run
2. **iPad A** を抜く
3. **iPad B** を接続 → Build & Run
4. 同一バイナリで複数 iPad に配備可能

---

## 参考資料

- **関連ドキュメント**: `QUICK_START.md`, `MAC_SETUP_CHECKLIST.md`
- **公式リファレンス**: 
  - Unity: https://docs.unity3d.com/Manual/ios-building.html
  - Apple: https://developer.apple.com/account/

---

**Last Updated**: 2026-07-03  
**作成者**: TQY Kobayashi  
**プロジェクト**: AR Solution 卒業制作支援
