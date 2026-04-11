using UnityEngine;

namespace McFlurryMenu;

public static class UIHelpers
{
    public static void ApplyUIColor()
    {
        if (CheatToggles.rgbMode)
        {
            // Set background color based on the cycling hue in MenuUI
            GUI.backgroundColor = Color.HSVToRGB(MenuUI.hue, 1f, 1f); 
        }
        else
        {
            // Rebranded reference to the main plugin's HTML color config
            var configHtmlColor = McFlurryPlugin.menuHtmlColor.Value;

            if (!ColorUtility.TryParseHtmlString(configHtmlColor, out var uiColor))
            {
                if (!configHtmlColor.StartsWith("#"))
                {
                    if (ColorUtility.TryParseHtmlString("#" + configHtmlColor, out uiColor))
                    {
                        GUI.backgroundColor = uiColor;
                    }
                }
            }
            else
            {
                GUI.backgroundColor = uiColor;
            }
        }
    }
}
