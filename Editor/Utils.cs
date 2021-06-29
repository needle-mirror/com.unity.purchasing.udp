using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;
using UnityEditor;

namespace UnityEngine.UDP.Editor
{
    public static class Utils
    {
        static Dictionary<string, Type> m_TypeCache = new Dictionary<string, Type>();
        private static string[] k_WhiteListedAssemblies = {"UnityEditor", "UnityEngine.Purchasing"};
        private static Type UnityConnectType;
        private static MethodInfo getOrgMethod;
        private static MethodInfo getUserIdMethod;
        private static MethodInfo getAccessTokenMethod;

        public static Type FindTypeByName(string name)
        {
            if (m_TypeCache.ContainsKey(name))
            {
                return m_TypeCache[name];
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                if (!AllowLookupForAssembly(assembly.FullName))
                    continue;

                try
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.FullName == name)
                        {
                            m_TypeCache[type.FullName] = type;
                            return type;
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(string.Format(
                        "Count not fetch list of types from assembly {0} due to error: {1}", assembly.FullName,
                        e.Message));
                }
            }

            return null;
        }

        private static bool AllowLookupForAssembly(string name)
        {
            return Array.Exists(k_WhiteListedAssemblies, name.StartsWith);
        }

        public static bool CheckURL(String URL)
        {
            string pattern =
                @"^(https?:\/\/[\w-]+(\.[\w-]+)+(:\d+)?((\/|\/[\w-]+)?|\/[\w\.]+|\/[\w]+(\.[\w]+)+)*(\?[\w-]+=[\w-]+((&[\w-]+=[\w-]+)?)*)?)?$";
            return new Regex(pattern, RegexOptions.IgnoreCase).IsMatch(URL);
        }

        public static PriceDetail ExtractUSDPrice(IapItem iapItem)
        {
            List<PriceDetail> prices = iapItem.priceSets.PurchaseFee.priceMap.DEFAULT;
            foreach (var price in prices)
            {
                if (price.currency == "USD")
                {
                    return price;
                }
            }

            PriceDetail newUSDPrice = new PriceDetail();
            newUSDPrice.currency = "USD";
            prices.Add(newUSDPrice);
            return newUSDPrice;
        }

        public static PriceSets FillUsdToPriceSet(string price)
        {
            var priceDetail = new PriceDetail() {currency = "USD", price = price};
            var priceList = new List<PriceDetail>() {priceDetail};
            var priceMap = new PriceMap() {DEFAULT = priceList};
            var priceSet = new PriceSets() {PurchaseFee = new PurchaseFee() {priceMap = priceMap}};
            return priceSet;
        }

        public static string GetOrganizationId(object instance = null)
        {
            if (UnityConnectType == null)
            {
                UnityConnectType = FindTypeByName("UnityEditor.Connect.UnityConnect");
            }

            if (getOrgMethod == null)
            {
                getOrgMethod = UnityConnectType.GetMethod("GetOrganizationId");
            }

            if (instance == null)
            {
                instance = UnityConnectType.GetProperty("instance").GetValue(null, null);
            }

            var orgId = getOrgMethod.Invoke(instance, null) as string;

            return orgId;
        }

        public static string GetUserId(object instance = null)
        {
            if (UnityConnectType == null)
            {
                UnityConnectType = FindTypeByName("UnityEditor.Connect.UnityConnect");
            }

            if (getUserIdMethod == null)
            {
                getUserIdMethod = UnityConnectType.GetMethod("GetUserId");
            }

            if (instance == null)
            {
                instance = UnityConnectType.GetProperty("instance").GetValue(null, null);
            }

            var userId = getUserIdMethod.Invoke(instance, null) as string;

            return userId;
        }

        public static string GetAccessToken(object instance = null)
        {
            if (UnityConnectType == null)
            {
                UnityConnectType = FindTypeByName("UnityEditor.Connect.UnityConnect");
            }

            if (getAccessTokenMethod == null)
            {
                getAccessTokenMethod = UnityConnectType.GetMethod("GetAccessToken");
            }

            if (instance == null)
            {
                instance = UnityConnectType.GetProperty("instance").GetValue(null, null);
            }

            var accessToken = getAccessTokenMethod.Invoke(instance, null) as string;

            return accessToken;
        }

        public static bool UserStateChanged(string userId, string orgId, string accessToken)
        {
            if (UnityConnectType == null)
            {
                UnityConnectType = FindTypeByName("UnityEditor.Connect.UnityConnect");
            }

            if (getAccessTokenMethod == null)
            {
                getAccessTokenMethod = UnityConnectType.GetMethod("GetAccessToken");
            }

            var instance = UnityConnectType.GetProperty("instance").GetValue(null, null);
            return userId != GetUserId(instance) || orgId != GetOrganizationId(instance) ||
                   accessToken != GetAccessToken(instance);
        }

        public static bool UnityIapExists()
        {
            return IsIapPackmanInstalled() || IsIapAssetstoreInstalled();
        }

        public static List<ProductInfo> IapListToProductInfoList(List<IapItem> iapItems)
        {
            var list = new List<ProductInfo>();
            foreach (var item in iapItems)
            {
                var productInfo = new ProductInfo
                {
                    ItemType = item.type,
                    ProductId = item.slug,
                    Consumable = item.consumable,
                    Price = ExtractUSDPrice(item).price,
                    Currency = ExtractUSDPrice(item).currency,
                    Title = item.name
                };
                productInfo.PriceAmountMicros = (long) (Convert.ToDouble(productInfo.Price) * 1000000);
                productInfo.Description = item.properties?.description;
                list.Add(productInfo);
            }

            return list;
        }

        public static void ActivateInspectorWindow()
        {
#if UNITY_2018_2_OR_NEWER
            EditorApplication.ExecuteMenuItem("Window/General/Inspector");
#else
            EditorApplication.ExecuteMenuItem("Window/Inspector");
#endif
        }

        public static void FocusOnUDPSettingsFile()
        {
            var existedAppStoreSettings = (AppStoreSettings) AssetDatabase.LoadAssetAtPath(
                AppStoreSettings.appStoreSettingsAssetPath,
                typeof(AppStoreSettings));
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = existedAppStoreSettings;
        }

        private static bool IsIapAssetstoreInstalled()
        {
            return File.Exists("Assets/Plugins/UnityPurchasing/Bin/Purchasing.Common.dll");
        }

        private static bool IsIapPackmanInstalled()
        {
            return Directory.Exists("Packages/com.unity.purchasing/Runtime");
        }
    }
}