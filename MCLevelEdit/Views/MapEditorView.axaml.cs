using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;
using MCLevelEdit.ViewModels;
using System;

namespace MCLevelEdit.Views
{
    public partial class MapEditorView : UserControl
    {
        private Point _ptCursor = new Point();
        private Point? _ptCursorDragStart = null;

        public Point PtCursor
        {
            get { return _ptCursor; }
        }

        public MapEditorViewModel? VmMapEditor { get { return (MapEditorViewModel?)this.DataContext; } }

        public MapEditorView()
        {
            InitializeComponent();

            if (pazMap != null)
            {
                pazMap.PointerCaptureLost += OnPazMap_PointerCaptureLost;
                pazMap.PointerPressed += OnPazMap_PointerPressed;
                pazMap.PointerReleased += OnPazMap_PointerReleased;
                pazMap.PointerMoved += OnPazMap_PointerMoved;
                pazMap.LayoutUpdated += OnPazMap_LayoutUpdated;
            }

            // the viewport clicked: it and the map swap views
            if (bdrViewport != null)
            {
                bdrViewport.PointerPressed += (sender, e) =>
                {
                    if (VmMapEditor is null || !e.GetCurrentPoint(bdrViewport).Properties.IsLeftButtonPressed)
                        return;
                    VmMapEditor.ShowIsometric = !VmMapEditor.ShowIsometric;
                    e.Handled = true;
                };
            }

            if (btnPanLeft != null)
            {
                btnPanLeft.Click += (sender, args) =>
                {
                    pazMap?.PanDelta(25, 0);
                };
            }

            if (btnPanRight != null)
            {
                btnPanRight.Click += (sender, args) =>
                {
                    pazMap?.PanDelta(-25, 0);
                };
            }

            if (btnPanUp != null)
            {
                btnPanUp.Click += (sender, args) =>
                {
                    pazMap?.PanDelta(0, 25);
                };
            }

            if (btnPanDown != null)
            {
                btnPanDown.Click += (sender, args) =>
                {
                    pazMap?.PanDelta(0, -25);
                };
            }

            if (btnZoomIn != null)
            {
                btnZoomIn.Click += OnBtnZoomIn_Click;
            }

            if (btnZoomOut != null)
            {
                btnZoomOut.Click += OnBtnZoomOut_Click;
            }

            if (btnReset != null)
            {
                btnReset.Click += OnBtnReset_Click;
            }

            this.DataContextChanged += OnDataContextChanged;

            ResetView();
        }

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            if (VmMapEditor is not null && VmMapEditor.CvEntity is null)
            {
                VmMapEditor.CvEntity = cvEntities;
            }
        }

        private void OnBtnZoomIn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (pazMap is not null)
            {
                pazMap.ZoomIn();
            }
        }

        private void OnBtnZoomOut_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (pazMap is not null)
            {
                pazMap.ZoomOut();
            }
        }

        private void OnBtnReset_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ResetView();
        }

        // what the left drag does (decided when it is pressed), where it was pressed (on the canvas and in this view),
        // whether the selection box is out (the cursor went far enough) and whether the entities selected moved
        private MapEditorViewModel.DragMode _dragMode = MapEditorViewModel.DragMode.Default;
        private Point _ptPressCanvas;
        private Point _ptPressView;
        private bool _boxActive;
        private bool _selectionMoved;

        // how far (in pixels on the screen) the cursor goes before a press becomes a selection box, not a click
        private const double BoxStartDistance = 4;

        private void OnPazMap_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var point = e.GetCurrentPoint(sender as Control);

            if (point.Properties.IsLeftButtonPressed)
            {
                _ptCursorDragStart = new Point(_ptCursor.X, _ptCursor.Y);
                _ptPressCanvas = GetCursorPoint(e);
                _ptPressView = e.GetPosition(this);
                _boxActive = false;
                _selectionMoved = false;
                _dragMode = VmMapEditor?.BeginDrag(_ptPressCanvas) ?? MapEditorViewModel.DragMode.Default;
            }
        }

        private void OnPazMap_PointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
        {
            EndDrag();
        }

        // the terrain is built again once a dragged entity is dropped, not at each step of the drag
        private void EndDrag()
        {
            if (_ptCursorDragStart is null)
                return;
            _ptCursorDragStart = null;
            if (_boxActive)
                VmMapEditor?.CancelSelectionBox();
            _boxActive = false;
            _selectionMoved = false;
            _dragMode = MapEditorViewModel.DragMode.Default;
            VmMapEditor?.OnCursorDragEnded();
        }

        private void OnPazMap_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Left || e.InitialPressMouseButton == MouseButton.Right)
            {
                // a box let go selects what is in it, the entities selected dropped where they were dragged: not clicks
                bool boxReleased = _boxActive && e.InitialPressMouseButton == MouseButton.Left;
                bool selectionDropped = _selectionMoved && e.InitialPressMouseButton == MouseButton.Left;
                EndDrag();
                _ptCursor = GetCursorPoint(e);
                if (VmMapEditor != null)
                {
                    VmMapEditor.CursorPosition = _ptCursor;
                    if (boxReleased)
                    {
                        // Ctrl or Shift held: added to those selected already
                        bool add = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Shift);
                        VmMapEditor.EndSelectionBox(_ptPressCanvas, _ptCursor, add);
                    }
                    else if (!selectionDropped)
                    {
                        // Ctrl+click: the entity there added to the selection or taken out of it
                        if (e.InitialPressMouseButton == MouseButton.Left && e.KeyModifiers.HasFlag(KeyModifiers.Control)
                            && VmMapEditor.OnCursorCtrlClicked(_ptCursor))
                            return;

                        VmMapEditor.OnCursorClicked(VmMapEditor.CursorPosition, e.InitialPressMouseButton == MouseButton.Left, e.InitialPressMouseButton == MouseButton.Right);
                    }
                }
            }
        }

        private void OnPazMap_PointerMoved(object? sender, PointerEventArgs e)
        {
            _ptCursor = GetCursorPoint(e);
            if (VmMapEditor != null)
            {
                VmMapEditor.CursorPosition = _ptCursor;

                if (_ptCursorDragStart is not null)
                {
                    switch (_dragMode)
                    {
                        case MapEditorViewModel.DragMode.SelectionBox:
                            var ptView = e.GetPosition(this);
                            if (!_boxActive && Math.Abs(ptView.X - _ptPressView.X) + Math.Abs(ptView.Y - _ptPressView.Y) > BoxStartDistance)
                                _boxActive = true;
                            if (_boxActive)
                                VmMapEditor.UpdateSelectionBox(_ptPressCanvas, _ptCursor);
                            break;
                        case MapEditorViewModel.DragMode.MoveSelection:
                            if (VmMapEditor.OnSelectionDragged(_ptCursor))
                                _selectionMoved = true;
                            break;
                        default:
                            VmMapEditor.OnCursorDragged((Point)_ptCursorDragStart, _ptCursor);
                            break;
                    }
                }
            }
        }

        private Point GetCursorPoint(PointerEventArgs e)
        {
            if (imgPreview != null)
                return e.GetPosition(imgPreview);
            else
                return new Point();
        }

        // ZoomBorder fits its content (by its Stretch) every time it is laid out again (ArrangeOverride calls AutoFit):
        // a new preview image or an entity shape added to the canvas put the zoom and pan back each time. It fits once,
        // when first laid out and on Reset, with Stretch None the rest of the time.
        private StretchMode _fitStretch = StretchMode.Uniform;
        private bool _fitPending;

        private void ResetView()
        {
            if (pazMap is null)
                return;
            if (pazMap.Stretch != StretchMode.None)
                _fitStretch = pazMap.Stretch;
            pazMap.Stretch = _fitStretch;
            pazMap.ResetMatrix();
            pazMap.AutoFit();
            _fitPending = true;
        }

        // once laid out with the fit, the Stretch is None until the next Reset
        private void OnPazMap_LayoutUpdated(object? sender, EventArgs e)
        {
            if (!_fitPending || pazMap is null || pazMap.Bounds.Width <= 0 || pazMap.Bounds.Height <= 0)
                return;
            _fitPending = false;
            pazMap.Stretch = StretchMode.None;
        }
    }
}
