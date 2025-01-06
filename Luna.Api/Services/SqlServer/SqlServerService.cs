using Dapper;
using Luna.Api.Models;
using Microsoft.Data.SqlClient;

namespace Luna.Api.Services;

public class SqlServerService : ISqlServerService
{
    private readonly IConfiguration _config;

    public SqlServerService(IConfiguration config)
    {
        _config = config;
    }

    public IEnumerable<UserCard> GetUserCards()
    {
        List<UserCard> users = new();

        using (var connection = new SqlConnection(_config.GetConnectionString("AZURE_SQL_CONNECTIONSTRING")))
        {
            var sql = "SELECT * FROM [UserCard]";
            users = connection.Query<UserCard>(sql).ToList();
        }

        return users;
    }
}