# AR位置ずれの調査と実験計画

調査日: 2026-10-11。Unity `2cb70c2`、Scanner `61cb146` のローカルソースを対象に静的調査。実機計測・ビルドは未実施。実装は変更していない。

追記（ユーザー確認）: **3cmは規定面への暫定オフセット**であり、認識精度の測定結果とは扱わない。座標系の確認・校正の実施手順は [CALIBRATION_GUIDE.md](CALIBRATION_GUIDE.md)、再開点は [STATE.md](STATE.md)。以下の3mは保存されたTest0818の値であり、この3cmと同一視しない。

最有力は参照オブジェクト原点と表示モデル座標系の不一致。次に初回検出姿勢の採用方法、スキャン品質、ワールドトラッキングを切り分ける。現在のビルド設定は3m上にCubeを出すデバッグ構成なので、実機で観察した数cmのずれを、このチェックアウトだけで確定診断することはできない。

## 1. 確認できた事実

ソースパスはUnityリポジトリ相対。Scannerのパスは `../ar-solution-scanner-app/` 相対。

| 根拠 | 確認内容 | 意味 |
|---|---|---|
| `ProjectSettings/EditorBuildSettings.asset:9`、`Assets/Scene/Test0818.unity:571` | 有効シーンはTest0818。contentPrefabはTargetCube、contentOffsetは(0,3,0) | メートルスケールでは3m上。数cmの現象とのビルド対応を最初に確認 |
| `Assets/Scene/Test0807_ObjectDetection.unity:471` | 旧シーンのoffsetは(0,0.097,0) | 過去に9.7cm補正が存在。どの対象に正しい値かは未確認 |
| Scanner `ScanningApp/3D UI and Visualizations /ScannedObject.swift:175` | 未調整の原点をbbox底面へ移動 | 幾何中心・モデルpivotとは通常一致しない |
| Scanner `ScanningApp/Controllers/Scan.swift:433` | bboxから抽出し `applyingTransform(origin.simdTransform)` を適用 | 原点操作の位置・向き・条件によってはスケールが出力に関係する |
| `Assets/Scripts/AR/ObjectDetectionSpawner.cs:109` | PrefabのlocalPositionをoffset、localRotationをidentityに上書き。scaleは明示補正しない | Prefabルートに保存した位置・回転調整は失われる |
| `Assets/Models/TargetCube.prefab:33` | Cubeのscaleは0.2、pivotは中心 | 底面を合わせたいなら中心を上に10cm置く必要がある。これは例であり他モデルへ流用不可 |
| `Assets/XR/KettleReferenceObjectLibrary.asset:17`、Spawner:30 | kettle、bottle、banana、Miffyの4対象に対しPrefab/offsetは1組 | 対象ごとの校正を保持できない |
| Spawner:58、98 | addedで即座に姿勢をコピーして別ARAnchorを作る | 初回姿勢の誤差を物体検出の後続情報で修正する処理がない |
| Spawner:63 | updatedは表示ON/OFFのみ | 検出物体と追加アンカーの相対姿勢は監視・修正されない |
| Spawner:74 | removedで非表示にして辞書とアンカーを保持 | 同IDのaddedならSpawnは早期return。新IDなら古いアンカーが残る |
| Spawner:119 | 長さ3m、太さ5cmのY軸を生成 | 数mm〜cmの判定用表示として粗い。非表示処理もcontentのみで軸は別子オブジェクト |
| Scanner `Controllers/Scan.swift:396`、`:122` | 低品質判定は特徴点100未満 | 100点以上で位置精度が保証されるわけではない |
| Scanner `Controllers/TestRun.swift:78` | 検出時間を集計し、検出アンカーを除去して再検出 | 既存テストの主指標は検出速度。位置・角度誤差の定量評価ではない |
| `Packages/manifest.json:3`、`packages-lock.json:35` | manifestのAR Foundationは5.1.0、lockは5.2.2。ARKitは5.2.2でFoundation 5.2.2へ依存 | 実ビルドの解決バージョンを記録する。混在記述だけで誤差原因とは断定しない |

TargetCube Prefabに保存されている約1mのZ位置はSpawnerが上書きするため、その値をそのまま実行時ずれの原因とはしない。シーンのOrigin/Cameraの保存scaleは1。CameraYOffset=1.1176、CameraFloorOffsetObject未指定もあるが、実行時のOrigin変換を確認せずに1.1176mの誤差とは判断しない。

READMEの「center/extentログ」は現行Spawnerに存在しない。現在出るのは検出名、アンカー位置、offset、scaleなどであり、調査には追加の計測が必要。

## 2. 原因候補と症状

| 優先度 | 仮説 | 予測される症状 | 改善 |
|---|---|---|---|
| 高 | スキャン原点とモデルpivotの不一致 | 対象座標系で毎回ほぼ同じ方向・量のずれ | 対象別の位置・回転・縮尺校正 |
| 高 | 軸方向・回転・モデル寸法の不一致 | 一点は合うが遠い部位ほど外れる | 複数ランドマークで回転・寸法を合わせる |
| 中〜高 | 初回検出姿勢を追加アンカーに採用 | 検出ごとに位置が違う、検出側の姿勢が変わっても表示が修正されない | 直接表示との比較、安定化後のアンカー生成 |
| 中 | 特徴点の分布不足、反射、対称性、変形、周囲の混入 | 視点・照明で誤差や未検出率が変わる | 硬質で模様のある対象、固定した状態で複数方向を再スキャン |
| 中 | カメラの自己位置推定・再ローカライズ | 端末を一周させる、遮る、時間をおくとずれる | 環境特徴確保、tracking状態の可視化、再検出による再配置 |
| 条件付き | 原点スケールと表示モデルの不一致 | カスタムモデルをScannerで読み込んだときに寸法が変わる | 原点のscaleとUnityモデル単位を照合 |
| 条件付き | 対象物自体を検出後に動かした | 表示が古い場所に残る | 静止展示を前提にするか再検出・再配置の操作を用意 |

半径rの場所で角度誤差θがあると、おおよそ rθ の位置誤差になる。原点から50cm離れた部位では2°で約17mm。縮尺5%の誤差でも50cm先では25mm。数cmという大きさだけでは平行移動誤差と断定できない。

ARAnchorは空間の推定位置を維持・更新する仕組みで、最初の物体認識姿勢を真値へ校正するものではない。別アンカーを作ることが常に精度を改善するとは限らない。逆に、ARTrackedObjectを親にすれば動く実物へ必ず連続追従する、とも仮定しない。対象の姿勢更新が実際に提供されるかをログで確認する。

## 3. 改善設計

### A. 対象別の校正データ（最優先）

`referenceObject.guid` をキーに、Prefab、localPosition、localRotation、uniformScale、arobjectのハッシュ、校正版を保存する。名前は表示用とし、TrackableIdは検出インスタンスの管理に使う。同一参照物体が複数存在しうる場合、GUIDだけでインスタンスを統合しない。

階層を `検出物体またはAnchor → CalibrationRoot → VisualModel` とする。CalibrationRootで校正を適用し、VisualModelにインポート由来のpivot/軸補正を保持する。モデル固有の単位変換と校正scaleを二重適用しない。

目的の変換は `T_world_visual = T_world_reference × T_reference_visual`。現行は主に後半の平行移動しか扱っていない。物理側とモデル側の対応点を最低3点、推奨6点以上、広く非共線に選び、剛体変換を最小二乗推定する。寸法が違う場合に限り一様scaleも推定する。校正点以外の点で評価し、モデル形状自体の違いを無理な非一様scaleで隠さない。

center/extentが分かっても任意の解剖モデルとの対応は決まらない。既存文書の「自動化手段なし」は現行実装の説明に留まり、対応点・治具・付随メタデータによる校正の自動化は設計可能。Scannerは無改造配布という運用に合わせ、まずUnity側に校正情報を保存する。

### B. 姿勢の採用方法

診断モードを3種類用意する。

1. 現行方式: added時に別アンカーを作る。
2. 直接方式: ARTrackedObject配下へ表示し、別アンカーを作らない。
3. 安定化方式: sessionとobjectの状態を記録し、姿勢が一定時間安定した後にアンカーを作る。

3の初期検討値は0.5〜1秒、位置変動5mm以下・角度変動1°以下。これは精度保証でも最終仕様でもない。同じ姿勢を繰り返し読むだけなら検出精度の改善根拠にはならない。更新がない場合は待機平均ではなく、明示的な再検出を別実験にする。タイムアウトを設けて状態を表示する。

既存ARAnchorのtransformを毎フレーム直接上書きしない。再校正は表示用の子を補正するか、ライフサイクルを管理してアンカーを作り直す。pending/追跡状態/作成失敗も確認し、added直後のtrackableIdだけを成功の証拠にしない。

### C. 再検出とログ

removed/None/Limited/再検出時の表示と破棄の方針を明示する。セッション再開時も辞書・アンカーを整理する。リセットごとに過去の非表示アンカーを残さない。Limitedを即削除するのではなく、計測中は低信頼状態として記録する。

記録項目: ビルドSHA、シーン、実解決package版、端末/OS、arobjectハッシュ、reference GUID/name、TrackableId、単調時刻、event種別、session/object/anchorのtracking状態、anchor.pending、camera/Origin/object/anchor/contentの位置・Quaternion・lossyScale、補正値、FPS、再検出回数。姿勢は同じ座標系・同じフレームで採取し、object→anchor相対変換も保存する。

## 4. 実験手順

### 共通の計測条件

実物を固定し、既知寸法の硬質物体を対照として本番対象と別に用意する。実物上のランドマークは既存の角などを使い、マーカー等を追加する場合は認識の手掛かりが変わるため条件を統一する。正面・側面・斜めから確認できる固定撮影位置を決め、距離、照明、背景、開始方向を記録する。

表示は小さな原点マーカー、XYZ軸、対応点、ワイヤーフレームを使う。現行の太さ5cm・長さ3mの軸では精密判定しない。画面上のずれはまずpixel単位で記録し、物体面と同じ奥行きの既知寸法から局所mm換算する。単眼スクリーンショットだけで3次元誤差と呼ばない。奥行き方向は別角度・測量済み治具・独立した外部計測で検証する。AR自身の座標だけを正解として使うと共通ドリフトを見落とす。

各条件10試行を予備実験、差があれば20〜30試行で再確認。毎試行セッションと生成物を初期化し、比較方式の実行順を入れ替える。検出不能も失敗として集計する。条件間で補正値を調整し直して誤差を消さない。

| 実験 | 操作 | 測定・判定 |
|---|---|---|
| E0: ビルド同定 | 実機の機種/OS、SHA、シーン、Prefab、offset、登録対象を確認。現行ビルドを保存し、診断用シーンを別作成 | Test0818の3m設定と観察が一致するか。未一致なら実機に対応するソースを確保してから再診断 |
| E1: 固定誤差/ばらつき | 同一対象・同一arobject・端末固定で10回独立検出。検出直後/1秒/5秒/30秒に保存 | 各対応点の符号付き平均誤差と試行間SD。同方向の平均誤差が大きければ校正優先。ばらつきが大きければ検出品質優先 |
| E2: 位置/回転/scale | 3点以上で校正、別の3点以上で検証。原点近くと遠く、正面と側面を含める。実物を90°回しセッションを初期化して再検出 | 平行移動だけで全点が改善するか。距離と共に増える誤差は回転/scale/形状を疑う。実物と一緒に誤差方向が回るか |
| E3: 表示方式A/B/C | 同じ校正で上記3方式を比較。可能なら同一セッションで検出原点とアンカー原点を色別表示 | object-anchor間の位置/角度差と、独立した実物基準への誤差を両方計測。直接方式のみ改善なら追加アンカー経路が有力 |
| E4: スキャン比較 | 同じ実物・原点定義で既存scanと新scanを比較。新scanは照明/視点等を一要因ずつ変える。Scanner自身のテスト表示も観察 | Scannerでも不安定ならUnityだけの原因ではない。ただし両アプリの表示基準・検出反復方式の差を考慮。検出速度と位置精度を別評価 |
| E5: ドリフト | 対象固定。端末固定30秒、一周して開始位置へ戻る、カメラ遮蔽して戻る、を別条件で実行 | 初期誤差からの増分、戻った直後/5秒後/30秒後の誤差、session tracking状態。ワールド座標変化だけで判定しない |
| E6: 再検出 | 対象を画面外にして戻す、セッションを再開する。対象移動は独立条件で実施 | 古い表示の残存、二重表示、ID変化、アンカー数、再配置誤差。対象移動への連続追従は現行方式の前提にしない |

初日の推奨順序は E0 → E1 → E2 → E3。平均誤差が減ってもばらつきが残る場合にE4、移動後だけ悪化する場合にE5を優先する。最後にE6で運用上の退行を確認する。

### 集計と暫定合格条件

初期登録誤差、試行間ばらつき、表示の時間方向の揺れ、移動後のドリフト、検出時間、未検出率を分ける。ランドマーク誤差は中央値・最大値・十分な試行数でのP95を報告し、平均ベクトルとばらつきも残す。少数試行のP95は参考値とする。

暫定目標例: 未使用の評価点で中央値10mm以下、P95 20mm以下、端末一周後の誤差増分10mm以下、30秒以内の検出成功率95%以上。用途に応じて変更する目標値で、ARKitの保証値ではない。計測系自体の誤差は目標より十分小さく（目安3mm以下）し、満たせなければmm精度の合否を出さない。改善採用時は同じ未使用条件で誤差低減と認識率・待ち時間の悪化有無を確認する。

## 5. 仕様の確認先

- Apple [Scanning and Detecting 3D Objects](https://developer.apple.com/documentation/arkit/scanning-and-detecting-3d-objects): 参照物体の原点に基づく検出とスキャン手順。`.arobject`は認識用データであり、表示用メッシュとの整合は別に必要。
- Unity [ARKit 5.2 Object tracking](https://docs.unity3d.com/Packages/com.unity.xr.arkit@5.2/manual/arkit-object-tracking.html): `.arobject`をプロバイダー用エントリーとしてライブラリへ登録する仕様。
- Unity [AR Foundation 5.2 Object tracking](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.2/manual/features/object-tracking.html): 参照ライブラリとARTrackedObjectの関係。
- Unity [AR Foundation 5.1 Anchors](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1/manual/features/anchors.html): anchor姿勢の自動更新、pending、transformを直接変更しないという指示。参照したページは5.1系列であり、実装時は実解決5.2.2でもAPIを照合する。

実機の使用シーン、対象物、ずれの方向・再現性、Scannerでカスタムモデルを使ったかは未確認。コード上の構造的な問題と、今回の現象の原因確定は区別する。
