namespace Padel.Application.DTOs;

public class CreateMemberDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string MemberType { get; set; }
    public int? SiteId { get; set; }
}
