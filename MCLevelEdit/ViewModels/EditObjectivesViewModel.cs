using Avalonia;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Domain.MC2;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;

namespace MCLevelEdit.ViewModels;

/// <summary>
/// A Magic Carpet 2 level's 8 objective slots (the Objectives node under the World). Each change is made to the level
/// at once. An objective's entity or place can be picked on the map, and while the panel is open the map shows each
/// objective as a numbered marker. There is one of these for the editor's life: the event aggregator cannot take a
/// handler off again, so it is loaded again each time the node is selected instead of made again.
/// </summary>
public class EditObjectivesViewModel : ReactiveObject
{
    public enum PickKind { Entity, Position }

    private const float PICK_RADIUS = 4;

    private readonly EventAggregator<object> _eventAggregator;
    private readonly IMapService _mapService;

    private bool _isShown;
    private int _pickSlot = -1;
    private PickKind _pickKind;

    public ObservableCollection<ObjectiveSlotViewModel> Slots { get; } = new ObservableCollection<ObjectiveSlotViewModel>();


    /// <summary>The slot picking on the map (0 to 7), or -1.</summary>
    public int PickSlot => _pickSlot;
    public PickKind Picking => _pickKind;

    public EditObjectivesViewModel(EventAggregator<object> eventAggregator, IMapService mapService)
    {
        _eventAggregator = eventAggregator;
        _mapService = mapService;

        _eventAggregator.RegisterEvent("OnCursorObjectivePicked", (sender, args) =>
        {
            if (args?.Item is ValueTuple<Point, bool, bool> cursorEvent)
                OnMapClicked(cursorEvent.Item1, cursorEvent.Item2, cursorEvent.Item3);
        });

        // something else took the map's clicks: no longer picking (the map has already stopped)
        _eventAggregator.RegisterEvent("OnToolSelected", (sender, args) => EndPick(tellMap: false));
        _eventAggregator.RegisterEvent("OnEntitySelectionModeChanged", (sender, args) => EndPick(tellMap: false));
        _eventAggregator.RegisterEvent("EntitiesSelected", (sender, args) => Hide());

        // the entities the objectives point at changed: their names, warnings and markers kept up to date
        _eventAggregator.RegisterEvent("AddEntity", (sender, args) => RefreshIfShown());
        _eventAggregator.RegisterEvent("UpdateEntity", (sender, args) => RefreshIfShown());
        _eventAggregator.RegisterEvent("DeleteEntity", (sender, args) => RefreshIfShown());
        _eventAggregator.RegisterEvent("RefreshEntities", (sender, args) => RefreshIfShown());
        _eventAggregator.RegisterEvent("RefreshWizards", (sender, args) => RefreshIfShown());
        _eventAggregator.RegisterEvent("UpdateWizard", (sender, args) => RefreshIfShown());
    }

    /// <summary>The level open now, if it is a Magic Carpet 2 level.</summary>
    public Mc2Map? Map => _mapService.GetMap() as Mc2Map;

    /// <summary>The objectives node selected: the level's objectives shown (8 slots, empty ones added if the level has fewer).</summary>
    public void Show()
    {
        var map = Map;
        if (map is null)
        {
            Hide();
            return;
        }

        while (map.Objectives.Count < LevelFormat.MC2_OBJECTIVE_COUNT)
            map.Objectives.Add(new Objective());

        _isShown = true;
        EndPick(tellMap: true);

        if (Slots.Count != LevelFormat.MC2_OBJECTIVE_COUNT)
        {
            Slots.Clear();
            for (int i = 0; i < LevelFormat.MC2_OBJECTIVE_COUNT; i++)
                Slots.Add(new ObjectiveSlotViewModel(this, i));
        }
        Refresh();
    }

    /// <summary>Another node selected: no longer picking (the map has already stopped), no markers.</summary>
    public void Hide()
    {
        _isShown = false;
        EndPick(tellMap: false);
    }

    internal Objective? GetObjective(int slot)
    {
        var objectives = Map?.Objectives;
        return objectives is not null && slot >= 0 && slot < objectives.Count ? objectives[slot] : null;
    }

    /// <summary>One slot changed: every slot read again (emptying one can leave the ones after it in a gap), and the markers.</summary>
    internal void OnSlotChanged(ObjectiveSlotViewModel slot) => Refresh();

    private void RefreshIfShown()
    {
        if (_isShown)
            Refresh();
    }

    public void Refresh()
    {
        foreach (var slot in Slots)
            slot.Refresh();
        ShowMarkers();
    }

    /// <summary>The objectives on the map: where each one's entity is, and its place for those that have one.</summary>
    private void ShowMarkers()
    {
        if (!_isShown)
            return;
        var markers = new List<(int Slot, Point Tile)>();
        var map = Map;
        if (map is not null)
        {
            for (int i = 0; i < map.Objectives.Count; i++)
            {
                var objective = map.Objectives[i];
                if (ObjectiveTypeInfo.UsesEntity(objective.Type) && FindEntity(objective.Value) is Entity entity)
                    markers.Add((i + 1, new Point(entity.Position.X, entity.Position.Y)));
                if (ObjectiveTypeInfo.UsesPosition(objective.Type))
                    markers.Add((i + 1, new Point(objective.X, objective.Y)));
            }
        }
        _eventAggregator.RaiseEvent("ShowObjectiveMarkers", this, new PubSubEventArgs<object>(markers));
    }

    internal Entity? FindEntity(int id)
    {
        if (id <= 0 || id > ushort.MaxValue)
            return null;
        var entity = _mapService.GetEntity((ushort)id);
        return entity is null || entity.IsInert() ? null : entity;
    }

    /// <summary>The objective in a slot swapped with the one in another (the narration goes by the slot, so it changes too).</summary>
    internal void Swap(int slot, int other)
    {
        var objectives = Map?.Objectives;
        if (objectives is null || slot < 0 || other < 0 || slot >= objectives.Count || other >= objectives.Count)
            return;
        (objectives[slot], objectives[other]) = (objectives[other], objectives[slot]);
        EndPick(tellMap: true);
        Refresh();
    }

    /// <summary>The next click on the map picks the slot's entity or place (picking it again stops).</summary>
    internal void StartPick(int slot, PickKind kind)
    {
        if (_pickSlot == slot && _pickKind == kind)
        {
            EndPick(tellMap: true);
            return;
        }

        _pickSlot = slot;
        _pickKind = kind;
        RaisePickChanged();
        _eventAggregator.RaiseEvent("OnObjectivePickModeChanged", this, new PubSubEventArgs<object>(true));
    }

    private void EndPick(bool tellMap)
    {
        bool wasPicking = _pickSlot >= 0;
        _pickSlot = -1;
        if (wasPicking)
        {
            RaisePickChanged();
            if (tellMap)
                _eventAggregator.RaiseEvent("OnObjectivePickModeChanged", this, new PubSubEventArgs<object>(false));
        }
    }

    private void RaisePickChanged()
    {
        this.RaisePropertyChanged(nameof(PickSlot));
        this.RaisePropertyChanged(nameof(Picking));
        foreach (var slot in Slots)
            slot.RaisePickChanged();
    }

    /// <summary>The map clicked while picking (its tile): the slot's entity (the nearest) or place set. The map has stopped picking.</summary>
    public void OnMapClicked(Point tile, bool left, bool right)
    {
        int slotIndex = _pickSlot;
        var kind = _pickKind;
        var objective = GetObjective(slotIndex);
        if (!left || right || objective is null)
        {
            EndPick(tellMap: false);
            return;
        }

        if (kind == PickKind.Position)
        {
            objective.X = (short)Math.Clamp((int)tile.X, 0, Globals.MAX_MAP_SIZE - 1);
            objective.Y = (short)Math.Clamp((int)tile.Y, 0, Globals.MAX_MAP_SIZE - 1);
            EndPick(tellMap: false);
        }
        else
        {
            var entity = NearestEntity(tile);
            if (entity is null)
            {
                EndPick(tellMap: false);
                return;
            }
            objective.Value = (short)entity.Id;
            EndPick(tellMap: false);
        }

        if (slotIndex < Slots.Count)
            OnSlotChanged(Slots[slotIndex]);
    }

    /// <summary>The entity on the tile, or else the nearest within a few tiles (lowest id first).</summary>
    private Entity? NearestEntity(Point tile)
    {
        var map = Map;
        if (map is null)
            return null;
        var at = new Vector2((float)tile.X, (float)tile.Y);
        return map.VisibleEntities
            .Select(e => (Entity: e, Distance: Vector2.Distance(at, new Vector2(e.Position.X, e.Position.Y))))
            .Where(e => e.Distance < PICK_RADIUS)
            .OrderBy(e => e.Distance)
            .ThenBy(e => e.Entity.Id)
            .Select(e => e.Entity)
            .FirstOrDefault();
    }

    /// <summary>The slot's entity selected on the map (and in the tree), which shows its own properties.</summary>
    internal void GoToEntity(int id)
    {
        if (FindEntity(id) is null)
            return;
        _eventAggregator.RaiseEvent("SelectEntities", this, new PubSubEventArgs<object>(((IList<int>)new List<int> { id }, false)));
    }
}

/// <summary>An objective type in the type combo box: its name, and what it does (the tooltip).</summary>
public record ObjectiveTypeOption(string Name, string Help);

/// <summary>One of the 8 objective slots: its type and what its Value, X and Y mean for that type.</summary>
public class ObjectiveSlotViewModel : ReactiveObject
{
    private readonly EditObjectivesViewModel _parent;
    private readonly int _slot;

    /// <summary>The types to choose from, in the order of the type combo box (empty first, then by number).</summary>
    public static IList<ObjectiveType> Types { get; } = Enum.GetValues<ObjectiveType>().OrderBy(t => (int)t).ToList();
    public static IList<ObjectiveTypeOption> TypeOptions { get; } = Types
        .Select(t => new ObjectiveTypeOption(t == ObjectiveType.None ? "(empty)" : $"{(int)t}: {ObjectiveTypeInfo.Name(t)}", ObjectiveTypeInfo.Help(t)))
        .ToList();

    public ObjectiveSlotViewModel(EditObjectivesViewModel parent, int slot)
    {
        _parent = parent;
        _slot = slot;
        PickEntityCommand = ReactiveCommand.Create(() => _parent.StartPick(_slot, EditObjectivesViewModel.PickKind.Entity));
        PickPositionCommand = ReactiveCommand.Create(() => _parent.StartPick(_slot, EditObjectivesViewModel.PickKind.Position));
        GoToEntityCommand = ReactiveCommand.Create(() => _parent.GoToEntity(Objective?.Value ?? 0));
        MoveUpCommand = ReactiveCommand.Create(() => _parent.Swap(_slot, _slot - 1));
        MoveDownCommand = ReactiveCommand.Create(() => _parent.Swap(_slot, _slot + 1));
        ClearCommand = ReactiveCommand.Create(() =>
        {
            if (Objective is Objective objective)
            {
                objective.Type = ObjectiveType.None;
                objective.Value = 0;
                objective.X = 0;
                objective.Y = 0;
                _parent.OnSlotChanged(this);
            }
        });
    }

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> PickEntityCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> PickPositionCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> GoToEntityCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> MoveUpCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> MoveDownCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> ClearCommand { get; }

    private Objective? Objective => _parent.GetObjective(_slot);
    private ObjectiveType Type => Objective?.Type ?? ObjectiveType.None;

    /// <summary>The slot's number, from 1.</summary>
    public int Number => _slot + 1;
    public string Header => $"Objective {Number}";
    public bool CanMoveUp => _slot > 0;
    public bool CanMoveDown => _slot < LevelFormat.MC2_OBJECTIVE_COUNT - 1;

    /// <summary>The type, as its place in <see cref="TypeNames"/> (-1: a type the editor does not know, kept as it is).</summary>
    public int TypeIdx
    {
        get => Types.IndexOf(Type);
        set
        {
            if (value < 0 || value >= Types.Count || Objective is not Objective objective || objective.Type == Types[value])
                return;
            objective.Type = Types[value];
            _parent.OnSlotChanged(this);
        }
    }

    public bool IsUsed => Type != ObjectiveType.None;
    public string Help => Enum.IsDefined(Type) ? ObjectiveTypeInfo.Help(Type) : $"Type {(int)Type} is not known: it is kept as it is.";

    public bool ShowEntity => ObjectiveTypeInfo.UsesEntity(Type);
    public bool ShowPosition => ObjectiveTypeInfo.UsesPosition(Type);
    public bool ShowWizard => ObjectiveTypeInfo.UsesWizard(Type);
    /// <summary>Value typed in as a number: the mana share, or the value of a type whose meaning is not known.</summary>
    public bool ShowNumber => ObjectiveTypeInfo.UsesPercent(Type) || ObjectiveTypeInfo.UsesRawValue(Type) || (IsUsed && !Enum.IsDefined(Type));
    public string NumberLabel => ObjectiveTypeInfo.UsesPercent(Type) ? "Mana %:" : "Value:";

    public bool IsPickingEntity => _parent.PickSlot == _slot && _parent.Picking == EditObjectivesViewModel.PickKind.Entity;
    public bool IsPickingPosition => _parent.PickSlot == _slot && _parent.Picking == EditObjectivesViewModel.PickKind.Position;
    public string PickEntityText => IsPickingEntity ? "Picking..." : "Pick on map";
    public string PickPositionText => IsPickingPosition ? "Picking..." : "Pick on map";

    /// <summary>The entity's id (its slot in the level).</summary>
    public int EntityId
    {
        get => Objective?.Value ?? 0;
        set => SetValue(value);
    }

    /// <summary>The mana share, or a value whose meaning is not known.</summary>
    public int NumberValue
    {
        get => Objective?.Value ?? 0;
        set => SetValue(value);
    }

    private void SetValue(int value)
    {
        if (Objective is not Objective objective)
            return;
        var v = (short)Math.Clamp(value, short.MinValue, short.MaxValue);
        if (objective.Value == v)
            return;
        objective.Value = v;
        _parent.OnSlotChanged(this);
    }

    public int X
    {
        get => Objective?.X ?? 0;
        set => SetPosition(value, null);
    }

    public int Y
    {
        get => Objective?.Y ?? 0;
        set => SetPosition(null, value);
    }

    private void SetPosition(int? x, int? y)
    {
        if (Objective is not Objective objective)
            return;
        if (x is int newX)
            objective.X = (short)Math.Clamp(newX, 0, Globals.MAX_MAP_SIZE - 1);
        if (y is int newY)
            objective.Y = (short)Math.Clamp(newY, 0, Globals.MAX_MAP_SIZE - 1);
        _parent.OnSlotChanged(this);
    }

    /// <summary>The wizards to choose from: slot, name, and whether the level has them.</summary>
    // the wizard list, made again only when the wizards change: a new list resets the combo box, which then writes its
    // choice back into Value
    private IList<string> _wizardNames = new List<string>();

    public IList<string> WizardNames => _wizardNames;

    private void RefreshWizardNames()
    {
        var names = _parent.Map?.Wizards
            .Select((w, i) => $"{i + 1}: {w.Name}{(w.IsActive ? string.Empty : " (not in level)")}")
            .ToList() ?? new List<string>();
        if (names.SequenceEqual(_wizardNames))
            return;
        _wizardNames = names;
        this.RaisePropertyChanged(nameof(WizardNames));
    }

    /// <summary>The wizard chosen, by its place in the list; Value is its number, from 1 (the player). Only a Kill wizard
    /// objective takes it: for the other types the (hidden) combo box leaves Value alone, as it is an entity, a share or
    /// something else there.</summary>
    public int WizardIdx
    {
        get => ShowWizard && Objective is Objective objective ? objective.Value - 1 : -1;
        set
        {
            if (ShowWizard && value >= 0)
                SetValue(value + 1);
        }
    }

    /// <summary>The entity the objective points at, as the tree names it.</summary>
    public string EntityText
    {
        get
        {
            var entity = _parent.FindEntity(EntityId);
            if (entity is null)
                return $"No entity {EntityId} in the level";
            var text = Describe(entity);
            return Type == ObjectiveType.KillCreatureType ? $"{text}: every {entity.EntityType.Model.Name}" : text;
        }
    }

    public bool CanGoToEntity => _parent.FindEntity(EntityId) is not null;

    public static string Describe(Entity entity) =>
        $"{entity.Id:D4}: {entity.EntityType.Model.Name} at ({entity.Position.X},{entity.Position.Y})";

    internal void RaisePickChanged()
    {
        this.RaisePropertyChanged(nameof(IsPickingEntity));
        this.RaisePropertyChanged(nameof(IsPickingPosition));
        this.RaisePropertyChanged(nameof(PickEntityText));
        this.RaisePropertyChanged(nameof(PickPositionText));
    }

    /// <summary>Every field read again from the level (the slot, its entity or the wizards changed).</summary>
    public void Refresh()
    {
        foreach (var name in new[]
        {
            nameof(TypeIdx), nameof(IsUsed), nameof(Help), nameof(ShowEntity), nameof(ShowPosition), nameof(ShowWizard),
            nameof(ShowNumber), nameof(NumberLabel), nameof(EntityId), nameof(NumberValue), nameof(X), nameof(Y),
            nameof(EntityText), nameof(CanGoToEntity)
        })
            this.RaisePropertyChanged(name);
        RefreshWizardNames();
        this.RaisePropertyChanged(nameof(WizardIdx));
        RaisePickChanged();
    }
}
