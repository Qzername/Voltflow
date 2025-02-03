using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
namespace VoltflowAPI.Contexts;

public class ApplicationContext : IdentityDbContext<Account>
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }
}
