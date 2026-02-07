using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class sp_settle_psp_transactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE PROCEDURE gold.sp_settle_psp_transactions(
	IN p_psp_id bigint,
	IN p_start_date timestamp with time zone,
	IN p_end_date timestamp with time zone,
	IN run_id integer,
	INOUT not_settled_cur refcursor,
	INOUT settled_cur refcursor)
LANGUAGE 'plpgsql'
AS $BODY$

	BEGIN
		-- populate the settlement table
		--
		INSERT INTO gold.psp_settlements(
			reconciliation_run_id, reconciliation_status, psp_id, currency_code, total_fees, net_settlement, created, created_by, last_modified, last_modified_by, tx_date, external_payment_id, internal_payment_id, amount
		)
		SELECT
			run_id,
			1,
			e.psp_id,
			e.currency_code,
			0,
			0,
			NOW(),
			e.created_by,
			NOW(),
			e.created_by,			
			e.tx_date,
			e.id,
			i.id,
			e.amount
		FROM SILVER.EXTERNAL_PAYMENTS E
		INNER JOIN SILVER.INTERNAL_PAYMENTS I ON 
			E.external_payment_id = I.provider_tx_id
			AND E.amount = I.amount
			AND E.currency_code = I.currency_code
			AND SILVER.FN_MAP_PAYMENT_STATUS (E.status) = SILVER.FN_MAP_PAYMENT_STATUS (I.status)		
		WHERE E.psp_id = p_psp_id
		  AND e.tx_date > p_start_date
		  AND e.tx_date < p_end_date;

		-- return the transactions not matched in the internal system
		--
		OPEN not_settled_cur FOR
			select 
	            ep.id as id,
	            ep.action as Action,
	            ep.external_system as ExternalSystem,
	            ep.external_payment_id as ExternalSystemPaymentId,				
	            p.id as PspId,
				p.name as PspName,
	            p.is_csv_based as IsCsvBased,
	            ep.amount as Amount,
	            ep.currency_code as CurrencyCode,
	            ep.tx_date as TransactionDate,
	            ep.status as Status,
				ep.created_by as ExternalPaymentCreatedBy,
				ep.created as ExternalPaymentCreated,
				ep.last_modified as ExternalPaymentLastModified,
				ep.last_modified_by as ExternalPaymentLastModifiedBy,	            
	            rp.file_name as SourceFileName,	            
	            rp.id as RawpaymentId,
	            rp.created_by as RawPaymentCreatedBy,
	            rp.created as RawPaymentCreatedDate
            from silver.external_payments ep
	            inner join public.Psps p on p.id = ep.psp_id
	            inner join bronze.raw_payments rp on rp.id = ep.raw_payment_id
			left JOIN SILVER.INTERNAL_PAYMENTS I ON 
				ep.external_payment_id = I.provider_tx_id
				AND ep.amount = I.amount
				AND ep.currency_code = I.currency_code
				AND SILVER.FN_MAP_PAYMENT_STATUS (ep.status) = SILVER.FN_MAP_PAYMENT_STATUS (I.status)
			where 
				ep.psp_id = p_psp_id
				AND ep.tx_date > p_start_date
				AND ep.tx_date < p_end_date
				AND i.id is null;

		-- return newly added transactions in the the settled table
		--
		OPEN settled_cur FOR
			SELECT 
				ps.reconciliation_run_id as ReconciliationRunId,
				ps.reconciliation_status as ReconciliationStatus, 
				ps.currency_code as CurrencyCode,
				ps.total_fees as TotalFees,
				ps.net_settlement as NetSettlement,
				ps.id as SettlementId,
				ps.created,
				ps.created_by,
				ps.last_modified,
				ps.last_modified_by,
				ps.tx_date as TransactionDate,
				ep.external_payment_id as ExternalSystemPaymentId,
				ep.external_system as ExternalSystem,
				ip.tx_id as InternalTransactionId,
				ps.amount as Amount,
				p.id as PspId,
				p.name as PspName,
				rp.id as RawPaymentId
			FROM gold.psp_settlements ps
			INNER JOIN public.Psps p on p.id = ps.psp_id
			INNER JOIN silver.external_payments ep on ep.id = ps.external_payment_id
			INNER JOIN silver.internal_payments ip on ip.id = ps.internal_payment_id
			INNER JOIN bronze.raw_payments rp on rp.id = ep.raw_payment_id
			WHERE 
				ps.psp_id = p_psp_id
				AND ps.tx_date > p_start_date
				AND ps.tx_date < p_end_date;
		
	END;
                                
$BODY$;
ALTER PROCEDURE gold.sp_settle_psp_transactions(bigint, timestamp with time zone, timestamp with time zone, integer, refcursor, refcursor)
    OWNER TO ""main-admin"";

            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
