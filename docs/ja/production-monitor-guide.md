# Production Monitor と ProductionMonitorProbe

## 目的

Monitor de Producao は設備イベント、現在状態、Kadouritsu、EC2、G-Bareru 予測などを確認するための画面です。`ProductionMonitorProbe` は同じバックエンドを使って診断、検証、監査、保守を行う CLI です。

## Monitor de Producao の基本手順

1. Dashboard から **Monitor de Producao** を開きます。
2. 対象日、セクター、ローカルを確認します。
3. 必要に応じてインポートを実行します。
4. 処理完了まで待ちます。
5. 設備カード、現在ステータス、Kadouritsu、EC2、G-Bareru 予測を確認します。

## 入力ファイル

- `yyMMdd_211D_E.txt`
- `yyMMdd_2400_E.txt`
- DAT 形式の生産計画ファイル
- EC2 Administrator 関連ファイル
- `App.config` で設定された BAT、フォルダー、DB パス

## よくある確認

- インポート対象ファイルが存在するか。
- ファイルが使用中またはロックされていないか。
- BAT が timeout していないか。
- `DatabasePath` が正しい DB を指しているか。
- 未知ステータスが Machine Status に登録されているか。
- EC2 の最新ファイルが想定どおりか。

## ProductionMonitorProbe の主なコマンド

| コマンド | 用途 |
| --- | --- |
| `schema-check` | schema、テーブル、列、インデックス、seed を検証します。 |
| `schema-repair` | migration と seed 修復を実行します。データ変更の可能性があります。 |
| `db-index-check` | 必要なインデックスを確認します。 |
| `production-diagnostics` | イベント数、ステータス、設備、日付範囲を診断します。 |
| `production-audit` | 製造データを監査します。 |
| `validate-reports` | 帳票計算の検証を行います。 |
| `validate-yakin-production` | 夜勤製造ルールを検証します。 |
| `validate-overtime-rules` | 残業ルールを検証します。 |
| `validate-overtime-real` | 実データで残業関連を確認します。 |
| `ec2-diagnostics` | EC2 最新状態、hash、設備、部品コードを確認します。 |
| `status-report` | ステータス分類、分数、影響を出力します。CSV 出力も可能です。 |
| `import` | 通常インポートを実行します。 |
| `import-profile` | インポートと性能測定を行います。 |
| `machine-cleanup` | 不正な設備コードを確認します。`--apply` で変更します。 |
| `machine-location-guard` | 設備とローカルの整合性を確認します。 |
| `dashboard` | バックエンドの Dashboard 計算を確認します。 |

## 実行例

```powershell
.\ProductionMonitorProbe.exe schema-check
.\ProductionMonitorProbe.exe db-index-check
.\ProductionMonitorProbe.exe production-diagnostics
.\ProductionMonitorProbe.exe status-report --start 2026-06-01 --end 2026-06-02 --sector dad --csv dad-status.csv
```

## データ変更を伴う可能性がある操作

- `schema-repair`
- `import`
- `import-profile`
- `ec2-reset-latest`
- `machine-cleanup --apply`

これらは実行前に必ず `DatabasePath`、対象日、対象セクター、バックアップ状況を確認してください。

## トラブル時の切り分け

1. `schema-check` で schema を確認します。
2. `db-index-check` で性能前提を確認します。
3. `production-diagnostics` でデータ有無を確認します。
4. `status-report` で未知ステータスや分類を確認します。
5. `ec2-diagnostics` で EC2 の取り込み状態を確認します。

## リスク

- ファイル形式やエンコードが想定外の場合、インポート結果が不正になります。
- ネットワークフォルダーが遅い場合、画面や Probe が遅くなります。
- UI、OperatorApp、Probe の DB 設定が異なると結果が一致しません。
