INSERT INTO HikitsuguiResponses
(HikitsuguiId, ResponderCodigoFJ, Message, Date)
VALUES
(@id, @codigoFJ, @text, CURRENT_TIMESTAMP);
