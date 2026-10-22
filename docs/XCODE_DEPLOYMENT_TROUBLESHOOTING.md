# Xcode iOS 配備トラブルシューティング

**作成日**: 2026-07-28
**更新**: 2026-10-22（問題5追加・Bundle Identifier重複／正典をこのリポジトリへ移管）
**対象**: Unity AR Foundation + Xcode Personal Team デプロイ

---

## 概要

Mac での iOS ビルド・Xcode 署名設定・iPad へのデプロイ時に発生した問題と解決方法をまとめました。

**本ドキュメントとの役割分担**: ここは個別トラブルの詳細カタログ。
Unity編集〜Xcodeビルド〜iPad実機テストまでの**通し手順**は、教員側の管理リポジトリ
（`melon-active/ar-solution/docs/UNITY_AR_BUILD_RUNBOOK.md`）が早見表として本ドキュメントの
問題を要約参照している。

---

## 問題と解決方法

### 問題 1: Provisioning Profile が生成されない

#### エラーメッセージ
```
"Unity-iPhone" requires a provisioning profile. 
Select a provisioning profile in the Signing & Capabilities editor.
```

#### 原因
- Xcode のキャッシュが古い状態
- Bundle Identifier がデフォルト値のままだった
- Team 設定後に Provisioning Profile の再生成が必要

#### 解決手順

**Step 1: Bundle Identifier を変更**

1. **Xcode** → **Targets** → **Unity-iPhone**
2. **Signing & Capabilities** タブ
3. **Bundle Identifier** フィールド
4. `com.DefaultCompany.ar-solution-unity` を一意な値（例: `com.tqy.arsolution`）に変更
5. **Return** キー押下

**Step 2: Xcode キャッシュをクリア**

```bash
# Xcode を完全に終了
killall Xcode

# キャッシュディレクトリを削除
rm -rf ~/Library/Developer/Xcode/DerivedData/*
rm -rf ~/Library/Caches/com.apple.dt.Xcode
```

**Step 3: Xcode 再起動・プロジェクト再度開く**

```bash
# Xcode を起動
open /Applications/Xcode.app

# プロジェクトを再度開く
open ~/repo/ar-solution-unity/Builds/iOS/Unity-iPhone.xcodeproj
```

**Step 4: UnityFramework target も確認**

1. **TARGETS** → **UnityFramework**
2. **Signing & Capabilities** タブ
3. **Automatically manage signing** ☑
4. **Team** → `[Your Name] (Personal Team)` を選択

#### チェックリスト
- ☐ Bundle Identifier が一意な値に変更されている
- ☐ Xcode キャッシュが削除されている
- ☐ **Unity-iPhone** target の Team が設定されている
- ☐ **UnityFramework** target の Team が設定されている
- ☐ Provisioning Profile が「Xcode Managed Profile」と表示されている

---

### 問題 2: Developer Mode が無効

#### エラーメッセージ
```
Domain: com.apple.dt.deviceprep
Code: -28
Recovery Suggestion: To use iPad for development, enable Developer Mode 
in Settings → Privacy & Security.
```

#### 原因
- iPad で Developer Mode が無効のままだった

#### 解決手順

**iPad で Developer Mode を有効化**

1. **iPad 設定** アプリを開く
2. **Privacy & Security**（プライバシーとセキュリティ）をタップ
3. **Developer Mode** トグルをオン（スイッチが青になる）
4. 確認ダイアログ → **Restart** をタップ（iPad が自動再起動）
5. 再起動後、iPad を Mac に USB 接続

#### チェックリスト
- ☐ iPad 設定 → Privacy & Security → Developer Mode が ON
- ☐ iPad が再起動完了後、Mac に接続されている
- ☐ Xcode が iPad デバイスを認識している（デバイス一覧に表示）

---

### 問題 3: Developer App Certificate が信頼されていない

#### エラーメッセージ
```
The application could not be launched because the Developer App Certificate is not trusted.
Domain: IDELaunchCoreDevice
Code: 0

Recovery Suggestion: Open Settings on the device and navigate to 
General -> VPN & Device Management, then select your Developer App 
certificate to trust it.
```

#### 原因
- iPad で Personal Team の Developer App Certificate が信頼設定されていなかった

#### 解決手順

**iPad で Developer App Certificate を信頼**

1. **iPad 設定** アプリを開く
2. **General**（一般）をタップ
3. **VPN & Device Management**（VPNとデバイス管理）をタップ
4. Developer App certificate（例：自分のApple ID）を選択
5. **Trust**（信頼）ボタンをタップ
6. 確認ダイアログ → **Trust** をタップ

#### チェックリスト
- ☐ iPad 設定 → General → VPN & Device Management に Developer App certificate が表示されている
- ☐ Developer App certificate が「信頼済み」の状態になっている
- ☐ iPad をロック解除状態で Mac に接続

---

### 問題 4: Undefined symbol: _UnityARKit_refPoints_* （リンクエラー多数）

#### エラーメッセージ

```
Undefined symbol: _UnityARKit_refPoints_acquireChanges
Undefined symbol: _UnityARKit_refPoints_onDestroy
Undefined symbol: _UnityARKit_refPoints_onStart
Undefined symbol: _UnityARKit_refPoints_onStop
Undefined symbol: _UnityARKit_refPoints_releaseChanges
Undefined symbol: _UnityARKit_refPoints_tryAdd
Undefined symbol: _UnityARKit_refPoints_tryAttach
Undefined symbol: _UnityARKit_refPoints_tryRemove
```

#### 原因

`ARSession` / `ARCameraManager` 等のコンポーネントをシーンに配置しても、
**XR Plug-in Management で iOS 用に ARKit ローダーを有効化していない**と、
Unityはネイティブプラグイン `libUnityARKit.a` をXcodeプロジェクトの `Libraries/` にコピーしない。
一方でC#側（IL2CPP生成コード）はそのネイティブ関数をシンボル参照するため、リンク時に「定義がない」エラーになる。

**もう一つの原因（同一症状）**: Xcodeの実行先デバイスに**Simulatorを選んでいる**場合も同じエラーが出る。
ARKitのネイティブライブラリは実機（arm64）専用でシミュレータ用のスライスを含まないため。

#### 確認方法

`~/repo/ar-solution-unity/Assets/XR/XRGeneralSettingsPerBuildTarget.asset` を開き、
`Values: []` になっていたら未設定（正しく設定されると `m_Loaders` にARKitLoaderが入り、
`Values` に `iPhone Settings` が追加される）。

#### 解決手順

1. Unity Editor: **Edit → Project Settings → XR Plug-in Management → iOS タブ**
2. **Plug-in Providers** の **ARKit** にチェック
3. **File → Build Settings → Build** で再出力（同じフォルダへの上書きで可）
4. Xcodeで再度開き、実行先デバイスが**実機のiPad**になっているか確認してから Run

#### チェックリスト
- ☐ XR Plug-in Management → iOS → ARKit にチェックが入っている
- ☐ `XRGeneralSettingsPerBuildTarget.asset` の `Values` が空でない
- ☐ Xcodeの実行先デバイスが実機（Simulatorではない）
- ☐ 出力されたXcodeプロジェクトの `Libraries/com.unity.xr.arkit/` に `libUnityARKit.a` が存在する

---

### 問題 5: 教員（検証用）と学生（本人ビルド）が同じ Bundle Identifier を使うと衝突する

#### 症状

教員が動作確認のため Personal Team でビルド・実機インストールした後、
学生が同じ Bundle Identifier のまま自分の Apple ID / Personal Team でビルドすると、
Provisioning Profile 関連のエラーや、端末上で以前のアプリが正常に上書きインストールされない
といった問題が起こりうる。

#### 原因

Personal Team の無料プロビジョニングは **Bundle Identifier + Apple ID の組が端末ごとに一意**であることを
前提にしている。異なる Apple ID（教員用 / 学生用）が同じ Bundle Identifier を名乗ると、
Apple 側のプロビジョニングプロファイル管理で競合する。

#### 解決手順

**Unity 側で Bundle Identifier を変更**（Xcode 側で都度直接変更するより、Unity 側を正とする方が
Build の度に上書きされず安全）:

1. Unity Editor: **Edit → Project Settings → Player**
2. **iOS タブ** → **Other Settings** → **Identification**
3. **Bundle Identifier** を学生ごとに一意な値に変更
   （例: `com.<学生名等>.arsolution`）
4. **File → Build Settings → Build** で再出力

#### チェックリスト
- ☐ 教員が検証用にビルドした Bundle Identifier と、学生が自分のビルドに使う Bundle Identifier が異なる
- ☐ Bundle Identifier の変更は **Unity の Player Settings 側**で行う（Xcode 側で直接編集すると次回 Unity Build で上書きされる）
- ☐ 学生ごとに値を変える運用（命名規則は未確定・現状は都度一意な文字列を割り当てる運用）

---

### 問題 6: iOS deployment target の警告（インストール済みXcodeのSDKがサポートしない下限）

#### 警告メッセージ

```
The iOS deployment target 'IPHONEOS_DEPLOYMENT_TARGET' is set to 14.0, but the range of
supported deployment target versions is 15.0 to 27.1.x.
```

#### 原因

Unity Player Settingsの `Target minimum iOS Version` が古い値（旧推奨14.0）のままだと、
新しいXcode/iOS SDKがサポート対象外にしている範囲外になる。

#### 解決手順

Unity Editor: **Edit → Project Settings → Player → iOS → Other Settings → Target minimum iOS Version**
を、インストール済みXcodeがサポートする範囲内（例: `15.0`）に変更してから再ビルドする。

#### 補足

警告が直らない場合、Xcode側が古いプロジェクトファイルをキャッシュ表示していることがある。
Xcodeを完全終了（`pkill -9 Xcode` 等）→ `~/Library/Developer/Xcode/DerivedData/` の該当フォルダを
削除 → 再度開き直す。

---

## 完全な配備フロー（正常系）

### 前提
- Unity 2022 LTS + iOS Build Support インストール済み
- AR Foundation / ARKit XR Plugin が Package Manager に導入済み
- iOS Build が `Builds/iOS/` に生成済み

### ステップ

| # | 作業 | 実行者 | チェック |
|---|------|-------|---------|
| 1 | Xcode でプロジェクトを開く | Mac | `open ~/repo/ar-solution-unity/Builds/iOS/Unity-iPhone.xcodeproj` |
| 2 | Bundle Identifier を一意な値に設定 | Mac | Unity Player Settings側で変更（問題5参照） |
| 3 | Unity-iPhone target の署名設定 | Mac | Automatically manage signing ☑、Team 設定 |
| 4 | UnityFramework target の署名設定 | Mac | Automatically manage signing ☑、Team 設定 |
| 5 | Xcode キャッシュをクリア | Mac | `rm -rf ~/Library/Developer/Xcode/DerivedData/*` |
| 6 | Xcode を再起動 | Mac | `killall Xcode && open /Applications/Xcode.app` |
| 7 | iPad で Developer Mode を有効化 | iPad | Settings → Privacy & Security → Developer Mode ON |
| 8 | iPad を Mac に USB 接続 | - | iPad が Mac に認識される |
| 9 | iPad で信頼設定 | iPad | 「このコンピュータを信頼しますか？」→ Trust |
| 10 | iPad で Developer App Certificate を信頼 | iPad | Settings → General → VPN & Device Management → Trust |
| 11 | Xcode で iPad デバイスを選択 | Mac | Scheme ドロップダウン → iPad を選択 |
| 12 | Build & Run を実行 | Mac | ⌘R または ▶ ボタンをクリック |
| 13 | iPad でアプリ起動確認 | iPad | AR アプリが起動・テストシーン表示 |

---

## よくある間違い

### ❌ Provisioning Profile が手動で必要だと思っている

**誤り**: Manual Provisioning Profile を取得しようとする  
**正解**: Xcode Managed Profile を使う（Personal Team は自動管理が推奨）

### ❌ UnityFramework target を署名設定し忘れた

**誤り**: Unity-iPhone target だけ署名設定して Build  
**正解**: **両方とも** Automatically manage signing ☑・Team 設定が必須

### ❌ キャッシュをクリアせず何度も Build を試す

**誤り**: Xcode を再起動せずに Build を繰り返す  
**正解**: キャッシュクリア → Xcode 再起動 → Build

### ❌ iPad が「このコンピュータを信頼」か「Developer Mode」のどちらかだけ設定

**誤り**: Developer Mode だけ有効にして、コンピュータ信頼設定を忘れる  
**正解**: **両方とも**設定が必須

### ❌ 教員と学生で同じ Bundle Identifier のまま運用

**誤り**: README/ガイドをそのままコピーしてビルドし続ける  
**正解**: 自分用に一意な Bundle Identifier へ変更する（問題5参照）

---

## デバッグコマンド

### Xcode のログを確認

```bash
# 最新のビルドログを表示
cat ~/Library/Logs/Xcode/DerivedData/*/Logs/Build/Build.log | tail -100
```

### iPad がデバイスとして認識されているか確認

```bash
# Mac に接続されている iOS デバイス一覧
xcrun xcode-select --print-path
system_profiler SPUSBDataType | grep -A 5 "iPad"
```

### Xcode キャッシュの完全削除

```bash
# 慎重に実行（既存のビルド成果物が全削除される）
rm -rf ~/Library/Developer/Xcode/DerivedData
rm -rf ~/Library/Caches/com.apple.dt.Xcode
rm -rf ~/Library/Developer/Xcode/iOS\ DeviceSupport
```

---

## 参考資料

- **Apple Developer**: https://developer.apple.com/account/
- **Xcode Help**: Xcode → Help → Xcode Help
- **AR Foundation Docs**: https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest

---

**最終更新**: 2026-10-22
**プロジェクト**: AR Solution 卒業制作支援（大阪芸術大学）
