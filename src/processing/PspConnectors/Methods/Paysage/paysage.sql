select 
	Id, REF_NUMBER as RefNumber, 
	REQUEST_DATE as RequestDate, 
	Status, 
	Amount, 
	Account_Id as AccountId, 
	Misc as Description, 
	PROVIDER_TRAN_ID as ProviderTxId 
	
FROM [OmegaReplica].[admin_all].[PAYMENT] 
where 
	REQUEST_DATE >= '2025-02-09 20:00:00' and 
	REQUEST_DATE <= '2025-02-09 23:59:59' and
	SUBMETHOD = 'Paysage'
--and REF_NUMBER = '100544002A2453129415'