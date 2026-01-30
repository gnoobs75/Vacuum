using System.Collections.Generic;
using Godot;

namespace Vacuum.Services;

/// <summary>
/// Manages service lifecycle and health monitoring.
/// </summary>
public partial class ServiceManager : BaseService
{
    public static ServiceManager? Instance { get; private set; }

    private readonly List<BaseService> _managedServices = new();

    protected override void InitializeService()
    {
        Instance = this;
    }

    public void RegisterService(BaseService service)
    {
        _managedServices.Add(service);
    }

    public void StartAll()
    {
        foreach (var svc in _managedServices)
        {
            if (svc.State == ServiceState.Ready)
                svc.StartService();
        }
        Log($"Started {_managedServices.Count} services.");
    }

    public void StopAll()
    {
        for (int i = _managedServices.Count - 1; i >= 0; i--)
        {
            if (_managedServices[i].State == ServiceState.Running)
                _managedServices[i].StopService();
        }
    }

    public Dictionary<string, string> GetHealthReport()
    {
        var report = new Dictionary<string, string>();
        foreach (var svc in _managedServices)
            report[svc.ServiceName] = svc.State.ToString();
        return report;
    }
}
