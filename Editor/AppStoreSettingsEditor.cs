#if (UNITY_5_6_OR_NEWER && !UNITY_5_6_0)

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine.Networking;

namespace UnityEngine.UDP.Editor
{
    [CustomEditor(typeof(AppStoreSettings))]
    public class AppStoreSettingsEditor : UnityEditor.Editor
    {
        public abstract class IMGUIAdaptor
        {
            public int foldOutLeftMargin { get; set; }
            public abstract void Icon();
            public abstract GUILayoutOption[] ButtonStyle();
        }
        
        public class v_2017Before : IMGUIAdaptor
        {
            private readonly GUILayoutOption[] _buttonStyles = {GUILayout.Width(180), GUILayout.Height(30)};
            public v_2017Before()
            {
                foldOutLeftMargin = 18;
            }
            
            public override void Icon()
            {
                GUILayout.Button(EditorGUIUtility.IconContent("d_console.infoicon"), 
                    EditorStyles.label, GUILayout.Width(35));
            }

            public override GUILayoutOption[] ButtonStyle()
            {
                return _buttonStyles;
            }
        }
        
        public class v_2018 : IMGUIAdaptor
        {
            private readonly GUILayoutOption[] _buttonStyles = {GUILayout.Width(200), GUILayout.Height(30)};
            public v_2018()
            {
                foldOutLeftMargin = 18;
            }

            public override void Icon()
            {
                GUILayout.Button(EditorGUIUtility.IconContent("d_console.infoicon"), 
                    EditorStyles.label, GUILayout.Width(35));
            }

            public override GUILayoutOption[] ButtonStyle()
            {
                return _buttonStyles;
            }
        }
        
        public class v_2019After : IMGUIAdaptor
        {
            private readonly GUILayoutOption[] _buttonStyles = {GUILayout.Width(200), GUILayout.Height(30)};
           
            public v_2019After()
            {
                foldOutLeftMargin = 9;
            }

            public override void Icon()
            {
                GUILayout.Button(EditorGUIUtility.IconContent("d_console.infoicon"),
                    new GUIStyle(EditorStyles.label)
                    {
                        margin = new RectOffset(10, 5, 5, 0)
                    }, GUILayout.Width(22));
            }

            public override GUILayoutOption[] ButtonStyle()
            {
                return _buttonStyles;
            }
        }
        
#if (UNITY_2020_1_OR_NEWER)
        [MenuItem("Window/Unity Distribution Portal/IAP Catalog", false, 111)]
        public static void ActivateSettingsWindow()
        {
            if (Utils.UnityIapExists())
            {
                // Unity IAP + UDP, then open the iap catalog window of Uni directly.
                EditorApplication.ExecuteMenuItem("Window/Unity IAP/IAP Catalog");
                return;
            }

            ActivateInspectorWindow();

            if (File.Exists(AppStoreSettings.appStoreSettingsAssetPath))
            {
                var existedAppStoreSettings = (AppStoreSettings) AssetDatabase.LoadAssetAtPath(
                    AppStoreSettings.appStoreSettingsAssetPath,
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
        }


        [MenuItem("Window/Unity Distribution Portal/Settings", false, 111)]
        public static void GoToUDPSettings()
        {
			SettingsService.OpenProjectSettings("Project/Services/Unity Distribution Portal");
        }

        [MenuItem("Window/Unity Distribution Portal/IAP Catalog", true)]
        [MenuItem("Window/Unity Distribution Portal/Settings", true)]
        public static bool CheckUnityOAuthValidation()
        {
            return OAuthEnabled;
        }
#else
        [MenuItem("Window/Unity Distribution Portal/Settings", false, 111)]
        public static void ActivateSettingsWindow()
        {
            ActivateInspectorWindow();

            if (File.Exists(AppStoreSettings.appStoreSettingsAssetPath))
            {
                var existedAppStoreSettings = (AppStoreSettings) AssetDatabase.LoadAssetAtPath(
                    AppStoreSettings.appStoreSettingsAssetPath,
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
        }

        [MenuItem("Window/Unity Distribution Portal/Settings", true)]
        public static bool CheckUnityOAuthValidation()
        {
            return OAuthEnabled;
        }

#endif

        private const bool debugEnabled = true;
        private const string defaultClientId = "Enter client ID";
        private string _clientIdToBeLinked = defaultClientId;
        private readonly string[] m_IapItemTypeOptions = {"Consumable", "Non consumable"};
        private static readonly bool OAuthEnabled = Utils.FindTypeByName("UnityEditor.Connect.UnityOAuth") != null;
        

#if(UNITY_2019_1_OR_NEWER)
        private IMGUIAdaptor UIAdaptor = new  v_2019After();
#elif(UNITY_2018_1_OR_NEWER) 
        private IMGUIAdaptor UIAdaptor = new v_2018();
#else
        private IMGUIAdaptor UIAdaptor =  new v_2017Before();
#endif

        private State currentState;
        private Queue<ReqStruct> requestQueue;

        private AppItem appItemInMemory;
        private string clientSecretInMemory;
        private string callbackUrlInMemory;

        private List<bool> m_IapItemsFoldout;
        private List<IapItem> m_IapItems;
        private List<bool> m_IapItemDirty;
        private List<string> m_IapValidationMsg;
        private bool m_InAppPurchaseFoldout;

        private bool isOperationRunning;
        private bool isGeneratingOrLinking;
        private bool isUpdatingIap;
        private string errorMsgGlobal;

        private string syncingStatus;
        private bool m_GenerateNewClientFoldOut;

#if (!UNITY_2020_1_OR_NEWER)
        private bool m_GameTitleChanged;
        private bool m_CallbackUrlChanged;
        private bool m_IsTestAccountUpdating;
        private bool m_TestAccountFoldout;
        private bool m_UdpClientSettingsFoldout;
        private string m_UpdateGameTitleErrorMsg;
        private string m_UpdateClientErrorMsg;
        private List<TestAccount> m_TestAccounts;
        private List<bool> m_TestAccountsDirty;
        private List<string> m_TestAccountsValidationMsg;
#endif

        // user state no need to check every frame, so we do checking every some seconds.
        private float checkTimer = 0.0f;
        private const float checkDuration = 100.0f;
        
        // switch the inspector window to save data
        private static bool refreshData = true;
        private static bool showInspectorCloseDialog = false;

        private static AppItem static_appItemInMemory;

        private static string static_callbackUrlInMemory;
        private static List<bool> static_m_IapItemsFoldout;
        private static List<IapItem> static_m_IapItems;
        private static List<bool> static_m_IapItemDirty;
        private static List<string> static_m_IapValidationMsg;

#if (!UNITY_2020_1_OR_NEWER)
        private static List<TestAccount> static_m_TestAccounts;
        private static List<bool> static_m_TestAccountsDirty;
        private static List<string> static_m_TestAccountsValidationMsg;
#endif

        private enum State
        {
            Initializing,
            NoOauth,
            Error,
            NoProjectId,
            LinkProject,
            Success,
        }

        private enum Step
        {
            InitData,
            FetchToken,
            GenerateNewClient,
            LinkProjectId,
            UpdateAll,
            UpdateIap,
            CreateIap,
            DeleteIap,
            DeletePlayer,
        }

        private struct ReqStruct
        {
            public UnityWebRequest request;
            public GeneralResponse response;
            public int arrayPos;
            public IapItem curIapItem;
#if (!UNITY_2020_1_OR_NEWER)
            public TestAccount testAccount;
#endif
            public Step currentStep;
            public Step nextStep;
            public FullUpdatePayload fullUpdatePayload;

            public bool IsEmpty()
            {
                return request == null;
            }
        }

        SerializedProperty m_UnityProjectId;
        SerializedProperty m_UnityClientId;
        SerializedProperty m_UnityClientKey;
        SerializedProperty m_UnityClientRsaPublicKey;
        SerializedProperty m_AppName;
        SerializedProperty m_AppSlug;
        SerializedProperty m_AppItemId;

#if(UNITY_2018_1_OR_NEWER)
        static bool WantsToQuit()
        {
            AppStoreSettingsEditor.showInspectorCloseDialog = false;
            return true;
        }
#endif

        private void OnEnable()
        {
            Log("enabled");
            RetrieveLocalSettings();
            InitParameters();
            EditorApplication.update -= CheckRequestUpdate;
            EditorApplication.update += CheckRequestUpdate;
            EditorApplication.update -= CheckUserState;
            EditorApplication.update += CheckUserState;
#if(UNITY_2018_1_OR_NEWER)
            EditorApplication.wantsToQuit -= WantsToQuit;
            EditorApplication.wantsToQuit += WantsToQuit;
#endif
            if (!OAuthEnabled)
            {
                currentState = State.NoOauth;
            }
            else if (string.IsNullOrEmpty(Application.cloudProjectId))
            {
                currentState = State.NoProjectId;
            }
            else
            {
                currentState = State.Initializing;
                Initialize();
            }
        }

        private void OnDestroy()
        {
            if (AppStoreSettingsEditor.showInspectorCloseDialog)
            {
                if (EditorUtility.DisplayDialog("Are you sure to leave?", "Your changes could not be saved.", "Leave",
                    "Cancel"))
                {
                    AppStoreSettingsEditor.refreshData = true;
                }
                else
                {
                    AppStoreSettingsEditor.refreshData = false;

                    AppStoreSettingsEditor.static_appItemInMemory = appItemInMemory;
                    AppStoreSettingsEditor.static_callbackUrlInMemory = callbackUrlInMemory;
                    AppStoreSettingsEditor.static_m_IapItemsFoldout = m_IapItemsFoldout;
                    AppStoreSettingsEditor.static_m_IapItems = m_IapItems;
                    AppStoreSettingsEditor.static_m_IapItemDirty = m_IapItemDirty;
                    AppStoreSettingsEditor.static_m_IapValidationMsg = m_IapValidationMsg;

#if (!UNITY_2020_1_OR_NEWER)
                    AppStoreSettingsEditor.static_m_TestAccounts = m_TestAccounts;
                    AppStoreSettingsEditor.static_m_TestAccountsDirty = m_TestAccountsDirty;
                    AppStoreSettingsEditor.static_m_TestAccountsValidationMsg = m_TestAccountsValidationMsg;
#endif
                    AppStoreSettingsEditor.ActivateSettingsWindow();
                }
            }
            Log("destroy");
            EditorApplication.update -= CheckRequestUpdate;
            EditorApplication.update -= CheckUserState;
        }
           
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var labelStyle = new GUIStyle(GUI.skin.label) {wordWrap = true, fontSize = 15};
            AppStoreSettingsEditor.showInspectorCloseDialog = false;

            switch (currentState)
            {
                case State.Initializing:
                    EditorGUILayout.LabelField("Loading...", labelStyle);
                    break;
                case State.NoOauth:
                    requestQueue.Clear();
                    const string msg =
                        "UDP editor extension can only work on Unity 5.6.1+. Please check your Unity version and retry.";
                    EditorGUILayout.LabelField(msg, labelStyle);
                    break;
                case State.Error:
                    EditorGUILayout.LabelField(errorMsgGlobal, labelStyle);
                    break;
                case State.NoProjectId:
                    RenderNoProjectIdView();
                    break;
                case State.LinkProject:
                    RenderLinkProjectView();
                    break;
                default:
                    AppStoreSettingsEditor.showInspectorCloseDialog = true;
                    RenderMainView();
                    break;
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
                    AppStoreOnBoardApi.tokenInfo = new TokenInfo();
                    InitParameters();
                    currentState = State.Initializing;
                    Initialize();
                }
            }
        }

        private void CheckRequestUpdate()
        {
            if (requestQueue.Count == 0)
            {
                return;
            }

            var currentRequestStruct = requestQueue.Dequeue();
            var request = currentRequestStruct.request;
            var response = currentRequestStruct.response;

            Action<Action> overSyncingWithFn = fn =>
            {
                isOperationRunning = false;
                syncingStatus = "";
                if (fn != null)
                {
                    fn();
                }
                Repaint();
            };

            if (request != null && request.isDone)
            {
                if (request.error != null || request.responseCode / 100 != 2)
                {
                    Log("request with error");
                    var errText = request.downloadHandler.text;
                    if (errText.Contains(AppStoreOnBoardApi.notAuthenticatedError))
                    {
                        // token invalid. reFetch it.
                        AppStoreOnBoardApi.tokenInfo = new TokenInfo();
                        var req = BuildRequestFromStepName(Step.FetchToken);
                        if (req.IsEmpty()) return;
                        req.nextStep = currentRequestStruct.currentStep;
                        requestQueue.Enqueue(req);
                    }
                    else if (errText.Contains(AppStoreOnBoardApi.authorizationError))
                    {
                        // no permission.
                        Debug.LogError(
                            "[UDP] authorization error, please ask your manager or owner to do this operation.");
                        EditorUtility.DisplayDialog("Error",
                            "authorization error, please ask your manager or owner to do this operation.",
                            "OK");

                        ResetAllStatus();
                        Repaint();
                    }
                    else if (errText.Contains(AppStoreOnBoardApi.clientNotLinked))
                    {
                        currentState = State.LinkProject;
                        ResetAllStatus();
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
                    Log("request successfully.");
                    if (response.GetType() == typeof(TokenInfo))
                    {
                        var res = JsonUtility.FromJson<TokenInfo>(request.downloadHandler.text);
                        AppStoreOnBoardApi.tokenInfo.token = res.token;
                        AppStoreOnBoardApi.tokenInfo.canManage = res.canManage;
                        AppStoreOnBoardApi.tokenInfo.orgId = res.orgId;

                        var req = BuildRequestFromStepName(currentRequestStruct.nextStep);
                        if (req.IsEmpty()) return;
                        requestQueue.Enqueue(req);
                    }
                    else if (response.GetType() == typeof(AllDataResponse))
                    {
                        currentState = State.Success;
                        var res = JsonUtility.FromJson<AllDataResponse>(request.downloadHandler.text);
                        LocalSaveAllDataFromServerResponse(res);
                        overSyncingWithFn(null);
                    }
                    else if (response.GetType() == typeof(UnityIapItemUpdateResponse))
                    {
                        ProcessIapResponse(currentRequestStruct, request);
                    }
                    else if (response.GetType() == typeof(UnityIapItemCreateResponse))
                    {
                        ProcessIapResponse(currentRequestStruct, request);
                    }
                    else if (response.GetType() == typeof(IapItemDeleteResponse))
                    {
                        isUpdatingIap = false;
                        RemoveIapItemLocally(currentRequestStruct.arrayPos);
                    }
                    else if (response.GetType() == typeof(FullUpdateResponse))
                    {
                        overSyncingWithFn(() =>
                        {
                            var res = JsonUtility.FromJson<FullUpdateResponse>(request.downloadHandler.text);

                            var errMsg = "";
                            if (!string.IsNullOrEmpty(res.gameTitleErrorMsg))
                            {
                                errMsg += "update game title failed: " + res.gameTitleErrorMsg + "\n";
                            }
                            else
                            {
#if (!UNITY_2020_1_OR_NEWER)
                                m_GameTitleChanged = false;
                                m_UpdateGameTitleErrorMsg = "";
#endif
                                LocalSaveAppItemFromServerResponse(res.allDataResponse.appItem);
                            }

                            if (!string.IsNullOrEmpty(res.callbackUrlErrorMsg))
                            {
                                errMsg += "update callback url failed: " + res.callbackUrlErrorMsg + "\n";
                            }
                            else
                            {
#if (!UNITY_2020_1_OR_NEWER)
                                m_CallbackUrlChanged = false;
                                m_UpdateClientErrorMsg = "";
#endif
                                LocalSaveClientSettingsFromServerResponse(res.allDataResponse.client);
                            }

                            LocalSaveIapItemsFromServerResponse(res.allDataResponse.iapItems);

                            var pushedIaps = currentRequestStruct.fullUpdatePayload.iapItems;
                            if (res.iapSuccess != null)
                            {
                                for (var i = 0; i < res.iapSuccess.Length; i++)
                                {
                                    if (string.IsNullOrEmpty(res.iapSuccess[i])) continue;
                                    var failedIap = pushedIaps[i];
                                    errMsg += "update iap " + failedIap.slug + " failed.\n";
                                    m_IapItems.Add(new IapItem()
                                    {
                                        id = failedIap.id, masterItemSlug = appItemInMemory.slug, slug = failedIap.slug,
                                        name = failedIap.name, consumable = failedIap.consumable,
                                        properties = new Properties() {description = failedIap.description},
                                        priceSets = Utils.FillUsdToPriceSet(failedIap.price)
                                    });
                                    m_IapItemDirty.Add(true);
                                    m_IapItemsFoldout.Add(true);
                                    m_IapValidationMsg.Add(res.iapSuccess[i]);
                                }
                            }

#if (!UNITY_2020_1_OR_NEWER)
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
                                        m_TestAccountsDirty.Add(true);
                                        m_TestAccountsValidationMsg.Add(res.testAccountSuccess[i]);
                                    }
                                }
                            }
#endif
                            if (!string.IsNullOrEmpty(errMsg))
                            {
                                Debug.LogError("[UDP] update all content failed: " + errMsg);
                                EditorUtility.DisplayDialog("Error",
                                    "[UDP] " + errMsg,
                                    "OK");
                            }
                        });
                    }
                    else if (response.GetType() == typeof(PlayerDeleteResponse))
                    {
#if (!UNITY_2020_1_OR_NEWER)
                        m_IsTestAccountUpdating = false;
                        EditorUtility.DisplayDialog("Success",
                            "TestAccount " + currentRequestStruct.testAccount.email + " has been Deleted.", "OK");
#endif
                        RemoveTestAccountLocally(currentRequestStruct.arrayPos);
                        Repaint();
                    }
                }
                request.Dispose();
            }
            else
            {
                requestQueue.Enqueue(currentRequestStruct);
            }
        }

        private void LoginRequired(Step nextStep)
        {
            if (AppStoreOnBoardApi.tokenInfo != null &&
                !string.IsNullOrEmpty(AppStoreOnBoardApi.tokenInfo.token))
            {
                var nextReq = BuildRequestFromStepName(nextStep);
                if (nextReq.IsEmpty()) return;
                nextReq.currentStep = nextStep;
                requestQueue.Enqueue(nextReq);
                return;
            }

            var req = BuildRequestFromStepName(Step.FetchToken);
            if (req.IsEmpty()) return;
            req.currentStep = Step.FetchToken;
            req.nextStep = nextStep;
            requestQueue.Enqueue(req);
        }

        private void Initialize()
        {
            Log("initialize");
#if (!UNITY_2020_1_OR_NEWER)
            m_UpdateGameTitleErrorMsg = "";
            m_UpdateClientErrorMsg = "";
            m_GameTitleChanged = false;
            m_CallbackUrlChanged = false;
            ClearTestAccounts();
#endif
            ClearIapItems();
            LoginRequired(Step.InitData);
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
        private void GenerateNewClient()
        {
            Log("generate new client");
            isGeneratingOrLinking = true;
            LoginRequired(Step.GenerateNewClient);
        }

        private void LinkProjectWithClient()
        {
            Log("link project id");
            isGeneratingOrLinking = true;
            LoginRequired(Step.LinkProjectId);
        }

        private void UpdateIapItem(IapItem iapItem, int pos)
        {
            isUpdatingIap = true;
            iapItem.status = "STAGE";
            UnityWebRequest request = AppStoreOnBoardApi.UpdateStoreItem(iapItem);
            UnityIapItemUpdateResponse resp = new UnityIapItemUpdateResponse();
            ReqStruct reqStruct = new ReqStruct
            {
                request = request,
                response = resp,
                arrayPos = pos,
                curIapItem = iapItem,
                currentStep = Step.UpdateIap
            };
            requestQueue.Enqueue(reqStruct);
        }

        private void CreateIapItem(IapItem iapItem, int pos)
        {
            isUpdatingIap = true;
            var request = AppStoreOnBoardApi.CreateStoreItem(iapItem);
            var clientResp = new UnityIapItemCreateResponse();
            var reqStruct = new ReqStruct
            {
                request = request, response = clientResp, curIapItem = iapItem, arrayPos = pos,
                currentStep = Step.CreateIap
            };
            requestQueue.Enqueue(reqStruct);
        }

        private void DeleteIapItem(IapItem iapItem, int pos)
        {
            isUpdatingIap = true;
            UnityWebRequest request = AppStoreOnBoardApi.DeleteStoreItem(iapItem.id);
            IapItemDeleteResponse clientResp = new IapItemDeleteResponse();
            ReqStruct reqStruct = new ReqStruct
            {
                request = request,
                response = clientResp,
                arrayPos = pos,
                curIapItem = iapItem,
                currentStep = Step.DeleteIap
            };
            requestQueue.Enqueue(reqStruct);
        }

        #region UI functions

        public void RenderNoProjectIdView()
        {
            var labelStyle = new GUIStyle(GUI.skin.label) {wordWrap = true, fontSize = 15};
            const string showingMsg =
                "To use the Unity distribution portal your project will need a Unity project ID. You can create a new project ID or link to an existing one in the Services window.";
            EditorGUILayout.LabelField(showingMsg, labelStyle);

            if (GUILayout.Button("Go to the Services Window"))
            {
#if (UNITY_2020_1_OR_NEWER)
                SettingsService.OpenProjectSettings("Project/Services/Unity Distribution Portal");
#elif UNITY_2018_2_OR_NEWER
                EditorApplication.ExecuteMenuItem("Window/General/Services");
#else
                EditorApplication.ExecuteMenuItem("Window/Services");
#endif
                Selection.activeObject = null;
            }
        }

        private Action<string> HinterFn(GUIStyle g)
        {
            return hint =>
            {
                EditorGUILayout.LabelField(hint, g);
            };
        }

        public void RenderLinkProjectView()
        {
            Func<float, float, GUIStyle> stretchHinterStyle = (minViewWidth, fixedWidth) =>
            {
                var ret = new GUIStyle(GUI.skin.label) {wordWrap = true};
                if (EditorGUIUtility.currentViewWidth < minViewWidth)
                {
                    ret.stretchWidth = false;
                    ret.stretchHeight = false;
                    ret.fixedWidth = fixedWidth;
                }

                return ret;
            };

            Action<string> headerHinter = HinterFn(stretchHinterStyle(320, 300));
            Action<string> hinter = HinterFn(new GUIStyle(GUI.skin.label) {wordWrap = true});

            Action goToPortalLabel = () => HorizontalGroup(() =>
            {
                
                HinterFn(stretchHinterStyle(320, 130))("-  On the UDP Console");
                if (GUILayout.Button("Go",  new GUIStyle(EditorStyles.miniButton) {alignment = TextAnchor.MiddleCenter}))
                {
                    Application.OpenURL(BuildConfig.UDP_ENDPOINT);
                }

                ;
                HinterFn(stretchHinterStyle(320, 125))(", select your game.");
                SpaceHolder();
            });

            GUILayoutOption[] buttonStyleOptions = UIAdaptor.ButtonStyle();

            var labelWidth = Convert.ToSingle(Math.Max(EditorGUIUtility.currentViewWidth * 0.75, 150));

            var titleHints = new List<string>
            {
                "Your Unity Project must be linked to your UDP game. You need your UDP game's Client ID to do this.",
                "Follow these steps:"
            };

            var hintSteps = new List<string>
            {
                "-  Go to its Game Info page.",
                "-  Go to the Integration Information section and copy the Client ID.",
                "-  Paste the Client ID below to link your UDP game to your Unity Project."
            };

            var generateClientHints = new List<string>
            {
                "Creating a new UDP game from here will generate a new Client ID and automatically link it to this Unity Project.",
                "Your new game will also appear on the UDP Console, under your current Unity organization.",
                "Consider carefully whether you or your team mates have created a game on the UDP Console already.",
                "Be aware that once a Unity Project ID and a UDP Client ID are linked, only organization Managers and Owners can unlink them. In general, we recommend you create new games from the UDP Console."
            };

            var linkButtonText = "Link Project to this UDP game";

            var foldOutLabel = "  Create your UDP game directly from inside the Unity Editor";

            var generateButtonText = "Create new UDP game anyway";

            EditorGUI.BeginDisabledGroup(isGeneratingOrLinking);

            // hints
            VerticalGroupWithStyle(GUILayout.Height(150))(() =>
            {
                titleHints.ForEach(s => headerHinter(s));

                SpaceHolder();

                goToPortalLabel();

                hintSteps.ForEach(s => headerHinter(s));

                SpaceHolder();
            });

            // link group
            VerticalGroupWithStyle(GUILayout.Height(110))(() =>
                {
                    VerticalGroupWithStyle(GUILayout.Height(60))(() =>
                    {
                        HorizontalGroup(() =>
                        {
                            _clientIdToBeLinked = EditorGUILayout.TextField(
                                text: _clientIdToBeLinked,
                                style: new GUIStyle(EditorStyles.textField)
                                {
                                    fixedWidth = labelWidth,
                                    fixedHeight = 20,
                                    fontSize = 13,
                                    wordWrap = true
                                });

                            SpaceHolder();
                        });

                        SpaceHolder();

                        HorizontalGroup(() =>
                        {
                            if (GUILayout.Button(linkButtonText, buttonStyleOptions))
                            {
                                if (!string.IsNullOrEmpty(_clientIdToBeLinked) &&
                                    _clientIdToBeLinked != defaultClientId)
                                {
                                    LinkProjectWithClient();
                                }
                            }

                            SpaceHolder();
                        });
                    });

                    SpaceHolder();
                }
            );

            // generate client group
            VerticalGroupWithStyle(GUILayout.Height(200))(() =>
            {
                m_GenerateNewClientFoldOut = EditorGUILayout.Foldout(
                    foldout: m_GenerateNewClientFoldOut,
                    content: foldOutLabel,
                    toggleOnLabelClick: false,
                    style: new GUIStyle(EditorStyles.toggle)
                    {
                        fixedWidth = 500,
                        margin = new RectOffset(UIAdaptor.foldOutLeftMargin, 0, 0, 0),
                        alignment = TextAnchor.UpperLeft
                    }
                );

                if (m_GenerateNewClientFoldOut)
                {
                    SpaceHolder(15);

                    HorizontalGroupWithStyle(GUILayout.MaxWidth(650))(() =>
                    {
                        UIAdaptor.Icon();

                        SpaceHolder();

                        VerticalGroupWithStyle()(() =>
                        {
                            // toggle hints  	
                            generateClientHints.ForEach(s =>
                            {
                                SpaceHolder(10);
                                hinter(s);
                            });

                            SpaceHolder(15);

                            if (GUILayout.Button(generateButtonText, buttonStyleOptions))
                            {
                                isOperationRunning = true;
                                GenerateNewClient();
                            }
                        });
                    });
                }
            });

            EditorGUI.EndDisabledGroup();
        }

        private void SpaceHolder(){
            GUILayout.FlexibleSpace();
        }

        private void SpaceHolder(float pixels)
        {
            GUILayout.Space(pixels);
        }

        // private void DisabledGroupWithFn(bool signal, Action f)
        // {
        //     EditorGUI.BeginDisabledGroup(signal);
        //     f();
        //     EditorGUI.EndDisabledGroup();   
        // }
        
        private Action<Action> HorizontalGroupWithStyle(params GUILayoutOption[] options)
        {
            return f =>
            {
                EditorGUILayout.BeginHorizontal(options);
                if (f != null)
                {
                    f();
                }
                EditorGUILayout.EndHorizontal();
            };
        }
        
        private void HorizontalGroup(Action f)
        {
            EditorGUILayout.BeginHorizontal();
            if (f != null)
            {
                f();
            }
            EditorGUILayout.EndHorizontal(); 
        }

        private Action<Action> VerticalGroupWithStyle(params GUILayoutOption[] options)
        {
            return f =>
            {
                EditorGUILayout.BeginVertical(options);
                if (f != null)
                {
                    f();
                }
                EditorGUILayout.EndVertical();
            };
        }

        private List<string> headerHints()
        {
#if(UNITY_2020_1_OR_NEWER)
            return new List<string>{
                "The IAP Catalog lives on the server. Your game always fetches the IAP catalog that’s set on the server side.",
                "",
                "'Pull' retrieves your last-saved catalog from the UDP server.",
                "'Push' saves all your catalog changes to the UDP server. You can also save individual IAP products from inside the catalog.",
                "",
                "Note: the UDP Console offers more IAP Catalog management features, such as CSV import of your entire catalog. Prices in local currencies and localized product descriptions are managed from the UDP Console."
            };
#else    
            return new List<string>{
                "UDP settings are saved on the server; not locally.",
                "'Pull' retrieves your last-saved settings from the UDP server.",
                "'Push' saves your changes to the UDP server."
            };
#endif
        }

        private void RenderMainView()
        {
            // Main Display
            EditorGUI.BeginDisabledGroup(isOperationRunning);

            Action<string> hinter = HinterFn(new GUIStyle(GUI.skin.label) {wordWrap = true});
            
            headerHints().ForEach(s=> hinter(s));

            HorizontalGroup(() => {
                SpaceHolder();
                hinter(syncingStatus);
            });
            
            HorizontalGroup(() => {
                SpaceHolder();
                RenderHeader();
            });

            GuiLine();

#if (!UNITY_2020_1_OR_NEWER)
            RenderTitleProjectId();
#endif

            RenderIapSection();

#if (!UNITY_2020_1_OR_NEWER)

            RenderClientSettings();

            RenderPlayers();

            RenderFooter();
#endif
            EditorGUI.EndDisabledGroup();
        }

        private void RenderHeader()
        {
            Action pulling = () =>
            {
                isOperationRunning = true;
                Initialize();
                syncingStatus = "Pulling...";
                AppStoreSettingsEditor.refreshData = true;
                Debug.Log("pulling...");
            };

            Action<ReqStruct> pushing = req =>
            {
                isOperationRunning = true;
                requestQueue.Enqueue(req);
                syncingStatus = "Pushing...";
                Debug.Log("Pushing...");
            };

            if (GUILayout.Button("Pull", GUILayout.Width(AppStoreStyles.kAppStoreSettingsButtonWidth)))
            {
                GUI.FocusControl(null);
                if (AnythingChanged())
                {
                    if (EditorUtility.DisplayDialog(
                        title:   "Local changes may be overwritten",
                        message: "There are pending local edits that will be lost if you pull.",
                        ok:      "Pull anyway",
                        cancel:  "Cancel"))
                    {
                        pulling();
                    }
                }
                else
                {
                    pulling();
                }
            }

            if (GUILayout.Button("Push", GUILayout.Width(AppStoreStyles.kAppStoreSettingsButtonWidth)))
            {
                var noError = true;

                // Slug check locally
                var slugs = new HashSet<String>();

                // Update IAP Items
                for (var i = 0; i < m_IapItemDirty.Count; i++)
                {
                    var pos = i;
                    if (!m_IapItemDirty[pos]) continue;

                    //Check validation
                    m_IapValidationMsg[pos] = m_IapItems[pos].Validate();
                    if (string.IsNullOrEmpty(m_IapValidationMsg[pos]))
                    {
                        m_IapValidationMsg[pos] = m_IapItems[pos].SlugValidate(slugs);
                    }

                    if (!string.IsNullOrEmpty(m_IapValidationMsg[pos]))
                    {
                        noError = false;
                        Debug.LogError(
                            "[UDP] Iap:" + m_IapItems[pos].slug + " " + m_IapValidationMsg[pos]);
                    }
                }

#if (!UNITY_2020_1_OR_NEWER)
                // Update UDP Client Settings
                if (m_CallbackUrlChanged)
                {
                    if (Utils.CheckURL(callbackUrlInMemory))
                    {
                        m_UpdateClientErrorMsg = "";
                    }
                    else
                    {
                        noError = false;
                        if (callbackUrlInMemory.StartsWith("https") || callbackUrlInMemory.StartsWith("http"))
                        {
                            m_UpdateClientErrorMsg = "Callback URL is invalid.";
                        }
                        else
                        {
                            m_UpdateClientErrorMsg = "Callback URL is invalid. (http/https is required)";
                        }
                    }
                }

                // Update Game Settings
                if (m_GameTitleChanged)
                {
                    if (!string.IsNullOrEmpty(appItemInMemory.name))
                    {
                        m_UpdateGameTitleErrorMsg = "";
                    }
                    else
                    {
                        noError = false;
                        m_UpdateGameTitleErrorMsg = "Game title cannot be null";
                    }
                }

                // Update Test Accounts
                for (var i = 0; i < m_TestAccounts.Count; i++)
                {
                    var pos = i;
                    if (m_TestAccountsDirty[pos])
                    {
                        m_TestAccountsValidationMsg[pos] = m_TestAccounts[pos].Validate();
                        if (!string.IsNullOrEmpty(m_TestAccountsValidationMsg[pos]))
                        {
                            noError = false;
                            Debug.LogError(
                                "[UDP] TestAccount:" + m_TestAccounts[pos].email + " " +
                                m_TestAccountsValidationMsg[pos]);
                        }
                    }
                }
#endif
                Repaint();

                if (noError)
                {
                    // full update
                    var req = BuildRequestFromStepName(Step.UpdateAll);
                    if (!req.IsEmpty())
                    {
                        pushing(req);
                    }
                }
            }
        }

        private void RenderTitleProjectId()
        {
            #region Title & ProjectID

#if (!UNITY_2020_1_OR_NEWER)
            {
                EditorGUILayout.LabelField("Game Title");
                if (!string.IsNullOrEmpty(m_UpdateGameTitleErrorMsg))
                {
                    var textStyle = new GUIStyle(GUI.skin.label)
                    {
                        wordWrap = true, normal = {textColor = Color.red}
                    };
                    EditorGUILayout.LabelField(m_UpdateGameTitleErrorMsg, textStyle);
                }

                EditorGUI.BeginChangeCheck();
                appItemInMemory.name = EditorGUILayout.TextField(appItemInMemory.name);

                if (GUI.changed)
                {
                    m_GameTitleChanged = true;
                }

                EditorGUI.EndChangeCheck();
            }

            {
                EditorGUILayout.LabelField("Unity Project ID");
                EditorGUILayout.BeginHorizontal();
                InsertDisabledLabel(Application.cloudProjectId);
                if (GUILayout.Button("Copy", GUILayout.Width(AppStoreStyles.kCopyButtonWidth)))
                {
                    var te = new TextEditor {text = Application.cloudProjectId};
                    te.SelectAll();
                    te.Copy();
                }

                EditorGUILayout.EndHorizontal();
                GuiLine();
            }
#endif

            #endregion
        }

        private void RenderIapSection()
        {
            #region In App Purchase Configuration
            
                EditorGUI.BeginDisabledGroup(isUpdatingIap);
                var currentRect = EditorGUILayout.BeginVertical();
                m_InAppPurchaseFoldout = EditorGUILayout.Foldout(m_InAppPurchaseFoldout, "IAP Catalog", true,
                    AppStoreStyles.KAppStoreSettingsHeaderGuiStyle);
                if (m_InAppPurchaseFoldout)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.LabelField(new Rect(currentRect.xMax - 120, currentRect.yMin, 120, 20),
                        string.Format("{0} total ({1} edited)", m_IapItems.Count, EditedIapCount()),
                        new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleRight});
                    for (var i = 0; i < m_IapItemsFoldout.Count; i++)
                    {
                        currentRect = EditorGUILayout.BeginVertical();
                        var pos = i;

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
                                if (!m_IapItemDirty[pos]) return;
                                m_IapValidationMsg[pos] = m_IapItems[pos].Validate();
                                if (!string.IsNullOrEmpty(m_IapValidationMsg[pos]))
                                {
                                    Debug.LogError(
                                        "[UDP] Iap:" + m_IapItems[pos].slug + " " +
                                        m_IapValidationMsg[pos]);
                                }

                                if (string.IsNullOrEmpty(m_IapValidationMsg[pos]))
                                {
                                    // If check succeeds
                                    if (!string.IsNullOrEmpty(m_IapItems[pos].id))
                                    {
                                        UpdateIapItem(m_IapItems[pos], pos);
                                    }
                                    else
                                    {
                                        CreateIapItem(m_IapItems[pos], pos);
                                    }
                                }
                                else
                                {
                                    Repaint();
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
                                    DeleteIapItem(m_IapItems[pos], pos);
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

                            PriceDetail pd = Utils.ExtractUSDPrice(item);
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
                        if (GUILayout.Button("Add New IAP Product", new GUIStyle(GUI.skin.button)
                            {
                                fontSize = AppStoreStyles.kAddNewIapButtonFontSize
                            },
                            GUILayout.Height(EditorGUIUtility.singleLineHeight *
                                             AppStoreStyles.kAddNewIapButtonRatio),
                            GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
                        {
                            AddIapItem(new IapItem
                            {
                                masterItemSlug = appItemInMemory.slug,
                            }, true, true);
                        }

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.EndVertical();
                EditorGUI.EndDisabledGroup();
                GuiLine();
#pragma warning restore

            #endregion
        }

        private void RenderClientSettings()
        {
            #region UDP Client Settings

#if (!UNITY_2020_1_OR_NEWER)
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

                LabelWithReadonlyTextField("Game ID", appItemInMemory.slug);
                LabelWithReadonlyTextField("Client ID", m_UnityClientId.stringValue);
                LabelWithReadonlyTextField("Client Key", m_UnityClientKey.stringValue);
                LabelWithReadonlyTextField("RSA Public Key", m_UnityClientRsaPublicKey.stringValue);
                LabelWithReadonlyTextField("Client Secret", clientSecretInMemory);

                EditorGUI.BeginChangeCheck();
                callbackUrlInMemory = LabelWithTextField("Callback URL", callbackUrlInMemory);

                if (GUI.changed)
                {
                    m_CallbackUrlChanged = true;
                }

                EditorGUI.EndChangeCheck();

                EditorGUI.indentLevel--;
            }

            GuiLine();
#endif

            #endregion
        }

        private void RenderPlayers()
        {
            #region Test Accounts

#if (!UNITY_2020_1_OR_NEWER)
            EditorGUI.BeginDisabledGroup(m_IsTestAccountUpdating);

            m_TestAccountFoldout = EditorGUILayout.Foldout(m_TestAccountFoldout, "UDP Sandbox Test Accounts",
                true,
                AppStoreStyles.KAppStoreSettingsHeaderGuiStyle);

            if (m_TestAccountFoldout)
            {
                for (var i = 0; i < m_TestAccounts.Count; i++)
                {
                    var pos = i;

                    if (!string.IsNullOrEmpty(m_TestAccountsValidationMsg[pos]))
                    {
                        GUIStyle textStyle = new GUIStyle(GUI.skin.label);
                        textStyle.wordWrap = true;
                        textStyle.normal.textColor = Color.red;

                        EditorGUILayout.LabelField(m_TestAccountsValidationMsg[pos], textStyle);
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    if (string.IsNullOrEmpty(m_TestAccounts[pos].playerId))
                    {
                        m_TestAccounts[pos].email = EditorGUILayout.TextField(m_TestAccounts[pos].email);
                        m_TestAccounts[pos].password =
                            EditorGUILayout.PasswordField(m_TestAccounts[pos].password);
                    }
                    else
                    {
                        InsertDisabledLabel(m_TestAccounts[pos].email);
                        InsertDisabledLabel(m_TestAccounts[pos].password);
                    }

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
#endif

            #endregion
        }

        protected virtual void RenderFooter()
        {
            #region Go to Portal

#if (!UNITY_2020_1_OR_NEWER)
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Go to UDP console", GUILayout.Width(AppStoreStyles.kGoToPortalButtonWidth)))
                {
                    Application.OpenURL(BuildConfig.UDP_ENDPOINT);
                }

                EditorGUILayout.EndHorizontal();
            }
#endif

            #endregion
        }

        #endregion

        private void InitParameters()
        {
            appItemInMemory = new AppItem();
            requestQueue = new Queue<ReqStruct>();
            currentState = State.Initializing;

            m_IapItemsFoldout = new List<bool>();
            m_IapItems = new List<IapItem>();
            m_IapItemDirty = new List<bool>();
            m_IapValidationMsg = new List<string>();

            isOperationRunning = false;
            isGeneratingOrLinking = false;
            isUpdatingIap = false;
            m_InAppPurchaseFoldout = true;
            errorMsgGlobal = "";
            syncingStatus = "";
            m_GenerateNewClientFoldOut = false;

#if (!UNITY_2020_1_OR_NEWER)
            m_GameTitleChanged = false;
            m_CallbackUrlChanged = false;
            m_IsTestAccountUpdating = false;
            m_TestAccountFoldout = false;
            m_UdpClientSettingsFoldout = false;
            m_UpdateClientErrorMsg = "";
            m_UpdateGameTitleErrorMsg = "";
            m_TestAccounts = new List<TestAccount>();
            m_TestAccounts = new List<TestAccount>();
            m_TestAccountsDirty = new List<bool>();
            m_TestAccountsValidationMsg = new List<string>();
#endif
            AppStoreOnBoardApi.unityOrgId = Utils.GetOrganizationId();
            AppStoreOnBoardApi.unityUserId = Utils.GetUserId();
            AppStoreOnBoardApi.unityProjectId = Application.cloudProjectId;
            AppStoreOnBoardApi.unityAccessToken = Utils.GetAccessToken();
        }

        private void RetrieveLocalSettings()
        {
            m_UnityProjectId = serializedObject.FindProperty("UnityProjectID");
            m_UnityClientId = serializedObject.FindProperty("UnityClientID");
            m_UnityClientKey = serializedObject.FindProperty("UnityClientKey");
            m_UnityClientRsaPublicKey = serializedObject.FindProperty("UnityClientRSAPublicKey");
            m_AppName = serializedObject.FindProperty("AppName");
            m_AppSlug = serializedObject.FindProperty("AppSlug");
            m_AppItemId = serializedObject.FindProperty("AppItemId");
        }

        private void LocalSaveAppItemFromServerResponse(AppItemResponse appItem)
        {
            if (appItem == null) return;
            m_AppItemId.stringValue = appItem.id;
            m_AppName.stringValue = appItem.name;
            m_AppSlug.stringValue = appItem.slug;
            if (AppStoreSettingsEditor.refreshData)
            {
                appItemInMemory.id = appItem.id;
                appItemInMemory.name = appItem.name;
                appItemInMemory.slug = appItem.slug;
                appItemInMemory.ownerId = appItem.ownerId;
                appItemInMemory.ownerType = appItem.ownerType;
                appItemInMemory.status = appItem.status;
                appItemInMemory.type = appItem.type;
                appItemInMemory.clientId = appItem.clientId;
                appItemInMemory.packageName = appItem.packageName;
                appItemInMemory.revision = appItem.revision;
            }
            else
            {
#if (!UNITY_2020_1_OR_NEWER)
                m_GameTitleChanged = AppStoreSettingsEditor.static_appItemInMemory != null && AppStoreSettingsEditor.static_appItemInMemory.name != appItem.name;
#endif
                appItemInMemory = AppStoreSettingsEditor.static_appItemInMemory;
            }
            
            serializedObject.ApplyModifiedProperties();
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
                clientSecretInMemory = client.channel.channelSecret;
                callbackUrlInMemory = client.channel.callbackUrl;
            }
            else
            {
                m_UnityClientRsaPublicKey.stringValue = "";
                m_UnityProjectId.stringValue = "";
                clientSecretInMemory = "";
                callbackUrlInMemory = "";
            }

            if (!AppStoreSettingsEditor.refreshData)
            {
#if (!UNITY_2020_1_OR_NEWER)
              m_CallbackUrlChanged = AppStoreSettingsEditor.static_callbackUrlInMemory != callbackUrlInMemory;
#endif
                callbackUrlInMemory = AppStoreSettingsEditor.static_callbackUrlInMemory;
            }

            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();

            saveGameSettingsProps(client.client_id);
        }

        private void LocalSaveIapItemsFromServerResponse(IEnumerable<IapItem> iapItems)
        {
            ClearIapInfosInMemory();
            if (iapItems == null) return;

            if (AppStoreSettingsEditor.refreshData)
            {
                foreach (var item in iapItems)
                {
                    m_IapItems.Add(item);
                    m_IapItemDirty.Add(false);
                    m_IapItemsFoldout.Add(false);
                    m_IapValidationMsg.Add("");
                }
            }
            else
            {
                m_IapItems = AppStoreSettingsEditor.static_m_IapItems;
                m_IapItemDirty = AppStoreSettingsEditor.static_m_IapItemDirty;
                m_IapItemsFoldout = AppStoreSettingsEditor.static_m_IapItemsFoldout;
                m_IapValidationMsg = AppStoreSettingsEditor.static_m_IapValidationMsg;
            }
            
        }

        private void LocalSavePlayerFromServerResponse(IEnumerable<PlayerResponse> players)
        {
#if (!UNITY_2020_1_OR_NEWER)
            if (players == null) return;
            ClearPlayersInMemory();
            if (AppStoreSettingsEditor.refreshData)
            {
                foreach (var player in players)
                {
                    m_TestAccounts.Add(new TestAccount()
                        {playerId = player.id, password = "******", email = player.nickName});
                    m_TestAccountsDirty.Add(false);
                    m_TestAccountsValidationMsg.Add("");
                }
            }
            else
            {
                m_TestAccounts = AppStoreSettingsEditor.static_m_TestAccounts;
                m_TestAccountsDirty = AppStoreSettingsEditor.static_m_TestAccountsDirty;
                m_TestAccountsValidationMsg = AppStoreSettingsEditor.static_m_TestAccountsValidationMsg;
            }    
#endif
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

        private void AddIapItem(IapItem item, bool dirty = true, bool foldout = false)
        {
            m_IapItems.Add(item);
            m_IapItemDirty.Add(dirty);
            m_IapItemsFoldout.Add(foldout);
            m_IapValidationMsg.Add("");
        }


        private void RemoveTestAccountLocally(int pos)
        {
#if (!UNITY_2020_1_OR_NEWER)
            m_TestAccounts.RemoveAt(pos);
            m_TestAccountsDirty.RemoveAt(pos);
            m_TestAccountsValidationMsg.RemoveAt(pos);
#endif
        }

        private void DeleteTestAccount(TestAccount account, int pos)
        {
#if (!UNITY_2020_1_OR_NEWER)
            m_IsTestAccountUpdating = true;

            UnityWebRequest request = AppStoreOnBoardApi.DeleteTestAccount(account.playerId);
            PlayerDeleteResponse response = new PlayerDeleteResponse();
            ReqStruct reqStruct = new ReqStruct
            {
                request = request,
                response = response,
                arrayPos = pos,
                testAccount = account,
                currentStep = Step.DeletePlayer
            };
            requestQueue.Enqueue(reqStruct);
#endif
        }

        private void RemoveIapItemLocally(int pos)
        {
            m_IapItems.RemoveAt(pos);
            m_IapItemDirty.RemoveAt(pos);
            m_IapItemsFoldout.RemoveAt(pos);
            m_IapValidationMsg.RemoveAt(pos);
        }

        private void ProcessIapResponse(ReqStruct reqStruct, UnityWebRequest request)
        {
            reqStruct.curIapItem = JsonUtility.FromJson<IapItem>(HandleIapItemResponse(request.downloadHandler.text));
            m_IapItemDirty[reqStruct.arrayPos] = false;
            m_IapItems[reqStruct.arrayPos] = reqStruct.curIapItem; // add id information
            isUpdatingIap = false;
            Repaint();
        }

        private ReqStruct BuildRequestFromStepName(Step targetStep)
        {
            var req = new ReqStruct();
            switch (targetStep)
            {
                case Step.FetchToken:
                {
                    try
                    {
                        var unityToken = Utils.GetAccessToken();

                        // exchange udp token
                        req.request = AppStoreOnBoardApi.GetUdpToken(unityToken, Application.cloudProjectId);
                        req.response = new TokenInfo();
                        req.currentStep = Step.FetchToken;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        currentState = State.NoOauth;
                    }
                }
                    break;
                case Step.InitData:
                {
                    req.request = AppStoreOnBoardApi.FetchData();
                    req.response = new AllDataResponse();
                    req.currentStep = Step.InitData;
                }
                    break;
                case Step.GenerateNewClient:
                {
                    req.request = AppStoreOnBoardApi.GenerateNewClient();
                    req.response = new AllDataResponse();
                    req.currentStep = Step.GenerateNewClient;
                }
                    break;
                case Step.LinkProjectId:
                {
                    if (_clientIdToBeLinked != "" && _clientIdToBeLinked != defaultClientId)
                    {
                        req.request = AppStoreOnBoardApi.LinkProjectId(_clientIdToBeLinked);
                        req.response = new AllDataResponse();
                        req.currentStep = Step.LinkProjectId;
                    }
                }
                    break;
                case Step.UpdateAll:
                {
                    var iapItems = new List<SimpleIapPayload>();
                    for (var i = 0; i < m_IapItemDirty.Count; i++)
                    {
                        if (!m_IapItemDirty[i]) continue;
                        var iap = m_IapItems[i];
                        var p = new SimpleIapPayload()
                        {
                            id = iap.id,
                            slug = iap.slug,
                            name = iap.name,
                            consumable = iap.consumable,
                            price = Utils.ExtractUSDPrice(iap).price,
                        };
                        if (iap.properties != null)
                        {
                            p.description = iap.properties.description;
                        }

                        iapItems.Add(p);
                    }

                    var players = new List<SimplePlayerPayload>();

#if (!UNITY_2020_1_OR_NEWER)
                    for (var i = 0; i < m_TestAccountsDirty.Count; i++)
                    {
                        var player = m_TestAccounts[i];

                        if (!m_TestAccountsDirty[i]) continue;
                        var p = new SimplePlayerPayload
                        {
                            id = player.playerId, email = player.email, password = player.password
                        };
                        players.Add(p);
                    }
#endif

                    var payload = new FullUpdatePayload()
                    {
                        clientId = m_UnityClientId.stringValue,
                        gameTitle = appItemInMemory.name,
                        iapItems = iapItems.ToArray(),
                    };
#if (!UNITY_2020_1_OR_NEWER)
                    payload.testAccounts = players.ToArray();

#endif
                    payload.callbackUrl = callbackUrlInMemory;
                    req.fullUpdatePayload = payload;
                    req.request = AppStoreOnBoardApi.UpdateAll(payload);
                    req.response = new FullUpdateResponse();
                    req.currentStep = Step.UpdateAll;
                }
                    break;
            }

            return req;
        }

        private void ProcessErrorResponse(ReqStruct reqStruct)
        {
            var request = reqStruct.request;
            var response = reqStruct.response;
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
                currentState = State.Error;
                errorMsgGlobal = "something happened during authorization, please try again.";
                return;
            }

            if (reqStruct.currentStep == Step.LinkProjectId || reqStruct.currentStep == Step.GenerateNewClient)
            {
                isGeneratingOrLinking = false;
            }

            if (reqStruct.currentStep == Step.CreateIap || reqStruct.currentStep == Step.UpdateIap ||
                reqStruct.currentStep == Step.DeleteIap)
            {
                isUpdatingIap = false;
                if (resErr != null && resErr.message != null)
                {
                    m_IapValidationMsg[reqStruct.arrayPos] = resErr.message;
                }
            }

            if (reqStruct.currentStep == Step.DeletePlayer)
            {
#if (!UNITY_2020_1_OR_NEWER)
                m_IsTestAccountUpdating = false;
                if (resErr != null && resErr.message != null)
                {
                    m_TestAccountsValidationMsg[reqStruct.arrayPos] = resErr.message;
                }
#endif
            }

            if (reqStruct.currentStep == Step.UpdateAll)
            {
                isOperationRunning = false;
            }
        }

        private void LocalSaveAllDataFromServerResponse(AllDataResponse all)
        {
            if (all == null) return;
            LocalSaveAppItemFromServerResponse(all.appItem);

            LocalSaveIapItemsFromServerResponse(all.iapItems);

            LocalSaveClientSettingsFromServerResponse(all.client);

            LocalSavePlayerFromServerResponse(all.players);
        }

        private void ResetAllStatus()
        {
            isUpdatingIap = false;
            isOperationRunning = false;
            isGeneratingOrLinking = false;
#if (!UNITY_2020_1_OR_NEWER)
            m_IsTestAccountUpdating = false;
#endif
        }

        private void ClearIapInfosInMemory()
        {
            m_IapItems.Clear();
            m_IapItemDirty.Clear();
            m_IapItemsFoldout.Clear();
            m_IapValidationMsg.Clear();
        }

        private void ClearPlayersInMemory()
        {
#if (!UNITY_2020_1_OR_NEWER)
            m_TestAccounts.Clear();
            m_TestAccountsDirty.Clear();
            m_TestAccountsValidationMsg.Clear();
#endif
        }

        #region  helper functions

        void LabelWithReadonlyTextField(string labelText, string textValue = "",
            float labelWidth = AppStoreStyles.kClientLabelWidth)
        {
            GUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField(labelText, GUILayout.Width(labelWidth));
            InsertDisabledLabel(textValue);
           
            GUILayout.EndHorizontal();
        }

        void InsertDisabledLabel(string labelText)
        {
            EditorGUI.BeginDisabledGroup(true);
            
            EditorGUILayout.SelectableLabel(
                text: labelText,
                style: EditorStyles.textField,
                options: GUILayout.Height(EditorGUIUtility.singleLineHeight));
            
            EditorGUI.EndDisabledGroup();
        }

        private string LabelWithTextField(string labelText, string defaultText = "",
            float labelWidth = AppStoreStyles.kClientLabelWidth)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(labelText, GUILayout.Width(labelWidth));
            string text = EditorGUILayout.TextField(defaultText);
            GUILayout.EndHorizontal();
            return text;
        }

        private static void ActivateInspectorWindow()
        {
#if UNITY_2018_2_OR_NEWER
            EditorApplication.ExecuteMenuItem("Window/General/Inspector");
#else
            EditorApplication.ExecuteMenuItem("Window/Inspector");
#endif
        }

        private static void GuiLine(int i_height = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, i_height);
            rect.height = i_height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }

        private int EditedIapCount()
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

        private static string HandleIapItemResponse(string oldData)
        {
            string newData = oldData.Replace("en-US", "thisShouldBeENHyphenUS");
            newData = newData.Replace("zh-CN", "thisShouldBeZHHyphenCN");
            return newData;
        }

        private bool AnythingChanged()
        {
#if (!UNITY_2020_1_OR_NEWER)
            if (m_GameTitleChanged || m_CallbackUrlChanged)
            {
                return true;
            }

            foreach (var dirty in m_TestAccountsDirty)
            {
                if (dirty)
                {
                    return true;
                }
            }
#endif

            foreach (var dirty in m_IapItemDirty)
            {
                if (dirty)
                {
                    return true;
                }
            }

            return false;
        }

        private static void Log(string msg)
        {
            if (debugEnabled)
            {
//                Debug.Log("[UDP] " + msg);
            }
        }

        #endregion
    }
}

#endif