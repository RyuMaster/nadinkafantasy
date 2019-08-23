/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OnlineMapsLocationService))]
public class OnlineMapsLocationServiceEditor : OnlineMapsLocationServiceEditorBase
{
    private SerializedProperty pDesiredAccuracy;
    private SerializedProperty pUpdateDistance;

    protected override void CacheSerializedProperties()
    {
        base.CacheSerializedProperties();
        pDesiredAccuracy = serializedObject.FindProperty("desiredAccuracy");
        pUpdateDistance = serializedObject.FindProperty("updateDistance");
    }

    public override void CustomInspectorGUI()
    {
        float labelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 160;

        EditorGUILayout.PropertyField(pDesiredAccuracy, new GUIContent("Desired Accuracy (meters)"));
        EditorGUIUtility.labelWidth = labelWidth;
    }

    public override void CustomUpdatePositionGUI()
    {
        EditorGUILayout.PropertyField(pUpdateDistance);
    }
}