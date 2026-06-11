namespace Padel.Domain.Entities;

public class Administrator
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public AdministratorType Type { get; set; }
    public int? SiteId { get; set; }
    public Site? Site { get; set; }
}
