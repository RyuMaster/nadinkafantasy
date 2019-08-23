/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

#if !UNITY_4_6 && !UNITY_4_7 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2
#define UNITY_5_3P
#endif

using System;
using UnityEditor;
using UnityEngine;

#if UNITY_5_3P
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

[CustomEditor(typeof(OnlineMapsUIImageControl), true)]
public class OnlineMapsUIImageControlEditor:Editor
{
#if !CURVEDUI
    private static bool hasCurvedUI;
#endif

    public static void CheckCurvedUI(OnlineMapsControlBase control)
    {
#if !CURVEDUI
        hasCurvedUI = false;
        Type[] types = control.GetType().Assembly.GetTypes();
        for (int i = 0; i < types.Length; i++)
        {
            if (types[i].Namespace == "CurvedUI")
            {
                hasCurvedUI = true;
                break;
            }
        }
#endif
    }

    public static void DrawCurvedUIWarning()
    {
#if !CURVEDUI
        if (hasCurvedUI)
        {
            EditorGUILayout.HelpBox("To make the map work properly with Curved UI, enable integration.", MessageType.Info);
            if (GUILayout.Button("Enable Curved UI"))
            {
                OnlineMapsEditor.AddCompilerDirective("CURVEDUI");
            }
        }
#endif
    }

    private void OnEnable()
    {
#if !CURVEDUI
        CheckCurvedUI(target as OnlineMapsControlBase);
#endif
    }

    public override void OnInspectorGUI()
    {
        bool dirty = false;

        OnlineMapsControlBase control = target as OnlineMapsControlBase;
        OnlineMapsControlBaseEditor.CheckMultipleInstances(control, ref dirty);

        OnlineMaps map = OnlineMapsControlBaseEditor.GetOnlineMaps(control);
        OnlineMapsControlBaseEditor.CheckTarget(map, OnlineMapsTarget.texture, ref dirty);

        DrawCurvedUIWarning();

        base.OnInspectorGUI();

        if (dirty)
        {
            EditorUtility.SetDirty(map);
            EditorUtility.SetDirty(control);
            if (!Application.isPlaying)
            {
#if UNITY_5_3P
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
#endif
            }
            else map.Redraw();
        }
    }
}