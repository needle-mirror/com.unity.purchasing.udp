#if (SERVICES_SDK_CORE_ENABLED && ENABLE_EDITOR_GAME_SERVICES)
using System;
using System.Collections.Generic;
using System.IO;
using Unity.Services.Core.Editor;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace UnityEngine.UDP.Editor
{
    class UDPSettingsProvider : EditorGameServiceSettingsProvider
    {
        protected override IEditorGameService EditorGameService =>
            EditorGameServiceRegistry.Instance.GetEditorGameService<UDPServiceIdentifier>();

        protected override string Title => UiConstants.LocalizedStrings.Title;
        protected override string Description => UiConstants.LocalizedStrings.Description;

        [SettingsProvider]
        static SettingsProvider CreateProjectSettingsProvider()
        {
           return new UDPSettingsProvider(SettingsScope.Project);
        }

        public UDPSettingsProvider(SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(GetSettingsPath(), scopes, keywords)
        {
        }

        internal static string GetSettingsPath()
        {
            return GenerateProjectSettingsPath(UDPService.ServiceIdentifier);
        }


        VisualTreeAsset m_MainTemplateAssets;
        VisualTreeAsset m_LinkProjectTemplateAssets;
        VisualElement m_Root;
        VisualElement m_ServiceWindowContainer;
        VisualElement m_ServiceLinkProjectContainer;
        VisualElement m_ServiceCannotOAuthContainer;
        VisualElement m_ServiceCannotGetProjectIdContainer;
        VisualElement m_ServiceGeneralErrorContainer;

        VisualElement m_UDPSettingsWindow;
        VisualElement m_UDPOperationBlock;
        VisualElement m_UDPBasicInfoBlock;
        VisualElement m_UDPClientSettingsBlock;
        VisualElement m_UDPPlayerBlock;
        VisualElement m_UDPPlayersContainer;
        VisualElement m_UDPExternalLinkBlock;
        VisualElement m_UDPLinkClientIdBlock;
        VisualElement m_UDPGenerateNewClientBlock;
        VisualElement m_UDPGoToPortalLiBlock;
        VisualElement m_UDPSyncStatusGroupBlock;
        VisualElement m_UDPGenerateClientFoldoutBlock;

        VisualElement m_UDPPullingLabel;

        VisualElement m_UDPPushingLabel;
        // private List<Dictionary<string, VisualElement>> _containerList = new List<Dictionary<string, VisualElement>>();

        #region local field

        private AppStoreSettings settings;

        string m_ClientSecretInMemory;
        string m_CallbackUrlInMemory;
        readonly List<string> k_PushRequestList = new List<string>();

        static readonly bool k_EnableOAuth =
            Utils.FindTypeByName("UnityEditor.Connect.UnityOAuth") != null;

        List<TestAccount> m_TestAccounts = new List<TestAccount>();
        List<EasyToSaveAccount> m_EasyToSaveAccounts = new List<EasyToSaveAccount>();
        readonly List<bool> k_TestAccountsDirty = new List<bool>();
        readonly List<string> k_TestAccountsValidationMsg = new List<string>();

        AppItem m_CurrentAppItem;
        string m_ClientIdToBeLinked = "UDP client ID";
        string m_UpdateClientErrorMsg;
        string m_UpdateGameTitleErrorMsg;

        private bool m_IsTestAccountUpdating;
        private bool m_IsGeneratingOrLinking;
        private bool m_IsFetchingToken;
        private bool m_IsOperationRunning;

        // user state no need to check every frame, so we do checking every some seconds.
        private float checkTimer = 0.0f;
        private const float checkDuration = 100.0f;

        private struct RequestStruct
        {
            public Step currentStep;
            public Step nextStep;

            public TestAccount testAccount;
            public int arrayPosition;
            public UnityWebRequest request;
            public GeneralResponse resp;
            public FullUpdatePayload fullUpdatePayload;

            public bool IsEmpty()
            {
                return request == null;
            }
        }

        private enum Step
        {
            InitData,
            FetchToken,
            GenerateNewClient,
            LinkProjectId,
            UpdateAll,
            DeletePlayer,
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

        #endregion

        protected override VisualElement GenerateServiceDetailUI()
        {
            m_Root = new VisualElement();
            return m_Root;
        }


        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            Log("activate");
            base.OnActivate(searchContext, rootElement);
            CreateSettingsAsset();
            m_Root = rootElement;

            settings = LoadSettingsAsset();
            if (settings == null)
            {
                Debug.LogError("UDP Settings.asset not found");
                return;
            }

            var script = MonoScript.FromScriptableObject(settings);
            var path = AssetDatabase.GetAssetPath(script);
            var assetRootPath = path.Contains("Runtime") ? path.Remove(path.Length - 15) : path.Remove(path.Length - 7);

            m_Root.styleSheets.Add(
                AssetDatabase.LoadAssetAtPath<StyleSheet>(
                    assetRootPath + UiConstants.StyleSheetPaths.UDPStyleSheetPath));
            m_MainTemplateAssets =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    assetRootPath + UiConstants.StyleSheetPaths.UDPMainTemplatePath);
            m_LinkProjectTemplateAssets =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    assetRootPath + UiConstants.StyleSheetPaths.UDPLinkProjectTemplatePath);

            if (!k_EnableOAuth)
            {
                Debug.LogError("cannot oauth");
                SetUpCannotOAuthWindow();
                return;
            }

            InitializeProperties();
            // Must reset properties every time this is activated
            // rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(k_udpStyleSheetPath));


            //            rootVisualElement.AddStyleSheetPath(EditorGUIUtility.isProSkin ? k_ServicesWindowDarkUssPath : k_ServicesWindowLightUssPath);
            //            rootVisualElement.AddStyleSheetPath(EditorGUIUtility.isProSkin ? k_ServicesDarkUssPath : k_ServicesLightUssPath);
            //            rootVisualElement.AddStyleSheetPath(k_CollaborateCommonUssPath);
            //            rootVisualElement.AddStyleSheetPath(EditorGUIUtility.isProSkin ? k_CollaborateDarkUssPath : k_CollaborateLightUssPath);
            //            m_UDPGameTitleInput = rootVisualElement.Q<TextField>(UiConstants.UiElementNames.UDPGameTitle);
            //            m_UDPProjectIdInput = rootVisualElement.Q<TextField>(UiConstants.UiElementNames.UDPProjectId);
            //            m_UDPGoToConsoleButton = rootVisualElement.Q<Button>(UiConstants.UiElementNames.GoToUdpDashboard);
            //            m_UDPOpenIapCatalogButton = rootVisualElement.Q<Button>(k);

//            RestoreTestAccount();

            EditorApplication.update -= CheckUserState;
            EditorApplication.update += CheckUserState;
            EditorApplication.update -= CheckRequestUpdate;
            EditorApplication.update += CheckRequestUpdate;
        }

        private void SetUpServiceWindow()
        {
            HideVisualElement(m_ServiceLinkProjectContainer);
            HideVisualElement(m_ServiceCannotOAuthContainer);
            HideVisualElement(m_ServiceCannotGetProjectIdContainer);
            HideVisualElement(m_ServiceGeneralErrorContainer);
            var root = m_Root;
            if (m_ServiceWindowContainer != null && root.Contains(m_ServiceWindowContainer))
            {
                root.Remove(m_ServiceWindowContainer);
            }

            m_ServiceWindowContainer = new VisualElement();
            m_ServiceWindowContainer.Add(m_MainTemplateAssets.CloneTree().contentContainer);
            root.Add(m_ServiceWindowContainer);

            m_UDPSettingsWindow = root.Q(UiConstants.UiElementNames.UDPSettingsWindow);
            m_UDPSyncStatusGroupBlock = root.Q(UiConstants.UiElementNames.UDPSyncStatusGroupBlock);
            m_UDPOperationBlock = root.Q(UiConstants.UiElementNames.UDPOperationBlock);
            m_UDPBasicInfoBlock = root.Q(UiConstants.UiElementNames.UDPBasicInfoBlock);
            m_UDPClientSettingsBlock = root.Q(UiConstants.UiElementNames.UDPClientSettingsBlock);
            m_UDPPlayerBlock = root.Q(UiConstants.UiElementNames.UDPPlayerBlock);
            m_UDPExternalLinkBlock = root.Q(UiConstants.UiElementNames.UDPExternalLinkBlock);

            SetUpSyncStatusGroup();
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
            HideVisualElement(m_ServiceGeneralErrorContainer);

            var root = m_Root;

            if (m_ServiceLinkProjectContainer != null && root.Contains(m_ServiceLinkProjectContainer))
            {
                root.Remove(m_ServiceLinkProjectContainer);
            }

            m_ServiceLinkProjectContainer = new VisualElement();
            m_ServiceLinkProjectContainer.Add(m_LinkProjectTemplateAssets.CloneTree().contentContainer);
            root.Add(m_ServiceLinkProjectContainer);

            m_UDPLinkClientIdBlock = root.Q(UiConstants.UiElementNames.UDPLinkClientBlock);
            m_UDPLinkClientIdBlock.Q<Button>(UiConstants.UiElementNames.LinkExistingClientButton).clicked -=
                OnLinkToExitingClientButtonClick;
            m_UDPLinkClientIdBlock.Q<Button>(UiConstants.UiElementNames.LinkExistingClientButton).clicked +=
                OnLinkToExitingClientButtonClick;

            Action onGoToPortalClick = () => Application.OpenURL(UiConstants.Urls.UDPDashboardLink);
            m_UDPGoToPortalLiBlock = root.Q(UiConstants.UiElementNames.UDPGoToPortalLiBlock);
            m_UDPGoToPortalLiBlock.Q(UiConstants.UiElementNames.UDPGoToPortalBtn)
                .AddManipulator(new Clickable(onGoToPortalClick));


            m_UDPGenerateNewClientBlock = root.Q(UiConstants.UiElementNames.UDPGenerateNewClientBlock);
            m_UDPGenerateNewClientBlock.Q<Button>(UiConstants.UiElementNames.GenNewClientButton).clicked -=
                OnGenerateNewClientButtonClick;
            m_UDPGenerateNewClientBlock.Q<Button>(UiConstants.UiElementNames.GenNewClientButton).clicked +=
                OnGenerateNewClientButtonClick;

            m_UDPGenerateClientFoldoutBlock =
                m_UDPGenerateNewClientBlock.Q(UiConstants.UiElementNames.UDPGenerateClientFoldoutBlock);
            HideVisualElement(m_UDPGenerateClientFoldoutBlock);
            m_UDPGenerateNewClientBlock.Q<Toggle>(UiConstants.UiElementNames.UDPToggleBtn)
                .RegisterCallback<ChangeEvent<bool>>(evt =>
                {
                    m_UDPGenerateClientFoldoutBlock.style.display =
                        evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                });
        }

        private void SetUpGeneralErrorWindow(string errorMsg)
        {
            EditorApplication.update -= CheckRequestUpdate;

            HideVisualElement(m_ServiceWindowContainer);
            HideVisualElement(m_ServiceCannotOAuthContainer);
            HideVisualElement(m_ServiceLinkProjectContainer);
            HideVisualElement(m_ServiceCannotGetProjectIdContainer);

            if (m_ServiceGeneralErrorContainer != null && m_Root.Contains(m_ServiceGeneralErrorContainer))
            {
                m_Root.Remove(m_ServiceGeneralErrorContainer);
            }

            m_ServiceGeneralErrorContainer = new VisualElement();
            m_ServiceGeneralErrorContainer.Add(new Label(errorMsg));
            m_Root.Add(m_ServiceGeneralErrorContainer);
        }

        private void SetUpCannotOAuthWindow()
        {
            EditorApplication.update -= CheckRequestUpdate;

            //hide others
            HideVisualElement(m_ServiceWindowContainer);
            HideVisualElement(m_ServiceLinkProjectContainer);
            HideVisualElement(m_ServiceCannotGetProjectIdContainer);
            HideVisualElement(m_ServiceGeneralErrorContainer);

            if (m_ServiceCannotOAuthContainer != null && m_Root.Contains(m_ServiceCannotOAuthContainer))
            {
                m_Root.Remove(m_ServiceCannotOAuthContainer);
            }

            m_ServiceCannotOAuthContainer = new VisualElement();
            var warningMsg = new Label(
                "UDP editor extension can only work on Unity 5.6.1+. Please check your Unity version and retry.");
            warningMsg.AddToClassList(UiConstants.UssStrings.UssStringsCannotOAuthWarning);
            m_ServiceCannotOAuthContainer.Add(warningMsg);
            m_Root.Add(m_ServiceCannotOAuthContainer);
        }

        private void SetUpCannotGetProjectIdWindow()
        {
            EditorApplication.update -= CheckRequestUpdate;

            //hide others
            HideVisualElement(m_ServiceWindowContainer);
            HideVisualElement(m_ServiceLinkProjectContainer);
            HideVisualElement(m_ServiceCannotOAuthContainer);
            HideVisualElement(m_ServiceGeneralErrorContainer);

            if (m_ServiceCannotGetProjectIdContainer != null &&
                m_Root.Contains(m_ServiceCannotGetProjectIdContainer))
            {
                m_Root.Remove(m_ServiceCannotGetProjectIdContainer);
            }

            m_ServiceCannotGetProjectIdContainer = new VisualElement();
            var warningMsg = new Label(
                "To use the Unity distribution portal your project will need a Unity project ID. You can create a new project ID or link to an existing one in the Services window.");
            warningMsg.AddToClassList(UiConstants.UssStrings.UssStringsCannotOAuthWarning);
            m_ServiceCannotGetProjectIdContainer.Add(warningMsg);
            m_Root.Add(m_ServiceCannotGetProjectIdContainer);
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            EditorApplication.update -= CheckRequestUpdate;
            SaveToTestAccountStruct();
        }

        private void ProcessErrorResponse(RequestStruct reqStruct)
        {
            var request = reqStruct.request;
            var response = reqStruct.resp;
            if (response == null)
            {
                return;
            }

            var resErr = new ErrorResponse();
            if (request != null)
            {
                resErr = JsonUtility.FromJson<ErrorResponse>(request.downloadHandler.text);
            }

            if (response.GetType() == typeof(TokenInfo))
            {
                ResetAllStatusFlag();
                SetUpGeneralErrorWindow("something happened during authorization, please try again.");
                return;
            }

            if (reqStruct.currentStep == Step.LinkProjectId || reqStruct.currentStep == Step.GenerateNewClient)
            {
                m_IsGeneratingOrLinking = false;
            }

            if (reqStruct.currentStep == Step.DeletePlayer)
            {
                m_IsTestAccountUpdating = false;
                if (resErr != null && resErr.message != null)
                {
                    k_TestAccountsValidationMsg[reqStruct.arrayPosition] = resErr.message;
                }
            }

            if (reqStruct.currentStep == Step.UpdateAll)
            {
                m_IsOperationRunning = false;
            }
        }

        private void CheckUserState()
        {
            checkTimer += Time.deltaTime;
            if (checkTimer > checkDuration)
            {
                // do check
                checkTimer -= checkDuration;

                if (Utils.UserStateChanged(AppStoreOnBoardApi.unityUserId, AppStoreOnBoardApi.unityOrgId,
                        AppStoreOnBoardApi.unityAccessToken) ||
                    AppStoreOnBoardApi.unityProjectId != Application.cloudProjectId)
                {
                    InitializeProperties();
                }
            }
        }

        private void CheckRequestUpdate()
        {
            var doing = IsOperationRunning() || k_PushRequestList.Count > 0;

            m_UDPSettingsWindow?.SetEnabled(!doing);

            m_UDPOperationBlock?.SetEnabled(!doing);

            m_UDPPlayerBlock?.SetEnabled(!m_IsTestAccountUpdating);

            m_UDPLinkClientIdBlock?.SetEnabled(!m_IsGeneratingOrLinking);

            m_UDPGenerateNewClientBlock?.SetEnabled(!m_IsGeneratingOrLinking);

            if (k_RequestQueue.Count == 0)
            {
                return;
            }

            RequestStruct currentRequestStruct = k_RequestQueue.Dequeue();
            UnityWebRequest request = currentRequestStruct.request;
            GeneralResponse resp = currentRequestStruct.resp;

            if (request != null && request.isDone)
            {
                if (request.error != null || request.responseCode / 100 != 2)
                {
                    // Deal with errors
                    Log("request with error");

                    var errText = request.downloadHandler.text;
                    if (errText.Contains(AppStoreOnBoardApi.notAuthenticatedError))
                    {
                        // token invalid. reFetch it.
                        m_IsFetchingToken = true;
                        AppStoreOnBoardApi.tokenInfo = new TokenInfo();
                        var req = BuildRequestFromStepName(Step.FetchToken);
                        if (req.IsEmpty()) return;
                        req.nextStep = currentRequestStruct.currentStep;
                        k_RequestQueue.Enqueue(req);
                    }
                    else if (errText.Contains(AppStoreOnBoardApi.authorizationError))
                    {
                        // no permission.
                        Debug.LogError(
                            "[UDP] authorization error, please ask your manager or owner to do this operation.");
                        EditorUtility.DisplayDialog("Error",
                            "authorization error, please ask your manager or owner to do this operation.",
                            "OK");

                        ResetAllStatusFlag();
                        Repaint();
                    }
                    else if (errText.Contains(AppStoreOnBoardApi.clientNotLinked))
                    {
                        ResetAllStatusFlag();
                        SetUpLinkProjectWindow();
                        Repaint();
                    }
                    else
                    {
                        // general error
                        var error = JsonUtility.FromJson<ErrorResponse>(request.downloadHandler.text);
                        if (error != null && error.message != null)
                        {
                            var errMsg = "[UDP] " + error.message;
                            if (error.target != "")
                            {
                                errMsg += " target: " + error.target;
                            }

                            Debug.LogError(errMsg);
                            EditorUtility.DisplayDialog("Error",
                                errMsg,
                                "OK");
                        }
                        else
                        {
                            Debug.LogError("[UDP] Network error, no response received.");
                            EditorUtility.DisplayDialog("Error",
                                "Network error, no response received",
                                "OK");
                        }

                        // if error during fetch token, show noOauth panel.
                        ProcessErrorResponse(currentRequestStruct);
                        Repaint();
                    }
                }
                else
                {
                    if (resp.GetType() == typeof(TokenInfo))
                    {
                        var res = JsonUtility.FromJson<TokenInfo>(request.downloadHandler.text);
                        AppStoreOnBoardApi.tokenInfo.token = res.token;
                        AppStoreOnBoardApi.tokenInfo.canManage = res.canManage;
                        AppStoreOnBoardApi.tokenInfo.orgId = res.orgId;
                        m_IsFetchingToken = false;
                        var req = BuildRequestFromStepName(currentRequestStruct.nextStep);
                        if (req.IsEmpty()) return;
                        k_RequestQueue.Enqueue(req);
                    }
                    else if (resp.GetType() == typeof(AllDataResponse))
                    {
                        var res = JsonUtility.FromJson<AllDataResponse>(request.downloadHandler.text);
                        LocalSaveAllDataFromServerResponse(res);
                        m_IsOperationRunning = false;
                        m_IsGeneratingOrLinking = false;

                        SetUpServiceWindow();
//                        Repaint();
                    }
                    else if (resp.GetType() == typeof(UnityIapItemUpdateResponse))
                    {
                        Debug.LogError("wont reach here!");
                    }
                    else if (resp.GetType() == typeof(UnityIapItemCreateResponse))
                    {
                        Debug.LogError("wont reach here!");
                    }
                    else if (resp.GetType() == typeof(IapItemDeleteResponse))
                    {
                        Debug.LogError("wont reach here!");
                    }
                    else if (resp.GetType() == typeof(FullUpdateResponse))
                    {
                        m_IsOperationRunning = false;
                        var res = JsonUtility.FromJson<FullUpdateResponse>(request.downloadHandler.text);

                        var errMsg = "";
                        if (!string.IsNullOrEmpty(res.gameTitleErrorMsg))
                        {
                            errMsg += "update game title failed: " + res.gameTitleErrorMsg + "\n";
                        }
                        else
                        {
                            m_UpdateGameTitleErrorMsg = "";
                            LocalSaveAppItemFromServerResponse(res.allDataResponse.appItem);
                        }

                        if (!string.IsNullOrEmpty(res.callbackUrlErrorMsg))
                        {
                            errMsg += "update callback url failed: " + res.callbackUrlErrorMsg + "\n";
                        }
                        else
                        {
                            m_UpdateClientErrorMsg = "";
                            LocalSaveClientSettingsFromServerResponse(res.allDataResponse.client);
                        }

                        LocalSavePlayerFromServerResponse(res.allDataResponse.players);

                        var pushedPlayers = currentRequestStruct.fullUpdatePayload.testAccounts;
                        if (res.testAccountSuccess != null)
                        {
                            for (var i = 0; i < res.testAccountSuccess.Length; i++)
                            {
                                if (!string.IsNullOrEmpty(res.testAccountSuccess[i]))
                                {
                                    var player = pushedPlayers[i];
                                    if (string.IsNullOrEmpty(player.id))
                                    {
                                        errMsg += "create testAccount " + player.email + " failed.\n";
                                    }
                                    else
                                    {
                                        errMsg += "update testAccount " + player.email + " failed.\n";
                                    }

                                    m_TestAccounts.Add(new TestAccount()
                                        {playerId = player.id, email = player.email, password = player.password});
                                    k_TestAccountsDirty.Add(true);
                                    k_TestAccountsValidationMsg.Add(res.testAccountSuccess[i]);
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(errMsg))
                        {
                            Debug.LogError("[UDP] update all content failed: " + errMsg);
                            EditorUtility.DisplayDialog("Error",
                                "[UDP] " + errMsg,
                                "OK");
                        }

                        HideVisualElement(m_UDPPushingLabel);
                        Repaint();
                    }
                    else if (resp.GetType() == typeof(PlayerDeleteResponse))
                    {
                        m_IsTestAccountUpdating = false;
                        EditorUtility.DisplayDialog("Success",
                            "TestAccount " + currentRequestStruct.testAccount.email + " has been Deleted.", "OK");
                        RemoveTestAccountLocally(currentRequestStruct.arrayPosition);
                        Repaint();
                    }
                }

                request.Dispose();
            }
            else
            {
                k_RequestQueue.Enqueue(currentRequestStruct);
            }
        }

        private void LocalSaveAllDataFromServerResponse(AllDataResponse all)
        {
            if (all == null) return;
            LocalSaveAppItemFromServerResponse(all.appItem);

            LocalSaveClientSettingsFromServerResponse(all.client);

            LocalSavePlayerFromServerResponse(all.players);
        }

        private void LocalSaveAppItemFromServerResponse(AppItemResponse appItem)
        {
            if (appItem == null) return;
            m_AppItemId.stringValue = appItem.id;
            m_AppName.stringValue = appItem.name;
            m_AppSlug.stringValue = appItem.slug;
            m_CurrentAppItem.id = appItem.id;
            m_CurrentAppItem.name = appItem.name;
            m_CurrentAppItem.slug = appItem.slug;
            m_CurrentAppItem.ownerId = appItem.ownerId;
            m_CurrentAppItem.ownerType = appItem.ownerType;
            m_CurrentAppItem.status = appItem.status;
            m_CurrentAppItem.type = appItem.type;
            m_CurrentAppItem.clientId = appItem.clientId;
            m_CurrentAppItem.packageName = appItem.packageName;
            m_CurrentAppItem.revision = appItem.revision;
            m_SerializedSettings.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        private void LocalSaveClientSettingsFromServerResponse(UnityClientResponse client)
        {
            if (client == null) return;
            m_UnityClientId.stringValue = client.client_id;
            m_UnityClientKey.stringValue = client.client_secret;
            if (client.channel != null)
            {
                m_UnityClientRsaPublicKey.stringValue = client.channel.publicRSAKey;
                m_UnityProjectId.stringValue = client.channel.projectGuid;
                m_ClientSecretInMemory = client.channel.channelSecret;
                m_CallbackUrlInMemory = client.channel.callbackUrl;
            }
            else
            {
                m_UnityClientRsaPublicKey.stringValue = "";
                m_UnityProjectId.stringValue = "";
                m_ClientSecretInMemory = "";
                m_CallbackUrlInMemory = "";
            }

            m_SerializedSettings.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();

            SaveGameSettingsProps(client.client_id);
        }

        private void LocalSavePlayerFromServerResponse(IEnumerable<PlayerResponse> players)
        {
            if (players == null) return;
            ClearTestAccounts();
            foreach (var player in players)
            {
                m_TestAccounts.Add(new TestAccount()
                    {playerId = player.id, password = "******", email = player.nickName});
                k_TestAccountsDirty.Add(false);
                k_TestAccountsValidationMsg.Add("");
            }
        }

        private AppStoreSettings LoadSettingsAsset()
        {
            if (settings == null)
            {
                settings = AssetDatabase.LoadAssetAtPath<AppStoreSettings>(AppStoreSettings.appStoreSettingsAssetPath);
            }

            return settings;
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
            settings = LoadSettingsAsset();
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

            AppStoreOnBoardApi.unityOrgId = Utils.GetOrganizationId();
            AppStoreOnBoardApi.unityUserId = Utils.GetUserId();
            AppStoreOnBoardApi.unityProjectId = Application.cloudProjectId;
            AppStoreOnBoardApi.unityAccessToken = Utils.GetAccessToken();

            if (!string.IsNullOrEmpty(Application.cloudProjectId))
            {
                Initialize();
            }
            else
            {
                SetUpCannotGetProjectIdWindow();
            }
        }

        private void ClearTestAccounts()
        {
            m_TestAccounts.Clear();
            k_TestAccountsDirty.Clear();
            k_TestAccountsValidationMsg.Clear();
        }

        private void LoginRequired(Step nextStep)
        {
            if (AppStoreOnBoardApi.tokenInfo != null &&
                !string.IsNullOrEmpty(AppStoreOnBoardApi.tokenInfo.token))
            {
                var nextReq = BuildRequestFromStepName(nextStep);
                if (nextReq.IsEmpty()) return;
                nextReq.currentStep = nextStep;
                k_RequestQueue.Enqueue(nextReq);
                return;
            }

            var req = BuildRequestFromStepName(Step.FetchToken);
            if (req.IsEmpty()) return;
            req.currentStep = Step.FetchToken;
            req.nextStep = nextStep;
            m_IsFetchingToken = true;
            k_RequestQueue.Enqueue(req);
        }

        private void Initialize()
        {
            Log("initialize");
            m_UpdateGameTitleErrorMsg = "";
            m_UpdateClientErrorMsg = "";
            ClearTestAccounts();

            m_IsOperationRunning = true;
            LoginRequired(Step.InitData);
        }

        private void SetUpSyncStatusGroup()
        {
            var container = m_UDPSyncStatusGroupBlock;

            m_UDPPullingLabel = container.Q<Label>(UiConstants.UiElementNames.UDPPullingLabel);
            HideVisualElement(m_UDPPullingLabel);

            m_UDPPushingLabel = container.Q<Label>(UiConstants.UiElementNames.UDPPushingLabel);
            HideVisualElement(m_UDPPushingLabel);
        }

        private void SetUpOperationsPart()
        {
            var container = m_UDPOperationBlock;

            container.Q<Button>(UiConstants.UiElementNames.UDPPullButton).clicked -= OnPullButtonClick;
            container.Q<Button>(UiConstants.UiElementNames.UDPPullButton).clicked += OnPullButtonClick;

            container.Q<Button>(UiConstants.UiElementNames.UDPPushButton).clicked -= OnPushButtonClick;
            container.Q<Button>(UiConstants.UiElementNames.UDPPushButton).clicked += OnPushButtonClick;
        }

        private void SetUpBasicInfoPart()
        {
            m_UDPBasicInfoBlock.Q<TextField>(UiConstants.UiElementNames.UDPGameTitle)
                .SetValueWithoutNotify(m_AppName.stringValue);
            var m_ProjectId = m_UDPBasicInfoBlock.Q<TextField>(UiConstants.UiElementNames.UDPProjectId);
            m_ProjectId.SetValueWithoutNotify(Application.cloudProjectId);
            m_ProjectId.SetEnabled(false);

            m_UDPBasicInfoBlock.Q<Button>(UiConstants.UiElementNames.CopyProjectIdButton).clicked -=
                OnCopyProjectIdButtonClick;
            m_UDPBasicInfoBlock.Q<Button>(UiConstants.UiElementNames.CopyProjectIdButton).clicked +=
                OnCopyProjectIdButtonClick;
        }

        private void RepaintGameIdPart()
        {
            m_UDPClientSettingsBlock.Q<TextField>(UiConstants.UiElementNames.UDPGameId)
                .SetValueWithoutNotify(m_AppSlug.stringValue);
        }

        private void SetUpClientSettingsPart()
        {
            if (!string.IsNullOrEmpty(m_UpdateClientErrorMsg))
            {
                m_UDPClientSettingsBlock.Q<Label>(UiConstants.UiElementNames.UDPClientErrorMessage).text =
                    m_UpdateClientErrorMsg;
            }

            var container = m_UDPClientSettingsBlock;

            Action<string, string> setDisabledTextFields = (k, v) =>
            {
                var ele = container.Q<TextField>(k);
                ele.SetValueWithoutNotify(v);
                ele.SetEnabled(false);
            };

            setDisabledTextFields(UiConstants.UiElementNames.UDPGameId, m_AppSlug.stringValue);
            setDisabledTextFields(UiConstants.UiElementNames.UDPClientId, m_UnityClientId.stringValue);
            setDisabledTextFields(UiConstants.UiElementNames.UDPClientKey, m_UnityClientKey.stringValue);
            setDisabledTextFields(UiConstants.UiElementNames.UDPRSAPublicKey, m_UnityClientRsaPublicKey.stringValue);
            setDisabledTextFields(UiConstants.UiElementNames.UDPClientSecret, m_ClientSecretInMemory);

            container.Q<TextField>(UiConstants.UiElementNames.UDPClientCallbackUrl)
                .SetValueWithoutNotify(m_CallbackUrlInMemory);
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
                m_UDPPlayersContainer = m_UDPPlayerBlock.Q(UiConstants.UiElementNames.Players);
            }

            VisualElement playerContainer = new VisualElement();
            playerContainer.name = "player-" + pos;
            playerContainer.AddToClassList("player-container");
            var errTag = new Label(k_TestAccountsValidationMsg[pos]);
            errTag.name = "player-error-" + pos;
            errTag.AddToClassList(UiConstants.UssStrings.UssStringsErrorTag);
            errTag.style.height = 12;
            errTag.style.marginLeft = 4;
            errTag.style.display = !string.IsNullOrEmpty(k_TestAccountsValidationMsg[pos])
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            playerContainer.Add(errTag);

            VisualElement player = new VisualElement();
            player.AddToClassList(UiConstants.UssStrings.UssStringsPlayer);
            TextField email = new TextField();
            email.name = "player-email-" + pos;
            TextField password = new TextField();
            password.name = "player-password-" + pos;

            email.AddToClassList("player-email");
            password.AddToClassList("player-password");
            email.value = m_TestAccounts[pos].email;
            password.value = m_TestAccounts[pos].password;
            password.isPasswordField = true;

            if (!string.IsNullOrEmpty(m_TestAccounts[pos].playerId))
            {
                email.isReadOnly = true;
                password.isReadOnly = true;
            }

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
                m_UDPPlayersContainer = m_UDPPlayerBlock.Q(UiConstants.UiElementNames.Players);
            }

            m_UDPPlayersContainer.Clear();
        }

        private void SetUpPlayerPart()
        {
            m_UDPPlayersContainer = m_UDPPlayerBlock.Q(UiConstants.UiElementNames.Players);
            for (var i = 0; i < m_TestAccounts.Count; i++)
            {
                SetUpSinglePlayer(i);
            }

            m_UDPPlayerBlock.Q<Button>(UiConstants.UiElementNames.AddNewPlayerButton).clicked -=
                OnAddNewPlayerButtonClick;
            m_UDPPlayerBlock.Q<Button>(UiConstants.UiElementNames.AddNewPlayerButton).clicked +=
                OnAddNewPlayerButtonClick;
        }

        private void SetUpExternalLinkPart()
        {
            var goToDashboard = m_UDPExternalLinkBlock.Q(UiConstants.UiElementNames.GoToUdpDashboard);
            if (goToDashboard != null)
            {
                var clickable = new Clickable(() => { Application.OpenURL(UiConstants.Urls.UDPDashboardLink); });

                goToDashboard.AddManipulator(clickable);
            }

            var goToIAPCatalog = m_UDPExternalLinkBlock.Q(UiConstants.UiElementNames.GoToIAPCatalog);
            if (goToIAPCatalog != null)
            {
                var clickable = new Clickable(() =>
                {
                    EditorApplication.ExecuteMenuItem("Services/Unity Distribution Portal/IAP Catalog");
                });

                goToIAPCatalog.AddManipulator(clickable);
            }
        }

        #region helper method

        private RequestStruct BuildRequestFromStepName(Step targetStep)
        {
            var req = new RequestStruct();
            switch (targetStep)
            {
                case Step.FetchToken:
                {
                    try
                    {
                        var unityToken = Utils.GetAccessToken();

                        // exchange udp token
                        req.request = AppStoreOnBoardApi.GetUdpToken(unityToken, Application.cloudProjectId);
                        req.resp = new TokenInfo();
                        req.currentStep = Step.FetchToken;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        ResetAllStatusFlag();
                        SetUpCannotOAuthWindow();
                    }
                }
                    break;
                case Step.InitData:
                {
                    req.request = AppStoreOnBoardApi.FetchData();
                    req.resp = new AllDataResponse();
                    req.currentStep = Step.InitData;
                }
                    break;
                case Step.GenerateNewClient:
                {
                    req.request = AppStoreOnBoardApi.GenerateNewClient();
                    req.resp = new AllDataResponse();
                    req.currentStep = Step.GenerateNewClient;
                }
                    break;
                case Step.LinkProjectId:
                {
                    m_ClientIdToBeLinked =
                        m_UDPLinkClientIdBlock.Q<TextField>(UiConstants.UiElementNames.UdpClientIdToBeLinked)?.value;
                    if (!string.IsNullOrEmpty(m_ClientIdToBeLinked))
                    {
                        m_ClientIdToBeLinked = m_ClientIdToBeLinked.Trim();
                        req.request = AppStoreOnBoardApi.LinkProjectId(m_ClientIdToBeLinked);
                        req.resp = new AllDataResponse();
                        req.currentStep = Step.LinkProjectId;
                    }
                }
                    break;
                case Step.UpdateAll:
                {
                    var iapItems = new List<SimpleIapPayload>();

                    var players = new List<SimplePlayerPayload>();
                    for (var i = 0; i < k_TestAccountsDirty.Count; i++)
                    {
                        var player = m_TestAccounts[i];

                        if (!k_TestAccountsDirty[i]) continue;
                        var p = new SimplePlayerPayload
                        {
                            id = player.playerId, email = player.email, password = player.password
                        };
                        players.Add(p);
                    }

                    var payload = new FullUpdatePayload()
                    {
                        clientId = m_UnityClientId.stringValue,
                        gameTitle = m_UDPBasicInfoBlock.Q<TextField>(UiConstants.UiElementNames.UDPGameTitle).value,
                        iapItems = iapItems.ToArray(),
                        testAccounts = players.ToArray(),
                        callbackUrl = m_UDPClientSettingsBlock
                            .Q<TextField>(UiConstants.UiElementNames.UDPClientCallbackUrl).value,
                    };

                    req.fullUpdatePayload = payload;
                    req.request = AppStoreOnBoardApi.UpdateAll(payload);
                    req.resp = new FullUpdateResponse();
                    req.currentStep = Step.UpdateAll;
                }
                    break;
            }

            return req;
        }

        // NOTICE: If you want to modify the format of the settings file.
        // Remember to check through the methods that are using it.
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
                        !string.IsNullOrEmpty(k_TestAccountsValidationMsg[pos]) ? DisplayStyle.Flex : DisplayStyle.None;
                }

                var playerEmail = accountUi.Q<TextField>("player-email-" + pos);
                var playerPassword = accountUi.Q<TextField>("player-password-" + pos);
                if (!string.IsNullOrEmpty(account.playerId))
                {
                    playerEmail.isReadOnly = true;
                    playerPassword.isReadOnly = true;
                }
                else
                {
                    playerEmail.isReadOnly = false;
                    playerPassword.isReadOnly = false;
                }
            }
        }

        private void RefreshAllInformation()
        {
            m_TestAccounts.Clear();
            k_TestAccountsDirty.Clear();
            k_TestAccountsValidationMsg.Clear();
            m_UpdateClientErrorMsg = "";
            m_UDPClientSettingsBlock.Q<Label>(UiConstants.UiElementNames.UDPClientErrorMessage).text =
                m_UpdateClientErrorMsg;

            Initialize();
        }

        private void DeleteTestAccount(TestAccount account, int pos)
        {
            m_IsTestAccountUpdating = true;
            UnityWebRequest request = AppStoreOnBoardApi.DeleteTestAccount(account.playerId);
            PlayerDeleteResponse response = new PlayerDeleteResponse();
            RequestStruct reqStruct = new RequestStruct()
            {
                request = request,
                resp = response,
                arrayPosition = pos,
                testAccount = account,
                currentStep = Step.DeletePlayer
            };
            k_RequestQueue.Enqueue(reqStruct);
        }

        private void OnPullButtonClick()
        {
            GUI.FocusControl(null);
            Action pulling = () =>
            {
                Debug.Log("Pulling...");
                ShowVisualElement(m_UDPPullingLabel);
                RefreshAllInformation();
            };

            if (AnythingChanged())
            {
                if (EditorUtility.DisplayDialog(
                    title: "Local changes may be overwritten",
                    message: "There are pending local edits that will be lost if you pull.",
                    ok: "Pull anyway",
                    cancel: "Cancel"))
                {
                    pulling();
                }
            }
            else
            {
                pulling();
            }
        }

        private void OnPushButtonClick()
        {
            var noError = true;
            // Update UDP Client Settings
            if (CallbackUrlChanged())
            {
                string callbackUrl = m_UDPClientSettingsBlock
                    .Q<TextField>(UiConstants.UiElementNames.UDPClientCallbackUrl).value;

                if (Utils.CheckURL(callbackUrl))
                {
                    m_UpdateClientErrorMsg = "";
                }
                else
                {
                    if (callbackUrl.StartsWith("https") || callbackUrl.StartsWith("http"))
                    {
                        m_UpdateClientErrorMsg = "Callback URL is invalid.";
                    }
                    else
                    {
                        m_UpdateClientErrorMsg = "Callback URL is invalid. (http/https is required)";
                    }

                    noError = false;
                }

                m_UDPClientSettingsBlock.Q<Label>(UiConstants.UiElementNames.UDPClientErrorMessage).text =
                    m_UpdateClientErrorMsg;
            }

            // Update Game Settings

            if (GameTitleChanged())
            {
                string gameTitle = m_UDPBasicInfoBlock.Q<TextField>(UiConstants.UiElementNames.UDPGameTitle).value;
                if (!string.IsNullOrEmpty(gameTitle))
                {
                    m_UpdateGameTitleErrorMsg = "";
                }
                else
                {
                    m_UpdateGameTitleErrorMsg = "Game title cannot be null";
                    noError = false;
                }

                m_UDPBasicInfoBlock.Q<Label>(UiConstants.UiElementNames.UDPGameErrorMessage).text =
                    m_UpdateGameTitleErrorMsg;
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
                    if (!string.IsNullOrEmpty(k_TestAccountsValidationMsg[pos]))
                    {
                        noError = false;
                        RepaintSingleTestAccount(pos);
                        Debug.LogError(
                            "[UDP] TestAccount:" + m_TestAccounts[pos].email + " " +
                            k_TestAccountsValidationMsg[pos]);
                    }
                }
            }

            if (noError)
            {
                // full update
                var req = BuildRequestFromStepName(Step.UpdateAll);
                if (!req.IsEmpty())
                {
                    m_IsOperationRunning = true;
                    ShowVisualElement(m_UDPPushingLabel);
                    Debug.Log("Pushing...");
                    k_RequestQueue.Enqueue(req);
                }
            }
        }

        private void OnCopyProjectIdButtonClick()
        {
            EditorGUIUtility.systemCopyBuffer = Application.cloudProjectId;
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
            GenerateNewClient();
        }

        private void GenerateNewClient()
        {
            Log("generate new client");
            m_IsGeneratingOrLinking = true;
            LoginRequired(Step.GenerateNewClient);
        }

        private void LinkProjectWithClient()
        {
            Log("link project id");
            m_IsGeneratingOrLinking = true;
            LoginRequired(Step.LinkProjectId);
        }

        private void OnLinkToExitingClientButtonClick()
        {
            m_ClientIdToBeLinked = m_UDPLinkClientIdBlock.Q<TextField>(UiConstants.UiElementNames.UdpClientIdToBeLinked)
                ?.value;
            if (!string.IsNullOrEmpty(m_ClientIdToBeLinked))
            {
                LinkProjectWithClient();
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

        private void ShowVisualElement(VisualElement visualElement)
        {
            if (visualElement != null)
            {
                visualElement.style.display = DisplayStyle.Flex;
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
            m_IsFetchingToken = false;
            m_IsOperationRunning = false;
        }

        private bool IsOperationRunning()
        {
            return m_IsOperationRunning ||
                   m_IsFetchingToken;
        }

        private bool AnythingChanged()
        {
            return GameTitleChanged() || CallbackUrlChanged() || TestAccountsChanged();
        }

        private bool GameTitleChanged()
        {
            var v = m_UDPBasicInfoBlock.Q<TextField>(UiConstants.UiElementNames.UDPGameTitle).value;
            return string.IsNullOrEmpty(v) || !v.Equals(m_AppName.stringValue);
        }

        private bool CallbackUrlChanged()
        {
            var v = m_UDPClientSettingsBlock.Q<TextField>(UiConstants.UiElementNames.UDPClientCallbackUrl).value;
            if (string.IsNullOrEmpty(v) && string.IsNullOrEmpty(m_CallbackUrlInMemory))
            {
                return false;
            }

            if (v == null)
            {
                v = "";
            }

            return !v.Equals(m_CallbackUrlInMemory);
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
//            Debug.Log(message);
        }

        #endregion
    }
}

#endif
