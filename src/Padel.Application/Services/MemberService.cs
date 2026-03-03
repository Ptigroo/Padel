using Padel.Application.DTOs;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Application.Services;

public class MemberService(IMemberRepository memberRepository, ISiteRepository siteRepository) : IMemberService
{
    public async Task<IEnumerable<MemberDto>> GetAllAsync()
    {
        var members = await memberRepository.GetAllAsync();
        return members.Select(MapToDto);
    }

    public async Task<IEnumerable<MemberDto>> GetBySiteIdAsync(int siteId)
    {
        var members = await memberRepository.GetBySiteIdAsync(siteId);
        return members.Select(MapToDto);
    }

    public async Task<MemberDto?> GetByMatriculeAsync(string matricule)
    {
        var member = await memberRepository.GetByMatriculeAsync(matricule);
        return member is null ? null : MapToDto(member);
    }

    public async Task<MemberDto> CreateAsync(CreateMemberDto dto)
    {
        if (!Enum.TryParse<MemberType>(dto.MemberType, true, out var memberType))
            throw new InvalidOperationException($"Invalid member type: {dto.MemberType}. Must be Global, Site, or Libre.");

        if (memberType == MemberType.Site && dto.SiteId is null)
            throw new InvalidOperationException("A Site member must have a SiteId.");

        if (memberType != MemberType.Site && dto.SiteId is not null)
            throw new InvalidOperationException("Only Site members can have a SiteId.");

        if (dto.SiteId.HasValue)
        {
            _ = await siteRepository.GetByIdAsync(dto.SiteId.Value)
                ?? throw new InvalidOperationException($"Site with id {dto.SiteId.Value} not found.");
        }

        var matricule = await GenerateMatriculeAsync(memberType);

        var member = new Member
        {
            Matricule = matricule,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            MemberType = memberType,
            SiteId = dto.SiteId,
            ReservationBlocked = false
        };

        var created = await memberRepository.CreateAsync(member);
        return MapToDto(created);
    }

    public async Task<MemberDto?> UpdateAsync(string matricule, UpdateMemberDto dto)
    {
        var member = await memberRepository.GetByMatriculeAsync(matricule);
        if (member is null)
            return null;

        member.FirstName = dto.FirstName;
        member.LastName = dto.LastName;
        member.Email = dto.Email;

        var updated = await memberRepository.UpdateAsync(member);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(string matricule)
    {
        var member = await memberRepository.GetByMatriculeAsync(matricule);
        if (member is null)
            return false;

        await memberRepository.DeleteAsync(member);
        return true;
    }

    private async Task<string> GenerateMatriculeAsync(MemberType memberType)
    {
        var (prefix, digits) = memberType switch
        {
            MemberType.Global => ("G", 4),
            MemberType.Site => ("S", 5),
            MemberType.Libre => ("L", 5),
            _ => throw new InvalidOperationException($"Unknown member type: {memberType}")
        };

        var lastMatricule = await memberRepository.GetLastMatriculeByTypeAsync(memberType);

        int nextNumber = 1;
        if (lastMatricule is not null)
        {
            var numericPart = lastMatricule[prefix.Length..];
            if (int.TryParse(numericPart, out var lastNumber))
                nextNumber = lastNumber + 1;
        }

        return $"{prefix}{nextNumber.ToString().PadLeft(digits, '0')}";
    }

    private static MemberDto MapToDto(Member member) => new()
    {
        Id = member.Id,
        Matricule = member.Matricule,
        FirstName = member.FirstName,
        LastName = member.LastName,
        Email = member.Email,
        MemberType = member.MemberType.ToString(),
        SiteId = member.SiteId,
        SiteName = member.Site?.Name,
        ReservationBlocked = member.ReservationBlocked,
        BlockedUntil = member.BlockedUntil
    };
}
