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
                pazMap.PointerReleased += OnPazMap_PointerReleased;
                pazMap.PointerMoved += OnPazMap_PointerMoved;
            }

            if (btnPanLeft != null)
            {
                btnPanLeft.Click += (sender, args) =>
                {
                    pazMap?.PanDelta(10, 0);
                };
            }

            if (btnPanRight != null)
            {
                btnPanRight.Click += (sender, args) =>
                {
                    pazMap?.PanDelta(-10, 0);
                };
            }

            if (btnPanUp != null)
            {
                btnPanUp.Click += (sender, args) =>
                {
                    pazMap?.PanDelta(0, 10);
                };
            }

            if (btnPanDown != null)
            {
                btnPanDown.Click += (sender, args) =>
                {
                    pazMap?.PanDelta(0, -10);
                };
            }

            if (btnZoomIn != null)
            {
                //btnZoomIn.Click += OnBtnZoomIn_Click;
            }

            if (btnZoomOut != null)
            {
                //btnZoomOut.Click += OnBtnZoomOut_Click;
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

        //private void OnBtnZoomIn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        //{
        //    if (pazMap is not null)
        //    {
        //        var center = MatrixHelper.ScaleAndTranslate(pazMap.ZoomX, pazMap.ZoomY, pazMap.OffsetX, pazMap.OffsetY).Transform(new Point(128 * 8, 128 * 8));
        //        var x = pazMap.Bounds.Width / 2.0;
        //        var y = pazMap.Bounds.Height / 2.0;
        //        var zoom = pazMap.ZoomX * pazMap.ZoomSpeed;

        //        if (zoom < 4.0)
        //            pazMap.ZoomTo(pazMap.ZoomSpeed, center.X, center.Y);
        //        else
        //            pazMap.Zoom(4.0, center.X, center.Y);
        //    }
        //}

        //private void OnBtnZoomOut_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        //{
        //    if (pazMap is not null)
        //    {
        //        var x = pazMap.Bounds.Width / 2.0;
        //        var y = pazMap.Bounds.Height / 2.0;
        //        var zoom = pazMap.ZoomX * (1 / pazMap.ZoomSpeed);

        //        if (zoom > 0.25)
        //            pazMap.ZoomTo(1 / pazMap.ZoomSpeed, x, y);
        //        else
        //            ResetView();
        //    }
        //}

        private void OnBtnReset_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ResetView();
        }

        private void OnPazMap_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
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
                VmMapEditor.CursorPosition = _ptCursor;
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
