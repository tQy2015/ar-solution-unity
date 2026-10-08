# AGENTS.md

> Agent向け索引。人間向け手順書は不要——このファイルの指示に従って作業すること。
> 人間向けステップガイドは `README.md`（通常は読まなくてよい）。

## Scope

Unity製iOS ARアプリ。ARKit Object Detectionで対象物を認識し、認識位置に解剖モデル（骨格/筋肉/臓器）をワールド固定表示する。マーカーレス。手動アラインメント操作UIは無し（旧実装はコード上に残存、§Code mapを参照）。

姉妹リポジトリ: [ar-solution-scanner-app](https://github.com/tQy2015/ar-solution-scanner-app) — `.arobject`生成元。本リポジトリ単体では新規対象物を追加できない。

## Environment

| 項目 | 値 | 確認コマンド |
|---|---|---|
| Unity | 2022.3.62f3 LTS 固定 | `cat ProjectSettings/ProjectVersion.txt` |
| AR Foundation | 5.1.0 | `Packages/manifest.json` |
| ARKit XR Plugin | 5.2.2 | 同上 |
| Build target | iOS実機のみ | Simulatorはビルド不可（ARKitネイティブ依存） |
| 署名 | Xcode Personal Team | 7日で失効。再Runで復帰 |

パッケージはmanifest.json記載済み。Editorで開けば自動解決— Package Manager操作不要。

## Build task

```
Unity:  File→Build Settings→iOS→Switch Platform→Scenes in Build に Assets/Scene/ 内最新日付シーン追加→Build→Builds/iOS/
Xcode:  open Builds/iOS/Unity-iPhone.xcodeproj→Signing & Capabilities→Team選択→実機選択(Simulator不可)→Run
```

## Code map

| ファイル | 役割 | 触るとき |
|---|---|---|
| `Assets/Scripts/AR/ObjectDetectionSpawner.cs` | 検知イベント→ARAnchor生成→コンテンツInstantiate。現行の中核 | 検知後の表示ロジックを変える時 |
| `Assets/Scripts/AR/CalibrationController.cs` | 旧・手動アラインメント方式。**死コード、現行シーンから未参照** | 触らない（削除予定なし、参照目的のみ） |
| `Assets/Scripts/AR/LayerToggleController.cs` | 骨格/筋肉/臓器レイヤーON/OFF | レイヤーUIを変える時 |
| `Assets/XR/*ReferenceObjectLibrary.asset` | `.arobject`登録先 | 対象物を追加/入替する時。`ARTrackedObjectManager`側の参照も更新必要 |

## Known unsolved: contentOffset

検知座標 = スキャン時の参照オブジェクト原点（既定: バウンディングボックス底面中心）≠ 対象物の見た目中心。差分はAR Foundation 5.2.2では自動取得不可（`ARReferenceObject.center/extent`非公開）。

対応: `ObjectDetectionSpawner.contentOffset`（Vector3, Inspector）を対象物ごとに実機調整——自動化手段は存在しない。対象物を追加・入替するたびに再発するタスクとして扱うこと。実機ログの`center`/`extent`出力を調整の手掛かりにする。

## License

Educational - Osaka University of Arts
