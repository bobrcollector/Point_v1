using System.Windows.Input;

namespace Point_v1.Controls;

public partial class AutoCompleteEntry : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(AutoCompleteEntry), default(string), BindingMode.TwoWay);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(AutoCompleteEntry), string.Empty);

    public static readonly BindableProperty SuggestionsProperty =
        BindableProperty.Create(nameof(Suggestions), typeof(List<string>), typeof(AutoCompleteEntry), new List<string>());

    public static readonly BindableProperty SuggestionSelectedCommandProperty =
        BindableProperty.Create(nameof(SuggestionSelectedCommand), typeof(ICommand), typeof(AutoCompleteEntry));

    public AutoCompleteEntry()
    {
        InitializeComponent();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public List<string> Suggestions
    {
        get => (List<string>)GetValue(SuggestionsProperty);
        set => SetValue(SuggestionsProperty, value);
    }

    public ICommand SuggestionSelectedCommand
    {
        get => (ICommand)GetValue(SuggestionSelectedCommandProperty);
        set => SetValue(SuggestionSelectedCommandProperty, value);
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue) || e.NewTextValue.Length < 3)
        {
            SuggestionsList.IsVisible = false;
            return;
        }
        if (Suggestions?.Count > 0)
        {
            SuggestionsList.IsVisible = true;
            SuggestionsList.ItemsSource = Suggestions;
        }
        else
        {
            SuggestionsList.IsVisible = false;
        }
    }

    private void OnSuggestionSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string selectedSuggestion)
        {
            Text = selectedSuggestion.Split(" - ")[0];
            SuggestionsList.IsVisible = false;

            SuggestionSelectedCommand?.Execute(selectedSuggestion);
        }
    }

    private void OnEntryCompleted(object sender, EventArgs e)
    {
        SuggestionsList.IsVisible = false;
    }
}
