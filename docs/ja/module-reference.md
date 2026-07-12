# モジュールリファレンス

## Dashboard

- **目的:** メインメニュー、概要指標、各モジュールへのルーティング。
- **主なファイル:** `Forms/FormDashboardHtml.cs`, `ui/dashboard/*`。
- **依存:** ログインユーザー、オペレーター、シフト、WebView2、Repository。
- **入力:** 認証済みユーザー、紐づくオペレーター、シフト、locale、`open:*` メッセージ。
- **出力:** HTML/WinForms 画面の起動、初期 payload、概要件数。
- **リスク:** ユーザーにオペレーター/シフトがない場合、Dashboard を開けません。WebView2 がない場合は白画面になります。

## Operadores

- **目的:** オペレーター情報の検索と保守。
- **主なファイル:** `HTMLFormOperators.cs`, `ui/operators/*`, `OperatorRepository.cs`。
- **依存:** SQLite、`Operators`。
- **リスク:** `CodigoFJ` の不整合はログイン、タスク、出勤、スケジュールに影響します。

## Atribuicoes

- **目的:** オペレーターをリーダー/グループへ紐づけます。
- **依存:** `Assignments`, `GroupLeaders`, `Operators`。
- **リスク:** 誤った紐づけは運用フローと帳票に影響します。

## Relatorios

- **目的:** 運用データの集計と専用帳票のハブ。
- **主な対象:** Hikitsugui、Follow、Tasks、Operadores、Presenca、Gerencial de Producao、MasterCard、PR、CL、Sobra de Peca。
- **入力:** 帳票選択、期間、シフト、セクター、グループ、ステータス、検索語など。
- **出力:** 集計表示、詳細一覧、印刷/エクスポート。
- **リスク:** 大きな期間やネットワーク DB では処理が遅くなる場合があります。

## Presenca/Layout

- **目的:** セクター/シフト別の出勤状況と配置を表示します。
- **依存:** `OperatorPresence`, `OperatorPositions`, `OperatorSchedule`, CSV スケジュール。
- **リスク:** CSV 不備や未登録オペレーターは表示漏れの原因になります。

## Haidai

- **目的:** 運用パネルと TV/モニター向け HTML エクスポート。
- **依存:** SQLite、`HaidaiExportDirectory`。
- **リスク:** エクスポート先フォルダーに権限がないと公開画面が更新されません。

## Follow-up

- **目的:** 発生事項、保留事項、理由、種類、設備、対応ステータスの管理。
- **依存:** `FollowUps`, `FollowUpReasons`, `FollowUpTypes`, `Equipments`, `Locals`。
- **リスク:** 補助マスタ不足は記録品質と分析品質を下げます。

## Tarefas

- **目的:** シフト、担当者、ステータス別のタスク管理。
- **依存:** `Tasks`, `TaskStatusHistory`。
- **出力:** タスク、ステータス履歴、未完了/完了/遅延の集計。

## Master Card

- **目的:** 進行中カードと follow-up 対象の管理。
- **依存:** SQLite、`MasterCardModuleService` による schema 保証。
- **出力:** カード、履歴、Dashboard 件数、帳票。

## Monitor de Producao

- **目的:** 設備イベントのインポート、現在状態、履歴、Kadouritsu、EC2、G-Bareru 予測の表示。
- **主なファイル:** `HTMLFormProductionMonitor.cs`, `ProductionFileImporter.cs`, `ProductionPlanDatImporter.cs`, `ProductionAnalyticsService.cs`, `ui/production-monitor/*`。
- **入力:** `yyMMdd_211D_E.txt`, `yyMMdd_2400_E.txt`, DAT 計画ファイル。
- **リスク:** ファイル形式、エンコード、BAT timeout、ネットワーク遅延、未知ステータス、DB 設定ミス。

## Gerencial de Producao

- **目的:** 期間、セクター、ローカル、シフト、グループ、オペレーター、設備、Part Code、リーダー別の製造指標を集計します。
- **出力:** 概要、ランキング、シフト比較、グループ比較、日別傾向、出勤との突合、アラート。
- **リスク:** Haidai、出勤、設備、イベントの整合性に依存します。

## ProductionMonitorProbe

- **目的:** 製造モニターの診断、検証、監査、保守用 CLI。
- **主なコマンド:** `schema-check`, `schema-repair`, `db-index-check`, `production-diagnostics`, `production-audit`, `status-report`, `import`, `import-profile`, `machine-cleanup`, `dashboard`。
- **リスク:** `import`, `schema-repair`, `ec2-reset-latest`, `machine-cleanup --apply` はデータを変更する可能性があります。実行前に `DatabasePath` を確認してください。

## Hikitsugui

- **目的:** 引き継ぎ情報、読了、返信、修正、添付の管理。
- **対象:** 作成画面、リーダー閲覧画面、管理者閲覧画面、OperatorApp 閲覧画面。
- **リスク:** 添付フォルダーまたは DB に問題があると読了履歴が残らない可能性があります。

## Sobra de Peca

- **目的:** 余剰部品の登録と確認。
- **入力:** ロット、オペレーター、設備、品目、重量、数量、shain、リーダー、備考。
- **出力:** 登録一覧、期間/シフト/設備/品目別帳票、数量と重量の合計。

## PR と CL

- **目的:** PR/CL 文書の作成、管理、帳票検索。
- **依存:** カテゴリ、優先度、セクター、オペレーター、Excel テンプレート、出力フォルダー、ClosedXML。
- **リスク:** テンプレートやフォルダーがないと文書を生成できません。

## Yukyu/Paid Leave

- **目的:** 有休/休暇関連の確認と管理。
- **依存:** `AcompYukyu`, `YukyuConferencia`, `YukyuTodoke`, `YukyuFolhaControle`, `TodokeMotivo`。

## Administracao

- **目的:** 運用、製造、Haidai、Presence、Follow-up、帳票、サポートで使う基本マスタを管理します。
- **詳細:** `docs/admin-panel.md` を参照してください。

## Controle de acesso

- **目的:** ユーザー、権限、パスワード、`CodigoFJ` の管理。
- **リスク:** `CodigoFJ` が不正なユーザーは Dashboard を正しく開けない場合があります。

## OperatorApp

- **目的:** オペレーター用 Hikitsugui 閲覧アプリ。
- **リスク:** TeamOps 本体と同じ `DatabasePath` を参照する必要があります。

## インフラ、設定、データ

- **目的:** 設定、エンティティ、検証、SQLite、migration、Repository、Service を提供します。
- **主な構成:** `TeamOps.Config/*`, `TeamOps.Core/*`, `TeamOps.Data/*`, `TeamOps.UI/App.config`, `TeamOps.OperatorApp/App.config`。
- **リスク:** UI、OperatorApp、Probe の DB 設定が異なると別データを参照します。
