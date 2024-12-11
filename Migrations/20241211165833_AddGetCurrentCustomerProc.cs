using Microsoft.EntityFrameworkCore.Migrations;

namespace DogGroomingAPI.Migrations
{
    public partial class AddGetCurrentCustomerProc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'GetCurrentCustomer')
                    DROP PROCEDURE [dbo].[GetCurrentCustomer]
                GO

                CREATE PROCEDURE [dbo].[GetCurrentCustomer]
                    @CustomerId INT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SELECT 
                        Id,
                        Username,
                        FullName
                    FROM Customers
                    WHERE Id = @CustomerId;
                END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[GetCurrentCustomer]");
        }
    }
}