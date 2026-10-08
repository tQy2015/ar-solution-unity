# 学生向けガイド — AR拡張展示 Unityプロジェクト

このガイドは、Unity・Xcode実機ビルドが初めての人向けの手順書です。技術仕様・コード構成だけを見たい場合（AIツールでのセットアップなど）は `AGENTS.md` を参照してください。

このプロジェクトは2つのリポジトリで1組です:

| | |
|---|---|
| **ar-solution-unity**（このリポジトリ） | AR表示アプリ本体 |
| [ar-solution-scanner-app](https://github.com/tQy2015/ar-solution-scanner-app) | 対象物スキャン用の別アプリ |

展示に新しい対象物（鹿の剥製など）を追加するときは、必ず **scanner-appでスキャン → .arobjectを書き出す → このUnityプロジェクトに取り込む** の順番になります。scanner-app側の手順はそちらのリポジトリの`README.md`を見てください。

---

## 0. 準備するもの

- Mac（Xcodeが動くこと。Apple Silicon/Intelどちらでも可）
- Apple ID（無料のものでOK。ただし**Personal Team署名は7日ごとに期限切れ**になる — 展示当日に動かない事故を避けるため、展示前には必ず動作確認を)
- iPad（iPadOS 16以降推奨）
- USB-C/Lightningケーブル

---

## 1. Unity をインストールする

1. [Unity Hub](https://unity.com/download) をダウンロード・インストール
2. Unity Hubを起動
3. 左メニュー **Installs** → **Install Editor**
4. バージョン `2022.3.62f3` を選択（検索に出ない場合は[公式アーカイブ](https://unity.com/releases/editor/archive)の該当バージョンから直接インストール）
5. モジュール選択画面で **iOS Build Support** に必ずチェック
6. インストール開始（30分〜1時間、Wi-Fi速度に依存）

確認コマンド（ターミナル）:
```bash
ls /Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity
```
パスが表示されればOK。

**よくある失敗**: iOS Build Supportのチェックを忘れると、後でビルドができない。Unity Hub → Installs → 該当バージョンの歯車アイコン → Add Modules で後から追加も可能。

---

## 2. プロジェクトを取得する

```bash
git clone https://github.com/tQy2015/ar-solution-unity.git
cd ar-solution-unity
```

Unity Hub → **Open** → cloneしたフォルダを指定。初回に「このバージョンでは開けません」等の警告が出た場合は、Unity Hubで該当バージョン(`2022.3.62f3`)がインストール済みか確認してください。

プロジェクトが開くと、Unityが自動でパッケージ（AR Foundation / ARKit XR Plugin）を解決します。進捗バーが出て数分固まったように見えても、閉じずに待ってください。

---

## 3. iPadを使えるようにする（初回のみ）

これは「アプリをインストールする許可」をiPad側で得る作業です。順番を守ってください。

| # | タイミング | 操作 |
|---|---|---|
| 1 | MacにiPadを初めてUSB接続した時 | iPad画面に「このコンピュータを信頼しますか？」→ **信頼** → パスコード入力 |
| 2 | Developer Mode（iOS 16以降は必須） | iPad: 設定 → プライバシーとセキュリティ → **Developer Mode** を **ON** → 再起動 → パスコード入力 → 「デベロッパモードをオンにする」を確認 |
| 3 | Xcodeで初回Run後、アプリが起動しない時 | iPad: 設定 → 一般 → **VPNとデバイス管理** → 「デベロッパApp」欄の自分のApple IDをタップ → **「信頼」** |
| 4 | アプリの初回起動時 | 「カメラへのアクセスを求めています」→ **許可**（拒否すると即クラッシュ・黒画面） |
| 5 | 以降7日ごと（署名期限切れ時） | Xcodeから再度 ▶ Run するだけでよい。#3の信頼が消えていたら再度実施 |

---

## 4. Xcodeで署名設定をする

```bash
open -a Xcode
```

1. **Xcode → Settings → Accounts**
2. `+` ボタン → Apple IDでサインイン（学校のApple IDでなく個人のもので構わない）
3. **Manage Certificates** → `+` → **Apple Development** を作成

---

## 5. ビルドしてiPadに入れる

### Unity側

1. **File → Build Settings**
2. Platformを **iOS** に（初回は**Switch Platform**ボタンを押す。5-15分かかる）
3. **Scenes in Build** に、`Assets/Scene/` フォルダ内で**ファイル名の日付が一番新しいシーン**を追加する（例: `Test0818.unity`。日付は`Test[月日]`の形式）
4. **Build** ボタン → 保存先は `Builds/iOS/` を指定 → ビルド完了まで待つ（初回10-20分）

### Xcode側

```bash
open Builds/iOS/Unity-iPhone.xcodeproj
```

1. 左のProject Navigatorでプロジェクトを選択 → **Signing & Capabilities**
2. **Team** に自分のApple ID（Personal Team）を選択
3. Xcode上部のデバイス選択メニューで、**iPadの実機名**を選ぶ
   ⚠️ **Simulatorは絶対に選ばない** — ARKitのネイティブ機能がSimulatorには存在せず、ビルド・動作しません
4. ▶ Run
5. 「pairing is in progress」のまま進まない場合 → iPad側に信頼ダイアログが出ていないか確認。出ていなければUSBケーブルを抜き差し

---

## 6. 動作確認のポイント

- 対象物（ペットボトル・ケトル・バナナなど検証用オブジェクト）をカメラに向ける
- 認識されるとコンテンツが表示される（表示位置が対象物からズレている場合があります — 下記「よくある詰まりどころ」参照）
- レイヤーボタンでON/OFFが切り替わるか確認
- iPadを動かしてもコンテンツが対象物に貼りついたままか確認（World Anchorによる座標固定）

---

## よくある詰まりどころ

### 認識はするのにコンテンツが見当違いの場所に出る/出ない

これは**既知の未解決課題**です。対象物ごとに`ObjectDetectionSpawner`の`contentOffset`という値を手動調整する必要があります。README.md の「⚠️ 既知の限界：キャリブレーション」を必ず読んでください。新しい対象物を追加するたびに発生します。

### Xcodeで署名エラーが出る

**Signing & Capabilities**でTeamが選択されているか確認。「Automatically manage signing」にもチェック。

### `Undefined symbol: _UnityARKit_refPoints_*` のようなリンクエラーが大量に出る

Xcodeのビルドキャッシュが古い可能性があります。Xcodeを完全終了 → `~/Library/Developer/Xcode/DerivedData/` 内の該当プロジェクトフォルダを削除 → Xcodeを再起動してビルドし直す。

### 7日経ったらアプリが起動しなくなった

Personal Team署名の期限切れです。故障ではありません。Xcodeから再度▶ Runするだけで直ります。

### iPadが認識されない

```bash
system_profiler SPUSBDataType | grep -A 5 "iPad"
```
で接続を確認。Xcode → **Window → Devices and Simulators** でも確認可能。

---

## 対象物を増やす・入れ替える（鹿剥製の本番投入など）

1. [ar-solution-scanner-app](https://github.com/tQy2015/ar-solution-scanner-app) のガイドに従って対象物をスキャン → `.arobject`を書き出す
2. Unity Editorで `Assets/XR/` にある `XR Reference Object Library` アセットを開き、`.arobject`を登録（新規対象なら新しいライブラリアセットを作成）
3. シーン内の `ARTrackedObjectManager` コンポーネントにそのライブラリをセット
4. `ObjectDetectionSpawner` の `contentPrefab` に表示したい3Dモデル（骨格/筋肉/臓器など）を設定
5. 実機でテストし、`contentOffset`を調整する（ログに出る`center`/`extent`の値を参考に）

---

## 困ったら

このREADME/GUIDEで解決しない場合は、担当教員（小林）に連絡してください。より詳しい設計背景資料は教員側の管理リポジトリにあります。
