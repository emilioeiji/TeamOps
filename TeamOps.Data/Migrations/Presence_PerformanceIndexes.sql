CREATE INDEX IF NOT EXISTS IX_OperatorPresence_DaySectorShiftOperator
ON OperatorPresence(date(Date), SectorId, ShiftId, CodigoFJ, Date);

CREATE INDEX IF NOT EXISTS IX_OperatorPresence_OperatorDay
ON OperatorPresence(CodigoFJ, date(Date), Date);
