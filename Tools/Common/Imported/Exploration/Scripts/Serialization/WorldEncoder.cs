using UnityEngine;
using System.Linq;
using System.Collections;

public static class WorldEncoder
{
    public static WorldData EncodeWorldData(ObjectManager manager, EventLibrary library)
    {
        string ids = "";
        foreach (IHasState obj in manager.GetObjects())
            ids += ((Component)obj).gameObject.name + ": " + obj.Id + "\n";
        Debug.Log(ids);

        StateData initialState = 
            new StateData(
                new WorldState(
                    manager.GetObjects().Cast<IHasState>()));
        EventData[] events = 
            library.GetSignatures().Convert(s => new EventData(s)).ToArray();

        return new WorldData(initialState, events);
    }
}
