# AR拡張展示 - Unity Project

大阪芸術大学 デジタルアーツコース 卒業制作支援  
鹿の剥製へのAR解剖展示（骨格・筋肉・臓器のレイヤー表示）

## 技術スタック

- **Unity**: 2022.3.62f LTS
- **AR Framework**: AR Foundation + ARKit Object Detection
- **Target Platform**: iOS (iPad)
- **Build Tool**: Xcode (Free Provisioning / Personal Team)

## セットアップ

```bash
git clone https://github.com/tQy2015/ar-solution-unity.git
cd ar-solution-unity
# Unity Editorで開く
```

## ドキュメント

設定・セットアップ・ワークフローの詳細は `~/repo/melon-active/ar-solution/docs/` を参照
（管理層は独立リポジトリ。詳細は本リポジトリ `CLAUDE.md`「命名矛盾の明示的説明」参照）。

- `SETUP_ROADMAP.md` — 全体ロードマップ
- `MAC_SETUP_CHECKLIST.md` — Unity インストール手順
- `IPAD_DEPLOYMENT.md` — iPad 配備・署名手順
- `UNITY_AR_BUILD_RUNBOOK.md` — Unity→Xcode→iPad 通しチェックリスト・詰まりどころ・今後の手順
- `XCODE_DEPLOYMENT_TROUBLESHOOTING.md` — Xcode配備の既知トラブル集
- `AR_CALIBRATION_ARCHITECTURE.md` — キャリブレーション仕様（認識方式は2026-08-06 ARKit Object Detectionに変更）
- `SCANIVERSE_WORKFLOW.md` — （凍結）旧方式のスキャンワークフロー

## ライセンス

Educational - Osaka University of Arts
