#if IET_FRAMEWORK_ENABLED
using UnityEditor;

namespace UnityEngine.UDP.Editor
{
    public static class IETHelper
    {
        private static bool cachedSdkInitialized = false;
        private static bool cachedIapPurchased = false;

        public static bool ClientIDCreated
        {
            get
            {
                var settings =
                    AssetDatabase.LoadAssetAtPath<AppStoreSettings>(AppStoreSettings.appStoreSettingsAssetPath);
                if (settings == null) return false;

                return !string.IsNullOrEmpty(settings.UnityClientID);
            }
        }

        public static bool IAPCatCreated
        {
            get
            {
                var asset = Resources.Load(AppStoreSettings.productCatalogFileName) as TextAsset;
                if (asset == null) return false;
                var catalog = ProductCatalogPersistModel.FromTextAsset(asset);
                return catalog.products.Count > 0;
            }
        }

        public static bool SdkInitialized
        {
            get
            {
                var clientId = FetchClientIdFromFile();
                if (string.IsNullOrEmpty(clientId)) return false;

                if (cachedSdkInitialized) return true;
                FetchSdkTestProgressFromServer(clientId);

                return cachedSdkInitialized;
            }
        }

        public static bool IapPurchased
        {
            get
            {
                var clientId = FetchClientIdFromFile();
                if (string.IsNullOrEmpty(clientId)) return false;

                if (cachedIapPurchased) return true;
                FetchSdkTestProgressFromServer(clientId);

                return cachedIapPurchased;
            }
        }

        private static void FetchSdkTestProgressFromServer(string clientId)
        {
            var success = true;
            const float waitTime = 10000f;
            float timer = 0;

            var asyncReq = AppStoreOnBoardApi.GetAppTestProgress(clientId);
            while (!asyncReq.isDone)
            {
                if (timer > waitTime)
                {
                    success = false;
                    break;
                }

                System.Threading.Thread.Sleep(50);
                timer += Time.deltaTime;
            }

            if (success && asyncReq.error == null)
            {
                var res = JsonUtility.FromJson<SdkTestProgressResponse>(asyncReq.downloadHandler.text);
                cachedIapPurchased = res.iapPurchased;
                cachedSdkInitialized = res.sdkInitialized;
            }
            else
            {
                cachedIapPurchased = false;
                cachedSdkInitialized = false;
            }

            asyncReq.Dispose();
        }

        private static string FetchClientIdFromFile()
        {
            var settings =
                AssetDatabase.LoadAssetAtPath<AppStoreSettings>(AppStoreSettings.appStoreSettingsAssetPath);
            if (settings == null) return "";
            return string.IsNullOrEmpty(settings.UnityClientID) ? "" : settings.UnityClientID;
        }
    }
}
#endif