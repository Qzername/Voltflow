/*
 * Before your first run:
 * You will have to modify appsettings in order to run the server
 * Check the appsettings.template.json for example
 */

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System.Text;

using VoltflowAPI.Contexts;
using VoltflowAPI.Models;
using VoltflowAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// --- Services ---
builder.Services.AddTransient<IEmailSender, DefaultEmailSender>();
builder.Services.AddTransient<IAccountTokenGenerator, DefaultAccountTokenGenerator>();

// --- Configuration ---
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// --- Contexts ---
//Requires ConnectionStrings -> Default to be set 
string connectionString = builder.Configuration.GetConnectionString("Default")!;

builder.Services.AddDbContext<IdentityContext>(opt => opt.UseNpgsql(connectionString));
builder.Services.AddDbContext<ApplicationContext>(opt => opt.UseNpgsql(connectionString));

// --- ASP.NET Identity configuration --- 
builder.Services.AddIdentity<Account, IdentityRole>()
	.AddEntityFrameworkStores<IdentityContext>()
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

//Ensure admin role is created
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    //we only need admin role
    //if user doesn't have a role, then he is a normal user
    if (!roleManager.Roles.Any())
        await roleManager.CreateAsync(new IdentityRole("Admin"));
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
