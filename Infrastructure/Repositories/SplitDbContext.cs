using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Split.Domain.Primitives;
using Split.Domain.Transaction;
using Split.Domain.User;

namespace Split.Infrastructure.Repositories;

public class SplitDbContext(DbContextOptions<SplitDbContext> options) : DbContext(options)
{
#nullable disable
    public DbSet<TransactionAggregate> Transactions { get; init; }
    public DbSet<UserAggregate> Users { get; init; }

#nullable restore

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        BuildTransactionModel(modelBuilder);
        BuildUserModel(modelBuilder);
    }

    private static void BuildTransactionModel(ModelBuilder modelBuilder)
    {
        var transactionBuilder = modelBuilder.Entity<TransactionAggregate>();
        transactionBuilder.HasKey(t => t.Id);
        transactionBuilder.Property(t => t.Description);

        var amountBuilder = transactionBuilder.OwnsOne(t => t.Amount);
        amountBuilder.Property(a => a.Amount);
        amountBuilder.Property(a => a.Currency);

        var userBuilder = modelBuilder.Entity<UserAggregate>();
        userBuilder.HasMany<TransactionAggregate>().WithOne().HasForeignKey(t => t.SenderId);

        var recipientsBuilder = transactionBuilder.PrimitiveCollection(t => t.RecipientIds);
        recipientsBuilder.ElementType().HasConversion<UserIdConverter>();

        transactionBuilder.Property(t => t.CreatedAt);
        transactionBuilder.Property(t => t.RemovedAt);
    }

    private static void BuildUserModel(ModelBuilder modelBuilder)
    {
        var userBuilder = modelBuilder.Entity<UserAggregate>();
        userBuilder.HasKey(u => u.Id);
        userBuilder.Property(u => u.Name);
        userBuilder.HasIndex(u => u.PhoneNumber).IsUnique();
        userBuilder.Property(u => u.CreatedAt);
        userBuilder.Property(u => u.RemovedAt);

        var authKeyBuilder = userBuilder.OwnsMany(u => u.AuthKeys);
        authKeyBuilder.HasKey(k => k.Id);
        authKeyBuilder.Property(k => k.Key);
        authKeyBuilder.Property(k => k.SignCount);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<UserId>().HaveConversion<UserIdConverter>();

        configurationBuilder.Properties<PhoneNumber>().HaveConversion<PhoneNumberConverter>();

        configurationBuilder.Properties<TransactionId>().HaveConversion<TransactionIdConverter>();

        configurationBuilder.Properties<Currency>().HaveConversion<CurrencyConverter>();

        configurationBuilder.Properties<AuthKeyId>().HaveConversion<AuthKeyIdConverter>();
    }
}

file class UserIdConverter() : ValueConverter<UserId, string>(v => v.Value, v => new UserId(v));

file class PhoneNumberConverter() : ValueConverter<PhoneNumber, string>(v => v.Value, v => new PhoneNumber(v));

file class TransactionIdConverter() : ValueConverter<TransactionId, Guid>(v => v.Value, v => new TransactionId(v));

file class CurrencyConverter() : ValueConverter<Currency, string>(v => v.Value, v => new Currency(v));

file class AuthKeyIdConverter() : ValueConverter<AuthKeyId, string>(v => v.Value, v => new AuthKeyId(v));
