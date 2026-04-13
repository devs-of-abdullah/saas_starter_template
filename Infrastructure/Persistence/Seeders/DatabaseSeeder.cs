using Domain.Entities.System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Seeders;

public sealed class DatabaseSeeder
{
    readonly AppDbContext _context;
    readonly IPasswordHasher<SystemOwnerEntity> _hasher;
    readonly IConfiguration _configuration;
    readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        AppDbContext context,
        IPasswordHasher<SystemOwnerEntity> hasher,
        IConfiguration configuration,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _hasher = hasher;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync(bool isProduction, CancellationToken ct = default)
    {
        if (await _context.SystemOwners.AnyAsync(ct))
            return; 

        string? email = _configuration["SystemOwner:Email"];
        string? password = Environment.GetEnvironmentVariable("SYSTEM_OWNER_PASSWORD") ?? _configuration["SystemOwner:Password"];

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("SystemOwner:Email is not configured. Set it in appsettings or SystemOwner:Email.");

        if (string.IsNullOrWhiteSpace(password))
        {
            if (isProduction)
                throw new InvalidOperationException("SystemOwner password is not configured. Set SYSTEM_OWNER_PASSWORD environment variable in production.");

            throw new InvalidOperationException("SystemOwner password is not configured. Set SYSTEM_OWNER_PASSWORD environment variable or SystemOwner:Password in appsettings.Development.json.");
        }

        SystemOwnerEntity owner = new()
        {
            Email = email.Trim().ToLowerInvariant(),
            IsActive = true
        };

        owner.PasswordHash = _hasher.HashPassword(owner, password);

        await _context.SystemOwners.AddAsync(owner, ct);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("SystemOwner seeded with email: {Email}", owner.Email);
    }
}
