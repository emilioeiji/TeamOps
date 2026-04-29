# Production Monitor Samples

Este diretorio guarda um conjunto minimo de arquivos para testar o `HTMLFormProductionMonitor`.

## O que o `import-production.bat` precisa retornar

O `ProductionFileImporter` nao espera JSON nem um formato complexo do BAT.

Ele so exige:

1. o processo terminar com `exit /b 0`
2. se `ProductionImportCompletionFile` estiver configurado, o BAT precisa criar esse arquivo
3. o conteudo desse arquivo pode ser um texto simples, por exemplo:

```text
2 arquivo(s) sincronizado(s) para importacao.
```

Esse texto aparece no painel no trecho `BAT: ...`.

Se o BAT retornar codigo diferente de `0`, a importacao falha.
Se o BAT nao criar o `completion file`, a importacao tambem falha por timeout.

## Formato esperado pelo importador

Os arquivos lidos pelo importador seguem este padrao de nome:

- `yyMMdd_211D_E.txt`
- `yyMMdd_2400_E.txt`

Mapeamento fixo usado no monitor:

- `211D` = `Setor 2 / DAD`
- `2400` = `Setor 1 / G-Bareru`

Cada linha precisa ter colunas separadas por `|`.
As posicoes usadas hoje sao:

- `3`: `LineCode`
- `4`: `MachineCode`
- `5`: `InternalState`
- `7`: `EventDate`
- `8`: `EventTime`
- `9`: `StatusText`
- `10`: `RecipeName`
- `12`: `LotNo`

Exemplo realista de `2400`:

```text
E|2026/04/29|00:00:00|2400|E01|00|0|2026/04/29|00:00:00|稼動中|0|0|0
```

Exemplo realista de `211D`:

```text
E|2026/04/29|00:00:00|211D|E49|00|0|2026/04/29|00:00:00|運転|RZM RZ345|0|264EHNV000
```

## Arquivos de exemplo

Os dois `.txt` deste diretorio seguem o padrao das telas enviadas e usam maquinas do tipo:

- `2400`: `E01`, `E02`, `E09`, `E10`, `E36`
- `211D`: `E49`, `E50`

Eles foram pensados para o dia `2026-04-29`, que e o recorte atual de teste.

## BAT modelo

O arquivo [import-production.example.bat](/C:/Users/emili/source/repos/TeamOps/TeamOps.UI/Samples/production-monitor/import-production.example.bat) mostra um fluxo simples:

- cria o diretorio de destino se necessario
- copia os `.txt` da origem para o diretorio de eventos
- escreve a mensagem final no `completion file`
- encerra com `exit /b 0`
