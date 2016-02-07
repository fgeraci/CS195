// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System.Collections;

public class RelationCondition : ICondition
{
    public readonly int IndexFrom;
    public readonly int IndexTo;
    public readonly RelationName[] Tags;

    public RelationCondition(
        int indexFrom,
        int indexTo,
        params RelationName[] tags)
    {
        this.IndexFrom = indexFrom;
        this.IndexTo = indexTo;
        this.Tags = tags;
    }

    private string PrintTag(RelationName tag)
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
            "[(" + this.IndexFrom
            + " -> " + this.IndexTo + "): ";
        for (int i = 0; i < this.Tags.Length; i++)
        {
            output += this.PrintTag(this.Tags[i]);
            if (i < this.Tags.Length - 1)
                output += ", ";
        }
        return output + "]";
    }
}