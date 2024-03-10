using Avalonia;
using Avalonia.Controls;
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

        private void OnPazMap_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var point = e.GetCurrentPoint(sender as Control);

            if (point.Properties.IsLeftButtonPressed)
            {
                _ptCursorDragStart = new Point(_ptCursor.X, _ptCursor.Y);
            }
        }

        private void OnPazMap_PointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
        {
            _ptCursorDragStart = null;
        }

        private void OnPazMap_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                _ptCursorDragStart = null;
                _ptCursor = GetCursorPoint(e);
                if (VmMapEditor != null)
                {
                    VmMapEditor.CursorPosition = _ptCursor;
                    VmMapEditor.OnCursorClicked(VmMapEditor.CursorPosition, true, false);
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
                    VmMapEditor.OnCursorDragged((Point)_ptCursorDragStart, _ptCursor);
            }
        }

        private Point GetCursorPoint(PointerEventArgs e)
        {
            if (imgPreview != null)
                return e.GetPosition(imgPreview);
            else
                return new Point();
        }

        private void ResetView()
        {
            pazMap?.ResetMatrix();
            pazMap?.AutoFit();
        }
    }
}
