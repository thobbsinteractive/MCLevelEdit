using Avalonia.Controls;
using MCLevelEdit.ViewModels;

namespace MCLevelEdit.Views
{
    public partial class EntityToolBarView : UserControl
    {
        private EntityToolBarViewModel EntityViewModel
        { 
            get
            {
                return (EntityToolBarViewModel)DataContext;
            } 
        }

        public EntityToolBarView()
        {
            InitializeComponent();
        }
    }
}
