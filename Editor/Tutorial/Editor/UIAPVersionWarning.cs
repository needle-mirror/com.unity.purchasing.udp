#if IET_FRAMEWORK_ENABLED 

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace UnityEngine.UDP.Editor
{
    public class UIAPVersionWarning : EditorWindow
    {

        public static void ShowWindow()
        {
            UIAPVersionWarning wnd = ScriptableObject.CreateInstance<UIAPVersionWarning>();

            wnd.titleContent = new GUIContent("Warning");

            float width = 350;
            float height = 200;
            wnd.position = new Rect(0, 0, width, height);
            Extensions.CenterOnMainWin(wnd);
            wnd.ShowPopup();
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            string[] guids1 = AssetDatabase.FindAssets("UIAPVersionWarning", null);
            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(guids1[1]));
            VisualElement labelFromUXML = visualTree.CloneTree();
            root.Add(labelFromUXML);

            Button CloseBTN = labelFromUXML.Query<Button>("Close");
            CloseBTN.clickable.clicked += () => CloseWin();
        }

        public void CloseWin()
        {
            this.Close();
        }
    }
}
#endif
