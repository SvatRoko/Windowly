using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Windowly;
using Xunit;

public class SettingsWindowTests
{
    [Fact]
    public void LoadsNavigatesAndRefreshesRules()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            SettingsWindow? window = null;
            try
            {
                var settings = new Settings();
                window = new SettingsWindow(() => settings, _ => { }, () => { }, new FakeStartup());
                window.Show();
                window.UpdateLayout();
                var layouts = (Grid)window.FindName("LayoutsPage");
                var preview = (Border)window.FindName("PreviewPage");
                Assert.Equal(Visibility.Visible, layouts.Visibility);
                var nav = Descendants(window).OfType<RadioButton>().ToList();
                Assert.Equal(3, nav.Count);
                foreach (var item in nav.Where(n => (string)n.Tag != "Layouts"))
                {
                    item.IsChecked = true;
                    Assert.Equal(Visibility.Collapsed, layouts.Visibility);
                    Assert.Equal(Visibility.Visible, preview.Visibility);
                    Assert.Equal(item.Content, ((TextBlock)window.FindName("PageTitle")).Text);
                }
                settings.Rules.Add(new Rule(@"C:\Editor.exe", new(0, 0, 800, 600), "Display 1", new(0, 0, 1920, 1080), 96));
                window.RefreshRules();
                Assert.Single(((ListBox)window.FindName("RuleList")).Items.Cast<object>());
                Assert.Equal(Visibility.Collapsed, ((TextBlock)window.FindName("EmptyState")).Visibility);
                ((RadioButton)window.FindName("LayoutsNav")).IsChecked = true;
                Assert.Equal(Visibility.Visible, layouts.Visibility);
                Assert.Equal(Visibility.Collapsed, preview.Visibility);
            }
            catch (Exception ex) { failure = ex; }
            finally { window?.Close(); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(20)), "WPF smoke test timed out.");
        Assert.Null(failure);
    }

    static IEnumerable<DependencyObject> Descendants(DependencyObject parent)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }

    sealed class FakeStartup : IStartup
    {
        public bool Enabled { get; private set; }
        public void Set(bool enabled) => Enabled = enabled;
    }
}
