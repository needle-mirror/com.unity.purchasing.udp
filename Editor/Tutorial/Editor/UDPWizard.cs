#if IET_FRAMEWORK_ENABLED 

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Unity.Tutorials.Core;
using Unity.Tutorials.Core.Editor;

namespace UnityEngine.UDP.Editor
{
    public class UDPWizard : EditorWindow
    {
        private bool _IAPBool;
        private bool _UIAPBool;
        private bool _UIAPCodeBool;

        private bool _boolSelected;

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            string[] guids1 = AssetDatabase.FindAssets("UDPWizard", null);
            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(guids1[2]));
            VisualElement labelFromUXML = visualTree.CloneTree();

            VisualElement UIAP = labelFromUXML.Query<VisualElement>("UIAP");
            UIAP.visible = false;

            VisualElement UIAPCode = labelFromUXML.Query<VisualElement>("UIAPCode");
            UIAPCode.visible = false;

            Toggle IAPToggleYes = labelFromUXML.Query<Toggle>("IAPYes");
            Toggle IAPToggleNo = labelFromUXML.Query<Toggle>("IAPNo");
            //IAPToggleNo.value = true;

            IAPToggleYes.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (IAPToggleYes.value)
                {
                    _IAPBool = true;
                    UIAP.visible = true;
                    IAPToggleNo.value = false;
                }
                else
                {
                    _IAPBool = false;
                    UIAP.visible = false;
                    IAPToggleNo.value = true;
                }

                _boolSelected = true;
            });

            IAPToggleNo.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (IAPToggleNo.value)
                {
                    _IAPBool = false;
                    UIAP.visible = false;
                    IAPToggleYes.value = false;
                }
                else
                {
                    _IAPBool = true;
                    UIAP.visible = true;
                    IAPToggleYes.value = true;
                }

                _boolSelected = true;
            });

            Toggle UIAPToggleYes = labelFromUXML.Query<Toggle>("UIAPYes");
            Toggle UIAPToggleNo = labelFromUXML.Query<Toggle>("UIAPNo");

            UIAPToggleYes.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (UIAPToggleYes.value)
                {
                    _UIAPBool = true;
                    UIAPCode.visible = true;
                    UIAPToggleNo.value = false;
                }
                else
                {
                    _UIAPBool = false;
                    UIAPCode.visible = false;
                    UIAPToggleNo.value = true;
                }

                _boolSelected = true;
            });

            UIAPToggleNo.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (UIAPToggleNo.value)
                {
                    _UIAPBool = false;
                    UIAPCode.visible = false;
                    UIAPToggleYes.value = false;
                }
                else
                {
                    _UIAPBool = true;
                    UIAPCode.visible = true;
                    UIAPToggleYes.value = true;
                }

                _boolSelected = true;
            });


            Toggle UIAPCodelessToggleYes = labelFromUXML.Query<Toggle>("UIAPCodelessYes");
            Toggle UIAPCodelessToggleNo = labelFromUXML.Query<Toggle>("UIAPCodelessNo");

            UIAPCodelessToggleYes.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (UIAPCodelessToggleYes.value)
                {
                    _UIAPCodeBool = true;
                    UIAPCodelessToggleNo.value = false;
                }
                else
                {
                    _UIAPCodeBool = false;
                    UIAPCodelessToggleNo.value = true;
                }

                _boolSelected = true;
            });

            UIAPCodelessToggleNo.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (UIAPCodelessToggleNo.value)
                {
                    _UIAPCodeBool = false;
                    UIAPCodelessToggleYes.value = false;
                }
                else
                {
                    _UIAPCodeBool = true;
                    UIAPCodelessToggleYes.value = true;
                }

                _boolSelected = true;
            });


            //Evalute Button

            Button EvaluateBTN = labelFromUXML.Query<Button>("Evaluate");
            EvaluateBTN.clickable.clicked += () => Evaluate();

            root.Add(labelFromUXML);
        }

        public void Evaluate()
        {
            if (!_boolSelected)
            {
                
                WizardPopup window = ScriptableObject.CreateInstance<WizardPopup>();
                float width = 250;
                float height = 150;
                window.position = new Rect(0, 0, width, height);
                Extensions.CenterOnMainWin(window);
                //window.position = new Rect(this.position.width + 175 , this.position.height , 250, 150);
                window.ShowPopup();
                return;
            }

            string[] guids1 = AssetDatabase.FindAssets("ScenaioContainer", null);

            TutorialContainer TLSO = ScriptableObject.CreateInstance<TutorialContainer>();
            TLSO = (TutorialContainer)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids1[0]),
                typeof(TutorialContainer));

            if (!_IAPBool && !_UIAPBool && !_UIAPCodeBool)
            {
                TLSO.Sections[0].StartTutorial();
            }
            else if (_IAPBool && !_UIAPBool && !_UIAPCodeBool)
            {
                TLSO.Sections[1].StartTutorial();
            }
            else if (_IAPBool && _UIAPBool && _UIAPCodeBool)
            {
                TLSO.Sections[2].StartTutorial();
            }
            else if (_IAPBool && _UIAPBool && !_UIAPCodeBool)
            {
                TLSO.Sections[3].StartTutorial();
            }

            this.Close();
        }
    }
}
#endif

