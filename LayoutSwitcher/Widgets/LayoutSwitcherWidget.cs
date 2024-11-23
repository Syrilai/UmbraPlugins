using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface;
using Umbra.Common;
using Umbra.Game;
using Umbra.Widgets;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace Umbra.SamplePlugin.Widgets;

[ToolbarWidget(
    "LayoutSwitcher",
    "Layout Switcher",
    "Let's you change your HUD Layout easily"
)]
public class LayoutSwitcherWidget(
    WidgetInfo info,
    string? guid = null,
    Dictionary<string, object>? configValues = null
) : DefaultToolbarWidget(info, guid, configValues)
{
    public override MenuPopup Popup { get; } = new();

    private IChatSender ChatSender { get; set; } = Framework.Service<IChatSender>();

    // Borrowed from https://github.com/zacharied/FFXIV-Plugin-HudManager/blob/testing/HUDManager/Hud.cs
    private const int DataSlotOffset = 0xCBD0; // Updated 7.1

    protected override IEnumerable<IWidgetConfigVariable> GetConfigVariables()
    {
        return [
            new BooleanWidgetConfigVariable(
                "ShowSlot0",
                "Show Slot 1",
                "Wether Layout Slot 1 should be shown as an option.",
                true
            ) {
                Category = I18N.Translate("Widget.ConfigCategory.WidgetAppearance")
            },
            new StringWidgetConfigVariable(
                "Slot0Label",
                "Slot 1 Label",
                "The label for Slot 1.",
                "Layout 1"
            ) {
                Category = I18N.Translate("Widget.ConfigCategory.WidgetAppearance")
            },
            new BooleanWidgetConfigVariable(
                "ShowSlot1",
                "Show Slot 2",
                "Wether Layout Slot 2 should be shown as an option.",
                true
            ) {
                Category = I18N.Translate("Widget.ConfigCategory.WidgetAppearance")
            },
            new StringWidgetConfigVariable(
                "Slot1Label",
                "Slot 2 Label",
                "The label for Slot 2.",
                "Layout 2"
            ) {
                Category = I18N.Translate("Widget.ConfigCategory.WidgetAppearance")
            },
            new BooleanWidgetConfigVariable(
                "ShowSlot2",
                "Show Slot 3",
                "Wether Layout Slot 3 should be shown as an option.",
                true
            ) {
                Category = I18N.Translate("Widget.ConfigCategory.WidgetAppearance")
            },
            new StringWidgetConfigVariable(
                "Slot2Label",
                "Slot 3 Label",
                "The label for Slot 3.",
                "Layout 3"
            ) {
                Category = I18N.Translate("Widget.ConfigCategory.WidgetAppearance")
            },
            new BooleanWidgetConfigVariable(
                "ShowSlot3",
                "Show Slot 4",
                "Wether Layout Slot 4 should be shown as an option.",
                true
            ) {
                Category = I18N.Translate("Widget.ConfigCategory.WidgetAppearance")
            },
            new StringWidgetConfigVariable(
                "Slot3Label",
                "Slot 4 Label",
                "The label for Slot 4.",
                "Layout 4"
            ) {
                Category = I18N.Translate("Widget.ConfigCategory.WidgetAppearance")
            },
            DefaultIconConfigVariable(31),
            ..DefaultToolbarWidgetConfigVariables,
            ..SingleLabelTextOffsetVariables
        ];
    }

    // Borrowed from https://github.com/zacharied/FFXIV-Plugin-HudManager/blob/testing/HUDManager/Hud.cs
    public static unsafe IntPtr GetDataPointer()
    {
        return (nint)AddonConfig.Instance()->ModuleData;
    }

    // Borrowed from https://github.com/zacharied/FFXIV-Plugin-HudManager/blob/testing/HUDManager/Hud.cs
    public int GetActiveHudSlot()
    {
        return Marshal.ReadInt32(GetDataPointer() + DataSlotOffset);
    }

    private bool theBuildingIsOnFire = false;

    /// <inheritdoc/>
    protected override void Initialize()
    {
        try
        {
            GetActiveHudSlot();
        }
        catch (Exception)
        {
            theBuildingIsOnFire = true;
        }

        if (theBuildingIsOnFire)
        {
            SetLabel("Layout Switcher Unavailable");
            SetDisabled(true);
            return;
        }

        for (var i = 0; i <= 3; i++)
        {
            var userShownI = i + 1;
            var layoutSlotName = GetConfigValue<string>($"Slot{i}Label");
            Popup.AddButton(
                $"Layout{i}",
                label: layoutSlotName,
                onClick: () => ChatSender.Send($"/hudlayout {userShownI}")
             );
        }
    }

    protected override void OnUpdate()
    {
        if (theBuildingIsOnFire)
        {
            return;
        }

        var activeHudSlot = GetActiveHudSlot();
        var seStringBuilder = new SeStringBuilder();

        SetIcon(GetConfigValue<uint>("IconId"));

        for (int i = 0; i <= 3; i++)
        {
            var isEnabled = GetConfigValue<bool>($"ShowSlot{i}");
            var isActive = activeHudSlot == i;
            var userShownI = i + 1;
            if (isEnabled)
            {
                if (isActive)
                    seStringBuilder.AddUiForeground($"{userShownI}", 31);
                else
                    seStringBuilder.AddUiForeground($"{userShownI}", 3);
            };

            if (!Popup.IsOpen) continue;

            var layoutSlotName = GetConfigValue<string>($"Slot{i}Label");

            Popup.SetButtonVisibility($"Layout{i}", isEnabled);
            Popup.SetButtonLabel($"Layout{i}", layoutSlotName);
            Popup.SetButtonIcon($"Layout{i}", isActive ? FontAwesomeIcon.Star : null);
        }

        SetLabel(
            seStringBuilder.Build()
        );

        base.OnUpdate();
    }
}
