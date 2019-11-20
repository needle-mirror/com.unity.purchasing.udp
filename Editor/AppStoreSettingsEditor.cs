using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.UDP.Editor.Analytics;

namespace UnityEngine.UDP.Editor
{

#if (UNITY_5_6_OR_NEWER && !UNITY_5_6_0)
    [CustomEditor(typeof(AppStoreSettings))]
    public class AppStoreSettingsEditor : UnityEditor.Editor
    {
#if (UNITY_2020_1_OR_NEWER)
        [MenuItem("Window/Unity Distribution Portal/IAP Catalog", false, 111)]
        public static void GoToUDPIAPCatalog()
        {
            if (File.Exists(AppStoreSettings.appStoreSettingsAssetPath))
            {
                AppStoreSettings existedAppStoreSettings = CreateInstance<AppStoreSettings>();
                existedAppStoreSettings =
                    (AppStoreSettings)AssetDatabase.LoadAssetAtPath(AppStoreSettings.appStoreSettingsAssetPath,
                        typeof(AppStoreSettings));
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = existedAppStoreSettings;
                // Go to Inspector Window
                EditorApplication.ExecuteMenuItem("Window/General/Inspector");
                return;
            }

            if (!Directory.Exists(AppStoreSettings.appStoreSettingsAssetFolder))
                Directory.CreateDirectory(AppStoreSettings.appStoreSettingsAssetFolder);

            var appStoreSettings = CreateInstance<AppStoreSettings>();
            AssetDatabase.CreateAsset(appStoreSettings, AppStoreSettings.appStoreSettingsAssetPath);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = appStoreSettings;

            // Go to Inspector Window
            EditorApplication.ExecuteMenuItem("Window/General/Inspector");
        }

        [MenuItem("Window/Unity Distribution Portal/Settings", false, 111)]
        public static void GoToUDPSettings()
        {
            // execute service settings window
			SettingsService.OpenProjectSettings("Project/Services/Unity Distribution Portal");
        }

        [MenuItem("Window/Unity Distribution Portal/IAP Catalog", true)]
        [MenuItem("Window/Unity Distribution Portal/Settings", true)]
        public static bool CheckUnityOAuthValidation()
        {
            return k_EnableOAuth;
        }
#else
        [MenuItem("Window/Unity Distribution Portal/Settings", false, 111)]
        public static void CreateAppStoreSettingsAsset()
        {
            if (File.Exists(AppStoreSettings.appStoreSettingsAssetPath))
            {
                AppStoreSettings existedAppStoreSettings = CreateInstance<AppStoreSettings>();
                existedAppStoreSettings =
                    (AppStoreSettings) AssetDatabase.LoadAssetAtPath(AppStoreSettings.appStoreSettingsAssetPath,
                        typeof(AppStoreSettings));
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = existedAppStoreSettings;
                return;
            }

            if (!Directory.Exists(AppStoreSettings.appStoreSettingsAssetFolder))
                Directory.CreateDirectory(AppStoreSettings.appStoreSettingsAssetFolder);

            var appStoreSettings = CreateInstance<AppStoreSettings>();
            AssetDatabase.CreateAsset(appStoreSettings, AppStoreSettings.appStoreSettingsAssetPath);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = appStoreSettings;

            // Go to Inspector Window
#if UNITY_2018_2_OR_NEWER
            EditorApplication.ExecuteMenuItem("Window/General/Inspector");
#else
            EditorApplication.ExecuteMenuItem("Window/Inspector");
#endif
        }

        [MenuItem("Window/Unity Distribution Portal/Settings", true)]
        public static bool CheckUnityOAuthValidation()
        {
            return k_EnableOAuth;
        }
#endif

        string m_ClientSecretInMemory;
        string m_CallbackUrlInMemory;
        List<string> pushRequestList = new List<string>();

        static readonly bool k_EnableOAuth =
            Utils.FindTypeByName("UnityEditor.Connect.UnityOAuth") != null;

        string m_CallbackUrlLast;

        const string k_StepGetClient = "get_client";
        const string k_StepUpdateClient = "update_client";
        const string k_StepUpdateClientSecret = "update_client_secret";
        const string k_StepUpdateGameTitle = "update_game_title";

        List<TestAccount> m_TestAccounts = new List<TestAccount>();
        List<bool> m_TestAccountsDirty = new List<bool>();
        List<string> m_TestAccountsValidationMsg = new List<string>();

        AppItem m_CurrentAppItem;
        string m_TargetStep;
        bool m_ClientChecked;

        public struct ReqStruct
        {
            public string currentStep;
            public string targetStep;
            public string eventName;
            public IapItem curIapItem;
            public TestAccount currTestAccount;
            public int arrayPos;
            public UnityWebRequest request;
            public GeneralResponse resp;
        }

        Queue<ReqStruct> m_RequestQueue = new Queue<ReqStruct>();

        SerializedProperty m_UnityProjectId;
        SerializedProperty m_UnityClientId;
        SerializedProperty m_UnityClientKey;
        SerializedProperty m_UnityClientRsaPublicKey;
        SerializedProperty m_AppName;
        SerializedProperty m_AppSlug;
        SerializedProperty m_AppItemId;

        bool m_IsOperationRunning; // Lock all panels
        bool m_IsIapUpdating ; // Lock iap part.
#if (!UNITY_2020_1_OR_NEWER)
		bool m_IsTestAccountUpdating ; // Lock testAccount part.
#endif
        State m_CurrentState = State.Success;

        void OnEnable()
        {
            // For unity client settings.
            m_UnityProjectId = serializedObject.FindProperty("UnityProjectID");
            m_UnityClientId = serializedObject.FindProperty("UnityClientID");
            m_UnityClientKey = serializedObject.FindProperty("UnityClientKey");
            m_UnityClientRsaPublicKey = serializedObject.FindProperty("UnityClientRSAPublicKey");
            m_AppName = serializedObject.FindProperty("AppName");
            m_AppSlug = serializedObject.FindProperty("AppSlug");
            m_AppItemId = serializedObject.FindProperty("AppItemId");

            m_TestAccounts = new List<TestAccount>();
            m_CurrentAppItem = new AppItem();

            EditorApplication.update += CheckRequestUpdate;

            if (!k_EnableOAuth)
            {
                m_CurrentState = State.CannotUseOAuth;
                return;
            }

            if (!string.IsNullOrEmpty(Application.cloudProjectId))
            {
                m_CloudProjectId = Application.cloudProjectId;
                InitializeSecrets();
            }
            else
            {
                m_CurrentState = State.CannotGetCloudProjectId;
            }
        }


        enum State
        {
            CannotUseOAuth,
            CannotGetCloudProjectId,
            LinkProject,
            Success,
        }

        void CheckRequestUpdate()
        {
            if (m_RequestQueue.Count == 0)
            {
                return;
            }

            ReqStruct reqStruct = m_RequestQueue.Dequeue();
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
                        UnityWebRequest newRequest = AppStoreOnboardApi.RefreshToken();
                        TokenInfo tokenInfoResp = new TokenInfo();
                        ReqStruct newReqStruct = new ReqStruct();
                        newReqStruct.request = newRequest;
                        newReqStruct.resp = tokenInfoResp;
                        newReqStruct.targetStep = reqStruct.targetStep;
                        m_RequestQueue.Enqueue(newReqStruct);
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
                        CallApiAsync();
                    }
                    else
                    {
                        m_IsOperationRunning = false;
                        m_IsIapUpdating = false;
#if (!UNITY_2020_1_OR_NEWER)
                        m_IsTestAccountUpdating = false;
#endif
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

                                ReqStruct analyticsReqStruct = new ReqStruct
                                {
                                    request = analyticsRequest,
                                    resp = new EventRequestResponse(),
                                    eventName = eventName,
                                };

                                m_RequestQueue.Enqueue(analyticsReqStruct);
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

                                ReqStruct analyticsRequestStruct = new ReqStruct
                                {
                                    request = analyticsRequest,
                                    resp = new EventRequestResponse(),
                                    eventName = eventName,
                                };

                                m_RequestQueue.Enqueue(analyticsRequestStruct);
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
                                m_CurrentState = State.CannotGetCloudProjectId;
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

                        this.Repaint();
                    }
                }
                else
                {
                    if (resp.GetType() == typeof(UnityClientResponse))
                    {
                        // LinkProject & Get Role (later action) will result in this response.
                        resp = JsonUtility.FromJson<UnityClientResponse>(request.downloadHandler.text);
                        m_UnityClientId.stringValue = ((UnityClientResponse) resp).client_id;
                        m_UnityClientKey.stringValue = ((UnityClientResponse) resp).client_secret;
                        m_UnityClientRsaPublicKey.stringValue = ((UnityClientResponse) resp).channel.publicRSAKey;
                        m_UnityProjectId.stringValue = ((UnityClientResponse) resp).channel.projectGuid;
                        m_ClientSecretInMemory = ((UnityClientResponse) resp).channel.channelSecret;
                        m_CallbackUrlInMemory = ((UnityClientResponse) resp).channel.callbackUrl;
                        m_CallbackUrlLast = m_CallbackUrlInMemory;
                        AppStoreOnboardApi.tps = ((UnityClientResponse) resp).channel.thirdPartySettings;
                        AppStoreOnboardApi.updateRev = ((UnityClientResponse) resp).rev;
                        serializedObject.ApplyModifiedProperties();
                        this.Repaint();
                        AssetDatabase.SaveAssets();
                        saveGameSettingsProps(((UnityClientResponse) resp).client_id);

                        if (request.method == UnityWebRequest.kHttpVerbPOST) // Generated Client
                        {
                            UnityWebRequest analyticsRequest =
                                EditorAnalyticsApi.ClientEvent(EditorAnalyticsApi.k_ClientCreateEventName,
                                    ((UnityClientResponse) resp).client_id, null);

                            ReqStruct analyticsReqStruct = new ReqStruct
                            {
                                request = analyticsRequest,
                                resp = new EventRequestResponse(),
                                eventName = EditorAnalyticsApi.k_ClientCreateEventName,
                            };

                            m_RequestQueue.Enqueue(analyticsReqStruct);
                        }
                        else if (request.method == UnityWebRequest.kHttpVerbPUT) // Updated Client
                        {
                            UnityWebRequest analyticsRequest =
                                EditorAnalyticsApi.ClientEvent(EditorAnalyticsApi.k_ClientUpdateEventName,
                                    ((UnityClientResponse) resp).client_id, null);

                            ReqStruct analyticsReqStruct = new ReqStruct
                            {
                                request = analyticsRequest,
                                resp = new EventRequestResponse(),
                                eventName = EditorAnalyticsApi.k_ClientUpdateEventName,
                            };

                            m_RequestQueue.Enqueue(analyticsReqStruct);
                        }

                        if (reqStruct.targetStep == "LinkProject")
                        {
                            UnityClientInfo unityClientInfo = new UnityClientInfo();
                            //                            unityClientInfo.ClientId = unityClientID.stringValue;
                            unityClientInfo.ClientId = _clientIdToBeLinked;
                            UnityWebRequest newRequest =
                                AppStoreOnboardApi.UpdateUnityClient(Application.cloudProjectId, unityClientInfo,
                                    m_CallbackUrlInMemory);
                            UnityClientResponse clientResp = new UnityClientResponse();
                            ReqStruct newReqStruct = new ReqStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = clientResp;
                            newReqStruct.targetStep = "GetRole";
                            m_RequestQueue.Enqueue(newReqStruct);
                        }
                        else if (reqStruct.targetStep == "GetRole")
                        {
                            UnityWebRequest newRequest = AppStoreOnboardApi.GetUserId();
                            UserIdResponse userIdResp = new UserIdResponse();
                            ReqStruct newReqStruct = new ReqStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = userIdResp;
                            newReqStruct.targetStep = k_StepGetClient;
                            m_RequestQueue.Enqueue(newReqStruct);
                        }
                        else
                        {
                            if (reqStruct.targetStep == k_StepUpdateClient)
                            {
                                EditorUtility.DisplayDialog("Hint",
                                    "Unity Client updated successfully.",
                                    "OK");
                                RemovePushRequest(m_UnityClientId.stringValue);
#if (!UNITY_2020_1_OR_NEWER)
                                m_CallbackUrlChanged = false;
#endif
                            }

                            if (m_CurrentAppItem.status == "STAGE")
                            {
                                UnityWebRequest newRequest = AppStoreOnboardApi.UpdateAppItem(m_CurrentAppItem);
                                AppItemResponse appItemResponse = new AppItemResponse();
                                ReqStruct newReqStruct = new ReqStruct();
                                newReqStruct.request = newRequest;
                                newReqStruct.resp = appItemResponse;
                                m_RequestQueue.Enqueue(newReqStruct);
                            }
                            else
                            {
                                UnityWebRequest newRequest = AppStoreOnboardApi.GetAppItem(m_UnityClientId.stringValue);
                                AppItemResponseWrapper appItemResponseWrapper = new AppItemResponseWrapper();
                                ReqStruct newReqStruct = new ReqStruct();
                                newReqStruct.request = newRequest;
                                newReqStruct.resp = appItemResponseWrapper;
                                m_RequestQueue.Enqueue(newReqStruct);
                            }
                        }
                    }
                    else if (resp.GetType() == typeof(UserIdResponse))
                    {
                        resp = JsonUtility.FromJson<UserIdResponse>(request.downloadHandler.text);
                        AppStoreOnboardApi.userId = ((UserIdResponse) resp).sub;
                        UnityWebRequest newRequest = AppStoreOnboardApi.GetOrgId(Application.cloudProjectId);
                        OrgIdResponse orgIdResp = new OrgIdResponse();
                        ReqStruct newReqStruct = new ReqStruct();
                        newReqStruct.request = newRequest;
                        newReqStruct.resp = orgIdResp;
                        newReqStruct.targetStep = reqStruct.targetStep;
                        m_RequestQueue.Enqueue(newReqStruct);
                    }
                    else if (resp.GetType() == typeof(OrgIdResponse))
                    {
                        resp = JsonUtility.FromJson<OrgIdResponse>(request.downloadHandler.text);
                        AppStoreOnboardApi.orgId = ((OrgIdResponse) resp).org_foreign_key;

                        if (reqStruct.targetStep == k_StepGetClient)
                        {
                            UnityWebRequest newRequest =
                                AppStoreOnboardApi.GetUnityClientInfo(Application.cloudProjectId);
                            UnityClientResponseWrapper clientRespWrapper = new UnityClientResponseWrapper();
                            ReqStruct newReqStruct = new ReqStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = clientRespWrapper;
                            newReqStruct.targetStep = reqStruct.targetStep;
                            m_RequestQueue.Enqueue(newReqStruct);
                        }
                        else if (reqStruct.targetStep == k_StepUpdateClient)
                        {
                            UnityClientInfo unityClientInfo = new UnityClientInfo();
                            unityClientInfo.ClientId = m_UnityClientId.stringValue;
                            string callbackUrl = m_CallbackUrlInMemory;
                            UnityWebRequest newRequest =
                                AppStoreOnboardApi.UpdateUnityClient(Application.cloudProjectId, unityClientInfo,
                                    callbackUrl);
                            UnityClientResponse clientResp = new UnityClientResponse();
                            ReqStruct newReqStruct = new ReqStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = clientResp;
                            newReqStruct.targetStep = reqStruct.targetStep;
                            m_RequestQueue.Enqueue(newReqStruct);
                        }
                        else if (reqStruct.targetStep == k_StepUpdateClientSecret)
                        {
                            string clientId = m_UnityClientId.stringValue;
                            UnityWebRequest newRequest = AppStoreOnboardApi.UpdateUnityClientSecret(clientId);
                            UnityClientResponse clientResp = new UnityClientResponse();
                            ReqStruct newReqStruct = new ReqStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = clientResp;
                            newReqStruct.targetStep = reqStruct.targetStep;
                            m_RequestQueue.Enqueue(newReqStruct);
                        }
                        else if (reqStruct.targetStep == "LinkProject")
                        {
                            UnityWebRequest newRequest =
                                AppStoreOnboardApi.GetUnityClientInfoByClientId(_clientIdToBeLinked);
                            UnityClientResponse unityClientResponse = new UnityClientResponse();
                            ReqStruct newReqStruct = new ReqStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = unityClientResponse;
                            newReqStruct.targetStep = reqStruct.targetStep;
                            m_RequestQueue.Enqueue(newReqStruct);
                        }

                        serializedObject.ApplyModifiedProperties();
                        AssetDatabase.SaveAssets();
                    }
                    else if (resp.GetType() == typeof(UnityClientResponseWrapper))
                    {
                        string raw = "{ \"array\": " + request.downloadHandler.text + "}";
                        resp = JsonUtility.FromJson<UnityClientResponseWrapper>(raw);
                        // only one element in the list
                        if (((UnityClientResponseWrapper) resp).array.Length > 0)
                        {
                            if (reqStruct.targetStep != null && reqStruct.targetStep == "CheckUpdate")
                            {
                                m_TargetStep = k_StepGetClient;
                                CallApiAsync();
                            }
                            else
                            {
                                UnityClientResponse unityClientResp = ((UnityClientResponseWrapper) resp).array[0];
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
                                serializedObject.ApplyModifiedProperties();
                                this.Repaint();
                                AssetDatabase.SaveAssets();
                                saveGameSettingsProps(unityClientResp.client_id);
                                UnityWebRequest newRequest = AppStoreOnboardApi.GetAppItem(m_UnityClientId.stringValue);
                                AppItemResponseWrapper appItemResponseWrapper = new AppItemResponseWrapper();
                                ReqStruct newReqStruct = new ReqStruct();
                                newReqStruct.request = newRequest;
                                newReqStruct.resp = appItemResponseWrapper;
                                m_RequestQueue.Enqueue(newReqStruct);
                            }
                        }
                        else
                        {
                            if (reqStruct.targetStep != null &&
                                (reqStruct.targetStep == "LinkProject" || reqStruct.targetStep == "CheckUpdate"))
                            {
                                m_IsOperationRunning = false;
                            }
                            // no client found, generate one or link to one
                            else
                            {
                                if (!m_ClientChecked)
                                {
                                    m_CurrentState = State.LinkProject;
                                    m_ClientChecked = true;
                                    m_IsOperationRunning = false;
                                    Repaint();
                                }
                                else
                                {
                                    UnityClientInfo unityClientInfo = new UnityClientInfo();
                                    string callbackUrl = m_CallbackUrlInMemory;
                                    UnityWebRequest newRequest =
                                        AppStoreOnboardApi.GenerateUnityClient(Application.cloudProjectId,
                                            unityClientInfo,
                                            callbackUrl);
                                    UnityClientResponse clientResp = new UnityClientResponse();
                                    ReqStruct newReqStruct = new ReqStruct();
                                    newReqStruct.request = newRequest;
                                    newReqStruct.resp = clientResp;
                                    newReqStruct.targetStep = reqStruct.targetStep;
                                    m_RequestQueue.Enqueue(newReqStruct);
                                }
                            }
                        }
                    }
                    else if (resp.GetType() == typeof(UnityClientResponse))
                    {
                        resp = JsonUtility.FromJson<UnityClientResponse>(request.downloadHandler.text);
                        m_UnityClientId.stringValue = ((UnityClientResponse) resp).client_id;
                        m_UnityClientKey.stringValue = ((UnityClientResponse) resp).client_secret;
                        m_UnityClientRsaPublicKey.stringValue = ((UnityClientResponse) resp).channel.publicRSAKey;
                        m_UnityProjectId.stringValue = ((UnityClientResponse) resp).channel.projectGuid;
                        m_ClientSecretInMemory = ((UnityClientResponse) resp).channel.channelSecret;
                        m_CallbackUrlInMemory = ((UnityClientResponse) resp).channel.callbackUrl;
                        m_CallbackUrlLast = m_CallbackUrlInMemory;
                        AppStoreOnboardApi.tps = ((UnityClientResponse) resp).channel.thirdPartySettings;
                        AppStoreOnboardApi.updateRev = ((UnityClientResponse) resp).rev;
                        serializedObject.ApplyModifiedProperties();
                        this.Repaint();
                        AssetDatabase.SaveAssets();
                        saveGameSettingsProps(((UnityClientResponse) resp).client_id);

                        if (request.method == UnityWebRequest.kHttpVerbPOST) // Generated Client
                        {
                            UnityWebRequest analyticsRequest =
                                EditorAnalyticsApi.ClientEvent(EditorAnalyticsApi.k_ClientCreateEventName,
                                    ((UnityClientResponse) resp).client_id, null);

                            ReqStruct analyticsReqStruct = new ReqStruct
                            {
                                request = analyticsRequest,
                                resp = new EventRequestResponse(),
                                eventName = EditorAnalyticsApi.k_ClientCreateEventName,
                            };

                            m_RequestQueue.Enqueue(analyticsReqStruct);
                        }
                        else if (request.method == UnityWebRequest.kHttpVerbPUT) // Updated Client
                        {
                            UnityWebRequest analyticsRequest =
                                EditorAnalyticsApi.ClientEvent(EditorAnalyticsApi.k_ClientUpdateEventName,
                                    ((UnityClientResponse) resp).client_id, null);

                            ReqStruct analyticsReqStruct = new ReqStruct
                            {
                                request = analyticsRequest,
                                resp = new EventRequestResponse(),
                                eventName = EditorAnalyticsApi.k_ClientUpdateEventName,
                            };

                            m_RequestQueue.Enqueue(analyticsReqStruct);
                        }

                        if (reqStruct.targetStep == "LinkProject")
                        {
                            UnityClientInfo unityClientInfo = new UnityClientInfo();
                            unityClientInfo.ClientId = m_UnityClientId.stringValue;
                            UnityWebRequest newRequest =
                                AppStoreOnboardApi.UpdateUnityClient(Application.cloudProjectId, unityClientInfo,
                                    m_CallbackUrlInMemory);
                            UnityClientResponse clientResp = new UnityClientResponse();
                            ReqStruct newReqStruct = new ReqStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = clientResp;
                            newReqStruct.targetStep = "GetRole";
                            m_RequestQueue.Enqueue(newReqStruct);
                        }
                        else if (reqStruct.targetStep == "GetRole")
                        {
                            UnityWebRequest newRequest = AppStoreOnboardApi.GetUserId();
                            UserIdResponse userIdResp = new UserIdResponse();
                            ReqStruct newReqStruct = new ReqStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = userIdResp;
                            newReqStruct.targetStep = k_StepGetClient;
                            m_RequestQueue.Enqueue(newReqStruct);
                        }
                        else
                        {
                            if (reqStruct.targetStep == k_StepUpdateClient)
                            {
                                EditorUtility.DisplayDialog("Hint",
                                    "Unity Client updated successfully.",
                                    "OK");
                            }

                            if (m_CurrentAppItem.status == "STAGE")
                            {
                                UnityWebRequest newRequest = AppStoreOnboardApi.UpdateAppItem(m_CurrentAppItem);
                                AppItemResponse appItemResponse = new AppItemResponse();
                                ReqStruct newReqStruct = new ReqStruct();
                                newReqStruct.request = newRequest;
                                newReqStruct.resp = appItemResponse;
                                m_RequestQueue.Enqueue(newReqStruct);
                            }
                            else
                            {
                                UnityWebRequest newRequest = AppStoreOnboardApi.GetAppItem(m_UnityClientId.stringValue);
                                AppItemResponseWrapper appItemResponseWrapper = new AppItemResponseWrapper();
                                ReqStruct newReqStruct = new ReqStruct();
                                newReqStruct.request = newRequest;
                                newReqStruct.resp = appItemResponseWrapper;
                                m_RequestQueue.Enqueue(newReqStruct);
                            }
                        }
                    }
                    else if (resp.GetType() == typeof(AppItemResponse))
                    {
                        resp = JsonUtility.FromJson<AppItemResponse>(request.downloadHandler.text);
                        m_AppItemId.stringValue = ((AppItemResponse) resp).id;
                        m_AppName.stringValue = ((AppItemResponse) resp).name;
                        m_AppSlug.stringValue = ((AppItemResponse) resp).slug;
                        m_CurrentAppItem.id = ((AppItemResponse) resp).id;
                        m_CurrentAppItem.name = ((AppItemResponse) resp).name;
                        m_CurrentAppItem.slug = ((AppItemResponse) resp).slug;
                        m_CurrentAppItem.ownerId = ((AppItemResponse) resp).ownerId;
                        m_CurrentAppItem.ownerType = ((AppItemResponse) resp).ownerType;
                        m_CurrentAppItem.status = ((AppItemResponse) resp).status;
                        m_CurrentAppItem.type = ((AppItemResponse) resp).type;
                        m_CurrentAppItem.clientId = ((AppItemResponse) resp).clientId;
                        m_CurrentAppItem.packageName = ((AppItemResponse) resp).packageName;
                        m_CurrentAppItem.revision = ((AppItemResponse) resp).revision;
                        serializedObject.ApplyModifiedProperties();
                        AssetDatabase.SaveAssets();

    #region Analytics

                        string eventName = null;
                        if (request.method == UnityWebRequest.kHttpVerbPOST)
                        {
                            eventName = EditorAnalyticsApi.k_AppCreateEventName;
                        }
                        else if (request.method == UnityWebRequest.kHttpVerbPUT)
                        {
                            eventName = EditorAnalyticsApi.k_AppUpdateEventName;
                        }

                        if (eventName != null)
                        {
                            ReqStruct analyticsReqStruct = new ReqStruct
                            {
                                eventName = eventName,
                                request = EditorAnalyticsApi.AppEvent(eventName, m_CurrentAppItem.clientId,
                                    (AppItemResponse) resp, null),
                                resp = new EventRequestResponse(),
                            };

                            m_RequestQueue.Enqueue(analyticsReqStruct);
                        }

    #endregion

                        if (reqStruct.targetStep == k_StepUpdateGameTitle)
                        {
#if (!UNITY_2020_1_OR_NEWER)
                            m_GameTitleChanged = false;
#endif
                            RemovePushRequest(m_CurrentAppItem.id);
                        }

                        this.Repaint();
                        PublishApp(m_AppItemId.stringValue, reqStruct.targetStep);
                    }
                    else if (resp.GetType() == typeof(AppItemPublishResponse))
                    {
                        AppStoreOnboardApi.loaded = true;
                        resp = JsonUtility.FromJson<AppItemPublishResponse>(request.downloadHandler.text);
                        m_CurrentAppItem.revision = ((AppItemPublishResponse) resp).revision;
                        m_CurrentAppItem.status = "PUBLIC";
                        if (!(reqStruct.targetStep == k_StepUpdateGameTitle))
                        {
#if (UNITY_2020_1_OR_NEWER)
                            PullIAPItems();
#else
                              ListPlayers();
#endif
                        }

                        this.Repaint();
                    }
                    else if (resp.GetType() == typeof(AppItemResponseWrapper))
                    {
                        resp = JsonUtility.FromJson<AppItemResponseWrapper>(request.downloadHandler.text);
                        if (((AppItemResponseWrapper) resp).total < 1)
                        {
                            // generate app
                            m_CurrentAppItem.clientId = m_UnityClientId.stringValue;
                            m_CurrentAppItem.name = m_UnityProjectId.stringValue;
                            m_CurrentAppItem.slug = Guid.NewGuid().ToString();
                            m_CurrentAppItem.ownerId = AppStoreOnboardApi.orgId;
                            UnityWebRequest newRequest = AppStoreOnboardApi.CreateAppItem(m_CurrentAppItem);
                            AppItemResponse appItemResponse = new AppItemResponse();
                            ReqStruct newReqStruct = new ReqStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = appItemResponse;
                            m_RequestQueue.Enqueue(newReqStruct);
                        }
                        else
                        {
                            var appItemResp = ((AppItemResponseWrapper) resp).results[0];
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
                            serializedObject.ApplyModifiedProperties();
                            AssetDatabase.SaveAssets();
                            this.Repaint();

                            if (appItemResp.status != "PUBLIC")
                            {
                                PublishApp(appItemResp.id, "");
                            }
                            else
                            {
                                AppStoreOnboardApi.loaded = true;
#if (UNITY_2020_1_OR_NEWER)
                            PullIAPItems();
#else
                                ListPlayers();
#endif
                            }
                        }
                    }
                    else if (resp.GetType() == typeof(PlayerResponse))
                    {
                        resp = JsonUtility.FromJson<PlayerResponse>(request.downloadHandler.text);

                        var playerId = ((PlayerResponse) resp).id;

                        m_TestAccounts[reqStruct.arrayPos].playerId = playerId;
                        m_TestAccounts[reqStruct.arrayPos].password = "******";
                        RemovePushRequest(m_TestAccounts[reqStruct.arrayPos].email);
                        m_TestAccountsDirty[reqStruct.arrayPos] = false;
                        this.Repaint();

                        UnityWebRequest newRequest = AppStoreOnboardApi.VerifyTestAccount(playerId);
                        PlayerVerifiedResponse playerVerifiedResponse = new PlayerVerifiedResponse();
                        ReqStruct newReqStruct = new ReqStruct();
                        newReqStruct.request = newRequest;
                        newReqStruct.resp = playerVerifiedResponse;
                        newReqStruct.targetStep = null;
                        m_RequestQueue.Enqueue(newReqStruct);
                    }
                    else if (resp.GetType() == typeof(PlayerResponseWrapper))
                    {
                        resp = JsonUtility.FromJson<PlayerResponseWrapper>(request.downloadHandler.text);
                        m_TestAccounts = new List<TestAccount>();
                        AppStoreStyles.kTestAccountBoxHeight = 25;
                        if (((PlayerResponseWrapper) resp).total > 0)
                        {
                            var exists = ((PlayerResponseWrapper) resp).results;
                            for (int i = 0; i < exists.Length; i++)
                            {
                                TestAccount existed = new TestAccount
                                {
                                    email = exists[i].nickName,
                                    password = "******",
                                    playerId = exists[i].id
                                };
                                m_TestAccounts.Add(existed);
                                m_TestAccountsDirty.Add(false);
                                m_TestAccountsValidationMsg.Add("");
                                AppStoreStyles.kTestAccountBoxHeight += 22;
                            }

                            this.Repaint();
                        }

                        PullIAPItems();
                    }
                    else if (resp.GetType() == typeof(PlayerVerifiedResponse))
                    {
                        // ListPlayers();
                    }
                    else if (resp.GetType() == typeof(PlayerChangePasswordResponse))
                    {
                        RemovePushRequest(m_TestAccounts[reqStruct.arrayPos].email);
                        m_TestAccounts[reqStruct.arrayPos].password = "******";
                        m_TestAccountsDirty[reqStruct.arrayPos] = false;
                        this.Repaint();
                        // ListPlayers();
                    }
                    else if (resp.GetType() == typeof(PlayerDeleteResponse))
                    {
#if (!UNITY_2020_1_OR_NEWER)
		                        m_IsTestAccountUpdating = false;
#endif
                        EditorUtility.DisplayDialog("Success",
                            "TestAccount " + reqStruct.currTestAccount.playerId + " has been Deleted.", "OK");
                        RemoveTestAccountLocally(reqStruct.arrayPos);
                        this.Repaint();
                    }
                    else if (resp.GetType() == typeof(TokenInfo))
                    {
                        resp = JsonUtility.FromJson<TokenInfo>(request.downloadHandler.text);
                        AppStoreOnboardApi.tokenInfo.access_token = ((TokenInfo) resp).access_token;
                        AppStoreOnboardApi.tokenInfo.refresh_token = ((TokenInfo) resp).refresh_token;
                        UnityWebRequest newRequest = AppStoreOnboardApi.GetUserId();
                        UserIdResponse userIdResp = new UserIdResponse();
                        ReqStruct newReqStruct = new ReqStruct();
                        newReqStruct.request = newRequest;
                        newReqStruct.resp = userIdResp;
                        newReqStruct.targetStep = reqStruct.targetStep;
                        m_RequestQueue.Enqueue(newReqStruct);
                    }
                    else if (resp.GetType() == typeof(UserIdResponse))
                    {
                        resp = JsonUtility.FromJson<UserIdResponse>(request.downloadHandler.text);
                        AppStoreOnboardApi.userId = ((UserIdResponse) resp).sub;
                        UnityWebRequest newRequest = AppStoreOnboardApi.GetOrgId(Application.cloudProjectId);
                        OrgIdResponse orgIdResp = new OrgIdResponse();
                        ReqStruct newReqStruct = new ReqStruct();
                        newReqStruct.request = newRequest;
                        newReqStruct.resp = orgIdResp;
                        newReqStruct.targetStep = reqStruct.targetStep;
                        m_RequestQueue.Enqueue(newReqStruct);
                    }
                    else if (resp.GetType() == typeof(OrgIdResponse))
                    {
                        resp = JsonUtility.FromJson<OrgIdResponse>(request.downloadHandler.text);
                        AppStoreOnboardApi.orgId = ((OrgIdResponse) resp).org_foreign_key;
                        UnityWebRequest newRequest = AppStoreOnboardApi.GetOrgRoles();
                        OrgRoleResponse orgRoleResp = new OrgRoleResponse();
                        ReqStruct newReqStruct = new ReqStruct();
                        newReqStruct.request = newRequest;
                        newReqStruct.resp = orgRoleResp;
                        newReqStruct.targetStep = reqStruct.targetStep;
                        m_RequestQueue.Enqueue(newReqStruct);
                    }
                    else if (resp.GetType() == typeof(IapItemSearchResponse))
                    {
                        IapItemSearchResponse response =
                            JsonUtility.FromJson<IapItemSearchResponse>(
                                HandleIapItemResponse(request.downloadHandler.text));

                        ClearIapItems();
                        for (int i = 0; i < response.total; i++)
                        {
                            AddIapItem(response.results[i], false);
                        }

                        m_IsOperationRunning = false;
                        m_CurrentState = State.Success;
                        this.Repaint();
                    }
                    else if (resp.GetType() == typeof(IapItemDeleteResponse))
                    {
                        m_IsIapUpdating = false;
                        EditorUtility.DisplayDialog("Success",
                            "Product " + reqStruct.curIapItem.slug + " has been Deleted.", "OK");
                        RemoveIapItemLocally(reqStruct.arrayPos);
                        this.Repaint();
                    }
                    else if (resp.GetType() == typeof(UnityIapItemUpdateResponse))
                    {
                        ProcessIAPResponse(reqStruct, request, EditorAnalyticsApi.k_IapUpdateEventName);
                    }
                    else if (resp.GetType() == typeof(UnityIapItemCreateResponse))
                    {
                        ProcessIAPResponse(reqStruct, request, EditorAnalyticsApi.k_IapCreateEventName);
                    }
                }
            }
            else
            {
                m_RequestQueue.Enqueue(reqStruct);
            }
        }

        void InitializeSecrets()
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
                m_IsOperationRunning = true;
                m_TargetStep = k_StepGetClient;
                CallApiAsync();
                return;
            }

            // Start initialization.
            m_IsOperationRunning = true;
            UnityWebRequest newRequest = AppStoreOnboardApi.GetUnityClientInfo(Application.cloudProjectId);
            UnityClientResponseWrapper clientRespWrapper = new UnityClientResponseWrapper();
            ReqStruct newReqStruct = new ReqStruct();
            newReqStruct.request = newRequest;
            newReqStruct.resp = clientRespWrapper;
            newReqStruct.targetStep =
                "CheckUpdate";
            m_RequestQueue.Enqueue(newReqStruct);
        }

        private string m_CloudProjectId;
        private string m_ShowingMsg;
        private bool m_InAppPurchaseFoldout = true;
        private List<bool> m_IapItemsFoldout = new List<bool>(); // TODO: Update this by API
        private List<IapItem> m_IapItems = new List<IapItem>();
        private List<bool> m_IapItemDirty = new List<bool>();
        private List<string> m_IapValidationMsg = new List<string>();
        private readonly string[] m_IapItemTypeOptions = {"Consumable", "Non consumable"};
        private string _clientIdToBeLinked = "UDP client ID";
#if (!UNITY_2020_1_OR_NEWER)
        private bool m_CallbackUrlChanged;
        private bool m_UdpClientSettingsFoldout ;
		private bool m_GameTitleChanged ;
		private string m_UpdateGameTitleErrorMsg ;
        private bool m_TestAccountFoldout ;
        private string m_UpdateClientErrorMsg ;
#endif

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label) {wordWrap = true};
            EditorGUI.BeginDisabledGroup(m_IsOperationRunning || pushRequestList.Count > 0);

            switch (m_CurrentState)
            {
                case State.CannotUseOAuth:
                    m_ShowingMsg =
                        "UDP editor extension can only work on Unity 5.6.1+. Please check your Unity version and retry.";
                    EditorGUILayout.LabelField(m_ShowingMsg, new GUIStyle(GUI.skin.label) {wordWrap = true});
                    break;
                case State.CannotGetCloudProjectId:
                    m_ShowingMsg =
                        "To use the Unity distribution portal your project will need a Unity project ID. You can create a new project ID or link to an existing one in the Services window.";
                    EditorGUILayout.LabelField(m_ShowingMsg, new GUIStyle(GUI.skin.label) {wordWrap = true});

                    if (GUILayout.Button("Go to the Services Window"))
                    {
#if UNITY_2018_2_OR_NEWER
                        EditorApplication.ExecuteMenuItem("Window/General/Services");
#else
                        EditorApplication.ExecuteMenuItem("Window/Services");
#endif
                        Selection.activeObject = null;
                    }

                    break;
                case State.LinkProject:
                    EditorGUILayout.LabelField("Your project must be linked to a UDP client.", labelStyle);
                    EditorGUILayout.LabelField(
                        "If you're starting your UDP project here, generate a new UDP client now.", labelStyle);
                    EditorGUILayout.LabelField(
                        "If your game client was created from the UDP portal, link it to your project using the client ID.",
                        labelStyle);

                    float labelWidth = Math.Max(EditorGUIUtility.currentViewWidth / 2, 180);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Generate new UDP client", GUILayout.Width(labelWidth)))
                    {
                        m_IsOperationRunning = true;
                        m_TargetStep = k_StepGetClient;
                        CallApiAsync();
                    }

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.LabelField("or",
                        new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter});

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    _clientIdToBeLinked = EditorGUILayout.TextField(_clientIdToBeLinked, GUILayout.Width(labelWidth));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Link to existing UDP client", GUILayout.Width(labelWidth)))
                    {
                        if (!string.IsNullOrEmpty(_clientIdToBeLinked))
                        {
                            m_IsOperationRunning = true;
                            UnityWebRequest newRequest =
                                AppStoreOnboardApi.GetUnityClientInfoByClientId(_clientIdToBeLinked);
                            UnityClientResponse unityClientResponse = new UnityClientResponse();
                            ReqStruct newReqStruct = new ReqStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = unityClientResponse;
                            newReqStruct.targetStep = "LinkProject";
                            m_RequestQueue.Enqueue(newReqStruct);
                        }
                    }

                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();


                    break;
                case State.Success: // Main Interface
                    // Main Display
                    EditorGUILayout.LabelField("UDP Settings.asset DOES NOT store your changes locally.", labelStyle);
                    EditorGUILayout.LabelField("'Push' will save your changes to the UDP server.", labelStyle);
                    EditorGUILayout.LabelField("'Pull' will retrieve your settings from the UDP server.", labelStyle);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Pull", GUILayout.Width(AppStoreStyles.kAppStoreSettingsButtonWidth)))
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

                    if (GUILayout.Button("Push", GUILayout.Width(AppStoreStyles.kAppStoreSettingsButtonWidth)))
                    {
                        // Slug check locally
                        var slugs = new HashSet<String>();

                        // Update IAP Items
                        for (int i = 0; i < m_IapItemDirty.Count; i++)
                        {
                            int pos = i;
                            if (m_IapItemDirty[pos])
                            {
                                //Check validation
                                m_IapValidationMsg[pos] = m_IapItems[pos].Validate();
                                if (string.IsNullOrEmpty(m_IapValidationMsg[pos]))
                                {
                                    m_IapValidationMsg[pos] = m_IapItems[pos].SlugValidate(slugs);
                                }

                                if (string.IsNullOrEmpty(m_IapValidationMsg[pos]))
                                {
                                    // If check succeeds
                                    if (!string.IsNullOrEmpty(m_IapItems[pos].id))
                                    {
                                        UpdateIAPItem(m_IapItems[pos], pos);
                                    }
                                    else
                                    {
                                        CreateIAPItem(m_IapItems[pos], pos);
                                    }

                                    AddPushRequests(m_IapItems[pos].slug);
                                }
                                else
                                {
                                    Debug.LogError(
                                        "[UDP] Iap:" + m_IapItems[pos].slug + " " + m_IapValidationMsg[pos]);
                                }
                            }
                        }

#if (!UNITY_2020_1_OR_NEWER)
                        // Update UDP Client Settings
                        if (m_CallbackUrlChanged)
                        {
                            if (CheckURL(m_CallbackUrlLast))
                            {
                                m_UpdateClientErrorMsg = "";
                                UpdateCallbackUrl();
                                AddPushRequests(m_UnityClientId.stringValue);
                            }
                            else
                            {
                                m_UpdateClientErrorMsg = "Callback URL is invalid. (http/https is required)";
                            }
                        }

                        // Update Game Settings
                        if (m_GameTitleChanged)
                        {
                            if (!string.IsNullOrEmpty(m_CurrentAppItem.name))
                            {
                                m_UpdateGameTitleErrorMsg = "";
                                UpdateGameTitle();
                                AddPushRequests(m_CurrentAppItem.id);
                            }
                            else
                            {
                                m_UpdateGameTitleErrorMsg = "Game title cannot be null";
                            }
                        }

                        // Update Test Accounts
                        for (int i = 0; i < m_TestAccounts.Count; i++)
                        {
                            int pos = i;
                            if (m_TestAccountsDirty[pos])
                            {
                                m_TestAccountsValidationMsg[pos] = m_TestAccounts[pos].Validate();
                                if (string.IsNullOrEmpty(m_TestAccountsValidationMsg[pos]))
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
                                    Debug.LogError(
                                        "[UDP] TestAccount:" + m_TestAccounts[pos].email + " " + m_TestAccountsValidationMsg[pos]);
                                }
                            }
                        }
#endif
                        this.Repaint();
                    }

                    EditorGUILayout.EndHorizontal();

                    GuiLine();

#if (!UNITY_2020_1_OR_NEWER)
    #region Title & ProjectID

                {
                    EditorGUILayout.LabelField("Game Title");
                    if (!string.IsNullOrEmpty(m_UpdateGameTitleErrorMsg))
                    {
                        GUIStyle textStyle = new GUIStyle(GUI.skin.label);
                        textStyle.wordWrap = true;
                        textStyle.normal.textColor = Color.red;
                        EditorGUILayout.LabelField(m_UpdateGameTitleErrorMsg, textStyle);
                    }

                    EditorGUI.BeginChangeCheck();
                    m_CurrentAppItem.name = EditorGUILayout.TextField(m_CurrentAppItem.name);

                    if (GUI.changed)
                    {
                        m_GameTitleChanged = true;
                    }

                    EditorGUI.EndChangeCheck();
                }

                {
                    EditorGUILayout.LabelField("Unity Project ID");
                    EditorGUILayout.BeginHorizontal();
                    SelectableLabel(m_CloudProjectId);
                    if (GUILayout.Button("Copy", GUILayout.Width(AppStoreStyles.kCopyButtonWidth)))
                    {
                        TextEditor te = new TextEditor();
                        te.text = m_CloudProjectId;
                        te.SelectAll();
                        te.Copy();
                    }

                    EditorGUILayout.EndHorizontal();
                    GuiLine();
                }

    #endregion
#endif
    #region In App Purchase Configuration

#pragma warning disable
                    if (BuildConfig.IAP_VERSION)
                    {
                        EditorGUILayout.BeginVertical();
                        m_InAppPurchaseFoldout = EditorGUILayout.Foldout(m_InAppPurchaseFoldout, "IAP Catalog", true,
                            AppStoreStyles.KAppStoreSettingsHeaderGuiStyle);
                        if (m_InAppPurchaseFoldout)
                        {
                            // Add New IAP Button (Center)
                            {
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button("Open Catalog", new GUIStyle(GUI.skin.button)
                                    {
                                        fontSize = AppStoreStyles.kAddNewIapButtonFontSize
                                    },
                                    GUILayout.Height(EditorGUIUtility.singleLineHeight *
                                                     AppStoreStyles.kAddNewIapButtonRatio),
                                    GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
                                {
                                    EditorApplication.ExecuteMenuItem("Window/Unity IAP/IAP Catalog");
                                }

                                GUILayout.FlexibleSpace();
                                EditorGUILayout.EndHorizontal();
                            }
                        }

                        EditorGUILayout.EndVertical();
                        GuiLine();
                    }
                    else
                    {
                        EditorGUI.BeginDisabledGroup(m_IsIapUpdating);
                        var currentRect = EditorGUILayout.BeginVertical();
                        m_InAppPurchaseFoldout = EditorGUILayout.Foldout(m_InAppPurchaseFoldout, "IAP Catalog", true,
                            AppStoreStyles.KAppStoreSettingsHeaderGuiStyle);
                        if (m_InAppPurchaseFoldout)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUI.LabelField(new Rect(currentRect.xMax - 120, currentRect.yMin, 120, 20),
                                string.Format("{0} total ({1} edited)", m_IapItems.Count, EditedIAPCount()),
                                new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleRight});
                            for (int i = 0; i < m_IapItemsFoldout.Count; i++)
                            {
                                currentRect = EditorGUILayout.BeginVertical();
                                int pos = i;

                                if (m_IapItemDirty[pos])
                                {
                                    EditorGUI.LabelField(new Rect(currentRect.xMax - 95, currentRect.yMin, 80, 20),
                                        "(edited)", new GUIStyle(GUI.skin.label) {alignment = TextAnchor.UpperRight});
                                }

                                if (GUI.Button(new Rect(currentRect.xMax - 15, currentRect.yMin, 15, 15), "\u2261"))
                                {
                                    GenericMenu menu = new GenericMenu();
                                    menu.AddItem(new GUIContent("Push"), false, () =>
                                    {
                                        if (m_IapItemDirty[pos])
                                        {
                                            m_IapValidationMsg[pos] = m_IapItems[pos].Validate();
                                            if (!string.IsNullOrEmpty(m_IapValidationMsg[pos]))
                                            {
                                                Debug.LogError(
                                                    "[UDP] Iap:" + m_IapItems[pos].slug + " " + m_IapValidationMsg[pos]);
                                            }

                                            if (string.IsNullOrEmpty(m_IapValidationMsg[pos]))
                                            {
                                                // If check suceeds
                                                if (!string.IsNullOrEmpty(m_IapItems[pos].id))
                                                {
                                                    UpdateIAPItem(m_IapItems[pos], pos);
                                                }
                                                else
                                                {
                                                    CreateIAPItem(m_IapItems[pos], pos);
                                                }

                                                AddPushRequests(m_IapItems[pos].slug);
                                            }
                                            else
                                            {
                                                Repaint();
                                            }
                                        }
                                    });
                                    menu.AddItem(new GUIContent("Delete"), false, () =>
                                    {
                                        if (string.IsNullOrEmpty(m_IapItems[pos].id))
                                        {
                                            RemoveIapItemLocally(pos);
                                        }
                                        else
                                        {
                                            DeleteIAPItem(m_IapItems[pos], pos);
                                        }
                                    });
                                    menu.ShowAsContext();
                                }

                                IapItem item = m_IapItems[pos];
                                m_IapItemsFoldout[pos] = EditorGUILayout.Foldout(m_IapItemsFoldout[pos],
                                    "Product: " + (item.name), true,
                                    new GUIStyle(EditorStyles.foldout)
                                    {
                                        wordWrap = false, clipping = TextClipping.Clip,
                                        fixedWidth = currentRect.xMax - 95
                                    });
                                if (m_IapItemsFoldout[pos])
                                {
                                    if (!string.IsNullOrEmpty(m_IapValidationMsg[pos]))
                                    {
                                        GUIStyle textStyle = new GUIStyle(GUI.skin.label);
                                        textStyle.wordWrap = true;
                                        textStyle.normal.textColor = Color.red;

                                        EditorGUILayout.LabelField(m_IapValidationMsg[pos], textStyle);
                                    }

                                    EditorGUI.BeginChangeCheck();
                                    item.slug = LabelWithTextField("Product ID", item.slug);
                                    item.name = LabelWithTextField("Name", item.name);

                                    GUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("Type",
                                        GUILayout.Width(AppStoreStyles.kClientLabelWidth));
                                    int index = item.consumable ? 0 : 1;
                                    index = EditorGUILayout.Popup(index, m_IapItemTypeOptions);
                                    item.consumable = index == 0;
                                    GUILayout.EndHorizontal();

                                    PriceDetail pd = ExtractUSDPrice(item);
                                    pd.price = LabelWithTextField("Price (USD)", pd.price);

                                    EditorGUILayout.BeginVertical();
                                    EditorGUILayout.LabelField("Description");

                                    item.properties.description = EditorGUILayout.TextArea(item.properties.description,
                                        GUILayout.Height(EditorGUIUtility.singleLineHeight * 4));
                                    EditorGUILayout.EndVertical();

                                    if (GUI.changed)
                                    {
                                        m_IapItemDirty[pos] = true;
                                    }

                                    EditorGUI.EndChangeCheck();
                                }

                                EditorGUILayout.EndVertical();
                            }

                            EditorGUI.indentLevel--;
                            // Add New IAP Button (Center)
                            {
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button("Add new IAP", new GUIStyle(GUI.skin.button)
                                    {
                                        fontSize = AppStoreStyles.kAddNewIapButtonFontSize
                                    },
                                    GUILayout.Height(EditorGUIUtility.singleLineHeight *
                                                     AppStoreStyles.kAddNewIapButtonRatio),
                                    GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
                                {
                                    AddIapItem(new IapItem
                                    {
                                        masterItemSlug = m_CurrentAppItem.slug,
                                    }, true, true);
                                }

                                GUILayout.FlexibleSpace();
                                EditorGUILayout.EndHorizontal();
                            }
                        }

                        EditorGUILayout.EndVertical();
                        EditorGUI.EndDisabledGroup();
                        GuiLine();
                    }

#pragma warning restore

    #endregion

#if (!UNITY_2020_1_OR_NEWER)
    #region UDP Client Settings

                    m_UdpClientSettingsFoldout = EditorGUILayout.Foldout(m_UdpClientSettingsFoldout, "Settings", true,
                        AppStoreStyles.KAppStoreSettingsHeaderGuiStyle);
                    if (m_UdpClientSettingsFoldout)
                    {
                        EditorGUI.indentLevel++;

                        if (!string.IsNullOrEmpty(m_UpdateClientErrorMsg))
                        {
                            GUIStyle textStyle = new GUIStyle(GUI.skin.label);
                            textStyle.wordWrap = true;
                            textStyle.normal.textColor = Color.red;

                            EditorGUILayout.LabelField(m_UpdateClientErrorMsg, textStyle);
                        }

                        LabelWithReadonlyTextField("Game ID", m_CurrentAppItem.id);
                        LabelWithReadonlyTextField("Client ID", m_UnityClientId.stringValue);
                        LabelWithReadonlyTextField("Client Key", m_UnityClientKey.stringValue);
                        LabelWithReadonlyTextField("RSA Public Key", m_UnityClientRsaPublicKey.stringValue);
                        LabelWithReadonlyTextField("Client Secret", m_ClientSecretInMemory);

                        EditorGUI.BeginChangeCheck();
                        m_CallbackUrlLast = LabelWithTextField("Callback URL", m_CallbackUrlLast);

                        if (GUI.changed)
                        {
                            m_CallbackUrlChanged = true;
                        }

                        EditorGUI.EndChangeCheck();

                        EditorGUI.indentLevel--;
                    }

                    GuiLine();

    #endregion

    #region Test Accounts

                    EditorGUI.BeginDisabledGroup(m_IsTestAccountUpdating);

                    m_TestAccountFoldout = EditorGUILayout.Foldout(m_TestAccountFoldout, "UDP Sandbox Test Accounts",
                        true,
                        AppStoreStyles.KAppStoreSettingsHeaderGuiStyle);

                    if (m_TestAccountFoldout)
                    {
                        for (int i = 0; i < m_TestAccounts.Count; i++)
                        {
                            int pos = i;

                            if (!string.IsNullOrEmpty(m_TestAccountsValidationMsg[pos]))
                            {
                                GUIStyle textStyle = new GUIStyle(GUI.skin.label);
                                textStyle.wordWrap = true;
                                textStyle.normal.textColor = Color.red;

                                EditorGUILayout.LabelField(m_TestAccountsValidationMsg[pos], textStyle);
                            }

                            EditorGUILayout.BeginHorizontal();
                            EditorGUI.BeginChangeCheck();
                            m_TestAccounts[pos].email = EditorGUILayout.TextField(m_TestAccounts[pos].email);
                            m_TestAccounts[pos].password = EditorGUILayout.PasswordField(m_TestAccounts[pos].password);

                            if (GUI.changed)
                            {
                                m_TestAccountsDirty[pos] = true;
                            }

                            EditorGUI.EndChangeCheck();

                            //delete action
                            if (GUILayout.Button("\u2212", new GUIStyle(GUI.skin.button)
                                {
                                    fontSize = AppStoreStyles.kAddNewIapButtonFontSize,
                                    margin = new RectOffset(0, 0, 2, 0)
                                },
                                GUILayout.Height(15),
                                GUILayout.Width(15)))
                            {
                                if ((string.IsNullOrEmpty(m_TestAccounts[pos].playerId)))
                                {
                                    RemoveTestAccountLocally(pos);
                                }
                                else
                                {
                                    DeleteTestAccount(m_TestAccounts[pos], pos);
                                }
                            }

                            EditorGUILayout.EndHorizontal();
                        }


                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("Add new test account", new GUIStyle(GUI.skin.button)
                                {
                                    fontSize = AppStoreStyles.kAddNewIapButtonFontSize
                                },
                                GUILayout.Height(EditorGUIUtility.singleLineHeight *
                                                 AppStoreStyles.kAddNewIapButtonRatio),
                                GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
                            {
                                m_TestAccounts.Add(new TestAccount
                                {
                                    email = "Email",
                                    password = "Password",
                                    isUpdate = false,
                                });
                                m_TestAccountsDirty.Add(true);
                                m_TestAccountsValidationMsg.Add("");
                            }

                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                        }
                    }

                    GuiLine();
                    EditorGUI.EndDisabledGroup();

    #endregion

    #region Go to Portal

                {
                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button("Go to UDP console", GUILayout.Width(AppStoreStyles.kGoToPortalButtonWidth)))
                    {
                        Application.OpenURL(BuildConfig.CONSOLE_URL);
                    }

                    EditorGUILayout.EndHorizontal();
                }

    #endregion

#endif
                    break;
            }

            EditorGUI.EndDisabledGroup();
        }

        private void OnDestroy()
        {
            EditorApplication.update -= CheckRequestUpdate;
        }

    #region helper method

        void CallApiAsync()
        {
            if (AppStoreOnboardApi.tokenInfo.access_token == null)
            {
                Type unityOAuthType = Utils.FindTypeByName("UnityEditor.Connect.UnityOAuth");
                Type authCodeResponseType = unityOAuthType.GetNestedType("AuthCodeResponse", BindingFlags.Public);
                var performMethodInfo =
                    typeof(AppStoreSettingsEditor).GetMethod("Perform").MakeGenericMethod(authCodeResponseType);
                var actionT =
                    typeof(Action<>).MakeGenericType(authCodeResponseType); // Action<UnityOAuth.AuthCodeResponse>
                var getAuthorizationCodeAsyncMethodInfo = unityOAuthType.GetMethod("GetAuthorizationCodeAsync");
                var performDelegate = Delegate.CreateDelegate(actionT, this, performMethodInfo);
                try
                {
                    getAuthorizationCodeAsyncMethodInfo.Invoke(null,
                        new object[] {AppStoreOnboardApi.oauthClientId, performDelegate});
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException is InvalidOperationException)
                    {
                        Debug.LogError("[UDP] You must login with Unity ID first.");
                        EditorUtility.DisplayDialog("Error", "You must login with Unity ID first.", "OK");
                        m_CurrentState = State.CannotGetCloudProjectId;
                        m_IsOperationRunning = false;
                        this.Repaint();
                    }
                }
            }
            else
            {
                UnityWebRequest request = AppStoreOnboardApi.GetUserId();
                UserIdResponse userIdResp = new UserIdResponse();
                ReqStruct reqStruct = new ReqStruct();
                reqStruct.request = request;
                reqStruct.resp = userIdResp;
                reqStruct.targetStep = m_TargetStep;
                m_RequestQueue.Enqueue(reqStruct);
            }
        }

        public void Perform<T>(T response)
        {
            var authCodePropertyInfo = response.GetType().GetProperty("AuthCode");
            var exceptionPropertyInfo = response.GetType().GetProperty("Exception");
            string authCode = (string) authCodePropertyInfo.GetValue(response, null);
            Exception exception = (Exception) exceptionPropertyInfo.GetValue(response, null);

            if (authCode != null)
            {
                UnityWebRequest request = AppStoreOnboardApi.GetAccessToken(authCode);
                TokenInfo tokenInfoResp = new TokenInfo();
                ReqStruct reqStruct = new ReqStruct();
                reqStruct.request = request;
                reqStruct.resp = tokenInfoResp;
                reqStruct.targetStep = m_TargetStep;
                m_RequestQueue.Enqueue(reqStruct);
            }
            else
            {
                Debug.LogError("[UDP] " + "Failed: " + exception.ToString());
                EditorUtility.DisplayDialog("Error", "Failed: " + exception.ToString(), "OK");
                m_CurrentState = State.CannotGetCloudProjectId;
                m_IsOperationRunning = false;
                Repaint();
            }
        }

        private void saveGameSettingsProps(String clientId)
        {
            if (!Directory.Exists(AppStoreSettings.appStoreSettingsPropFolder))
                Directory.CreateDirectory(AppStoreSettings.appStoreSettingsPropFolder);
            StreamWriter writter = new StreamWriter(AppStoreSettings.appStoreSettingsPropPath, false);
            String warningMessage = "*** DO NOT DELETE OR MODIFY THIS FILE !! ***";
            writter.WriteLine(warningMessage);
            writter.WriteLine(clientId);
            writter.WriteLine(warningMessage);
            writter.Close();
        }

        void GuiLine(int i_height = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, i_height);
            rect.height = i_height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }

        string LabelWithTextField(string labelText, string defaultText = "",
            float labelWidth = AppStoreStyles.kClientLabelWidth)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(labelText, GUILayout.Width(labelWidth));
            string text = EditorGUILayout.TextField(defaultText);
            GUILayout.EndHorizontal();
            return text;
        }

        void SelectableLabel(string labelText)
        {
            EditorGUILayout.SelectableLabel(labelText, EditorStyles.textField,
                GUILayout.Height(EditorGUIUtility.singleLineHeight));
        }

        void LabelWithReadonlyTextField(string labelText, string defaultText = "",
            float labelWidth = AppStoreStyles.kClientLabelWidth)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(labelText, GUILayout.Width(labelWidth));
            EditorGUILayout.SelectableLabel(defaultText, EditorStyles.textField,
                GUILayout.Height(EditorGUIUtility.singleLineHeight));
            GUILayout.EndHorizontal();
        }

        private void PublishApp(String appItemId, string targetStep)
        {
            UnityWebRequest newRequest = AppStoreOnboardApi.PublishAppItem(appItemId);
            AppItemPublishResponse appItemPublishResponse = new AppItemPublishResponse();
            ReqStruct newReqStruct = new ReqStruct();
            newReqStruct.request = newRequest;
            newReqStruct.resp = appItemPublishResponse;
            newReqStruct.targetStep = targetStep;
            m_RequestQueue.Enqueue(newReqStruct);
        }

        private void ListPlayers()
        {
            UnityWebRequest newRequest = AppStoreOnboardApi.GetTestAccount(m_UnityClientId.stringValue);
            PlayerResponseWrapper playerResponseWrapper = new PlayerResponseWrapper();
            ReqStruct newReqStruct = new ReqStruct();
            newReqStruct.request = newRequest;
            newReqStruct.resp = playerResponseWrapper;
            newReqStruct.targetStep = null;
            m_RequestQueue.Enqueue(newReqStruct);
        }

        private void PullIAPItems()
        {
            UnityWebRequest request = AppStoreOnboardApi.SearchStoreItem(m_CurrentAppItem.slug);
            IapItemSearchResponse clientResp = new IapItemSearchResponse();
            ReqStruct reqStruct = new ReqStruct();
            reqStruct.request = request;
            reqStruct.resp = clientResp;
            reqStruct.curIapItem = null;
            m_RequestQueue.Enqueue(reqStruct);
        }

        private void ProcessIAPResponse(ReqStruct reqStruct, UnityWebRequest request, string eventName)
        {
            reqStruct.curIapItem = JsonUtility.FromJson<IapItem>(HandleIapItemResponse(request.downloadHandler.text));
            RemovePushRequest(reqStruct.curIapItem.slug);
            m_IapItemDirty[reqStruct.arrayPos] = false;
            ReqStruct analyticsReq = new ReqStruct
            {
                eventName = eventName,
                request = EditorAnalyticsApi.IapEvent(eventName, m_UnityClientId.stringValue,
                    reqStruct.curIapItem, null),
                resp = new EventRequestResponse(),
            };

            m_IapItems[reqStruct.arrayPos] = reqStruct.curIapItem; // add id information

            m_RequestQueue.Enqueue(analyticsReq);
            Repaint();
        }

        private string HandleIapItemResponse(string oldData)
        {
            string newData = oldData.Replace("en-US", "thisShouldBeENHyphenUS");
            newData = newData.Replace("zh-CN", "thisShouldBeZHHyphenCN");
            return newData;
        }

        private PriceDetail ExtractUSDPrice(IapItem iapItem)
        {
            List<PriceDetail> prices = iapItem.priceSets.PurchaseFee.priceMap.DEFAULT;
            foreach (var price in prices)
            {
                if (price.currency == "USD")
                {
                    return price;
                }
            }

            PriceDetail newUSDPrice = new PriceDetail();
            newUSDPrice.currency = "USD";
            prices.Add(newUSDPrice);
            return newUSDPrice;
        }

        private void AddIapItem(IapItem item, bool dirty = true, bool foldout = false)
        {
            m_IapItems.Add(item);
            m_IapItemDirty.Add(dirty);
            m_IapItemsFoldout.Add(foldout);
            m_IapValidationMsg.Add("");
        }

        private void RemoveIapItemLocally(int pos)
        {
            m_IapItems.RemoveAt(pos);
            m_IapItemDirty.RemoveAt(pos);
            m_IapItemsFoldout.RemoveAt(pos);
            m_IapValidationMsg.RemoveAt(pos);
        }

        private void RemoveTestAccountLocally(int pos)
        {
            m_TestAccounts.RemoveAt(pos);
            m_TestAccountsDirty.RemoveAt(pos);
            m_TestAccountsValidationMsg.RemoveAt(pos);
        }

        private void ClearIapItems()
        {
            m_IapItems = new List<IapItem>();
            m_IapItemDirty = new List<bool>();
            m_IapItemsFoldout = new List<bool>();
            m_IapValidationMsg = new List<string>();
        }

#if (!UNITY_2020_1_OR_NEWER)
        private void ClearTestAccounts()
        {
            m_TestAccounts = new List<TestAccount>();
            m_TestAccountsDirty = new List<bool>();
            m_TestAccountsValidationMsg = new List<string>();
        }
#endif

        private void RefreshAllInformation()
        {
#if (!UNITY_2020_1_OR_NEWER)
            m_UpdateGameTitleErrorMsg = "";
            m_UpdateClientErrorMsg = "";
            m_GameTitleChanged = false;
            m_CallbackUrlChanged = false;
            ClearTestAccounts();
#endif
            ClearIapItems();
            m_IsOperationRunning = true;
            m_TargetStep = k_StepGetClient;
            CallApiAsync();
        }

        // product id, client id or game id or test account email
        // For IAPs, we add product id to the list
        // For ClientSettings, we add client id to the list
        // For Game Settings, we add game id to the list
        // For Test Accounts, we add the email.
        private void AddPushRequests(string id)
        {
            pushRequestList.Add(id);
        }

        private void RemovePushRequest(string id)
        {
            pushRequestList.Remove(id);
        }

        private void DeleteIAPItem(IapItem iapItem, int pos)
        {
            m_IsIapUpdating = true;
            UnityWebRequest request = AppStoreOnboardApi.DeleteStoreItem(iapItem.id);
            IapItemDeleteResponse clientResp = new IapItemDeleteResponse();
            ReqStruct reqStruct = new ReqStruct
            {
                request = request,
                resp = clientResp,
                arrayPos = pos,
                curIapItem = iapItem
            };
            m_RequestQueue.Enqueue(reqStruct);
        }

        private void DeleteTestAccount(TestAccount account, int pos)
        {
#if (!UNITY_2020_1_OR_NEWER)
	      m_IsTestAccountUpdating = true;
#endif
            UnityWebRequest request = AppStoreOnboardApi.DeleteTestAccount(account.playerId);
            PlayerDeleteResponse response = new PlayerDeleteResponse();
            ReqStruct reqStruct = new ReqStruct
            {
                request = request,
                resp = response,
                arrayPos = pos,
                currTestAccount = account
            };
            m_RequestQueue.Enqueue(reqStruct);
        }

        private void UpdateIAPItem(IapItem iapItem, int pos)
        {
            iapItem.status = "STAGE";
            UnityWebRequest request = AppStoreOnboardApi.UpdateStoreItem(iapItem);
            UnityIapItemUpdateResponse resp = new UnityIapItemUpdateResponse();
            ReqStruct reqStruct = new ReqStruct
            {
                request = request,
                resp = resp,
                arrayPos = pos,
                curIapItem = iapItem
            };
            m_RequestQueue.Enqueue(reqStruct);
        }

        public void CreateIAPItem(IapItem iapItem, int pos)
        {
            UnityWebRequest request = AppStoreOnboardApi.CreateStoreItem(iapItem);
            UnityIapItemCreateResponse clientResp = new UnityIapItemCreateResponse();
            ReqStruct reqStruct = new ReqStruct();
            reqStruct.request = request;
            reqStruct.resp = clientResp;
            reqStruct.curIapItem = iapItem;
            reqStruct.arrayPos = pos;
            reqStruct.targetStep = "DIALOG";
            m_RequestQueue.Enqueue(reqStruct);
        }

        private int EditedIAPCount()
        {
            if (m_IapItemDirty == null)
                return 0;

            int count = 0;

            foreach (bool dirty in m_IapItemDirty)
            {
                if (dirty)
                {
                    count++;
                }
            }

            return count;
        }

        bool CheckURL(String URL)
        {
            string pattern =
                @"^(https?://[\w\-]+(\.[\w\-]+)+(:\d+)?((/[\w\-]*)?)*(\?[\w\-]+=[\w\-]+((&[\w\-]+=[\w\-]+)?)*)?)?$";
            return new Regex(pattern, RegexOptions.IgnoreCase).IsMatch(URL);
        }

        private void UpdateCallbackUrl()
        {
            UnityClientInfo unityClientInfo = new UnityClientInfo();
            unityClientInfo.ClientId = m_UnityClientId.stringValue;
            string callbackUrl = m_CallbackUrlLast;
            UnityWebRequest newRequest =
                AppStoreOnboardApi.UpdateUnityClient(Application.cloudProjectId, unityClientInfo,
                    callbackUrl);
            UnityClientResponse clientResp = new UnityClientResponse();
            ReqStruct newReqStruct = new ReqStruct();
            newReqStruct.request = newRequest;
            newReqStruct.resp = clientResp;
            newReqStruct.targetStep = k_StepUpdateClient;
            m_RequestQueue.Enqueue(newReqStruct);
        }

        private void UpdateGameTitle()
        {
            m_CurrentAppItem.status = "STAGE";
            UnityWebRequest newRequest = AppStoreOnboardApi.UpdateAppItem(m_CurrentAppItem);
            AppItemResponse appItemResponse = new AppItemResponse();
            ReqStruct newReqStruct = new ReqStruct();
            newReqStruct.request = newRequest;
            newReqStruct.resp = appItemResponse;
            newReqStruct.targetStep = k_StepUpdateGameTitle;
            m_RequestQueue.Enqueue(newReqStruct);
        }

        // Remove failure entries from pushRequstList
        private void ProcessErrorRequest(ReqStruct reqStruct)
        {
            if (reqStruct.curIapItem != null)
            {
                RemovePushRequest(reqStruct.curIapItem.slug);
            }

            else if (reqStruct.resp.GetType() == typeof(UnityClientResponse))
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
                RemovePushRequest(m_TestAccounts[reqStruct.arrayPos].email);
            }
        }

        private void CreateTestAccount(TestAccount testAccount, int pos)
        {
            Player player = new Player();
            player.email = testAccount.email;
            player.password = testAccount.password;
            UnityWebRequest request =
                AppStoreOnboardApi.SaveTestAccount(player, m_UnityClientId.stringValue);
            PlayerResponse playerResponse = new PlayerResponse();
            ReqStruct reqStruct = new ReqStruct();
            reqStruct.request = request;
            reqStruct.resp = playerResponse;
            reqStruct.targetStep = null;
            reqStruct.arrayPos = pos;
            m_RequestQueue.Enqueue(reqStruct);
        }

        private void UpdateTestAccount(TestAccount testAccount, int pos)
        {
            PlayerChangePasswordRequest player = new PlayerChangePasswordRequest();
            player.password = testAccount.password;
            player.playerId = testAccount.playerId;
            UnityWebRequest request = AppStoreOnboardApi.UpdateTestAccount(player);
            PlayerChangePasswordResponse playerChangePasswordResponse = new PlayerChangePasswordResponse();
            ReqStruct reqStruct = new ReqStruct();
            reqStruct.request = request;
            reqStruct.resp = playerChangePasswordResponse;
            reqStruct.targetStep = null;
            reqStruct.arrayPos = pos;
            m_RequestQueue.Enqueue(reqStruct);
        }

        private bool AnythingChanged()
        {

#if (!UNITY_2020_1_OR_NEWER)
            if (m_GameTitleChanged || m_CallbackUrlChanged)
            {
                return true;
            }
            foreach (bool dirty in m_TestAccountsDirty)
            {
                if (dirty)
                {
                    return true;
                }
            }
#endif

            foreach (bool dirty in m_IapItemDirty)
            {
                if (dirty)
                {
                    return true;
                }
            }

            return false;
        }

    #endregion
    }
#endif
}
