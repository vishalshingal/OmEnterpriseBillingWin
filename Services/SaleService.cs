using Microsoft.Data.SqlClient; // Consistent namespace usage
using OmEnterpriseBillingWin.Models;
using OmEnterpriseBillingWin.Data;

namespace OmEnterpriseBillingWin.Services
{
    public class SaleService
    {
        private readonly DbContext _dbContext;
        private readonly ItemService _itemService;

        public SaleService()
        {
            _dbContext = new DbContext();
            _itemService = new ItemService();
        }

        public async Task<List<Sale>> GetAllSalesAsync()
        {
            const string query = @"
                SELECT ISNULL(s.SaleID, 0) as SaleID, 
                       ISNULL(s.ItemId, 0) as ItemId, 
                       ISNULL(s.Quantity, 0) as Quantity, 
                       ISNULL(s.Price, 0) as Price, 
                       ISNULL(s.Date, GETDATE()) as Date, 
                       ISNULL(s.StakeholderId, 0) as StakeholderId,
                       ISNULL(i.Name, 'Unknown') as Name, 
                       ISNULL(i.SalePrice, 0) as SalePrice, 
                       ISNULL(i.StockQty, 0) as StockQty, 
                       ISNULL(i.MinStockLevel, 0) as MinStockLevel
                FROM Sales s
                LEFT JOIN Items i ON s.ItemId = i.ItemId
                ORDER BY s.Date DESC";

            try
            {
                var sales = await _dbContext.ExecuteReaderAsync(query, reader => new Sale
                {
                    Id = DbContext.SafeDataReader.GetSafeInt32(reader, 0),
                    ItemId = DbContext.SafeDataReader.GetSafeInt32(reader, 1),
                    Quantity = DbContext.SafeDataReader.GetSafeInt32(reader, 2),
                    Price = DbContext.SafeDataReader.GetSafeDecimal(reader, 3),
                    Date = DbContext.SafeDataReader.GetSafeDateTime(reader, 4),
                    StakeholderId = DbContext.SafeDataReader.GetSafeInt32(reader, 5),
                    Item = new Item
                    {
                        ItemId = DbContext.SafeDataReader.GetSafeInt32(reader, 1),
                        Name = DbContext.SafeDataReader.GetSafeString(reader, 6),
                        SalePrice = DbContext.SafeDataReader.GetSafeDecimal(reader, 7),
                        StockQty = DbContext.SafeDataReader.GetSafeInt32(reader, 8),
                        MinStockLevel = DbContext.SafeDataReader.GetSafeInt32(reader, 9)
                    }
                });

                return sales ?? new List<Sale>();
            }
            catch (Exception ex)
            {
                // Log the error if you have logging
                System.Diagnostics.Debug.WriteLine($"Error in GetAllSalesAsync: {ex.Message}");
                return new List<Sale>();
            }
        }

        public async Task<decimal> GetMonthlySaleTotal()
        {
            const string query = @"
                SELECT ISNULL(SUM(ISNULL(Quantity, 0) * ISNULL(Price, 0)), 0)
                FROM Sales
                WHERE Date >= DATEADD(MONTH, -1, GETDATE())";

            try
            {
                var result = await _dbContext.ExecuteScalarAsync<decimal>(query);
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetMonthlySaleTotal: {ex.Message}");
                return 0;
            }
        }

        public async Task<decimal> GetDiscountedPrice(decimal originalPrice, int stakeholderId)
        {
            const string query = @"
                SELECT DiscountPercentage 
                FROM Stakeholders 
                WHERE StakeholderId = @StakeholderId";

            var parameters = new Dictionary<string, object> { { "@StakeholderId", stakeholderId } };
            var discount = await _dbContext.ExecuteScalarAsync<decimal>(query, parameters);

            return originalPrice * (1 - discount / 100);
        }

        public async Task<decimal> GetTotalSaleAmount(DateTime startDate, DateTime endDate)
        {
            const string query = @"
                SELECT COALESCE(SUM(Quantity * Price), 0)
                FROM Sales
                WHERE Date >= @StartDate AND Date < @EndDate";

            var parameters = new Dictionary<string, object>
            {
                { "@StartDate", startDate },
                { "@EndDate", endDate }
            };

            return await _dbContext.ExecuteScalarAsync<decimal>(query, parameters);
        }

        public async Task<int> AddSaleAsync(Sale sale)
        {
            // Validate sale object
            if (sale == null)
                throw new ArgumentNullException(nameof(sale));

            // Ensure required properties have valid values
            sale.Quantity = sale.Quantity <= 0 ? 1 : sale.Quantity;
            sale.Price = sale.Price < 0 ? 0 : sale.Price;
            sale.Date = sale.Date == default ? DateTime.Now : sale.Date;
            sale.StakeholderId = sale.StakeholderId <= 0 ? 0 : sale.StakeholderId;

            using var connection = new SqlConnection(_dbContext.ConnectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Check stock availability
                var item = await _itemService.GetItemByIdAsync(sale.ItemId);
                if (item == null || item.StockQty < sale.Quantity)
                {
                    throw new InvalidOperationException("Insufficient stock");
                }

                // Insert sale record using SqlCommand directly with the transaction
                const string saleQuery = @"
                    INSERT INTO Sales (ItemId, Quantity, Price, Date, StakeholderId)
                    VALUES (@ItemId, @Quantity, @Price, @Date, @StakeholderId);
                    SELECT SCOPE_IDENTITY();";

                using var command = new SqlCommand(saleQuery, connection, transaction);
                command.Parameters.AddWithValue("@ItemId", sale.ItemId);
                command.Parameters.AddWithValue("@Quantity", sale.Quantity);
                command.Parameters.AddWithValue("@Price", sale.Price);
                command.Parameters.AddWithValue("@Date", sale.Date);
                command.Parameters.AddWithValue("@StakeholderId", sale.StakeholderId);

                var result = await command.ExecuteScalarAsync();
                var saleId = Convert.ToInt32(result);

                // Update item stock - if your ItemService doesn't support the new transaction type,
                // you may need to implement the stock update directly here
                try
                {
                    await _itemService.UpdateStockAsync(sale.ItemId, -sale.Quantity, transaction);
                }
                catch (ArgumentException)
                {
                    // If ItemService doesn't support Microsoft.Data.SqlClient.SqlTransaction,
                    // implement stock update directly
                    const string stockUpdateQuery = @"
                        UPDATE Items 
                        SET StockQty = StockQty + @QuantityChange 
                        WHERE ItemId = @ItemId";

                    using var stockCommand = new SqlCommand(stockUpdateQuery, connection, transaction);
                    stockCommand.Parameters.AddWithValue("@ItemId", sale.ItemId);
                    stockCommand.Parameters.AddWithValue("@QuantityChange", -sale.Quantity);
                    await stockCommand.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return saleId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                System.Diagnostics.Debug.WriteLine($"Error in AddSaleAsync: {ex.Message}");
                throw;
            }
        }
    }
}