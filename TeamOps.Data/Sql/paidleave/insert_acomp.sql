INSERT INTO AcompYukyu (
    OperatorCodigoFJ,
    RequestDate,
    AuthorizedByCodigoFJ,
    Notes,
    TodokeMotivoId
)
VALUES (
    @OperatorCodigoFJ,
    @RequestDate,
    @AuthorizedByCodigoFJ,
    @Notes,
    @TodokeMotivoId
);
