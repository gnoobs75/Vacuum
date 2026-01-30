using System.Collections.Generic;
using Vacuum.Data.Models;

namespace Vacuum.Services.Interfaces;

public interface IExplorationService
{
    List<string> DiscoverSystem(string characterId, string systemId);
    bool ScanAnomaly(string characterId, string anomalyId);
    List<BookmarkData> GetBookmarks(string characterId, string? folder = null);
    BookmarkData CreateBookmark(string characterId, string name, float x, float y, float z, string folder = "Default");
    void DeleteBookmark(string bookmarkId);
    RouteData CalculateRoute(string characterId, float destX, float destY, float destZ);
}
