using GenotypeApplication.MVVM.TreeView;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GenotypeApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            App.GlobalCts.Cancel();
            base.OnClosing(e);
        }

        private void OpenInExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem &&
                menuItem.DataContext is FileNodeViewModel node)
            {
                var path = node.IsDirectory
                    ? node.FullPath
                    : System.IO.Path.GetDirectoryName(node.FullPath);

                if (Directory.Exists(path))
                {
                    Process.Start("explorer.exe", path);
                }
            }
        }
    }
}