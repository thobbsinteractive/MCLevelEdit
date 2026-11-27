using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCLevelEdit.ViewModels
{
    /// <summary>
    /// The properties of more than one entity selected together. A field shows its value where they all have the same,
    /// else it is empty; a value entered is given to them all. The position is that of the first of them (the lowest id):
    /// changed, they all move by as much, round the edges of the map, so they stay the same distance apart.
    /// </summary>
    public class EditEntitiesViewModel : ViewModelBase
    {
        private readonly List<EntityViewModel> _entities;

        // a change of this view's own being given to the entities (their updates are not to refresh it as they come)
        private bool _applying;

        public EditEntitiesViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService, IEnumerable<EntityViewModel> entities)
            : base(eventAggregator, mapService, terrainService)
        {
            _entities = entities.OrderBy(e => e.Id).ToList();
        }

        public int Count => _entities.Count;

        public string Title => $"{Count} Entities Selected";

        /// <summary>The ids of the entities, to show which they are.</summary>
        public string Ids => string.Join(", ", _entities.Select(e => e.Id));

        private EntityViewModel First => _entities[0];

        /// <summary>All of the entities are of the same type (then their model can be set too).</summary>
        public bool SameType => _entities.All(e => e.Type == First.Type);

        /// <summary>All of the entities are of the same type and model: the help on each field is for them.</summary>
        public bool SameModel => SameType && _entities.All(e => e.Model == First.Model);

        /// <summary>The type of them all (its place in TypeIds), -1 if they are not all the same.</summary>
        public int TypeIdx
        {
            get => SameType ? TypeIds.ToList().FindIndex(t => t.Key == First.Type) : -1;
            set
            {
                if (value < 0 || value >= TypeIds.Count || (SameType && TypeIds[value].Key == First.Type))
                    return;
                int type = TypeIds[value].Key;
                Apply(e => e.Type = type);
            }
        }

        /// <summary>The models of their type, when they are all of the one type.</summary>
        public IList<KeyValuePair<int, string>> ModelTypes =>
            SameType ? First.ModelTypes.ToList() : new List<KeyValuePair<int, string>>();

        /// <summary>The model of them all (its place in ModelTypes), -1 if they are not all the same.</summary>
        public int ModelIdx
        {
            get => SameModel ? ModelTypes.ToList().FindIndex(m => m.Key == First.Model) : -1;
            set
            {
                var models = ModelTypes;
                if (!SameType || value < 0 || value >= models.Count || (SameModel && models[value].Key == First.Model))
                    return;
                int model = models[value].Key;
                Apply(e => e.Model = model);
            }
        }

        /// <summary>The position of the first of them; changed, they all move by as much.</summary>
        public string X
        {
            get => First.X.ToString();
            set
            {
                if (!byte.TryParse(value, out byte x) || x == First.X)
                    return;
                int dx = x - First.X;
                Apply(e => e.X = MapEditorViewModel.WrapTile(e.X + dx));
            }
        }

        /// <summary>The position of the first of them; changed, they all move by as much.</summary>
        public string Y
        {
            get => First.Y.ToString();
            set
            {
                if (!byte.TryParse(value, out byte y) || y == First.Y)
                    return;
                int dy = y - First.Y;
                Apply(e => e.Y = MapEditorViewModel.WrapTile(e.Y + dy));
            }
        }

        public string DisId { get => Shared(e => e.DisId); set => SetAll(value, (e, v) => e.DisId = v); }
        public string SwitchSize { get => Shared(e => e.SwitchSize); set => SetAll(value, (e, v) => e.SwitchSize = v); }
        public string SwitchId { get => Shared(e => e.SwitchId); set => SetAll(value, (e, v) => e.SwitchId = v); }
        public string Parent { get => Shared(e => e.Parent); set => SetAll(value, (e, v) => e.Parent = v); }
        public string Child { get => Shared(e => e.Child); set => SetAll(value, (e, v) => e.Child = v); }
        public string Par3 { get => Shared(e => e.Par3); set => SetAll(value, (e, v) => e.Par3 = v); }

        /// <summary>Par3 is only in Magic Carpet 2 levels.</summary>
        public bool HasPar3 => First.HasPar3;

        private const string MixedTip = "The entities selected have different values: a value entered is given to them all";
        private const string MoveTip = "The position of the first entity (the lowest id): changed, all of them move by as much, the same distance apart, round the edges of the map";

        // the help on a field: for their type and model when they all have the same, with a note where the values differ
        private string Tip(Func<EntityViewModel, string> tip, Func<EntityViewModel, ushort>? value = null)
        {
            string text = SameModel ? tip(First) : "The entities selected are of different types or models";
            if (value is not null && !_entities.All(e => value(e) == value(First)))
                text = $"{text}\n\n{MixedTip}";
            return text;
        }

        public string XTip => $"{(SameModel ? First.XTip + "\n\n" : string.Empty)}{MoveTip}";
        public string YTip => $"{(SameModel ? First.YTip + "\n\n" : string.Empty)}{MoveTip}";
        public string DisIdTip => Tip(e => e.DisIdTip, e => e.DisId);
        public string SwitchSizeTip => Tip(e => e.SwitchSizeTip, e => e.SwitchSize);
        public string SwitchIdTip => Tip(e => e.SwitchIdTip, e => e.SwitchId);
        public string ParentTip => Tip(e => e.ParentTip, e => e.Parent);
        public string ChildTip => Tip(e => e.ChildTip, e => e.Child);
        public string Par3Tip => Tip(e => e.Par3Tip, e => e.Par3);

        // a field's value when they all have the same, else empty
        private string Shared(Func<EntityViewModel, ushort> value)
        {
            ushort first = value(First);
            return _entities.All(e => value(e) == first) ? first.ToString() : string.Empty;
        }

        // a value entered for a field given to them all (unless it is not a number, or they all have it already)
        private void SetAll(string text, Action<EntityViewModel, ushort> set)
        {
            if (!ushort.TryParse(text, out ushort value))
                return;
            Apply(e => set(e, value));
        }

        /// <summary>A change given to each of the entities and saved, the terrain built once after them all.</summary>
        private void Apply(Action<EntityViewModel> change)
        {
            _applying = true;
            try
            {
                foreach (var entity in _entities)
                {
                    change(entity);
                    UpdateEntity(entity, deferTerrain: true);
                }
                _mapService.CommitTerrain();
            }
            finally
            {
                _applying = false;
            }
            RaiseAll();
        }

        /// <summary>One of the entities changed elsewhere (moved on the map): the one here replaced by it.</summary>
        public void OnEntityUpdated(EntityViewModel entity)
        {
            if (_applying)
                return;
            int index = _entities.FindIndex(e => e.Id == entity.Id);
            if (index < 0)
                return;
            _entities[index] = entity;
            RaiseAll();
        }

        private void RaiseAll()
        {
            foreach (var name in new[] { nameof(TypeIdx), nameof(SameType), nameof(SameModel), nameof(ModelTypes), nameof(ModelIdx),
                nameof(X), nameof(Y), nameof(DisId), nameof(SwitchSize), nameof(SwitchId), nameof(Parent), nameof(Child), nameof(Par3),
                nameof(XTip), nameof(YTip), nameof(DisIdTip), nameof(SwitchSizeTip), nameof(SwitchIdTip), nameof(ParentTip),
                nameof(ChildTip), nameof(Par3Tip) })
                this.RaisePropertyChanged(name);
        }
    }
}
