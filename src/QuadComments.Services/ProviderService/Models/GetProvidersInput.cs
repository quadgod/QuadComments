using QuadComments.Services.Enums;
using QuadComments.Services.ProviderService.Enums;

namespace QuadComments.Services.ProviderService.Models
{
  public class GetProvidersInput
  {
    public int? Page { get; set; }
    public int? Limit { get; set; }
    public SortDirectionEnum? Sort { get; set; }
    public OrderByProviderFieldEnum? OrderBy { get; set; }
  }
}