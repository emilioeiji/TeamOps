SELECT 
    Id,
    HikitsuguiId,
    FileName,
    FilePath,
    CreatedAt
FROM HikitsuguiAttachments
WHERE HikitsuguiId = @id
ORDER BY Id;
