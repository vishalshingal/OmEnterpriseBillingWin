using OmEnterpriseBillingWin.Models;
using OmEnterpriseBillingWin.Data;

namespace OmEnterpriseBillingWin.Services
{
    public class StakeholderService
    {
        private readonly DbContext _dbContext;

        public StakeholderService()
        {
            _dbContext = new DbContext();
        }

        public async Task<List<Stakeholder>> GetAllStakeholdersAsync()
        {
            const string query = "SELECT StakeholderId, Name, ContactNumber, Address, Type, DiscountPercentage FROM Stakeholders";
            return await _dbContext.ExecuteReaderAsync(query, reader => new Stakeholder
            {
                StakeholderId = reader.GetInt32(0),
                Name = reader.GetString(1),
                ContactNumber = reader.GetString(2),
                Address = reader.GetString(3),
                Type = (StakeholderType)reader.GetInt32(4),
                DiscountPercentage = reader.GetDecimal(5)
            });
        }

        public async Task<List<Stakeholder>> GetStakeholdersByTypeAsync(StakeholderType type)
        {
            const string query = "SELECT StakeholderId, Name, ContactNumber, Address, Type, DiscountPercentage FROM Stakeholders WHERE Type = @Type";
            var parameters = new Dictionary<string, object> { { "@Type", (int)type } };
            
            return await _dbContext.ExecuteReaderAsync(query, reader => new Stakeholder
            {
                StakeholderId = reader.GetInt32(0),
                Name = reader.GetString(1),
                ContactNumber = reader.GetString(2),
                Address = reader.GetString(3),
                Type = (StakeholderType)reader.GetInt32(4),
                DiscountPercentage = reader.GetDecimal(5)
            }, parameters);
        }

        public async Task<int> AddStakeholderAsync(Stakeholder stakeholder)
        {
            const string query = @"
                INSERT INTO Stakeholders (Name, ContactNumber, Address, Type, DiscountPercentage)
                VALUES (@Name, @ContactNumber, @Address, @Type, @DiscountPercentage);
                SELECT SCOPE_IDENTITY();";

            var parameters = new Dictionary<string, object>
            {
                { "@Name", stakeholder.Name },
                { "@ContactNumber", stakeholder.ContactNumber },
                { "@Address", stakeholder.Address },
                { "@Type", (int)stakeholder.Type },
                { "@DiscountPercentage", stakeholder.DiscountPercentage }
            };

            var id = await _dbContext.ExecuteScalarAsync<decimal>(query, parameters);
            return (int)id;
        }

        public async Task UpdateStakeholderAsync(Stakeholder stakeholder)
        {
            const string query = @"
                UPDATE Stakeholders 
                SET Name = @Name,
                    ContactNumber = @ContactNumber,
                    Address = @Address,
                    Type = @Type,
                    DiscountPercentage = @DiscountPercentage
                WHERE StakeholderId = @StakeholderId";

            var parameters = new Dictionary<string, object>
            {
                { "@StakeholderId", stakeholder.StakeholderId },
                { "@Name", stakeholder.Name },
                { "@ContactNumber", stakeholder.ContactNumber },
                { "@Address", stakeholder.Address },
                { "@Type", (int)stakeholder.Type },
                { "@DiscountPercentage", stakeholder.DiscountPercentage }
            };

            await _dbContext.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task DeleteStakeholderAsync(int stakeholderId)
        {
            const string query = "DELETE FROM Stakeholders WHERE StakeholderId = @StakeholderId";
            var parameters = new Dictionary<string, object> { { "@StakeholderId", stakeholderId } };
            await _dbContext.ExecuteNonQueryAsync(query, parameters);
        }
    }
}
