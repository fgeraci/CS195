using UnityEngine;
using System.Collections;

/// <summary>
/// Version of the GUI rendered in engine.
/// </summary>
public class GUIInEngineVersion : MonoBehaviour 
{
    private ActionBar mainBar;

    private CrowdAuthoringEditor editor;

    //Set the skin to be used.
    public GUISkin skin;

    //Whether we should render the GUI.
    public bool showGUI;

    private Texture2D background;

	// Use this for initialization
	void Start () 
    {
        editor = new CrowdAuthoringEditor(new InEngineLineRenderer());
        background = new Texture2D(1, 1);
        background.SetPixel(0, 0, new Color(1.0f, 1.0f, 1.0f, 0.5f));
        background.Apply();
	}

    void Update()
    {
        if (Input.GetButtonUp("GUI Show"))
        {
            showGUI = !showGUI;
        }
    }
	
	// Update is called once per frame
	void OnGUI () 
    {
        if (skin != null)
        {
            GUI.skin = skin;
        }
        if (!showGUI) //&& GUILayout.Button("Show", GUILayout.MinWidth(100)))
        {
            //showGUI = true;
        }
        else if (showGUI)
        {
            Texture2D oldBackground = GUI.skin.label.normal.background;
            GUI.skin.label.normal.background = background;
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "");
            GUI.skin.label.normal.background = oldBackground;
            editor.Render(Screen.width, Screen.height);
        }
	}

    void OnDisable()
    {
        editor.OnDisable();
    }
}
