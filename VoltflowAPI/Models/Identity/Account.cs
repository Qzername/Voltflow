using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

public class Account : IdentityUser
{
    [Column("Name")]
    public string Name { get; set; }
    [Column("Surname")]
    public string Surname { get; set; }
}