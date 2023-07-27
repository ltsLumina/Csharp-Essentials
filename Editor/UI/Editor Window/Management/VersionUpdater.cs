#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using static Lumina.Essentials.Editor.UI.Management.VersionManager;
#endregion

namespace Lumina.Essentials.Editor.UI.Management
{
/// <summary>
/// Updates the version of Lumina's Essentials.
/// </summary>
    internal static class VersionUpdater
    {
        /// <summary> Whether or not the current version is the latest version. </summary>
        internal static string LastUpdateCheck => TimeManager.TimeSinceLastUpdate();
        
        /// <summary> The queue of coroutines to run. </summary>
        readonly static Queue<IEnumerator> coroutineQueue = new ();

        internal static void CheckForUpdates()
        {
            EditorApplication.update += Update;
            coroutineQueue.Enqueue(RequestUpdateCheck());
        }

        static void Update()
        {
            if (coroutineQueue.Count > 0)
            {
                IEnumerator coroutine = coroutineQueue.Peek();
                if (!coroutine.MoveNext()) coroutineQueue.Dequeue();
            }
            else { EditorApplication.update -= Update; }
        }

        static IEnumerator RequestUpdateCheck()
        {
            using UnityWebRequest www = UnityWebRequest.Get("https://api.github.com/repos/ltsLumina/Unity-Essentials/releases/latest");

            yield return www.SendWebRequest();

            while (!www.isDone)
            {
                yield return null; // Wait for the request to complete
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("UnityWebRequest failed with result: " + www.result);
                Debug.LogError("Error message: "                      + www.error);
            }
            else
            {
                string jsonResult = Encoding.UTF8.GetString(www.downloadHandler.data);
                string tag        = JsonUtility.FromJson<Release>(jsonResult).tag_name;

                // Update LatestVersion, UpToDate, LastUpdateCheck accordingly.
                EditorPrefs.SetString("LastUpdateCheck", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                UpdateStatistics(tag);
            }
        }

    }

    [Serializable]
    internal class Release
    {
        internal string tag_name;
    }
}
