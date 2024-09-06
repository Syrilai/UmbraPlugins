using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using Syrilai.UniversalCommandInput.Nodes;
using Umbra.Common;
using Umbra.Game;
using Umbra.Widgets;

namespace Syrilai.CommandInput.Widgets
{
    public partial class CommandInputWidgetPopup : MenuPopup
    {
        private bool isInitialized = false;
        private StringInputNode? inputNode;
        private IChatSender? chatSender;

        public string ConfiguredCommand = "/";

        public void InitializeNodes()
        {
            if (isInitialized) return;
            inputNode = new StringInputNode("CommandInput", "", 128, null, null, 0, false);

            inputNode.OnValueChanged += (rawValue) =>
            {
                var value = rawValue.Trim();

                if (string.IsNullOrEmpty(value)) return;

                var configuredCommand = GetConfiguredCommand();

                GetChatSender().Send(configuredCommand switch
                {
                    "/" => $"{configuredCommand}{value}",
                    _ => $"{configuredCommand} {value}"
                });

                inputNode.Value = "";
                Close();
            };

            var wrapperNode = new Una.Drawing.Node()
            {
                Id = "CommandInputPanel",
                ChildNodes = [
                    new() {
                        Id = "CommandInputWrapper",
                        ChildNodes = [ inputNode ],
                        Style = new() {
                            Size = new() {
                                Width = 300
                            },
                            Padding = new() {
                                Left = 5,
                                Bottom = 5
                            }
                        }
                    }
                ]
            };

            AddButton("CurrentPrefix", label: "Unknown State, Waiting for Update");
            Node.AppendChild(wrapperNode);

            OnPopupOpen += () => inputNode?.CaptureFocus();
            isInitialized = true;
        }

        public void UpdateNodes()
        {
            var configuredCommand = GetConfiguredCommand();

            SetButtonLabel("CurrentPrefix", label: configuredCommand switch {
                "/" => "Acting as command input",
                _ => $"Command '{configuredCommand}'"
            });
        }

        private IChatSender GetChatSender()
        {
            chatSender ??= Framework.Service<IChatSender>();

            return chatSender;
        }

        private string GetConfiguredCommand()
        {
            var configuredPrefix = ConfiguredCommand.TrimStart();

            if (!configuredPrefix.StartsWith('/'))
            {
                configuredPrefix = "/" + configuredPrefix;
            }

            return configuredPrefix;
        }
    }
}
