using System.Data.SqlClient;
using System.Data;

namespace OmEnterpriseBillingWin.Data
{
    public class DbContext
    {
        private readonly string _connectionString;

        public string ConnectionString => _connectionString;

        public DbContext()
        {
            _connectionString = "Server=localhost\\SQLEXPRESS;Database=OmEnterpriseBilling;Trusted_Connection=True;";
        }

        private SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        private async Task<SqlConnection> EnsureConnectionOpenAsync(SqlConnection? connection)
        {
            if (connection == null)
            {
                connection = CreateConnection();
                await connection.OpenAsync();
            }
            return connection;
        }

        public async Task<int> ExecuteNonQueryAsync(string query, IDictionary<string, object>? parameters = null, System.Data.SqlClient.SqlTransaction? transaction = null)
        {
            var connection = transaction?.Connection ?? await EnsureConnectionOpenAsync(null);
            var shouldDisposeConnection = transaction == null;

            try
            {
                using var command = connection.CreateCommand();
                if (transaction != null)
                    command.Transaction = transaction;
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                return await command.ExecuteNonQueryAsync();
            }
            finally
            {
                if (shouldDisposeConnection)
                    await connection.DisposeAsync();
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(string query, IDictionary<string, object>? parameters = null, System.Data.SqlClient.SqlTransaction? transaction = null)
        {
            var connection = transaction?.Connection ?? await EnsureConnectionOpenAsync(null);
            var shouldDisposeConnection = transaction == null;

            try
            {
                using var command = connection.CreateCommand();
                if (transaction != null)
                    command.Transaction = transaction;
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                var result = await command.ExecuteScalarAsync();
                return result == DBNull.Value ? default : (T)result;
            }
            finally
            {
                if (shouldDisposeConnection)
                    await connection.DisposeAsync();
            }
        }

        public async Task<List<T>> ExecuteReaderAsync<T>(string query, Func<SqlDataReader, T> mapper, IDictionary<string, object>? parameters = null, System.Data.SqlClient.SqlTransaction? transaction = null)
        {
            var connection = transaction?.Connection ?? await EnsureConnectionOpenAsync(null);
            var shouldDisposeConnection = transaction == null;

            try
            {
                using var command = connection.CreateCommand();
                if (transaction != null)
                    command.Transaction = transaction;
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                var results = new List<T>();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    results.Add(mapper(reader));
                }

                return results;
            }
            finally
            {
                if (shouldDisposeConnection)
                    await connection.DisposeAsync();
            }
        }
    }
}
