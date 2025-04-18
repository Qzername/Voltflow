﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
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
public class CarsController : ControllerBase
{
    readonly UserManager<Account> _userManager;
    readonly ApplicationContext _applicationContext;

    public CarsController(UserManager<Account> userManager, ApplicationContext applicationContext)
    {
        _userManager = userManager;
        _applicationContext = applicationContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetCars()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return BadRequest();

        var cars = await _applicationContext.Cars.Where(x => x.AccountId == user.Id).ToArrayAsync();

        return Ok(cars);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCar([FromBody] CreateCarModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return BadRequest();

        //meet data criteria    
        if (model.Name.Length > 100 ||
            model.BatteryCapacity < 1 ||
            model.ChargingRate < 1)
            return BadRequest(new { InvalidData = true });

        Car car = new Car()
        {
            AccountId = user.Id,
            Name = model.Name,
        };

        if (model.BatteryCapacity is not null)
            car.BatteryCapacity = model.BatteryCapacity.Value;

        if (model.ChargingRate is not null)
            car.ChargingRate = model.ChargingRate.Value;

        await _applicationContext.AddAsync(car);
        await _applicationContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPatch]
    public async Task<IActionResult> PatchCar([FromBody] PatchCarModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return BadRequest();

        //meet data criteria    
        if (model.Name?.Length > 100 ||
            (model.BatteryCapacity is not null && model.BatteryCapacity < 1) ||
            (model.ChargingRate is not null && model.ChargingRate < 1))
            return BadRequest(new { InvalidData = true });

        var car = _applicationContext.Cars.Single(x => x.Id == model.Id);

        if (car is null)
            return BadRequest(new { CarExist = false });

        if (!string.IsNullOrEmpty(model.Name) && model.Name.Length < 100)
            car.Name = model.Name;

        if (model.BatteryCapacity is not null && model.BatteryCapacity > 0)
            car.BatteryCapacity = model.BatteryCapacity.Value;

        if (model.ChargingRate is not null && model.ChargingRate > 0)
            car.ChargingRate = model.ChargingRate.Value;

        _applicationContext.Update(car);
        await _applicationContext.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCar(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return BadRequest();

        var car = _applicationContext.Cars.Single(x => x.Id == id);

        if (car is null)
            return BadRequest(new { CarExist = false });

        _applicationContext.Cars.Remove(car);
        await _applicationContext.SaveChangesAsync();

        return Ok();
    }

    public struct CreateCarModel
    {
        public string Name { get; set; }
        public int? BatteryCapacity { get; set; }
        public int? ChargingRate { get; set; }
    }

    public struct PatchCarModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? BatteryCapacity { get; set; }
        public int? ChargingRate { get; set; }
    }

    public struct DeleteCarModel
    {
        public int Id { get; set; }
    }
}
