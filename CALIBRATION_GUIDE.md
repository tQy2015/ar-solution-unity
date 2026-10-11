# 座標系の確認とキャリブレーション手順書

更新: 2026-10-11。対象: ar-solution-unity / Unity 2022.3.62f3。

ユーザー確認: **3cmは規定面に合わせるための暫定オフセット**。3cmを認識器の測定誤差や最終校正値と扱わない。本書では規定面を「基準面」と呼ぶ。オフセットの符号・軸・適用階層は実機構成で確認する。

本書は実施手順と必要な実装の設計。校正ツール、GUID別設定、計測ログはまだ実装していない。実機試験も未実施。まず以下の手動手順で基準を決め、その後に対象別設定へ移行する。原因分析は [調査書](AR_ALIGNMENT_INVESTIGATION.md)、再開点は [STATE.md](STATE.md) を参照。

## 1. 作業条件を固定する

1. 使用端末・OS・ビルドのコミット・シーン・Prefab・arobjectファイルを記録する。arobjectのハッシュも保存する。
2. 実機に入っている構成とInspectorを照合する。リポジトリのTest0818には `(0,3,0)`（通常のメートル単位で3m）が残っている。ユーザーが説明した3cmとは別の記録なので、機械的に0.03へ書き換えない。
3. 元のシーン・Prefab・補正値を保存し、診断用コピーで作業する。初めは対象を1種類に限定する。
4. 実物を固定し、照明と背景を一定にする。定規で実寸を測る。実物とモデルに対応する点を6点以上選び、校正用と検証用に分ける。最低3点の非共線点が校正に必要。点は狭い範囲に集中させない。
5. 現在の3cm補正が `contentOffset`、Prefabの子、メッシュ自体、別スクリプトのどこにあるか特定する。複数箇所への適用を確認し、記録する。

完了条件: 使用データと補正箇所が一意に説明でき、元の構成に戻せる。

## 2. 原点・基準面・軸・単位を定義する

対象ごとに次の表を埋め、写真や図で正方向を残す。

| 項目 | 記入内容 |
|---|---|
| 基準面 | 実物の底面、設置台上面など具体的な面。両者の段差も記録 |
| 基準原点O | 基準面上の再現可能な一点。目視の「中心」だけで定義しない |
| +Y | 基準面の法線のうち上向き |
| +X | 特定の部位・稜線などに向かう方向 |
| +Z | 採用した座標系でX/Yと整合する方向。実物写真に記載 |
| スキャン原点R | Scannerで設定した原点位置・向き。底面中心は既定であり実データの保証ではない |
| 表示モデルM | pivot、上向き軸、正面、インポート後の実寸 |
| 単位 | Unity側は本手順では1 unit=1m。30mm=0.03m |

Scannerで原点を再設定する場合は基準点と軸を合わせ、再出力したarobjectを別版として管理する。既存arobjectを維持する場合は、Rから基準原点Oへの差を校正で吸収する。原点を変えて再スキャンしたのに旧校正を流用しない。

## 3. 座標変換を確認する

### 通常のAR Foundation経路

`ARTrackedObject.transform.position/rotation` はUnityのワールドTransformとして使用する。`localPosition/localRotation` は親座標であり、混ぜない。通常のプロバイダー経路に対して、ARKitの右手系を理由にZ反転を追加しない。追加が必要かを調べる対象は、独自に読み込む生の座標やモデルの軸定義である。

1. 診断用の原点と短いXYZ軸を、offsetゼロ・回転identity・scale=1で検出物体の子に表示する。この操作には診断表示の追加が必要。現行の長さ3m・太さ5cmの軸は精密測定に使わない。
2. Camera、Origin、検出物体、Anchor、表示モデルの実行時Transformを記録する。Originを含む祖先scaleを確認し、診断では1に統一する。非一様scale・負scaleは校正前に解消する。
3. 実物の+X/+Y/+Zと表示軸を照合する。左右反転、前後反転、90°回転、単位の100倍差を先に解消する。
4. 対象を90°回し、セッションをリセットして再検出する。モデルが実物と同じ方向へ回ることを確認する。検出後に物体を動かす連続追従テストとは分ける。

### Swift等の生データを独自に取り込む場合のみ

双方の原点・X/Yが一致し、Zの向きだけが逆という基底変更なら、列ベクトル表記で `C=diag(1,1,-1,1)`、点は `pU=C pA`、同じ基底変更を入出力に施す姿勢行列は `TU=C TA C⁻¹`。この条件ではQuaternionの対応は `(x,y,z,w) → (-x,-y,z,w)`（全成分の符号反転も同じ回転）。これは基底変換の式であり、任意のモデル軸や原点の差まで解決する式ではない。

左右の基底が異なる場合は `TU=C_world TA C_local⁻¹` として定義を揃える。単位換算・モデルpivot補正は別途必要。行列のメモリ配置と転置、Quaternionの成分順を確認する。identity、各軸への既知の10cm移動、各軸90°回転で変換を検証し、変換は入力境界で一度だけ行う。`ARTrackedObject`へこの反転を再適用しない。

完了条件: 既知の方向と実寸が一致し、どの入力境界で変換したか説明できる。

## 4. 現行コードで手動校正する

現行SpawnerはPrefabルートの位置をcontentOffsetへ、回転をidentityへ上書きする。このため、PrefabルートのInspectorだけで回転を合わせても実機で保持されない。

1. 診断用Prefabを `CalibrationRoot → VisualModel` の2階層にする。ルートは位置0・回転0・scale=1。表示メッシュはVisualModelに置く。
2. contentPrefabにこのPrefabを指定し、診断用シーンのcontentOffsetを0にする。保存済みの3cm補正がVisualModelやメッシュに残っていないか確認する。元の設定は保存しておく。
3. VisualModelのインポート単位を実寸へ合わせる。まずscaleを決定し、次に回転、最後に位置を調整する。正面だけでなく側面・斜めから照合する。
4. 基準面とモデルの対応点を合わせる。底面中心原点へ中心pivotの高さ20cmのCubeを載せる例では中心を+10cmに置く。これはCubeの例で、対象ごとの値を測る。
5. 3cmが実際の台の厚みなど幾何的に必要な段差なら、その方向と長さを校正変換へ一度だけ含める。単なる目視補正なら多点校正で置き換える。最終結果へ一律+0.03を足さない。
6. 校正に使っていない点で誤差を測る。一点が合って遠い点ほどずれる場合は回転・scale・モデル形状を再確認する。

この手動方式は対象1種類の検証用。現行SpawnerにはGUID別切替がないため、複数対象へ同じ値を適用しない。

## 5. 多点校正と対象別データへ移行する（要実装）

列ベクトル・右から作用する変換として、目標は `T_world_model = T_world_reference × T_reference_model`。追加の基準面Oを使うなら `T_reference_model = T_reference_O × T_O_model`。

1. 単位・モデル軸を正規化したモデル点mと、対応する実物点rを集める。rは参照物体座標系で表す。実物点の取得には測量した治具や校正UIなどが必要で、現在のアプリには用意されていない。
2. Unityでワールド点を取得する場合は同フレームの参照Transformの `InverseTransformPoint` でrへ変換する。点を測っている途中に参照姿勢が変わった場合は再取得する。ARの計測値を独立した真値とはみなさない。
3. `r ≈ s R m + t` の残差二乗和を最小化する。まず実寸を用いたs=1の剛体合わせ、寸法差が確認された場合だけ一様scaleを許す。回転はdet(R)=+1に制約し、鏡映で誤差を隠さない。
4. 得られたt、Quaternion、sをGUID別設定へ保存する。arobjectを差し替えたときはハッシュ照合で再校正を要求する。TrackableIdは実行時インスタンス管理に使う。
5. Prefabの軸・単位補正とフィット結果を二重適用しない。どの階層にどの変換を適用したか保存する。
6. 別点・別開始方向・別セッションで検証してから本番へ反映する。

保存する記録のひな形（未計測値を0で埋めない）:

```text
target_name / reference_guid / arobject_sha256:
build_commit / scene / prefab / package_versions:
origin_definition / reference_plane / axis_definition:
model_units / import_axis_correction:
temporary_offset_before: 0.03 m（軸・符号・適用箇所は要記入）
calibrated_translation_m / quaternion_xyzw / uniform_scale:
applied_transform_node / calibration_version / date:
calibration_points / held_out_points / measurement_method:
median_error_mm / p95_error_mm / detection_failures:
```

## 6. 実機で合否を確認する

1. 固定した実物を同一条件で10回独立検出する。毎回セッション・生成物を初期化し、認識直後/1秒/5秒/30秒を記録する。
2. 同じ校正で開始方向を変えて再検出する。正面・側面の未使用点を測り、一定方向のずれと試行ごとのばらつきを分ける。
3. 対象固定で端末を一周させて戻し、誤差の増分を測る。認識時の誤差と移動後の誤差を混同しない。
4. 固定誤差が減ってもばらつきが残る場合は、現行の初回Anchor固定とARTrackedObjectへの直接表示を比較する。比較用実装が必要。既存AnchorのTransformを毎フレーム上書きしない。
5. 画面外からの再検出で二重表示・古い表示が残らないことを確認する。

暫定目標: 未使用点の中央値10mm以下、十分な試行数でP95 20mm以下、端末一周後の誤差増分10mm以下。用途に合わせて実験前に目標を確定する。10回は予備試験とし、採用判断は20〜30回で再確認する。未検出率・検出時間も併記する。

単眼の画面ずれはpixelまたは同じ奥行きの定規による面内mmとして記録する。3D誤差を主張するには奥行きも評価できる独立した計測が必要。実験の詳細は[調査書の実験計画](AR_ALIGNMENT_INVESTIGATION.md#4-実験手順)を参照。

## 7. 参照仕様

- [Unity AR Foundation 5.2 Object tracking](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.2/manual/features/object-tracking.html): 参照物体と検出オブジェクトの関係。
- [Unity Anchors](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1/manual/features/anchors.html): 自動更新される姿勢・pending・アンカーの扱い。
- [Apple Scanning and Detecting 3D Objects](https://developer.apple.com/documentation/arkit/scanning-and-detecting-3d-objects): スキャン原点と参照データ。

基底変換式は本書で定義した軸に対する数学的変換。プロバイダーへのパッチ指示ではない。実装時は実際に解決されたパッケージと実機の軸表示で確認する。
