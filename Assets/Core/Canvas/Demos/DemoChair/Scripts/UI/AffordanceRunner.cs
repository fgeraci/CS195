using UnityEngine;
using TreeSharpPlus;
using System.Collections;

public class AffordanceRunner : MonoBehaviour 
{
    private bool busy = false;
    private SmartObject obj = null;
    private SmartObject user = null;
    private string curAffordance = null;

    void Start()
    {
        this.user = this.GetComponent<SmartCharacter>();
    }

	void Update() 
    {
        if (this.curAffordance != null && this.obj != null)
        {
            RunStatus status = this.obj.RunAffordance(curAffordance, this.user);
            if (status != RunStatus.Running)
            {
                if (status == RunStatus.Failure)
                {
                    MousePick.instance.PlayFailed();
                    Debug.LogWarning("Failed " + this.obj + " : " + this.curAffordance);
                }
                this.curAffordance = null;
                this.obj = null;
                this.busy = false;
            }
        }
	}

    public void RunAffordance(SmartObject obj, string name)
    {
        if (this.busy == false)
        {
            this.obj = obj;
            this.curAffordance = name;
            this.busy = true;
        }
        else
        {
            MousePick.instance.PlayFailed();
            Debug.LogWarning("Too busy for " + obj + " : " + name);
        }
    }
}
