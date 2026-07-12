# Referencia de modulos

## Dashboard

- **Finalidade:** menu principal, indicadores resumidos e roteamento para modulos.
- **Arquivos principais:** `Forms/FormDashboardHtml.cs`, `ui/dashboard/*`.
- **Dependencias:** usuario logado, operador, turno, WebView2, repositorios.
- **Inputs:** mensagens `open:*`, locale.
- **Outputs:** abertura de forms, payload inicial para HTML.
- **Riscos:** usuario sem operador/turno impede abertura; WebView2 ausente gera tela branca.

## Operadores

- **Finalidade:** consulta/manutencao de operadores.
- **Arquivos principais:** `HTMLFormOperators.cs`, `ui/operators/*`, `OperatorRepository.cs`.
- **Dependencias:** SQLite, tabela `Operators`.
- **Inputs:** dados cadastrais de operador.
- **Outputs:** lista/alteracoes de operadores.
- **Riscos:** inconsistencias em `CodigoFJ` afetam login, tarefas e escala.

## Atribuicoes

- **Finalidade:** associar operadores a liderancas/grupos.
- **Arquivos principais:** `FormAssignments.cs`, `AssignmentRepository.cs`, `GroupLeaderRepository.cs`.
- **Dependencias:** `Assignments`, `GroupLeaders`, `Operators`.
- **Inputs:** operador, lider/grupo.
- **Outputs:** atribuicoes gravadas.
- **Riscos:** permissao administrativa exigida; atribuicoes incorretas afetam fluxo operacional.

## Relatorios

- **Finalidade:** consulta consolidada de dados operacionais.
- **Arquivos principais:** `HTMLFormReports.cs`, `ui/reports/*`, `HTMLFormOperatorManagerReport.cs`, `ui/operator-manager-report/*`, `HTMLFormPresenceReport.cs`, `ui/presence-report/*`, `OperatorManagerReportService.cs`.
- **Dependencias:** Hikitsugui, leituras, operadores e SQLite.
- **Inputs:** filtros de relatorio.
- **Outputs:** visualizacao e possiveis exportacoes/impressao.
- **Relatorio gerencial de operadores:** consolida presenca, producao, Master Card, follow-up e historico diario do operador selecionado. O percentual de producao usa minutos rodando divididos pelos minutos programados por maquina/local no periodo.
- **Relatorio de presenca:** consulta focada em escala e comparecimento, com filtros por periodo, turno, setor, grupo, status e busca por FJ/nome. Considera Haidai, registros de presenca, Yukyu/Todoke e movimentos para destacar presenca conforme, falta, Yukyu, atraso, saida antecipada e Todoke pendente.
- **Riscos:** consultas grandes podem ficar lentas em banco de rede; percentuais dependem de escala/local e eventos de maquina cadastrados corretamente.

## Presenca/Layout

- **Finalidade:** exibir presenca e posicoes de operadores.
- **Arquivos principais:** `HTMLFormPresenceLayout.cs`, `FormPresenceLayout.cs`, `ui/presence-layout/*`, `ui/presence/*`.
- **Dependencias:** `OperatorPresence`, `OperatorPositions`, `OperatorSchedule`, CSVs de agenda.
- **Inputs:** setor, turno, data, CSV de schedule.
- **Outputs:** layout/posicoes/presenca.
- **Riscos:** CSV ausente ou fora do padrao deixa operadores fora do layout.

## Haidai

- **Finalidade:** painel operacional com exportacao para monitor/TV.
- **Arquivos principais:** `HTMLFormHaidai.cs`, `HaidaiModuleService.cs`, `ui/haidai/*`.
- **Dependencias:** SQLite, `HaidaiExportDirectory`.
- **Inputs:** dados operacionais do modulo.
- **Outputs:** visualizacao e HTML exportado.
- **Riscos:** pasta de exportacao sem permissao impede atualizacao de monitor publico.

## Follow-up

- **Finalidade:** registrar e acompanhar ocorrencias/pendencias.
- **Arquivos principais:** `HTMLFormFollowUp.cs`, `ui/follow-up/*`, `FollowUpRepository.cs`, repositorios de motivos/tipos/equipamentos.
- **Dependencias:** `FollowUps`, `FollowUpReasons`, `FollowUpTypes`, `Equipments`, `Locals`.
- **Inputs:** tipo, motivo, equipamento, descricao, responsavel/status.
- **Outputs:** registros e relatorios.
- **Riscos:** cadastros auxiliares incompletos reduzem qualidade do registro.

## Relatorios de Follow-up

- **Finalidade:** analise por operador, consolidado, grafico ou item unico.
- **Arquivos principais:** `HTMLFormFollowReport.cs`, `HTMLFormFollowOperatorReport.cs`, `HTMLFormFollowSingleReport.cs`, `HTMLFormFollowChart.cs`, `ui/follow-*/*`.
- **Dependencias:** dados de follow-up e WebView2.
- **Inputs:** filtros e comandos de impressao/PDF.
- **Outputs:** relatorios e PDFs.
- **Riscos:** impressao/PDF depende do WebView2.

## Tarefas

- **Finalidade:** controle de tarefas por turno/responsavel/status.
- **Arquivos principais:** `HTMLFormTasks.cs`, `HTMLFormTasksReport.cs`, `ui/tasks/*`, `ui/tasks-report/*`.
- **Dependencias:** tabelas `Tasks` e `TaskStatusHistory`.
- **Inputs:** tarefa, responsavel, prazo, status.
- **Outputs:** tarefas e historico de status.
- **Riscos:** status inconsistentes afetam contagem de tarefas abertas.

## Master Card

- **Finalidade:** acompanhamento de itens em andamento/follow-up.
- **Arquivos principais:** `HTMLFormMasterCard.cs`, `HTMLFormMasterCardReport.cs`, `MasterCardModuleService.cs`, `ui/mastercard/*`.
- **Dependencias:** schema garantido pelo service e SQLite.
- **Inputs:** dados do card e status.
- **Outputs:** cards, relatorios e contagens no dashboard.
- **Riscos:** schema dinamico precisa ser garantido antes das consultas.

## Monitor de producao

- **Finalidade:** importar eventos de maquinas e apresentar status atual/historico.
- **Arquivos principais:** `HTMLFormProductionMonitor.cs`, `ProductionFileImporter.cs`, `ProductionPlanDatImporter.cs`, `ProductionAnalyticsService.cs`, `ui/production-monitor/*`.
- **Dependencias:** `Machines`, `MachineEvents`, `MachineCurrentStatus`, `MachineStatuses`, arquivos TXT/DAT, BAT, EC2 Administrator e configuracoes em `App.config`.
- **Inputs:** arquivos `yyMMdd_211D_E.txt`, `yyMMdd_2400_E.txt`, DATs de plano.
- **Outputs:** eventos importados, maquinas criadas, status atual, indicadores de kadouritsu, EC2, codigos parametrizados e previsao G-Bareru quando configurada.
- **Probe:** comandos documentados em `docs/production-monitor-guide.md`.
- **Riscos:** layout/encoding inesperado, timeout do BAT, rede lenta, status desconhecido sem classificacao setorial, config apontando para banco errado.

## Hikitsugui

- **Finalidade:** passagem de informacao, leitura, respostas, correcoes e anexos.
- **Arquivos principais:** `HTMLHikitsuguiCreate.cs`, `HTMLHikitsuguiLeaderRead.cs`, `HTMLFormHikitsuguiReader.cs`, `TeamOps.OperatorApp/Forms/HTMLHikitsuguiOperatorRead.cs`, `ui/hikitsugui-*/*`.
- **Dependencias:** `Hikitsugui`, `HikitsuguiResponses`, `HikitsuguiCorrections`, `HikitsuguiReads`, `HikitsuguiAttachments`, pasta de anexos.
- **Inputs:** registro, resposta, leitura, anexo.
- **Outputs:** comunicados e rastreabilidade de leitura.
- **Riscos:** pasta de anexos indisponivel; leituras podem nao ser registradas se houver erro de banco.

## Sobra de peca

- **Finalidade:** registrar e acompanhar sobra de pecas.
- **Arquivos principais:** `HTMLFormSobraDePeca.cs`, `ui/sobra-de-peca/*`, `SobraDePecaRepository.cs`.
- **Dependencias:** tabela `SobraDePeca`, operador atual.
- **Inputs:** dados da sobra.
- **Outputs:** registros consultaveis.
- **Riscos:** falta de padronizacao nos campos pode dificultar relatorio.

## PR

- **Finalidade:** criar/controlar documentos PR.
- **Arquivos principais:** `FormPR.cs`, `PRRepository.cs`, `PRCategoriaRepository.cs`, `PRPrioridadeRepository.cs`.
- **Dependencias:** `PR`, categorias, prioridades, setores, operadores, `PRTemplate`, `PRDirectory`.
- **Inputs:** dados do PR.
- **Outputs:** registro e arquivo gerado.
- **Riscos:** template/pasta ausente.

## CL

- **Finalidade:** criar/controlar documentos CL.
- **Arquivos principais:** `FormCL.cs`, `CLRepository.cs`, `CLCategoriaRepository.cs`, `CLPrioridadeRepository.cs`.
- **Dependencias:** `CL`, categorias, prioridades, setores, operadores, `CLTemplate`, `CLDirectory`.
- **Inputs:** dados do CL.
- **Outputs:** registro e arquivo gerado.
- **Riscos:** template/pasta ausente.

## Yukyu/Paid Leave

- **Finalidade:** acompanhar folgas/licencas e conferencia.
- **Arquivos principais:** `FormPaidLeaveTracking.cs`, `ui/paidleave/*`, SQL em `Sql/paidleave`.
- **Dependencias:** `AcompYukyu`, `YukyuConferencia`, `YukyuTodoke`, `YukyuFolhaControle`, `TodokeMotivo`.
- **Inputs:** operador, turno, motivos, conferencia.
- **Outputs:** acompanhamento e registros de controle.
- **Riscos:** dados de motivo/operador incompletos.

## Administracao

- **Finalidade:** manter cadastros e consultas administrativas usados por producao, Haidai, presence, follow-up, relatorios e suporte.
- **Arquivos principais:** `HTMLFormAdmin.cs`, `ui/admin/*`.
- **Dependencias:** usuario administrador e SQLite.
- **Inputs:** cadastros base, maquinas, locais, status de maquina, codigos de producao, tempos de procedimento, motivos/tipos de follow, shain e filtros do log do sistema.
- **Outputs:** alteracoes administrativas, consultas de pendencias de maquina e consulta ao log do sistema.
- **Riscos:** alteracoes incorretas em locais, maquinas, status ou tempos de procedimento afetam producao, relatorios e indicadores. Acesso indevido pode afetar operacao geral.
- **Guia detalhado:** `docs/admin-panel.md`.

## Controle de acesso

- **Finalidade:** gerenciar usuarios e permissoes.
- **Arquivos principais:** `HTMLFormAccessControl.cs`, `FormAccessControl.cs`, `FormAddUser.cs`, `FormChangePassword.cs`, `ui/access-control/*`, `UserRepository.cs`.
- **Dependencias:** `Users`, BCrypt, niveis `AccessLevel`.
- **Inputs:** usuario, senha, nivel, CodigoFJ.
- **Outputs:** usuarios e permissoes.
- **Riscos:** usuario sem `CodigoFJ` valido nao abre dashboard corretamente.

## Aplicacao do operador

- **Finalidade:** leitura de Hikitsugui por operador em app separado.
- **Arquivos principais:** `TeamOps.OperatorApp/Program.cs`, `Forms/HTMLHikitsuguiOperatorRead.cs`, `ui/hikitsugui-operator-read/*`.
- **Dependencias:** WebView2, SQLite, anexos.
- **Inputs:** acao de leitura/resposta do operador.
- **Outputs:** registro de leitura e visualizacao de comunicados.
- **Riscos:** configuracao do `DatabasePath` deve apontar para o mesmo banco operacional.
