using OmEnterpriseBillingWin.Models;
using OmEnterpriseBillingWin.Data;
using System.Data.SqlClient;

namespace OmEnterpriseBillingWin.Services
{
    public class PurchaseService
    {
        private readonly DbContext _dbContext;
        private readonly ItemService _itemService;

        public PurchaseService()
        {
            _dbContext = new DbContext();
            _itemService = new ItemService();
        }

        public async Task<List<Purchase>> GetAllPurchasesAsync()
        {
            const string query = @"
                SELECT p.PurchaseID, p.ItemId, p.Quantity, p.Price, p.Date, p.Seller,
                       i.Name, i.SalePrice, i.StockQty, i.MinStockLevel
                FROM Purchases p
                JOIN Items i ON p.ItemId = i.ItemId
                ORDER BY p.Date DESC";

            return await _dbContext.ExecuteReaderAsync(query, reader => new Purchase
            {
                Id = reader.GetInt32(0),
                ItemId = reader.GetInt32(1),
                Quantity = reader.GetInt32(2),
                Price = reader.GetDecimal(3),
                Date = reader.GetDateTime(4),
                Seller = reader.GetString(5),
                Item = new Item
                {
                    ItemId = reader.GetInt32(1),
                    Name = reader.GetString(6),
                    SalePrice = reader.GetDecimal(7),
                    StockQty = reader.GetInt32(8),
                    MinStockLevel = reader.GetInt32(9)
                }
            });
        }

        public async Task<decimal> GetMonthlyPurchaseTotal()
        {
            const string query = @"
                SELECT COALESCE(SUM(Quantity * Price), 0)
                FROM Purchases
                WHERE Date >= DATEADD(MONTH, -1, GETDATE())";

            return await _dbContext.ExecuteScalarAsync<decimal>(query);
        }

        public async Task<decimal> GetTotalPurchaseAmount(DateTime startDate, DateTime endDate)
        {
            const string query = @"
                SELECT COALESCE(SUM(Quantity * Price), 0)
                FROM Purchases
                WHERE Date >= @StartDate AND Date < @EndDate";

            var parameters = new Dictionary<string, object>
            {
                { "@StartDate", startDate },
                { "@EndDate", endDate }
            };

            return await _dbContext.ExecuteScalarAsync<decimal>(query, parameters);
        }

        public async Task<int> AddPurchaseAsync(Purchase purchase)
        {
            using var connection = new SqlConnection(_dbContext.ConnectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Insert purchase record
                const string purchaseQuery = @"
                    INSERT INTO Purchases (ItemId, Quantity, Price, Date, StakeholderId)
                    VALUES (@ItemId, @Quantity, @Price, @Date, @StakeholderId);
                    SELECT SCOPE_IDENTITY();";

                var purchaseParams = new Dictionary<string, object>
                {
                    { "@ItemId", purchase.ItemId },
                    { "@Quantity", purchase.Quantity },
                    { "@Price", purchase.Price },
                    { "@Date", purchase.Date },
                    { "@StakeholderId", purchase.StakeholderId }
                };

                // Use the same connection for all operations
                var id = await _dbContext.ExecuteScalarAsync<decimal>(purchaseQuery, purchaseParams, transaction);

                // Update item stock using the same transaction
                await _itemService.UpdateStockAsync(purchase.ItemId, purchase.Quantity, transaction);

                await transaction.CommitAsync();
                return (int)id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
