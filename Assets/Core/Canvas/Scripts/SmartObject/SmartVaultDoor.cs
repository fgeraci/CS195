using TreeSharpPlus;
using RootMotion.FinalIK;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A vault door that can be opened.
/// </summary>
public class SmartVaultDoor : SmartDoor 
{
    public override string Archetype
    {
        get { return this.GetType().Name; }
    }

    [System.Serializable]
    public class LockBarPull
    {
        public Transform LockBar;

        public Vector3 PullDirection;
    }

    //Currently updating lock bars?
    private bool updateLockBars = false;

    //Currently updating wheel rotation?
    private bool updateWheelRotation = false;

    //Currently updating door rotation?
    private bool updateDoorRotation = false;

    //Maximum rotation of door
    private float doorMaxRotation = 90.0f;

    //Maximum offset of lockbars
    private float lockBarMaxOffset = 0.4f;

    //Time to rotate door for
    private float doorRotTime = 2.0f;

    //Time to open bars for
    private float lockBarOpenTime = 1.5f;


    void Update()
    {
        if (updateLockBars)
        {
            PullLockBars(Time.deltaTime);
        }
        if (updateWheelRotation)
        {
            DoorHandle.Rotate(Vector3.forward, Time.deltaTime * 100.0f);
        }
        if (updateDoorRotation)
        {
            PhysicalDoor.Rotate(Vector3.up, doorMaxRotation * Time.deltaTime / doorRotTime);
        }
    }

    /// <summary>
    /// The physical vault door that can be opened by rotating it.
    /// </summary>
    public Transform PhysicalDoor;

    /// <summary>
    /// The rotating door handle of the vault door
    /// </summary>
    public Transform DoorHandle;

    /// <summary>
    /// The lock bars that need to be pulled inside.
    /// </summary>
    public LockBarPull[] LockBars;

    /// <summary>
    /// Lets the user open the door.
    /// </summary>
    [Affordance]
    protected Node Open(SmartCharacter user)
    {
        float elapsedTime = 0.0f;

        return new Sequence(
            this.UnlockFront(user),
            new LeafWait(1000),
            new LeafInvoke(() => updateLockBars = true),
            new LeafWait((long) lockBarOpenTime * 1000),
            new LeafInvoke(() => updateLockBars = false),
            new LeafInvoke(() => updateWheelRotation = true),
            new LeafWait(1500),
            new LeafInvoke(() => updateWheelRotation = false),
            new LeafInvoke(() => updateDoorRotation = true),
            new LeafWait((long) doorRotTime * 1000),
            new LeafInvoke(() => updateDoorRotation = false),
            this.Node_Set(StateName.IsOpen, StateName.IsUnlocked));
    }

    private void PullLockBars(float time)
    {
        for (int i = 0; i < LockBars.Length; i++)
        {
            LockBars[i].LockBar.position += LockBars[i].PullDirection.normalized * lockBarMaxOffset * time / lockBarOpenTime;
        }
    }
}
