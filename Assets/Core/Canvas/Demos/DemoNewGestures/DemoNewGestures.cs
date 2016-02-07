using UnityEngine;
using System.Collections.Generic;
using System;

public class DemoNewGestures : MonoBehaviour 
{
    private class AnimationChoice
    {
        public bool isActive;

        public string gestureName;

        public Action<string, bool> animationFunc;

        public AnimationChoice(string gestureName, Action<string, bool> animationFunc)
        {
            this.gestureName = gestureName;
            this.animationFunc = animationFunc;
            this.isActive = false;
        }
    }

    private List<AnimationChoice> animations;

    public BodyMecanim body;

    void Start()
    {
        animations = new List<AnimationChoice>();
        animations.Add(new AnimationChoice("Cry", body.HandAnimation));
        animations.Add(new AnimationChoice("Yawn", body.HandAnimation));
        animations.Add(new AnimationChoice("Think", body.HandAnimation));
        animations.Add(new AnimationChoice("Surprised", body.HandAnimation));
        animations.Add(new AnimationChoice("Wave", body.HandAnimation));
        animations.Add(new AnimationChoice("Texting", body.HandAnimation));
        animations.Add(new AnimationChoice("Clap", body.HandAnimation));
        animations.Add(new AnimationChoice("Talking On Phone", body.BodyAnimation));
        animations.Add(new AnimationChoice("CallOver", body.HandAnimation));
        animations.Add(new AnimationChoice("StayAway", body.HandAnimation));
        animations.Add(new AnimationChoice("MouthWipe", body.HandAnimation));
        animations.Add(new AnimationChoice("Shock", body.HandAnimation));
    }

    void OnGUI()
    {
        foreach (AnimationChoice choice in animations)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(choice.gestureName);
            choice.isActive = GUILayout.Toggle(choice.isActive, "Active");
            if (GUILayout.Button("Set"))
            {
                choice.animationFunc.Invoke(choice.gestureName, choice.isActive);
            }
            GUILayout.EndHorizontal();
        }
    }

}
