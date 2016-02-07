using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;

public class Storyteller
{
    public static string Run(ExplorationSpace space)
    {
        string allStories = "";
        foreach (var scorer in GoalSelector.Scorers)
        {
            allStories += "#" + scorer.Method.Name + "\n";
            IEnumerable<Candidate> candidates =
                GoalSelector.GetCandidates(
                    space,
                    space.Nodes[0],
                    scorer);

            foreach (Candidate candidate in candidates)
                allStories += FormatStory(candidate) + "%\n";
        }
        return allStories;
    }

    private static string FormatStory(Candidate candidate)
    {
        string output = "";
        foreach (ExplorationEdge edge in candidate.Path)
        {
            TransitionEvent evt = edge.Events[0];
            EventDescriptor desc = evt.Descriptor;
            string name = desc.Name;

            string[] members = new string[evt.Participants.Length];
            for (int i = 0; i < members.Length; i++)
                members[i] = FormatId(evt.Participants[i]);

            output += FormatLine(name, members);
            if (desc.Sentiments.Length > 0)
              output += " (" + string.Join(", ", desc.Sentiments) + ")";
            output += "\n";
        }
        return output;
    }

    private static string FormatLine(string name, string[] members)
    {

        switch (name)
        {
            case "Mini_UnlockTellerDoor":
                return members[0] + " unlocks the teller door.";
            case "Mini_TellerButtonPress":
                return members[0] + " presses the teller button.";
            case "Mini_CoerceIntoGivingKey":
                return members[0] + " coerces " + members[1] + " to hand over the keys at gunpoint.";
            case "Mini_UnlockManagerDoor":
                return members[0] + " unlocks the manager door.";
            case "Mini_PickupBriefcase":
                return members[0] + " picks up the stolen money.";
            case "Mini_TalkTo":
                return members[0] + " talks to " + members[1] + ".";
            case "Mini_Leave":
                return members[0] + " leaves.";
            case "Mini_PickupWeaponTeller":
                return members[0] + " picks up the gun.";
            case "Mini_PickupWeapon":
                return members[0] + " picks up the gun.";
            case "Mini_DropWeapon":
                return members[0] + " drops the gun.";
            case "Mini_ManagerButtonPress":
                return members[0] + " presses the manager button.";
            case "Mini_OpenVaultDoor":
                return members[0] + " opens the vault door.";
            case "Mini_PickupMoneyFromCart":
                return members[0] + " picks up money from a vault cart.";
            case "Mini_TakeKeysFromIncapacitated":
                return members[0] + " takes keys from incapacitated " + members[1] + ".";
            case "Mini_IncapacitateStealthily":
                return members[0] + " incapacitates " + members[1] + ".";
            case "Mini_IncapacitateStealthilySpecial":
                return members[0] + " incapacitates " + members[1] + ".";
            default:
                return RawFormat(name, members);
        }
    }

    private static string FormatId(uint id)
    {
        switch(id)
        {
            case 0: return "Teller";
            case 5: return "Robber";
            case 22: return "Guard";
            case 122: return "VaultContainer";
            case 127: return "TellerDoor";
            case 128: return "ManagerDoor";
            case 129: return "VaultDoor";
            case 131: return "TellerButton";
            case 130: return "ManagerButton";
            case 204: return "DynamicContainer1";
            case 205: return "DynamicContainer2";
            case 903: return "LeaveWaypoint";
            default: return "ID:" + id;
        }
    }

    private static string RawFormat(string name, string[] members)
    {
        string output = name + "(";
        for (int i = 0; i < members.Length; i++)
            output += members[i] + ((i == members.Length - 1) ? "" : ", ");
        return output + ")";
    }
}

