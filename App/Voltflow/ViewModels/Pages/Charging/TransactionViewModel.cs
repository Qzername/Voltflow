using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Voltflow.Models;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Platform.Storage;
using System.Net.Http;

namespace Voltflow.ViewModels.Pages.Charging;

public class TransactionViewModel(ChargingStation station, double cost, double energyUsed, bool isWon, IScreen screen) : ViewModelBase(screen)
{
    public Visual? Parent;
    [Reactive] public double Cost { get; set; } = cost;
	[Reactive] public double EnergyConsumed { get; set; } = energyUsed;
	[Reactive] public bool IsWon { get; set; } = isWon;

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

	public async void GenerateInvoice()
    {
        var topLevel = TopLevel.GetTopLevel(Parent);
        if (topLevel == null)
            return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new()
        {
            Title = "Save File",
            SuggestedFileName = "output",
            DefaultExtension = ".pdf",
            FileTypeChoices =
            [
                new FilePickerFileType("PDF Files")
                {
                    Patterns = ["*.pdf"],
                    MimeTypes = ["text/pdf"]
                },
                new FilePickerFileType("All Files")
                {
                    Patterns = ["*.*"]
                }
            ]
        });

        if (file == null)
            return;

        var stream = await file.OpenWriteAsync();

        var client = GetService<HttpClient>();

        //get pdf from server
        var response = await client.GetAsync("api/Transactions/invoice");
        var fileStream = await response.Content.ReadAsStreamAsync();
        fileStream.CopyTo(stream);

        stream.Close();
    }
}