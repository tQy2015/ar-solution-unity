# AR Solution — Mac セットアップチェックリスト

**目的**: Mac 上で ar-solution 開発環境を完全セットアップ  
**対象**: ローカル開発 + iPad 配備  
**実施日**: 2026-07-03

---

## Phase 1: Unity 2022 LTS インストール

### 1.1 Unity Hub から 2022 LTS インストール

- [ ] Unity Hub を起動: `open "/Applications/Unity Hub.app"`
- [ ] **Installs** → **Install Editor**
- [ ] **2022.3.0f1** (LTS) を検索
- [ ] **iOS Build Support** をチェック ✅
- [ ] インストール開始
- [ ] インストール完了を待つ（30分〜1時間）

### 1.2 インストール確認

```bash
# 確認コマンド
ls /Applications/Unity/Hub/Editor/2022*/Unity.app/Contents/MacOS/Unity
```

**期待される出力**:
```
/Applications/Unity/Hub/Editor/2022.3.0f1/Unity.app/Contents/MacOS/Unity
```

---

## Phase 2: ar-solution プロジェクト初期化（Mac）

### 2.1 プロジェクトディレクトリ作成

```bash
# ローカルプロジェクトディレクトリ
mkdir -p ~/Projects/ar-solution-unity
cd ~/Projects/ar-solution-unity
```

**STATE.md に記録**:
```
Unityプロジェクト本体パス: ~/Projects/ar-solution-unity
Unity作業マシン: ローカルMac（開発・iOS Build）
```

### 2.2 Unity プロジェクト作成

```bash
# Unity Hub CLI で新規プロジェクト作成
open "/Applications/Unity/Hub/Editor/2022.3.0f1/Unity.app/Contents/MacOS/Unity" \
  -createProject ~/Projects/ar-solution-unity
```

または Unity Hub GUI:
- [ ] **Create** → **2022.3.0f1** 選択
- [ ] Project Template: **3D (Built-in Render Pipeline)**
- [ ] Project name: `ar-solution-unity`
- [ ] Location: `~/Projects/ar-solution-unity`
- [ ] **Create project**

---

## Phase 3: AR Foundation + ARKit 設定

### 3.1 Unity Editor で Package Manager 開く

1. Unity Editor 起動: `open ~/Projects/ar-solution-unity`
2. **Window** → **TextAsset & Package Manager**
3. **+ ボタン** → **Add package from git URL**

### 3.2 AR Foundation インポート

- [ ] **Add package from git URL**
  ```
  com.unity.xr.arfoundation
  ```
- [ ] インポート完了を待つ

### 3.3 ARKit XR Plugin インポート

- [ ] 同様に追加:
  ```
  com.unity.xr.arkit
  ```

### 3.4 iOS Build Support 確認

**Build Settings** で確認:
1. **File** → **Build Settings**
2. **Platform** リスト → **iOS** を見つける
3. **Switch Platform** ボタン（初回のみ時間がかかります）
4. 完了を待つ

---

## Phase 4: iPad 配備設定

### 4.1 Apple ID 登録（個人チーム）

**Xcode で実施**:

```bash
# Xcode を開く
open -a Xcode
```

1. **Xcode** → **Settings** → **Accounts**
2. **+ ボタン** → **Apple ID を追加**
3. Apple ID でサインイン
4. **Manage Certificates** → **+ ボタン**
5. **Apple Development** を作成

### 4.2 iPad デバイス登録

1. iPad を Mac に USB 接続
2. Xcode が自動認識するのを待つ
3. iPad 側: **信頼する** をタップ
4. Xcode に Device として登録される

### 4.3 Bundle ID 設定（Unity → Xcode）

**Unity で**:
1. **Edit** → **Project Settings** → **Player**
2. **iOS** タブ
3. **Other Settings** → **Bundle Identifier**
4. 入力例: `com.arsolution.graduation2026`

---

## Phase 5: 初回ビルド・デプロイテスト

### 5.1 シンプルなテストシーン作成

**Unity で**:

1. **Hierarchy** → **+ ボタン** → **3D Object** → **Cube**
2. **File** → **Save Scene**
3. Scene 名: `ARTestScene`

### 5.2 iOS Build 生成

1. **File** → **Build Settings**
2. **Scenes in Build** に `ARTestScene` を追加
3. **Player Settings** で Bundle ID 確認
4. **Build** → `Builds/iOS` フォルダを指定
5. ビルド完了を待つ（初回は 10-20 分）

### 5.3 Xcode で iPad にデプロイ

```bash
# Xcode プロジェクトを開く
open Builds/iOS/Unity-iPhone.xcodeproj
```

Xcode で:
1. **Signing & Capabilities**
2. **Team** → 個人チーム選択
3. ▶ ボタン → iPad にビルド開始
4. iPad 上でアプリが起動 → **テスト成功**

---

## チェックリスト確認フォーム

| フェーズ | チェック項目 | 完了 | 日付 | メモ |
|---------|------------|------|------|------|
| 1.1 | Unity 2022 LTS インストール | ☐ | | |
| 1.2 | インストール確認コマンド実行 | ☐ | | |
| 2.1 | プロジェクトディレクトリ作成 | ☐ | | |
| 2.2 | Unity プロジェクト作成 | ☐ | | |
| 3.1 | AR Foundation インポート | ☐ | | |
| 3.3 | ARKit XR Plugin インポート | ☐ | | |
| 3.4 | iOS Build Support 切り替え | ☐ | | |
| 4.1 | Apple ID 登録（個人チーム） | ☐ | | |
| 4.2 | iPad デバイス登録 | ☐ | | |
| 4.3 | Bundle ID 設定 | ☐ | | |
| 5.1 | テストシーン作成 | ☐ | | |
| 5.2 | iOS Build 生成 | ☐ | | |
| 5.3 | iPad デプロイ・テスト | ☐ | | |

---

## トラブルシューティング

### Unity 2022 LTS が見つからない

```bash
# インストール確認
ls /Applications/Unity/Hub/Editor/ | grep 2022
```

**対処**: Unity Hub から再度インストール

### iOS Build Support がない

1. Unity Hub → **Installs** → 2022.3.0f1 の歯車アイコン
2. **Add Modules** → **iOS Build Support** をチェック
3. インストール

### Xcode で署名エラー

1. **Signing & Capabilities**
2. **Team** が正しく選択されているか確認
3. 個人チーム（Personal Team）を選択

### iPad が認識されない

```bash
# 接続確認
system_profiler SPUSBDataType | grep -A 5 "iPad"

# Xcode で見える場合もある
# Window → Devices & Simulators
```

---

## 次のステップ（P0 フェーズ）

- [ ] 椅子を SCANIVERSE で 360° スキャン → OBJ 出力
- [ ] OBJ を Unity にインポート
- [ ] ARKit Object Detection でスキャン・検出データ生成
- [ ] iPad 上でリアルタイム認識・追従テスト
- [ ] 精度検証（1cm 以内の SYNC）

---

**最終更新**: 2026-07-03  
**責任**: TQY Kobayashi  
**参考資料**: `./CLAUDE.md`, `./STATE.md`, `$MELON_ROOT/716-AR-solution-research.md`
