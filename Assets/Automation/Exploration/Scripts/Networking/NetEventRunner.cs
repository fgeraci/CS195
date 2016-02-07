using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Lidgren.Network;

public class NetEventRunner : MonoBehaviour 
{
    public GUISkin skin;
    public bool FullStoryMode = false;

    private string[] names;
    private uint[][] participants;

    public NetworkClient Client;

    bool[] sentimentsLocal;
    bool[] sentimentsRemote;

    private bool paused = true;

    private static Dictionary<uint, string> participantToName = 
      new Dictionary<uint, string>
    {
      {0, "Teller" },
      {22, "Guard" },
      {5, "Robber" },
    };

    void Awake()
    {
      this.names = null;
      this.participants = null;

      this.sentimentsLocal = null;
      this.sentimentsRemote = null;
    }

    private void EventStatusChanged(
        BehaviorEvent sender, 
        EventStatus newStatus)
    {
        if (newStatus == EventStatus.Finished)
        {
            this.SendFreeObjects(sender.Token.Get<uint[]>());
            this.SendRecommendationRequest();
            this.paused = true;
        }
    }

    public void OnConnected()
    {
        this.SendRecommendationRequest();
    }

    private string FormatParticipant(uint id)
    {
        if (participantToName.ContainsKey(id) == true)
            return participantToName[id];
        return "";
    }

    private string FormatParticipants(uint[] ids)
    {
      List<string> formatted = new List<string>();
      foreach (uint id in ids)
      {
          string name = FormatParticipant(id);
          if (name.Length > 0)
              formatted.Add(name);
      }
      return string.Join(", ", formatted.ToArray());
    }

    private string FormatName(string name)
    {
        string formatted = name.Substring(5);
        if (formatted == "IncapacitateStealthilySpecial")
            return "Incapacitate";
        if (formatted == "IncapacitateStealthily")
            return "Incapacitate";
        if (formatted == "ManagerButtonPress")
            return "PressManagerButton";
        if (formatted == "TellerButtonPress")
            return "PressTellerButton";
        return formatted;
    }

    private string[] GetOptions(string[] names, uint[][] participants)
    {
        int count = names.Length;
        string[] result = new string[count];

        for (int i = 0; i < count; i++)
        {
          result[i] =
            FormatName(names[i]) +
            "(" +
            FormatParticipants(participants[i]) +
            ")";
        }

        return result;
    }
	
    public void ReceivedCandidates(string[] names, uint[][] participants)
    {
        string[] options = GetOptions(names, participants);
        foreach (string name in options)
          Debug.Log(name);

        this.names = names;
        this.participants = participants;

        if (this.FullStoryMode == true)
        {
          if (names.Length > 0)
          {
            this.ExecuteEvent(0);
          }
          else
          {
            Debug.Log("No remaining event options to execute.");
          }
        }
    }

    private void ExecuteEvent(int index)
    {
        string name = this.names[index];
        uint[] participants = this.participants[index];
        Debug.LogWarning("Executing: " + GetOptions(this.names, this.participants)[0]);

        EventSignature sig = EventLibrary.Instance.GetSignature(name);
        SmartObject[] objs = new SmartObject[participants.Length];
        for (int i = 0; i < participants.Length; i++)
            objs[i] = ObjectManager.Instance.GetObjectById(participants[i]);

        SmartEvent evt = sig.Create(objs);
        evt.Behavior.StatusChanged += EventStatusChanged;
        evt.StartEvent(1.0f);
        this.paused = false;

        uint[] actualParticipants = 
            sig.FilterNonParticipantIds(participants).ToArray();
        evt.Behavior.Token = new Token(actualParticipants);

        this.SendSelection(index, actualParticipants);
        this.names = null;
        this.participants = null;
    }

    private void SendRecommendationRequest()
    {
        NetOutgoingMessage msg = Client.CreateMessage();
        msg.Write((byte)MsgType.RecommendationRequest);
        Client.SendMessage(msg);
    }

    private void SendSentiments()
    {
      NetOutgoingMessage msg = Client.CreateMessage();
      msg.Write((byte)MsgType.Sentiments);

      List<string> wantSentiments = new List<string>();
      List<string> dontSentiments = new List<string>();
      for (int i = 0; i < sentimentsLocal.Length; i++)
        if (sentimentsLocal[i] == true)
          wantSentiments.Add(SENTIMENTS[i]);
        else
          dontSentiments.Add(SENTIMENTS[i]);

      msg.Write((int)wantSentiments.Count);
      foreach (string sentiment in wantSentiments)
        msg.Write(sentiment);

      msg.Write((int)dontSentiments.Count);
      foreach (string sentiment in dontSentiments)
        msg.Write(sentiment);

      Client.SendMessage(msg);
      this.sentimentsRemote = this.sentimentsLocal;
    }

    private void SendSelection(int index, IEnumerable<uint> usedObjs)
    {
        NetOutgoingMessage msg = Client.CreateMessage();
        msg.Write((byte)MsgType.RecommendationSelected);
        msg.Write(index);
        msg.Write(usedObjs.Count());
        foreach (uint id in usedObjs)
            msg.Write(id);
        Client.SendMessage(msg);
    }

    private void SendFreeObjects(uint[] ids)
    {
        NetOutgoingMessage msg = Client.CreateMessage();
        msg.Write((byte)MsgType.ObjectsFreed);
        msg.Write(ids.Count());
        foreach (uint id in ids)
            msg.Write(id);
        Client.SendMessage(msg);
    }

	// Update is called once per frame
	void Update () 
    {
        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    this.ExecuteEvent(0);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    this.ExecuteEvent(1);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    this.ExecuteEvent(2);
        //}
	}

  private bool initialized = false;

  private static string[] SENTIMENTS = 
  { 
    "Brave", "Violent", "Nonviolent", "Negligent" 
  };

  void OnGUI()
  {
    if (this.initialized == false)
    {
      this.sentimentsLocal = new bool[SENTIMENTS.Length];

      this.initialized = true;
    }

    GUI.skin = this.skin;

    int padding = 10;

    int boxX = padding;
    int boxY = padding;
    int boxWidth = 400;

    int pauseX = boxX + padding;
    int pauseY = boxY + padding;
    int pauseWidth = boxWidth - (padding * 2);
    int pauseHeight = 25;

    int sentX = boxX + padding;
    int sentY = pauseY + pauseHeight + padding;
    int sentWidth = boxWidth - (padding * 2);
    int sentHeight = 20;
    int sentPadding = 5;
    int totalSentHeight =
      CalcTotalElementHeight(sentHeight, sentPadding, SENTIMENTS);

    int choiceX = boxX + padding;
    int choiceY = sentY + totalSentHeight + padding;
    int choiceWidth = boxWidth - (padding * 2);
    int choiceHeight = 25;
    int choicePadding = 5;

    bool hasChoices = (this.names != null && this.names.Length > 0);

    int boxHeight;
    if (hasChoices == true)
    {
      string[] choices = this.GetOptions(this.names, this.participants);
      int totalChoiceHeight =
        CalcTotalElementHeight(choiceHeight, choicePadding, choices);
      boxHeight =
        pauseHeight + totalSentHeight + totalChoiceHeight + (padding * 4);
    }
    else
    {
      boxHeight =
        pauseHeight + totalSentHeight + (padding * 3);
    }

    GUI.Box(
      new Rect(
        boxX,
        boxY,
        boxWidth,
        boxHeight),
        "");

    string controlText;
    if (this.FullStoryMode == true)
      controlText = "Pause after Next Event";
    else
      controlText = "Select Events Automatically";

    bool pressed = GUI.Button(
      new Rect(
        pauseX,
        pauseY,
        pauseWidth,
        pauseHeight),
      controlText);

    if (pressed == true)
    {
      this.FullStoryMode = !this.FullStoryMode;
      if (this.FullStoryMode == true)
        this.ExecuteEvent(0);
    }
      
    this.sentimentsLocal = DrawSentiments(
      sentX,
      sentY,
      sentWidth,
      sentHeight,
      sentPadding,
      SENTIMENTS,
      this.sentimentsLocal);

    if (this.Client.Connected == true)
      UpdateSentiments();

    // Recalculate this since it might change mid-draw
    hasChoices = (this.names != null && this.names.Length > 0);

    if (hasChoices == true)
    {
      string[] choices =
        this.GetOptions(this.names, this.participants);

      bool[] selected = DrawChoices(
        choiceX,
        choiceY,
        choiceWidth,
        choiceHeight,
        choicePadding,
        choices);

      for (int i = 0; i < selected.Length; i++)
      {
        if (selected[i] == true)
        {
          ExecuteEvent(i);
          break;
        }
      }
    }
  }

  private void UpdateSentiments()
  {
    if (this.sentimentsRemote == null)
    {
      this.SendSentiments();
    }
    else
    {
      for (int i = 0; i < this.sentimentsLocal.Length; i++)
      {
        if (this.sentimentsRemote[i] != sentimentsLocal[i])
        {
          this.SendSentiments();
          this.SendRecommendationRequest();
          return;
        }
      }
    }
  }

  private static int CalcTotalElementHeight(
    int height,
    int padding,
    string[] names)
  {
    if (names.Length > 0)
      return
        (names.Length * (height + padding)) - padding;
    return 0;
  }

  private static bool[] DrawSentiments(
    int sentX,
    int sentY,
    int sentWidth,
    int sentHeight,
    int sentPadding,
    string[] sentNames,
    bool[] currentSentiments)
  {
    bool[] selected = new bool[sentNames.Length];

    int offset = 0;
    for (int i = 0; i < sentNames.Length; i++)
    {
      selected[i] = GUI.Toggle(
        new Rect(
          sentX,
          sentY + offset,
          sentWidth,
          sentHeight),
          currentSentiments[i],
          sentNames[i]);
      offset += sentHeight + sentPadding;
    }
    return selected;
  }

  private static bool[] DrawChoices(
    int choiceX,
    int choiceY,
    int choiceWidth,
    int choiceHeight,
    int choicePadding,
    string[] choiceNames)
  {
    bool[] selected = new bool[choiceNames.Length];

    int offset = 0;
    for (int i = 0; i < choiceNames.Length; i++)
    {
      selected[i] = GUI.Button(
        new Rect(
          choiceX,
          choiceY + offset,
          choiceWidth,
          choiceHeight),
        choiceNames[i]);
      offset += choiceHeight + choicePadding;
    }
    return selected;
  }
}
