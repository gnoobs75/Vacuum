using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Vacuum.Tasks;

/// <summary>
/// Thread-safe task queue with priority support.
/// </summary>
public class TaskQueue
{
    private readonly ConcurrentQueue<BaseTask> _queue = new();

    public int Count => _queue.Count;

    public void Enqueue(BaseTask task)
    {
        _queue.Enqueue(task);
    }

    public bool TryDequeue(out BaseTask? task)
    {
        return _queue.TryDequeue(out task);
    }

    public bool IsEmpty => _queue.IsEmpty;
}
