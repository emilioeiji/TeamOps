# アーキテクチャ

## 概要

TeamOps は複数プロジェクトで構成されます。

- `TeamOps.UI`: メイン WinForms アプリ。WebView2 で HTML 画面を表示します。
- `TeamOps.OperatorApp`: オペレーター向け Hikitsugui 閲覧アプリ。
- `TeamOps.Core`: ドメインモデル、サービス、共通ロジック。
- `TeamOps.Data`: SQLite 接続、Repository、migration。
- `TeamOps.Config`: 設定値とパス取得。
- `ProductionMonitorProbe`: 製造モニター用 CLI 診断ツール。

## UI 構成

WinForms の Form が WebView2 をホストし、`ui/<module>/` 配下の HTML/CSS/JavaScript を読み込みます。画面操作は WebView2 メッセージを通じて C# 側へ送られ、Repository や Service が DB を操作します。

## データ層

SQLite を中心に、Dapper と Repository パターンでアクセスします。migration と seed は `TeamOps.Data` が管理します。

## 主なデータフロー

1. ユーザーがログインします。
2. Dashboard がユーザー、オペレーター、シフト、概要件数を取得します。
3. HTML ボタンから `open:*` メッセージが送信されます。
4. 対応する WinForms/WebView2 画面が開きます。
5. JS から C# へ操作が送られます。
6. Service/Repository が SQLite、ファイル、共有フォルダーを読み書きします。
7. 結果が HTML へ戻ります。

## 外部ファイルと設定

- SQLite DB パス。
- PR/CL テンプレートと出力フォルダー。
- Hikitsugui 添付フォルダー。
- Haidai HTML エクスポート先。
- 製造 TXT/DAT/EC2 ファイル。
- BAT 実行パス。

## リスク

- WebView2 がない場合、HTML 画面が表示されません。
- DB パスが環境ごとに異なるとデータ不一致になります。
- ネットワーク共有の遅延や権限不足は保存、添付、インポートに影響します。
- migration 未適用や seed 不足は画面エラーにつながります。

## 保守方針

- UI 表示は `ui/<module>` に閉じ、業務ロジックは C# Service/Repository に寄せます。
- 新しいドキュメントは `docs/index.html` に統合します。
- 多言語化する場合は PT-BR と JP の両方で表示確認します。
