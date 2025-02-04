/*
 * Before your first run:
 * You will have to modify appsettings in order to run the server
 * 
 * Required elements are:
 * - ConnectionStrings -> Default
 * - Jwt -> Key
 * 
 * Check the appsettings.template.json for example
 */

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System.Text;

using VoltflowAPI.Contexts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// --- Contexts ---

// --- Dependency Injection ---

// --- PostgreSQL Server configuration ---
//Requires ConnectionStrings -> Default to be set 
string connectionString = builder.Configuration.GetConnectionString("Default")!;
builder.Services.AddDbContext<ApplicationContext>(opt => opt.UseNpgsql(connectionString));

// --- ASP.NET Identity configuration --- 
builder.Services.AddIdentity<Account, IdentityRole>()
	.AddEntityFrameworkStores<ApplicationContext>()
	.AddDefaultTokenProviders();

// --- Authentication and autorization --- 
//Requires Jwt -> Key to be set
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(o =>
	{
		o.TokenValidationParameters = new TokenValidationParameters
		{
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			//for now, we don't have domain
			//this will require configuration when server is bought
			ValidateAudience = false,
			ValidateIssuer = false,
		};
	});
builder.Services.AddAuthorization();

// --- App conifg ---
var app = builder.Build();

if (app.Environment.IsDevelopment())
	app.MapOpenApi();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
