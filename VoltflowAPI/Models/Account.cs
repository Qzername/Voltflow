using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Account : IdentityUser
{
	[Key]
	[Column("id")]
	public uint Id { get; set; }
	[Column("name")]
	public string Name { get; set; }
	[Column("surname")]
	public string Surname { get; set; }
	[Column("email")]
	public string Email { get; set; }
	[Column("phone")]
	public string Phone { get; set; }
}