# AR Solution — キャリブレーション & 座標固定アーキテクチャ

**目的**: 大型動物（鹿剥製）への AR 投影を実現する初回キャリブレーション手法  
**作成日**: 2026-07-03

---

## コンセプト図

```
展示スペース

iPad（カメラ向け）
  ↓
  実時間：鹿の映像をカメラで捉える
  ↓
  【初回キャリブレーション】
    └─ SCANIVERSE OBJ（鹿のリファレンス）を AR 空間に配置
    └─ ユーザーがマニュアルでアラインメント（位置・回転・スケール調整）
    └─ 確定時点で iPad の座標系を記録 → World Anchor 設定
  ↓
  【展示モード】（キャリブレーション後）
    ├─ 骨格モデル（GLB）が鹿内部に固定投影
    ├─ 筋肉モデル（GLB）が鹿内部に固定投影
    └─ 臓器モデル（GLB）が鹿内部に固定投影
       （iPad を動かしても AR コンテンツは鹿に固定）
  ↓
  レイヤーボタン ON/OFF で骨・筋・臓を切り替え表示
```

---

## 実装パイプライン

### Phase P0: 椅子での検証

**目的**: キャリブレーション手法のパイプライン検証

1. **椅子を SCANIVERSE でスキャン** → OBJ
2. **OBJ を Unity AR シーン に配置**
3. **初回キャリブレーション UI を実装** (Cube で簡易表示)
4. **World Anchor で座標固定**
5. **iPad 実機テスト** (認識・固定・追従確認)

### Phase P1: 鹿本番

1. **鹿を SCANIVERSE で 360° スキャン** → OBJ
2. **OBJ を Unity にインポート**（P0 と同一パイプライン）
3. **初回キャリブレーション UI が自動的に動作** (OBJ が別の鹿に置き換わるだけ)
4. **World Anchor で座標系固定**

### Phase P2: 解剖モデル投入

1. **骨格・筋肉・臓器 GLB モデルを取得**（外部リソース or 学内制作）
2. **World Anchor の座標系を基準に配置**
3. **GLB が OBJ と完全にアラインメント**（同じ座標系内）

### Phase P3: UI & 展示

1. **レイヤーボタン UI 実装** (Bone / Muscle / Organ)
2. **ON/OFF トグル** → GLB の表示・非表示
3. **接近警告** → iPad と鹿の距離 < 1m で警告表示
4. **最終 iPad へ配備**（学生 Mac 経由）

---

## 技術仕様

### 初回キャリブレーション手法

**手法**: Vision-based マニュアルアラインメント

```
ステップ 1: AR Foundation Session 起動
  └─ ARSession を初期化
  └─ ARRaycastManager を初期化（タップ検出用）

ステップ 2: SCANIVERSE OBJ (鹿のリファレンス) を AR 空間に配置
  ├─ GameObject: ReferenceDeer
  ├─ Mesh: Chair.obj（椅子の場合）/ Deer.obj（本番）
  └─ Renderer: ワイヤーフレーム表示（調整用）

ステップ 3: ユーザーがマニュアル調整
  ├─ 2 本指ドラッグ: 位置移動 (X, Y, Z)
  ├─ ピンチ: スケール調整
  └─ 回転ジェスチャー: Y 軸回転

ステップ 4: 「確定」ボタンをタップ
  ├─ iPad のワールド座標系スナップショット取得
  ├─ World Anchor を設定
  ├─ ReferenceDeer の GameObject を非表示化
  └─ 骨格・筋肉・臓器 GLB をロード → 同一座標系に配置

ステップ 5: 展示モード開始
  ├─ World Anchor により座標系は固定
  ├─ iPad を動かしても AR コンテンツは鹿に貼りついたまま
  └─ レイヤーボタンで ON/OFF 切り替え
```

### 座標系設定

```
World Space (Unity)
  ↓
  ✓ AR Session Origin (ARObjManager)
  ├─ X 軸: 左右（iPad から見て）
  ├─ Y 軸: 上下
  ├─ Z 軸: 奥行き（iPad から見て奥側が +）
  ↓
  Anchor Space (World Anchor)
  ├─ キャリブレーション確定時の iPad 位置・向きを基準
  ├─ 以降、この座標系に対して骨格・筋肉・臓器を配置
  └─ iPad の移動に追従しない（座標系が固定）
```

---

## Unity 実装スケルトン

### Hierarchy 構造

```
Scene: ARCalibrationScene
├─ AR Session
│  └─ AR Session Origin
│     ├─ Main Camera
│     └─ AR Managers
│        ├─ AR Raycast Manager
│        ├─ AR Anchor Manager
│        └─ AR Light Estimate Manager
├─ Calibration UI
│  ├─ Canvas
│  │  ├─ ReferenceDeer Display (RawImage)
│  │  ├─ Position Slider (X, Y, Z)
│  │  ├─ Scale Slider
│  │  ├─ Rotation Slider
│  │  └─ Confirm Button
│  └─ Status Text
├─ AR Content
│  ├─ ReferenceDeer (GameObject)
│  │  ├─ Mesh Filter: Chair.obj / Deer.obj
│  │  ├─ Mesh Renderer: ワイヤーフレーム
│  │  └─ Transform: ユーザー調整
│  └─ AnatomyModels (親: World Anchor)
│     ├─ BoneModel (GLB)
│     ├─ MuscleModel (GLB)
│     └─ OrganModel (GLB)
└─ Debug UI
   ├─ Distance Indicator (鹿との距離表示)
   ├─ Close Warning (接近警告)
   └─ Calibration Status
```

### スクリプト概要

```csharp
// CalibrationManager.cs
public class CalibrationManager : MonoBehaviour
{
    private GameObject referenceDeer;
    private ARAnchorManager anchorManager;
    private bool calibrationComplete = false;

    // 初回キャリブレーション
    public void StartCalibration(GameObject deerPrefab)
    {
        // SCANIVERSE OBJ をロード
        referenceDeer = Instantiate(deerPrefab);
        // ユーザー調整用ジェスチャーリスナーをアタッチ
    }

    // 調整確定
    public void ConfirmCalibration()
    {
        // iPad の現在位置・回転を取得
        var camTransform = Camera.main.transform;
        
        // World Anchor を設定
        var anchor = anchorManager.AddAnchor(
            new Pose(camTransform.position, camTransform.rotation)
        );
        
        // 骨格・筋肉・臓器を World Anchor の子に配置
        LoadAnatomyModels(anchor);
        
        // リファレンスを非表示化
        referenceDeer.SetActive(false);
        
        calibrationComplete = true;
    }

    private void LoadAnatomyModels(ARAnchor anchor)
    {
        // BoneModel.glb / MuscleModel.glb / OrganModel.glb をロード
        // anchor.transform の子に配置
    }
}

// LayerToggleUI.cs
public class LayerToggleUI : MonoBehaviour
{
    public void ToggleBone(bool active) => boneModel.SetActive(active);
    public void ToggleMuscle(bool active) => muscleModel.SetActive(active);
    public void ToggleOrgan(bool active) => organModel.SetActive(active);
}

// DistanceWarning.cs
public class DistanceWarning : MonoBehaviour
{
    private const float CLOSE_DISTANCE = 1.0f; // 1m 以内は危険

    void Update()
    {
        float distance = Vector3.Distance(
            Camera.main.transform.position,
            referenceDeer.transform.position
        );
        
        if (distance < CLOSE_DISTANCE)
        {
            warningUI.SetActive(true);
            warningUI.GetComponent<Text>().text = 
                $"接近しすぎです\n距離: {distance:F2}m\n推奨: 1-3m";
        }
        else
        {
            warningUI.SetActive(false);
        }
    }
}
```

---

## キャリブレーション手順（運用）

### 初回セットアップ時（展示前）

1. **iPad を起動** → AR アプリを開く
2. **「キャリブレーション開始」ボタンをタップ**
3. **iPad カメラで鹿全体を捕捉** (1-3m 距離)
4. **SCANIVERSE OBJ（リファレンス鹿）が画面に表示される**
5. **ジェスチャーで調整**:
   - 2本指ドラッグ: 位置ズレを修正
   - ピンチ: サイズを調整（実物の鹿の大きさに合わせる）
   - 回転: Y 軸を中心に回転（鹿の向きを合わせる）
6. **「確定」ボタンをタップ**
   - World Anchor が設定される
   - リファレンス非表示 → 骨格・筋肉・臓器が表示開始
7. **「展示開始」ボタンをタップ**
   - UI が簡潔に（ボタンのみ）
   - レイヤー切り替え可能

### 展示中（毎日）

- iPad を立てかけて鹿を指す
- 来場者が iPad でタップして 骨・筋・臓を切り替え表示
- 接近しすぎたら警告表示

### 再キャリブレーション（iPad 再起動時など）

- 「キャリブレーション開始」ボタン → 手順 2-6 を繰り返し

---

## トラブルシューティング

### Q: キャリブレーション後、AR コンテンツが鹿からズレている

**原因**: ユーザー調整が不正確 / World Anchor の設定ミス

**対処**:
1. 「再キャリブレーション」ボタンをタップ
2. 手順を繰り返す

### Q: iPad を動かすと AR が揺らぐ

**原因**: World Anchor ではなく Camera Space で配置されている

**対処**:
```csharp
// 間違い（Camera Space）
anatomyModel.transform.parent = Camera.main.transform;

// 正しい（World Anchor）
anatomyModel.transform.parent = anchor.transform;
```

### Q: 接近警告が出続ける

**原因**: 推奨距離（1-3m）より近い

**対処**:
- iPad を少し離す（1.5m 程度まで下がる）
- または警告の閾値を調整（設定で変更）

---

## P0 検証チェックリスト（椅子）

| # | 項目 | 確認方法 | 期待値 |
|---|-----|--------|--------|
| 1 | キャリブレーション UI が起動 | 「キャリブレーション開始」ボタンをタップ | リファレンス椅子が表示される |
| 2 | ジェスチャー調整が動作 | 2 本指ドラッグ / ピンチ / 回転 | 椅子の位置・大きさ・向きが変わる |
| 3 | 「確定」後、World Anchor が設定 | 確定ボタンをタップ | リファレンス非表示 → Cube 表示 |
| 4 | Cube が椅子に貼りつく | iPad を動かす | Cube が椅子位置から移動しない ✓ |
| 5 | レイヤーボタンが動作（今後実装） | ON/OFF トグル | Cube の表示・非表示が切り替わる |
| 6 | 距離警告が動作 | iPad を鹿に近づける（<1m） | 警告メッセージが表示される |

---

## 参考資料

- **Unity AR Foundation**: https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest
- **AR Anchors**: https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest/manual/anchors.html
- **ARKit 5 Features**: https://developer.apple.com/documentation/arkit

---

**Last Updated**: 2026-07-03  
**作成者**: TQY Kobayashi  
**プロジェクト**: AR Solution 卒業制作支援
