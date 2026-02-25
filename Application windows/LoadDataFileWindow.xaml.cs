using System.Windows;
using System.Windows.Media.Animation;

namespace GenotypeApplication.Application_windows
{
    /// <summary>
    /// Interaction logic for LoadDataFileWindow.xaml
    /// </summary>
    public partial class LoadDataFileWindow : Window
    {
        private const double _collapsedWidth = 412.5;
        private const double _expandedWidth = 1044.36;


        public LoadDataFileWindow()
        {
            InitializeComponent();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            AnimateWindowWidth(_expandedWidth);
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            AnimateWindowWidth(_collapsedWidth);
        }

        private void AnimateWindowWidth(double toWidth)
        {
            var animation = new DoubleAnimation
            {
                To = toWidth,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            this.BeginAnimation(WidthProperty, animation);
        }
    }
}
