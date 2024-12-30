using MudBlazor;

namespace ShelterViewer.Shared.Themes
{
    public class VaultTheme : MudTheme
    {
        public VaultTheme()
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#00FF00",       // Neon Green
                Secondary = "#00AA00",     // Slightly Dimmer Green
                Background = "#A2BAA2",    // Light Green (Needs to be altered)
                Surface = "rgba(245, 245, 245, 0.95)", // Semi-transparent panels
                AppbarBackground = "#597159", // Darkish Green
                AppbarText = "#00FF00",    // Neon Green
                TextPrimary = "#121212",   // Dark Text
                TextSecondary = "#404040", // Gray Secondary Text
                ActionDefault = "#00FF00", // Bright Green for actions
                ActionDisabled = "#B0B0B0", // Light Gray for disabled actions
                ActionDisabledBackground = "rgba(0, 0, 0, 0.1)", // Transparent disabled background
            };

            PaletteDark = new PaletteDark
            {
                Primary = "#00FF00",       // Neon Green
                Secondary = "#00AA00",     // Slightly Dimmer Green
                Background = "#121212",    // Dark Background
                Surface = "rgba(18, 18, 18, 0.85)", // Semi-transparent panels
                AppbarBackground = "#121212", // Appbar same as background
                AppbarText = "#00FF00",    // Neon Green
                TextPrimary = "#FFFFFF",   // White Text
                TextSecondary = "#A0A0A0", // Gray Secondary Text
                ActionDefault = "#00FF00", // Bright Green for actions
                ActionDisabled = "#404040", // Dim Gray for disabled actions
                ActionDisabledBackground = "rgba(255, 255, 255, 0.1)", // Transparent disabled background
            };
        }
    }
}
