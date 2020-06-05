using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.Networking;

namespace UnityEngine.UDP.Editor
{
    public static class AppStoreOnBoardApi
    {
        public static TokenInfo tokenInfo = new TokenInfo();

        private const string udpUrl = BuildConfig.UDP_ENDPOINT;

        public const string notAuthenticatedError = "NotAuthenticated";
        public const string authorizationError = "AuthorizationError";
        public const string clientNotLinked = "udpClientNotLinked";

        // these fields to detect whether current user state
        // or org state changed, then we refresh udp token.
        public static string unityOrgId;
        public static string unityUserId;
        public static string unityAccessToken;
        public static string unityProjectId;

        public static UnityWebRequest AsyncRequest(string method, string url, string api, string token,
            object postObject)
        {
            var request = new UnityWebRequest(url + api, method);

            if (postObject != null)
            {
                string postData = HandlePostData(JsonUtility.ToJson(postObject));
                byte[] postDataBytes = Encoding.UTF8.GetBytes(postData);
                request.uploadHandler = (UploadHandler) new UploadHandlerRaw(postDataBytes);
            }

            request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            // set content-type header
            request.SetRequestHeader("Content-Type", "application/json");
            // set auth header
            if (token != null)
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            {
                request.SetRequestHeader("unity_version", Application.unityVersion);
                request.SetRequestHeader("sdk_version", BuildConfig.VERSION);
                request.SetRequestHeader("device_id", SystemInfo.deviceUniqueIdentifier);
                request.SetRequestHeader("sdk_dist", BuildConfig.SDK_DIST);
            }

            MethodInfo sendWebRequest = request.GetType().GetMethod("SendWebRequest");
            if (sendWebRequest == null)
            {
                sendWebRequest = request.GetType().GetMethod("Send");
            }

            sendWebRequest.Invoke(request, null);

            return request;
        }

        private static string HandlePostData(string oldData)
        {
            string newData = oldData.Replace("thisShouldBeENHyphenUS", "en-US");
            newData = newData.Replace("thisShouldBeZHHyphenCN", "zh-CN");
            Regex re = new Regex("\"\\w+?\":\"\",");
            newData = re.Replace(newData, "");
            re = new Regex(",\"\\w+?\":\"\"");
            newData = re.Replace(newData, "");
            re = new Regex("\"\\w+?\":\"\"");
            newData = re.Replace(newData, "");
            return newData;
        }

        public static UnityWebRequest GetUdpToken(string unityToken, string projectId)
        {
            var req = new TokenRequest {token = unityToken, projectId = projectId};
            return AsyncRequest(UnityWebRequest.kHttpVerbPOST, udpUrl, "/v2/store/token", tokenInfo.token, req);
        }

        public static UnityWebRequest FetchData()
        {
            return AsyncRequest(UnityWebRequest.kHttpVerbGET, udpUrl, "/v2/store/appData", tokenInfo.token, null);
        }

        public static UnityWebRequest GenerateNewClient()
        {
            return AsyncRequest(UnityWebRequest.kHttpVerbPOST, udpUrl, "/v2/store/insertAppData", tokenInfo.token, null);
        }

        public static UnityWebRequest LinkProjectId(string clientId)
        {
            var body = new LinkProjectPayload(){clientId = clientId};
            return AsyncRequest(UnityWebRequest.kHttpVerbPOST, udpUrl, "/v2/store/link", tokenInfo.token, body);
        }

        public static UnityWebRequest UpdateAll(FullUpdatePayload payload)
        {
            return AsyncRequest(UnityWebRequest.kHttpVerbPUT, udpUrl, "/v2/store/updateAppData", tokenInfo.token, payload);
        }

        public static UnityWebRequest UpdateStoreItem(IapItem iapItem)
        {
            var api = "/v2/store/items/" + iapItem.id;
            return AsyncRequest(UnityWebRequest.kHttpVerbPUT, udpUrl, api, tokenInfo.token, iapItem);
        }

        public static UnityWebRequest CreateStoreItem(IapItem iapItem)
        {
            var api = "/v2/store/items";
            iapItem.ownerId = tokenInfo.orgId;
            return AsyncRequest(UnityWebRequest.kHttpVerbPOST, udpUrl, api, tokenInfo.token, iapItem);
        }

        public static UnityWebRequest DeleteStoreItem(string iapItemId)
        {
            var api = "/v2/store/items/" + iapItemId;
            return AsyncRequest(UnityWebRequest.kHttpVerbDELETE, udpUrl, api, tokenInfo.token, null);
        }

        public static UnityWebRequest DeleteTestAccount(string playerId)
        {
            var api = "/v2/player/" + playerId;
            return AsyncRequest(UnityWebRequest.kHttpVerbDELETE, udpUrl, api, tokenInfo.token, null);
        }
    }
}