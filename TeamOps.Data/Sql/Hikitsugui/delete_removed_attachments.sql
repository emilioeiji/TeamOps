-- delete_removed_attachments.sql
DELETE FROM HikitsuguiAttachments
WHERE HikitsuguiId = @HikitsuguiId
  AND Id NOT IN @KeepIds;
