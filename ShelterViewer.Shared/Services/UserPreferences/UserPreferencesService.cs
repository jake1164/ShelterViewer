using Blazored.LocalStorage;

namespace ShelterViewer.Shared.Services.UserPreferences;

public class UserPreferencesService : IUserPreferencesService
{
    private readonly ILocalStorageService _localStorage;
    private const string key = "vaultPreferences";

    public UserPreferencesService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task SaveUserPreferences(UserPreferences userPreferences)
    {
        await _localStorage.SetItemAsync<UserPreferences>(key, userPreferences);
    }

    public async Task<UserPreferences?> LoadUserPreferences()
    {
        return await _localStorage.GetItemAsync<UserPreferences>(key);
    }
}
