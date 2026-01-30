using System;
using Godot;
using Vacuum.Services.Mining.Config;

namespace Vacuum.AI.Mining;

/// <summary>
/// AI behavior system for claim jumper NPCs with patrol, ambush, and retreat patterns.
/// </summary>
public partial class ClaimJumperAI : Node3D
{
    public enum BehaviorState { Patrol, Approach, Ambush, Attack, Retreat, Destroyed }

    [Export] public float DetectionRange { get; set; } = 200f;
    [Export] public float AttackRange { get; set; } = 100f;
    [Export] public float RetreatHealthThreshold { get; set; } = 20f;

    public BehaviorState CurrentState { get; private set; } = BehaviorState.Patrol;
    public float Health { get; set; } = 100f;
    public float MaxHealth { get; set; } = 100f;
    public float Speed { get; set; } = 40f;
    public float DifficultyMultiplier { get; set; } = 1f;
    public string? FactionId { get; set; }

    private Node3D? _target;
    private float _stateTimer;
    private float _fireTimer;
    private float _fireInterval = 2f;
    private Vector3 _patrolCenter;
    private float _patrolAngle;

    [Signal] public delegate void StateChangedEventHandler(int newState);
    [Signal] public delegate void DestroyedEventHandler();
    [Signal] public delegate void FiredAtTargetEventHandler();

    public void Initialize(Node3D target, Vector3 patrolCenter, float difficulty = 1f)
    {
        _target = target;
        _patrolCenter = patrolCenter;
        DifficultyMultiplier = difficulty;
        MaxHealth = MiningConfig.ClaimJumperBaseHealth * difficulty;
        Health = MaxHealth;
        Speed = (float)GD.RandRange(MiningConfig.ClaimJumperMinSpeed, MiningConfig.ClaimJumperMaxSpeed) * difficulty;
        _fireInterval = Math.Max(0.5f, 2f / difficulty);
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _stateTimer += dt;

        if (Health <= 0)
        {
            TransitionTo(BehaviorState.Destroyed);
            return;
        }

        switch (CurrentState)
        {
            case BehaviorState.Patrol: UpdatePatrol(dt); break;
            case BehaviorState.Approach: UpdateApproach(dt); break;
            case BehaviorState.Ambush: UpdateAmbush(dt); break;
            case BehaviorState.Attack: UpdateAttack(dt); break;
            case BehaviorState.Retreat: UpdateRetreat(dt); break;
            case BehaviorState.Destroyed: break;
        }
    }

    private void UpdatePatrol(float dt)
    {
        _patrolAngle += dt * 0.5f;
        var target = _patrolCenter + new Vector3(
            Mathf.Cos(_patrolAngle) * 80f, 0f, Mathf.Sin(_patrolAngle) * 80f);
        GlobalPosition = GlobalPosition.Lerp(target, dt * 0.5f);

        if (_target != null && GlobalPosition.DistanceTo(_target.GlobalPosition) < DetectionRange)
            TransitionTo(BehaviorState.Approach);
    }

    private void UpdateApproach(float dt)
    {
        if (_target == null) { TransitionTo(BehaviorState.Patrol); return; }

        var dir = (_target.GlobalPosition - GlobalPosition).Normalized();
        GlobalPosition += dir * Speed * dt;
        LookAtTarget();

        float dist = GlobalPosition.DistanceTo(_target.GlobalPosition);
        if (dist < AttackRange)
            TransitionTo(BehaviorState.Attack);
    }

    private void UpdateAmbush(float dt)
    {
        // Wait silently, then surprise attack
        if (_stateTimer > 3f && _target != null)
        {
            float dist = GlobalPosition.DistanceTo(_target.GlobalPosition);
            if (dist < DetectionRange)
                TransitionTo(BehaviorState.Attack);
            else
                TransitionTo(BehaviorState.Approach);
        }
    }

    private void UpdateAttack(float dt)
    {
        if (_target == null) { TransitionTo(BehaviorState.Patrol); return; }

        // Check retreat
        if (Health < RetreatHealthThreshold)
        {
            TransitionTo(BehaviorState.Retreat);
            return;
        }

        // Orbit and fire
        float dist = GlobalPosition.DistanceTo(_target.GlobalPosition);
        if (dist > AttackRange * 1.5f)
        {
            var dir = (_target.GlobalPosition - GlobalPosition).Normalized();
            GlobalPosition += dir * Speed * dt;
        }
        else
        {
            _patrolAngle += dt * 0.8f;
            var orbit = _target.GlobalPosition + new Vector3(
                Mathf.Cos(_patrolAngle) * 35f,
                Mathf.Sin(_patrolAngle * 0.5f) * 10f,
                Mathf.Sin(_patrolAngle) * 35f);
            GlobalPosition = GlobalPosition.Lerp(orbit, 2f * dt);
        }

        LookAtTarget();

        _fireTimer += dt;
        if (_fireTimer >= _fireInterval)
        {
            _fireTimer = 0f;
            EmitSignal(SignalName.FiredAtTarget);
        }
    }

    private void UpdateRetreat(float dt)
    {
        if (_target == null) { TransitionTo(BehaviorState.Patrol); return; }

        var awayDir = (GlobalPosition - _target.GlobalPosition).Normalized();
        GlobalPosition += awayDir * Speed * 1.2f * dt;

        if (_target.GlobalPosition.DistanceTo(GlobalPosition) > DetectionRange * 2f)
            QueueFree();
    }

    private void TransitionTo(BehaviorState state)
    {
        if (CurrentState == state) return;
        CurrentState = state;
        _stateTimer = 0f;
        EmitSignal(SignalName.StateChanged, (int)state);

        if (state == BehaviorState.Destroyed)
        {
            EmitSignal(SignalName.Destroyed);
            QueueFree();
        }
    }

    public void TakeDamage(float amount)
    {
        Health -= amount;
        if (Health <= 0)
            TransitionTo(BehaviorState.Destroyed);
        else if (Health < RetreatHealthThreshold && CurrentState == BehaviorState.Attack)
            TransitionTo(BehaviorState.Retreat);
    }

    private void LookAtTarget()
    {
        if (_target == null) return;
        var dir = _target.GlobalPosition - GlobalPosition;
        if (dir.LengthSquared() > 1f)
            LookAt(_target.GlobalPosition, Vector3.Up);
    }
}
