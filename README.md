# Omawari

いわゆる Web 更新チェッカーです。静的サイトのチェックだけでなく、動的サイトのチェックも行えます。また、CSS セレクターでチェック範囲を指定することも可能です。

## インストール

- ClickOnce: https://yanagi.blob.core.windows.net/clickonce-omawari/setup.exe
- ZIP アーカイブ（アップデート非対応）:  https://github.com/daruyanagi/Omawari/releases

## 使い方

![](https://cdn-ak.f.st-hatena.com/images/fotolife/d/daruyanagi/20171007/20171007192510.png)

初回起動時フォルダー選択ダイアログが現れるので（アップデート時にも現れます、ごめんなさい）、適当なフォルダーを選びます。「Omawari」はそこに設定ファイルやログデータを保管します。OneDrive などのクラウドストレージを指定すると、複数環境で状況を同期することができて便利だと思います。

![](https://cdn-ak.f.st-hatena.com/images/fotolife/d/daruyanagi/20171007/20171007192611.png)

起動したら、更新チェックルールを登録します。

![](https://cdn-ak.f.st-hatena.com/images/fotolife/d/daruyanagi/20171007/20171007192844.png)

- Name: わかりやすい名前を付けましょう（何もつけない場合は適当な名前が入力されます）
- Target: チェックする URL を指定します
- Selectors: CSS セレクターを指定してください（フッターなら footer、<div class="foo"> ならば .foo みたいな感じ）
- Interval: チェック間隔を秒単位で指定します

［Test］ボタンを押して“Success”になるのを確認したら、［OK］ボタンで登録を完了してください。

ルールを登録したら、［Start］ボタンで監視を開始します。接待ダイアログで Auto Start を有効化しておくと、アプリケーションの起動と同時に監視が始まります。

## 更新情報

- http://blog.daruyanagi.jp/archive/category/Omawari
