
-- clear user-data
--
delete from silver.external_payments;
delete from silver.internal_payments;
delete from bronze.raw_payments;
delete from gold.psp_settlements;
delete from public.reconciliation_runs;

delete from public.Serilogs;


-- get matching payments
--
SELECT	
	I.*,
	E.*
FROM
	SILVER.EXTERNAL_PAYMENTS E
	LEFT JOIN SILVER.INTERNAL_PAYMENTS I ON E."external_payment_id" = I."provider_tx_id"
	AND E."amount" = I."amount"
	AND E."currency_code" = I."currency_code"
	AND SILVER.FN_MAP_PAYMENT_STATUS (E."status") = SILVER.FN_MAP_PAYMENT_STATUS (I."status")


delete from gold.psp_settlement;

CALL gold.sp_settle_psp_transactions(
	 1,
	'20251201',
	'20260109',
    1, -- run id
	'unmatched', 
	'settled'
);

FETCH ALL FROM unmatched;
FETCH ALL FROM settled;
