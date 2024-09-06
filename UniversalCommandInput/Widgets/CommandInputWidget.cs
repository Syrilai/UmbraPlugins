using System.Collections.Generic;
using Umbra.Widgets;

namespace Syrilai.CommandInput.Widgets; 

[ToolbarWidget(
    "CommandInput",
    "Command Input",
    "Easily run specific commands with custom input"
)]
public class CommandInputWidget(
    WidgetInfo info,
    string? guid = null,
    Dictionary<string, object>? configValues = null
) : DefaultToolbarWidget(info, guid, configValues)
{
    public override CommandInputWidgetPopup Popup { get; } = new();

    /// <inheritdoc/>
    protected override IEnumerable<IWidgetConfigVariable> GetConfigVariables()
    {
        return [
            new StringWidgetConfigVariable(
                "Command",
                "Command",
                "What command should be ran with the given input. A / is automatically added at the start if none is entered.",
                "/"
            ),
            new StringWidgetConfigVariable(
                "ToolbarText",
                "Toolbar Text",
                "The text to display on the toolbar.",
                "Command Input"
            ),
            DefaultIconConfigVariable(29),
            ..DefaultToolbarWidgetConfigVariables,
            ..SingleLabelTextOffsetVariables
        ];
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
        Popup.InitializeNodes();
    }

    /// <inheritdoc/>
    protected override void OnUpdate()
    {
        SetLabel(GetConfigValue<string>("ToolbarText"));
        SetIcon(GetConfigValue<uint>("IconId"));

        Popup.ConfiguredCommand = GetConfigValue<string>("Command");

        Popup.UpdateNodes();
        base.OnUpdate();
    }
}
