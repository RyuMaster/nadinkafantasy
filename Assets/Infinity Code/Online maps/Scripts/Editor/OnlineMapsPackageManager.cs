/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using UnityEditor;

public class OnlineMapsPackageManager
{
    [MenuItem("GameObject/Infinity Code/Online Maps/Playmaker Integration Kit", false, 1)]
    public static void ImportPlayMakerIntegrationKit()
    {
        OnlineMapsEditorUtils.ImportPackage("Packages\\OnlineMaps-Playmaker-Integration-Kit.unitypackage", 
            new OnlineMapsEditorUtils.Warning
            {
                title = "Playmaker Integration Kit",
                message = "You have Playmaker in your project?",
                ok = "Yes, I have a Playmaker"
            },
            "Could not find Playmaker Integration Kit."
        );
    }
}