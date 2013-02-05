using System.Linq;
using System.Threading;
using System.Windows;
using GypiAutoUpdater.Model;

namespace GypiAutoUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
        }

        private MainViewModel ViewModel
        {
            get { return (MainViewModel) DataContext; }
            set { DataContext = value; }
        }

        private void UIElement_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data is DataObject && ((DataObject)e.Data).ContainsFileDropList())
            {
                var filePaths = ((DataObject) e.Data).GetFileDropList().Cast<string>().ToList();
                var viewModel = ViewModel;
                ThreadPool.QueueUserWorkItem(x => viewModel.Drop(filePaths));
            }
        }
    }
}
