# I18N 監査レポート

## 目的

TeamOps の画面表示を PT-BR と JP で扱えるようにするため、表示文字列、ローカライズ方式、未対応箇所を整理します。

## 現状

- 一部の HTML 画面は `I18N` オブジェクトや `L(pt, ja)` のような helper を使っています。
- すべての画面が完全に同じ方式に統一されているわけではありません。
- WinForms 側のラベル、メッセージ、ダイアログには固定文字列が残る可能性があります。
- `ui/presence`、`ui/paidleave`、OperatorApp などは追加確認が必要です。

## 推奨方針

- 画面ごとに PT-BR と JP の文言を同じ構造で管理します。
- ボタン、ステータス、エラーメッセージ、列名、placeholder、toast、confirm を対象にします。
- 新規画面では、最初から PT-BR/JP の両方を登録します。
- 表示文言をコード内に直接増やさないようにします。

## 監査対象

- HTML タイトル。
- ボタンラベル。
- テーブル列名。
- フィルターラベル。
- placeholder。
- empty state。
- toast/alert/confirm。
- CSV/印刷/PDF 出力文言。
- WinForms メッセージ。

## 残課題

1. すべての `ui/` 配下で固定文字列を洗い出す。
2. PT-BR と JP のキー差分を検出する。
3. OperatorApp の文言を同じ方式に統一する。
4. WinForms 側のメッセージを helper 化する。
5. QA で PT-BR と JP の両方の画面崩れを確認する。

## ドキュメント方針

`docs/` のドキュメントは PT-BR を原文とし、`docs/ja/` に JP 版を配置します。`docs/index.html` では両方を読めるようにします。
