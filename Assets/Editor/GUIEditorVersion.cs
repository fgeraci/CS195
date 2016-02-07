using UnityEditor;
using UnityEngine;

/// <summary>
/// Version of the GUI as an editor.
/// </summary>
public class GUIEditorVersion : EditorWindow
{
    private static string SKIN_PATH = "Assets/Authoring/Skin/OrangeGUISkin.guiskin";

    [MenuItem("Window/Crowd Authoring Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<GUIEditorVersion>();
    }

    private CrowdAuthoringEditor editor;

    private GUISkin skin;

    void OnEnable()
    {
        editor = new CrowdAuthoringEditor(new HandlesLineRenderer());
        //Reset does somehow NOT work in editor version, so I am removing it here.
        editor.GetMainActionBar().RemoveButton("Reset");
        skin = (GUISkin)AssetDatabase.LoadAssetAtPath(SKIN_PATH, typeof(GUISkin));
        PeriodicMethodCaller.GetInstance().StartCallPeriodically(Repaint, 0.05f);
    }

    void OnGUI()
    {
        GUI.skin = skin;
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), Color.white);
        editor.Render(position.width, position.height);
    }

    void OnDisable()
    {
        editor.OnDisable();
    }
}
