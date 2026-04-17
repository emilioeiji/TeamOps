SELECT
    h.Id,
    h.Date,
    o.NameRomanji AS OperatorName,

    c.NamePt AS Category,
    e.NamePt AS Equipment,
    l.NamePt AS Local,
    s.NamePt AS Sector,

    h.Description,
    h.AttachmentPath,

    CASE WHEN r.Id IS NULL THEN 0 ELSE 1 END AS IsRead

FROM Hikitsugui h
JOIN Operators o ON o.CodigoFJ = h.CreatorCodigoFJ
LEFT JOIN Categories c ON c.Id = h.CategoryId
LEFT JOIN Equipments e ON e.Id = h.EquipmentId
LEFT JOIN Locals l ON l.Id = h.LocalId
LEFT JOIN Sectors s ON s.Id = h.SectorId

LEFT JOIN HikitsuguiReads r
    ON r.HikitsuguiId = h.Id
    AND r.ReaderCodigoFJ = @codigoFJ

WHERE 1=1
AND h.Date BETWEEN @dtInicial AND @dtFinal

-- FILTRO POR PÚBLICO (HIERARQUIA CORRETA)
AND (
        (@publico = 'operador' AND h.ForOperators = 1)
        OR (@publico = 'lider' AND h.ForLeaders = 1)
        OR (@publico = 'masv' AND h.ForMaSv = 1)

        -- TODOS → retorna tudo que o usuário pode ver
        OR (
            @publico = 'todos'
            AND (
                -- operador → só operador
                (@accessLevel = 1 AND h.ForOperators = 1)

                -- líder → operador + líder
                OR (@accessLevel = 2 AND (h.ForOperators = 1 OR h.ForLeaders = 1))

                -- GL → operador + líder + masv
                OR (@accessLevel >= 3 AND (h.ForOperators = 1 OR h.ForLeaders = 1 OR h.ForMaSv = 1))
            )
        )
    )

-- FILTROS ADICIONAIS
AND (@shiftId = 0 OR h.ShiftId = @shiftId)
AND (@operatorId = 0 OR h.CreatorCodigoFJ = @operatorId)
AND (@reasonId = 0 OR h.CategoryId = @reasonId)
AND (@equipId = 0 OR h.EquipmentId = @equipId)
AND (@sectorId = 0 OR h.SectorId = @sectorId)

-- FILTRO DE TEXTO (PT / JP / QUALQUER IDIOMA)
AND (
        @search = ''
        OR h.Description LIKE '%' || @search || '%'
        OR o.NameRomanji LIKE '%' || @search || '%'
        OR c.NamePt LIKE '%' || @search || '%'
        OR e.NamePt LIKE '%' || @search || '%'
        OR l.NamePt LIKE '%' || @search || '%'
        OR s.NamePt LIKE '%' || @search || '%'
    )

ORDER BY h.Date DESC, h.Id DESC;
