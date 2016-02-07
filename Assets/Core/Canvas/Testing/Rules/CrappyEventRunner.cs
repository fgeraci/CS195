using UnityEngine;

using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class CrappyEventRunner : MonoBehaviour 
{
    public EventName eventName;
    public SmartObject param1;
    public SmartObject param2;
    public SmartObject param3;
    public SmartObject param4;
    public SmartObject param5;
    public SmartObject param6;

    public enum EventName
    {
        TestUnlockDoorFront,
        TestUnlockDoorRear,
        TestLockDoorFront,
        TestLockDoorRear,
        TestIsInZone,
        TestGoWaypoint1,
        TestGoWaypoint2,
        TestIncapacitate,
    }

    private SmartObject[] GetParams()
    {
        return new[] { param1, param2, param3, param4, param5, param6 }.Where(so => so != null).ToArray();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) == true)
        {
            EventSignature sig = EventLibrary.Instance.GetSignature(this.eventName.ToString());
            bool reqs =
                sig.CheckRequirements(
                    this.GetParams(),
                    ObjectManager.Instance.GetObjects().Cast<IHasState>());
            if (reqs == true)
                sig.Create(this.GetParams()).StartEvent(1.0f);
            else
                Debug.Log(false);
        }
    }
}
