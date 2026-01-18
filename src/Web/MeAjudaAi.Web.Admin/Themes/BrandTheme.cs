using MudBlazor;

namespace MeAjudaAi.Web.Admin.Themes;

/// <summary>
/// Tema da marca MeAjudaAi para o Admin Portal.
/// Paleta de cores: Azul (primária), Creme, Laranja (secundária), Branco
/// </summary>
public static class BrandTheme
{
    /// <summary>
    /// Obtém o tema da marca MeAjudaAi com paleta de cores customizada.
    /// </summary>
    public static MudTheme Theme => new()
    {
        PaletteLight = new PaletteLight
        {
            // Primary: Brand Blue
            Primary = "#1E88E5",           // Material Blue 600
            PrimaryContrastText = "#FFFFFF",
            PrimaryLighten = "#42A5F5",    // Blue 400
            PrimaryDarken = "#1565C0",     // Blue 800
            
            // Secondary: Brand Orange
            Secondary = "#FB8C00",         // Material Orange 600
            SecondaryContrastText = "#FFFFFF",
            SecondaryLighten = "#FFA726",  // Orange 400
            SecondaryDarken = "#EF6C00",   // Orange 800
            
            // Tertiary: Brand Cream
            Tertiary = "#FFF8E1",          // Light Cream
            TertiaryContrastText = "#5D4037",
            
            // Info, Success, Warning, Error (standard Material Design)
            Info = "#0288D1",
            InfoContrastText = "#FFFFFF",
            
            Success = "#388E3C",
            SuccessContrastText = "#FFFFFF",
            
            Warning = "#F57C00",
            WarningContrastText = "#FFFFFF",
            
            Error = "#D32F2F",
            ErrorContrastText = "#FFFFFF",
            
            // Background & Surface
            Background = "#FFFFFF",        // White
            BackgroundGray = "#FAFAFA",    // Very light gray
            Surface = "#FFFFFF",
            
            // Text
            TextPrimary = "#212121",       // Almost black
            TextSecondary = "#757575",     // Gray 600
            TextDisabled = "#BDBDBD",      // Gray 400
            
            // Appbar
            AppbarBackground = "#1E88E5",  // Primary Blue
            AppbarText = "#FFFFFF",
            
            // Drawer
            DrawerBackground = "#FFFFFF",
            DrawerText = "#212121",
            DrawerIcon = "#757575",
            
            // Lines & Dividers
            LinesDefault = "#E0E0E0",      // Gray 300
            LinesInputs = "#BDBDBD",       // Gray 400
            
            // Table
            TableLines = "#E0E0E0",
            TableStriped = "#FFF8E1",      // Cream for alternating rows
            TableHover = "#FFF3E0",        // Light orange on hover
            
            // Actions & Hover
            ActionDefault = "#757575",
            ActionDisabled = "#BDBDBD",
            ActionDisabledBackground = "#EEEEEE",
            
            HoverOpacity = 0.06,
            RippleOpacity = 0.1,
        },
        
        PaletteDark = new PaletteDark
        {
            // Primary: Brand Blue (adjusted for dark mode)
            Primary = "#42A5F5",           // Lighter blue for dark backgrounds
            PrimaryContrastText = "#000000",
            PrimaryLighten = "#64B5F6",
            PrimaryDarken = "#1E88E5",
            
            // Secondary: Brand Orange (adjusted for dark mode)
            Secondary = "#FFA726",         // Lighter orange
            SecondaryContrastText = "#000000",
            SecondaryLighten = "#FFB74D",
            SecondaryDarken = "#FB8C00",
            
            // Tertiary: Brand Cream (darker variant)
            Tertiary = "#5D4037",          // Brown for dark mode
            TertiaryContrastText = "#FFF8E1",
            
            // Info, Success, Warning, Error
            Info = "#29B6F6",
            InfoContrastText = "#000000",
            
            Success = "#66BB6A",
            SuccessContrastText = "#000000",
            
            Warning = "#FFA726",
            WarningContrastText = "#000000",
            
            Error = "#EF5350",
            ErrorContrastText = "#000000",
            
            // Background & Surface
            Background = "#121212",        // Material dark background
            BackgroundGray = "#1E1E1E",
            Surface = "#1E1E1E",
            
            // Text
            TextPrimary = "#FFFFFF",
            TextSecondary = "#B0B0B0",
            TextDisabled = "#6C6C6C",
            
            // Appbar
            AppbarBackground = "#1E1E1E",
            AppbarText = "#FFFFFF",
            
            // Drawer
            DrawerBackground = "#1E1E1E",
            DrawerText = "#FFFFFF",
            DrawerIcon = "#B0B0B0",
            
            // Lines & Dividers
            LinesDefault = "#424242",
            LinesInputs = "#6C6C6C",
            
            // Table
            TableLines = "#424242",
            TableStriped = "#2C2C2C",
            TableHover = "#333333",
            
            // Actions & Hover
            ActionDefault = "#B0B0B0",
            ActionDisabled = "#6C6C6C",
            ActionDisabledBackground = "#2C2C2C",
            
            HoverOpacity = 0.08,
            RippleOpacity = 0.12,
        }
    };
}
