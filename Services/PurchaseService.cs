using OmEnterpriseBillingWin.Models;
using OmEnterpriseBillingWin.Data;
using Microsoft.Data.SqlClient;

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
                SELECT ISNULL(p.PurchaseID, 0) as PurchaseID,
                       ISNULL(p.ItemId, 0) as ItemId,
                       ISNULL(p.Quantity, 0) as Quantity,
                       ISNULL(p.Price, 0) as Price,
                       ISNULL(p.Date, GETDATE()) as Date,
                       ISNULL(p.Seller, '') as Seller,
                       ISNULL(i.Name, 'Unknown') as Name,
                       ISNULL(i.SalePrice, 0) as SalePrice,
                       ISNULL(i.StockQty, 0) as StockQty,
                       ISNULL(i.MinStockLevel, 0) as MinStockLevel
                FROM Purchases p
                LEFT JOIN Items i ON p.ItemId = i.ItemId
                ORDER BY p.Date DESC";

            return await _dbContext.ExecuteReaderAsync(query, reader => new Purchase
            {
                Id = DbContext.SafeDataReader.GetSafeInt32(reader, 0),
                ItemId = DbContext.SafeDataReader.GetSafeInt32(reader, 1),
                Quantity = DbContext.SafeDataReader.GetSafeInt32(reader, 2),
                Price = DbContext.SafeDataReader.GetSafeDecimal(reader, 3),
                Date = DbContext.SafeDataReader.GetSafeDateTime(reader, 4),
                Seller = DbContext.SafeDataReader.GetSafeString(reader, 5),
                Item = new Item
                {
                    ItemId = DbContext.SafeDataReader.GetSafeInt32(reader, 1),
                    Name = DbContext.SafeDataReader.GetSafeString(reader, 6),
                    SalePrice = DbContext.SafeDataReader.GetSafeDecimal(reader, 7),
                    StockQty = DbContext.SafeDataReader.GetSafeInt32(reader, 8),
                    MinStockLevel = DbContext.SafeDataReader.GetSafeInt32(reader, 9)
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
            // Validate purchase object
            if (purchase == null)
                throw new ArgumentNullException(nameof(purchase));

            // Ensure required properties have valid values
            purchase.Quantity = purchase.Quantity <= 0 ? 1 : purchase.Quantity;
            purchase.Price = purchase.Price < 0 ? 0 : purchase.Price;
            purchase.Date = purchase.Date == default ? DateTime.Now : purchase.Date;

            using var connection = new SqlConnection(_dbContext.ConnectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Insert purchase record
                const string purchaseQuery = @"
                    INSERT INTO Purchases (ItemId, Quantity, Price, Date, Seller)
                    VALUES (@ItemId, @Quantity, @Price, @Date, @Seller);
                    SELECT SCOPE_IDENTITY();";

                var purchaseParams = new Dictionary<string, object>
                {
                    { "@ItemId", purchase.ItemId },
                    { "@Quantity", purchase.Quantity },
                    { "@Price", purchase.Price },
                    { "@Date", purchase.Date },
                    { "@Seller", purchase.Seller ?? string.Empty }
                };

                // Use the same connection for all operations
                var id = await _dbContext.ExecuteScalarAsync<decimal>(purchaseQuery, purchaseParams, transaction);

                // Update item stock using the same transaction (positive quantity to increase stock)
                await _itemService.UpdateStockAsync(purchase.ItemId, purchase.Quantity, transaction);

                await transaction.CommitAsync();
                return (int)id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                System.Diagnostics.Debug.WriteLine($"Error in AddPurchaseAsync: {ex.Message}");
                throw;
            }
        }
    }
}