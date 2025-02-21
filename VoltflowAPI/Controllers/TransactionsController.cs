using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoltflowAPI.Contexts;

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
    public async Task<IActionResult> GetTransactions()
    {
        var user = await _userManager.GetUserAsync(User);

        var allClientTransactions = await _applicationContext.Transactions.Where(x=>x.AccountId == user.Id).ToListAsync();

        return Ok(allClientTransactions);
    }

    /// <summary>
    /// Get all transactions from all clients
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllTransactions()
    {
        var allTransactions = await _applicationContext.Transactions.ToListAsync();

        return Ok(allTransactions);
    }
}