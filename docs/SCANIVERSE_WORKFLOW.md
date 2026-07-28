# AR Solution — SCANIVERSE 3D スキャンワークフロー

**目的**: 椅子・鹿を SCANIVERSE でスキャン → OBJ 出力 → Unity でリファレンスモデルとして使用  
**OBJ の役割**: 初回キャリブレーション時のマニュアルアラインメント用リファレンス  
**検証対象**: 椅子（P0） → 鹿（P1）

---

## 環境・必要なもの

| 項目 | 指定 |
|-----|------|
| **スキャンアプリ** | SCANIVERSE (iOS・無料版で十分) |
| **スキャン端末** | iPad または iPhone |
| **出力形式** | **OBJ** (GLB 不可) |
| **テクスチャ** | あり（RGB テクスチャ含める） |
| **推奨スキャン時間** | 椅子: 3-5 分 / 鹿: 10-15 分 |

---

## P0 フェーズ: 椅子スキャン（検証）

### Step 1: SCANIVERSE セットアップ

**iPad / iPhone:**

1. **App Store** → `SCANIVERSE` を検索・インストール
2. アプリを開く
3. 初回: Apple ID でサインイン（または無料版 skip）

### Step 2: スキャン対象（椅子）の準備

**物理環境**:

- **照度**: 明るい室内（自然光推奨・逆光避ける）
- **背景**: 単色背景が望ましい（黒・白・灰色）
- **対象物**: 椅子全体が見えるように配置
- **動き**: スキャン中は椅子を動かさない

### Step 3: スキャン実行

**SCANIVERSE UI:**

1. **+ (新規スキャン)** をタップ
2. **360° Scan Mode** を選択（推奨）
3. iPad カメラで椅子を指す
4. **START SCANNING** をタップ
5. **ゆっくり椅子の周りを歩く** (時計回り)
   - 目安: 1 周 3-5 分
   - カメラは椅子から 1-2m 距離
   - 高さは椅子の中央付近を常に捉える

6. スキャン完了 → **STOP** をタップ
7. ⏳ 処理中（1-2 分）

### Step 4: スキャン結果確認

**SCANIVERSE UI:**

```
Scan Result
├─ 3D モデルプレビュー ← タップで確認
├─ "Looks Good!" / "Needs Work" ← 品質評価
└─ メッシュ密度・テクスチャ品質
```

**確認ポイント**:
- ☐ 椅子全体が正確に スキャンされているか
- ☐ テクスチャ（色）がキャプチャされているか
- ☐ 穴・ノイズがないか

**品質が低い場合**: **再スキャン** 選択

### Step 5: OBJ エクスポート

**SCANIVERSE UI → Export:**

1. **Export** ボタン
2. **Format** → `OBJ + Textures` を選択
3. **Resolution** → `High` (デフォルト)
4. **Export** をタップ
5. ⏳ エクスポート中（30秒-2 分）

**出力ファイル**:
```
Chair.zip
├─ Chair.obj          ← ジオメトリ
├─ Chair.mtl          ← マテリアル定義
├─ Chair_texture.jpg  ← RGB テクスチャ
└─ ...
```

### Step 6: ファイル転送（Mac へ）

**転送方法 A: iCloud Drive（推奨・簡単）**

```
iPad: Files App → iCloud Drive
      ↓ (SCANIVERSE フォルダに Chair.zip)
      ↓
Mac: iCloud Drive → ar-solution → scans/ にコピー
```

**転送方法 B: AirDrop**

```
iPad: SCANIVERSE → Export → AirDrop 選択 → Mac へ送信
Mac: Accept → ar-solution/scans/ に保存
```

**転送方法 C: メール**

```
iPad: Files App → Chair.zip を添付 → Mac メールで受信
Mac: ダウンロード → ar-solution/scans/ に移動
```

### Step 7: ファイル解凍・確認

**Mac ターミナル:**

```bash
cd ~/Dropbox/melon/ar-solution/scans
unzip Chair.zip
ls -la Chair/
# 出力:
# Chair.obj
# Chair.mtl
# Chair_texture.jpg
```

---

## P1 フェーズ: 鹿スキャン（本番）

**流れは椅子と同じ**、ただし:

### 相違点

| 項目 | 椅子（P0） | 鹿（P1） |
|-----|----------|--------|
| スキャン時間 | 3-5 分 | 10-15 分 |
| 複雑度 | 低 | 高（足・角・毛） |
| テクスチャ | 単純 | 複雑（毛並み・細部） |
| 出力サイズ | ~50-100 MB | ~200-500 MB |

### 鹿スキャンの追加ポイント

1. **照度**: より明るく（細かい毛並みをキャプチャ）
2. **複数パス**: 角・足などを念入りにスキャン
3. **高解像度エクスポート**: `Ultra High` 検討
4. **処理時間**: 5-10 分見積もり

---

## Unity への OBJ インポート & キャリブレーション設定

### Step 1: OBJ ファイルを Unity Project にコピー

```bash
cp ~/Dropbox/melon/ar-solution/scans/Chair.obj \
   ~/Projects/ar-solution-unity/Assets/Models/
cp ~/Dropbox/melon/ar-solution/scans/Chair_texture.jpg \
   ~/Projects/ar-solution-unity/Assets/Textures/
```

### Step 2: Prefab としてセットアップ

**Unity Editor:**

1. **Assets → Models** で `Chair.obj` を確認
2. Inspector → Model インポートセッティング
   - **Mesh Compression**: OFF（正確性重視）
   - **Generate Lightmap UVs**: OFF
   - **Apply**
3. Scene に配置 → **Mesh Renderer** を確認

### Step 3: キャリブレーション用 Prefab 作成

**Hierarchy:**

1. `Chair` オブジェクトを選択
2. Inspector → **Mesh Renderer** を選択
   - **Material**: ワイヤーフレーム表示用（調整時の視認性）
3. **Prefabs フォルダに Drag & Drop** → Prefab 化
   - パス: `Assets/Prefabs/ReferenceDeer.prefab`（椅子の場合）

### Step 4: AR Session に CalibrationManager スクリプト をアタッチ

**Hierarchy:**

1. **AR Session Origin** を選択
2. **Add Component → CalibrationManager.cs** をアタッチ
3. Inspector で **Reference Deer Prefab** に `ReferenceDeer.prefab` を割り当て

**CalibrationManager.cs の役割**:
- キャリブレーション開始時に Prefab をロード
- ユーザーのジェスチャー入力を受け取り
- World Anchor で座標系を固定
- 骨格・筋肉・臓器 GLB を同一座標系に配置

---

## トラブルシューティング

### Q: SCANIVERSE のスキャンが失敗

**症状**: "Scan Failed" 表示

**原因**: カメラの動きが速すぎた / 照度不足

**対処**:
- ゆっくり動く（5-10cm/sec）
- 照度を上げる（窓側で実施）
- 背景を単色に

### Q: OBJ インポート後、モデルが見えない

**症状**: Unity Scene に OBJ が表示されない

**原因**: スケールが大きすぎる、または位置がずれている

**対処**:
- **Inspector → Scale** を調整 (例: 0.01)
- **Position** を (0, 0, 0) に

### Q: テクスチャが表示されない

**症状**: OBJ はあるが色が白・灰色

**原因**: MTL ファイルの参照パスが不正 / テクスチャが見つからない

**対処**:
- `Chair.mtl` をテキストエディタで開く
- `map_Kd Chair_texture.jpg` の記述を確認
- テクスチャファイルが同じフォルダにあるか確認

### Q: ファイルサイズが大きすぎて転送できない

**症状**: iCloud / AirDrop 転送が失敗

**対処**:
1. **Dropbox** アプリで直接アップロード
2. **Google Drive** で共有
3. Mac / iPad で USB ケーブル接続 → ファイル転送

---

## チェックリスト（椅子 P0）

| # | ステップ | 完了 | 日付 | メモ |
|---|---------|------|------|------|
| 1 | SCANIVERSE インストール | ☐ | | |
| 2 | 椅子スキャン実行（360° / 3-5分） | ☐ | | |
| 3 | スキャン品質確認（「Looks Good」） | ☐ | | |
| 4 | OBJ + Textures エクスポート | ☐ | | Chair.zip |
| 5 | Mac に Chair.zip 転送 | ☐ | | iCloud / AirDrop |
| 6 | ar-solution/scans/ に解凍 | ☐ | | |
| 7 | Unity Assets/ に OBJ インポート | ☐ | | |
| 8 | Unity Scene に配置・スケール調整 | ☐ | | |
| 9 | AR Session に統合 | ☐ | | |
| 10 | iPad で ARKit Object Detection テスト | ☐ | | 認識・追従確認 |

---

## チェックリスト（鹿 P1）

| # | ステップ | 完了 | 日付 | メモ |
|---|---------|------|------|------|
| 1 | 鹿スキャン実行（360° / 10-15分） | ☐ | | |
| 2 | スキャン品質確認 | ☐ | | 角・足・毛を確認 |
| 3 | OBJ エクスポート | ☐ | | Deer.zip |
| 4 | Mac に転送 | ☐ | | |
| 5 | Unity にインポート | ☐ | | |
| 6 | 解剖モデル（GLB）と入れ替え準備 | ☐ | | P2 向け |

---

## ファイルパス参照

```
~/Dropbox/melon/ar-solution/
├── scans/
│   ├── Chair/
│   │   ├── Chair.obj
│   │   ├── Chair.mtl
│   │   └── Chair_texture.jpg
│   └── Deer/           ← P1 用
│       └── ...
```

```
~/Projects/ar-solution-unity/Assets/
├── Models/
│   ├── Chair.obj
│   └── Deer.obj
└── Textures/
    ├── Chair_texture.jpg
    └── Deer_texture.jpg
```

---

## 参考資料

- **SCANIVERSE**: https://scaniverse.com
- **OBJ フォーマット**: https://en.wikipedia.org/wiki/Wavefront_.obj_file
- **Unity OBJ Import**: https://docs.unity3d.com/Manual/3D-formats.html

---

**Last Updated**: 2026-07-03  
**作成者**: TQY Kobayashi  
**プロジェクト**: AR Solution 卒業制作支援
