using UnityEngine;
using System.Collections;

public class AssignId : MonoBehaviour 
{
    [SerializeField]
    private int id = -1;

    public uint Id 
    { 
        get 
        {
            if (this.id < 0)
                this.id = IdManager.Instance.NextId;
            return (uint)this.id;
        } 
    }

    void Start()
    {
       // Debug.Log("AssignId: " + this.gameObject.name + " " + this.id);

        if (this.id < 0)
            this.id = IdManager.Instance.NextId;
        else
            IdManager.Instance.RegisterId(this.name, this.Id);
    }
}
