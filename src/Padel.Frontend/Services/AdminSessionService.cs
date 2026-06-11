using Padel.Frontend.Models;

namespace Padel.Frontend.Services;

public class AdminSessionService
{
    public AdministratorDto? CurrentAdmin { get; private set; }
    public bool IsLoggedIn => CurrentAdmin != null;
    public bool IsGlobalAdmin => CurrentAdmin?.Type == "Global";
    public bool IsSiteAdmin => CurrentAdmin?.Type == "Site";
    public int? SiteId => CurrentAdmin?.SiteId;

    public event Action? OnChange;

    public void Login(AdministratorDto admin)
    {
        CurrentAdmin = admin;
        OnChange?.Invoke();
    }

    public void Logout()
    {
        CurrentAdmin = null;
        OnChange?.Invoke();
    }
}
