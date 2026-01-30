using System.Threading;
using System.Threading.Tasks;

namespace Vacuum.Tasks.Examples;

/// <summary>
/// Example task: calculates whether a number is prime.
/// Demonstrates BaseTask usage pattern.
/// </summary>
public class PrimeCheckTask : BaseTask
{
    private readonly long _number;

    public PrimeCheckTask(long number)
    {
        _number = number;
        Name = $"PrimeCheck({number})";
    }

    public override Task<TaskResult> ExecuteAsync(CancellationToken ct)
    {
        if (_number <= 1)
            return Task.FromResult(TaskResult.Ok(false));

        for (long i = 2; i * i <= _number; i++)
        {
            ct.ThrowIfCancellationRequested();
            if (_number % i == 0)
                return Task.FromResult(TaskResult.Ok(false));
            if (i % 1000 == 0)
                ReportProgress((float)i / (float)_number);
        }

        ReportProgress(1f);
        return Task.FromResult(TaskResult.Ok(true));
    }
}
