#if (!(SERVICES_SDK_CORE_ENABLED && ENABLE_EDITOR_GAME_SERVICES))

using System.IO;
using UnityEditor;

namespace UnityEngine.UDP.Editor
{
    public class AppStoreTopMenu
    {
        [MenuItem("Window/Unity Distribution Portal/Settings", false, 111)]
        public static void ActivateSettingsWindow()
        {
            Utils.ActivateInspectorWindow();

            if (File.Exists(AppStoreSettings.appStoreSettingsAssetPath))
            {
                Utils.FocusOnUDPSettingsFile();
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
