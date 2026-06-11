namespace Padel.Frontend.Models;

public class AdministratorDto
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Type { get; set; }
    public int? SiteId { get; set; }
    public string? SiteName { get; set; }
}
