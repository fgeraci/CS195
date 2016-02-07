using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IdManager : MonoBehaviour 
{
    [SerializeField]
    private static int nextId = 0;

    private Dictionary<uint, string> usedIds;

    public int NextId { 
        get {
            IdManager.nextId += 1;
            return IdManager.nextId; 
        } 
    }

    private static IdManager instance = null;
    public static IdManager Instance
    {
        get {
            if (instance == null)
                instance = new IdManager();
            return instance; 
        }
    }

    void OnEnable()
    {
        if (instance != null)
            Debug.LogError("Multiple IdManager instances");
        instance = this;
    }

    public void RegisterId(string name, uint id)
    {
        if (this.usedIds == null)
            this.usedIds = new Dictionary<uint, string>();
        if (this.usedIds.ContainsKey(id) == true)
            IdManager.nextId = (int) id + 1;
        else
            this.usedIds.Add(id, name);

        if (id > IdManager.nextId)
            IdManager.nextId = (int) id + 1;
    }
}
