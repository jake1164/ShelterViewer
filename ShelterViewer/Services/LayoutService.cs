﻿using ShelterViewer.Services.UserPreferences;

namespace ShelterViewer.Services;

public class LayoutService
{
    private readonly IUserPreferencesService _userPreferenceService;
    private UserPreferences.UserPreferences? _preferences;

    public bool IsDarkMode { get; private set; } = false;

    public LayoutService(IUserPreferencesService service)
    {
        _userPreferenceService = service;
    }

    public void SetDarkMode(bool isDarkMode)
    {
        IsDarkMode = isDarkMode;
    }

    public async Task ApplyUserPreferences(bool isDarkModeDefault)
    {
        _preferences = await _userPreferenceService.LoadUserPreferences();

        if (_preferences is not null)
        {
            IsDarkMode = _preferences.DarkTheme;
        }
        else
        {
            IsDarkMode = isDarkModeDefault;
            _preferences = new UserPreferences.UserPreferences { DarkTheme = IsDarkMode };
            await _userPreferenceService.SaveUserPreferences(_preferences);
        }
    }

    public event Action? OnMajorUpdate;

    public async Task ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
        _preferences!.DarkTheme = IsDarkMode;
        await _userPreferenceService.SaveUserPreferences(_preferences);
        OnMajorUpdate?.Invoke();
    }
}