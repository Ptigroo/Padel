using Padel.Application.DTOs;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;
using MatchType = Padel.Domain.Entities.MatchType;

namespace Padel.Application.Services;

public class MatchService(
    IMatchRepository matchRepository,
    IMemberRepository memberRepository,
    ICourtRepository courtRepository,
    ISiteScheduleRepository scheduleRepository,
    IClosureDayRepository closureDayRepository,
    IPaymentRepository paymentRepository) : IMatchService
{
    private const decimal PricePerPlayer = 15m;
    private static readonly TimeSpan MatchDuration = TimeSpan.FromMinutes(90);

    public async Task<IEnumerable<MatchDto>> GetAllAsync()
    {
        var matches = await matchRepository.GetAllAsync();
        return matches.Select(MapToDto);
    }

    public async Task<MatchDto?> GetByIdAsync(int id)
    {
        var match = await matchRepository.GetByIdAsync(id);
        return match is null ? null : MapToDto(match);
    }

    public async Task<IEnumerable<MatchDto>> GetPublicAsync(int? siteId)
    {
        var matches = await matchRepository.GetPublicAsync(siteId);
        return matches.Select(MapToDto);
    }

    public async Task<IEnumerable<MatchDto>> GetByOrganizerAsync(string matricule)
    {
        var member = await memberRepository.GetByMatriculeAsync(matricule)
            ?? throw new InvalidOperationException($"Member with matricule {matricule} not found.");
        var matches = await matchRepository.GetByOrganizerAsync(member.Id);
        return matches.Select(MapToDto);
    }

    public async Task<IEnumerable<MatchDto>> GetByPlayerAsync(string matricule)
    {
        var member = await memberRepository.GetByMatriculeAsync(matricule)
            ?? throw new InvalidOperationException($"Member with matricule {matricule} not found.");
        var matches = await matchRepository.GetByPlayerAsync(member.Id);
        return matches.Select(MapToDto);
    }

    public async Task<IEnumerable<MatchDto>> GetBySiteAsync(int siteId)
    {
        var matches = await matchRepository.GetBySiteAsync(siteId);
        return matches.Select(MapToDto);
    }

    public async Task<MatchDto> CreateAsync(CreateMatchDto dto)
    {
        // Parse match type
        if (!Enum.TryParse<MatchType>(dto.MatchType, true, out var matchType))
            throw new InvalidOperationException($"Invalid match type: {dto.MatchType}. Must be Private or Public.");

        // Get organizer
        var organizer = await memberRepository.GetByMatriculeAsync(dto.OrganizerMatricule)
            ?? throw new InvalidOperationException($"Member with matricule {dto.OrganizerMatricule} not found.");

        // CF-RV-014: Check if organizer is blocked
        ValidateNotBlocked(organizer);

        // CF-RV-013: Check unpaid balance
        var balance = await paymentRepository.GetUnpaidBalanceAsync(organizer.Id);
        if (balance > 0)
            throw new InvalidOperationException("Organizer has an unpaid balance and cannot create a new match.");

        // Get court with site
        var court = await courtRepository.GetByIdAsync(dto.CourtId)
            ?? throw new InvalidOperationException($"Court with id {dto.CourtId} not found.");

        // CF-RV-015: Site member can only book on their own site
        ValidateSiteAccess(organizer, court);

        // CF-RV-016: Reservation window
        ValidateReservationWindow(organizer, dto.ScheduledAt);

        var matchDate = DateOnly.FromDateTime(dto.ScheduledAt);

        // CF-RV-017: Check closure days
        var isClosed = await closureDayRepository.ExistsForDateAndSiteAsync(matchDate, court.SiteId);
        if (isClosed)
            throw new InvalidOperationException("Cannot create a match on a closure day.");

        // CF-RV-018: Check site schedule
        var schedule = await scheduleRepository.GetForYearAsync(court.SiteId, dto.ScheduledAt.Year)
            ?? throw new InvalidOperationException($"No schedule defined for site {court.SiteId} and year {dto.ScheduledAt.Year}.");

        var matchStart = TimeOnly.FromDateTime(dto.ScheduledAt);
        var endsAt = dto.ScheduledAt.Add(MatchDuration);
        var matchEnd = TimeOnly.FromDateTime(endsAt);

        if (matchStart < schedule.StartTime || matchEnd > schedule.EndTime)
            throw new InvalidOperationException("Match time is outside the site's operating hours.");

        // CF-RV-019: Check for conflicts
        var hasConflict = await matchRepository.HasConflictAsync(dto.CourtId, dto.ScheduledAt, endsAt);
        if (hasConflict)
            throw new InvalidOperationException("A match is already scheduled on this court at this time.");

        // Create the match
        var match = new Match
        {
            CourtId = dto.CourtId,
            OrganizerId = organizer.Id,
            ScheduledAt = dto.ScheduledAt,
            EndsAt = endsAt,
            MatchType = matchType,
            Status = MatchStatus.Scheduled
        };

        var created = await matchRepository.CreateAsync(match);

        // CF-RC-003: Auto-register organizer as first player + create payment
        await AddPlayerToMatchAsync(created, organizer);

        // Reload with full includes
        var result = await matchRepository.GetByIdAsync(created.Id);
        return MapToDto(result!);
    }

    public async Task<MatchDto> AddPlayerAsync(int matchId, AddPlayerDto dto)
    {
        var match = await matchRepository.GetByIdAsync(matchId)
            ?? throw new InvalidOperationException($"Match with id {matchId} not found.");

        // CF-RV-022: Only organizer can add players to private match
        var organizer = await memberRepository.GetByMatriculeAsync(dto.OrganizerMatricule)
            ?? throw new InvalidOperationException($"Organizer with matricule {dto.OrganizerMatricule} not found.");

        if (match.OrganizerId != organizer.Id)
            throw new InvalidOperationException("Only the organizer can add players to this match.");

        if (match.MatchType != MatchType.Private)
            throw new InvalidOperationException("Players can only be added by the organizer to private matches.");

        // CF-RV-021: Match not full
        if (match.Status == MatchStatus.Full || match.Players.Count >= 4)
            throw new InvalidOperationException("Match is already full.");

        var player = await memberRepository.GetByMatriculeAsync(dto.Matricule)
            ?? throw new InvalidOperationException($"Member with matricule {dto.Matricule} not found.");

        // CF-RV-020: Not already in match
        if (match.Players.Any(p => p.MemberId == player.Id))
            throw new InvalidOperationException("This player is already registered for this match.");

        await AddPlayerToMatchAsync(match, player);

        var result = await matchRepository.GetByIdAsync(matchId);
        return MapToDto(result!);
    }

    public async Task<MatchDto> JoinAsync(int matchId, string matricule)
    {
        var match = await matchRepository.GetByIdAsync(matchId)
            ?? throw new InvalidOperationException($"Match with id {matchId} not found.");

        if (match.MatchType != MatchType.Public)
            throw new InvalidOperationException("Can only join public matches.");

        // CF-RV-021: Match not full
        if (match.Status == MatchStatus.Full || match.Players.Count >= 4)
            throw new InvalidOperationException("Match is already full.");

        var player = await memberRepository.GetByMatriculeAsync(matricule)
            ?? throw new InvalidOperationException($"Member with matricule {matricule} not found.");

        // CF-RV-009: Check blocked
        ValidateNotBlocked(player);

        // CF-RV-015: Site access
        ValidateSiteAccess(player, match.Court!);

        // CF-RV-016: Reservation window
        ValidateReservationWindow(player, match.ScheduledAt);

        // CF-RV-020: Not already in match
        if (match.Players.Any(p => p.MemberId == player.Id))
            throw new InvalidOperationException("This player is already registered for this match.");

        await AddPlayerToMatchAsync(match, player);

        var result = await matchRepository.GetByIdAsync(matchId);
        return MapToDto(result!);
    }

    public async Task ProcessDayBeforeAsync()
    {
        var tomorrow = DateTime.Today.AddDays(1);

        // Rule 1: Private matches with < 4 players become public + organizer penalty
        var incompleteMatches = await matchRepository.GetMatchesBecomingPublicAsync(tomorrow);
        foreach (var match in incompleteMatches)
        {
            match.MatchType = MatchType.Public;
            await matchRepository.UpdateAsync(match);

            if (match.Organizer is not null)
            {
                match.Organizer.ReservationBlocked = true;
                match.Organizer.BlockedUntil = DateTime.Now.AddDays(7);
                await memberRepository.UpdateAsync(match.Organizer);
            }
        }

        // Rule 2: Remove unpaid players
        var matchesWithUnpaid = await matchRepository.GetMatchesWithUnpaidPlayersAsync(tomorrow);
        foreach (var match in matchesWithUnpaid)
        {
            var unpaidPlayers = match.Players
                .Where(p => p.Payment is not null && p.Payment.Status == PaymentStatus.Pending)
                .ToList();

            foreach (var player in unpaidPlayers)
            {
                match.Players.Remove(player);
            }

            // CF-RC-007: Recalculate status
            if (match.Players.Count < 4)
            {
                match.Status = MatchStatus.Scheduled;
                match.MatchType = MatchType.Public;
            }

            await matchRepository.UpdateAsync(match);
        }
    }

    private async Task AddPlayerToMatchAsync(Match match, Member player)
    {
        var matchPlayer = new MatchPlayer
        {
            MatchId = match.Id,
            MemberId = player.Id,
            JoinedAt = DateTime.Now
        };

        match.Players.Add(matchPlayer);

        // CF-RC-005: Auto-transition to Full
        if (match.Players.Count >= 4)
            match.Status = MatchStatus.Full;

        await matchRepository.UpdateAsync(match);

        // Need to get the saved matchPlayer Id
        var savedPlayer = match.Players.First(p => p.MemberId == player.Id);

        // CF-RC-003: Create pending payment
        var payment = new Payment
        {
            MatchPlayerId = savedPlayer.Id,
            MatchId = match.Id,
            MemberId = player.Id,
            Amount = PricePerPlayer,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.Now
        };

        await paymentRepository.CreateAsync(payment);
    }

    private static void ValidateNotBlocked(Member member)
    {
        if (member.ReservationBlocked && member.BlockedUntil > DateTime.Now)
            throw new InvalidOperationException($"Member {member.Matricule} is blocked until {member.BlockedUntil:dd/MM/yyyy}.");
    }

    private static void ValidateSiteAccess(Member member, Court court)
    {
        if (member.MemberType == MemberType.Site && member.SiteId != court.SiteId)
            throw new InvalidOperationException("Site members can only book courts on their own site.");
    }

    private static void ValidateReservationWindow(Member member, DateTime scheduledAt)
    {
        var daysBeforeMatch = (scheduledAt.Date - DateTime.Today).Days;
        var maxDays = member.MemberType switch
        {
            MemberType.Global => 21,
            MemberType.Site => 14,
            MemberType.Libre => 5,
            _ => 0
        };

        if (daysBeforeMatch > maxDays)
            throw new InvalidOperationException($"Members of type {member.MemberType} can only book up to {maxDays} days in advance.");

        if (daysBeforeMatch < 0)
            throw new InvalidOperationException("Cannot create a match in the past.");
    }

    private static MatchDto MapToDto(Match match) => new()
    {
        Id = match.Id,
        CourtId = match.CourtId,
        CourtName = match.Court?.Name ?? "",
        SiteId = match.Court?.SiteId ?? 0,
        SiteName = match.Court?.Site?.Name ?? "",
        OrganizerId = match.OrganizerId,
        OrganizerMatricule = match.Organizer?.Matricule ?? "",
        OrganizerName = $"{match.Organizer?.FirstName} {match.Organizer?.LastName}".Trim(),
        ScheduledAt = match.ScheduledAt,
        EndsAt = match.EndsAt,
        MatchType = match.MatchType.ToString(),
        Status = match.Status.ToString(),
        PlayerCount = match.Players.Count,
        Players = match.Players.Select(p => new MatchPlayerDto
        {
            Id = p.Id,
            MemberId = p.MemberId,
            Matricule = p.Member?.Matricule ?? "",
            FirstName = p.Member?.FirstName ?? "",
            LastName = p.Member?.LastName ?? "",
            JoinedAt = p.JoinedAt,
            PaymentStatus = p.Payment?.Status.ToString()
        }).ToList()
    };
}
