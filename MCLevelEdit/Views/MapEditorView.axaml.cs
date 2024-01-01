using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;

namespace MCLevelEdit.Views
{
    public partial class MapEditorView : UserControl
    {
        private readonly ZoomBorder? _pazMap;
        private readonly Button? _btnReset;

        public MapEditorView()
        {
            InitializeComponent();

            _pazMap = this.Find<ZoomBorder>("pazMap");
            _btnReset = this.Find<Button>("btnReset");       

            if (_pazMap != null)
            {
                _pazMap.PointerReleased += OnPazMap_PointerReleased;
            }

            if (_btnReset != null)
            {
                _btnReset.Click += OnBtnReset_Click;
            }
            ResetView();
        }

        private void OnBtnReset_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ResetView();
        }

        private void OnPazMap_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
            {

            }
        }

        private void ResetView()
        {
            _pazMap.ResetMatrix();
            _pazMap.Zoom(0.25, _pazMap.OffsetX, _pazMap.OffsetY, false);
            _pazMap?.AutoFit();
        }
    }
}
