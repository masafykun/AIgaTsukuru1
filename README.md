# 🪙 コイン落としゲーム

> クリックした位置からコインを落として、ペグに弾かれながらスコアを狙え。

Plinko スタイルのコイン落としゲームです。  
物理演算で予測不能に跳ね返るコインを、高得点ゾーンへ導こう！

![Unity](https://img.shields.io/badge/Unity-6000.4.5f1-000000?style=flat-square&logo=unity)
![C#](https://img.shields.io/badge/C%23-10+-239120?style=flat-square&logo=csharp)
![URP](https://img.shields.io/badge/Render-URP-blue?style=flat-square)
![Platform](https://img.shields.io/badge/Platform-WebGL%20%7C%20Windows%20%7C%20Mac-lightgrey?style=flat-square)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)

🎮 **[ブラウザでプレイ（itch.io）](https://masafy.itch.io/coin-drop)**

---

## 📸 スクリーンショット

![gameplay](Screenshots/gameplay.png)

---

## 🎮 操作方法

| 操作 | 動作 |
|---|---|
| 画面クリック | クリックした X 座標からコインを落下 |

コインが下部のスコアゾーンに入ると得点。10枚使い切るとゲームオーバー。

---

## ✨ 特徴

- **ランダム反発係数** — コインごとに異なる跳ね方をする
- **カラーバリエーション** — 落とすたびにランダムな色のコインが出現
- **パーティクルエフェクト** — 着地時にカラーバーストとトレイルエフェクト
- **プロシージャル BGM・SE** — コードで生成したチップチューン BGM と効果音
- **Bloom 発光** — URP ポストプロセスによる発光エフェクト
- **ゲームオーバー画面** — スコア表示 & リトライボタン

---

## 🏆 スコアゾーン

| ゾーン | 得点 |
|---|---|
| 外側（青） | 100点 |
| 内側（緑） | 200点 |
| 中央寄り（黄） | 500点 |
| 中央（赤） | 1000点 |

---

## 🛠️ 技術スタック

| カテゴリ | 技術 |
|---|---|
| ゲームエンジン | Unity 6000.4.5f1 |
| レンダリング | Universal Render Pipeline (URP) |
| 入力 | Unity Input System |
| 物理演算 | Unity Physics (3D) |
| 言語 | C# |

---

## 📁 ディレクトリ構成

```
AIgaTsukuru1/
├── Assets/
│   ├── Editor/
│   │   └── CoinDropSetup.cs     # シーン一括構築ツール（CoinDrop メニュー）
│   ├── Materials/               # URP マテリアル
│   ├── Prefabs/
│   │   └── Coin.prefab          # コインプレハブ
│   ├── Scenes/
│   │   └── SampleScene.unity
│   └── Scripts/
│       ├── Coin.cs              # コイン物理・エフェクト
│       ├── CoinDropper.cs       # 入力処理・コイン生成
│       ├── GameManager.cs       # スコア・ゲームオーバー管理
│       ├── GameSFX.cs           # プロシージャル SE
│       ├── ProceduralBGM.cs     # プロシージャル BGM
│       ├── ScorePopup.cs        # 得点ポップアップ
│       └── ScoreZone.cs         # スコアゾーン判定
└── Screenshots/
    └── gameplay.png
```

---

## 🚀 セットアップ

```bash
# 1. リポジトリをクローン
git clone https://github.com/masafykun/AIgaTsukuru1.git

# 2. Unity Hub でプロジェクトを開く
# Unity 6000.4.5f1 が必要

# 3. シーンを構築（初回のみ）
# Unity メニュー → CoinDrop → Setup Scene

# 4. Play ボタンで動作確認
```

---

## ライセンス

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://opensource.org/licenses/MIT)

このプロジェクトは **MIT ライセンス** のもとで公開しています。

© 2026 masafykun (https://github.com/masafykun)
