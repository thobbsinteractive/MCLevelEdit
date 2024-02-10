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
        private readonly ZoomBorder? _pazMap;
        private readonly Button? _btnReset;
        private readonly Image? _imgPreview;
        private readonly Canvas? _cvEntities;

        private Point _ptCursor = new Point();

        public Point PtCursor
        {
            get { return _ptCursor; }
        }

        public MapEditorViewModel? VmMapEditor { get { return (MapEditorViewModel?)this.DataContext; } }

        public MapEditorView()
        {
            InitializeComponent();

            _cvEntities = this.Find<Canvas>("cvEntities");
            _pazMap = this.Find<ZoomBorder>("pazMap");
            _btnReset = this.Find<Button>("btnReset");
            _imgPreview = this.Find<Image>("imgPreview");

            if (_pazMap != null)
            {
                _pazMap.PointerReleased += OnPazMap_PointerReleased;
                _pazMap.PointerMoved += OnPazMap_PointerMoved;
            }

            if (_btnReset != null)
            {
                _btnReset.Click += OnBtnReset_Click;
            }

            this.DataContextChanged += OnDataContextChanged;
            ResetView();
        }

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            if (VmMapEditor is not null && VmMapEditor.CvEntity is null)
            {
                VmMapEditor.CvEntity = _cvEntities;
            }
        }

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
            if (_imgPreview != null)
                return e.GetPosition(_imgPreview);
            else
                return new Point();
        }

        private void ResetView()
        {
            _pazMap.ResetMatrix();
            _pazMap?.AutoFit();
        }
    }
}
