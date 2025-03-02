using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Diagnostics;
using System;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Voltflow.Models;
using Avalonia.Controls;
using Microsoft.Extensions.Options;
using Avalonia;
using Avalonia.Platform.Storage;

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

			if(IsWon)
                Cost *= 0.9;

            ShowText = true;
		}
	}

	[Reactive] public string? Text { get; set; }
	[Reactive] public bool ShowText { get; set; }

	public async void GenerateInvoice()
	{
        string outputPath = "ev_charging_invoice.pdf";

        // Create PDF document
        PdfDocument document = new PdfDocument();
        document.Info.Title = "EV Charging Invoice";

        // Add page
        PdfPage page = document.AddPage();
        XGraphics gfx = XGraphics.FromPdfPage(page);
        XFont fontTitle = new XFont("Arial", 16, XFontStyleEx.Bold);
        XFont fontText = new XFont("Arial", 12, XFontStyleEx.Regular);
        XFont fontTableHeader = new XFont("Arial", 10, XFontStyleEx.Bold);

        // Header
        gfx.DrawString("Electric Vehicle Charging Invoice", fontTitle, XBrushes.Black, new XPoint(40, 40));
        gfx.DrawString($"Charging Date: {DateTime.Now:yyyy-MM-dd HH:mm}", fontText, XBrushes.Black, new XPoint(40, 70));
        gfx.DrawString($"Invoice Number: EV-2024/001", fontText, XBrushes.Black, new XPoint(40, 90));
        gfx.DrawString($"Charging Station ID: {station.Id}", fontText, XBrushes.Black, new XPoint(40, 130));

        // Table
        int startX = 40;
        int startY = 180;
        int rowHeight = 25;
        int col1 = 200, col2 = 300, col3 = 380;

        gfx.DrawString("Description", fontTableHeader, XBrushes.Black, new XPoint(startX, startY));
        gfx.DrawString("Quantity", fontTableHeader, XBrushes.Black, new XPoint(startX + col1, startY));
        gfx.DrawString("Price", fontTableHeader, XBrushes.Black, new XPoint(startX + col2, startY));
        gfx.DrawString("Total", fontTableHeader, XBrushes.Black, new XPoint(startX + col3, startY));

        startY += rowHeight;

        // Invoice item: EV charging
        gfx.DrawString("Electric vehicle charging", fontText, XBrushes.Black, new XPoint(startX, startY));
        gfx.DrawString($"{energyUsed:F2} kWh", fontText, XBrushes.Black, new XPoint(startX + col1, startY));
        gfx.DrawString($"{station.Cost:F2} per kWh", fontText, XBrushes.Black, new XPoint(startX + col2, startY));
        gfx.DrawString($"{cost:F2}", fontText, XBrushes.Black, new XPoint(startX + col3, startY));

        startY += rowHeight + 10;

        // Summary
        gfx.DrawString($"Total Amount: {cost:F2}", fontTitle, XBrushes.Black, new XPoint(startX + col3 - 40, startY));

        // Save to file
        Console.WriteLine($"Invoice saved as: {outputPath}");

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

        await using var stream = await file.OpenWriteAsync();
        document.Save(stream);
        stream.Close();
    }
}