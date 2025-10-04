using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using OmEnterpriseBillingWin.Data;
using OmEnterpriseBillingWin.Models;

namespace OmEnterpriseBillingWin.Services
{
    public class SaleService
    {
        private readonly DatabaseConnection _dbConnection;

        public SaleService()
        {
            _dbConnection = new DatabaseConnection();
        }

        // Existing methods remain the same...

        public async Task<int> CreateSaleOrderAsync(SaleOrder order)
        {
            using var connection = _dbConnection.GetConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Generate order number
                order.OrderNumber = await GenerateOrderNumberAsync(connection, transaction);
                order.CreatedAt = DateTime.Now;

                // Insert sale order
                var orderSql = @"
                    INSERT INTO SaleOrders (OrderNumber, StakeholderId, OrderDate, Subtotal, TaxAmount, GrandTotal, Status, Notes, CreatedAt)
                    VALUES (@OrderNumber, @StakeholderId, @OrderDate, @Subtotal, @TaxAmount, @GrandTotal, @Status, @Notes, @CreatedAt);
                    SELECT last_insert_rowid();";

                using var orderCmd = new SqliteCommand(orderSql, connection, transaction);
                orderCmd.Parameters.AddWithValue("@OrderNumber", order.OrderNumber);
                orderCmd.Parameters.AddWithValue("@StakeholderId", order.StakeholderId);
                orderCmd.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                orderCmd.Parameters.AddWithValue("@Subtotal", order.Subtotal);
                orderCmd.Parameters.AddWithValue("@TaxAmount", order.TaxAmount);
                orderCmd.Parameters.AddWithValue("@GrandTotal", order.GrandTotal);
                orderCmd.Parameters.AddWithValue("@Status", order.Status);
                orderCmd.Parameters.AddWithValue("@Notes", order.Notes ?? string.Empty);
                orderCmd.Parameters.AddWithValue("@CreatedAt", order.CreatedAt);

                var orderId = Convert.ToInt32(await orderCmd.ExecuteScalarAsync());
                order.OrderId = orderId;

                // Insert order items
                foreach (var item in order.OrderItems)
                {
                    var itemSql = @"
                        INSERT INTO SaleOrderItems (OrderId, ItemId, Quantity, UnitPrice, Total)
                        VALUES (@OrderId, @ItemId, @Quantity, @UnitPrice, @Total)";

                    using var itemCmd = new SqliteCommand(itemSql, connection, transaction);
                    itemCmd.Parameters.AddWithValue("@OrderId", orderId);
                    itemCmd.Parameters.AddWithValue("@ItemId", item.ItemId);
                    itemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    itemCmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                    itemCmd.Parameters.AddWithValue("@Total", item.Total);

                    await itemCmd.ExecuteNonQueryAsync();

                    // Create individual sale records for backward compatibility
                    var sale = new Sale
                    {
                        ItemId = item.ItemId,
                        Quantity = item.Quantity,
                        Price = item.UnitPrice,
                        Date = order.OrderDate,
                        StakeholderId = order.StakeholderId
                    };
                    await AddSaleAsync(sale, connection, transaction);
                }

                await transaction.CommitAsync();
                return orderId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<SaleOrder>> GetSaleOrdersByStakeholderAsync(int stakeholderId)
        {
            using var connection = _dbConnection.GetConnection();
            await connection.OpenAsync();

            var sql = @"
                SELECT so.*, s.Name as StakeholderName, s.Type as StakeholderType, s.Email, s.Phone, s.Address
                FROM SaleOrders so
                INNER JOIN Stakeholders s ON so.StakeholderId = s.StakeholderId
                WHERE so.StakeholderId = @StakeholderId
                ORDER BY so.OrderDate DESC";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@StakeholderId", stakeholderId);

            var orders = new List<SaleOrder>();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var order = new SaleOrder
                {
                    OrderId = reader.GetInt32("OrderId"),
                    OrderNumber = reader.GetString("OrderNumber"),
                    StakeholderId = reader.GetInt32("StakeholderId"),
                    OrderDate = reader.GetDateTime("OrderDate"),
                    Subtotal = reader.GetDecimal("Subtotal"),
                    TaxAmount = reader.GetDecimal("TaxAmount"),
                    GrandTotal = reader.GetDecimal("GrandTotal"),
                    Status = reader.GetString("Status"),
                    Notes = reader.GetString("Notes"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    Stakeholder = new Stakeholder
                    {
                        StakeholderId = reader.GetInt32("StakeholderId"),
                        Name = reader.GetString("StakeholderName"),
                        Type = Enum.Parse<StakeholderType>(reader.GetString("StakeholderType")),
                        Email = reader.GetString("Email"),
                        Phone = reader.GetString("Phone"),
                        Address = reader.GetString("Address")
                    }
                };

                orders.Add(order);
            }

            // Load order items for each order
            foreach (var order in orders)
            {
                order.OrderItems = await GetSaleOrderItemsAsync(order.OrderId);
            }

            return orders;
        }

        public async Task<SaleOrder?> GetSaleOrderByIdAsync(int orderId)
        {
            using var connection = _dbConnection.GetConnection();
            await connection.OpenAsync();

            var sql = @"
                SELECT so.*, s.Name as StakeholderName, s.Type as StakeholderType, s.Email, s.Phone, s.Address
                FROM SaleOrders so
                INNER JOIN Stakeholders s ON so.StakeholderId = s.StakeholderId
                WHERE so.OrderId = @OrderId";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@OrderId", orderId);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var order = new SaleOrder
                {
                    OrderId = reader.GetInt32("OrderId"),
                    OrderNumber = reader.GetString("OrderNumber"),
                    StakeholderId = reader.GetInt32("StakeholderId"),
                    OrderDate = reader.GetDateTime("OrderDate"),
                    Subtotal = reader.GetDecimal("Subtotal"),
                    TaxAmount = reader.GetDecimal("TaxAmount"),
                    GrandTotal = reader.GetDecimal("GrandTotal"),
                    Status = reader.GetString("Status"),
                    Notes = reader.GetString("Notes"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    Stakeholder = new Stakeholder
                    {
                        StakeholderId = reader.GetInt32("StakeholderId"),
                        Name = reader.GetString("StakeholderName"),
                        Type = Enum.Parse<StakeholderType>(reader.GetString("StakeholderType")),
                        Email = reader.GetString("Email"),
                        Phone = reader.GetString("Phone"),
                        Address = reader.GetString("Address")
                    }
                };

                order.OrderItems = await GetSaleOrderItemsAsync(orderId);
                return order;
            }

            return null;
        }

        private async Task<List<SaleOrderItem>> GetSaleOrderItemsAsync(int orderId)
        {
            using var connection = _dbConnection.GetConnection();
            await connection.OpenAsync();

            var sql = @"
                SELECT soi.*, i.Name as ItemName, i.SalePrice, i.StockQty
                FROM SaleOrderItems soi
                INNER JOIN Items i ON soi.ItemId = i.ItemId
                WHERE soi.OrderId = @OrderId
                ORDER BY soi.OrderItemId";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@OrderId", orderId);

            var items = new List<SaleOrderItem>();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var item = new SaleOrderItem
                {
                    OrderItemId = reader.GetInt32("OrderItemId"),
                    OrderId = reader.GetInt32("OrderId"),
                    ItemId = reader.GetInt32("ItemId"),
                    Quantity = reader.GetInt32("Quantity"),
                    UnitPrice = reader.GetDecimal("UnitPrice"),
                    Total = reader.GetDecimal("Total"),
                    Item = new Item
                    {
                        ItemId = reader.GetInt32("ItemId"),
                        Name = reader.GetString("ItemName"),
                        SalePrice = reader.GetDecimal("SalePrice"),
                        StockQty = reader.GetInt32("StockQty")
                    }
                };

                items.Add(item);
            }

            return items;
        }

        private async Task<string> GenerateOrderNumberAsync(SqliteConnection connection, SqliteTransaction transaction)
        {
            var today = DateTime.Today;
            var prefix = $"ORD{today:yyyyMMdd}";

            var sql = "SELECT COUNT(*) FROM SaleOrders WHERE OrderNumber LIKE @Prefix";
            using var command = new SqliteCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@Prefix", $"{prefix}%");

            var count = Convert.ToInt32(await command.ExecuteScalarAsync()) + 1;
            return $"{prefix}{count:D3}";
        }

        // Existing AddSaleAsync method should be updated to support transactions
        public async Task AddSaleAsync(Sale sale, SqliteConnection connection, SqliteTransaction transaction)
        {
            var sql = @"
                INSERT INTO Sales (ItemId, Quantity, Price, Date, StakeholderId)
                VALUES (@ItemId, @Quantity, @Price, @Date, @StakeholderId)";

            using var command = new SqliteCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@ItemId", sale.ItemId);
            command.Parameters.AddWithValue("@Quantity", sale.Quantity);
            command.Parameters.AddWithValue("@Price", sale.Price);
            command.Parameters.AddWithValue("@Date", sale.Date);
            command.Parameters.AddWithValue("@StakeholderId", sale.StakeholderId);

            await command.ExecuteNonQueryAsync();

            // Update stock
            var updateStockSql = "UPDATE Items SET StockQty = StockQty - @Quantity WHERE ItemId = @ItemId";
            using var updateCommand = new SqliteCommand(updateStockSql, connection, transaction);
            updateCommand.Parameters.AddWithValue("@Quantity", sale.Quantity);
            updateCommand.Parameters.AddWithValue("@ItemId", sale.ItemId);
            await updateCommand.ExecuteNonQueryAsync();
        }
    }
}
