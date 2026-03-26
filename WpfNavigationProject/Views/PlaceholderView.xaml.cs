using System.Windows.Controls;

namespace WpfNavigationProject.Views
{
    /// <summary>
    /// Lógica de interacción para PlaceholderView.xaml
    /// </summary>
    public partial class PlaceholderView : UserControl
    {
        public string Title { get; set; }

        public PlaceholderView(string title)
        {
            InitializeComponent();
            this.Title = title;
            this.DataContext = this; // Establecer el DataContext para enlazar el Title
        }
    }
}