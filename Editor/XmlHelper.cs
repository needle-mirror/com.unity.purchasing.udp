using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Xml;

namespace UnityEngine.UDP.Editor
{
    public static class XmlHelper
    {
        private static readonly string _android_folder_path = "Assets/Plugins/Android/";
        private static readonly string _android_manifest_path = _android_folder_path + "AndroidManifest.xml";
        private static string _udp_client_id_key = "UDP_INTERNAL_CLIENT_ID";
        private static string _android_namespace = "http://schemas.android.com/apk/res/android";

        private static string _xml_template =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
                "<manifest\n" +
                "xmlns:android=\"http://schemas.android.com/apk/res/android\"\n" +
                "package=\"com.unity3d.player\"\n" +
                "xmlns:tools=\"http://schemas.android.com/tools\">\n" +
                "<application>\n" +
                "<activity android:name=\"com.unity3d.player.UnityPlayerActivity\"\n" +
                "android:theme=\"@style/UnityThemeSelector\">\n" +
                "<intent-filter>\n" +
                "<action android:name=\"android.intent.action.MAIN\" />\n" +
                "<category android:name=\"android.intent.category.LAUNCHER\" />\n" +
                "</intent-filter>\n" +
                "<meta-data android:name=\"unityplayer.UnityActivity\" android:value=\"true\" />\n" +
                "</activity>\n" +
                "<meta-data android:name=\"${UDP_CLIENT_ID_KEY}\" android:value=\"${UDP_CLIENT_ID}\"/>\n" +
                "</application>\n" +
                "</manifest>";

        private static bool IsAndroidManifestExist()
        {
            return File.Exists(_android_manifest_path);
        }

        private static void UpdateExistingManifestWithClientId(string clientId)
        {
            XmlDocument document = new XmlDocument();
            document.Load(_android_manifest_path);
            XmlNode manifestNode = document.DocumentElement.SelectSingleNode("/manifest");
            if (manifestNode == null)
            {
                manifestNode = document.CreateElement("manifest");
                ((XmlElement)manifestNode).SetAttribute("xmlns:android",
                    "\"http://schemas.android.com/apk/res/android\"");
                document.AppendChild(manifestNode);
            }

            XmlNode appNode = document.DocumentElement.SelectSingleNode("/manifest/application");
            if (appNode != null)
            {
                // remove old if exists
                foreach (XmlNode node in appNode.ChildNodes)
                {
                    if (node.Attributes != null)
                        foreach (XmlAttribute attr in node.Attributes)
                        {
                            if (attr.Value == _udp_client_id_key)
                            {
                                appNode.RemoveChild(node);
                            }
                        }
                }
            }
            else
            {
                appNode = document.CreateElement("application");
                manifestNode.AppendChild(appNode);
            }

            // insert under <application/> 
            XmlElement clientIdNode = document.CreateElement("meta-data");
            clientIdNode.SetAttribute("name", _android_namespace, _udp_client_id_key);
            clientIdNode.SetAttribute("value", _android_namespace, clientId);
            appNode.AppendChild(clientIdNode);

            document.Save(_android_manifest_path);
        }

        public static void PersistClientId(string clientId)
        {
            try
            {
                if (String.IsNullOrEmpty(clientId))
                {
                    return;
                }

                if (!Directory.Exists(_android_folder_path))
                {
                    Directory.CreateDirectory(_android_folder_path);
                }

                if (!IsAndroidManifestExist())
                {
                    // create new AndroidManifest.xml and persist clientId
                    var content = _xml_template.Replace("${UDP_CLIENT_ID_KEY}", _udp_client_id_key)
                        .Replace("${UDP_CLIENT_ID}", clientId);
                    StreamWriter writer = new StreamWriter(_android_manifest_path);
                    writer.Write(content);
                    writer.Close();
                }
                else
                {
                    // update on existing AndroidManifest.xml with clientId
                    UpdateExistingManifestWithClientId(clientId);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                Debug.LogError(
                    "Failed to persist clientId to AndroidManifest, try to delete it and pull data from server again.");
            }
        }
    }
}