using System.Collections.Generic;
using Godot;
using Vacuum.Core;
using Vacuum.Data.Enums;
using Vacuum.Systems.Flight;
using Vacuum.Systems.Navigation;

namespace Vacuum.UI;

/// <summary>
/// Solar system map panel toggled by M key.
/// Lists all celestial bodies. Right-click for Approach / Warp To context menu.
/// </summary>
public partial class SolarSystemMap : Control
{
    [Export] public NodePath ShipPath { get; set; } = "";
    [Export] public NodePath SolarSystemPath { get; set; } = "";

    private RigidBody3D? _ship;
    private Node3D? _solarSystem;
    private VBoxContainer? _listContainer;
    private PopupMenu? _contextMenu;
    private readonly List<MapEntry> _entries = new();
    private int _contextEntryIndex = -1;

    // Warp-To state
    private bool _warpCharging;
    private float _warpChargeTimer;
    private const float WarpChargeTime = 3f;
    private const float WarpArrivalDistance = 100f;
    private Vector3 _warpTarget;
    private string _warpTargetName = "";
    private Label? _warpChargeLabel;

    // Approach state
    private bool _approaching;
    private Vector3 _approachTarget;
    private string _approachTargetName = "";
    private Label? _approachLabel;

    private struct MapEntry
    {
        public string Name;
        public Vector3 Position;
        public string Type;
    }

    public override void _Ready()
    {
        Visible = false;
        BuildUi();
        CallDeferred(MethodName.ConnectNodes);
    }

    private void ConnectNodes()
    {
        if (!string.IsNullOrEmpty(ShipPath))
            _ship = GetNodeOrNull<RigidBody3D>(ShipPath);
        if (!string.IsNullOrEmpty(SolarSystemPath))
            _solarSystem = GetNodeOrNull<Node3D>(SolarSystemPath);

        GD.Print($"[Map] Ship resolved: {_ship != null}, SolarSystem resolved: {_solarSystem != null}");
        if (_solarSystem != null)
            GD.Print($"[Map] SolarSystem children: {_solarSystem.GetChildCount()}");

        // Defer population one more frame to ensure SolarSystemGenerator._Ready() has run
        GetTree().CreateTimer(0.1f).Timeout += PopulateEntries;
    }

    private void BuildUi()
    {
        // Full-screen overlay that catches clicks outside the panel to close
        var overlay = new ColorRect();
        overlay.Color = new Color(0, 0, 0, 0.4f);
        overlay.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        overlay.MouseFilter = MouseFilterEnum.Stop;
        AddChild(overlay);

        // Centered panel using anchor math
        var panel = new PanelContainer();
        panel.LayoutMode = 1;
        panel.AnchorLeft = 0.5f;
        panel.AnchorTop = 0.5f;
        panel.AnchorRight = 0.5f;
        panel.AnchorBottom = 0.5f;
        panel.OffsetLeft = -280;
        panel.OffsetTop = -300;
        panel.OffsetRight = 280;
        panel.OffsetBottom = 300;
        panel.GrowHorizontal = GrowDirection.Both;
        panel.GrowVertical = GrowDirection.Both;

        var styleBox = new StyleBoxFlat
        {
            BgColor = new Color(0.04f, 0.04f, 0.1f, 0.95f),
            BorderColor = new Color(0.25f, 0.35f, 0.65f, 0.9f),
            BorderWidthBottom = 2, BorderWidthTop = 2,
            BorderWidthLeft = 2, BorderWidthRight = 2,
            CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
            CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
            ContentMarginLeft = 12, ContentMarginRight = 12,
            ContentMarginTop = 12, ContentMarginBottom = 12
        };
        panel.AddThemeStyleboxOverride("panel", styleBox);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 6);

        // Title
        var title = new Label
        {
            Text = "SOLAR SYSTEM MAP",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.AddThemeFontSizeOverride("font_size", 20);
        title.Modulate = new Color(0.6f, 0.8f, 1f);
        vbox.AddChild(title);

        // Hint
        var hint = new Label
        {
            Text = "Right-click an entry for navigation options",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        hint.AddThemeFontSizeOverride("font_size", 12);
        hint.Modulate = new Color(0.45f, 0.45f, 0.55f);
        vbox.AddChild(hint);

        vbox.AddChild(new HSeparator());

        // Scroll list
        var scroll = new ScrollContainer();
        scroll.SizeFlagsVertical = SizeFlags.ExpandFill;

        _listContainer = new VBoxContainer();
        _listContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _listContainer.AddThemeConstantOverride("separation", 1);
        scroll.AddChild(_listContainer);
        vbox.AddChild(scroll);

        vbox.AddChild(new HSeparator());

        // Footer
        var footer = new Label
        {
            Text = "[M] Close",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        footer.Modulate = new Color(0.4f, 0.4f, 0.5f);
        vbox.AddChild(footer);

        panel.AddChild(vbox);
        AddChild(panel);

        // Context menu
        _contextMenu = new PopupMenu();
        _contextMenu.AddItem("Approach", 0);
        _contextMenu.AddItem("Warp To", 1);
        _contextMenu.IdPressed += OnContextMenuSelected;
        AddChild(_contextMenu);

        // Warp charge overlay label (shown during 3s charge)
        _warpChargeLabel = new Label();
        _warpChargeLabel.SetAnchorsAndOffsetsPreset(LayoutPreset.CenterBottom);
        _warpChargeLabel.OffsetTop = -80;
        _warpChargeLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _warpChargeLabel.AddThemeFontSizeOverride("font_size", 24);
        _warpChargeLabel.Modulate = new Color(0.4f, 0.7f, 1f);
        _warpChargeLabel.Visible = false;
        // Add to parent so it shows even when map is closed
        CallDeferred(MethodName.AddWarpLabelToParent);

        // Approach label
        _approachLabel = new Label();
        _approachLabel.SetAnchorsAndOffsetsPreset(LayoutPreset.CenterBottom);
        _approachLabel.OffsetTop = -50;
        _approachLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _approachLabel.AddThemeFontSizeOverride("font_size", 18);
        _approachLabel.Modulate = new Color(0.5f, 1f, 0.5f);
        _approachLabel.Visible = false;
    }

    private void AddWarpLabelToParent()
    {
        // Move warp/approach labels to the scene root so they stay visible when map closes
        var parent = GetParent();
        if (parent != null && _warpChargeLabel != null)
        {
            if (_warpChargeLabel.GetParent() != null)
                _warpChargeLabel.GetParent().RemoveChild(_warpChargeLabel);
            parent.AddChild(_warpChargeLabel);
        }
        if (parent != null && _approachLabel != null)
        {
            if (_approachLabel.GetParent() != null)
                _approachLabel.GetParent().RemoveChild(_approachLabel);
            parent.AddChild(_approachLabel);
        }
    }

    private void PopulateEntries()
    {
        _entries.Clear();
        if (_solarSystem == null) return;

        foreach (var child in _solarSystem.GetChildren())
        {
            if (child is not Node3D node3d) continue;
            string name = node3d.Name.ToString();

            string type;
            if (name.StartsWith("Star"))
                type = "Star";
            else if (name.StartsWith("Planet"))
                type = "Planet";
            else if (name.StartsWith("AsteroidBelt"))
                type = "Belt";
            else if (name.StartsWith("Station"))
                type = "Station";
            else if (name.StartsWith("SunDirectional") || name.StartsWith("WorldEnvironment"))
                continue; // skip non-celestial nodes
            else
                continue;

            _entries.Add(new MapEntry
            {
                Name = name,
                Position = node3d.GlobalPosition,
                Type = type
            });
        }

        GD.Print($"[Map] Populated {_entries.Count} entries");
        RefreshList();
    }

    private void RefreshList()
    {
        if (_listContainer == null) return;

        foreach (var child in _listContainer.GetChildren())
            child.QueueFree();

        for (int i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            int index = i;

            var row = new PanelContainer();
            var rowStyle = new StyleBoxFlat
            {
                BgColor = (i % 2 == 0)
                    ? new Color(0.06f, 0.06f, 0.14f, 0.6f)
                    : new Color(0.05f, 0.05f, 0.11f, 0.4f),
                ContentMarginLeft = 8, ContentMarginRight = 8,
                ContentMarginTop = 4, ContentMarginBottom = 4
            };
            row.AddThemeStyleboxOverride("panel", rowStyle);

            var hbox = new HBoxContainer();
            hbox.AddThemeConstantOverride("separation", 8);

            // Type icon
            Color typeColor = entry.Type switch
            {
                "Star" => new Color(1f, 0.9f, 0.3f),
                "Planet" => new Color(0.5f, 0.8f, 1f),
                "Belt" => new Color(0.8f, 0.65f, 0.45f),
                "Station" => new Color(0.5f, 1f, 0.5f),
                _ => Colors.White
            };
            string icon = entry.Type switch
            {
                "Star" => "★",
                "Planet" => "●",
                "Belt" => "◌◌◌",
                "Station" => "▣",
                _ => "?"
            };

            var iconLabel = new Label { Text = icon };
            iconLabel.Modulate = typeColor;
            iconLabel.CustomMinimumSize = new Vector2(40, 0);
            hbox.AddChild(iconLabel);

            // Name
            var nameLabel = new Label { Text = entry.Name };
            nameLabel.Modulate = typeColor;
            nameLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            hbox.AddChild(nameLabel);

            // Type tag
            var typeTag = new Label { Text = $"[{entry.Type}]" };
            typeTag.Modulate = new Color(typeColor, 0.6f);
            typeTag.CustomMinimumSize = new Vector2(60, 0);
            hbox.AddChild(typeTag);

            // Distance
            float dist = _ship != null ? _ship.GlobalPosition.DistanceTo(entry.Position) : 0;
            string distText = dist >= 1000 ? $"{dist / 1000:F1}km" : $"{dist:F0}m";
            var distLabel = new Label { Text = distText };
            distLabel.Modulate = new Color(0.5f, 0.5f, 0.6f);
            distLabel.CustomMinimumSize = new Vector2(70, 0);
            distLabel.HorizontalAlignment = HorizontalAlignment.Right;
            hbox.AddChild(distLabel);

            row.AddChild(hbox);

            // Right-click handler via an invisible button overlay
            var clickArea = new Button();
            clickArea.Flat = true;
            clickArea.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            clickArea.MouseFilter = MouseFilterEnum.Stop;
            clickArea.GuiInput += (evt) => OnRowInput(evt, index);
            row.AddChild(clickArea);

            _listContainer.AddChild(row);
        }
    }

    private void OnRowInput(InputEvent evt, int entryIndex)
    {
        if (evt is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Right)
        {
            _contextEntryIndex = entryIndex;
            _contextMenu?.Popup(new Rect2I((Vector2I)mb.GlobalPosition, new Vector2I(140, 60)));
        }
    }

    private void OnContextMenuSelected(long id)
    {
        if (_contextEntryIndex < 0 || _contextEntryIndex >= _entries.Count) return;
        var entry = _entries[_contextEntryIndex];

        if (id == 0)
        {
            // Approach - sublight travel
            StartApproach(entry.Position, entry.Name);
        }
        else if (id == 1)
        {
            // Warp To - 3 second charge then teleport
            StartWarpCharge(entry.Position, entry.Name);
        }

        // Close map
        Visible = false;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    private void StartApproach(Vector3 target, string name)
    {
        _approaching = true;
        _approachTarget = target;
        _approachTargetName = name;
        _warpCharging = false;
        GD.Print($"[Map] Approaching {name}");
    }

    private void StartWarpCharge(Vector3 target, string name)
    {
        _warpCharging = true;
        _warpChargeTimer = 0f;
        _warpTarget = target;
        _warpTargetName = name;
        _approaching = false;
        GD.Print($"[Map] Warp charging to {name}...");
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        // Warp charge countdown
        if (_warpCharging && _ship != null)
        {
            _warpChargeTimer += dt;
            float remaining = WarpChargeTime - _warpChargeTimer;

            if (_warpChargeLabel != null)
            {
                _warpChargeLabel.Visible = true;
                _warpChargeLabel.Text = $"WARP DRIVE CHARGING: {remaining:F1}s — {_warpTargetName}";
            }

            if (_warpChargeTimer >= WarpChargeTime)
            {
                // Teleport to 100m from target
                Vector3 direction = (_ship.GlobalPosition - _warpTarget).Normalized();
                // If ship is at the target, pick an arbitrary direction
                if (direction.LengthSquared() < 0.001f)
                    direction = Vector3.Forward;

                _ship.LinearVelocity = Vector3.Zero;
                _ship.AngularVelocity = Vector3.Zero;
                _ship.GlobalPosition = _warpTarget + direction * WarpArrivalDistance;

                // Face the target after warp
                Vector3 lookDir = (_warpTarget - _ship.GlobalPosition).Normalized();
                if (lookDir.LengthSquared() > 0.001f)
                {
                    var basis = Basis.LookingAt(lookDir, Vector3.Up);
                    _ship.GlobalTransform = new Transform3D(basis, _ship.GlobalPosition);
                }

                _warpCharging = false;
                if (_warpChargeLabel != null)
                    _warpChargeLabel.Visible = false;

                GD.Print($"[Map] Warped to {_warpTargetName}");
                GameEventBus.Instance?.EmitSignal(GameEventBus.SignalName.WarpCompleted,
                    _ship.Name, _ship.GlobalPosition);
            }
        }

        // Approach - fly at sublight speed toward target
        if (_approaching && _ship != null)
        {
            float dist = _ship.GlobalPosition.DistanceTo(_approachTarget);

            if (_approachLabel != null)
            {
                _approachLabel.Visible = true;
                string distText = dist >= 1000 ? $"{dist / 1000:F1}km" : $"{dist:F0}m";
                _approachLabel.Text = $"APPROACHING: {_approachTargetName} — {distText}";
            }

            if (dist < WarpArrivalDistance)
            {
                // Arrived
                _approaching = false;
                _ship.LinearVelocity = Vector3.Zero;
                if (_approachLabel != null)
                    _approachLabel.Visible = false;
                GD.Print($"[Map] Arrived at {_approachTargetName}");
                return;
            }

            // Steer ship toward target at its current max sublight speed
            Vector3 direction = (_approachTarget - _ship.GlobalPosition).Normalized();

            // Rotate toward target
            var targetBasis = Basis.LookingAt(direction, Vector3.Up);
            _ship.GlobalTransform = new Transform3D(
                _ship.GlobalTransform.Basis.Slerp(targetBasis, dt * 2f),
                _ship.GlobalTransform.Origin
            );

            // Apply thrust forward
            var shipPhysics = _ship as ShipPhysics;
            float thrustForce = shipPhysics?.ThrustForce ?? 50f;
            float maxSpeed = shipPhysics?.MaxSpeed ?? 200f;

            // Slow down as we get close
            float speedFactor = Mathf.Clamp(dist / 300f, 0.1f, 1f);
            Vector3 desiredVelocity = direction * maxSpeed * speedFactor;
            _ship.LinearVelocity = _ship.LinearVelocity.Lerp(desiredVelocity, dt * 3f);
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("toggle_map"))
        {
            Visible = !Visible;
            Input.MouseMode = Visible
                ? Input.MouseModeEnum.Visible
                : Input.MouseModeEnum.Captured;

            if (Visible)
            {
                if (_entries.Count == 0)
                    PopulateEntries();
                else
                    RefreshList();
            }
        }
    }
}
