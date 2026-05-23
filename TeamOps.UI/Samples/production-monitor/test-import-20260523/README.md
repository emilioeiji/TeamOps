# Pacote de teste - Importacao de Producao

Este pacote foi montado para validar a importacao do monitor de producao no dia `2026-05-23`, cobrindo:

- arquivos de eventos de `ontem` e `hoje`
- `G-Bareru` (`2400`)
- `DAD` (`211D`)
- status de `rodando`, `parado`, `erro` e `inativo`
- arquivos `.dat` de plano para validar leitura de area e snapshot

## Arquivos incluidos

- `260522_2400_E.txt`
- `260523_2400_E.txt`
- `260522_211D_E.txt`
- `260523_211D_E.txt`
- `2400_plan_20260523.dat`
- `211D_plan_20260523.dat`
- `import-production.validation.bat`

## Como usar

### Opcao 1: manual

1. copiar os `*.txt` para o diretorio configurado em `ProductionSourceEventsDirectory`
2. copiar os `*.dat` para o diretorio configurado em `ProductionSourceDatDirectory`
3. clicar em `Importar` no monitor de producao

### Opcao 2: usando o BAT deste pacote

O BAT usa as variaveis de ambiente que o TeamOps ja envia ao processo:

- `TEAMOPS_PRODUCTION_SOURCE_EVENTS_DIR`
- `TEAMOPS_PRODUCTION_SOURCE_DAT_DIR`
- `TEAMOPS_PRODUCTION_COMPLETION_FILE`

Se quiser testar com ele:

1. copie ou aponte o `ProductionImportBatchPath` para `import-production.validation.bat`
2. rode a importacao pelo proprio monitor

## O que este pacote ajuda a validar

- leitura dos arquivos `yyMMdd_211D_E.txt` e `yyMMdd_2400_E.txt`
- criacao/atualizacao de `MachineEvents`
- leitura de status diferentes por setor
- leitura de `.dat` com cabecalho tabulado
- area label no plano (`AREA 1`, `AREA 2`, etc.)
- comportamento do dashboard e dos graficos apos importacao
