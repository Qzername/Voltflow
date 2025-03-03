using Avalonia;
using Avalonia.Controls.Notifications;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Ursa.Controls;
using Voltflow.Models;
using Voltflow.Services;

namespace Voltflow.ViewModels.Pages.Charging;

public class TransactionViewModel : ViewModelBase
{
    public TransactionViewModel(ChargingStation station, double cost, double energyConsumed, bool isWon, IScreen screen) : base(screen)
    {
        _httpClient = GetService<HttpClient>();
        _dialogService = GetService<DialogService>();

        Cost = cost;
        EnergyConsumed = energyConsumed;
        IsWon = isWon;
    }

    private readonly HttpClient _httpClient;
    private readonly DialogService _dialogService;

    public WindowToastManager? ToastManager;
    public Visual? Parent;

    [Reactive] public double Cost { get; set; }
    [Reactive] public double EnergyConsumed { get; set; }
    [Reactive] public bool IsWon { get; set; }

    public bool IsDone
    {
        set
        {
            Text = IsWon ? "You won! -10%" : "Better luck next time...";

            if (IsWon)
                Cost = Math.Round(Cost * 0.9, 2);

            ShowText = true;
        }
    }

    [Reactive] public string? Text { get; set; }
    [Reactive] public bool ShowText { get; set; }

    public async Task GenerateInvoice()
    {
        // Get pdf from server
        var request = await _httpClient.GetAsync("api/Transactions/invoice");
        var fileStream = await request.Content.ReadAsStreamAsync();

        // Save pdf
        var results = await _dialogService.SaveFileDialog(Parent, fileStream, DialogService.PdfOptions);

        ToastManager?.Show(
            new Toast(results ? "Successfully saved the file!" : "Couldn't save the file!"),
            showIcon: true,
            showClose: false,
            type: results ? NotificationType.Success : NotificationType.Error,
            classes: ["Light"]);
    }
}