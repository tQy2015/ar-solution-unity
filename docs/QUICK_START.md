# AR Solution — クイックスタート

**用途**: Mac 上で ar-solution 開発環境をセットアップして iPad にデプロイ  
**所要時間**: 約 2-3 時間（Unity 2022 LTS インストール含む）

---

## TL;DR — 最速フロー

1. **Unity Hub で Unity 2022 LTS + iOS Build Support をインストール** (30-60分)
   ```bash
   open "/Applications/Unity Hub.app"
   # GUI: Installs → Install Editor → 2022.3.0f1
   # iOS Build Support にチェック
   ```

2. **プロジェクト作成**
   ```bash
   mkdir -p ~/Projects/ar-solution-unity
   # Unity Hub または GUI から 2022.3.0f1 で作成
   ```

3. **AR Foundation + ARKit 追加**
   - Unity Editor → **Window → Package Manager**
   - `com.unity.xr.arfoundation` → Add
   - `com.unity.xr.arkit` → Add

4. **iPad 設定**
   - iPad を Mac に USB 接続
   - Xcode → **Accounts** → Apple ID 追加
   - Xcode → **Preferences → Accounts → Manage Certificates** → Apple Development 作成

5. **ビルド & デプロイ**
   ```bash
   cd ~/Projects/ar-solution-unity
   # Unity: File → Build Settings → iOS Switch → Build
   # Xcode: Signing & Capabilities → Team 選択 → Run
   ```

---

## 詳細手順

**→ `MAC_SETUP_CHECKLIST.md` を参照**

---

## ファイル構成

```
ar-solution/
├── CLAUDE.md                    ← プロジェクト設計書
├── STATE.md                     ← 現在地・進捗
├── docs/
│   ├── QUICK_START.md          ← このファイル
│   ├── MAC_SETUP_CHECKLIST.md  ← 詳細チェックリスト
│   ├── IPAD_DEPLOYMENT.md      ← iPad配備手順（後続）
│   └── SCANIVERSE_WORKFLOW.md  ← OBJスキャンフロー（後続）
├── scans/                       ← SCANIVERSE → OBJ データ
├── builds/                      ← iOS Xcode プロジェクト参照
└── unity-refs/                  ← Unity シーン構成・スクショ
```

---

## 環境メモ

| 項目 | 値 |
|-----|-----|
| **Development Mac** | ローカルMac（現在のマシン） |
| **Unity Version** | 2022.3.0f1 LTS |
| **Unity Project Path** | `~/Projects/ar-solution-unity` |
| **Target Platform** | iOS（iPad） |
| **AR Framework** | AR Foundation + ARKit Object Detection |
| **Build Output** | `Builds/iOS/Unity-iPhone.xcodeproj` |
| **iPad Provisioning** | Personal Team（Xcode 無料） |
| **iOS Version** | iPadOS 16.0+ 推奨 |

---

## トラブル対応

| 症状 | 原因 | 対処 |
|-----|-----|------|
| `Unity not found` | PATH に Unity が見つからない | `/Applications/Unity/Hub/Editor/2022.3.0f1/Unity.app/Contents/MacOS/Unity` を直接指定 |
| iOS Build Support がない | Unity 2022 LTS インストール時に選択なし | Unity Hub → Installs → 2022.3.0f1 → Add Modules |
| iPad 認識されない | USB 接続・デバイス信頼設定なし | Xcode → Window → Devices & Simulators で確認 |
| Xcode 署名エラー | Personal Team が未設定 | Xcode → Preferences → Accounts → Team 確認 |

---

## 次のステップ（順序）

1. ✅ **Mac 開発環境セットアップ** ← 今ここ
2. ⬜ **椅子スキャン** (SCANIVERSE → OBJ)
3. ⬜ **ARKit Object Detection** スキャン・検出データ生成
4. ⬜ **Unity シーン構築** (AR Session + Cube)
5. ⬜ **iPad 実機テスト** (認識・追従・精度)
6. ⬜ **鹿本番スキャン** (SCANIVERSE 360°)
7. ⬜ **解剖モデル投入** (骨格・筋肉・臓器 GLB)
8. ⬜ **レイヤーUI実装** (ボタンスクリプト)
9. ⬜ **展示端末最終配備** (学生Mac)

---

**Last Updated**: 2026-07-03  
**作成者**: TQY Kobayashi via Claude Code  
**関連**: `CLAUDE.md` / `STATE.md` / `$MELON_ROOT/716-AR-solution-research.md`
