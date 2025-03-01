using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        
        if(since is null)
            allClientTransactions = await _applicationContext.Transactions.Where(x=>x.AccountId == user.Id).ToListAsync();
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
            allTransactions = await _applicationContext.Transactions.Where(x=>x.StartDate > since).ToListAsync();

        return Ok(allTransactions);
    }
}