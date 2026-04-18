using GenotypeApplication.Services.Application_configuration.Logger;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GenotypeApplication.MVVM.List_box
{
    public static class ListBoxAutoScrollBehavior
    {
        public static readonly DependencyProperty ScrollSourceProperty =
        DependencyProperty.RegisterAttached(
            "ScrollSource",
            typeof(LoggerService),
            typeof(ListBoxAutoScrollBehavior),
            new PropertyMetadata(null, OnScrollSourceChanged));

        public static LoggerService GetScrollSource(DependencyObject obj)
            => (LoggerService)obj.GetValue(ScrollSourceProperty);
        public static void SetScrollSource(DependencyObject obj, LoggerService value)
            => obj.SetValue(ScrollSourceProperty, value);

        private static void OnScrollSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListBox listBox) return;

            if (e.OldValue is LoggerService oldService)
                oldService.LogScrollRequested -= ScrollToEnd;

            if (e.NewValue is LoggerService newService)
            {
                void Handler()
                {
                    if (listBox.Items.Count == 0) return;

                    var scrollViewer = GetScrollViewer(listBox);
                    scrollViewer?.ScrollToEnd(); // только вертикальный скролл
                }

                newService.LogScrollRequested += Handler;

                // отписка при выгрузке
                listBox.Unloaded += (_, _) => newService.LogScrollRequested -= Handler;
            }
        }

        private static ScrollViewer? GetScrollViewer(DependencyObject obj)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is ScrollViewer sv) return sv;
                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }

        private static void ScrollToEnd() { } // заглушка для отписки старого

        //public static readonly DependencyProperty EnabledProperty =
        //    DependencyProperty.RegisterAttached(
        //        "Enabled",
        //        typeof(bool),
        //        typeof(ListBoxAutoScrollBehavior),
        //        new PropertyMetadata(false, OnEnabledChanged));

        //public static bool GetEnabled(DependencyObject obj) => (bool)obj.GetValue(EnabledProperty);
        //public static void SetEnabled(DependencyObject obj, bool value) => obj.SetValue(EnabledProperty, value);

        //private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (d is not ListBox listBox) return;

        //    if ((bool)e.NewValue)
        //    {
        //        // подписываемся когда ListBox загрузится и Items будут готовы
        //        listBox.Loaded += ListBox_Loaded;
        //    }
        //}

        //private static void ListBox_Loaded(object sender, RoutedEventArgs e)
        //{
        //    var listBox = (ListBox)sender;
        //    listBox.Loaded -= ListBox_Loaded;

        //    ((INotifyCollectionChanged)listBox.Items).CollectionChanged += (_, _) =>
        //    {
        //        if (listBox.Items.Count > 0)
        //            listBox.ScrollIntoView(listBox.Items[listBox.Items.Count - 1]);
        //    };
        //}
    }
}
