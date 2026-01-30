using Godot;

namespace Vacuum.Services;

/// <summary>
/// Manages scene transitions while preserving singleton service state.
/// </summary>
public partial class SceneTransitionManager : BaseService
{
    public static SceneTransitionManager? Instance { get; private set; }

    public bool IsTransitioning { get; private set; }

    [Signal] public delegate void SceneChangingEventHandler(string scenePath);
    [Signal] public delegate void SceneChangedEventHandler(string scenePath);

    protected override void InitializeService()
    {
        Instance = this;
    }

    public Error ChangeScene(string scenePath)
    {
        if (IsTransitioning) return Error.Busy;

        IsTransitioning = true;
        EmitSignal(SignalName.SceneChanging, scenePath);

        var err = GetTree().ChangeSceneToFile(scenePath);
        if (err == Error.Ok)
        {
            CallDeferred(MethodName.OnSceneChanged, scenePath);
        }
        else
        {
            IsTransitioning = false;
            LogError($"Scene change failed: {err}");
        }
        return err;
    }

    private void OnSceneChanged(string scenePath)
    {
        IsTransitioning = false;
        EmitSignal(SignalName.SceneChanged, scenePath);
        Log($"Transitioned to {scenePath}");
    }
}
