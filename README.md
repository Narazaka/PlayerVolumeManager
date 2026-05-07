# Player Volume Manager

プレイヤーの音量・アバター音量をエリア・条件ごとに調整するUdonシステム

聞く側側視点で各グループへの音量設定を管理する形式です。

## Install

### VCC用インストーラーunitypackageによる方法（おすすめ）

https://github.com/Narazaka/PlayerVolumeManager/releases/latest から `net.narazaka.vrchat.player-volume-manager-installer.zip` をダウンロードして解凍し、対象のプロジェクトにインポートする。

### VCCによる方法

1. https://vpm.narazaka.net/ から「Add to VCC」ボタンを押してリポジトリをVCCにインストールします。
2. VCCでSettings→Packages→Installed Repositoriesの一覧中で「Narazaka VPM Listing」にチェックが付いていることを確認します。
3. アバタープロジェクトの「Manage Project」から「Player Volume Manager」をインストールします。

## Usage

1. PlayerVolumeManagerを置く
2. PlayerAudioGroup (Area/Switch/All)等を置いて設定する。

## Changelog

- 1.0.0-alpha.3: listen fromのインスペクタ改善
- 1.0.0-alpha.2: (breaking) group指定場所を変更
- 1.0.0-alpha.1: exampleシーン修正 / ベンチマーク追加
- 1.0.0-alpha.0: リリース

## License

[Zlib License](LICENSE.txt)
