using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using VoltflowAPI.Contexts;
using VoltflowAPI.Models.Application;

namespace VoltflowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TransactionsController : Controller
{
    ApplicationContext _applicationContext;
    UserManager<Account> _userManager;

    public TransactionsController(ApplicationContext applicationContext, UserManager<Account> userManager)
    {
        _applicationContext = applicationContext;
        _userManager = userManager;
    }

    /// <summary>
    /// Get all client's only transactions
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTransactions([FromQuery] DateTime? since)
    {
        var user = await _userManager.GetUserAsync(User);

        List<Transaction> allClientTransactions;

        if (since is null)
            allClientTransactions = await _applicationContext.Transactions.Where(x => x.AccountId == user.Id).ToListAsync();
        else
            allClientTransactions = await _applicationContext.Transactions.Where(x => x.AccountId == user.Id && x.StartDate > since).ToListAsync();

        return Ok(allClientTransactions);
    }

    /// <summary>
    /// Get all transactions from all clients
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllTransactions([FromQuery] DateTime? since)
    {
        List<Transaction> allTransactions;

        if (since is null)
            allTransactions = await _applicationContext.Transactions.ToListAsync();
        else
            allTransactions = await _applicationContext.Transactions.Where(x => x.StartDate > since).ToListAsync();

        return Ok(allTransactions);
    }

    [HttpGet("invoice")]
    public async Task<IActionResult> GenerateInvoice()
    {
        var user = await _userManager.GetUserAsync(User);
        var transactions = await _applicationContext.Transactions.Where(x => x.AccountId == user.Id).ToListAsync();
        var latestTransaction = transactions.OrderByDescending(x => x.EndDate).FirstOrDefault();

        var station = await _applicationContext.ChargingStations.FindAsync(latestTransaction.ChargingStationId);

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
        gfx.DrawString($"{latestTransaction.EnergyConsumed:F2} kWh", fontText, XBrushes.Black, new XPoint(startX + col1, startY));
        gfx.DrawString($"{station.Cost:F2} per kWh", fontText, XBrushes.Black, new XPoint(startX + col2, startY));
        gfx.DrawString($"{latestTransaction.Cost:F2}", fontText, XBrushes.Black, new XPoint(startX + col3, startY));

        startY += rowHeight + 10;

        // Summary
        gfx.DrawString($"Total Amount: {latestTransaction.Cost:F2}", fontTitle, XBrushes.Black, new XPoint(startX + col3 - 40, startY));

        MemoryStream stream = new MemoryStream();
        document.Save(stream);

        //return stream as pdf
        return File(stream, "application/pdf", "invoice.pdf");
    }
}