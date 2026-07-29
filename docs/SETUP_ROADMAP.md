# AR Solution — セットアップロードマップ

**目的**: Mac 側全設定 + iPad デプロイ + スキャンワークフロー の完全ガイド  
**総所要時間**: 約 3-4 日（Unity インストール含む）  
**作成日**: 2026-07-03

---

## 全体フロー（図解）

```
Day 1: Unity 2022 LTS インストール (1-2時間)
  ↓
Day 1-2: Mac 開発環境セットアップ (2-3時間)
  │ ├─ ar-solution プロジェクト初期化
  │ ├─ AR Foundation + ARKit 追加
  │ └─ iOS Build Support 確認
  ↓
Day 2: iPad 配備設定 (1-2時間)
  │ ├─ Xcode Apple ID 登録
  │ ├─ Personal Team 署名設定
  │ └─ テストビルド & デプロイ
  ↓
Day 3: 椅子スキャン & Unity インポート (30分-1時間)
  │ ├─ SCANIVERSE でスキャン (5分)
  │ ├─ OBJ エクスポート (2分)
  │ ├─ Unity にインポート (5分)
  │ └─ AR Session 統合 (10分)
  ↓
Day 3-4: iPad 実機テスト (1-2時間)
  │ ├─ iOS Build 生成
  │ ├─ iPad にデプロイ
  │ ├─ キャリブレーション手順実行
  │ ├─ World Anchor 座標固定確認
  │ ├─ 距離警告 (<1m) テスト
  │ └─ 推奨距離 (1-3m) での表示確認
  ↓
✅ P0 フェーズ完了
```

---

## Day 1: Unity インストール

### 1-1 Unity Hub を開く (2分)

```bash
open "/Applications/Unity Hub.app"
```

### 1-2 Unity 2022 LTS をインストール (30-60分)

**Unity Hub GUI:**

1. **Installs** タブ
2. **Install Editor** ボタン
3. `2022.3.0f1` を検索
4. **iOS Build Support** ☑ にチェック
5. **Install** → 完了を待つ

**進捗確認:**
```bash
# ターミナルで確認
ls /Applications/Unity/Hub/Editor/2022.3.0f1/
```

**参考**: `MAC_SETUP_CHECKLIST.md` Phase 1

---

## Day 1-2: Mac 開発環境セットアップ

### 2-1 プロジェクトディレクトリ作成 (2分)

```bash
mkdir -p ~/Projects/ar-solution-unity
cd ~/Projects/ar-solution-unity
```

### 2-2 Unity プロジェクト作成 (5分)

**方法 A: Unity Hub GUI（推奨）**

1. Unity Hub → **Create**
2. **2022.3.0f1** 選択
3. **Template**: 3D (Built-in Render Pipeline)
4. **Project name**: `ar-solution-unity`
5. **Location**: `~/Projects/ar-solution-unity`
6. **Create** → ⏳ プロジェクト初期化中（5分）

**方法 B: Unity Hub CLI（高速）**

```bash
/Applications/Unity/Hub/Editor/2022.3.0f1/Unity.app/Contents/MacOS/Unity \
  -createProject ~/Projects/ar-solution-unity
```

### 2-3 AR Foundation + ARKit インポート (5-10分)

**Unity Editor で:**

1. **Window → TextAsset & Package Manager**
2. **+ ボタン** → **Add package from git URL**
3. 入力: `com.unity.xr.arfoundation`
4. **Add** → ⏳ インポート中（2-3分）
5. 同様に `com.unity.xr.arkit` を追加

### 2-4 iOS Build Support 切り替え (5-15分)

**Unity Editor → Build Settings:**

1. **File → Build Settings**
2. **Platform** → **iOS** 選択
3. **Switch Platform** → ⏳ 初回は時間がかかります（5-15分）
4. 完了後、iOS アイコンが Platform リストで青くハイライト

**参考**: `MAC_SETUP_CHECKLIST.md` Phase 2-3

---

## Day 2: iPad 配備設定

### 3-1 Xcode で Apple ID 設定 (5分)

```bash
open -a Xcode
```

**Xcode UI:**

1. **Xcode → Preferences** (Mac 版は **Settings**)
2. **Accounts** タブ
3. **+ ボタン** → **Apple ID を追加**
4. Apple ID でサインイン

### 3-2 Personal Team 署名設定 (5min)

**Xcode → Preferences → Accounts:**

1. Apple ID を選択
2. **Manage Certificates** ボタン
3. **+ ボタン** → **Apple Development** を作成
4. 完了

### 3-3 テストビルド & iPad デプロイ (30-45min)

**Unity で:**

1. **File → Build Settings → iOS**
2. **Scenes in Build** に テストシーン (例: `ARTestScene`) を追加
3. **Build** → `Builds/iOS/` フォルダ指定 → ⏳ ビルド中（10-20分）

**Xcode で:**

```bash
open Builds/iOS/Unity-iPhone.xcodeproj
```

1. **Signing & Capabilities**
2. **Team** → Personal Team 選択
3. **▶ (Play) ボタン** → iPad にビルド開始 → ⏳ 3-5分
4. iPad 画面に Cube が表示される → ✅ テスト成功

**参考**: `IPAD_DEPLOYMENT.md`

---

## Day 3: 椅子スキャン & Unity 統合

### 4-1 椅子を SCANIVERSE でスキャン (5-10min)

**iPad で:**

1. **SCANIVERSE** アプリを開く
2. **+ (新規スキャン)** → **360° Scan**
3. **START SCANNING** → ゆっくり椅子の周りを歩く（3-5分）
4. **STOP** → 処理中（1-2分）

### 4-2 OBJ エクスポート (2-3min)

**SCANIVERSE UI:**

1. **Export** → **OBJ + Textures**
2. **Export** → ⏳ エクスポート中（1-2分）
3. `Chair.zip` 生成

### 4-3 Mac に転送 (2-5min)

**方法 A: iCloud Drive（推奨）**

- iPad: Files App → `Chair.zip` をコピー
- Mac: `~/Dropbox/melon/ar-solution/scans/` に移動

**方法 B: AirDrop**

- iPad: `Chair.zip` → AirDrop → Mac へ

### 4-4 Unity にインポート (5-10min)

**Mac ターミナル:**

```bash
cd ~/Dropbox/melon/ar-solution/scans
unzip Chair.zip
```

**Unity Editor:**

1. **Assets → Models** フォルダに `Chair.obj` をドラッグ
2. `Chair_texture.jpg` も **Assets → Textures** に配置
3. Scene に `Chair` をドラッグ&ドロップ

### 4-5 AR Session 統合 (10-15min)

**Unity Editor:**

1. **Hierarchy → + → AR Foundation → AR Session**
2. `Chair` を AR Session の子に配置
3. **AR Object Manager** (またはカスタムスクリプト) で追跡設定

**参考**: `SCANIVERSE_WORKFLOW.md`

---

## Day 3: iPad 実機テスト

### 5-1 iOS Build 生成 (10-20min)

```bash
# Unity Editor
File → Build Settings → iOS
Build → Builds/iOS/
```

### 5-2 iPad にデプロイ (3-5min)

```bash
open Builds/iOS/Unity-iPhone.xcodeproj
# Xcode → ▶ ボタン
```

### 5-3 実機テスト (10-15min)

**iPad で:**

1. アプリ起動
2. 椅子をカメラで指す
3. **確認ポイント**:
   - ☑ 椅子が認識されるか（ARKit Object Detection）
   - ☑ 3D モデルが正確に追従するか
   - ☑ テクスチャが表示されるか
   - ☑ 精度は 1cm 以内か（目視確認）

**テスト成功**: ✅ P0 フェーズ完了

---

## チェックリスト（全体）

### Unity インストール

- [ ] Unity Hub で 2022.3.0f1 インストール開始
- [ ] iOS Build Support を選択
- [ ] インストール完了

### Mac 開発環境

- [ ] ar-solution-unity プロジェクト作成
- [ ] AR Foundation Package インポート
- [ ] ARKit XR Plugin インポート
- [ ] iOS Build Support に切り替え成功

### iPad 配備

- [ ] Xcode で Apple ID 登録
- [ ] Personal Team 署名設定
- [ ] テストビルド生成 ✅ Cube 表示

### 椅子スキャン

- [ ] SCANIVERSE でスキャン
- [ ] OBJ + Textures エクスポート
- [ ] Chair.zip を Mac に転送
- [ ] Unity にインポート
- [ ] AR Session 統合

### iPad 実機テスト

- [ ] iOS Build 生成
- [ ] iPad にデプロイ成功
- [ ] 椅子が認識・追従
- [ ] テクスチャ表示 ✅ P0 完了

---

## ファイル参照マップ

```
~/Dropbox/melon/ar-solution/
├── CLAUDE.md                 ← プロジェクト設計書
├── STATE.md                  ← 進捗（このセッションで更新）
└── docs/
    ├── SETUP_ROADMAP.md      ← 本ファイル（全体ロードマップ）
    ├── QUICK_START.md        ← TL;DR 版
    ├── MAC_SETUP_CHECKLIST.md ← Unity インストール詳細
    ├── IPAD_DEPLOYMENT.md    ← iPad 配備詳細
    └── SCANIVERSE_WORKFLOW.md ← スキャン・インポート詳細

~/Projects/ar-solution-unity/  ← Unity プロジェクト本体
├── Assets/
├── Builds/iOS/               ← Xcode プロジェクト出力
└── ...
```

---

## トラブル対応チャート

| 症状 | ドキュメント | セクション |
|-----|-----------|---------|
| Unity インストール失敗 | MAC_SETUP_CHECKLIST | Phase 1 / トラブル |
| iOS Build Support がない | MAC_SETUP_CHECKLIST | Phase 3 |
| iPad 認識されない | IPAD_DEPLOYMENT | Step 4 / トラブル |
| 署名エラー | IPAD_DEPLOYMENT | Step 3 / トラブル |
| OBJ が見えない | SCANIVERSE_WORKFLOW | トラブル |
| テクスチャが白い | SCANIVERSE_WORKFLOW | トラブル |

---

## 次のフェーズ（P1 以降）

```
P0 完了 ✅
  ↓
P1: 鹿本番スキャン
  ├─ 360° スキャン (10-15分)
  └─ OBJ エクスポート
  ↓
P2: 解剖モデル投入
  ├─ 骨格・筋肉・臓器 GLB モデル取得
  └─ Unity で Cube と入れ替え
  ↓
P3: レイヤー UI 実装
  ├─ ボタンスクリプト
  └─ ON/OFF トグル
  ↓
本番配備: 学生 Mac へ転送
```

---

**Total Estimated Time**: 3-4 日  
**Last Updated**: 2026-07-03  
**作成者**: TQY Kobayashi  
**プロジェクト**: AR Solution 卒業制作支援（大阪芸術大学）
