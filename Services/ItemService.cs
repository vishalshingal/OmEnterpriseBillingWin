using OmEnterpriseBillingWin.Models;
using OmEnterpriseBillingWin.Data;
using System.Data.SqlClient;

namespace OmEnterpriseBillingWin.Services
{
    public class ItemService
    {
        private readonly DbContext _dbContext;

        public ItemService()
        {
            _dbContext = new DbContext();
        }

        public async Task<List<Item>> GetAllItemsAsync()
        {
            const string query = "SELECT ItemId, Name, SalePrice, StockQty, MinStockLevel FROM Items";
            return await _dbContext.ExecuteReaderAsync(query, reader => new Item
            {
                ItemId = reader.GetInt32(0),
                Name = reader.GetString(1),
                SalePrice = reader.GetDecimal(2),
                StockQty = reader.GetInt32(3),
                MinStockLevel = reader.GetInt32(4)
            });
        }

        public async Task<Item?> GetItemByIdAsync(int id)
        {
            const string query = "SELECT ItemId, Name, SalePrice, StockQty, MinStockLevel FROM Items WHERE ItemId = @ItemId";
            var parameters = new Dictionary<string, object> { { "@ItemId", id } };

            var items = await _dbContext.ExecuteReaderAsync(query, reader => new Item
            {
                ItemId = reader.GetInt32(0),
                Name = reader.GetString(1),
                SalePrice = reader.GetDecimal(2),
                StockQty = reader.GetInt32(3),
                MinStockLevel = reader.GetInt32(4)
            }, parameters);

            return items.FirstOrDefault();
        }

        public async Task<int> AddItemAsync(Item item)
        {
            const string query = @"
                INSERT INTO Items (Name, SalePrice, StockQty, MinStockLevel)
                VALUES (@Name, @SalePrice, @StockQty, @MinStockLevel);
                SELECT SCOPE_IDENTITY();";

            var parameters = new Dictionary<string, object>
            {
                { "@Name", item.Name },
                { "@SalePrice", item.SalePrice },
                { "@StockQty", item.StockQty },
                { "@MinStockLevel", item.MinStockLevel }
            };

            var id = await _dbContext.ExecuteScalarAsync<decimal>(query, parameters);
            return (int)id;
        }

        public async Task UpdateItemAsync(Item item)
        {
            const string query = @"
                UPDATE Items 
                SET Name = @Name,
                    SalePrice = @SalePrice,
                    StockQty = @StockQty,
                    MinStockLevel = @MinStockLevel
                WHERE ItemId = @ItemId";

            var parameters = new Dictionary<string, object>
            {
                { "@ItemId", item.ItemId },
                { "@Name", item.Name },
                { "@SalePrice", item.SalePrice },
                { "@StockQty", item.StockQty },
                { "@MinStockLevel", item.MinStockLevel }
            };

            await _dbContext.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task UpdateStockAsync(int itemId, int quantity, SqlTransaction? transaction = null)
        {
            const string query = @"
                UPDATE Items 
                SET StockQty = StockQty + @Quantity
                WHERE ItemId = @ItemId";

            var parameters = new Dictionary<string, object>
            {
                { "@ItemId", itemId },
                { "@Quantity", quantity }
            };

            await _dbContext.ExecuteNonQueryAsync(query, parameters, transaction);
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            // First check if the item has any related sales or purchases
            const string checkQuery = @"
                SELECT COUNT(*) 
                FROM (
                    SELECT ItemId FROM Sales WHERE ItemId = @Id
                    UNION
                    SELECT ItemId FROM Purchases WHERE ItemId = @Id
                ) AS T";

            var parameters = new Dictionary<string, object> { { "@Id", id } };
            var count = await _dbContext.ExecuteScalarAsync<int>(checkQuery, parameters);

            if (count > 0)
            {
                throw new InvalidOperationException("Cannot delete item: It has associated sales or purchases.");
            }

            const string deleteQuery = "DELETE FROM Items WHERE Id = @Id";
            var rowsAffected = await _dbContext.ExecuteNonQueryAsync(deleteQuery, parameters);
            return rowsAffected > 0;
        }
    }
}
