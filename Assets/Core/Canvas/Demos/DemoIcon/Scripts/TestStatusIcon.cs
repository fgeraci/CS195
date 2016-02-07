using UnityEngine;
using System.Collections;

public class TestStatusIcon : MonoBehaviour 
{

    public StatusIcon target;

	void Update () 
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            target.SendMessage("SetIcon", "happy");
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            target.SendMessage("SetIcon", "sad");
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            target.SendMessage("SetIcon", "exclamation");
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            target.SendMessage("SetIcon", "key");
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            target.SendMessage("SetIcon", "speaking");
        else if (Input.GetKeyDown(KeyCode.Alpha6))
            target.SendMessage("ClearIcon");
	}
}
