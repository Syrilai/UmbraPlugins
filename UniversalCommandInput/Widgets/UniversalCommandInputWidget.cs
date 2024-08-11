using System;
using System.Collections.Generic;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using Lumina.Excel.GeneratedSheets;
using Syrilai.UniversalCommandInput.Despair;
using Umbra.Common;
using Umbra.Game;
using Umbra.Widgets;
using Una.Drawing;
using System.Reflection;

namespace Syrilai.UniversalCommandInput.Widgets;

[ToolbarWidget(
    "UniversalCommandInput",
    "Command Input",
    "Easily run specific commands with custom input"
)]
public class LayoutSwitcherWidget(
    WidgetInfo info,
    string? guid = null,
    Dictionary<string, object>? configValues = null
) : DefaultToolbarWidget(info, guid, configValues)
{
    public override MenuPopup Popup { get; } = new();

    private StringInputNode? inputNode { get; set; }

    private IToastGui ToastGui { get; set; } = Framework.Service<IToastGui>();
    private IChatSender ChatSender { get; set; } = Framework.Service<IChatSender>();

    protected override IEnumerable<IWidgetConfigVariable> GetConfigVariables()
    {
        return [
            new BooleanWidgetConfigVariable(
                "Decorate",
                "Decorate the widget",
                "Whether to decorate the widget with a background and border.",
                true
            ) {
                Category = "General",
            },
            new BooleanWidgetConfigVariable(
                "ShowIcon", 
                "Show Icon", 
                "Whether to show the left icon or not.", 
                false
            ) {
                Category = "General"
            },
            new IntegerWidgetConfigVariable(
                "IconId",
                "Icon ID",
                "The icon to display on the left side of the widget. You can get this via /xldata icons.",
                0
            ) {
                Category = "General"
            },
            new StringWidgetConfigVariable(
                "Prefix",
                "Command",
                "What command should be ran with the given input. A / is automatically added at the start if none is entered.",
                "/"
            ) {
                Category = "Widget"
            },
            new StringWidgetConfigVariable(
                "ToolbarText",
                "Toolbar Text",
                "The text to display on the toolbar.",
                "Command Input"
            ) {
                Category = "Widget"
            }
        ];
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
        SetLabel(GetConfigValue<string>("ToolbarText"));
        SetGhost(!GetConfigValue<bool>("Decorate"));
        SetLeftIcon(GetConfigValue<bool>("ShowIcon") ? (uint)GetConfigValue<int>("IconId") : null);

        inputNode = new StringInputNode("Prefix", "", 128, null, null, 0, false);

        inputNode.OnValueChanged += (value) =>
        {
            if (string.IsNullOrEmpty(value.Trim())) return;

            var command = GetConfiguredCommand();

            // Apparently if the user decides to just have a '/'
            // and use this as pure command input, we should
            // rather not just add a space after.. lol
            if (command.Trim() == "/")
            {
                ChatSender.Send(command + value);
            }
            else
            {
                ChatSender.Send(command + " " + value);
            }

            inputNode.Value = "";
            ClosePopup();
        };

        var node = new Node()
        {
            Id = "PrefixPanel",
            ChildNodes = [
                new() {
                    Id = "SearchInputWrapper",
                    ChildNodes = [ inputNode ],
                    Style = new()
                    {
                        Size = new()
                        {
                            Width = 300
                        },
                        Padding = new()
                        {
                            Left = 5,
                            Bottom = 5,
                        }
                    }
                }
            ]
        };

        Popup.AddButton("CurrentPrefix", label: $"Command: {GetConfiguredCommand()}");

        GetPopupNode()?.AppendChild(node);

        Popup.OnPopupOpen += OnPopupOpen;
    }

    private Node? GetPopupNode()
    {
        var popupNode = Popup.GetType().BaseType?
                        .GetProperty("Node", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)?
                        .GetValue(Popup) as Node;

        return popupNode;
    }

    private void ClosePopup()
    {
        Popup.GetType().BaseType?
            .GetMethod("Close", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)?
            .Invoke(Popup, null);
    }

    protected override void OnUpdate()
    {
        SetLabel(GetConfigValue<string>("ToolbarText"));
        SetGhost(!GetConfigValue<bool>("Decorate"));
        SetLeftIcon(GetConfigValue<bool>("ShowIcon") ? (uint)GetConfigValue<int>("IconId") : null);
        var configuredCommand = GetConfiguredCommand();
        if (configuredCommand == "/")
        {
            Popup.SetButtonLabel("CurrentPrefix", label: $"Acting as a command input");
        } 
        else
        {
            Popup.SetButtonLabel("CurrentPrefix", label: $"Command: '{GetConfiguredCommand()}'");
        }
    }

    private string GetConfiguredCommand()
    {
        var configuredPrefix = GetConfigValue<string>("Prefix").TrimStart();

        if (!configuredPrefix.StartsWith('/'))
        {
            configuredPrefix = "/" + configuredPrefix;
        }

        return configuredPrefix;
    }

    private void OnPopupOpen()
    {
        inputNode?.CaptureFocus();
    }
}
