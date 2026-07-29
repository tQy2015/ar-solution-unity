**ID**: ar-solution-unity-dev
**TTL**: AR拡張展示 - Unity試験作業プロジェクト（卒業制作支援）
**VER**: 0.1
**CRT**: 2026-07-28
**MOD**: 2026-07-29
**LLM**: Claude Haiku 4.5

---

# ar-solution-unity — 試験作業リポジトリ

## 位置づけ

**このリポジトリの役割**: Unity プロジェクト本体の開発・ビルド作業  
**対応する管理層**: `~/repo/melon-active/ar-solution` (STATE.md・ドキュメント・バックヤード作業)

---

## ⚠️ 命名矛盾の明示的説明

**「ar-solution」という名前が2つの異なる場所に存在します。混同しないこと。**

| 役割 | ローカルパス | GitHub URL | 内容 | 担当 |
|-----|-----------|-----------|------|------|
| **✓ Unity 試験作業** | `~/repo/ar-solution-unity` | `https://github.com/tQy2015/ar-solution` | Unity プロジェクト本体（Assets/, ProjectSettings/ 等） | Mac |
| **✓ 管理・バックヤード** | `~/repo/melon-active/ar-solution` | `https://github.com/tQy2015/melon-active/tree/main/ar-solution` | ドキュメント・STATE.md・スキャンデータ | Mac / Z240 |

**命名が異なる理由**:
- ローカルパス `ar-solution-unity` は「Unity 関連」を明示
- GitHub `ar-solution` は業務名で統一（melon-active 内の `ar-solution` とは異なるリポジトリ）
- 両者は **別の git リポジトリ** であり、別のリモート URL を持つ

**確認方法**（ローカルで git remote を確認）:
```bash
cd ~/repo/ar-solution-unity && git remote -v
# → https://github.com/tQy2015/ar-solution.git (Unity)

cd ~/repo/melon-active/ar-solution && git remote -v
# → https://github.com/tQy2015/melon-active.git (管理層)
```

### 何をここに置くか

✅ **コミット対象**:
- `Assets/` - C# スクリプト・Prefab・シーン
- `Packages/manifest.json` - Package Manager 依存関係
- `ProjectSettings/` - iOS ビルド設定・XR設定

❌ **コミット除外** (.gitignore):
- `Library/` - ビルド中間成果物
- `Temp/` - 一時ファイル
- `Builds/iOS/` - Xcode 出力（大容量）
- `obj/` - コンパイル成果物

### 何を管理層に置くか

`~/repo/melon-active/ar-solution/` に以下を記録：
- `STATE.md` - 進捗・次アクション
- `docs/` - セットアップ手順・トラブルシューティング
- `scans/` - SCANIVERSE エクスポート OBJ ファイル
- `unity-refs/` - シーン構成メモ・スクリーンショット

---

## ディレクトリ構成

```
ar-solution-unity/
├── CLAUDE.md                       ← 本ファイル
├── Assets/
│   ├── Scenes/
│   │   └── Test0728.unity          ← テストシーン
│   └── XR/
│       ├── Loaders/                ← ARKit Loader 設定
│       ├── Settings/               ← AR Foundation 設定
│       └── Resources/
├── Packages/
│   ├── manifest.json               ← AR Foundation 5.1.0 etc
│   └── packages-lock.json
├── ProjectSettings/
│   ├── ProjectSettings.asset       ← iOS ビルド設定
│   ├── EditorBuildSettings.asset
│   └── XRPackageSettings.asset
├── .gitignore
└── README.md
```

---

## 確定済み設計

| 項目 | 決定 |
|-----|------|
| **AR フレームワーク** | AR Foundation 5.1.0 + ARKit XR Plugin 5.1.0 |
| **Unity バージョン** | 2022.3.62f3 LTS |
| **ビルドターゲット** | iOS（ARKit） |
| **デプロイ方式** | Xcode Personal Team（無料プロビジョニング） |
| **参考リポジトリ** | https://github.com/tQy2015/melon-active (ar-solution ブランチ) |

---

## 開発フロー

1. **Mac** で Unity Editor で実装（C#・シーン構築）
2. **コミット**: Assets/・ProjectSettings/ をコミット
3. **テスト**: iOS Build → Xcode → iPad デプロイ
4. **ドキュメント**: 進捗は `~/repo/melon-active/ar-solution/STATE.md` に記録

---

## 参考リンク

- **管理層の正典**: `~/repo/melon-active/ar-solution/STATE.md`
- **セットアップ手順**: `~/repo/melon-active/ar-solution/docs/MAC_SETUP_CHECKLIST.md`
- **iPad 配備**: `~/repo/melon-active/ar-solution/docs/IPAD_DEPLOYMENT.md`
- **トラブルシューティング**: `~/repo/melon-active/ar-solution/docs/XCODE_DEPLOYMENT_TROUBLESHOOTING.md`

---

**作成者**: TQy Kobayashi  
**プロジェクト**: AR Solution 卒業制作支援
