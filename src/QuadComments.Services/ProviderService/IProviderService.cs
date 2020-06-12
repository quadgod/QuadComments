using System;
using System.Threading.Tasks;
using QuadComments.Data.Entities;
using QuadComments.Services.Models;
using QuadComments.Services.ProviderService.Models;

namespace QuadComments.Services.ProviderService
{
  public interface IProviderService
  {
    public Task<GenericPagedResult<Provider>> GetProviders(GetProvidersInput input);
    public Task<Provider> AddProvider(string name);
    public Task<Provider> AddProviderToken(Guid providerId);
    public Task<Provider> DeleteProviderToken(Guid providerId, string token);
    public Task<Provider> DisableProvider(Guid providerId);
    public Task<Provider> EnableProvider(Guid providerId);
    public Task<Provider> GetProvider(Guid providerId);
    public Task<Provider> RenameProvider(Guid providerId, string name);
  }
}