using System.Windows;
using System.Windows.Controls;


namespace ShutDownVM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly MainWindowViewModel ViewModel;
        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new MainWindowViewModel();
            base.DataContext = ViewModel;
        }

        private void OutputTextChanged(object sender, TextChangedEventArgs e)
        {
            Output.ScrollToEnd();
        }
    }
}
