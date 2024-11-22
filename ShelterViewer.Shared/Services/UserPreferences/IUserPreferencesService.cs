namespace ShelterViewer.Shared.Services.UserPreferences;

public interface IUserPreferencesService
{
    public Task SaveUserPreferences(UserPreferences userPreferences);

    public Task<UserPreferences?> LoadUserPreferences();
}
