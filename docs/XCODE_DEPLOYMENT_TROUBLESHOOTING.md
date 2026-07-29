# Xcode iOS 配備トラブルシューティング

**作成日**: 2026-07-28  
**対象**: Unity AR Foundation + Xcode Personal Team デプロイ

---

## 概要

Mac での iOS ビルド・Xcode 署名設定・iPad へのデプロイ時に発生した問題と解決方法をまとめました。

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
4. `com.DefaultCompany.ar-solution-unity` を `com.tqy.arsolution` に変更
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
- ☐ Bundle Identifier が `com.tqy.arsolution` に変更されている
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
4. Developer App certificate（例：`Apple Development: tqy2010@gmail.com`）を選択
5. **Trust**（信頼）ボタンをタップ
6. 確認ダイアログ → **Trust** をタップ

#### チェックリスト
- ☐ iPad 設定 → General → VPN & Device Management に Developer App certificate が表示されている
- ☐ Developer App certificate が「信頼済み」の状態になっている
- ☐ iPad をロック解除状態で Mac に接続

---

## 完全な配備フロー（正常系）

### 前提
- Unity 2022 LTS + iOS Build Support インストール済み
- AR Foundation 5.1.0・ARKit XR Plugin 5.1.0 が Package Manager に導入済み
- iOS Build が `Builds/iOS/` に生成済み

### ステップ

| # | 作業 | 実行者 | チェック |
|---|------|-------|---------|
| 1 | Xcode でプロジェクトを開く | Mac | `open ~/repo/ar-solution-unity/Builds/iOS/Unity-iPhone.xcodeproj` |
| 2 | Bundle Identifier を設定 | Mac | `com.tqy.arsolution` に変更 |
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

## 次のステップ

✅ iOS Build が iPad で起動確認できたら：

1. **タスク7**: SCANIVERSE で椅子をスキャン → OBJ 出力
2. **タスク8**: OBJ を Unity にインポート
3. **タスク9**: AR Session に統合
4. **タスク10**: iPad 実機テスト（認識・追従・精度確認）

---

**最終更新**: 2026-07-28  
**作成者**: TQY Kobayashi  
**プロジェクト**: AR Solution 卒業制作支援
