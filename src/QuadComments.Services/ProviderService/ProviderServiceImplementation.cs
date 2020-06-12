using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuadComments.Data;
using QuadComments.Data.Entities;
using QuadComments.Services.Enums;
using QuadComments.Services.Models;
using QuadComments.Services.ProviderService.Enums;
using QuadComments.Services.ProviderService.Models;
using QuadComments.Services.ProviderService.Validators;

namespace QuadComments.Services.ProviderService
{
  public class ProviderServiceImplementation : IProviderService
  {
    private QuadCommentsDbContext QuadCommentsDbContext { get; }
    
    public ProviderServiceImplementation(QuadCommentsDbContext quadCommentsDbContext)
    {
      QuadCommentsDbContext = quadCommentsDbContext;
    }
    
    public async Task<GenericPagedResult<Provider>> GetProviders(GetProvidersInput input)
    {
      var inputPage = input?.Page ?? 1;
      var inputLimit = input?.Limit ?? 10;

      if (inputPage < 1)
      {
        inputPage = 1;
      }
      
      var inputSort = input?.Sort ?? SortDirectionEnum.Asc;
      var inputOrderBy = input?.OrderBy ?? OrderByProviderFieldEnum.Name;

      var limit = inputLimit < 1 ? 1 : inputLimit > 100 ? 100 : inputLimit;
      var skip = (inputPage - 1) * limit;

      var query = QuadCommentsDbContext
        .Providers
        .Take(limit)
        .Skip(skip);

      query = inputOrderBy switch
      {
        OrderByProviderFieldEnum.Name => inputSort == SortDirectionEnum.Asc
          ? query.OrderBy(x => x.Name)
          : query.OrderByDescending(x => x.Name),
        OrderByProviderFieldEnum.Created => inputSort == SortDirectionEnum.Asc
          ? query.OrderBy(x => x.Created)
          : query.OrderByDescending(x => x.Created),
        OrderByProviderFieldEnum.Updated => inputSort == SortDirectionEnum.Asc
          ? query.OrderBy(x => x.Updated)
          : query.OrderByDescending(x => x.Updated),
        _ => query
      };

      var providers = await query.ToListAsync();
      var total = await QuadCommentsDbContext.Providers.CountAsync();
      var pages = Math.Ceiling((double) total / limit);

      return new GenericPagedResult<Provider>()
      {
        Page = inputPage,
        Pages = double.IsNaN(pages) ? 0 : (int) pages,
        Total = total,
        Limit = limit,
        Data = providers
      };
    }
    public async Task<Provider> AddProvider(string name)
    {
      await ProviderNameValidator.ValidateAndThrowAsync(name);
      var now = DateTimeOffset.Now;

      var provider = new Provider
      {
        Name = name,
        Enabled = true,
        Created = now,
        Updated = now,
        Tokens = new List<string>()
      };

      await using var transaction = await QuadCommentsDbContext.Database.BeginTransactionAsync();
      try
      {
        await QuadCommentsDbContext.Providers.AddAsync(provider);
        await QuadCommentsDbContext.SaveChangesAsync();
        await transaction.CommitAsync();
      }
      catch
      {
        await transaction.RollbackAsync();
        throw;
      }

      return provider;
    }
    public async Task<Provider> AddProviderToken(Guid providerId)
    {
      await using var transaction = await QuadCommentsDbContext.Database.BeginTransactionAsync();
      try
      {
        var provider = await QuadCommentsDbContext
          .Providers
          .FirstOrDefaultAsync(x => x.Id == providerId);
        
        if (provider == null)
          throw new ArgumentException(providerId.ToString(), nameof(providerId));

        var tokensHashSet = provider.Tokens.ToHashSet();
        var tokensPrevCount = tokensHashSet.Count;
        while (tokensPrevCount == tokensHashSet.Count)
        {
          var hmac = new HMACSHA256();
          tokensHashSet.Add(Convert.ToBase64String(hmac.Key).Replace("==", string.Empty));
        }

        provider.Tokens = tokensHashSet.ToList();
        provider.Updated = DateTimeOffset.Now;

        await QuadCommentsDbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return provider;
      }
      catch
      {
        await transaction.RollbackAsync();
        throw;
      }
    }
    public async Task<Provider> DeleteProviderToken(Guid providerId, string token)
    {
      await ProviderTokenValidator.ValidateAndThrowAsync(token);
      await using var transaction = await QuadCommentsDbContext.Database.BeginTransactionAsync();
      try
      {
        var provider = await QuadCommentsDbContext
          .Providers
          .FirstOrDefaultAsync(x => x.Id == providerId);
        
        if (provider == null)
          throw new ArgumentException(providerId.ToString(), nameof(providerId));

        provider.Tokens.Remove(token);
        provider.Updated = DateTimeOffset.Now;

        await QuadCommentsDbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        return provider;
      }
      catch
      {
        await transaction.RollbackAsync();
        throw;
      }
    }
    public async Task<Provider> DisableProvider(Guid providerId)
    {
      await using var transaction = await QuadCommentsDbContext.Database.BeginTransactionAsync();
      try
      {
        var provider = await QuadCommentsDbContext
          .Providers
          .FirstOrDefaultAsync(x => x.Id == providerId);

        if (provider == null)
          throw new ArgumentException(providerId.ToString(), nameof(providerId));
        
        provider.Enabled = false;
        provider.Updated = DateTimeOffset.Now;

        await QuadCommentsDbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return provider;
      }
      catch
      {
        await transaction.RollbackAsync();
        throw;
      }
    }
    public async Task<Provider> EnableProvider(Guid providerId)
    {
      await using var transaction = await QuadCommentsDbContext.Database.BeginTransactionAsync();
      try
      {
        var provider = await QuadCommentsDbContext
          .Providers
          .FirstOrDefaultAsync(x => x.Id == providerId);
        
        if (provider == null)
          throw new ArgumentException(providerId.ToString(), nameof(providerId));

        provider.Enabled = true;
        provider.Updated = DateTimeOffset.Now;

        await QuadCommentsDbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return provider;
      }
      catch
      {
        await transaction.RollbackAsync();
        throw;
      }
    }
    public async Task<Provider> GetProvider(Guid providerId)
    {
      var provider = await QuadCommentsDbContext
        .Providers
        .FirstOrDefaultAsync(x => x.Id == providerId);

      if (provider == null)
        throw new ArgumentException(providerId.ToString(), nameof(providerId));
      
      return provider;
    }
    public async Task<Provider> RenameProvider(Guid providerId, string name)
    {
      await ProviderNameValidator.ValidateAndThrowAsync(name);
      await using var transaction = await QuadCommentsDbContext.Database.BeginTransactionAsync();
      try
      {
        var provider = await QuadCommentsDbContext
          .Providers
          .FirstOrDefaultAsync(x => x.Id == providerId);

        if (provider == null)
          throw new ArgumentException(providerId.ToString(), nameof(providerId));

        provider.Name = name;
        provider.Updated = DateTimeOffset.Now;

        await QuadCommentsDbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return provider;
      }
      catch
      {
        await transaction.RollbackAsync();
        throw;
      }
    }
  }
}