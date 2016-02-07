// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System.Collections;

public class StateCondition : ICondition
{
    public readonly int Index;
    public readonly StateName[] Tags;

    public StateCondition(int index, params StateName[] tags)
    {
        this.Index = index;
        this.Tags = tags;
    }

    private string PrintTag(StateName tag)
    {
        string output = "";
        if (tag < 0)
        {
            tag = ~tag;
            output += "~";
        }
        return output + tag.ToString();
    }

    public override string ToString()
    {
        string output =
            "[(" + this.Index + "): ";
        for (int i = 0; i < this.Tags.Length; i++)
        {
            output += this.PrintTag(this.Tags[i]);
            if (i < this.Tags.Length - 1)
                output += ", ";
        }
        return output + "]";
    }

}