using UnityEngine;
using System.Collections;

using TreeSharpPlus;
using RootMotion.FinalIK;

public class SmartCart : SmartObject 
{
    public override string Archetype
    {
        get { return "SmartCart"; }
    }

    public Material MoneyMaterial;
    public ParticleSystem[] Particles;
    public Transform ApproachWaypoint;
    public InteractionObject InteractionReachOut;
    public InteractionObject InteractionReachIn;

    public float Delay = 1.5f;

    private Interpolator<float> fade;

    void Start()
    {
        this.fade = new Interpolator<float>(0.0f, 1.0f, Mathf.Lerp);
        this.fade.ForceMax();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            this.PlayParticles();

        Color color = this.MoneyMaterial.color;
        color.a = this.fade.Value;
        this.MoneyMaterial.color = color;
    }

    public void FadeOut()
    {
        this.fade.ToMin(this.Delay);
    }

    public void FadeIn()
    {
        this.fade.ToMax(this.Delay);
    }

    public void PlayParticles()
    {
        foreach (ParticleSystem part in this.Particles)
        {
            if (part != null)
            {
                part.Stop();
                part.Clear();
                part.Play();
            }
        }
    }

    [Affordance]
    protected Node PickupMoney(SmartCharacter user)
    {
        return new Sequence(
            user.ST_StandAtWaypoint(this.ApproachWaypoint),
            user.Node_StartInteraction(FullBodyBipedEffector.RightHand, this.InteractionReachOut),
            user.Node_StartInteraction(FullBodyBipedEffector.LeftHand, this.InteractionReachOut),
            new LeafWait(800),
            user.Node_StartInteraction(FullBodyBipedEffector.RightHand, this.InteractionReachIn),
            new LeafWait(300),
            new LeafInvoke(() => this.FadeOut()),
            new LeafInvoke(() => user.Backpack.FadeIn()),
            new LeafWait(500),
            new LeafInvoke(() => this.PlayParticles()),
            user.Node_StartInteraction(FullBodyBipedEffector.LeftHand, this.InteractionReachIn),
            new LeafWait(800),
            user.Node_StopInteraction(FullBodyBipedEffector.LeftHand),
            user.Node_StopInteraction(FullBodyBipedEffector.RightHand),
            this.Node_Set(~StateName.HasBackpack),
            user.Node_Set(StateName.HasBackpack),
            new LeafWait(500));
    }
}
