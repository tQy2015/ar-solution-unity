# AR Foundation + ARKit — 技術ノート（Unity→Xcode→iPad実装の詰まりどころ）

**目的**: Unity編集→Xcodeビルド→iPad実機テストの過程で実際に遭遇した、再利用可能な技術知識を集約する。
日付入りの意思決定の経緯・実施記録（「いつ何を試したか」）は教員側の管理リポジトリ
（`melon-active/ar-solution/docs/UNITY_AR_BUILD_RUNBOOK.md`）が正典。ここには
「次に同じ問題に当たった時にすぐ使える知識」だけを置く。

---

## 1. Unity側シーン構築の落とし穴

### Editorスクリプトを書く際の注意

| 落とし穴 | 症状 | 対処 |
|---|---|---|
| `??` / `?.` を `UnityEngine.Object` に使う | `GetComponent<T>() ?? AddComponent<T>()` が意図通り動かず、後続で `MissingComponentException` | 必ず `if (x == null) x = AddComponent<T>();` の明示的null判定にする。UnityのObjectはオーバーロードされた`==`があるが`??`/`?.`はそれをバイパスするため、破棄済み/未初期化オブジェクトを「非null」と誤判定することがある |
| `UnityEvent.AddPersistentListener` にラムダ式を渡す | `ArgumentException: The class null does not derive from UnityEngine.Object` | 永続リスナーは実在するMonoBehaviourの**引数なしpublicメソッド**しか登録できない。ラムダの代わりに橋渡しメソッドを用意する |

---

## 2. iOS Player Settings チェックリスト

**この3点を必ず設定する。未設定だとビルド後にクラッシュ・拒否される:**

| 項目 | 未設定時の症状 | 設定値の目安 |
|---|---|---|
| **Bundle Identifier**（Other Settings） | Xcodeビルド不可。**教員の検証用と自分のビルドで同じ値のままだと衝突する**（詳細: `./XCODE_DEPLOYMENT_TROUBLESHOOTING.md` 問題5） | 逆ドメイン形式・自分専用のユニークな値にする |
| **Camera Usage Description**（Other Settings） | **ARKit起動時に即クラッシュ**（NSCameraUsageDescription欠如） | 日本語で「ARで○○を表示するためにカメラを使用します」等、空文字列は不可 |
| **Target minimum iOS Version**（Other Settings） | 低すぎるとインストール済みXcodeのSDKがサポート対象外として警告（詳細: `./XCODE_DEPLOYMENT_TROUBLESHOOTING.md` 問題6） | インストール済みXcodeがサポートする範囲内（2026年時点で`15.0`程度） |

Bundle ID・署名（Team選択）はXcode側でも設定するので二重管理になるが、
**Camera Usage Descriptionは必ずUnity側で設定する**（Xcode側のInfo.plistはUnityの出力を毎回上書きするため）。

---

## 3. XR Plug-in Management（★最重要・見落としやすい）

**Edit → Project Settings → XR Plug-in Management → iOS タブ → Plug-in Providers → ARKit にチェック**

このチェックを入れないと何が起きるか:

```
症状: Xcodeビルド時に大量のリンクエラー
  Undefined symbol: _UnityARKit_refPoints_acquireChanges  ... (計8個程度)

原因: ARSession・ARCameraManager等のC#コンポーネントをシーンに置いても、
      XR Plug-in Management で ARKit ローダーを「有効化」していないと、
      Unityはネイティブプラグイン libUnityARKit.a を
      Xcodeプロジェクトの Libraries/ にコピーしない。
      一方でC#側（IL2CPP生成コード）はそのネイティブ関数をシンボル参照するため、
      リンク時に「定義がない」エラーになる。

確認方法: Assets/XR/XRGeneralSettingsPerBuildTarget.asset を開き、
          Values: [] になっていたら未設定（iOSタブでARKitを有効化すれば
          m_Loaders にARKitLoaderが追加され、Values に iPhone Settings が入る）

対処: XR Plug-in Management → iOS → ARKit にチェック → 再度 Build
```

---

## 4. TrackedPoseDriver — iPad実機でカメラ姿勢が更新されない問題

**症状**: AR Anchorを使った座標固定を実装しても、iPadを動かすとコンテンツが画面に張り付いたまま
（＝ワールド座標固定ではなくカメラ座標固定になっている）。

**原因**: AR CameraにInput Systemの`TrackedPoseDriver`コンポーネントが無いと、`Camera.transform`は
起動時のデフォルト値`(0,0,0)`/回転なしのまま一度も更新されない。

`Unity.XR.CoreUtils.XROrigin`（`ARSessionOrigin`の実体）はAwake時に
`Camera "AR Camera" does not use a Tracked Pose Driver (Input System)...`という警告を出すが、
**これは「コンポーネントの有無」しか見ておらず、バインディングが実際に機能しているかは見ていない**。
誤ったバインディングでも警告は消えるため、警告消失を「直った」の判定に使うと誤判定する。

**さらにハマりやすい点**: `<XRHMD>/centerEyePosition`・`<XRHMD>/centerEyeRotation`だけに
バインドしても直らない。**iPad実機のARKitハンドヘルドARは`XRHMD`デバイスを一切生成しない**
（`XRHMD`はVR/ARヘッドセット向けのレイアウト）。ARKitが実際に登録するのは
`HandheldARInputDevice`（product `"(ARKit)"`）で、姿勢データはその`devicePosition`/`deviceRotation`に流れる。

**正しい設定**（Unity公式の「XR Origin (Mobile AR)」メニューが内部で実行するコードと同一）:

```csharp
var positionAction = new InputAction("Position",
    binding: "<XRHMD>/centerEyePosition", expectedControlType: "Vector3");
positionAction.AddBinding("<HandheldARInputDevice>/devicePosition");
var rotationAction = new InputAction("Rotation",
    binding: "<XRHMD>/centerEyeRotation", expectedControlType: "Quaternion");
rotationAction.AddBinding("<HandheldARInputDevice>/deviceRotation");
trackedPoseDriver.positionInput = new InputActionProperty(positionAction);
trackedPoseDriver.rotationInput = new InputActionProperty(rotationAction);
```

**`<XRHMD>`と`<HandheldARInputDevice>`の両方を同一アクションにバインドする**
（エディタ/他のXR環境用に`XRHMD`も残しつつ、iOS実機では`HandheldARInputDevice`側が解決する二重バインド構成）。

**Editorスクリプトを直しただけでは直らない**: すでにシーンに保存済みの設定（シーンファイルに
シリアライズされた古いバインディング）は、メニューを再実行してシーンへ反映・保存し直すまで
そのまま使われ続ける。Editor拡張のコードとシーンファイルの実データは別物であることを常に意識する。

---

## 5. World Anchor（座標固定）実装の注意

- `ARAnchorManager.AttachAnchor(ARPlane, Pose)`は**ARPlane専用**のオーバーロードしか存在せず、
  `ARTrackedObject`には使えない
- AR Foundation 5.2.2の`ARAnchorManager.AddAnchor(Pose)`は`[Obsolete]`属性付きで、
  そのメッセージが`AddComponent<ARAnchor>()`を使うよう指示している。つまり
  **空GameObjectにPoseをセット→`AddComponent<ARAnchor>()`が正式な新規アンカー生成手段**
  （`ARAnchorManager.AddAnchorAsync(Pose)`という非同期APIは存在しない）
- `ARAnchor.OnEnable()`が`AddComponent`と同一フレームで同期的に`ARAnchorManager.TryAddAnchor(this)`を
  呼ぶため、`AddComponent`前にGameObjectのTransformを目的のPoseへセットしておけば正しく登録される

---

## 6. `contentOffset`（検知座標と見た目中心のズレ）

**検知座標は対象の見た目中心と一致しない（バグではなく仕様）**: ARKit Object Detectionの
検知座標（`trackedObject.transform.position`）が指すのは、スキャン時にScanningAppが記録した
「参照オブジェクトの原点」であり、既定ではバウンディングボックスの**底面中心**（対象の視覚的な
中心ではない）。

AR Foundation 5.2.2は`ARReferenceObject`の`center`/`extent`をAPIとして公開していないため
（`XRReferenceObject`は`name`/`guid`のみ）、プログラムから自動補正する手段は無い。
`ObjectDetectionSpawner`の**`contentOffset`（`SerializeField`のVector3）**を対象ごとに
実機で見ながらInspectorで手動調整する。**対象を切り替えるたびに再調整が必要**。

実際の`.arobject`ファイルはZIPアーカイブで、`Info.plist`に`ReferenceOrigin.rotation/translation`が
平文で入っている（回転は常にY軸=上下方向の軸で、角度（Yaw）はスキャンごとに無作為という傾向が
複数サンプルで確認されている）。確認方法:
```bash
unzip -o <対象>.arobject -d /tmp/arobject_inspect
plutil -p /tmp/arobject_inspect/Info.plist
```

**切り分けに使える手法（「検知はしたが表示されない」系バグの再利用可能な手順）**:
1. カメラ前方固定でプレハブを1個だけ強制表示 → レンダリングパイプライン自体が生きているか確認
2. Anchor原点を中心に小キューブを立体グリッド状にばらまく →
   「カメラが対象の内側に入り込み裏面カリングで見えない」ケースを排除できる
3. 軸を色分けしたセグメント（グラデーション）で可視化 → どのセグメントまで見えるかでズレの方向・量を特定

---

## 7. 実機ビルドだけでランタイム生成マテリアルがピンク（マゼンタ）になる

**症状**: `GetComponent<Renderer>().material.color = color;`でランタイム生成した色付きマテリアルが、
Unity Editor上のPlayモードでは正しく表示されるのに、iOS実機ビルドだけピンクになる。

**原因**: ピンクはUnityの「シェーダーが見つからない」を示す既知の表示。**実機ビルドでは、
プロジェクト内のどのアセットからも静的参照されていないシェーダーはビルド時にストリップ(除外)される**。
ランタイムで`new Material(...)`しても、ストリッパーの判定には影響しない。

**対処**（3段構え。うち1つで十分だが安全のため重ねられる）:
1. 実プロジェクトアセットとして`.mat`ファイルを作成し、シーン上のコンポーネントから参照させる
2. キーワード分岐(`multi_compile`等)を一切持たない最小の自作シェーダーを使う
3. `ProjectSettings/GraphicsSettings.asset`の`m_AlwaysIncludedShaders`に直接登録する

---

## 8. デバッグギズモが「見えない」＝実装バグとは限らない

ワールド座標に軸ギズモ（X/Y/Z）を表示する実装で「X軸だけ見えてY/Z軸が見えない」症状が出ても、
コードは正しいことがある。**軸はワールド座標系の絶対方向を向いているため、iPadをどの角度で
構えているかによってはY軸やZ軸が画面外・エッジオン（細長い方向を正面から見て点にしか見えない）
になる**——実装バグではなくカメラの向きの問題。

**対策**: アンカー原点に**必ず全方位から見える中心マーカー（球など）**を併置しておくと、
「そもそもアンカーがそこに固定されているか」自体は軸の向きに関係なく判定できる。

---

## 9. Strip Engine Code・ビルド出力先クリアに関する注意

- **`Strip Engine Code`**（Player Settings → iOS → Other Settings → Optimization）が有効だと、
  `CreatePrimitive(PrimitiveType.Sphere)`が自動追加する`SphereCollider`等のPhysics関連クラスが
  実機ビルドから欠落し、`Can't add component because class 'SphereCollider' doesn't exist!`の
  ような例外が出ることがある。ランタイムにコンポーネントを動的追加するデバッグコードを書く場合は要注意
  （プレハブに静的にシリアライズされたコンポーネントは影響を受けない）
- **ビルド出力先の上書きで`IOException: Directory not empty`が発生することがある**:
  Unityの`iOSBuildPostprocessor`が既存出力の削除に失敗する既知の不具合（`.DS_Store`等の
  拡張属性が残っているディレクトリで起きやすい）。対策として`IPreprocessBuildWithReport`を
  実装し、ビルド直前に出力先ディレクトリの中身を強制削除するEditorスクリプトを用意できる
  （本プロジェクトでは`Assets/Editor/CleanBuildOutputPreprocessor.cs`が対応済み）

---

## 参考

- CLI経由でのビルド自動化（`-executeMethod`方式、本ドキュメント§4〜6の実例もこの方式で検証済み）:
  本リポジトリの`Assets/Editor/*BuildSetup.cs`を参照。「Unity CLI」という言葉が指す3つの別物
  （Editor batchmode / Unity公式CLI / Unity CLI Loop）の整理は`melon-active/MCP/CLAUDE.md`参照
- Xcode署名・デプロイのトラブルシューティング: `./XCODE_DEPLOYMENT_TROUBLESHOOTING.md`
- ARKit Object Detectionの対象物選定・照明条件等: `./ARKIT_OBJECT_DETECTION_REFERENCE.md`
- 日付入りの実施記録・意思決定の経緯（教員側正典）: `melon-active/ar-solution/docs/UNITY_AR_BUILD_RUNBOOK.md`

---

**最終更新**: 2026-10-22
