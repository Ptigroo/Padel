using Padel.Application.DTOs;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Application.Services;

public class StatsService(
    ISiteRepository siteRepository,
    IMatchRepository matchRepository,
    IPaymentRepository paymentRepository,
    IMemberRepository memberRepository) : IStatsService
{
    public async Task<GlobalStatsDto> GetGlobalStatsAsync()
    {
        var sites = await siteRepository.GetAllAsync();
        var members = await memberRepository.GetAllAsync();
        var matches = await matchRepository.GetAllAsync();
        var totalRevenue = await paymentRepository.GetTotalRevenueAsync();

        var matchList = matches.ToList();

        return new GlobalStatsDto
        {
            TotalSites = sites.Count(),
            TotalMembers = members.Count(),
            TotalMatches = matchList.Count,
            TotalRevenue = totalRevenue,
            MatchesScheduled = matchList.Count(m => m.Status == MatchStatus.Scheduled),
            MatchesFull = matchList.Count(m => m.Status == MatchStatus.Full),
            MatchesCompleted = matchList.Count(m => m.Status == MatchStatus.Completed),
            MatchesCancelled = matchList.Count(m => m.Status == MatchStatus.Cancelled)
        };
    }

    public async Task<SiteStatsDto?> GetSiteStatsAsync(int siteId)
    {
        var site = await siteRepository.GetByIdAsync(siteId);
        if (site is null) return null;

        var matches = (await matchRepository.GetBySiteAsync(siteId)).ToList();
        var members = await memberRepository.GetBySiteIdAsync(siteId);
        var revenue = await paymentRepository.GetRevenueByEsiteAsync(siteId);

        return new SiteStatsDto
        {
            SiteId = site.Id,
            SiteName = site.Name,
            TotalCourts = site.Courts.Count,
            TotalMembers = members.Count(),
            TotalMatches = matches.Count,
            Revenue = revenue,
            MatchesScheduled = matches.Count(m => m.Status == MatchStatus.Scheduled),
            MatchesFull = matches.Count(m => m.Status == MatchStatus.Full),
            MatchesCompleted = matches.Count(m => m.Status == MatchStatus.Completed),
            MatchesCancelled = matches.Count(m => m.Status == MatchStatus.Cancelled)
        };
    }

    public async Task<IEnumerable<SiteStatsDto>> GetAllSiteStatsAsync()
    {
        var sites = await siteRepository.GetAllAsync();
        var result = new List<SiteStatsDto>();

        foreach (var site in sites)
        {
            var stats = await GetSiteStatsAsync(site.Id);
            if (stats is not null)
                result.Add(stats);
        }

        return result;
    }
}
