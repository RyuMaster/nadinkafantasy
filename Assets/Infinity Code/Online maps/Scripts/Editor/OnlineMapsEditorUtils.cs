/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using System.IO;
using UnityEditor;
using UnityEngine;

public static class OnlineMapsEditorUtils
{
    private static string _assetPath;

    public static string assetPath
    {
        get
        {
            if (_assetPath == null)
            {
                string[] dirs = Directory.GetDirectories("Assets", "Online maps", SearchOption.AllDirectories);
                _assetPath = dirs.Length > 0 ? dirs[0] : string.Empty;
            }
            return _assetPath;
        }
    }

    public static void ImportPackage(string path, Warning warning = null, string errorMessage = null)
    {
        if (warning != null && !warning.Show()) return;
        if (string.IsNullOrEmpty(assetPath))
        {
            if (!string.IsNullOrEmpty(errorMessage)) Debug.LogError(errorMessage);
            return;
        }

        string filaname = assetPath + "\\" + path;
        if (!File.Exists(filaname))
        {
            if (!string.IsNullOrEmpty(errorMessage)) Debug.LogError(errorMessage);
            return;
        }

        AssetDatabase.ImportPackage(filaname, true);
    }

    public static T LoadAsset<T>(string path, bool throwOnMissed = false) where T : Object
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            if (throwOnMissed) throw new FileNotFoundException(assetPath);
            return default(T);
        }
        string filaname = assetPath + "\\" + path;
        if (!File.Exists(filaname))
        {
            if (throwOnMissed) throw new FileNotFoundException(assetPath);
            return default(T);
        }
        return (T)AssetDatabase.LoadAssetAtPath(filaname, typeof(T));
    }

    public class Warning
    {
        public string title = "Warning";
        public string message;
        public string ok = "OK";
        public string cancel = "Cancel";

        public bool Show()
        {
            return EditorUtility.DisplayDialog(title, message, ok, cancel);
        }
    }
}