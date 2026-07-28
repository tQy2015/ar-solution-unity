**ID**: melon-ar-solution-claude-code
**TTL**: AR拡張展示 - Vuforia+Unity 卒業制作支援（大阪芸術大学）
**VER**: 0.1
**CRT**: 2026-06-30
**MOD**: 2026-06-30
**LLM**: Claude Sonnet 4.6

---

> ⚠️ **パスハードコード禁止**（melonルート規約を継承）
> - ファイル参照 → 相対パス（`./`）
> - コマンド中の絶対パス → `$CLAUDE_MELON_PROJECT_ROOT`
> - `/home/tqy/...` 等の絶対パス記述は禁止

---

# AR拡張展示 サブプロジェクト・プロファイル

## 位置づけ

- **種別**: 卒業制作支援（大阪芸術大学 デジタルアーツコース）
- **目標**: 鹿の剥製へのAR解剖展示（骨格・筋肉・臓器のレイヤー表示）
- **正典**: `./STATE.md`（現在地・次アクション）
- **元資料**: `$CLAUDE_MELON_PROJECT_ROOT/716-AR-solution-research.md`

## 確定済み設計判断

| 項目 | 決定 | 備考 |
|---|---|---|
| ARフレームワーク | Vuforia Engine（モデルベーストラッキング）| Basic = 無料 |
| 開発環境 | Unity 2022 LTS | iOS Build Support 必須 |
| 3Dスキャン | SCANIVERSE → OBJ | GLB不可（後工程非対応） |
| 検証対象 | 椅子 → 5月以降：鹿の剥製 | 同一パイプラインで差し替え |
| 展示端末 | **iPad**（iOS） | Xcode無料プロビジョニングで配備 |
| 目標精度 | 1cm以内のSYNC | |
| 追加ライセンス料 | **なし** | Vuforia Basic + 無料Apple ID |

## 配備方針（iPad）

- Apple Developer（$99/年）は**使わない**
- Xcode無料プロビジョニング（Personal Team）でiPadに直接インストール
- アプリ有効期限7日 → Xcodeで再Runするだけで更新（再スキャン不要）
- **Mac必須**（Xcode実行環境）

## ディレクトリ構成

```
ar-solution/
├── CLAUDE.md        ← 本ファイル
├── STATE.md         ← 現在地・次アクション
├── docs/            ← 手順書・チェックリスト
├── scans/           ← SCANIVERSEエクスポートOBJデータ
├── builds/          ← Xcodeプロジェクト・IPAの参照メモ
└── unity-refs/      ← Unityシーン構成メモ・スクリーンショット
```

> Unityプロジェクト本体はDropbox外（容量・同期速度の都合）に置くこと。
> パスはSTATE.mdの「環境メモ」欄に記録する。

## フェーズ構成

| フェーズ | 内容 |
|---|---|
| P0 | **iPad検証**（椅子スキャン → Vuforia → Xcode → iPad動作確認） |
| P1 | **鹿本番スキャン**（SCANIVERSE 360°・OBJ書き出し） |
| P2 | **解剖モデル投入**（骨格・筋肉・臓器GLBモデル差し替え） |
| P3 | **レイヤーUI実装**（ボタンON/OFFスクリプト・展示端末確定） |

## 応答スタンス

melonルート規約を継承（`constructive_challenge` / 結論先行 / ミニマル表示）。
技術選定は「追加費用なし・学生が再現可能」を最優先制約として評価する。
