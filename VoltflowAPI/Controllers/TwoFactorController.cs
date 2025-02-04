using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace VoltflowAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TwoFactorController : ControllerBase
{
	readonly UserManager<Account> _userManager;

	public TwoFactorController(UserManager<Account> userManager)
	{
		_userManager = userManager;
	}

	#region 2FA setting control
	[HttpPost]
	public async Task<IActionResult> Enable()
	{

	}

	[HttpPost]
	public async Task<IActionResult> Disable()
	{

	}
	#endregion

	#region 2FA Process 
	[HttpPost]
	public async Task<IActionResult> Send()
	{

	}

	[HttpPost]
	public async Task<IActionResult> Verify()
	{

	}
	#endregion
}