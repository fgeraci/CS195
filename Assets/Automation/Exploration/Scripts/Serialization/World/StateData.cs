// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System.Collections;

[System.Serializable]
public class StateData
{
    public uint[] ObjectIds;
    public long[] StateMatrix;

    public StateData()
    {
        this.ObjectIds = null;
        this.StateMatrix = null;
    }

    public StateData(
        WorldState state)
    {
        this.ObjectIds = state.GetIds();
        this.StateMatrix = state.GetStateMatrix();
    }

    public WorldState Decode()
    {
        return new WorldState(
            this.ObjectIds,
            this.StateMatrix);
    }
}
