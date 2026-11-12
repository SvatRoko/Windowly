using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Windowly;

public partial class SettingsWindow : Window
{
    readonly Func<Settings> readSettings;
    readonly Action<Rule> deleteRule;
    readonly Action capture;
    readonly IStartup startup;

    public SettingsWindow(Func<Settings> readSettings, Action<Rule> deleteRule, Action capture, IStartup startup)
    {
        this.readSettings = readSettings;
        this.deleteRule = deleteRule;
        this.capture = capture;
        this.startup = startup;
        InitializeComponent();
        RefreshRules();
        try { StartupToggle.IsChecked = startup.Enabled; }
        catch (Exception ex) { StartupToggle.IsEnabled = false; Status.Text = "Could not read sign-in setting: " + ex.Message; }
        LayoutsNav.IsChecked = true;
    }

    public void RefreshRules()
    {
        var selected = (RuleList.SelectedItem as RuleItem)?.Path;
        var items = readSettings().Rules.OrderBy(r => r.Path, StringComparer.OrdinalIgnoreCase)
            .Select(r => new RuleItem(r)).ToList();
        RuleList.ItemsSource = items;
        RuleList.SelectedItem = items.FirstOrDefault(r => r.Path == selected);
        RuleCount.Text = $"Saved layouts ({items.Count})";
        EmptyState.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    void Navigate(object sender, RoutedEventArgs e)
    {
        var section = (string)((RadioButton)sender).Tag;
        bool layouts = section == "Layouts";
        LayoutsPage.Visibility = layouts ? Visibility.Visible : Visibility.Collapsed;
        PreviewPage.Visibility = layouts ? Visibility.Collapsed : Visibility.Visible;
        PageTitle.Text = layouts ? "Window layouts" : section == "Taskbar" ? "Taskbar" : "Notification panel";
        PageDescription.Text = layouts ? "Keep your applications right where you want them." : "A little more control over your desktop. Coming in a future release.";
        PreviewTitle.Text = section == "Taskbar" ? "A taskbar that fits your style" : "Your notifications, your space";
        PreviewDescription.Text = section == "Taskbar"
            ? "Future customization inspired by TranslucentTB, including taskbar transparency and appearance."
            : "A dedicated space for future notification panel appearance options.";
    }

    void Remember(object sender, RoutedEventArgs e) => capture();

    void SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DeleteButton != null) DeleteButton.IsEnabled = RuleList.SelectedItem is RuleItem;
    }

    void Delete(object sender, RoutedEventArgs e)
    {
        if (RuleList.SelectedItem is not RuleItem item) return;
        try
        {
            deleteRule(item.Rule);
            RefreshRules();
            Status.Text = $"Deleted layout for {item.Name}.";
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Could not delete layout", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    void ToggleStartup(object sender, RoutedEventArgs e)
    {
        try
        {
            startup.Set(StartupToggle.IsChecked == true);
            StartupToggle.IsChecked = startup.Enabled;
            Status.Text = startup.Enabled ? "Windowly will start at sign-in." : "Sign-in startup is disabled.";
        }
        catch (Exception ex)
        {
            StartupToggle.IsChecked = !(StartupToggle.IsChecked == true);
            MessageBox.Show(this, ex.Message, "Startup setting failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    sealed record RuleItem(Rule Rule)
    {
        public string Path => Rule.Path;
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
        public string Geometry => $"{Rule.Bounds.Width} × {Rule.Bounds.Height} px · Position {Rule.Bounds.X}, {Rule.Bounds.Y} · {Rule.Monitor}";
    }
}
