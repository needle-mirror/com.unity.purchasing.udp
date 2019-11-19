#if (UNITY_2020_1_OR_NEWER)

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.UDP;
using UnityEditor.Connect;
using UnityEngine.UIElements;
using System.Linq;
using UnityEngine;
using UnityEngine.UDP.Editor.Analytics;

namespace UnityEngine.UDP.Editor
{

    internal class AppStoreProjectSettingsEditor : ServicesProjectSettings
    {
        protected override Notification.Topic[] notificationTopicsToSubscribe => new[] { Notification.Topic.UDPService };
        protected override SingleService serviceInstance => UDPService.instance;

        protected override string serviceUssClassName => "udp";

        const string k_UDPStyleSheetPath =
            "Packages/com.unity.purchasing.udp/Editor/Template/ProjectSettingsUDP.uss";

        const string k_UDPMainTemplatePath =
            "Packages/com.unity.purchasing.udp/Editor/Template/ProjectSettingsUDP.uxml";

        const string k_UDPLinkProjectTemplatePath =
            "Packages/com.unity.purchasing.udp/Editor/Template/ProjectSettingsLinkProject.uxml";

        const string k_UDPDashboardLink = "https://distribute.dashboard.unity.com";

        private static class UxmlStrings
        {
            internal static readonly string k_UDPSettingsWindow = "UDPSettingsWindow";
            internal static readonly string k_UDPOperationBlock = "UDPOperationBlock";
            internal static readonly string k_UDPBasicInfoBlock = "UDPBasicInfoBlock";
            internal static readonly string k_UDPClientSettingsBlock = "UDPClientSettingsBlock";
            internal static readonly string k_UDPPlayerBlock = "UDPPlayerBlock";
            internal static readonly string k_UDPExternalLinkBlock = "UDPExternalLinkBlock";
            internal static readonly string k_UDPLinkClientBlock = "LinkClientIdBlock";

            internal static readonly string k_UDPPullButton = "PullBtn";
            internal static readonly string k_UDPPushButton = "PushBtn";
            internal static readonly string k_UDPGameTitle = "UdpGameTitle";
            internal static readonly string k_UDPProjectId = "UdpUnityProjectId";
            internal static readonly string k_CopyProjectIdButton = "CopyProjectIdBtn";

            internal static readonly string k_UDPGameErrorMessage = "UdpGameErrorMessage";
            internal static readonly string k_UDPClientErrorMessage = "UdpClientErrorMessage";

            internal static readonly string k_UDPGameId = "UdpGameId";
            internal static readonly string k_UDPClientId = "UdpClientId";
            internal static readonly string k_UDPClientKey = "UdpClientKey";
            internal static readonly string k_UDPRSAPublicKey = "UdpRSAPublicKey";
            internal static readonly string k_UDPClientSecret = "UdpClientSecret";
            internal static readonly string k_UDPClientCallbackUrl = "UdpClientCallbackUrl";
            internal static readonly string k_Players = "UDPPlayers";
            internal static readonly string k_AddNewPlayerButton = "AddNewPlayerBtn";
            internal static readonly string k_GoToUdpDashboard = "GoToUDPDashboard";
            internal static readonly string k_GoToIAPCatalog = "GoToIAPCatalog";

            internal static readonly string k_GenNewClientButton = "GenerateNewClientBtn";
            internal static readonly string k_UdpClientIdToBeLinked = "UdpClientIdToBeLinked";
            internal static readonly string k_LinkExistingClientButton = "LinkExistingClientBtn";
        }

        private static class UssStrings
        {
            internal static readonly string k_UssStringsErrorTag = "warning-message";
            internal static readonly string k_UssStringsPlayer = "player";
            internal static readonly string k_UssStringsCannotOAuthWarning = "udp-can-not-oauth";
        }

        VisualTreeAsset m_MainTemplateAssets;
        VisualTreeAsset m_LinkProjectTemplateAssets;
        VisualElement m_ServiceWindowContainer;
        VisualElement m_ServiceLinkProjectContainer;
        VisualElement m_ServiceCannotOAuthContainer;
        VisualElement m_ServiceCannotGetProjectIdContainer;
        VisualElement m_UDPSettingsWindow;
        VisualElement m_UDPOperationBlock;
        VisualElement m_UDPBasicInfoBlock;
        VisualElement m_UDPClientSettingsBlock;
        VisualElement m_UDPPlayerBlock;
        VisualElement m_UDPPlayersContainer;
        VisualElement m_UDPExternalLinkBlock;
        VisualElement m_UDPLinkClientIdBlock;

        #region local field
        string m_ClientSecretInMemory;
        string m_CallbackUrlInMemory;
        readonly List<string> k_PushRequestList = new List<string>();

        static readonly bool k_EnableOAuth =
            Utils.FindTypeByName("UnityEditor.Connect.UnityOAuth") != null;

        string m_CallbackUrlLast;
        const string k_StepGenerateClient = "gen_client";
        const string k_StepGetClient = "get_client";
        const string k_StepUpdateClient = "update_client";
        const string k_UpdateClientSecret = "update_client_secret";
        const string k_StepUpdateGameTitle = "update_game_title";

        List<TestAccount> m_TestAccounts = new List<TestAccount>();
        List<EasyToSaveAccount> m_EasyToSaveAccounts = new List<EasyToSaveAccount>();
        readonly List<bool> k_TestAccountsDirty = new List<bool>();
        readonly List<string> k_TestAccountsValidationMsg = new List<string>();

        AppItem m_CurrentAppItem;
        string m_TargetStep;
        string m_ClientIdToBeLinked = "UDP client ID";
        string m_UpdateClientErrorMsg;
        string m_UpdateGameTitleErrorMsg;

        private struct RequestStruct
        {
            public string targetStep;
            public string currentStep;
            public string eventName;

            public TestAccount currTestAccount;
            public int arrayPosition;
            public UnityWebRequest request;
            public GeneralResponse resp;
        }

        private struct EasyToSaveAccount
        {
            public string email;
            public string password;
            public string playerId;
            public bool deleted;
            public bool isUpdate;
        }

        readonly Queue<RequestStruct> k_RequestQueue = new Queue<RequestStruct>();

        SerializedObject m_SerializedSettings;
        SerializedProperty m_UnityProjectId;
        SerializedProperty m_UnityClientId;
        SerializedProperty m_UnityClientKey;
        SerializedProperty m_UnityClientRsaPublicKey;
        SerializedProperty m_AppName;
        SerializedProperty m_AppSlug;
        SerializedProperty m_AppItemId;

        bool m_IsGettingAuthCode ;
        bool m_IsGettingAccessToken ;
        bool m_IsGettingUserId ;
        bool m_IsGettingOrgId ;
        bool m_IsSearchingClient;
        bool m_IsCreatingClient ;
        bool m_IsGettingClient ;
        bool m_IsUpdatingClient;
        bool m_IsSearchingAppItem ;
        bool m_IsUpdatingAppItem ;
        bool m_IsCreatingAppItem ;
        bool m_IsPublishingAppItem;
        bool m_IsSearchingPlayer ;
        bool m_IsDeletingPlayer ;

        #endregion

        [SettingsProvider]
        static SettingsProvider CreateProjectSettingsProvider()
        {
            var provider =
                new AppStoreProjectSettingsEditor("Project/Services/Unity Distribution Portal", SettingsScope.Project);
            return provider;
        }

        protected AppStoreProjectSettingsEditor(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, "UDP", keywords)
        {
        }

        protected override void ActivateAction(string searchContext)
        {
            Log("activate");
            if (!k_EnableOAuth)
            {
                Debug.LogError("cannot oauth");
                SetUpCannotOAuthWindow();
                return;
            }

            CreateSettingsAsset();
            InitializeProperties();
            // Must reset properties every time this is activated
            // rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(k_udpStyleSheetPath));
            rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(k_UDPStyleSheetPath));
            m_MainTemplateAssets = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_UDPMainTemplatePath);
            m_LinkProjectTemplateAssets = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_UDPLinkProjectTemplatePath);

            //            rootVisualElement.AddStyleSheetPath(EditorGUIUtility.isProSkin ? k_ServicesWindowDarkUssPath : k_ServicesWindowLightUssPath);
            //            rootVisualElement.AddStyleSheetPath(EditorGUIUtility.isProSkin ? k_ServicesDarkUssPath : k_ServicesLightUssPath);
            //            rootVisualElement.AddStyleSheetPath(k_CollaborateCommonUssPath);
            //            rootVisualElement.AddStyleSheetPath(EditorGUIUtility.isProSkin ? k_CollaborateDarkUssPath : k_CollaborateLightUssPath);
            //            m_UDPGameTitleInput = rootVisualElement.Q<TextField>(UxmlStrings.k_UDPGameTitle);
            //            m_UDPProjectIdInput = rootVisualElement.Q<TextField>(UxmlStrings.k_UDPProjectId);
            //            m_UDPGoToConsoleButton = rootVisualElement.Q<Button>(UxmlStrings.k_GoToUdpDashboard);
            //            m_UDPOpenIapCatalogButton = rootVisualElement.Q<Button>(k);

            RestoreTestAccount();
            SetUpServiceWindow();
            EditorApplication.update -= CheckRequestUpdate;
            EditorApplication.update += CheckRequestUpdate;
        }

        private void SetUpServiceWindow()
        {
            HideVisualElement(m_ServiceLinkProjectContainer);
            HideVisualElement(m_ServiceCannotOAuthContainer);
            HideVisualElement(m_ServiceCannotGetProjectIdContainer);

            if (m_ServiceWindowContainer != null && rootVisualElement.Contains(m_ServiceWindowContainer))
            {
                rootVisualElement.Remove(m_ServiceWindowContainer);
            }

            m_ServiceWindowContainer = new VisualElement();
            m_ServiceWindowContainer.Add(m_MainTemplateAssets.CloneTree().contentContainer);
            rootVisualElement.Add(m_ServiceWindowContainer);

            m_UDPSettingsWindow = rootVisualElement.Q(UxmlStrings.k_UDPSettingsWindow);
            m_UDPOperationBlock = rootVisualElement.Q(UxmlStrings.k_UDPOperationBlock);
            m_UDPBasicInfoBlock = rootVisualElement.Q(UxmlStrings.k_UDPBasicInfoBlock);
            m_UDPClientSettingsBlock = rootVisualElement.Q(UxmlStrings.k_UDPClientSettingsBlock);
            m_UDPPlayerBlock = rootVisualElement.Q(UxmlStrings.k_UDPPlayerBlock);
            m_UDPExternalLinkBlock = rootVisualElement.Q(UxmlStrings.k_UDPExternalLinkBlock);

            SetUpOperationsPart();
            SetUpBasicInfoPart();
            SetUpClientSettingsPart();
            SetUpPlayerPart();
            SetUpExternalLinkPart();
        }

        private void SetUpLinkProjectWindow()
        {
            HideVisualElement(m_ServiceWindowContainer);
            HideVisualElement(m_ServiceCannotOAuthContainer);
            HideVisualElement(m_ServiceCannotGetProjectIdContainer);

            if (m_ServiceLinkProjectContainer != null && rootVisualElement.Contains(m_ServiceLinkProjectContainer))
            {
                rootVisualElement.Remove(m_ServiceLinkProjectContainer);
            }

            m_ServiceLinkProjectContainer = new VisualElement();
            m_ServiceLinkProjectContainer.Add(m_LinkProjectTemplateAssets.CloneTree().contentContainer);
            rootVisualElement.Add(m_ServiceLinkProjectContainer);

            m_UDPLinkClientIdBlock = rootVisualElement.Q(UxmlStrings.k_UDPLinkClientBlock);
            m_UDPLinkClientIdBlock.Q<Button>(UxmlStrings.k_GenNewClientButton).clicked -=
                OnGenerateNewClientButtonClick;
            m_UDPLinkClientIdBlock.Q<Button>(UxmlStrings.k_GenNewClientButton).clicked +=
                OnGenerateNewClientButtonClick;

            m_UDPLinkClientIdBlock.Q<Button>(UxmlStrings.k_LinkExistingClientButton).clicked -=
                OnLinkToExitingClientButtonClick;
            m_UDPLinkClientIdBlock.Q<Button>(UxmlStrings.k_LinkExistingClientButton).clicked +=
                OnLinkToExitingClientButtonClick;
        }

        private void SetUpCannotOAuthWindow()
        {
            EditorApplication.update -= CheckRequestUpdate;

            //hide others
            HideVisualElement(m_ServiceWindowContainer);
            HideVisualElement(m_ServiceLinkProjectContainer);
            HideVisualElement(m_ServiceCannotGetProjectIdContainer);

            if (m_ServiceCannotOAuthContainer != null && rootVisualElement.Contains(m_ServiceCannotOAuthContainer))
            {
                rootVisualElement.Remove(m_ServiceCannotOAuthContainer);
            }

            m_ServiceCannotOAuthContainer = new VisualElement();
            var warningMsg = new Label(
                "UDP editor extension can only work on Unity 5.6.1+. Please check your Unity version and retry.");
            warningMsg.AddToClassList(UssStrings.k_UssStringsCannotOAuthWarning);
            m_ServiceCannotOAuthContainer.Add(warningMsg);
            rootVisualElement.Add(m_ServiceCannotOAuthContainer);
        }

        private void SetUpCannotGetProjectIdWindow()
        {
            EditorApplication.update -= CheckRequestUpdate;

            //hide others
            HideVisualElement(m_ServiceWindowContainer);
            HideVisualElement(m_ServiceLinkProjectContainer);
            HideVisualElement(m_ServiceCannotOAuthContainer);

            if (m_ServiceCannotGetProjectIdContainer != null &&
                rootVisualElement.Contains(m_ServiceCannotGetProjectIdContainer))
            {
                rootVisualElement.Remove(m_ServiceCannotGetProjectIdContainer);
            }

            m_ServiceCannotGetProjectIdContainer = new VisualElement();
            var warningMsg = new Label(
                "To use the Unity distribution portal your project will need a Unity project ID. You can create a new project ID or link to an existing one in the Services window.");
            warningMsg.AddToClassList(UssStrings.k_UssStringsCannotOAuthWarning);
            m_ServiceCannotGetProjectIdContainer.Add(warningMsg);
            rootVisualElement.Add(m_ServiceCannotGetProjectIdContainer);
        }

        protected override void ToggleRestrictedVisualElementsAvailability(bool enable)
        {
            return;
        }

        protected override void DeactivateAction()
        {
            EditorApplication.update -= CheckRequestUpdate;
            SaveToTestAccountStruct();
        }

        private void CheckRequestUpdate()
        {
            var doing = IsOperationRunning() || k_PushRequestList.Count > 0;

            m_UDPSettingsWindow?.SetEnabled(!doing);

            m_UDPOperationBlock?.SetEnabled(!doing);

            m_UDPPlayerBlock?.SetEnabled(!m_IsDeletingPlayer);

            if (k_RequestQueue.Count == 0)
            {
                return;
            }

            RequestStruct reqStruct = k_RequestQueue.Dequeue();
            UnityWebRequest request = reqStruct.request;
            GeneralResponse resp = reqStruct.resp;

            if (request != null && request.isDone)
            {
                if (request.error != null || request.responseCode / 100 != 2)
                {
                    // Deal with errors
                    if (request.downloadHandler.text.Contains(AppStoreOnboardApi.invalidAccessTokenInfo)
                        || request.downloadHandler.text.Contains(AppStoreOnboardApi.forbiddenInfo)
                        || request.downloadHandler.text.Contains(AppStoreOnboardApi.expiredAccessTokenInfo))
                    {
                        m_IsGettingAccessToken = true;
                        UnityWebRequest newRequest = AppStoreOnboardApi.RefreshToken();
                        TokenInfo tokenInfoResp = new TokenInfo();
                        RequestStruct newReqStruct = new RequestStruct();
                        newReqStruct.request = newRequest;
                        newReqStruct.resp = tokenInfoResp;
                        newReqStruct.targetStep = reqStruct.targetStep;
                        k_RequestQueue.Enqueue(newReqStruct);
                    }
                    else if (request.downloadHandler.text.Contains(AppStoreOnboardApi.invalidRefreshTokenInfo)
                             || request.downloadHandler.text.Contains(AppStoreOnboardApi.expiredRefreshTokenInfo))
                    {
                        if (reqStruct.targetStep == "LinkProject")
                        {
                            m_TargetStep = reqStruct.targetStep;
                        }
                        else
                        {
                            m_TargetStep = k_StepGetClient;
                        }

                        AppStoreOnboardApi.tokenInfo.access_token = null;
                        AppStoreOnboardApi.tokenInfo.refresh_token = null;
                        GetAuthCode();
                    }
                    else
                    {
                        ResetAllStatusFlag();

                        ErrorResponse response = JsonUtility.FromJson<ErrorResponse>(request.downloadHandler.text);

                        #region Analytics Fails

                        if (resp.GetType() == typeof(EventRequestResponse))
                        {
                            // Debug.Log("[Debug] Event Request Failed: " + reqStruct.eventName);
                            return; // Do not show error dialog
                        }

                        if (resp.GetType() == typeof(UnityClientResponse))
                        {
                            string eventName = null;
                            switch (request.method)
                            {
                                case UnityWebRequest.kHttpVerbPOST:
                                    eventName = EditorAnalyticsApi.k_ClientCreateEventName;
                                    break;
                                case UnityWebRequest.kHttpVerbPUT:
                                    eventName = EditorAnalyticsApi.k_ClientUpdateEventName;
                                    break;
                                default:
                                    eventName = null;
                                    break;
                            }

                            if (eventName != null)
                            {
                                UnityWebRequest analyticsRequest =
                                    EditorAnalyticsApi.ClientEvent(eventName, null, response.message);

                                RequestStruct analyticsReqStruct = new RequestStruct
                                {
                                    request = analyticsRequest,
                                    resp = new EventRequestResponse(),
                                    eventName = eventName,
                                };

                                k_RequestQueue.Enqueue(analyticsReqStruct);
                            }
                        }

                        if (resp.GetType() == typeof(AppItemResponse))
                        {
                            string eventName;
                            switch (request.method)
                            {
                                case UnityWebRequest.kHttpVerbPOST:
                                    eventName = EditorAnalyticsApi.k_AppCreateEventName;
                                    break;
                                case UnityWebRequest.kHttpVerbPUT:
                                    eventName = EditorAnalyticsApi.k_AppUpdateEventName;
                                    break;
                                default:
                                    eventName = null;
                                    break;
                            }

                            if (eventName != null)
                            {
                                UnityWebRequest analyticsRequest =
                                    EditorAnalyticsApi.AppEvent(eventName, m_UnityClientId.stringValue, null,
                                        response.message);

                                RequestStruct analyticsRequestStruct = new RequestStruct
                                {
                                    request = analyticsRequest,
                                    resp = new EventRequestResponse(),
                                    eventName = eventName,
                                };

                                k_RequestQueue.Enqueue(analyticsRequestStruct);
                            }
                        }

                        #endregion

                        ProcessErrorRequest(reqStruct);

                        if (response != null && response.message != null && response.details != null &&
                            response.details.Length != 0)
                        {
                            Debug.LogError("[UDP] " + response.details[0].field + ": " + response.message);
                            EditorUtility.DisplayDialog("Error",
                                response.details[0].field + ": " + response.message,
                                "OK");
                            if (response.message == "Project not found")
                            {
                                SetUpCannotGetProjectIdWindow();
                            }
                        }
                        else if (response != null && response.message != null)
                        {
                            Debug.LogError("[UDP] " + response.message);
                            EditorUtility.DisplayDialog("Error",
                                response.message,
                                "OK");
                        }
                        else
                        {
                            Debug.LogError("[UDP] Network error, no response received.");
                            EditorUtility.DisplayDialog("Error",
                                "Network error, no response received",
                                "OK");
                        }
                    }
                }

                else
                {
                    Log(resp.GetType().ToString());
                    if (resp.GetType() == typeof(UnityClientResponse))
                    {
                        Log("update client success");
                        // LinkProject & Get Role (later action) will result in this response.
                        resp = JsonUtility.FromJson<UnityClientResponse>(request.downloadHandler.text);
                        m_UnityClientId.stringValue = ((UnityClientResponse)resp).client_id;
                        m_UnityClientKey.stringValue = ((UnityClientResponse)resp).client_secret;
                        m_UnityClientRsaPublicKey.stringValue = ((UnityClientResponse)resp).channel.publicRSAKey;
                        m_UnityProjectId.stringValue = ((UnityClientResponse)resp).channel.projectGuid;
                        m_ClientSecretInMemory = ((UnityClientResponse)resp).channel.channelSecret;
                        m_CallbackUrlInMemory = ((UnityClientResponse)resp).channel.callbackUrl;
                        m_CallbackUrlLast = m_CallbackUrlInMemory;
                        AppStoreOnboardApi.tps = ((UnityClientResponse)resp).channel.thirdPartySettings;
                        AppStoreOnboardApi.updateRev = ((UnityClientResponse)resp).rev;
                        m_SerializedSettings.ApplyModifiedProperties();
                        AssetDatabase.SaveAssets();
                        SaveGameSettingsProps(((UnityClientResponse)resp).client_id);
                        SetUpClientSettingsPart();

                        if (request.method == UnityWebRequest.kHttpVerbPOST) // Generated Client
                        {
                            m_IsCreatingClient = false;
                            UnityWebRequest analyticsRequest =
                                EditorAnalyticsApi.ClientEvent(EditorAnalyticsApi.k_ClientCreateEventName,
                                    ((UnityClientResponse)resp).client_id, null);

                            RequestStruct analyticsReqStruct = new RequestStruct
                            {
                                request = analyticsRequest,
                                resp = new EventRequestResponse(),
                                eventName = EditorAnalyticsApi.k_ClientCreateEventName,
                            };

                            k_RequestQueue.Enqueue(analyticsReqStruct);
                        }
                        else if (request.method == UnityWebRequest.kHttpVerbPUT) // Updated Client
                        {
                            m_IsUpdatingClient = false;
                            UnityWebRequest analyticsRequest =
                                EditorAnalyticsApi.ClientEvent(EditorAnalyticsApi.k_ClientUpdateEventName,
                                    ((UnityClientResponse)resp).client_id, null);

                            RequestStruct analyticsReqStruct = new RequestStruct
                            {
                                request = analyticsRequest,
                                resp = new EventRequestResponse(),
                                eventName = EditorAnalyticsApi.k_ClientUpdateEventName,
                            };

                            k_RequestQueue.Enqueue(analyticsReqStruct);
                        }
                        else
                        {
                            m_IsGettingClient = false;
                        }

                        if (reqStruct.targetStep == "LinkProject")
                        {
                            if (reqStruct.currentStep == "GetClientBeforeLink")
                            {
                                SetUpServiceWindow();
                            }

                            Log("link project to client");

                            m_IsUpdatingClient = true;
                            UnityClientInfo unityClientInfo = new UnityClientInfo();
                            //                            unityClientInfo.ClientId = unityClientID.stringValue;
                            unityClientInfo.ClientId = m_ClientIdToBeLinked;
                            UnityWebRequest newRequest =
                                AppStoreOnboardApi.UpdateUnityClient(Application.cloudProjectId, unityClientInfo,
                                    m_CallbackUrlInMemory);
                            UnityClientResponse clientResp = new UnityClientResponse();
                            RequestStruct newReqStruct = new RequestStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = clientResp;
                            newReqStruct.targetStep = "GetRole";
                            k_RequestQueue.Enqueue(newReqStruct);
                        }

                        else if (reqStruct.targetStep == "GetRole")
                        {
                            Log("get role");

                            m_IsGettingUserId = true;
                            UnityWebRequest newRequest = AppStoreOnboardApi.GetUserId();
                            UserIdResponse userIdResp = new UserIdResponse();
                            RequestStruct newReqStruct = new RequestStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = userIdResp;
                            newReqStruct.targetStep = k_StepGetClient;
                            k_RequestQueue.Enqueue(newReqStruct);
                        }

                        else
                        {
                            if (reqStruct.targetStep == k_StepUpdateClient)
                            {
                                EditorUtility.DisplayDialog("Hint",
                                    "Unity Client updated successfully.",
                                    "OK");
                                //                                RemovePushRequest(m_UnityClientId.stringValue);
                            }

                            if (m_CurrentAppItem.status == "STAGE")
                            {
                                Log("update appItem");
                                m_IsUpdatingAppItem = true;
                                UnityWebRequest newRequest = AppStoreOnboardApi.UpdateAppItem(m_CurrentAppItem);
                                AppItemResponse appItemResponse = new AppItemResponse();
                                RequestStruct newReqStruct = new RequestStruct();
                                newReqStruct.request = newRequest;
                                newReqStruct.resp = appItemResponse;
                                k_RequestQueue.Enqueue(newReqStruct);
                                //                                AddPushRequests(m_CurrentAppItem.id);
                            }
                            else
                            {
                                Log("get appItemByClientId");
                                m_IsSearchingAppItem = true;
                                UnityWebRequest newRequest = AppStoreOnboardApi.GetAppItem(m_UnityClientId.stringValue);
                                AppItemResponseWrapper appItemResponseWrapper = new AppItemResponseWrapper();
                                RequestStruct newReqStruct = new RequestStruct();
                                newReqStruct.request = newRequest;
                                newReqStruct.resp = appItemResponseWrapper;
                                k_RequestQueue.Enqueue(newReqStruct);
                                //                                AddPushRequests(m_CurrentAppItem.id);
                            }
                        }
                    }

                    else if (resp.GetType() == typeof(UserIdResponse))
                    {
                        Log("fetch userId success");
                        m_IsGettingUserId = false;
                        resp = JsonUtility.FromJson<UserIdResponse>(request.downloadHandler.text);
                        AppStoreOnboardApi.userId = ((UserIdResponse)resp).sub;

                        m_IsGettingOrgId = true;
                        UnityWebRequest newRequest = AppStoreOnboardApi.GetOrgId(Application.cloudProjectId);
                        OrgIdResponse orgIdResp = new OrgIdResponse();
                        RequestStruct newReqStruct = new RequestStruct();
                        newReqStruct.request = newRequest;
                        newReqStruct.resp = orgIdResp;
                        newReqStruct.targetStep = reqStruct.targetStep;
                        k_RequestQueue.Enqueue(newReqStruct);
                    }

                    else if (resp.GetType() == typeof(OrgIdResponse))
                    {
                        Log("fetch orgId success");
                        m_IsGettingOrgId = false;

                        resp = JsonUtility.FromJson<OrgIdResponse>(request.downloadHandler.text);
                        AppStoreOnboardApi.orgId = ((OrgIdResponse)resp).org_foreign_key;

                        if (reqStruct.targetStep == k_StepGenerateClient)
                        {
                            UnityClientInfo unityClientInfo = new UnityClientInfo();
                            string callbackUrl = m_CallbackUrlInMemory;

                            m_IsCreatingClient = true;
                            UnityWebRequest newRequest =
                                AppStoreOnboardApi.GenerateUnityClient(Application.cloudProjectId,
                                    unityClientInfo,
                                    callbackUrl);
                            UnityClientResponse clientResp = new UnityClientResponse();
                            RequestStruct newReqStruct = new RequestStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = clientResp;
                            newReqStruct.targetStep = k_StepGetClient;
                            k_RequestQueue.Enqueue(newReqStruct);
                        }

                        if (reqStruct.targetStep == k_StepGetClient)
                        {
                            Log("start fetch userClient");
                            m_IsSearchingClient = true;
                            UnityWebRequest newRequest =
                                AppStoreOnboardApi.GetUnityClientInfo(Application.cloudProjectId);
                            UnityClientResponseWrapper clientRespWrapper = new UnityClientResponseWrapper();
                            RequestStruct newReqStruct = new RequestStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = clientRespWrapper;
                            newReqStruct.targetStep = reqStruct.targetStep;
                            k_RequestQueue.Enqueue(newReqStruct);
                        }
                        else if (reqStruct.targetStep == k_StepUpdateClient)
                        {
                            Log("update userClient");

                            UnityClientInfo unityClientInfo = new UnityClientInfo();
                            unityClientInfo.ClientId = m_UnityClientId.stringValue;
                            string callbackUrl = m_CallbackUrlInMemory;
                            m_IsUpdatingClient = true;
                            UnityWebRequest newRequest =
                                AppStoreOnboardApi.UpdateUnityClient(Application.cloudProjectId, unityClientInfo,
                                    callbackUrl);
                            UnityClientResponse clientResp = new UnityClientResponse();
                            RequestStruct newReqStruct = new RequestStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = clientResp;
                            newReqStruct.targetStep = reqStruct.targetStep;
                            k_RequestQueue.Enqueue(newReqStruct);
                        }
                        else if (reqStruct.targetStep == k_UpdateClientSecret)
                        {
                            Log("update userClient secret");

                            string clientId = m_UnityClientId.stringValue;
                            m_IsUpdatingClient = true;
                            UnityWebRequest newRequest = AppStoreOnboardApi.UpdateUnityClientSecret(clientId);
                            UnityClientResponse clientResp = new UnityClientResponse();
                            RequestStruct newReqStruct = new RequestStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = clientResp;
                            newReqStruct.targetStep = reqStruct.targetStep;
                            k_RequestQueue.Enqueue(newReqStruct);
                        }
                        else if (reqStruct.targetStep == "LinkProject")
                        {
                            Log("link project");
                            m_IsGettingClient = true;
                            UnityWebRequest newRequest =
                                AppStoreOnboardApi.GetUnityClientInfoByClientId(m_ClientIdToBeLinked);
                            UnityClientResponse unityClientResponse = new UnityClientResponse();
                            RequestStruct newReqStruct = new RequestStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = unityClientResponse;
                            newReqStruct.targetStep = reqStruct.targetStep;
                            k_RequestQueue.Enqueue(newReqStruct);
                        }

                        m_SerializedSettings.ApplyModifiedProperties();
                        AssetDatabase.SaveAssets();
                    }

                    else if (resp.GetType() == typeof(UnityClientResponseWrapper))
                    {
                        string raw = "{ \"array\": " + request.downloadHandler.text + "}";
                        resp = JsonUtility.FromJson<UnityClientResponseWrapper>(raw);

                        m_IsSearchingClient = false;

                        // only one element in the list
                        if (((UnityClientResponseWrapper)resp).array.Length > 0)
                        {
                            if (reqStruct.targetStep != null && reqStruct.targetStep == "CheckUpdate")
                            {
                                m_TargetStep = k_StepGetClient;
                                GetAuthCode();
                            }
                            else
                            {
                                UnityClientResponse unityClientResp = ((UnityClientResponseWrapper)resp).array[0];
                                AppStoreOnboardApi.tps = unityClientResp.channel.thirdPartySettings;
                                m_UnityClientId.stringValue = unityClientResp.client_id;
                                m_UnityClientKey.stringValue = unityClientResp.client_secret;
                                m_UnityClientRsaPublicKey.stringValue = unityClientResp.channel.publicRSAKey;
                                m_UnityProjectId.stringValue = unityClientResp.channel.projectGuid;
                                m_ClientSecretInMemory = unityClientResp.channel.channelSecret;
                                m_CallbackUrlInMemory = unityClientResp.channel.callbackUrl;
                                m_CallbackUrlLast = m_CallbackUrlInMemory;
                                AppStoreOnboardApi.updateRev = unityClientResp.rev;
                                AppStoreOnboardApi.loaded = true;
                                m_SerializedSettings.ApplyModifiedProperties();
                                this.SetUpClientSettingsPart();

                                AssetDatabase.SaveAssets();
                                SaveGameSettingsProps(unityClientResp.client_id);

                                m_IsSearchingAppItem = true;
                                UnityWebRequest newRequest = AppStoreOnboardApi.GetAppItem(m_UnityClientId.stringValue);
                                AppItemResponseWrapper appItemResponseWrapper = new AppItemResponseWrapper();
                                RequestStruct newReqStruct = new RequestStruct();
                                newReqStruct.request = newRequest;
                                newReqStruct.resp = appItemResponseWrapper;
                                k_RequestQueue.Enqueue(newReqStruct);
                            }
                        }
                        else
                        {
                            //                            if (reqStruct.targetStep != null &&
                            //                                (reqStruct.targetStep == "LinkProject" || reqStruct.targetStep == "CheckUpdate"))
                            //                            {
                            //                                ResetAllStatusFlag();
                            //                            }
                            //                            else
                            //                            {
                            // no client found, generate one or link to one

                            ResetAllStatusFlag();
                            SetUpLinkProjectWindow();
                            //                            }
                        }
                    }

                    else if (resp.GetType() == typeof(AppItemResponse))
                    {
                        Log("update appItem success");

                        resp = JsonUtility.FromJson<AppItemResponse>(request.downloadHandler.text);
                        m_AppItemId.stringValue = ((AppItemResponse)resp).id;
                        m_AppName.stringValue = ((AppItemResponse)resp).name;
                        m_AppSlug.stringValue = ((AppItemResponse)resp).slug;
                        m_CurrentAppItem.id = ((AppItemResponse)resp).id;
                        m_CurrentAppItem.name = ((AppItemResponse)resp).name;
                        m_CurrentAppItem.slug = ((AppItemResponse)resp).slug;
                        m_CurrentAppItem.ownerId = ((AppItemResponse)resp).ownerId;
                        m_CurrentAppItem.ownerType = ((AppItemResponse)resp).ownerType;
                        m_CurrentAppItem.status = ((AppItemResponse)resp).status;
                        m_CurrentAppItem.type = ((AppItemResponse)resp).type;
                        m_CurrentAppItem.clientId = ((AppItemResponse)resp).clientId;
                        m_CurrentAppItem.packageName = ((AppItemResponse)resp).packageName;
                        m_CurrentAppItem.revision = ((AppItemResponse)resp).revision;
                        m_SerializedSettings.ApplyModifiedProperties();
                        AssetDatabase.SaveAssets();
                        //                        RemovePushRequest(m_CurrentAppItem.id);

                        SetUpBasicInfoPart();
                        RepaintGameIdPart();

                        #region Analytics

                        string eventName = null;
                        if (request.method == UnityWebRequest.kHttpVerbPOST)
                        {
                            eventName = EditorAnalyticsApi.k_AppCreateEventName;
                            m_IsCreatingAppItem = false;
                        }
                        else if (request.method == UnityWebRequest.kHttpVerbPUT)
                        {
                            eventName = EditorAnalyticsApi.k_AppUpdateEventName;
                            m_IsUpdatingAppItem = false;
                        }

                        if (eventName != null)
                        {
                            RequestStruct analyticsReqStruct = new RequestStruct
                            {
                                eventName = eventName,
                                request = EditorAnalyticsApi.AppEvent(eventName, m_CurrentAppItem.clientId,
                                    (AppItemResponse)resp, null),
                                resp = new EventRequestResponse(),
                            };

                            k_RequestQueue.Enqueue(analyticsReqStruct);
                        }

                        #endregion

                        if (reqStruct.targetStep == k_StepUpdateGameTitle)
                        {
                            //                            RemovePushRequest(m_CurrentAppItem.id);
                        }

                        PublishApp(m_AppItemId.stringValue, reqStruct.targetStep);
                    }

                    else if (resp.GetType() == typeof(AppItemPublishResponse))
                    {
                        Log("publish appItem success");
                        m_IsPublishingAppItem = false;
                        AppStoreOnboardApi.loaded = true;
                        resp = JsonUtility.FromJson<AppItemPublishResponse>(request.downloadHandler.text);
                        m_CurrentAppItem.revision = ((AppItemPublishResponse)resp).revision;
                        m_CurrentAppItem.status = "PUBLIC";
                        if (reqStruct.targetStep != k_StepUpdateGameTitle)
                        {
                            ListPlayers();
                        }
                    }

                    else if (resp.GetType() == typeof(AppItemResponseWrapper))
                    {
                        Log("search appItem success");
                        resp = JsonUtility.FromJson<AppItemResponseWrapper>(request.downloadHandler.text);
                        m_IsSearchingAppItem = false;

                        if (((AppItemResponseWrapper)resp).total < 1)
                        {
                            // generate app
                            m_CurrentAppItem.clientId = m_UnityClientId.stringValue;
                            m_CurrentAppItem.name = m_UnityProjectId.stringValue;
                            m_CurrentAppItem.slug = Guid.NewGuid().ToString();
                            m_CurrentAppItem.ownerId = AppStoreOnboardApi.orgId;

                            m_IsCreatingAppItem = true;
                            UnityWebRequest newRequest = AppStoreOnboardApi.CreateAppItem(m_CurrentAppItem);
                            AppItemResponse appItemResponse = new AppItemResponse();
                            RequestStruct newReqStruct = new RequestStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = appItemResponse;
                            k_RequestQueue.Enqueue(newReqStruct);
                        }
                        else
                        {
                            var appItemResp = ((AppItemResponseWrapper)resp).results[0];
                            m_AppName.stringValue = appItemResp.name;
                            m_AppSlug.stringValue = appItemResp.slug;
                            m_AppItemId.stringValue = appItemResp.id;
                            m_CurrentAppItem.id = appItemResp.id;
                            m_CurrentAppItem.name = appItemResp.name;
                            m_CurrentAppItem.slug = appItemResp.slug;
                            m_CurrentAppItem.ownerId = appItemResp.ownerId;
                            m_CurrentAppItem.ownerType = appItemResp.ownerType;
                            m_CurrentAppItem.status = appItemResp.status;
                            m_CurrentAppItem.type = appItemResp.type;
                            m_CurrentAppItem.clientId = appItemResp.clientId;
                            m_CurrentAppItem.packageName = appItemResp.packageName;
                            m_CurrentAppItem.revision = appItemResp.revision;
                            m_SerializedSettings.ApplyModifiedProperties();
                            AssetDatabase.SaveAssets();
                            SetUpBasicInfoPart();
                            RepaintGameIdPart();
                            //RemovePushRequest(m_CurrentAppItem.id);

                            if (appItemResp.status != "PUBLIC")
                            {
                                PublishApp(appItemResp.id, "");
                            }
                            else
                            {
                                AppStoreOnboardApi.loaded = true;
                                ListPlayers();
                            }
                        }
                    }

                    else if (resp.GetType() == typeof(PlayerResponse))
                    {
                        Log("update player");

                        resp = JsonUtility.FromJson<PlayerResponse>(request.downloadHandler.text);

                        var playerId = ((PlayerResponse)resp).id;

                        m_TestAccounts[reqStruct.arrayPosition].playerId = playerId;
                        m_TestAccounts[reqStruct.arrayPosition].password = "******";
                        m_TestAccounts[reqStruct.arrayPosition].deleted = false;
                        RemovePushRequest(m_TestAccounts[reqStruct.arrayPosition].email);
                        k_TestAccountsDirty[reqStruct.arrayPosition] = false;
                        RepaintSingleTestAccount(reqStruct.arrayPosition);

                        UnityWebRequest newRequest = AppStoreOnboardApi.VerifyTestAccount(playerId);
                        PlayerVerifiedResponse playerVerifiedResponse = new PlayerVerifiedResponse();
                        RequestStruct newReqStruct = new RequestStruct();
                        newReqStruct.request = newRequest;
                        newReqStruct.resp = playerVerifiedResponse;
                        newReqStruct.targetStep = null;
                        k_RequestQueue.Enqueue(newReqStruct);
                    }

                    else if (resp.GetType() == typeof(PlayerResponseWrapper))
                    {
                        Log("fetch players success");

                        m_IsSearchingPlayer = false;
                        resp = JsonUtility.FromJson<PlayerResponseWrapper>(request.downloadHandler.text);
                        m_TestAccounts = new List<TestAccount>();

                        if (((PlayerResponseWrapper)resp).total > 0)
                        {
                            var exists = ((PlayerResponseWrapper)resp).results;
                            for (int i = 0; i < exists.Length; i++)
                            {
                                TestAccount existed = new TestAccount
                                {
                                    email = exists[i].nickName,
                                    password = "******",
                                    playerId = exists[i].id
                                };
                                m_TestAccounts.Add(existed);
                                k_TestAccountsDirty.Add(false);
                                k_TestAccountsValidationMsg.Add("");
                            }
                        }

                        this.ClearPlayerPart();
                        this.SetUpPlayerPart();
                    }

                    else if (resp.GetType() == typeof(PlayerVerifiedResponse))
                    {
                        // ListPlayers();
                    }

                    else if (resp.GetType() == typeof(PlayerChangePasswordResponse))
                    {
                        Log("update player password");
                        RemovePushRequest(m_TestAccounts[reqStruct.arrayPosition].email);
                        m_TestAccounts[reqStruct.arrayPosition].password = "******";
                        m_TestAccounts[reqStruct.arrayPosition].deleted = false;
                        k_TestAccountsDirty[reqStruct.arrayPosition] = false;
                        RepaintSingleTestAccount(reqStruct.arrayPosition);
                    }

                    else if (resp.GetType() == typeof(PlayerDeleteResponse))
                    {
                        Log("delete player");

                        m_IsDeletingPlayer = false;
                        EditorUtility.DisplayDialog("Success",
                            "TestAccount " + reqStruct.currTestAccount.playerId + " has been Deleted.", "OK");
                        RemoveTestAccountLocally(reqStruct.arrayPosition);
                        // this.Repaint();
                    }

                    else if (resp.GetType() == typeof(TokenInfo))
                    {
                        m_IsGettingAccessToken = false;
                        resp = JsonUtility.FromJson<TokenInfo>(request.downloadHandler.text);
                        AppStoreOnboardApi.tokenInfo.access_token = ((TokenInfo)resp).access_token;
                        AppStoreOnboardApi.tokenInfo.refresh_token = ((TokenInfo)resp).refresh_token;

                        m_IsGettingUserId = true;
                        UnityWebRequest newRequest = AppStoreOnboardApi.GetUserId();
                        UserIdResponse userIdResp = new UserIdResponse();
                        RequestStruct newReqStruct = new RequestStruct();
                        newReqStruct.request = newRequest;
                        newReqStruct.resp = userIdResp;
                        newReqStruct.targetStep = reqStruct.targetStep;
                        k_RequestQueue.Enqueue(newReqStruct);
                    }

                    else if (resp.GetType() == typeof(UserIdResponse))
                    {
                        m_IsGettingUserId = false;
                        resp = JsonUtility.FromJson<UserIdResponse>(request.downloadHandler.text);
                        AppStoreOnboardApi.userId = ((UserIdResponse)resp).sub;
                        m_IsGettingOrgId = true;
                        UnityWebRequest newRequest = AppStoreOnboardApi.GetOrgId(Application.cloudProjectId);
                        OrgIdResponse orgIdResp = new OrgIdResponse();
                        RequestStruct newReqStruct = new RequestStruct();
                        newReqStruct.request = newRequest;
                        newReqStruct.resp = orgIdResp;
                        newReqStruct.targetStep = reqStruct.targetStep;
                        k_RequestQueue.Enqueue(newReqStruct);
                    }

                    else if (resp.GetType() == typeof(OrgIdResponse))
                    {
                        m_IsGettingOrgId = false;
                        resp = JsonUtility.FromJson<OrgIdResponse>(request.downloadHandler.text);
                        AppStoreOnboardApi.orgId = ((OrgIdResponse)resp).org_foreign_key;
                        //                        UnityWebRequest newRequest = AppStoreOnboardApi.GetOrgRoles();
                        //                        OrgRoleResponse orgRoleResp = new OrgRoleResponse();
                        //                        RequestStruct newReqStruct = new RequestStruct();
                        //                        newReqStruct.request = newRequest;
                        //                        newReqStruct.resp = orgRoleResp;
                        //                        newReqStruct.targetStep = reqStruct.targetStep;
                        //                        k_RequestQueue.Enqueue(newReqStruct);
                    }

                    else if (resp.GetType() == typeof(IapItemSearchResponse))
                    {
                        Debug.LogError("won't reach here.");
                    }

                    else if (resp.GetType() == typeof(IapItemDeleteResponse))
                    {
                        Debug.LogError("won't reach here.");
                    }

                    else if (resp.GetType() == typeof(UnityIapItemUpdateResponse))
                    {
                        Debug.LogError("won't reach here.");
                    }

                    else if (resp.GetType() == typeof(UnityIapItemCreateResponse))
                    {
                        Debug.LogError("won't reach here.");
                    }
                }
            }
            else
            {
                k_RequestQueue.Enqueue(reqStruct);
            }
        }

        private static void CreateSettingsAsset()
        {
            if (File.Exists(AppStoreSettings.appStoreSettingsAssetPath))
            {
                return;
            }

            if (!Directory.Exists(AppStoreSettings.appStoreSettingsAssetFolder))
                Directory.CreateDirectory(AppStoreSettings.appStoreSettingsAssetFolder);

            var appStoreSettings = ScriptableObject.CreateInstance<AppStoreSettings>();
            AssetDatabase.CreateAsset(appStoreSettings, AppStoreSettings.appStoreSettingsAssetPath);
        }

        private void InitializeProperties()
        {
            AppStoreSettings settings =
                AssetDatabase.LoadAssetAtPath<AppStoreSettings>(AppStoreSettings.appStoreSettingsAssetPath);
            if (settings == null)
            {
                Debug.LogError("UDP Settings.asset not found");
                return;
            }

            m_SerializedSettings = new UnityEditor.SerializedObject(settings);
            m_UnityProjectId = m_SerializedSettings.FindProperty("UnityProjectID");
            m_UnityClientId = m_SerializedSettings.FindProperty("UnityClientID");
            m_UnityClientKey = m_SerializedSettings.FindProperty("UnityClientKey");
            m_UnityClientRsaPublicKey = m_SerializedSettings.FindProperty("UnityClientRSAPublicKey");
            m_AppName = m_SerializedSettings.FindProperty("AppName");
            m_AppSlug = m_SerializedSettings.FindProperty("AppSlug");
            m_AppItemId = m_SerializedSettings.FindProperty("AppItemId");

            m_TestAccounts = new List<TestAccount>();
            m_CurrentAppItem = new AppItem();

            if (!string.IsNullOrEmpty(Application.cloudProjectId))
            {
                InitializeSecrets();
            }
            else
            {
                SetUpCannotGetProjectIdWindow();
            }
        }

        private void InitializeSecrets()
        {
            // If client secret is in the memory, We do nothing
            if (!string.IsNullOrEmpty(m_ClientSecretInMemory))
            {
                return;
            }

            // If client ID is not serialized, it means this is a newly created GameSettings.asset
            // We provide a chance to link the project to an existing client
            if (string.IsNullOrEmpty(m_UnityClientId.stringValue))
            {
                m_TargetStep = k_StepGetClient;
                GetAuthCode();
                return;
            }

            // Start initialization.
            m_IsSearchingClient = true;
            UnityWebRequest newRequest = AppStoreOnboardApi.GetUnityClientInfo(Application.cloudProjectId);
            UnityClientResponseWrapper clientRespWrapper = new UnityClientResponseWrapper();
            RequestStruct newReqStruct = new RequestStruct();
            newReqStruct.request = newRequest;
            newReqStruct.resp = clientRespWrapper;
            newReqStruct.targetStep =
                "CheckUpdate";
            k_RequestQueue.Enqueue(newReqStruct);
        }

        private void SetUpOperationsPart()
        {
            m_UDPOperationBlock.Q<Button>(UxmlStrings.k_UDPPullButton).clicked -= OnPullButtonClick;
            m_UDPOperationBlock.Q<Button>(UxmlStrings.k_UDPPullButton).clicked += OnPullButtonClick;

            m_UDPOperationBlock.Q<Button>(UxmlStrings.k_UDPPushButton).clicked -= OnPushButtonClick;
            m_UDPOperationBlock.Q<Button>(UxmlStrings.k_UDPPushButton).clicked += OnPushButtonClick;
        }

        private void SetUpBasicInfoPart()
        {
            m_UDPBasicInfoBlock.Q<TextField>(UxmlStrings.k_UDPGameTitle).SetValueWithoutNotify(m_AppName.stringValue);
            m_UDPBasicInfoBlock.Q<TextField>(UxmlStrings.k_UDPProjectId)
                .SetValueWithoutNotify(m_UnityProjectId.stringValue);

            m_UDPBasicInfoBlock.Q<Button>(UxmlStrings.k_CopyProjectIdButton).clicked -= OnCopyProjectIdButtonClick;
            m_UDPBasicInfoBlock.Q<Button>(UxmlStrings.k_CopyProjectIdButton).clicked += OnCopyProjectIdButtonClick;
        }

        private void RepaintGameIdPart()
        {
            m_UDPClientSettingsBlock.Q<TextField>(UxmlStrings.k_UDPGameId).SetValueWithoutNotify(m_AppSlug.stringValue);
        }

        private void SetUpClientSettingsPart()
        {
            if (m_UpdateClientErrorMsg != "")
            {
                m_UDPClientSettingsBlock.Q<Label>(UxmlStrings.k_UDPClientErrorMessage).text = m_UpdateClientErrorMsg;
            }

            m_UDPClientSettingsBlock.Q<TextField>(UxmlStrings.k_UDPGameId).SetValueWithoutNotify(m_AppSlug.stringValue);
            m_UDPClientSettingsBlock.Q<TextField>(UxmlStrings.k_UDPClientId)
                .SetValueWithoutNotify(m_UnityClientId.stringValue);
            m_UDPClientSettingsBlock.Q<TextField>(UxmlStrings.k_UDPClientKey)
                .SetValueWithoutNotify(m_UnityClientKey.stringValue);
            m_UDPClientSettingsBlock.Q<TextField>(UxmlStrings.k_UDPRSAPublicKey)
                .SetValueWithoutNotify(m_UnityClientRsaPublicKey.stringValue);
            m_UDPClientSettingsBlock.Q<TextField>(UxmlStrings.k_UDPClientSecret)
                .SetValueWithoutNotify(m_ClientSecretInMemory);
            m_UDPClientSettingsBlock.Q<TextField>(UxmlStrings.k_UDPClientCallbackUrl)
                .SetValueWithoutNotify(m_CallbackUrlLast);
        }

        private void SetUpSinglePlayer(int index)
        {
            int pos = index;

            if (m_TestAccounts[pos].deleted)
            {
                return;
            }

            if (m_UDPPlayersContainer == null)
            {
                m_UDPPlayersContainer = m_UDPPlayerBlock.Q(UxmlStrings.k_Players);
            }

            VisualElement playerContainer = new VisualElement();
            playerContainer.name = "player-" + pos;
            playerContainer.AddToClassList("player-container");
            var errTag = new Label(k_TestAccountsValidationMsg[pos]);
            errTag.name = "player-error-" + pos;
            errTag.AddToClassList(UssStrings.k_UssStringsErrorTag);
            errTag.style.height = 12;
            errTag.style.marginLeft = 4;
            errTag.style.display = k_TestAccountsValidationMsg[pos] != "" ? DisplayStyle.Flex : DisplayStyle.None;
            playerContainer.Add(errTag);

            VisualElement player = new VisualElement();
            player.AddToClassList(UssStrings.k_UssStringsPlayer);
            TextField email = new TextField();
            email.name = "player-email-" + pos;
            TextField password = new TextField();
            password.name = "player-password-" + pos;

            email.AddToClassList("player-email");
            password.AddToClassList("player-password");
            email.value = m_TestAccounts[pos].email;
            password.value = m_TestAccounts[pos].password;
            password.isPasswordField = true;

            // add delete btn
            Button delBtn = new Button();
            delBtn.text = "\u2212";
            delBtn.clicked += () =>
            {
                if ((string.IsNullOrEmpty(m_TestAccounts[pos].playerId)))
                {
                    RemoveTestAccountLocally(pos);
                }
                else
                {
                    DeleteTestAccount(m_TestAccounts[pos], pos);
                }
            };
            player.Add(email);
            player.Add(password);
            player.Add(delBtn);

            playerContainer.Add(player);
            m_UDPPlayersContainer.Add(playerContainer);
        }

        private void ClearPlayerPart()
        {
            if (m_UDPPlayersContainer == null)
            {
                m_UDPPlayersContainer = m_UDPPlayerBlock.Q(UxmlStrings.k_Players);
            }

            m_UDPPlayersContainer.Clear();
        }

        private void SetUpPlayerPart()
        {
            m_UDPPlayersContainer = m_UDPPlayerBlock.Q(UxmlStrings.k_Players);

            for (var i = 0; i < m_TestAccounts.Count; i++)
            {
                SetUpSinglePlayer(i);
            }

            m_UDPPlayerBlock.Q<Button>(UxmlStrings.k_AddNewPlayerButton).clicked -= OnAddNewPlayerButtonClick;
            m_UDPPlayerBlock.Q<Button>(UxmlStrings.k_AddNewPlayerButton).clicked += OnAddNewPlayerButtonClick;
        }

        private void SetUpExternalLinkPart()
        {
            var goToDashboard = m_UDPExternalLinkBlock.Q(UxmlStrings.k_GoToUdpDashboard);
            if (goToDashboard != null)
            {
                var clickable = new Clickable(() => { Application.OpenURL(k_UDPDashboardLink); });

                goToDashboard.AddManipulator(clickable);
            }
            var goToIAPCatalog = m_UDPExternalLinkBlock.Q(UxmlStrings.k_GoToIAPCatalog);
            if (goToIAPCatalog != null)
            {
                var clickable = new Clickable(() =>
                {
                    EditorApplication.ExecuteMenuItem("Window/Unity Distribution Portal/IAP Catalog");
                });

                goToIAPCatalog.AddManipulator(clickable);
            }
        }

        #region helper method

        private void GetAuthCode()
        {
            Log("getAuthCode");
            if (AppStoreOnboardApi.tokenInfo.access_token == null)
            {
                m_IsGettingAuthCode = true;
                Type unityOAuthType = Utils.FindTypeByName("UnityEditor.Connect.UnityOAuth");
                Type authCodeResponseType = unityOAuthType.GetNestedType("AuthCodeResponse", BindingFlags.Public);
                var performMethodInfo =
                    typeof(AppStoreProjectSettingsEditor).GetMethod("Perform").MakeGenericMethod(authCodeResponseType);
                var actionT =
                    typeof(Action<>).MakeGenericType(authCodeResponseType); // Action<UnityOAuth.AuthCodeResponse>
                var getAuthorizationCodeAsyncMethodInfo = unityOAuthType.GetMethod("GetAuthorizationCodeAsync");
                var performDelegate = Delegate.CreateDelegate(actionT, this, performMethodInfo);
                try
                {
                    getAuthorizationCodeAsyncMethodInfo.Invoke(null,
                        new object[] { AppStoreOnboardApi.oauthClientId, performDelegate });
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException is InvalidOperationException)
                    {
                        Debug.LogError("[UDP] You must login with Unity ID first.");
                        EditorUtility.DisplayDialog("Error", "You must login with Unity ID first.", "OK");
                        ResetAllStatusFlag();
                        SetUpCannotGetProjectIdWindow();
                    }
                }
            }
            else
            {
                m_IsGettingUserId = true;
                UnityWebRequest request = AppStoreOnboardApi.GetUserId();
                UserIdResponse userIdResp = new UserIdResponse();
                RequestStruct reqStruct = new RequestStruct();
                reqStruct.request = request;
                reqStruct.resp = userIdResp;
                reqStruct.targetStep = m_TargetStep;
                k_RequestQueue.Enqueue(reqStruct);
            }
        }

        public void Perform<T>(T response)
        {
            var authCodePropertyInfo = response.GetType().GetProperty("AuthCode");
            var exceptionPropertyInfo = response.GetType().GetProperty("Exception");
            string authCode = (string)authCodePropertyInfo.GetValue(response, null);
            Exception exception = (Exception)exceptionPropertyInfo.GetValue(response, null);
            m_IsGettingAuthCode = false;
            if (authCode != null)
            {
                m_IsGettingAccessToken = true;
                UnityWebRequest request = AppStoreOnboardApi.GetAccessToken(authCode);
                TokenInfo tokenInfoResp = new TokenInfo();
                RequestStruct reqStruct = new RequestStruct();
                reqStruct.request = request;
                reqStruct.resp = tokenInfoResp;
                reqStruct.targetStep = m_TargetStep;
                k_RequestQueue.Enqueue(reqStruct);
            }
            else
            {
                Debug.LogError("[UDP] " + "Failed: " + exception.ToString());
                EditorUtility.DisplayDialog("Error", "Failed: " + exception.ToString(), "OK");
                ResetAllStatusFlag();
                SetUpCannotGetProjectIdWindow();
            }
        }

        private void ProcessErrorRequest(RequestStruct reqStruct)
        {
            /** if (reqStruct.curIapItem != null)
             {
                 RemovePushRequest(reqStruct.curIapItem.slug);
             }
             **/

            if (reqStruct.resp.GetType() == typeof(UnityClientResponse))
            {
                RemovePushRequest(m_UnityClientId.stringValue);
            }

            else if (reqStruct.resp.GetType() == typeof(AppItemResponse))
            {
                RemovePushRequest(m_CurrentAppItem.id);
            }

            else if (reqStruct.resp.GetType() == typeof(PlayerResponse) ||
                     reqStruct.resp.GetType() == typeof(PlayerChangePasswordResponse))
            {
                RemovePushRequest(m_TestAccounts[reqStruct.arrayPosition].email);
            }
        }

        private void SaveGameSettingsProps(String clientId)
        {
            Log("save game settings");

            if (!Directory.Exists(AppStoreSettings.appStoreSettingsPropFolder))
                Directory.CreateDirectory(AppStoreSettings.appStoreSettingsPropFolder);
            StreamWriter writter = new StreamWriter(AppStoreSettings.appStoreSettingsPropPath, false);
            String warningMessage = "*** DO NOT DELETE OR MODIFY THIS FILE !! ***";
            writter.WriteLine(warningMessage);
            writter.WriteLine(clientId);
            writter.WriteLine(warningMessage);
            writter.Close();
        }

        private void PublishApp(String appItemId, string targetStep)
        {
            Log("publish app");
            m_IsPublishingAppItem = true;
            UnityWebRequest newRequest = AppStoreOnboardApi.PublishAppItem(appItemId);
            AppItemPublishResponse appItemPublishResponse = new AppItemPublishResponse();
            RequestStruct newReqStruct = new RequestStruct();
            newReqStruct.request = newRequest;
            newReqStruct.resp = appItemPublishResponse;
            newReqStruct.targetStep = targetStep;
            k_RequestQueue.Enqueue(newReqStruct);
        }

        private void ListPlayers()
        {
            Log("list players");
            m_IsSearchingPlayer = true;
            UnityWebRequest newRequest = AppStoreOnboardApi.GetTestAccount(m_UnityClientId.stringValue);
            PlayerResponseWrapper playerResponseWrapper = new PlayerResponseWrapper();
            RequestStruct newReqStruct = new RequestStruct();
            newReqStruct.request = newRequest;
            newReqStruct.resp = playerResponseWrapper;
            newReqStruct.targetStep = null;
            k_RequestQueue.Enqueue(newReqStruct);
        }

        private void RemoveTestAccountLocally(int pos)
        {
            m_TestAccounts[pos].deleted = true;
            k_TestAccountsDirty[pos] = false;
            k_TestAccountsValidationMsg[pos] = "";

            RepaintSingleTestAccount(pos);
        }

        private void RepaintSingleTestAccount(int pos)
        {
            var account = m_TestAccounts[pos];
            var accountUi = m_UDPPlayersContainer.Q("player-" + pos);
            if (accountUi != null)
            {
                if (account.deleted)
                {
                    m_UDPPlayersContainer.Remove(accountUi);
                    return;
                }

                var playerAtPosErr = accountUi.Q<Label>("player-error-" + pos);
                if (playerAtPosErr != null)
                {
                    playerAtPosErr.text = k_TestAccountsValidationMsg[pos];
                    playerAtPosErr.style.display =
                        k_TestAccountsValidationMsg[pos] != "" ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
        }

        private void RefreshAllInformation()
        {
            m_TestAccounts.Clear();
            k_TestAccountsDirty.Clear();
            k_TestAccountsValidationMsg.Clear();
            m_UpdateClientErrorMsg = "";
            m_UDPClientSettingsBlock.Q<Label>(UxmlStrings.k_UDPClientErrorMessage).text = m_UpdateClientErrorMsg;
            m_TargetStep = k_StepGetClient;

            GetAuthCode();
        }

        private bool CheckURL(string url)
        {
            string pattern =
                @"^(https?://[\w\-]+(\.[\w\-]+)+(:\d+)?((/[\w\-]*)?)*(\?[\w\-]+=[\w\-]+((&[\w\-]+=[\w\-]+)?)*)?)?$";
            return new Regex(pattern, RegexOptions.IgnoreCase).IsMatch(url);
        }

        private void UpdateCallbackUrl(string url)
        {
            UnityClientInfo unityClientInfo = new UnityClientInfo();
            unityClientInfo.ClientId = m_UnityClientId.stringValue;

            m_IsUpdatingClient = true;
            UnityWebRequest newRequest =
                AppStoreOnboardApi.UpdateUnityClient(Application.cloudProjectId, unityClientInfo,
                    url);
            UnityClientResponse clientResp = new UnityClientResponse();
            RequestStruct newReqStruct = new RequestStruct();
            newReqStruct.request = newRequest;
            newReqStruct.resp = clientResp;
            newReqStruct.targetStep = k_StepUpdateClient;
            k_RequestQueue.Enqueue(newReqStruct);
        }

        private void UpdateGameTitle(string title)
        {
            m_CurrentAppItem.status = "STAGE";
            m_CurrentAppItem.name = title;
            m_IsUpdatingAppItem = true;
            UnityWebRequest newRequest = AppStoreOnboardApi.UpdateAppItem(m_CurrentAppItem);
            AppItemResponse appItemResponse = new AppItemResponse();
            RequestStruct newReqStruct = new RequestStruct();
            newReqStruct.request = newRequest;
            newReqStruct.resp = appItemResponse;
            newReqStruct.targetStep = k_StepUpdateGameTitle;
            k_RequestQueue.Enqueue(newReqStruct);
        }

        private void CreateTestAccount(TestAccount testAccount, int pos)
        {
            Player player = new Player();
            player.email = testAccount.email;
            player.password = testAccount.password;
            UnityWebRequest request =
                AppStoreOnboardApi.SaveTestAccount(player, m_UnityClientId.stringValue);
            PlayerResponse playerResponse = new PlayerResponse();
            RequestStruct reqStruct = new RequestStruct();
            reqStruct.request = request;
            reqStruct.resp = playerResponse;
            reqStruct.targetStep = null;
            reqStruct.arrayPosition = pos;
            k_RequestQueue.Enqueue(reqStruct);
        }

        private void UpdateTestAccount(TestAccount testAccount, int pos)
        {
            PlayerChangePasswordRequest player = new PlayerChangePasswordRequest();
            player.password = testAccount.password;
            player.playerId = testAccount.playerId;
            UnityWebRequest request = AppStoreOnboardApi.UpdateTestAccount(player);
            PlayerChangePasswordResponse playerChangePasswordResponse = new PlayerChangePasswordResponse();
            RequestStruct reqStruct = new RequestStruct();
            reqStruct.request = request;
            reqStruct.resp = playerChangePasswordResponse;
            reqStruct.targetStep = null;
            reqStruct.arrayPosition = pos;
            k_RequestQueue.Enqueue(reqStruct);
        }

        private void DeleteTestAccount(TestAccount account, int pos)
        {
            m_IsDeletingPlayer = true;
            UnityWebRequest request = AppStoreOnboardApi.DeleteTestAccount(account.playerId);
            PlayerDeleteResponse response = new PlayerDeleteResponse();
            RequestStruct reqStruct = new RequestStruct
            {
                request = request,
                resp = response,
                arrayPosition = pos,
                currTestAccount = account
            };
            k_RequestQueue.Enqueue(reqStruct);
        }

        private void OnPullButtonClick()
        {
            GUI.FocusControl(null);
            if (AnythingChanged())
            {
                if (EditorUtility.DisplayDialog("Local changes may be overwritten",
                    "There are pending local edits that will be lost if you pull.",
                    "Pull anyway", "Cancel"))
                {
                    RefreshAllInformation();
                }
            }
            else
            {
                RefreshAllInformation();
            }
        }

        private void OnPushButtonClick()
        {
            // Update UDP Client Settings
            if (CallbackUrlChanged())
            {
                string callbackUrl = m_UDPClientSettingsBlock.Q<TextField>(UxmlStrings.k_UDPClientCallbackUrl).value;

                if (CheckURL(callbackUrl))
                {
                    m_UpdateClientErrorMsg = "";
                    UpdateCallbackUrl(callbackUrl);
                    //                    AddPushRequests(m_UnityClientId.stringValue);
                }
                else
                {
                    //
                    if (callbackUrl.StartsWith("https") || callbackUrl.StartsWith("http"))
                    {
                        m_UpdateClientErrorMsg = "Callback URL is invalid.";
                    }
                    else
                    {
                        m_UpdateClientErrorMsg = "Callback URL is invalid. (http/https is required)";
                    }
                }

                m_UDPClientSettingsBlock.Q<Label>(UxmlStrings.k_UDPClientErrorMessage).text = m_UpdateClientErrorMsg;
            }

            // Update Game Settings

            if (GameTitleChanged())
            {
                string gameTitle = m_UDPBasicInfoBlock.Q<TextField>(UxmlStrings.k_UDPGameTitle).value;
                if (!string.IsNullOrEmpty(gameTitle))
                {
                    m_UpdateGameTitleErrorMsg = "";
                    UpdateGameTitle(gameTitle);
                    //                    AddPushRequests(m_CurrentAppItem.id);
                }
                else
                {
                    m_UpdateGameTitleErrorMsg = "Game title cannot be null";
                }

                m_UDPBasicInfoBlock.Q<Label>(UxmlStrings.k_UDPGameErrorMessage).text = m_UpdateGameTitleErrorMsg;
            }

            // Update Test Accounts
            for (int i = 0; i < m_TestAccounts.Count; i++)
            {
                int pos = i;
                if (k_TestAccountsDirty[pos])
                {
                    m_TestAccounts[pos].email = m_UDPPlayersContainer.Q<TextField>("player-email-" + pos).value;
                    m_TestAccounts[pos].password = m_UDPPlayersContainer.Q<TextField>("player-password-" + pos).value;
                    k_TestAccountsValidationMsg[pos] = m_TestAccounts[pos].Validate();
                    if (k_TestAccountsValidationMsg[pos] == "")
                    {
                        if (string.IsNullOrEmpty(m_TestAccounts[pos].playerId))
                        {
                            CreateTestAccount(m_TestAccounts[pos], pos);
                        }
                        else
                        {
                            UpdateTestAccount(m_TestAccounts[pos], pos);
                        }

                        AddPushRequests(m_TestAccounts[pos].email);
                    }
                    else
                    {
                        RepaintSingleTestAccount(pos);
                        Debug.LogError(
                            "[UDP] TestAccount:" + m_TestAccounts[pos].email + " " +
                            k_TestAccountsValidationMsg[pos]);
                    }
                }
            }
        }

        private void OnCopyProjectIdButtonClick()
        {
            EditorGUIUtility.systemCopyBuffer = m_UnityProjectId.stringValue;
        }

        private void OnAddNewPlayerButtonClick()
        {
            m_TestAccounts.Add(new TestAccount
            {
                email = "Email",
                password = "Password",
                isUpdate = false,
                deleted = false
            });
            k_TestAccountsDirty.Add(true);
            k_TestAccountsValidationMsg.Add("");

            SetUpSinglePlayer(m_TestAccounts.Count - 1);
        }

        private void OnGenerateNewClientButtonClick()
        {
            SetUpServiceWindow();
            m_TargetStep = k_StepGenerateClient;
            GetAuthCode();
        }

        private void OnLinkToExitingClientButtonClick()
        {
            m_ClientIdToBeLinked = m_UDPLinkClientIdBlock.Q<TextField>(UxmlStrings.k_UdpClientIdToBeLinked)?.value;
            if (!string.IsNullOrEmpty(m_ClientIdToBeLinked))
            {
                m_ClientIdToBeLinked = m_ClientIdToBeLinked.Trim();
                UnityWebRequest newRequest =
                    AppStoreOnboardApi.GetUnityClientInfoByClientId(m_ClientIdToBeLinked);
                UnityClientResponse unityClientResponse = new UnityClientResponse();
                RequestStruct newReqStruct = new RequestStruct();
                newReqStruct.request = newRequest;
                newReqStruct.currentStep = "GetClientBeforeLink";
                newReqStruct.resp = unityClientResponse;
                newReqStruct.targetStep = "LinkProject";
                k_RequestQueue.Enqueue(newReqStruct);
            }
        }

        private void SaveToTestAccountStruct()
        {
            m_EasyToSaveAccounts = new List<EasyToSaveAccount>();

            if (m_TestAccounts == null || m_TestAccounts.Count == 0)
            {
                return;
            }

            foreach (var account in m_TestAccounts)
            {
                var local = new EasyToSaveAccount();
                local.email = account.email;
                local.password = account.password;
                local.deleted = account.deleted;
                local.isUpdate = account.isUpdate;
                local.playerId = account.playerId;
                m_EasyToSaveAccounts.Add(local);
            }

            m_TestAccounts = new List<TestAccount>();
        }

        private void RestoreTestAccount()
        {
            m_TestAccounts = new List<TestAccount>();

            if (m_EasyToSaveAccounts == null || m_EasyToSaveAccounts.Count == 0)
            {
                return;
            }

            foreach (var account in m_EasyToSaveAccounts)
            {
                var local = new TestAccount();
                local.email = account.email;
                local.password = account.password;
                local.deleted = account.deleted;
                local.isUpdate = account.isUpdate;
                local.playerId = account.playerId;
                m_TestAccounts.Add(local);
            }

            m_EasyToSaveAccounts = new List<EasyToSaveAccount>();
        }

        private void HideVisualElement(VisualElement visualElement)
        {
            if (visualElement != null)
            {
                visualElement.style.display = DisplayStyle.None;
            }
        }

        private void AddPushRequests(string id)
        {
            k_PushRequestList.Add(id);
        }

        private void RemovePushRequest(string id)
        {
            k_PushRequestList.Remove(id);
        }

        private void ResetAllStatusFlag()
        {
            m_IsGettingAuthCode = false;
            m_IsGettingAccessToken = false;
            m_IsGettingUserId = false;
            m_IsGettingOrgId = false;
            m_IsSearchingClient = false;
            m_IsCreatingClient = false;
            m_IsGettingClient = false;
            m_IsUpdatingClient = false;
            m_IsSearchingAppItem = false;
            m_IsUpdatingAppItem = false;
            m_IsCreatingAppItem = false;
            m_IsPublishingAppItem = false;
            m_IsSearchingPlayer = false;
            m_IsDeletingPlayer = false;
        }

        private bool IsOperationRunning()
        {
            return m_IsGettingAuthCode ||
                   m_IsGettingAccessToken ||
                   m_IsGettingUserId ||
                   m_IsGettingOrgId ||
                   m_IsSearchingClient ||
                   m_IsCreatingClient ||
                   m_IsGettingClient ||
                   m_IsUpdatingClient ||
                   m_IsSearchingAppItem ||
                   m_IsUpdatingAppItem ||
                   m_IsCreatingAppItem ||
                   m_IsPublishingAppItem ||
                   m_IsSearchingPlayer;
        }

        private bool AnythingChanged()
        {
            return GameTitleChanged() || CallbackUrlChanged() || TestAccountsChanged();
        }

        private bool GameTitleChanged()
        {
            var v = m_UDPBasicInfoBlock.Q<TextField>(UxmlStrings.k_UDPGameTitle).value;
            return string.IsNullOrEmpty(v) || !v.Equals(m_AppName.stringValue);
        }

        private bool CallbackUrlChanged()
        {
            var v = m_UDPClientSettingsBlock.Q<TextField>(UxmlStrings.k_UDPClientCallbackUrl).value;
            if (string.IsNullOrEmpty(v) && string.IsNullOrEmpty(m_CallbackUrlLast))
            {
                return false;
            }

            if (v == null)
            {
                v = "";
            }

            return !v.Equals(m_CallbackUrlLast);
        }

        private bool TestAccountsChanged()
        {
            foreach (bool dirty in k_TestAccountsDirty)
            {
                if (dirty)
                {
                    return true;
                }
            }

            return false;
        }

        private void Log(string message)
        {
           // Debug.Log(message);
        }

        #endregion
    }
}

#endif
