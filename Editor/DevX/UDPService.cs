#if (SERVICES_SDK_CORE_ENABLED && ENABLE_EDITOR_GAME_SERVICES)
using Unity.Services.Core.Editor;
using UnityEditor;

namespace UnityEngine.UDP.Editor
{
    class UDPService: IEditorGameService
    {
        public const string ServiceIdentifier = "UDP";

        public string Name => "Unity Distribution Portal";

        public IEditorGameServiceIdentifier Identifier => new UDPServiceIdentifier();

        public bool RequiresCoppaCompliance => false;

        public bool HasDashboard => true;

        public string GetFormattedDashboardUrl()
        {
            return $"https://dashboard.unity3d.com/organizations/{CloudProjectSettings.organizationKey}/udp/get-started";
        }

        public IEditorGameServiceEnabler Enabler { get; } = null;
    }
}

#endif