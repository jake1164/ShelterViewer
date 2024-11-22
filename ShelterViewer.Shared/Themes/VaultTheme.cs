using MudBlazor;

namespace ShelterViewer.Shared.Themes
{
    public class VaultTheme : MudTheme
    {
        public VaultTheme()
        {
            PaletteLight = new PaletteLight 
            {
                Primary = Colors.Blue.Accent3
            };

            PaletteDark = new PaletteDark
            {
                Primary = Colors.Lime.Accent3
            };
        }
    }
}
