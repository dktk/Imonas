using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fn_map_payment_status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- FUNCTION: silver.fn_map_payment_status(text)
-- DROP FUNCTION IF EXISTS silver.fn_map_payment_status(text);

CREATE OR REPLACE FUNCTION silver.fn_map_payment_status(
	p_status text)
    RETURNS text
    LANGUAGE 'plpgsql'
    COST 100
    IMMUTABLE STRICT PARALLEL UNSAFE
AS $BODY$

                    BEGIN
                      RETURN CASE p_status
                        WHEN 'BalanceUpdateFailed'  THEN 'Failed'
                        WHEN 'CancelledByUser'      THEN 'Failed'
                        WHEN 'Captured'             THEN 'Failed'
                        WHEN 'Completed'            THEN 'Completed'
						WHEN 'completed'            THEN 'Completed'
						WHEN 'expired'				THEN 'Failed'
						WHEN 'failed'				THEN 'Failed'
                        WHEN 'Declined'             THEN 'Failed'
                        WHEN 'Initialized'          THEN 'Initialized'
                        WHEN 'Pending'              THEN 'Pending'
                        WHEN 'Processing'           THEN 'Pending'
                        WHEN 'ProviderError'        THEN 'Failed'
                        WHEN 'Validated'            THEN 'Pending'
                        WHEN 'WaitingForApproval'   THEN 'Pending'
                        WHEN 'WaitingUserInput'     THEN 'Pending'
                        ELSE concat(p_status, ' not mapped')
                      END;
                    END;
                    
$BODY$;

ALTER FUNCTION silver.fn_map_payment_status(text)
    OWNER TO ""main-admin"";
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
