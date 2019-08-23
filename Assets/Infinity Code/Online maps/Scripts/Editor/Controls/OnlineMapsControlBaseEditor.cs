/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OnlineMapsControlBase), true)]
public class OnlineMapsControlBaseEditor:Editor
{
    public static void CheckMultipleInstances(OnlineMapsControlBase control, ref bool dirty)
    {
        OnlineMapsControlBase[] controls = control.GetComponents<OnlineMapsControlBase>();
        if (controls.Length > 1)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.HelpBox("Problem detected:\nMultiple instances of controls.\nYou can use only one control.", MessageType.Error);

            int controlIndex = -1;

            for (int i = 0; i < controls.Length; i++)
            {
                if (GUILayout.Button("Use " + controls[i].GetType())) controlIndex = i;
            }

            if (controlIndex != -1)
            {
                OnlineMapsControlBase activeControl = controls[controlIndex];
                foreach (OnlineMapsControlBase c in controls) if (c != activeControl) OnlineMapsUtils.DestroyImmediate(c);
                dirty = true;
            }

            EditorGUILayout.EndVertical();
        }
    }

    public static void CheckTarget(OnlineMaps map, OnlineMapsTarget target, ref bool dirty)
    {
        if (map == null) return;
        if (map.target == target) return;

        EditorGUILayout.BeginVertical(GUI.skin.box);

        string targetName = Enum.GetName(typeof(OnlineMapsTarget), target);
        targetName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(targetName);
        EditorGUILayout.HelpBox("Problem detected:\nWrong target.\nFor this control target must be " + targetName + "!", MessageType.Error);
        if (GUILayout.Button("Fix Target"))
        {
            map.target = target;
            dirty = true;
        }

        EditorGUILayout.EndVertical();
    }

    public static OnlineMaps GetOnlineMaps(OnlineMapsControlBase control)
    {
        if (control == null) return null;
        OnlineMaps map = control.GetComponent<OnlineMaps>();

        if (map == null)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.HelpBox("Problem detected:\nCan not find OnlineMaps component.", MessageType.Error);

            if (GUILayout.Button("Add OnlineMaps Component"))
            {
                map = control.gameObject.AddComponent<OnlineMaps>();
                UnityEditorInternal.ComponentUtility.MoveComponentUp(map);
                if (control is OnlineMapsTileSetControl) map.target = OnlineMapsTarget.tileset;
            }

            EditorGUILayout.EndVertical();
        }
        return map;
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Label("Please do not use this Control.\nIt is the base class for other Controls.", OnlineMapsEditor.warningStyle);

        if (GUILayout.Button("Remove"))
        {
            OnlineMapsUtils.DestroyImmediate(target);
        }
    }
}