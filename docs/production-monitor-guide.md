# Production Monitor e ProductionMonitorProbe

## Objetivo

Este documento orienta o uso do Monitor de Producao no TeamOps e dos comandos do `ProductionMonitorProbe.exe`.

Use o Probe para validacao tecnica, diagnostico de banco, conferencia de importacao, status por setor, EC2 Administrator e performance. O usuario final normalmente usa apenas a tela **Producao** dentro do dashboard do TeamOps.

## Onde executar o Probe

No ambiente publicado, abra PowerShell ou Prompt de Comando na pasta onde esta o executavel:

```powershell
cd C:\caminho\do\publish\ProductionMonitorProbe
.\ProductionMonitorProbe.exe schema-check
```

O banco usado pelo Probe vem do arquivo `.config` publicado, pela chave:

```xml
<add key="DatabasePath" value="C:\TeamOps\DB\teamops.db" />
```

Em producao, confirme que o `.config` do Probe aponta para o mesmo banco usado pelo TeamOps.

## Comandos do ProductionMonitorProbe

| Comando | Exemplo | O que faz | Quando usar |
| --- | --- | --- | --- |
| `import` | `.\ProductionMonitorProbe.exe import` | Executa a importacao de producao usando as configuracoes atuais. Mostra arquivos lidos, linhas importadas/ignoradas, maquinas criadas, resultado do BAT, resultado EC2 e tempos de performance. | Validar se a importacao roda fora da tela principal. |
| `import-profile` | `.\ProductionMonitorProbe.exe import-profile --date 2026-06-01 --sector dad` | Executa importacao e, se houver `--date`, mede tambem analytics pos-importacao para a data/setor informado. | Investigar lentidao, travamento ou divergencia depois de importar. |
| `dashboard` | `.\ProductionMonitorProbe.exe dashboard` | Monta dashboards de exemplo pelo backend e imprime kadouritsu, maquinas, areas e ranking. | Conferir se o backend do dashboard consegue calcular dados sem abrir a UI. |
| `db-index-check` | `.\ProductionMonitorProbe.exe db-index-check` | Lista os indices existentes e marca indices obrigatorios como `OK` ou `MISSING`. | Validar performance e migrations em producao/homologacao. |
| `schema-check` | `.\ProductionMonitorProbe.exe schema-check` | Valida schema do monitor de producao, integridade SQLite, tabelas, colunas, indices e seeds principais. Nao altera dados. | Primeiro comando para checar banco em producao. |
| `schema-repair` | `.\ProductionMonitorProbe.exe schema-repair` | Executa o migrator e repara seeds de status de producao/DAD. Depois roda a mesma validacao do `schema-check`. | Corrigir banco com migration/seeds faltando, com suporte tecnico acompanhando. |
| `status-hardening` | `.\ProductionMonitorProbe.exe status-hardening` | Valida regra defensiva de status por setor, fallback global, seeds DAD e formula de `StopNoCount`. | Antes de publicar ou depois de reparar schema. |
| `status-report` | `.\ProductionMonitorProbe.exe status-report --start 2026-06-01 --end 2026-06-02 --sector dad --csv dad-status.csv` | Gera relatorio de status encontrados em `MachineEvents`, com classificacao, fonte, fallback, minutos, impacto estimado e warnings. Pode exportar CSV. | Validar codigos reais do DAD/G-Bareru sem alterar classificacao automaticamente. |
| `production-diagnostics` | `.\ProductionMonitorProbe.exe production-diagnostics` | Mostra contagens de eventos, status, maquinas ativas, primeira/ultima data e distribuicoes principais. | Confirmar se a importacao gravou dados e se o dashboard tem base para exibir. |
| `machine-cleanup` | `.\ProductionMonitorProbe.exe machine-cleanup` | Lista maquinas ativas com codigo invalido, como valores numericos/decimais inseridos por importacao incorreta. Nao altera dados sem `--apply`. | Limpar seletores de maquina sem apagar historico ligado por FK. |
| `ec2-diagnostics` | `.\ProductionMonitorProbe.exe ec2-diagnostics` | Mostra ultima importacao EC2, arquivo/hash, maquinas atuais, rodando/paradas/desconsideradas, media, linhas auditadas e estilos de codigo de peca. | Conferir EC2 Administrator e cards EC2 do dashboard. |
| `ec2-reset-latest` | `.\ProductionMonitorProbe.exe ec2-reset-latest` | Remove a ultima importacao EC2 do arquivo configurado e seus snapshots. | Uso restrito de suporte para forcar reimportacao do mesmo arquivo EC2. |
| `demo` ou sem comando | `.\ProductionMonitorProbe.exe` | Executa `import` e depois `dashboard`. | Teste rapido em ambiente de desenvolvimento. |

## Parametros aceitos

### `import-profile`

```powershell
.\ProductionMonitorProbe.exe import-profile --date 2026-06-01 --sector dad
```

- `--date yyyy-MM-dd`: data usada para medir analytics depois da importacao.
- `--sector dad`: filtra o analytics para DAD.
- `--sector gbareru` ou `--sector g-bareru`: filtra para G-Bareru.
- `--sector 1`, `--sector 2`: aceita tambem o Id numerico do setor.

### `status-report`

```powershell
.\ProductionMonitorProbe.exe status-report --start 2026-06-01 --end 2026-06-02 --sector dad --csv dad-status.csv
```

- `--start yyyy-MM-dd` ou `yyyy-MM-dd HH:mm:ss`: inicio do periodo.
- `--end yyyy-MM-dd` ou `yyyy-MM-dd HH:mm:ss`: fim do periodo. O fim e exclusivo.
- `--sector dad`, `gbareru`, `g-bareru` ou Id numerico.
- `--csv caminho.csv`: grava o relatorio em CSV alem de mostrar no console.

## Ordem recomendada para validar producao

1. Fechar telas do TeamOps que estejam importando ou atualizando producao.
2. Rodar:

```powershell
.\ProductionMonitorProbe.exe schema-check
```

3. Se aparecer issue de schema/seeds, rodar com suporte:

```powershell
.\ProductionMonitorProbe.exe schema-repair
.\ProductionMonitorProbe.exe schema-check
```

4. Validar indices:

```powershell
.\ProductionMonitorProbe.exe db-index-check
```

5. Conferir dados importados:

```powershell
.\ProductionMonitorProbe.exe production-diagnostics
```

6. Conferir se ha maquinas invalidas contaminando seletores:

```powershell
.\ProductionMonitorProbe.exe machine-cleanup
```

Se a lista estiver correta, aplicar:

```powershell
.\ProductionMonitorProbe.exe machine-cleanup --apply
```

7. Validar status reais do DAD:

```powershell
.\ProductionMonitorProbe.exe status-report --start 2026-06-01 --end 2026-06-02 --sector dad --csv dad-status.csv
```

8. Se usa EC2 Administrator:

```powershell
.\ProductionMonitorProbe.exe ec2-diagnostics
```

9. Medir importacao e analytics:

```powershell
.\ProductionMonitorProbe.exe import-profile --date 2026-06-01 --sector dad
```

## Como interpretar warnings do `status-report`

| Warning | Significado | Acao recomendada |
| --- | --- | --- |
| `UNKNOWN_DAD_STATUS` | O DAD teve codigo fora da lista esperada `0, 1, 3, 4, 17, 18, 19`. | Conferir com amostra real antes de cadastrar regra nova. |
| `FALLBACK_USED_FOR_DAD` | Nao encontrou status setorial DAD e usou status global. | Cadastrar status setorial se o significado do DAD for diferente. |
| `AUTO_CREATED_STATUS` | Status parece ter sido criado automaticamente ou manualmente fora do seed esperado. | Conferir classificacao no Admin/banco. |
| `POSSIBLE_EFFICIENCY_IMPACT` | Status nao rodando esta entrando no denominador do kadouritsu. | Validar se deve ser `StopCounts` ou `StopNoCount`. |

## Uso da tela Producao

Abra pelo dashboard:

1. Login no TeamOps.
2. Clique em **Producao**.
3. Escolha data, turno, setor, area ou maquina quando necessario.
4. Use **Importar producao** para buscar arquivos TXT/DAT e EC2 configurados.
5. Aguarde a mensagem de conclusao.
6. O dashboard recarrega depois do commit da importacao.

Principais blocos da tela:

- **Resumo de producao:** kadouritsu real, rodando, paradas, erros e tempos.
- **Filtros:** data, turno, setor, area/local e maquina.
- **Importacao:** botao de importacao e mensagem de sucesso/erro.
- **EC2 Administrator:** total de maquinas, rodando, paradas, desconsideradas e media de tempo operando.
- **Codigos parametrizados:** legenda visual de codigos de peca, como `RJ2A7`.
- **Previsao G-Bareru:** simulacao de capacidade/kadouritsu prevista quando tempos `ECII`, `BUNKATSU`, `DCS` e Haidai estao disponiveis.
- **Legenda de status:** cores e classificacoes dos status de maquina.
- **Detalhes por maquina/area:** lista operacional usada para investigar divergencias.

## Modulos relacionados no dashboard

| Grupo | Botao | Uso principal |
| --- | --- | --- |
| Operacao | **Operadores** | Cadastro e consulta de operadores. |
| Operacao | **Acompanhamento** | Follow-up, registro e orientacao. |
| Operacao | **Tasks** | Planejamento e acompanhamento de tarefas do turno. |
| Operacao | **MasterCard** | Treinamento, follow e fechamento. |
| Operacao | **Producao** | Monitor de maquinas, importacao, EC2 e kadouritsu. |
| Operacao | **Sobra de Peca** | Lancamento de perdas/sobras. |
| Operacao | **Relatorios** | Hub de relatorios e impressoes. |
| Hikitsugui e Documentos | **Hikitsugui** | Cadastro rapido de passagem de informacao. |
| Hikitsugui e Documentos | **Leitura Hikitsugui** | Consulta, leitura, resposta e correcao. |
| Hikitsugui e Documentos | **PR** | Documento de processo. |
| Hikitsugui e Documentos | **CL** | Controle de linha. |
| Hikitsugui e Documentos | **Todoke** | Solicitacoes e folhas relacionadas a Yukyu/Todoke. |
| Presenca | **Presenca** | Painel dos setores e presenca operacional. |
| Presenca | **Haidai** | Escala diaria por grupo, usada tambem na previsao G-Bareru. |
| Administracao | **Admin** | Parametrizacoes do sistema, incluindo status/producao/codigos. |
| Administracao | **Acesso** | Usuarios e permissoes. |

## Modulos do hub Relatorios

| Botao | Uso principal |
| --- | --- |
| **Hikitsugui** | Relatorio/consulta de passagem de informacao. |
| **Follow-up** | Relatorio consolidado de follow-up. |
| **Graficos de Follow-up** | Analise grafica de follow-up. |
| **Tasks** | Relatorio de tarefas. |
| **Operadores** | Relatorio gerencial por operador. |
| **Presenca** | Relatorio de presenca, ausencias, Yukyu e ocorrencias. |
| **MasterCard** | Relatorio de Master Card. |
| **PR** | Relatorio de PR. |
| **CL** | Relatorio de CL. |

## Parametrizacoes importantes no Admin

Na tela **Admin**, confira as parametrizacoes ligadas a producao:

- **Status de Maquina:** status global e status por setor, com `Classification`.
- **Codigos da Producao:** legenda visual para codigos destacados, como `RJ2A7`.
- **Tempos de Procedimento:** tempos `ECII`, `BUNKATSU` e `DCS` usados na previsao G-Bareru.
- **Setores, Locais e Maquinas:** cadastros base usados por importacao e dashboard.

## Prints das telas

E possivel colocar prints na documentacao. Como as telas recebem dados reais pelo WinForms/WebView2, os prints mais confiaveis devem ser capturados com o TeamOps aberto e logado no ambiente correto.

Sugestao de arquivos:

| Tela | Caminho sugerido |
| --- | --- |
| Dashboard principal | `docs/screenshots/dashboard.png` |
| Hub de relatorios | `docs/screenshots/reports.png` |
| Monitor de producao | `docs/screenshots/production-monitor.png` |
| Admin - parametrizacoes de producao | `docs/screenshots/admin-production.png` |
| ProductionMonitorProbe no console | `docs/screenshots/production-monitor-probe.png` |

Depois de salvar os prints nesses caminhos, eles podem ser referenciados no Markdown assim:

```markdown
![Dashboard principal](screenshots/dashboard.png)
```

No momento, esta documentacao deixa a estrutura pronta para os prints reais, mas nao inclui imagem gerada automaticamente porque a renderizacao estatica fora do TeamOps nao carrega os dados do WebView2.

## Cuidados operacionais

- Nao apague arquivos `.wal`, `.shm` ou `.journal` com o sistema aberto.
- Evite rodar importacao pelo Probe ao mesmo tempo que a tela Producao importa.
- Em banco de rede, rode diagnosticos em horario controlado quando possivel.
- Use `schema-repair` e `ec2-reset-latest` apenas com suporte tecnico.
- Nao altere `Classification` de status desconhecido sem relatorio real e aprovacao do responsavel do setor.
