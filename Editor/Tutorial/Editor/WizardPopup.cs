#if IET_FRAMEWORK_ENABLED

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEngine.UDP.Editor
{
    public class WizardPopup : EditorWindow
    {


        void OnGUI()
        {

            GUILayout.Space(40);


            GUI.contentColor = Color.yellow;
            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };


            EditorGUILayout.LabelField("Please select an answer", style, GUILayout.ExpandWidth(true));

            GUILayout.Space(50);
            GUI.contentColor = Color.white;
            if (GUILayout.Button("Close")) this.Close();
        }
    }
}
#endif