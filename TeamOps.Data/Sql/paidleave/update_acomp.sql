UPDATE AcompYukyu
SET
    OperatorCodigoFJ = @OperatorCodigoFJ,
    RequestDate = @RequestDate,
    Notes = @Notes,
    TodokeMotivoId = @TodokeMotivoId
WHERE Id = @Id;
