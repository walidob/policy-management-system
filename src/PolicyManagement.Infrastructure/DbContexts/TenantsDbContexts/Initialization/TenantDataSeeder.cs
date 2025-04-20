// Generated using AI.
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolicyManagement.Domain.Entities.DefaultDb;
using PolicyManagement.Domain.Entities.TenantsDb;
using PolicyManagement.Domain.Entities.TenantsDb.Lookup;
using PolicyManagement.Domain.Enums;

namespace PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts.Initialization;

public class TenantDataSeeder
{
    private readonly IMultiTenantStore<AppTenantInfo> _tenantStore;
    private readonly ILogger<TenantDataSeeder> _logger;
    private readonly Random _random = new();

    public TenantDataSeeder(
        IMultiTenantStore<AppTenantInfo> tenantStore,
        ILogger<TenantDataSeeder> logger)
    {
        _tenantStore = tenantStore;
        _logger = logger;
    }

    public async Task SeedAllTenantsAsync()
    {
        _logger.LogInformation("Seeding tenants databases");

        var tenants = await _tenantStore.GetAllAsync();
        if (!tenants.Any())
        {
            _logger.LogInformation("No tenants found");
            return;
        }

        foreach (var tenant in tenants)
        {
            _logger.LogInformation("Seeding tenant {TenantName} ({TenantId})", tenant.Name, tenant.Id);

            try
            {
                var options = new DbContextOptionsBuilder<TenantDbContextBase>()
                    .UseSqlServer(tenant.ConnectionString)
                    .Options;

                using var context = new TenantDbContextBase(options);

                await SeedTenantDetailsAsync(context, tenant);
                await SeedLookupTablesAsync(context);
                await GenerateRandomDataAsync(context, tenant.Id);

                _logger.LogInformation("Successfully seeded tenant {TenantName}", tenant.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding tenant {TenantName} ({TenantId})",
                    tenant.Name, tenant.Id);
            }
        }

        _logger.LogInformation("Completed seeding all tenant databases");
    }

    private async Task SeedTenantDetailsAsync(TenantDbContextBase context, AppTenantInfo tenant)  
    {
        if (await context.TenantDetails.AnyAsync())
        {
            return;
        }

        string[] primaryColors = { "#1976D2", "#F44336", "#4CAF50", "#FF9800", "#9C27B0", "#607D8B" };
        string[] secondaryColors = { "#03A9F4", "#E91E63", "#8BC34A", "#FFC107", "#673AB7", "#795548" };

        var tenantDetail = new TenantDetails
        {
            Name = tenant.Name,
            LogoUrl = $"/images/tenant-logos/{tenant.Identifier.ToLower()}-logo.png",
            PrimaryColor = primaryColors[_random.Next(primaryColors.Length)],
            SecondaryColor = secondaryColors[_random.Next(secondaryColors.Length)],
            Address = "123 Business Ave, Suite 100, New York, NY 10001",
            ContactEmail = $"contact@{tenant.Identifier.ToLower()}.com",
            ContactPhone = $"+1-{_random.Next(100, 999)}-{_random.Next(100, 999)}-{_random.Next(1000, 9999)}",
            Website = $"https://www.{tenant.Identifier.ToLower()}.com",
            IsActive = true
        };

        await context.TenantDetails.AddAsync(tenantDetail);
        await context.SaveChangesAsync();
    }

    private static async Task SeedLookupTablesAsync(TenantDbContextBase context)
    {
        var policyTypes = Enum.GetValues<PolicyType>()
            .Cast<PolicyType>()
            .Select(pt => new PolicyTypeLookup { Name = pt.ToString() });

        foreach (var policyType in policyTypes)
        {
            if (!await context.PolicyTypes.AnyAsync(pt => pt.Name == policyType.Name))
            {
                context.PolicyTypes.Add(policyType);
            }
        }

        var claimStatuses = Enum.GetValues<ClaimStatus>()
            .Cast<ClaimStatus>()
            .Select(cs => new ClaimStatusLookup { Name = cs.ToString() });

        foreach (var claimStatus in claimStatuses)
        {
            if (!await context.ClaimStatuses.AnyAsync(cs => cs.Name == claimStatus.Name))
            {
                context.ClaimStatuses.Add(claimStatus);
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task GenerateRandomDataAsync(TenantDbContextBase context, string tenantId)
    {
        if (await context.Policies.AnyAsync())
        {
            return;
        }

        var policyTypes = await context.PolicyTypes.ToListAsync();

        var policies = new List<Policy>();
        for (int i = 0; i < 25; i++)  
        {
            var randomPolicyType = policyTypes[_random.Next(policyTypes.Count)];
            string policyName;
            string policyDescription;
            
            // Generate more diverse policy names and descriptions
            switch (i % 5)
            {
                case 0:
                    policyName = $"Home Insurance Policy {i + 1}";
                    policyDescription = $"Comprehensive home insurance coverage protecting against damage, theft, and liability for property {i + 1}";
                    break;
                case 1:
                    policyName = $"Auto Insurance Plan {i + 1}";
                    policyDescription = $"Full coverage auto insurance with collision, liability, and personal injury protection for vehicle {i + 1}";
                    break;
                case 2:
                    policyName = $"Life Insurance Package {i + 1}";
                    policyDescription = $"Term life insurance policy providing financial protection for beneficiaries in case of policy holder {i + 1}'s death";
                    break;
                case 3:
                    policyName = $"Health Insurance Coverage {i + 1}";
                    policyDescription = $"Medical insurance plan covering hospitalization, medications, and preventive care for policy {i + 1}";
                    break;
                default:
                    policyName = $"Business Liability Policy {i + 1}";
                    policyDescription = $"Commercial insurance protecting against financial loss from business-related liability claims for business {i + 1}";
                    break;
            }
            
            var policy = new Policy
            {
                Name = policyName,
                Description = policyDescription,
                CreationDate = DateTime.Now.AddDays(-_random.Next(30, 60)),
                EffectiveDate = DateTime.Now.AddDays(-_random.Next(1, 30)),
                ExpiryDate = DateTime.Now.AddDays(_random.Next(30, 365)),
                PolicyTypeId = randomPolicyType.Id,
                IsActive = i < 20, // Make some policies inactive
                TenantId = tenantId
            };

            policies.Add(policy);
        }

        await context.Policies.AddRangeAsync(policies);
        await context.SaveChangesAsync();

        var clients = new List<Client>();
        for (int i = 0; i < 15; i++)  
        {
            var randomName = GetRandomName();
            
            var client = new Client
            {
                FullName = randomName,
                Email = $"client{_random.Next(1000, 9999)}@example.com",
                PhoneNumber = $"+1-{_random.Next(100, 999)}-{_random.Next(100, 999)}-{_random.Next(1000, 9999)}",
                DateOfBirth = DateTime.Now.AddYears(-_random.Next(20, 80))
            };

            clients.Add(client);
        }

        await context.Clients.AddRangeAsync(clients);
        await context.SaveChangesAsync();

        var clientPolicies = new List<ClientPolicy>();
        
        foreach (var client in clients)
        {
            // Assign 2-4 policies to each client
            var assignedPolicyCount = _random.Next(2, 5);
            var randomPolicies = policies.OrderBy(x => Guid.NewGuid()).Take(assignedPolicyCount).ToList();
            
            foreach (var policy in randomPolicies)
            {
                var clientPolicy = new ClientPolicy
                {
                    PolicyId = policy.Id,
                    ClientId = client.Id
                };
                
                clientPolicies.Add(clientPolicy);
            }
        }
        
        foreach (var policy in policies)
        {
            if (!clientPolicies.Any(cp => cp.PolicyId == policy.Id))
            {
                var randomClient = clients[_random.Next(clients.Count)];
                
                var clientPolicy = new ClientPolicy
                {
                    PolicyId = policy.Id,
                    ClientId = randomClient.Id
                };
                
                clientPolicies.Add(clientPolicy);
            }
        }
        
        await context.ClientPolicies.AddRangeAsync(clientPolicies);
        await context.SaveChangesAsync();

        var claims = new List<Claim>();
        var claimStatuses = await context.ClaimStatuses.ToListAsync();

        foreach (var policy in policies)
        {
            var claimCount = _random.Next(0, 5);
            for (int i = 0; i < claimCount; i++)
            {
                var randomStatus = claimStatuses[_random.Next(claimStatuses.Count)];
                
                var policyClients = clientPolicies
                    .Where(cp => cp.PolicyId == policy.Id)
                    .Select(cp => cp.ClientId)
                    .ToList();
                
                int? clientId = null;
                if (policyClients.Count != 0)
                {
                    clientId = policyClients[_random.Next(policyClients.Count)];
                }
                
                var claim = new Claim
                {
                    PolicyId = policy.Id,
                    ClientId = clientId,
                    Title = $"Claim for {policy.Name} #{i + 1}",
                    Description = $"This is a sample claim description for policy {policy.Name}",
                    Amount = _random.Next(100, 2000),
                    Status = randomStatus.Id,
                    ClaimNumber = $"CLM-{DateTime.Now.Year}-{100000 + _random.Next(0, 900000)}"
                };

                claims.Add(claim);
            }
        }

        await context.Claims.AddRangeAsync(claims);
        await context.SaveChangesAsync();
    }

    private string GetRandomName()
    {
        string[] firstNames = { "John", "Jane", "Michael", "Sarah", "David", "Emily", "Robert", "Lisa", "James", "Mary", 
                               "William", "Emma", "Daniel", "Olivia", "Matthew", "Sophia", "Thomas", "Isabella", "Richard", "Mia" };
        string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Wilson", "Taylor", 
                              "Anderson", "Thomas", "Jackson", "White", "Harris", "Martin", "Thompson", "Robinson", "Lewis", "Walker" };

        return $"{firstNames[_random.Next(firstNames.Length)]} {lastNames[_random.Next(lastNames.Length)]}";
    }
}