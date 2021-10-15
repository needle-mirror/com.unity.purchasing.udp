using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;

namespace UnityEngine.UDP.Editor
{
    public class IETTopMenu : EditorWindow
    {
        const int k_ConfigureMenuPriority = 100;
        const string k_ServiceMenuRoot = "Services/Unity Distribution Portal/";
        private const string k_ietFrameworkPackageName = "com.unity.learn.iet-framework";
        private const string k_ietFrameworkAuthoringPackageName = "com.unity.learn.iet-framework.authoring";
        static AddRequest Request;

#if (SERVICES_SDK_CORE_ENABLED && ENABLE_EDITOR_GAME_SERVICES)
        [MenuItem(k_ServiceMenuRoot + "Implementation Guide", priority = k_ConfigureMenuPriority + 22)]
#else
        [MenuItem("Window/Unity Distribution Portal/Implementation Guide", false, 122)]
#endif
        public static void ShowWindow()
        {
#if IET_FRAMEWORK_ENABLED
            float width = 550;
            float height = 600;
            UDPWizard wnd = GetWindow<UDPWizard>();
            wnd.titleContent = new GUIContent("Implementation Guide");
            wnd.position = new Rect(0, 0, width, height);
            Extensions.CenterOnMainWin(wnd);
#else 
            // show dialog to import iet framework package
            if (EditorUtility.DisplayDialog("IET framework required", "The In-Editor Tutorial (IET) Framework will be imported from the Package Manager.", "Ok",
                "Cancel"))
            {
                Request = Client.Add(k_ietFrameworkPackageName+"@2.0.0");
                EditorApplication.update += Progress;
            }

#endif
        }
            

        static void Progress()
        {
            if (Request.IsCompleted)
            {
                if (Request.Status == StatusCode.Success)
                    Debug.Log("Installed: " + Request.Result.packageId);
                else if (Request.Status >= StatusCode.Failure)
                    Debug.Log(Request.Error.message);

                EditorApplication.update -= Progress;
            }

        }

    }
}
