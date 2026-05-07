using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ClientPlatform.EntityFrameworkCore;

public static class ClientPlatformDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<ClientPlatformDbContext> builder, string connectionString)
    {
        builder.UseNpgsql(connectionString);
    }

    public static void Configure(DbContextOptionsBuilder<ClientPlatformDbContext> builder, DbConnection connection)
    {
        builder.UseNpgsql(connection.ConnectionString);
    }
}
