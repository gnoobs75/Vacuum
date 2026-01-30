using Vacuum.Data.Enums;
using Vacuum.Data.Models;

namespace Vacuum.UI.Market.Components;

/// <summary>
/// Filter state manager for market search.
/// </summary>
public static class MarketSearchFilters
{
    public static MarketSearchFilter CreateDefault() => new();

    public static MarketSearchFilter ForCategory(ItemCategory category) => new() { Category = category };

    public static MarketSearchFilter ForSearch(string term) => new() { SearchTerm = term };

    public static MarketSearchFilter ForPriceRange(double min, double max) =>
        new() { MinPrice = min, MaxPrice = max };

    public static MarketSearchFilter SortedBy(MarketSortOption sort, bool ascending = true) =>
        new() { SortBy = sort, Ascending = ascending };
}
