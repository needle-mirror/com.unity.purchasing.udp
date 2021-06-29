#if (SERVICES_SDK_CORE_ENABLED && ENABLE_EDITOR_GAME_SERVICES)
using System.IO;
using UnityEditor;

namespace UnityEngine.UDP.Editor
{
    static class UDPSettingsTopMenu
    {
        const int k_ConfigureMenuPriority = 100;
        const string k_ServiceMenuRoot = "Services/Unity Distribution Portal/";

        [MenuItem(k_ServiceMenuRoot + "Configure", priority = k_ConfigureMenuPriority)]
        public static void ShowProjectSettings()
        {
            var path = UDPSettingsProvider.GetSettingsPath();
            SettingsService.OpenProjectSettings(path);
        }

        [MenuItem(k_ServiceMenuRoot + "IAP Catalog", priority = k_ConfigureMenuPriority + 11)]
        public static void ActivateSettingsWindow()
        {
            if (Utils.UnityIapExists())
            {
                // Unity IAP + UDP, then open the iap catalog window of Uni directly.
                EditorApplication.ExecuteMenuItem("Window/Unity IAP/IAP Catalog");
                return;
            }

            Utils.ActivateInspectorWindow();

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

            var appStoreSettings = ScriptableObject.CreateInstance<AppStoreSettings>();
            AssetDatabase.CreateAsset(appStoreSettings, AppStoreSettings.appStoreSettingsAssetPath);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = appStoreSettings;
        }
    }
}
#endif
