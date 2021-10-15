#if IET_FRAMEWORK_ENABLED 

using UnityEngine;
using Unity.Tutorials.Core;
using Unity.Tutorials.Core.Editor;
using UnityEngine.UDP.Editor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor;
using System.Collections.Generic;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using System.Linq;


/// <summary>
/// Implement your Tutorial callbacks here.
/// </summary>
//[CreateAssetMenu(fileName = DefaultFileName, menuName = "Tutorials/UDP" + DefaultFileName + " Instance")]
public class TutorialCallbacks : ScriptableObject
{
    public const string DefaultFileName = "TutorialCallbacks";
    static SearchRequest Request;
    public bool UIAPDialog;

    public static ScriptableObject CreateAndShowAsset(string assetPath = null)
    {
        assetPath = assetPath ?? $"{TutorialEditorUtils.GetActiveFolderPath()}/{DefaultFileName}.asset";
        var asset = CreateInstance<TutorialCallbacks>();
        AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(assetPath));
        EditorUtility.FocusProjectWindow(); // needed in order to make the selection of newly created asset to really work
        Selection.activeObject = asset;
        return asset;
    }

    public bool BillingModeCheck()
    {
        TextAsset txtAsset = (TextAsset)Resources.Load("BillingMode");
        if (txtAsset != null)
        {
            string _temp = txtAsset.text;
            return _temp.Contains("UDP");
        }
        else
        {
            return false;
        }
    }

    public void ResetDialogFlag()
    {
        UIAPDialog = false;
    }
   
    public bool UDPProjectCreated()
    {
        return IETHelper.ClientIDCreated;
    }

    public bool UDPIAPCreated()
    {
        return IETHelper.IAPCatCreated;
    }

    public bool UDPSandbox1()
    {
        return IETHelper.SdkInitialized;
    }

    public bool UDPSandbox2()
    {
        return IETHelper.IapPurchased;
    }

    public bool PackmanCheck()
    {
        List<PackageInfo> packageJsons = AssetDatabase.FindAssets("package")
                .Select(AssetDatabase.GUIDToAssetPath).Where(x => AssetDatabase.LoadAssetAtPath<TextAsset>(x) != null)
                .Select(PackageInfo.FindForAssetPath).ToList();
        PackageInfo _temp = packageJsons.Find(x => x.name == "com.unity.purchasing");
        string[] _SplitVersion;
        float[] __SplitVersionNumbers;
        if (_temp == null)
        {
            return false;
        }
        else
        {
            _SplitVersion = _temp.version.Split('.');
            __SplitVersionNumbers = new float[_SplitVersion.Length];
            for (int i = 0; i < _SplitVersion.Length; i++)
            {
                __SplitVersionNumbers[i] = float.Parse(_SplitVersion[i]);
            }
        }
        if(__SplitVersionNumbers[0] < 3 && !UIAPDialog)
        {
            UIAPVersionWarning.ShowWindow();
            UIAPDialog = true;
    
        }
        return true;
        
    }
}
#endif