using System;
using System.Linq;
using Abp.Zero.EntityFrameworkCore;
using ClientPlatform.Authorization.Roles;
using ClientPlatform.Authorization.Users;
using ClientPlatform.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using ClientPlatform.AllianceManagement;
using ClientPlatform.VABusiness;
using ClientPlatform.UserManagement;
using ClientPlatform.BasicDataManagement;
using ClientPlatform.Kyc;
using ClientPlatform.Orders;
using ClientPlatform.Settings;

namespace ClientPlatform.EntityFrameworkCore;

public class ClientPlatformDbContext : AbpZeroDbContext<Tenant, Role, User, ClientPlatformDbContext>
{
    /* Define a DbSet for each entity of the application */

    public DbSet<Alliance> Alliances { get; set; }

    public DbSet<Merchant> Merchants { get; set; }

    public DbSet<VAAccount> VAAccounts { get; set; }

    public DbSet<DynamicForm> DynamicForm { get; set; }

    public DbSet<ClientUser> ClientUsers { get; set; }

    public DbSet<UserIdentity> UserIdentities { get; set; }

    public DbSet<VAAccountIdentity> VAAccountIdentities { get; set; }

    public DbSet<VAAccountIdentityDetail> VAAccountIdentityDetails { get; set; }


    public DbSet<CountryInfo> CountryInfos { get; set; }

    /* ================= KYC Entities ================= */
    public DbSet<KycApplicant> KycApplicants { get; set; }
    public DbSet<KycApplicantDocument> KycApplicantDocuments { get; set; }
    public DbSet<KycApplicantBeneficiary> KycApplicantBeneficiaries { get; set; }

    /* ================= Pay Entities (支付渠道) ================= */
    public DbSet<ClientPlatform.Pay.Entities.UserPayChannelCustomer> PayChannelCustomers { get; set; }
    public DbSet<ClientPlatform.Pay.Entities.UserPayChannelAccount> PayChannelAccounts { get; set; }
    public DbSet<ClientPlatform.Pay.Entities.PayChannelAccountPaymentMethod> PayChannelAccountPaymentMethods { get; set; }
    public DbSet<ClientPlatform.Pay.Entities.PayChannelRequestLog> PayChannelRequestLogs { get; set; }
    public DbSet<ClientPlatform.Pay.Entities.PayChannelServiceRequest> PayChannelServiceRequests { get; set; }
    public DbSet<ClientPlatform.Pay.Entities.PayChannelTransaction> PayChannelTransactions { get; set; }

    public DbSet<CollectionOrder> CollectionOrders { get; set; }
    public DbSet<CollectionOrderDetail> CollectionOrderDetails { get; set; }

    public DbSet<MerchantChannelCurrency> MerchantChannelCurrencies { get; set; }

    public DbSet<DataDictionary> DataDictionaries { get; set; }

    public DbSet<MerchantPaySetting> MerchantPaySettings { get; set; }

    public DbSet<MerchantSubCode> MerchantSubCodes { get; set; }

    public ClientPlatformDbContext(DbContextOptions<ClientPlatformDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AbpUser：邮箱按业务(UserType)区分唯一，同一邮箱可同时存在平台用户与业务用户
        modelBuilder.Entity<User>(entity =>
        {
            var oldIndex = entity.Metadata.GetIndexes()
                .FirstOrDefault(i => i.Name == "IX_AbpUsers_TenantId_NormalizedEmailAddress");
            if (oldIndex != null)
                entity.Metadata.RemoveIndex(oldIndex);

            entity.HasIndex(u => new { u.TenantId, u.NormalizedEmailAddress, u.UserType })
                .HasDatabaseName("IX_AbpUsers_TenantId_NormalizedEmailAddress_UserType")
                .IsUnique();
        });

        modelBuilder.Entity<MerchantSubCode>().ToTable(nameof(MerchantSubCodes));

        modelBuilder.Entity<Alliance>().ToTable(nameof(Alliances));

        modelBuilder.Entity<Merchant>().ToTable(nameof(Merchants));

        modelBuilder.Entity<VAAccount>().ToTable(nameof(VAAccounts));

        modelBuilder.Entity<DynamicForm>().ToTable(nameof(DynamicForm));

        modelBuilder.Entity<UserIdentity>().ToTable(nameof(UserIdentities));

        modelBuilder.Entity<ClientUser>().ToTable(nameof(ClientUsers));

        modelBuilder.Entity<VAAccountIdentity>().ToTable(nameof(VAAccountIdentities));

        modelBuilder.Entity<MerchantPaySetting>().ToTable(nameof(MerchantPaySettings));

        /* ================= KYC Entities Config ================= */
        modelBuilder.Entity<KycApplicant>().ToTable("AppKycApplicants");
        modelBuilder.Entity<KycApplicantDocument>().ToTable("AppKycApplicantDocuments");
        modelBuilder.Entity<KycApplicantBeneficiary>().ToTable("AppKycApplicantBeneficiaries");
        modelBuilder.Entity<MerchantChannelCurrency>().ToTable(nameof(MerchantChannelCurrencies));

        modelBuilder.Entity<DataDictionary>().ToTable(nameof(DataDictionaries));


        modelBuilder.Entity<VAAccountIdentityDetail>().ToTable(nameof(VAAccountIdentityDetails));

        modelBuilder.Entity<VAAccountIdentityDetail>().ToTable(nameof(VAAccountIdentityDetails));

        modelBuilder.Entity<CollectionOrder>().ToTable(nameof(CollectionOrders));
        modelBuilder.Entity<CollectionOrderDetail>().ToTable(nameof(CollectionOrderDetails));

        // PayChannelServiceRequest: Unique Index to prevent duplicate pending requests
        modelBuilder.Entity<ClientPlatform.Pay.Entities.PayChannelServiceRequest>()
            .HasIndex(r => new { r.UserId, r.ChannelProvider, r.Currency })
            .IsUnique()
            // Postgres Syntax: Status != Completed (4) AND Status != Failed (5)
            // Assuming Enum values: Completed=4, Failed=5
            // You might need to adjust the integer values based on your Enum definition
            .HasFilter("\"Status\" NOT IN (4, 5)");

        // Force all DateTime properties to be saved and read as UTC
        var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableDateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? v.Value.ToUniversalTime() : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsKeyless)
            {
                continue;
            }

            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableDateTimeConverter);
                }
            }
        }
    }
}
