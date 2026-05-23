# Pacote de teste rico - Importacao de Producao

Este pacote foi feito para uma validacao mais realista do monitor de producao.

Ele traz:

- mais maquinas por setor
- variacao de `rodando`, `parado`, `erro` e `inativo`
- areas com percentuais diferentes entre si
- arquivos de `ontem` e `hoje`
- arquivos `.dat` com varias areas

## Arquivos

- `260522_2400_E.txt`
- `260523_2400_E.txt`
- `260522_211D_E.txt`
- `260523_211D_E.txt`
- `2400_plan_20260523.dat`
- `211D_plan_20260523.dat`
- `import-production.validation.bat`

## Uso rapido

1. aponte o `ProductionImportBatchPath` para o BAT desta pasta
2. clique em `Importar` no monitor

Ou copie manualmente:

- `*.txt` para `C:\TeamOps\Production\Events\`
- `*.dat` para `C:\TeamOps\Production\Source\Dat\`

## Objetivo deste pacote

Validar melhor:

- cards de area com percentuais diferentes
- ranking de paradas
- historico por area
- ranking estimado por operador
- detalhamento visual do monitor
