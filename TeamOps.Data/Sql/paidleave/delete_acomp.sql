DELETE FROM YukyuConferencia WHERE AcompYukyuId = @Id;
DELETE FROM YukyuFolhaControle WHERE AcompYukyuId = @Id;
DELETE FROM YukyuTodoke WHERE AcompYukyuId = @Id;
DELETE FROM AcompYukyu WHERE Id = @Id;
