using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using Umbra.Common;
using Umbra.Game;
using Umbra.Widgets;

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

    private IToastGui ToastGui { get; set; } = Framework.Service<IToastGui>();
    private ISigScanner SigScanner { get; set; } = Framework.Service<ISigScanner>();
    private IChatSender ChatSender { get; set; } = Framework.Service<IChatSender>();

    // Borrowed from https://github.com/zacharied/FFXIV-Plugin-HudManager/blob/testing/HUDManager/Hud.cs
    private const int FileDataPointerOffset = 0x50;
    private const int DataSlotOffset = 0xC8E0; // Updated 7.0
    private delegate IntPtr GetFilePointerDelegate(byte index);
    private GetFilePointerDelegate? _getFilePointer;

    protected override IEnumerable<IWidgetConfigVariable> GetConfigVariables()
    {
        return [
            new BooleanWidgetConfigVariable(
                "Decorate",
                "Decorate the widget",
                "Whether to decorate the widget with a background and border.",
                true
            ),
            new BooleanWidgetConfigVariable(
                "Icon", 
                "Show Icon", 
                "Whether to show the left icon or not.", 
                true
            ),
            new BooleanWidgetConfigVariable(
                "ShowSlot0",
                "Show Slot 1",
                "Wether Layout Slot 1 should be shown as an option.",
                true
            ),
            new BooleanWidgetConfigVariable(
                "ShowSlot1",
                "Show Slot 2",
                "Wether Layout Slot 2 should be shown as an option.",
                true
            ),
            new BooleanWidgetConfigVariable(
                "ShowSlot2",
                "Show Slot 3",
                "Wether Layout Slot 3 should be shown as an option.",
                true
            ),
            new BooleanWidgetConfigVariable(
                "ShowSlot3",
                "Show Slot 4",
                "Wether Layout Slot 4 should be shown as an option.",
                true
            )
        ];
    }

    // Borrowed from https://github.com/zacharied/FFXIV-Plugin-HudManager/blob/testing/HUDManager/Hud.cs
    public IntPtr GetDataPointer()
    {
        var dataPtr = GetFilePointer(0) + FileDataPointerOffset;
        return Marshal.ReadIntPtr(dataPtr);
    }

    // Borrowed from https://github.com/zacharied/FFXIV-Plugin-HudManager/blob/testing/HUDManager/Hud.cs
    public IntPtr GetFilePointer(byte index)
    {
        return _getFilePointer?.Invoke(index) ?? IntPtr.Zero;
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
            var getFilePointerPtr = SigScanner.ScanText("E8 ?? ?? ?? ?? 48 85 C0 74 14 83 7B 44 00");

            if (getFilePointerPtr != IntPtr.Zero)
            {
                _getFilePointer = Marshal.GetDelegateForFunctionPointer<GetFilePointerDelegate>(getFilePointerPtr);
            }
        } catch (Exception) {
            theBuildingIsOnFire = true;
        }

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
            Popup.AddButton(
                $"Layout{i}",
                label: $"Layout {userShownI}",
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

        SetLeftIcon(GetConfigValue<bool>("Icon") ? 31 : null);

        for (int i = 0; i <= 3; i++)
        {
            var isEnabled = GetConfigValue<bool>($"ShowSlot{i}");
            var userShownI = i + 1;
            if (isEnabled)
            {
                if (activeHudSlot == i)
                    seStringBuilder.AddUiForeground($"{userShownI}", 31);
                else
                    seStringBuilder.AddUiForeground($"{userShownI}", 3);
            };

            if (!Popup.IsOpen) continue;

            Popup.SetButtonVisibility($"Layout{i}", isEnabled);
        }

        SetLabel(
            seStringBuilder.Build()
        );
        SetGhost(!GetConfigValue<bool>("Decorate"));

        if (!Popup.IsOpen) {
            return;
        }
    }
}
