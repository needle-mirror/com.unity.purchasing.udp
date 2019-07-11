using UnityEditor;
using UnityEngine.Networking;

namespace UnityEngine.UDP.Editor.Analytics
{
    [InitializeOnLoad]
    public static class ProjectOpenEvent
    {
        private const string k_ProjectOpened = "UdpProjectOpened";

        static ProjectOpenEvent()
        {
            if (!SessionState.GetBool(k_ProjectOpened, false))
            {
                if (EditorUserBuildSettings.activeBuildTarget.Equals(BuildTarget.Android) && Common.TargetUDP())
                {
                    SessionState.SetBool(k_ProjectOpened, true);

                    UnityWebRequest request = EditorAnalyticsApi.ProjectOpened();

                    EditorAnalyticsReqStruct reqStruct = new EditorAnalyticsReqStruct
                    {
                        eventName = EditorAnalyticsApi.k_ProjectOpenEventName,
                        webRequest = request,
                    };

                    WebRequestQueue.Enqueue(reqStruct);
                }
            }
        }
    }
}