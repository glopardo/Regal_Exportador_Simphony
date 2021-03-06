--	CHECK_DETAIL
--		- DETAILTYPE:
--			- 1		: MENU ITEM
--			- 2		: DISCOUNTS
--			- 4		: TENDER MEDIA
--			- 5		: (SIEMPRE CERO)
--			- 15	: MENU ITEM??; IGNORAR
--			- 20	: (SIEMPRE CERO)
--			- 22	: (SIEMPRE CERO)

-----------------------------------------------------------------------------------------
--                                     MENU ITEMS                                      --
-----------------------------------------------------------------------------------------
SELECT
	CD.RevCtrID,
	CD.CHECKID,
	CD.CHECKDETAILID,
	CD.DETAILINDEX,
	CD.DETAILTYPE,
	CD.REVCTRID,
	CD.EMPLOYEEID,
	CD.TOTAL,
	ST.STRINGTEXT,
	CASE
		WHEN MIDEF.MenuItemClassObjNum BETWEEN 2000 AND 2010 THEN '031'
		WHEN MIDEF.MenuItemClassObjNum BETWEEN 3000 AND 3001 THEN '062'
		WHEN MIDEF.MenuItemClassObjNum BETWEEN 7000 AND 7006 THEN '031'
		ELSE '022'
	END CCOSTO
FROM
	CHECK_DETAIL CD
	INNER JOIN MENU_ITEM_DETAIL MIDET ON MIDET.CHECKDETAILID = CD.CHECKDETAILID
	INNER JOIN MENU_ITEM_DEFINITION MIDEF ON MIDEF.MENUITEMDEFID = MIDET.MENUITEMDEFID
	INNER JOIN STRING_TABLE ST ON ST.STRINGNUMBERID = MIDEF.NAME1ID
WHERE
	CHECKID = '50930'
	AND DETAILTYPE <> 15
-----------------------------------------------------------------------------------------
--                                     DISCOUNT                                        --
-----------------------------------------------------------------------------------------
SELECT
	CD.RevCtrID,
	CD.CHECKID,
	CD.CHECKDETAILID,
	CD.DETAILINDEX,
	CD.DETAILTYPE,
	CD.REVCTRID,
	CD.EMPLOYEEID,
	CD.TOTAL,
	ST.STRINGTEXT
FROM
	CHECK_DETAIL CD
	INNER JOIN DISCOUNT_DETAIL DDET ON DDET.CHECKDETAILID = CD.CHECKDETAILID
	INNER JOIN DISCOUNT D ON D.DSCNTID = DDET.DSCNTID
	INNER JOIN STRING_TABLE ST ON ST.STRINGNUMBERID = D.NAMEID
WHERE CHECKID = '50961'
-----------------------------------------------------------------------------------------
--                                     TENDER MEDIA                                    --
-----------------------------------------------------------------------------------------
SELECT
	CD.RevCtrID,
	CD.CHECKID,
	CONVERT(VARCHAR(10), CD.DETAILPOSTINGTIME, 105),
	CD.CHECKDETAILID,
	CD.DETAILINDEX,
	CD.DETAILTYPE,
	CD.REVCTRID,
	CD.EMPLOYEEID,
	CONVERT(INT, ROUND(CD.TOTAL, 0)) TOTAL,
	CONVERT(INT, ROUND(CD.TOTAL / 1.19, 0)) NETO,
	CONVERT(INT, ROUND((CD.TOTAL / 1.19) * 0.19, 0)) IVA,
	ST.STRINGTEXT DESCRIPCION,
	TMDET.CHARGETIP PROPINA,
	TMDET.TendMedID,
	CASE TMDET.TendMedID
		WHEN 111 THEN '11-04-015'
		WHEN 128 THEN '11-04-015'
		WHEN 110 THEN '11-04-016'
		WHEN 127 THEN '11-04-016'
		WHEN 109 THEN '11-04-017'
		WHEN 126 THEN '11-04-017'
		WHEN 108 THEN '11-04-018'
		WHEN 125 THEN '11-04-018'
		WHEN 112 THEN '11-04-018'
		WHEN 112 THEN '11-04-018'
		WHEN 129 THEN '11-04-018'
		WHEN 97 THEN '11-01-009'
		WHEN 130 THEN '11-01-009'
		WHEN 131 THEN '11-01-007'
		WHEN 103 THEN '11-01-007'
		ELSE 'CUENTANODEFINIDA'
	END CUENTA
FROM
	CHECK_DETAIL CD
	INNER JOIN TENDER_MEDIA_DETAIL TMDET ON TMDET.CHECKDETAILID = CD.CHECKDETAILID
	INNER JOIN TENDER_MEDIA TM ON TM.TENDMEDID = TMDET.TENDMEDID
	INNER JOIN STRING_TABLE ST ON ST.STRINGNUMBERID = TM.NAMEID
WHERE CD.CheckID = 50931
-----------------------------------------------------------------------------------------