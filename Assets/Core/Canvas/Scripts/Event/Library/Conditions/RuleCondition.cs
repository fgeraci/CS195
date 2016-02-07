// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System.Collections;

public class RuleCondition : ICondition
{
    public readonly int IndexFrom;
    public readonly int IndexTo;
    public readonly RuleName[] Rules;

    public RuleCondition(
        int indexFrom,
        int indexTo,
        params RuleName[] rules)
    {
        this.IndexFrom = indexFrom;
        this.IndexTo = indexTo;
        this.Rules = rules;
    }

    public override string ToString()
    {
        string output =
            "[(" + this.IndexFrom
            + " -> " + this.IndexTo + "): ";
        for (int i = 0; i < this.Rules.Length; i++)
        {
            output += this.Rules[i].ToString();
            if (i < this.Rules.Length - 1)
                output += ", ";
        }
        return output + "]";
    }
}