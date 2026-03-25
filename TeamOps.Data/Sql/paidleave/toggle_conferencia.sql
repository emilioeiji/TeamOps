UPDATE AcompYukyu
SET Conferencia = CASE WHEN Conferencia = 0 THEN 1 ELSE 0 END
WHERE Id = @Id;
