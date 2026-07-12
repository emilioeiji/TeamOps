# TeamOps - ドキュメント

## 概要

TeamOps は工場運用を支援する社内システムです。WinForms、WebView2、HTML/CSS/JavaScript モジュール、SQLite を組み合わせ、リーダー、オペレーター、管理者がシフト、作業者、設備、タスク、引き継ぎ、帳票、製造データを確認できるようにします。

この日本語版は `docs/` 配下の PT-BR ドキュメントと同じ情報を参照できるようにしたものです。画面名、ファイル名、コマンド名、設定名は実システムに合わせて原文の表記を残しています。

## 対象者

- 工場オペレーター。
- グループリーダー、現場リーダー、管理担当者。
- ユーザー、権限、基本マスタを管理する管理者。
- SQLite、共有フォルダー、配布、サポートを担当する技術チーム。

## 主なモジュール

- Dashboard。
- Operadores と Atribuicoes。
- Relatorios。
- Presenca/Layout。
- Haidai。
- Follow-up。
- Tarefas。
- Master Card。
- Monitor de Producao。
- Hikitsugui。
- Sobra de Peca。
- PR と CL。
- Yukyu/Paid Leave。
- Administracao。
- Controle de acesso。
- OperatorApp。

## 主要ドキュメント

- `docs/user-manual.md`: ユーザー向け操作手順。
- `docs/module-reference.md`: モジュール別の目的、依存関係、入力、出力、リスク。
- `docs/admin-panel.md`: 管理パネルのボタン、マスタ、読み取り専用画面、ログ。
- `docs/production-monitor-guide.md`: 製造モニター、インポート、EC2、Probe コマンド。
- `docs/architecture.md`: 構成とデータフロー。
- `docs/deployment.md`: 配布、設定、公開前チェック。
- `docs/troubleshooting.md`: よくある障害と確認手順。
- `docs/i18n-audit-report.md`: 多言語対応の監査結果。

## ドキュメント更新ルール

新しい Markdown ドキュメントを `docs/` に追加した場合は、必ず `docs/index.html` にも統合し、HTML だけで内容を読める状態にします。PT-BR と JP の両方で読めるようにすることを標準とします。
