#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Core
{
    public static class CacheDataEditor
    {
        [MenuItem("Tools/Cache/Clear/All")]
        public static void ClearAllCache()
        {
            UnityWebRequest.ClearCookieCache();
            Caching.ClearCache();
            Directory.Delete(Application.temporaryCachePath, true);
            Directory.Delete(Application.persistentDataPath, true);
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        [MenuItem("Tools/Cache/Clear/PlayerPrefs")]
        public static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        [MenuItem("Tools/Cache/Clear/Cache")]
        public static void ClearCache()
        {
            Directory.Delete(Application.temporaryCachePath, true);
        }

        [MenuItem("Tools/Cache/Clear/Data")]
        public static void ClearData()
        {
            Directory.Delete(Application.persistentDataPath, true);
        }

        [MenuItem("Tools/Cache/Open/Cache Folder")]
        public static void OpenCacheFolder()
        {
            EditorUtility.RevealInFinder(Application.temporaryCachePath + "/cache");
        }

        [MenuItem("Tools/Cache/Open/Data Folder")]
        public static void OpenDataFolder()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }
    }
}
#endif