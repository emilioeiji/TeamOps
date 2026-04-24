# CSV de Previsao para Presence Layout

O importador de previsao usa este formato por linha:

```csv
CodigoFJ,LocalId,SectorId
```

Exemplo:

```csv
FJ12345,7,2
FJ54321,6,2
FJ99999,5,2
```

Regra do nome do arquivo:

- `11-YYYYMMDD.csv` = setor `1`, turno `1`
- `12-YYYYMMDD.csv` = setor `1`, turno `2`
- `21-YYYYMMDD.csv` = setor `2`, turno `1`
- `22-YYYYMMDD.csv` = setor `2`, turno `2`

Exemplo para 24/04/2026:

- `11-20260424.csv`
- `12-20260424.csv`
- `21-20260424.csv`
- `22-20260424.csv`

Observacoes:

- o `SectorId` da linha precisa bater com o setor do arquivo
- o `LocalId` precisa existir naquele layout
- o arquivo deve ficar na pasta configurada em `OperatorScheduleDirectory`
