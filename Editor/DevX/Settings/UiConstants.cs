#if (SERVICES_SDK_CORE_ENABLED && ENABLE_EDITOR_GAME_SERVICES)
using UnityEditor;

namespace UnityEngine.UDP.Editor
{
    public class UiConstants
    {
        internal static class StyleSheetPaths
        {
            internal const string UDPStyleSheetPath =
                "Editor/DevX/Template/ProjectSettingsUDP.uss";

            internal const string UDPMainTemplatePath =
                "Editor/DevX/Template/ProjectSettingsUDP.uxml";

            internal const string UDPLinkProjectTemplatePath =
                "Editor/DevX/Template/ProjectSettingsLinkProject.uxml";
        }

        internal static class LocalizedStrings
        {
            internal static readonly string Title = "Unity Distribution Portal";

            internal static readonly string Description =
                "Distribute to multiple app stores through a single hub.";
        }

        internal static class Urls
        {
            internal const string UDPDashboardLink = "https://distribute.dashboard.unity.com";
        }

        internal static class UiElementNames
        {
            internal static readonly string UDPSettingsWindow = "UDPSettingsWindow";
            internal static readonly string UDPOperationBlock = "UDPOperationBlock";
            internal static readonly string UDPBasicInfoBlock = "UDPBasicInfoBlock";
            internal static readonly string UDPClientSettingsBlock = "UDPClientSettingsBlock";
            internal static readonly string UDPPlayerBlock = "UDPPlayerBlock";
            internal static readonly string UDPExternalLinkBlock = "UDPExternalLinkBlock";
            internal static readonly string UDPLinkClientBlock = "LinkClientIdBlock";
            internal static readonly string UDPGenerateNewClientBlock = "GenerateNewClientBlock";
            internal static readonly string UDPGoToPortalLiBlock = "GoToPortalLi";
            internal static readonly string UDPSyncStatusGroupBlock = "syncStatusGroupBlock";
            internal static readonly string UDPGenerateClientFoldoutBlock = "GenerateClientFoldoutContainer";

            internal static readonly string UDPPullButton = "PullBtn";
            internal static readonly string UDPPushButton = "PushBtn";
            internal static readonly string UDPGameTitle = "UdpGameTitle";
            internal static readonly string UDPProjectId = "UdpUnityProjectId";
            internal static readonly string CopyProjectIdButton = "CopyProjectIdBtn";

            internal static readonly string UDPGameErrorMessage = "UdpGameErrorMessage";
            internal static readonly string UDPClientErrorMessage = "UdpClientErrorMessage";

            internal static readonly string UDPGameId = "UdpGameId";
            internal static readonly string UDPClientId = "UdpClientId";
            internal static readonly string UDPClientKey = "UdpClientKey";
            internal static readonly string UDPRSAPublicKey = "UdpRSAPublicKey";
            internal static readonly string UDPClientSecret = "UdpClientSecret";
            internal static readonly string UDPClientCallbackUrl = "UdpClientCallbackUrl";
            internal static readonly string Players = "UDPPlayers";
            internal static readonly string AddNewPlayerButton = "AddNewPlayerBtn";
            internal static readonly string GoToUdpDashboard = "GoToUDPDashboard";
            internal static readonly string GoToIAPCatalog = "GoToIAPCatalog";

            internal static readonly string GenNewClientButton = "GenerateNewClientBtn";
            internal static readonly string UdpClientIdToBeLinked = "UdpClientIdToBeLinked";
            internal static readonly string LinkExistingClientButton = "LinkExistingClientBtn";
            internal static readonly string UDPGoToPortalBtn = "GoToPortalBtn";
            internal static readonly string UDPToggleBtn = "GenerateToggleBox";

            internal static readonly string UDPPullingLabel = "PullingLabel";
            internal static readonly string UDPPushingLabel = "PushingLabel";
        }

        internal static class UssStrings
        {
            internal static readonly string UssStringsErrorTag = "warning-message";
            internal static readonly string UssStringsPlayer = "player";
            internal static readonly string UssStringsCannotOAuthWarning = "udp-can-not-oauth";
        }
    }
}


#endif