using Point_v1.ViewModels;
using Point_v1.Services;
using Point_v1.Models;

namespace Point_v1.Views;

public partial class CreateEventPage : ContentPage
{
    public CreateEventPage(CreateEventViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Обрабатываем данные, переданные обратно со страницы выбора местоположения
        if (BindingContext is CreateEventViewModel viewModel)
        {
            // Восстанавливаем состояние формы
            if (CreateEventStateService.HasState)
            {
                System.Diagnostics.Debug.WriteLine("💾 Восстанавливаем состояние формы из CreateEventStateService");
                CreateEventStateService.RestoreState(viewModel);
                System.Diagnostics.Debug.WriteLine("✅ Состояние формы восстановлено");
            }
            
            // Проверяем, есть ли сохраненные данные в LocationSelectionService
            if (LocationSelectionService.HasSelection)
            {
                System.Diagnostics.Debug.WriteLine($"📍 Восстанавливаем координаты из LocationSelectionService: lat={LocationSelectionService.SelectedLatitude}, lon={LocationSelectionService.SelectedLongitude}");
                
                viewModel.Latitude = LocationSelectionService.SelectedLatitude.Value;
                viewModel.Longitude = LocationSelectionService.SelectedLongitude.Value;
                
                if (!string.IsNullOrEmpty(LocationSelectionService.SelectedAddress))
                {
                    viewModel.Address = LocationSelectionService.SelectedAddress;
                }
                
                viewModel.SelectionStatus = $"✅ Местоположение выбрано: {viewModel.Address}";
                
                System.Diagnostics.Debug.WriteLine($"✅ Координаты восстановлены: Address={viewModel.Address}");
                
                // Очищаем сохраненные данные
                LocationSelectionService.Clear();
            }
        }
    }

    private void OnCategoryClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Interest interest)
        {
            if (BindingContext is CreateEventViewModel viewModel)
            {
                viewModel.ToggleCategoryCommand.Execute(interest);
            }
        }
    }
}