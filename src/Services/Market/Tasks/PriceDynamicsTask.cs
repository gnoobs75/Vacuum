using System.Threading;
using System.Threading.Tasks;
using Vacuum.Tasks;

namespace Vacuum.Services.Market.Tasks;

/// <summary>
/// Background task for dynamic price adjustments based on supply/demand.
/// </summary>
public class PriceDynamicsTask : BaseTask
{
    public int ItemsUpdated { get; private set; }

    public PriceDynamicsTask()
    {
        Name = "PriceDynamics";
    }

    public override Task<TaskResult> ExecuteAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReportProgress(0.2f);

        var engine = PriceCalculationEngine.Instance;
        if (engine == null)
        {
            ReportProgress(1f);
            return Task.FromResult(TaskResult.Fail("PriceCalculationEngine not available"));
        }

        // Update supply/demand from order books
        var service = MarketService.Instance;
        if (service != null)
        {
            foreach (var item in service.Data.Items.GetAll())
            {
                ct.ThrowIfCancellationRequested();
                engine.UpdateSupplyDemand(item.ItemId);
            }
        }

        ReportProgress(0.5f);

        // Run price tick
        ItemsUpdated = engine.RunPriceTick();

        ReportProgress(0.8f);

        // Cleanup expired orders
        OrderMatchingEngine.Instance?.CleanupExpiredOrders();

        ReportProgress(1f);
        return Task.FromResult(TaskResult.Ok($"Price dynamics: {ItemsUpdated} items updated"));
    }
}
