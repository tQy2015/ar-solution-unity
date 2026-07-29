# AR拡張展示 — STATE（現在地・次アクションの正典）

**更新**: 2026-07-29
**フェーズ**: P0 — iPad実機デプロイ成功（AR Foundation + ARKit 動作確認済み）

---

## 環境・役割分担（2026-07-28更新・git移行完了）

**ディレクトリ構成（git ベース）**:
```
~/repo/
  ar-solution-unity/     ← Unity プロジェクト本体（独立リポジトリ）
  melon-active/
    ar-solution/         ← ドキュメント・プロジェクト管理（正典）
```

- **Mac側**: 本プロジェクト構築済み（Unity/Xcode実行環境）。Unity Editor・iOS Buildの実作業はMacで行う。
  - Unity プロジェクト本体: `~/repo/ar-solution-unity`
- **Z240側（このDropbox/tmux）**: バックヤード業務専用。
  - **理由**: Macのtmuxセッションには継続性がない（再起動等でコンテキストが失われる）ため、永続的な作業記録・スクリプト整備はこちら（Z240 tmux）で行う方針にした。
  - 担当範囲: Unity C#スクリプトの下書き、解剖モデル(GLB)最適化パイプライン、SCANIVERSEスキャン後処理、ドキュメント整備など、Mac実機非依存の作業。
- **マルチマシン制御プレーン**（`$CLAUDE_MELON_PROJECT_ROOT/claude-code-config/MULTI-MACHINE-CONTROL-PLANE.md`）: 現状 `omen`/`z240` のみ登録、Macは未登録。MacのtmuxをZ240から直接操作する必要が生じた場合、同ドキュメント§6の手順（Tailscale導入・SSH鍵交換・peer-run.sh拡張）でMacをpeer追加する（未着手・必要になったら着手）。

---

## 方針（2026-07-28更新・git 移行）

- **フレームワーク**: AR Foundation + ARKit Object Detection（Unity公式・無料・ロゴなし）
- **展示端末**: iPad 確定
- **Android**: ARCore で同一コードが動くが精度限定・検証は任意
- **iPad配備**: Xcode無料プロビジョニング（Personal Team・追加費用なし）
- **Unityプロジェクト本体パス**: `~/repo/ar-solution-unity` (git ベース)
- **プロジェクト管理**: `~/repo/melon-active/ar-solution/STATE.md`（このファイル）
- **Unity作業マシン**: ローカルMac（開発・iOS Build） → 学生Mac（本番配備）
- **研究・計画マシン**: Z240 Ubuntu（参考・Android 検証用）
- **使用端末（iPad）**: （機種・iPadOSバージョンを記入）

## 確定事実（2026-07-03 修正）

1. **ARコンセプト**: 大型動物（鹿剥製）に近接して内部を投影
   - 初回キャリブレーション: SCANIVERSE OBJ をリファレンスにビジョンベース マッチング
   - 座標固定: キャリブレーション後、AR World Anchor で座標を静止
   - 推奨距離: 1-3m（接近しすぎ NG）

2. **フレームワーク**: AR Foundation + ARKit（ビジョンベース初回検出） + World Anchor
3. **iPad配備方針**: Xcode無料プロビジョニング（Mac必要・7日更新）
4. **検証対象**: 椅子（P0 パイプライン検証）→ 鹿の剥製（P1 本番）
5. **Unity**: ✅ 2022.3.62f3 LTS + iOS Build Support インストール済み

---

## セットアップ進捗（2026-07-03）

### ✅ 完了事項

1. ✅ Unity 2022.3.62f3 LTS インストール済み
2. ✅ iOS Build Support インストール済み
3. ✅ ar-solution-unity プロジェクト初期化完了
   - **パス**: `~/Projects/ar-solution-unity`
   - **状態**: Assets / Library / Packages 生成完了
4. ✅ AR Foundation 5.1.0 → manifest.json に追加
5. ✅ ARKit XR Plugin 5.1.0 → manifest.json に追加

### ⏳ 次のステップ

1. **Unity Editor を開く** → AR Foundation/ARKit Package 自動インポート
2. **iOS Build Support に切り替え**（Build Settings）
3. **Xcode で Apple ID 登録** + Personal Team 設定
4. **テストビルド生成** → iPad デプロイテスト

## セットアップドキュメント（2026-07-03 作成・修正）

| ドキュメント | 内容 | 参照時期 |
|----------|-----|--------|
| `docs/SETUP_ROADMAP.md` | 全体ロードマップ（修正版） | 全体像把握 |
| `docs/MAC_SETUP_CHECKLIST.md` | Unity インストール詳細 | 環境構築 |
| `docs/IPAD_DEPLOYMENT.md` | iPad 配備・署名 | iPad デプロイ時 |
| `docs/AR_CALIBRATION_ARCHITECTURE.md` | **NEW** キャリブレーション仕様 | AR 実装の詳細設計 |
| `docs/SCANIVERSE_WORKFLOW.md` | スキャンワークフロー（修正版） | P0/P1 フェーズ |

## タスクキュー（P0: iPad検証）

凡例: ⬜todo 🔄進行 ✅done ⚠️ブロッカー

| # | 状態 | タスク | ドキュメント |
|---|---|---|---|
| 1 | ✅ | Unity 2022 LTS インストール (iOS Build Support 含む) | MAC_SETUP_CHECKLIST Phase 1 |
| 2 | ✅ | ar-solution プロジェクト初期化（Mac） | MAC_SETUP_CHECKLIST Phase 2 |
| 3 | ✅ | AR Foundation + ARKit XR Plugin インポート（manifest.json追加済み、Unity Editorでの自動インポート完了済み） | MAC_SETUP_CHECKLIST Phase 3 |
| 4 | ✅ | Xcode Apple ID 登録 + Personal Team 設定 | IPAD_DEPLOYMENT Step 1-3 |
| 5 | ✅ | テストビルド生成（Cube） | IPAD_DEPLOYMENT Step 2 |
| 6 | ✅ | iPad に テストビルド デプロイ | IPAD_DEPLOYMENT Step 4 |
| 7 | ⬜ | 椅子を SCANIVERSE でスキャン → OBJ出力 | SCANIVERSE_WORKFLOW P0 |
| 8 | ⬜ | OBJ を Unity にインポート | SCANIVERSE_WORKFLOW Step Unity |
| 9 | ⬜ | AR Session に統合 | SCANIVERSE_WORKFLOW Step AR |
| 10 | ⬜ | iPad実機テスト（認識・追従・精度） | SETUP_ROADMAP Day 3 |

---

## タスクキュー（P1以降）

| フェーズ | 状態 | タスク | 詳細 |
|---|---|---|---|
| P1 | ⬜ | 鹿の剥製を SCANIVERSE で 360° スキャン → OBJ | リファレンスモデル生成 |
| P1 | ⬜ | OBJ を Unity に インポート → AR シーン配置 | 初回キャリブレーション用 |
| P2 | ⬜ | 初回キャリブレーション UI 実装 | iPad でマニュアルアラインメント |
| P2 | ⬜ | World Anchor で座標系を固定 | キャリブレーション後、AR コンテンツ静止 |
| P2 | ⬜ | 解剖モデル（骨格・筋肉・臓器）GLB 投影 | World Anchor の座標系に配置 |
| P3 | ⬜ | レイヤー切り替え UI スクリプト | 骨格・筋肉・臓器の ON/OFF ボタン |
| P3 | ⬜ | iPad 実機テスト（推奨距離 1-3m） | 接近しすぎ検出・警告 |
| P3 | ⬜ | 展示用 iPad 専用機に IPA 配備 | 学生 Mac → iPad へ最終配備 |

---

## 観察ログ

形式: `日付 / 観察内容 / 分類(ok/blocker/design/idea) / 対処`

- 2026-07-29 / P0フェーズ iPad実機デプロイ成功。Provisioning Profile エラー→Bundle ID変更・キャッシュクリアで解決。Developer Mode・Certificate信頼設定で最終決着。トラブルシューティングドキュメント `XCODE_DEPLOYMENT_TROUBLESHOOTING.md` 作成済み。 / ok / タスク7以降のSCANIVERSEスキャンへ進行
- 2026-07-05 / OMEN→Z240のSSHが不通になっていた。原因はOMEN側でTailscaleがsnap版/apt版二重インストールでTUNデバイス競合しネットワークから脱落していたこと。apt版に統一し復旧済み（詳細: `$CLAUDE_MELON_PROJECT_ROOT/1593_n1-tailscale-ssh-connection-guide.md`「既知の不具合」節）。 / blocker→対処済み / 再発時は同ドキュメントの診断コマンドを確認
- 2026-07-05 / このプロジェクト（Z240側tmuxセッション）が頻繁に落ちる傾向を確認。原因未特定（上記SSH/Tailscale不調との関連含め要調査）。 / blocker / 次回セッションでtmuxのkill/切断ログ・dmesg・OOM killerの有無を確認する
- 2026-07-05 / 上記の一次調査: dmesg/journalctl にOOM Kill記録なし、メモリは21GB空きと余裕あり、システムuptimeは1日9h超で連続稼働（再起動なし）。`ar` tmuxセッションは今日20:20作成の1本のみで生存中だが、attachプロセスが2重（512350, 522343）に張られていた状態を確認 → OOM等のシステム要因ではなく、接続（attach）側の問題の可能性が高い。 / blocker→調査継続 / 実際に切断が発生した瞬間のエラーメッセージ・再現手順があれば追加調査
- 2026-07-05 / 上記の重複attach解消: 古い方(PID 512350, pts/18, 20:21開始)をkillし、`ar`は522343の1本に整理。他の全tmuxセッション（cbp/critique/fgo/git-dx/llm-bench/pm2/srd/uma）も確認したが重複なし。 / blocker→対処済み / システムレベルのクラッシュ（OOM等）の証跡はなし。「頻繁に落ちる」の実態は二重attachの可能性が高いが、断定には再発時の実測が必要
- 2026-07-05 / 原因確定: tmux `ar`セッションへのattach重複が原因と判明、解消済み。 / blocker→解消 / 同一tmuxセッションへの多重attachを避ける（複数ターミナル/Claude Codeセッションから同時attachしない）

---

## 設計判断アーカイブ（凍結・再議論しない）

### Vuforia → AR Foundation 切り替え（2026-06-30）

**経緯**: Vuforia Basic（無料）はロゴ（ウォーターマーク）が常時表示される。展示本番での使用に懸念。

**Vuforia の特性（参考記録）**:
- Vuforia Engine Basic: 無料・クロスプラットフォーム（Android/iOS）
- Model Target: 3Dオブジェクトスキャン → マーカーレス追跡（精度◎）
- 制約: 無料プランはカメラ映像にVuforiaロゴが常時重畳
- Plus プラン: ~$499/年（ロゴなし）→ 学生展示には費用過大と判断
- 参考: developer.vuforia.com / License Manager → Get Basic

**切り替え理由**:
1. ロゴなし・無料を優先
2. 展示端末がiPad確定方向 → ARKit Object Detection で要件充足
3. AR Foundation は Unity公式・同一コードで ARCore（Android）にも対応

**凍結判断**:
- ARKit Object Detection を主フレームワークとする
- Vuforia は「Android対応が必須かつiPad不使用」になった場合の差し戻し候補
- スキャン形式: OBJ（GLBは後工程非対応のため不可）
- Unity バージョン: 2022 LTS 固定

---

*恒久仕様: `./CLAUDE.md` / 元指示書: `$CLAUDE_MELON_PROJECT_ROOT/716-AR-solution-research.md`*
