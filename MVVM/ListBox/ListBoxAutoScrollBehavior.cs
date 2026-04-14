using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace GenotypeApplication.MVVM.List_box
{
    public static class ListBoxAutoScrollBehavior
    {
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached(
                "Enabled",
                typeof(bool),
                typeof(ListBoxAutoScrollBehavior),
                new PropertyMetadata(false, OnEnabledChanged));

        public static bool GetEnabled(DependencyObject obj) => (bool)obj.GetValue(EnabledProperty);
        public static void SetEnabled(DependencyObject obj, bool value) => obj.SetValue(EnabledProperty, value);

        private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListBox listBox) return;

            if ((bool)e.NewValue)
            {
                // подписываемся когда ListBox загрузится и Items будут готовы
                listBox.Loaded += ListBox_Loaded;
            }
        }

        private static void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var listBox = (ListBox)sender;
            listBox.Loaded -= ListBox_Loaded;

            ((INotifyCollectionChanged)listBox.Items).CollectionChanged += (_, _) =>
            {
                if (listBox.Items.Count > 0)
                    listBox.ScrollIntoView(listBox.Items[listBox.Items.Count - 1]);
            };
        }
    }
}
