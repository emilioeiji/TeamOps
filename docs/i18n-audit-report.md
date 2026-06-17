# Auditoria de Internacionalizacao PT-BR / JA - TeamOps

Data da auditoria: 2026-06-17

Escopo: TeamOps.UI, TeamOps.OperatorApp, Forms WinForms, HTML Forms, WebView UIs, dashboards e relatorios.

Restricoes respeitadas: nenhuma regra de negocio, calculo ou banco de dados foi alterado. Esta entrega documenta a auditoria e o padrao de apresentacao bilíngue.

## Sumario executivo

| Metrica | Total |
| --- | ---: |
| Arquivos auditados | 97 |
| Telas/superficies auditadas | 66 |
| Forms C# auditados | 37 |
| UIs de formulario auditadas | 14 |
| Relatorios auditados | 11 |
| Dashboards auditados | 3 |
| OperatorApp auditado | 1 |
| Totalmente bilíngues | 48 |
| Parcialmente bilíngues | 7 |
| Fora do padrao bilíngue | 9 |
| Tecnico/sem UI direta | 2 |

## Padrao de referencia encontrado

### WebView moderna

Arquivos de referencia: `TeamOps.UI/ui/access-control/app.js`, `TeamOps.UI/ui/follow-chart/app.js`, `TeamOps.UI/ui/sobra-de-peca/app.js`, `TeamOps.UI/ui/production-monitor/app.js`.

Padrao:

- `const I18N = { "pt-BR": { ... }, "ja-JP": { ... } }`.
- Estado de idioma vindo de `Program.CurrentLocale`.
- Funcao `t(key)` para leitura de textos.
- `applyLocale()` aplica titulo, labels, botoes, colunas, placeholders e opcoes.
- `localizedName(name, nameJp)` ou equivalente para dados mestres e nomes.
- Textos tecnicos preservados: `EC2`, `DCS`, `BUNKATSU`, `CAD`, `FJ`, `ID`.

### HTML Forms C#

Arquivos de referencia: `TeamOps.UI/Forms/HTMLFormAccessControl.cs`, `TeamOps.UI/Forms/HTMLFormHaidai.cs`, `TeamOps.UI/Forms/HTMLFormOperators.cs`, `TeamOps.UI/Forms/HTMLFormPresenceReport.cs`.

Padrao:

- `Text = L("Portugues", "Japones")`.
- Mensagens de excecao, erro, sucesso e validacao usando `L(pt, ja)`.
- Payload inicial envia `locale = Program.CurrentLocale`.
- Consultas de lookup retornam `NamePt` e `NameJp`, ou nome ja resolvido por locale.

### Dados mestres e nomes

Padrao encontrado:

- Operador: `NameRomanji` para PT-BR e `NameNihongo` para JA.
- Turno, setor, grupo, local: `NamePt` / `NameJp`.
- Fallback tecnico permitido: `Turno {Id}`, `Setor {Id}`, `Grupo {Id}`, `LocalId`.

## Glossario recomendado

| PT-BR | JA |
| --- | --- |
| Operador | 作業者 |
| Setor | 工程 |
| Maquina | 設備 |
| Data | 日付 |
| Producao | 生産 |
| Presenca | 出勤 |
| Horas Extras | 残業 |
| Salvar | 保存 |
| Atualizar | 更新 |
| Pesquisar | 検索 |
| Cancelar | キャンセル |
| Excluir | 削除 |
| Importar | 取込 |
| Exportar | 出力 |
| Imprimir | 印刷 |
| Rodando | 運転 |
| Parado | 停止 |
| Erro | 異常 |
| Suspenso | 除外 |
| Desconsiderado | 除外 |
| Ativo | 有効 |
| Inativo | 無効 |
| Aviso | 通知 |
| Erro ao carregar | 読込エラー |
| Salvo com sucesso | 保存しました |
| Registro excluido | 削除しました |

## Termos tecnicos que nao devem ser traduzidos

Manter como estao: `MachineCode`, `MachineId`, `OperatorId`, `ShiftId`, `LocalId`, `SectorId`, `EC2`, `DCS`, `BUNKATSU`, `CAD`, `ID`, `UUID`, `FJ`.

## Inventario por tela

| Tipo | Tela | Arquivo principal | Status |
| --- | --- | --- | --- |
| Dashboard | dashboard | `TeamOps.UI/ui/dashboard/app.js` | Bilíngue |
| Dashboard | presence | `TeamOps.UI/ui/presence/app.js` | Nao bilíngue |
| Dashboard | production-monitor | `TeamOps.UI/ui/production-monitor/app.js` | Bilíngue |
| Report | follow-chart | `TeamOps.UI/ui/follow-chart/app.js` | Bilíngue |
| Report | follow-operator-report | `TeamOps.UI/ui/follow-operator-report/app.js` | Bilíngue |
| Report | follow-report | `TeamOps.UI/ui/follow-report/app.js` | Bilíngue |
| Report | follow-single-report | `TeamOps.UI/ui/follow-single-report/app.js` | Bilíngue |
| Report | mastercard-report | `TeamOps.UI/ui/mastercard-report/app.js` | Bilíngue |
| Report | operator-manager-report | `TeamOps.UI/ui/operator-manager-report/app.js` | Bilíngue |
| Report | pr-cl-report | `TeamOps.UI/ui/pr-cl-report/app.js` | Bilíngue |
| Report | presence-report | `TeamOps.UI/ui/presence-report/app.js` | Bilíngue |
| Report | reports | `TeamOps.UI/ui/reports/app.js` | Bilíngue |
| Report | sobra-de-peca-report | `TeamOps.UI/ui/sobra-de-peca-report/app.js` | Bilíngue |
| Report | tasks-report | `TeamOps.UI/ui/tasks-report/app.js` | Bilíngue |
| UI Form | access-control | `TeamOps.UI/ui/access-control/app.js` | Bilíngue |
| UI Form | admin | `TeamOps.UI/ui/admin/app.js` | Bilíngue |
| UI Form | follow-up | `TeamOps.UI/ui/follow-up/app.js` | Bilíngue |
| UI Form | haidai | `TeamOps.UI/ui/haidai/app.js` | Parcial |
| UI Form | hikitsugui-create | `TeamOps.UI/ui/hikitsugui-create/app.js` | Bilíngue |
| UI Form | hikitsugui-leader-read | `TeamOps.UI/ui/hikitsugui-leader-read/app.js` | Bilíngue |
| UI Form | hikitsugui-reader | `TeamOps.UI/ui/hikitsugui-reader/app.js` | Bilíngue |
| UI Form | mastercard | `TeamOps.UI/ui/mastercard/app.js` | Bilíngue |
| UI Form | operators | `TeamOps.UI/ui/operators/app.js` | Bilíngue |
| UI Form | paidleave | `TeamOps.UI/ui/paidleave/app.js` | Nao bilíngue |
| UI Form | pr-cl | `TeamOps.UI/ui/pr-cl/app.js` | Bilíngue |
| UI Form | presence-layout | `TeamOps.UI/ui/presence-layout/app.js` | Bilíngue |
| UI Form | sobra-de-peca | `TeamOps.UI/ui/sobra-de-peca/app.js` | Bilíngue |
| UI Form | tasks | `TeamOps.UI/ui/tasks/app.js` | Bilíngue |
| Form | FormAccessControl | `TeamOps.UI/Forms/FormAccessControl.cs` | Tecnico/sem UI |
| Form | FormAddUser | `TeamOps.UI/Forms/FormAddUser.cs` | Nao bilíngue |
| Form | FormAssignments | `TeamOps.UI/Forms/FormAssignments.cs` | Nao bilíngue |
| Form | FormChangePassword | `TeamOps.UI/Forms/FormChangePassword.cs` | Nao bilíngue |
| Form | FormCL | `TeamOps.UI/Forms/FormCL.cs` | Parcial |
| Form | FormDashboardHtml | `TeamOps.UI/Forms/FormDashboardHtml.cs` | Parcial |
| Form | FormHikitsuguiPreview | `TeamOps.UI/Forms/FormHikitsuguiPreview.cs` | Tecnico/sem UI |
| Form | FormHikitsuguiReader | `TeamOps.UI/Forms/FormHikitsuguiReader.cs` | Parcial |
| Form | FormLogin | `TeamOps.UI/Forms/FormLogin.cs` | Nao bilíngue |
| Form | FormPaidLeaveTracking | `TeamOps.UI/Forms/FormPaidLeaveTracking.cs` | Nao bilíngue |
| Form | FormPR | `TeamOps.UI/Forms/FormPR.cs` | Parcial |
| Form | FormPresenceLayout | `TeamOps.UI/Forms/FormPresenceLayout.cs` | Nao bilíngue |
| Form | HTMLFormAccessControl | `TeamOps.UI/Forms/HTMLFormAccessControl.cs` | Bilíngue |
| Form | HTMLFormAdmin | `TeamOps.UI/Forms/HTMLFormAdmin.cs` | Bilíngue |
| Form | HTMLFormFollowChart | `TeamOps.UI/Forms/HTMLFormFollowChart.cs` | Bilíngue |
| Form | HTMLFormFollowOperatorReport | `TeamOps.UI/Forms/HTMLFormFollowOperatorReport.cs` | Bilíngue |
| Form | HTMLFormFollowReport | `TeamOps.UI/Forms/HTMLFormFollowReport.cs` | Bilíngue |
| Form | HTMLFormFollowSingleReport | `TeamOps.UI/Forms/HTMLFormFollowSingleReport.cs` | Bilíngue |
| Form | HTMLFormFollowUp | `TeamOps.UI/Forms/HTMLFormFollowUp.cs` | Bilíngue |
| Form | HTMLFormHaidai | `TeamOps.UI/Forms/HTMLFormHaidai.cs` | Bilíngue |
| Form | HTMLFormHikitsuguiReader | `TeamOps.UI/Forms/HTMLFormHikitsuguiReader.cs` | Bilíngue |
| Form | HTMLFormMasterCard | `TeamOps.UI/Forms/HTMLFormMasterCard.cs` | Bilíngue |
| Form | HTMLFormMasterCardReport | `TeamOps.UI/Forms/HTMLFormMasterCardReport.cs` | Bilíngue |
| Form | HTMLFormOperatorManagerReport | `TeamOps.UI/Forms/HTMLFormOperatorManagerReport.cs` | Bilíngue |
| Form | HTMLFormOperators | `TeamOps.UI/Forms/HTMLFormOperators.cs` | Bilíngue |
| Form | HTMLFormPrCl | `TeamOps.UI/Forms/HTMLFormPrCl.cs` | Bilíngue |
| Form | HTMLFormPrClReport | `TeamOps.UI/Forms/HTMLFormPrClReport.cs` | Bilíngue |
| Form | HTMLFormPresenceLayout | `TeamOps.UI/Forms/HTMLFormPresenceLayout.cs` | Bilíngue |
| Form | HTMLFormPresenceReport | `TeamOps.UI/Forms/HTMLFormPresenceReport.cs` | Bilíngue |
| Form | HTMLFormProductionMonitor | `TeamOps.UI/Forms/HTMLFormProductionMonitor.cs` | Parcial |
| Form | HTMLFormReports | `TeamOps.UI/Forms/HTMLFormReports.cs` | Bilíngue |
| Form | HTMLFormSobraDePeca | `TeamOps.UI/Forms/HTMLFormSobraDePeca.cs` | Bilíngue |
| Form | HTMLFormSobraDePecaReport | `TeamOps.UI/Forms/HTMLFormSobraDePecaReport.cs` | Bilíngue |
| Form | HTMLFormTasks | `TeamOps.UI/Forms/HTMLFormTasks.cs` | Bilíngue |
| Form | HTMLFormTasksReport | `TeamOps.UI/Forms/HTMLFormTasksReport.cs` | Bilíngue |
| Form | HTMLHikitsuguiCreate | `TeamOps.UI/Forms/HTMLHikitsuguiCreate.cs` | Parcial |
| Form | HTMLHikitsuguiLeaderRead | `TeamOps.UI/Forms/HTMLHikitsuguiLeaderRead.cs` | Bilíngue |
| OperatorApp | hikitsugui-operator-read | `TeamOps.OperatorApp/ui/hikitsugui-operator-read/app.js` | Nao bilíngue |

## Telas totalmente conformes

Principais conformes: `access-control`, `dashboard`, `follow-chart`, `follow-report`, `follow-up`, `hikitsugui-reader`, `mastercard`, `mastercard-report`, `operator-manager-report`, `operators`, `presence-layout`, `presence-report`, `production-monitor`, `reports`, `sobra-de-peca`, `sobra-de-peca-report`, `tasks`, `tasks-report`, e os HTML Forms correspondentes com `L(pt, ja)`.

## Telas parcialmente conformes

| Tela | Campo/problema | Atual | Esperado |
| --- | --- | --- | --- |
| `ui/haidai` | Estrutura i18n | Textos PT/JA misturados sem `I18N` central | Migrar para `I18N["pt-BR"]` e `I18N["ja-JP"]` |
| `HTMLFormProductionMonitor` | Wrapper C# | Tem PT/JA por dados e titulo, mas sem `L(pt, ja)` consistente | Padronizar titulo/mensagens com `L()` |
| `HTMLHikitsuguiCreate` | Wrapper C# | Textos bilíngues parciais | Usar `L()` para titulo e mensagens |
| `FormCL` | WinForms legado | Mistura PT e JA em codigo | Substituir literais por `L()` ou recurso localizado |
| `FormPR` | WinForms legado | Mistura PT e JA em codigo | Substituir literais por `L()` ou recurso localizado |
| `FormDashboardHtml` | Wrapper legado | Resolucao parcial por locale | Padronizar com `L()` |
| `FormHikitsuguiReader` | WinForms legado | Leitura parcial com textos fixos | Padronizar labels/mensagens |

## Telas fora do padrao

| Tela | Campo/problema | Atual | Esperado |
| --- | --- | --- | --- |
| `ui/presence` | Dashboard de presenca | Textos fixos em PT-BR | Criar `I18N` PT-BR/JA e aplicar `Program.CurrentLocale` |
| `ui/paidleave` | Paid leave | Textos fixos em PT-BR | Criar `I18N` PT-BR/JA |
| `TeamOps.OperatorApp/ui/hikitsugui-operator-read` | OperatorApp | Textos fixos em PT-BR | Criar `I18N` PT-BR/JA e receber locale |
| `FormAddUser` | WinForms legado | Mensagens/titulo em PT | Usar `L(pt, ja)` |
| `FormAssignments` | WinForms legado | Mensagens/titulo em PT | Usar `L(pt, ja)` |
| `FormChangePassword` | WinForms legado | Mensagens/titulo em PT | Usar `L(pt, ja)` |
| `FormLogin` | Login | Textos majoritariamente em JA/legado sem par PT | Definir PT-BR/JA por locale |
| `FormPaidLeaveTracking` | Paid leave wrapper | Textos em PT | Usar `L(pt, ja)` |
| `FormPresenceLayout` | Presenca layout wrapper | Textos em PT | Usar `L(pt, ja)` |

## Auditoria de relatorios

| Relatorio | Status | Observacao |
| --- | --- | --- |
| Relatorio de Presenca | Bilíngue | `I18N` em `presence-report/app.js`; atencao a chaves JA ainda em ingles em alguns titulos operacionais. |
| Relatorio de Producao / Production Monitor | Bilíngue | WebView tem `I18N`; parte PT usa termos sem acento por padrao ASCII do projeto. |
| Relatorio de Operadores | Bilíngue | `operator-manager-report` usa `I18N` e dados `NameJp`. |
| Relatorio de Sobra de Pecas | Bilíngue | `sobra-de-peca-report` usa `I18N`. |
| Relatorios EC2 | Bilíngue parcial | EC2 aparece dentro de Production Monitor; termo tecnico nao deve ser traduzido. |
| Relatorios Gerenciais | Bilíngue | `presence-report` e `operator-manager-report` seguem padrao moderno. |

## Auditoria de dashboards

| Dashboard | Status | Observacao |
| --- | --- | --- |
| Dashboard Produção | Bilíngue | `production-monitor` usa `I18N`; status operacionais precisam manter glossario unico. |
| Dashboard Operador | Nao bilíngue | OperatorApp precisa receber locale e aplicar `I18N`. |
| Dashboard Presenca | Nao bilíngue | `ui/presence` tem textos fixos em PT-BR. |
| Dashboard EC2 | Bilíngue parcial | Integrado ao Production Monitor; manter `EC2` tecnico sem traducao. |

## Status operacionais

Padrao recomendado:

- `Rodando / 運転`
- `Parado / 停止`
- `Erro / 異常`
- `Suspenso / 除外`
- `Desconsiderado / 除外`
- `Ativo / 有効`
- `Inativo / 無効`

Achado: `Suspenso`, `Desconsiderado`, `Ignored`, `Inactive` e `Suspensa` aparecem em pontos diferentes. Recomenda-se centralizar status de producao em uma tabela/constante de apresentacao por locale, preservando codigos tecnicos.

## Botoes

Padrao recomendado:

- `Salvar / 保存`
- `Atualizar / 更新`
- `Pesquisar / 検索`
- `Cancelar / キャンセル`
- `Excluir / 削除`
- `Importar / 取込`
- `Exportar / 出力`
- `Imprimir / 印刷`

Achado: telas modernas ja usam chaves `save`, `refresh`, `search`, `cancel`, `delete`, `import`, `export`. Telas legadas WinForms ainda possuem literais.

## Mensagens do sistema

Padrao recomendado:

- Toast: `Aviso / 通知`
- Erro: `Erro ao carregar / 読込エラー`
- Sucesso: `Salvo com sucesso / 保存しました`
- Exclusao: `Registro excluido / 削除しました`

Achado: HTML Forms recentes retornam mensagens por `L(pt, ja)`. Telas legadas e OperatorApp ainda precisam de padronizacao.

## Consistencia visual

Itens validados:

- Estrutura moderna usa cards compactos, tabs, filtros e tabelas com `applyLocale()`.
- Textos longos em JA podem aumentar largura; telas com tabelas fixas devem manter `overflow-wrap` ou colunas flexiveis.
- Evitar PT em `pt-BR` e ingles em `ja-JP`; usar japones real nas chaves `ja-JP`.
- Manter termos tecnicos em monospace/labels originais quando forem IDs, codigos ou siglas.

## Arquivo x quantidade de ajustes recomendados

| Arquivo/tela | Ajustes recomendados |
| --- | ---: |
| `TeamOps.UI/ui/presence/app.js` | 20+ |
| `TeamOps.UI/ui/paidleave/app.js` | 20+ |
| `TeamOps.OperatorApp/ui/hikitsugui-operator-read/app.js` | 15+ |
| `TeamOps.UI/ui/haidai/app.js` | 20+ |
| `TeamOps.UI/Forms/FormAddUser.cs` | 5+ |
| `TeamOps.UI/Forms/FormAssignments.cs` | 5+ |
| `TeamOps.UI/Forms/FormChangePassword.cs` | 5+ |
| `TeamOps.UI/Forms/FormLogin.cs` | 5+ |
| `TeamOps.UI/Forms/FormPaidLeaveTracking.cs` | 5+ |
| `TeamOps.UI/Forms/FormPresenceLayout.cs` | 5+ |
| `TeamOps.UI/Forms/FormCL.cs` | 5+ |
| `TeamOps.UI/Forms/FormPR.cs` | 5+ |
| `TeamOps.UI/Forms/FormHikitsuguiReader.cs` | 5+ |

## Ajustes realizados nesta auditoria

- Criado este relatorio de auditoria bilíngue.
- Nenhuma regra de negocio, calculo ou schema foi alterado.
- Nenhuma tela foi migrada nesta rodada para evitar mudancas amplas sem revisao visual por tela.

## Pendencias restantes

1. Migrar `ui/presence`, `ui/paidleave` e OperatorApp para `I18N`.
2. Padronizar wrappers WinForms legados com `L(pt, ja)`.
3. Revisar chaves `ja-JP` que ainda usam ingles em vez de japones.
4. Criar um glossario compartilhado de status e botoes.
5. Adicionar uma verificacao automatica simples para impedir novos textos visiveis fora de `I18N`/`L()`.
6. Fazer QA visual em PT-BR e JA nas telas com tabelas largas.

## Sugestao de padronizacao futura

Criar um pequeno modulo compartilhado de apresentacao:

- `CommonLabels` para botoes/status.
- `I18N` por tela para textos especificos.
- Helper de validacao que acusa chaves ausentes entre `pt-BR` e `ja-JP`.
- Checklist de PR: titulo, botoes, labels, placeholders, colunas, toast, alert, confirm, CSV e impressao.

