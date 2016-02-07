using UnityEngine;
using TreeSharpPlus;
using System.Collections.Generic;
using System;

using RootMotion.FinalIK;

public class SmartCharacter : SmartObject
{

    protected struct AnimationDescription
    {
        public readonly string Name;
        public readonly AnimationLayer Layer;

        public AnimationDescription(string name, AnimationLayer layer)
        {
            this.Name = name;
            this.Layer = layer;
        }
    }

    #region AnimationDescriptions for various conversation types
    private Tuple<AnimationDescription[], AnimationDescription[]> normalConversation =
        new Tuple<AnimationDescription[], AnimationDescription[]>(
            new AnimationDescription[] {
                new AnimationDescription("cheer", AnimationLayer.Hand),
                new AnimationDescription("BeingCocky", AnimationLayer.Hand),
                new AnimationDescription("acknowledge", AnimationLayer.Face),
                new AnimationDescription("lookaway", AnimationLayer.Face)
            },
            new AnimationDescription[] {
                new AnimationDescription("HeadNod", AnimationLayer.Face),
                new AnimationDescription("HeadShake", AnimationLayer.Face),
                new AnimationDescription("chestpumpsalute", AnimationLayer.Hand),
                new AnimationDescription("cowboy", AnimationLayer.Hand)
            });

    private Tuple<AnimationDescription[], AnimationDescription[]> secretiveConversation =
        new Tuple<AnimationDescription[], AnimationDescription[]>(
            new AnimationDescription[] {
                new AnimationDescription("acknowledge", AnimationLayer.Face),
                new AnimationDescription("headnod", AnimationLayer.Face)
            },
            new AnimationDescription[] {
                new AnimationDescription("acknowledge", AnimationLayer.Face),
                new AnimationDescription("headshake", AnimationLayer.Face)
            });

    private Tuple<AnimationDescription[], AnimationDescription[]> happyConversation =
        new Tuple<AnimationDescription[], AnimationDescription[]>(
            new AnimationDescription[] {
                new AnimationDescription("cheer", AnimationLayer.Hand),
                new AnimationDescription("BeingCocky", AnimationLayer.Hand),
                new AnimationDescription("acknowledge", AnimationLayer.Face),
                new AnimationDescription("lookaway", AnimationLayer.Face)
            },
            new AnimationDescription[] {
                new AnimationDescription("HeadNod", AnimationLayer.Face),
                new AnimationDescription("HeadShake", AnimationLayer.Face),
                new AnimationDescription("chestpumpsalute", AnimationLayer.Hand),
                new AnimationDescription("cowboy", AnimationLayer.Hand)
            });

    private Tuple<AnimationDescription[], AnimationDescription[]> haggle =
        new Tuple<AnimationDescription[], AnimationDescription[]>(
            new AnimationDescription[] {
                new AnimationDescription("cowboy", AnimationLayer.Hand),
                new AnimationDescription("wonderful", AnimationLayer.Hand),
                new AnimationDescription("HeadShakeThink", AnimationLayer.Face),
                new AnimationDescription("Acknowledge", AnimationLayer.Face)
            },
            new AnimationDescription[] {
                new AnimationDescription("LookAway", AnimationLayer.Face),
                new AnimationDescription("HeadNod", AnimationLayer.Face),
                new AnimationDescription("crowdpump", AnimationLayer.Hand),
                new AnimationDescription("cowboy", AnimationLayer.Hand)
            });
    #endregion

    public override string Archetype { get { return "SmartCharacter"; } }

    public Transform trackTarget = null;

    /// <summary>
    /// Head marker for making eye contact
    /// </summary>
    public Transform MarkerHead;

    /// <summary>
    /// Fading backpack prop, if applicable
    /// </summary>
    public FadingObject Backpack;

    /// <summary>
    /// Interaction point for holding the ball right
    /// </summary>
    public InteractionObject InteractionHoldRight;

    /// <summary>
    /// Interaction point for reaching to receive the ball
    /// </summary>
    public InteractionObject InteractionTake;

    /// <summary>
    /// Interaction point for reaching to give the ball
    /// </summary>
    public InteractionObject InteractionGive;

    /// <summary>
    /// Interaction point for reaching when stealing the wallet
    /// </summary>
    public InteractionObject InteractionStealWallet;

    /// <summary>
    /// Interaction point for reaching on self when storing the wallet
    /// </summary>
    public InteractionObject InteractionStoreWallet;

    /// <summary>
    /// Reflexive interaction point for pointing a gun
    /// </summary>
    public InteractionObject InteractionPointGun;

    /// <summary>
    /// Reflexive interaction point for pointing a gun into the air.
    /// </summary>
    public InteractionObject InteractionPointGunUpwards;

    /// <summary>
    /// For giving a key during coercion
    /// </summary>
    public InteractionObject InteractionCoerceKeyGive;

    /// <summary>
    /// For taking a key during coercion
    /// </summary>
    public InteractionObject InteractionCoerceKeyTake;

    /// <summary>
    /// For taking a key from an incapacitated character
    /// </summary>
    public InteractionObject InteractionGetKeyIncapacitated;

    /// <summary>
    /// For taking a gun from an incapacitated character
    /// </summary>
    public InteractionObject InteractionGetGunIncapacitated;

    /// <summary>
    /// For surrendering
    /// </summary>
    public InteractionObject InteractionSurrenderLeft;

    /// <summary>
    /// For surrendering
    /// </summary>
    public InteractionObject InteractionSurrenderRight;

    /// <summary>
    /// For dropping a weapon
    /// </summary>
    public InteractionObject InteractionDropGun;

    /// <summary>
    /// The prop holder for the right hand
    /// </summary>
    public PropHolder HoldPropRightHand;

    /// <summary>
    /// The prop holder for the left hand
    /// </summary>
    public PropHolder HoldPropLeftHand;

    /// <summary>
    /// The prop holder for the pocket
    /// </summary>
    public PropHolder HoldPropPocket;

    /// <summary>
    /// A hidden prop holder for objects to vanish but still be referencable
    /// </summary>
    public PropHolder HoldPropHidden;

    /// <summary>
    /// Waypoint for standing behind the character.
    /// </summary>
    public Transform WaypointBack;

    /// <summary>
    /// Waypoint for standing in front of the character.
    /// </summary>
    public Transform WaypointFront;

    /// <summary>
    /// Waypoint for picking up a key from an incapacitated character
    /// </summary>
    public Transform WaypointPickupKey;

    /// <summary>
    /// Waypoint for picking up a gun from an incapacitated character
    /// </summary>
    public Transform WaypointPickupGun;

    /// <summary>
    /// Stars decoration
    /// </summary>
    public Stars DecorationStars;

    /// <summary>
    /// For transform correction during incapacitate.
    /// </summary>
    private Interpolator<Vector3> IncapacitateNudge = null;

    private CharacterMecanim character = null;
    private BehaviorMecanim behavior = null;

    public CharacterMecanim Character { get { return this.character; } }
    public BehaviorMecanim Behavior { get { return this.behavior; } }

    protected override void Initialize(BehaviorObject obj)
    {
        base.Initialize(obj);

        this.character = this.GetComponent<CharacterMecanim>();
        this.behavior = this.GetComponent<BehaviorMecanim>();

        if (this.MarkerHead == null)
            this.MarkerHead = 
                this.GetComponent<Animator>().GetBoneTransform(
                    HumanBodyBones.Head);
    }

    void Awake()
    {
        this.Initialize(
            new BehaviorAgent(
                new DecoratorLoop(
                    new LeafAssert(() => true))));
    }

    void Update()
    {
        if (this.trackTarget != null)
            this.transform.rotation =
                Quaternion.LookRotation(
                    this.trackTarget.position - this.transform.position,
                    Vector3.up);
        if (this.IncapacitateNudge != null)
            this.transform.position = this.IncapacitateNudge.Value;
    }

    private void CleanupOrientation(SmartObject user)
    {
        user.GetComponent<CharacterMecanim>().NavOrientBehavior(OrientationBehavior.LookForward);
        character.NavOrientBehavior(OrientationBehavior.LookForward);
    }

    public Prop GetRightProp()
    {
        return this.HoldPropRightHand.CurrentProp;
    }

    [Affordance]
    protected Node RaiseGun(SmartObject user)
    {
        return this.Node_StartInteraction(FullBodyBipedEffector.RightHand, this.InteractionPointGun);
    }

    [Affordance]
    protected Node LowerGun(SmartObject user)
    {
        return this.Node_StopInteraction(FullBodyBipedEffector.RightHand);
    }

    [Affordance]
    protected Node Surrender(SmartCharacter user)
    {
        return new Sequence(
            this.Node_StartInteraction(FullBodyBipedEffector.LeftHand, this.InteractionSurrenderLeft),
            this.Node_StartInteraction(FullBodyBipedEffector.RightHand, this.InteractionSurrenderRight),
            new LeafWait(1200),
            this.Node_Set(StateName.IsImmobile));
    }

    [Affordance]
    protected Node CoerceGiveKey(SmartCharacter user)
    {
        return new Sequence(
            //user.ST_StandAtWaypoint(this.WaypointFront),
            user.Node_Icon("key"),
            this.Node_StartInteraction(FullBodyBipedEffector.LeftHand, this.InteractionCoerceKeyTake),
            new LeafWait(1000),
            user.Node_StartInteraction(FullBodyBipedEffector.RightHand, this.InteractionCoerceKeyGive),
            new LeafWait(1000),
            user.Node_Icon(null),
            this.Node_Icon("key"),
            this.Node_StopInteraction(FullBodyBipedEffector.LeftHand),
            new LeafWait(300),
            user.Node_StopInteraction(FullBodyBipedEffector.RightHand),
            new LeafWait(700),
            this.Node_Icon(null));
    }

    [Affordance]
    protected Node GetKeyIncapacitated(SmartCharacter user)
    {
        return new Sequence(
            user.ST_StandAtWaypoint(this.WaypointPickupKey),
            user.Node_StartInteraction(FullBodyBipedEffector.LeftHand, this.InteractionGetKeyIncapacitated),
            new LeafWait(1000),
            user.Node_Icon("key"),
            user.Node_StopInteraction(FullBodyBipedEffector.LeftHand),
            new LeafWait(1000),
            user.Node_Set(StateName.HasKeys),
            this.Node_Set(~StateName.HasKeys),
            user.Node_Icon(null));
    }

    [Affordance]
    protected Node GetGunIncapacitated(SmartCharacter user)
    {
        return new Sequence(
            user.ST_StandAtWaypoint(this.WaypointPickupGun),
            user.Node_StartInteraction(FullBodyBipedEffector.RightHand, this.InteractionGetGunIncapacitated),
            new LeafWait(500),
            new LeafInvoke(() => this.GetRightProp().FadeOut()),
            new LeafWait(500),
            new LeafInvoke(() => user.HoldPropRightHand.Attach(this.HoldPropRightHand.Release())),
            new LeafInvoke(() => user.GetRightProp().FadeIn()),
            user.Node_StopInteraction(FullBodyBipedEffector.RightHand),
            this.Node_Set(~StateName.RightHandOccupied, ~StateName.HoldingWeapon),
            user.Node_Set(StateName.RightHandOccupied, StateName.HoldingWeapon),
            new LeafWait(1000));
    }

    [Affordance]
    protected Node GiveBriefcase(SmartCharacter user)
    {
        return new Sequence(
            user.Node_GoToUpToRadius(Val.V(() => this.transform.position), 1.5f),
            new SequenceParallel(
                user.Node_OrientTowards(Val.V(() => this.transform.position)),
                this.Node_OrientTowards(Val.V(() => user.transform.position))),
            new SequenceParallel(
                user.Node_StartInteraction(FullBodyBipedEffector.LeftHand, this.InteractionGive),
                new Sequence(
                    new LeafWait(1000),
                    this.Node_StartInteraction(FullBodyBipedEffector.LeftHand, this.InteractionTake)),
                user.Node_WaitForTrigger(FullBodyBipedEffector.LeftHand),
                this.Node_WaitForTrigger(FullBodyBipedEffector.LeftHand)),
            new LeafInvoke(() => this.HoldPropLeftHand.Attach(user.HoldPropLeftHand.Release())),
            user.Node_Set(~StateName.HasBackpack),
            this.Node_Set(StateName.HasBackpack),
            new SequenceParallel(
                user.Node_StopInteraction(FullBodyBipedEffector.LeftHand),
                this.Node_StopInteraction(FullBodyBipedEffector.LeftHand)));
    }

    [Affordance]
    protected Node TakeWeaponIncapacitated(SmartCharacter user)
    {
        return new Sequence(
            user.Node_GoToUpToRadius(Val.V(() => this.WaypointFront.position), 1.0f),
            user.Node_OrientTowards(Val.V(() => this.transform.position)),
            //TODO new interaction object to take the weapon?
            user.Node_StartInteraction(FullBodyBipedEffector.RightHand, this.InteractionGive),
            user.Node_WaitForTrigger(FullBodyBipedEffector.RightHand),
            new LeafInvoke(() => user.HoldPropRightHand.Attach(this.HoldPropRightHand.Release())),
            this.Node_Set(~StateName.RightHandOccupied, ~StateName.HoldingWeapon),
            user.Node_Set(StateName.RightHandOccupied, StateName.HoldingWeapon),
            new LeafInvoke(() => Debug.Log("Here2")),
            user.Node_StopInteraction(FullBodyBipedEffector.RightHand));
    }

    private void CreateSlidebackTarget()
    {
        Vector3 slideBack = 
            this.transform.position - (this.transform.forward * 0.4f);
        this.IncapacitateNudge =
            new Interpolator<Vector3>(
                transform.position,
                slideBack,
                Vector3.Lerp);
        this.IncapacitateNudge.ForceMin();
        this.IncapacitateNudge.ToMax(0.8f);
    }

    [Affordance]
    protected Node IncapacitateFromBehind(SmartCharacter user)
    {
        return new Sequence(
            user.Node_GoTo(Val.V(() => this.WaypointBack.transform.position)),
            user.Node_Orient(Val.V(() => this.WaypointBack.transform.rotation)),
            user.behavior.ST_PlayHandGesture("bash", 1000),
            new LeafInvoke(() => this.CreateSlidebackTarget()),
            this.behavior.ST_PlayBodyGesture("dying", 2000),
            new LeafInvoke(() => this.DecorationStars.gameObject.SetActive(true)),
            this.Node_Set(StateName.IsIncapacitated));
    }

    [Affordance]
    public Node TakeKeysFromBehind(SmartCharacter user)
    {
        return new Sequence(
            user.Node_GoTo(new Val<Vector3>(() => this.WaypointBack.transform.position)),
            user.Node_OrientTowards(Val.V(() => this.transform.position)),
            //this.Node_Icon("key"),
            user.Node_StartInteraction(FullBodyBipedEffector.RightHand, this.InteractionStealWallet),
            // user.Node_WaitForTrigger(FullBodyBipedEffector.RightHand),
            user.Node_StopInteraction(FullBodyBipedEffector.RightHand),
            // this.Node_Icon(null),
            // user.Node_Icon("key"),
            new LeafWait(1000)//,
            // user.Node_Icon(null),
            // this.Node_Set(~StateName.HasKeys),
            // user.Node_Set(StateName.HasKeys));
        );    
    }


    [Affordance]
    protected Node Talk(SmartCharacter user)
    {
        return new Sequence(
            user.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated),
            this.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated),
            new LeafAssert(() => user != this), // Not reflexive
            this.ST_Talk(user)
        );
    }

    [Affordance]
    protected Node TalkHappily(SmartCharacter user)
    {
        return new Sequence(
            user.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated),
            this.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated),
            new LeafAssert(() => user != this), // Not reflexive
            this.ST_TalkHappily(user),
            new SequenceParallel(
                this.Node_Icon("happy"),
                user.Node_Icon("happy")),
            new LeafWait(1500),
            new SequenceParallel(
                this.Node_Icon(null),
                user.Node_Icon(null))
        );
    }

    [Affordance]
    protected Node TalkAngrily(SmartCharacter user)
    {
        return new Sequence(
            user.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated),
            this.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated),
            new LeafAssert(() => user != this), // Not reflexive
            this.ST_Talk(user),
            new SequenceParallel(
                this.Node_Icon("sad"),
                user.Node_Icon("sad")),
            new LeafWait(1500),
            new SequenceParallel(
                this.Node_Icon(null),
                user.Node_Icon(null))
        );
    }

    [Affordance]
    protected Node TalkSecretively(SmartObject user)
    {
        return new Sequence(
            user.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated),
            this.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated),
            new LeafAssert(() => user != this), // Not reflexive
            this.ST_TalkSecretive((SmartCharacter)user)
        );
    }

    [Affordance]
    protected Node Incapacitate(SmartCharacter user)
    {
        return new Sequence(
            user.Node_Require(StateName.RoleActor, StateName.IsStanding, 
                ~StateName.HoldingBall, ~StateName.HoldingDrink, ~StateName.IsIncapacitated),
            this.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.IsDead),
            new LeafAssert(() => user != this), // Not reflexive
            user.Node_GoToUpToRadius(new Val<Vector3>(this.transform.position), 1.0f),
            new LeafInvoke(() => this.DecorationStars.gameObject.SetActive(true)),
            this.Node_Set(StateName.IsIncapacitated)
        );
    }

    [Affordance]
    protected Node WakeUp(SmartObject user)
    {
        // User Casting
        // TODO: Ths is going to get ugly if the cast fails...
        SmartCharacter character = (SmartCharacter)user;

        return new Sequence(
            user.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated),
            this.Node_Require(StateName.RoleActor, StateName.IsIncapacitated, ~StateName.IsDead),
            new LeafAssert(() => user != this), // Not reflexive
            character.Node_GoToUpToRadius(new Val<Vector3>(this.transform.position), 1.0f),
            new LeafInvoke(() => this.DecorationStars.gameObject.SetActive(false)),
            this.Node_Set(~StateName.IsIncapacitated)
        );
    }

    [Affordance]
    protected Node Kill(SmartObject user)
    {
        // User Casting
        // TODO: Ths is going to get ugly if the cast fails...
        SmartCharacter character = (SmartCharacter)user;

        return new Sequence(
            user.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.HoldingBall, ~StateName.HoldingDrink, ~StateName.IsIncapacitated),
            this.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.IsDead),
            new LeafAssert(() => user != this), // Not reflexive
            character.Node_GoToUpToRadius(new Val<Vector3>(this.transform.position), 1.0f),
            new LeafInvoke(() => this.DecorationStars.gameObject.SetActive(false)),
            this.Node_Icon("skull"),
            this.Node_Set(StateName.IsDead, StateName.IsIncapacitated)
        );
    }

    [Affordance]
    protected Node TakeBall(SmartObject user)
    {
        // User Casting
        // TODO: Ths is going to get ugly if the cast fails...
        SmartCharacter character = (SmartCharacter)user;

        return new Sequence(
            user.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.HoldingBall, ~StateName.HoldingDrink, ~StateName.HoldingWallet, ~StateName.IsIncapacitated),
            this.Node_Require(StateName.RoleActor, StateName.HoldingBall, StateName.IsIncapacitated),
            character.Node_GoToUpToRadius(new Val<Vector3>(this.transform.position), 1.0f),
            character.Node_OrientTowards(new Val<Vector3>(this.transform.position)),

            new LeafInvoke(() => character.HoldPropRightHand.Attach(this.HoldPropRightHand.Release())),

            this.Node_Set(~StateName.HoldingBall),
            user.Node_Set(StateName.HoldingBall),

            new SequenceParallel(
                this.Node_StopInteraction(FullBodyBipedEffector.RightHand),
                character.Node_StartInteraction(FullBodyBipedEffector.RightHand, character.InteractionHoldRight)));
    }

    [Affordance]
    public Node TakeWallet(SmartObject user)
    {
        SmartCharacter character = (SmartCharacter)user;

        return new Sequence(
            //user.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.HoldingBall, ~StateName.HoldingWallet, ~StateName.HoldingDrink, ~StateName.IsIncapacitated),
            // this.Node_Require(StateName.RoleActor, StateName.HoldingWallet, StateName.IsIncapacitated),
            character.Node_GoToUpToRadius(new Val<Vector3>(this.transform.position), 1.0f),
            character.Node_OrientTowards(new Val<Vector3>(this.transform.position)),

            new LeafInvoke(() => character.HoldPropRightHand.Attach(this.HoldPropPocket.Release())),

            this.Node_Set(~StateName.HoldingWallet),
            user.Node_Set(StateName.HoldingWallet),

            character.Node_StartInteraction(FullBodyBipedEffector.RightHand, character.InteractionStoreWallet),
            character.Node_WaitForTrigger(FullBodyBipedEffector.RightHand),
            new LeafInvoke(() => character.HoldPropPocket.Attach(character.HoldPropRightHand.Release())),
            character.Node_ResumeInteraction(FullBodyBipedEffector.RightHand));
    }

    [Affordance]
    protected Node GiveBall(SmartCharacter user)
    {
        return ST_Give(
            user, 
            StateName.HoldingBall,
            new LeafWait(0),
            new LeafWait(0));
    }

    [Affordance]
    protected Node GiveWallet(SmartCharacter user)
    {
        return ST_Give(
            user, 
            StateName.HoldingWallet,
            user.ST_TakeWalletFromPocket(),
            this.ST_PutWalletInPocket());
    }

    [Affordance]
    public Node Steal(SmartObject user)
    {
        SmartCharacter character = (SmartCharacter)user;

        return new Sequence(
            // user.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.HoldingBall, ~StateName.HoldingWallet, ~StateName.HoldingDrink, ~StateName.IsIncapacitated),
            // this.Node_Require(StateName.RoleActor, StateName.IsStanding, StateName.HoldingWallet, ~StateName.IsIncapacitated),
            character.Node_GoTo(Val.V(() => this.WaypointBack.position)),
            character.Node_OrientTowards(Val.V(() => this.transform.position)),
            new SelectorParallel(
                character.Node_GoToUpToRadius(Val.V(() => this.transform.position), 0.1f),
                    new LeafWait(1000)), // Timeout
            character.Node_OrientTowards(Val.V(() => this.transform.position)),
            character.Node_StartInteraction(FullBodyBipedEffector.RightHand, this.InteractionStealWallet),
            character.Node_WaitForTrigger(FullBodyBipedEffector.RightHand),
            character.Node_ResumeInteraction(FullBodyBipedEffector.RightHand)
            // new LeafInvoke(() => character.HoldPropRightHand.Attach(this.HoldPropPocket.Release())),

            // this.Node_Set(~StateName.HoldingWallet),
            // user.Node_Set(StateName.HoldingWallet),

            //character.Node_StartInteraction(FullBodyBipedEffector.RightHand, character.InteractionStoreWallet),
            //character.Node_WaitForTrigger(FullBodyBipedEffector.RightHand)
            // new LeafInvoke(() => character.HoldPropPocket.Attach(character.HoldPropRightHand.Release())),
            //character.Node_ResumeInteraction(FullBodyBipedEffector.RightHand));
            );
    }

    [Affordance]
    protected Node Coerce(SmartCharacter user)
    {
        return new Sequence(
            // user.Node_Require(StateName.RoleActor, StateName.HoldingWeapon, StateName.IsStanding,
                // ~StateName.IsIncapacitated),
            // this.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated),
            new DecoratorForceStatus(
                RunStatus.Success,
                new Sequence(
                    new LeafAssert(() => WaypointFront == null),
                    this.ST_DoApproach(user, 1.0f))),
            new DecoratorForceStatus(
                RunStatus.Success,
                new Sequence(
                    new LeafAssert(() => WaypointFront != null),
                    user.Node_GoTo(Val.V(() => WaypointFront.position)),
                    user.Node_OrientTowards(this.transform.position),
                    this.Node_OrientTowards(Val.V(() => user.transform.position)))),
            //this.ST_DoApproach(user, 1.0f),
            user.Node_Icon("exclamation"),
            new LeafWait(2000),
            user.Node_Icon(null));
    }

    [Affordance]
    protected Node GiveKey(SmartCharacter user)
    {
        return new Sequence(
            user.Node_Require(StateName.RoleActor, StateName.HasKeys, StateName.IsStanding,
                ~StateName.IsIncapacitated),
            this.Node_Require(StateName.RoleActor, ~StateName.HasKeys, StateName.IsStanding,
                ~StateName.IsIncapacitated),
            user.Node_Icon("key"),
            new LeafWait(2000),
            user.Node_Icon(null),
            this.Node_Icon("key"),
            new LeafWait(2000),
            this.Node_Icon(null),
            user.Node_Set(~StateName.HasKeys),
            this.Node_Set(StateName.HasKeys));
    }

    #region Helper Subtrees
    public Node ST_StandAtWaypoint(Transform waypoint)
    {
        return new Sequence(
            this.Node_GoTo(Val.V(() => waypoint.position)),
            new Race(
                new LeafWait(3000),
                new Sequence(
                    this.Node_Orient(Val.V(() => waypoint.rotation)),
                    this.Node_NudgeTo(Val.V(() => waypoint.position)))),
            new LeafInvoke(() => this.transform.rotation = waypoint.rotation));
    }

    private Node ST_DoApproach(SmartCharacter user, float distance)
    {
        return new Sequence(user.ST_StandAtWaypoint(this.WaypointFront));
    }

    protected Node ST_Talk(SmartCharacter user)
    {
        return ST_Converse(
            user,
            normalConversation.Item1,
            normalConversation.Item2);
	}

    protected Node ST_TalkSecretive(SmartCharacter user)
    {
		return ST_Converse(
            user,
            secretiveConversation.Item1,
            secretiveConversation.Item2);
    }

    public Node ST_TalkHappily(SmartCharacter user)
	{
        return ST_Converse(
            user,
            happyConversation.Item1,
            happyConversation.Item1);
	}

    public Node ST_JustTalk(SmartCharacter user) {
        return ST_PlainConverse(
            user,
            happyConversation.Item1,
            happyConversation.Item1);
    }

    /// <summary>
    /// ABAS
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public Node ST_ApproachAndConverse(SmartCharacter user) {
        return ST_ApproachConverse(
            user,
            happyConversation.Item1,
            happyConversation.Item1);
    }

    protected Node ST_Haggle(SmartCharacter user)
    {
        return ST_Converse(
            user,
            haggle.Item1,
            haggle.Item2);
    }

    public Node ST_TalkWithoutApproach(SmartCharacter user)
    {
        return ST_PlayConversationAnims(
            user,
            secretiveConversation.Item1,
            secretiveConversation.Item2);
    }

    protected Node ST_Converse(
        SmartCharacter user, 
        AnimationDescription[] myAnims, 
        AnimationDescription[] userAnims)
    {
        return new Sequence(
			new SequenceParallel (
				Node_GoToUpToRadius(Val.V (() => user.transform.position), 1.3f),
				user.Node_GoToUpToRadius(Val.V (() => this.transform.position), 1.3f)),
                Node_HeadLook(Val.V(() => user.MarkerHead.position)),
                user.Node_HeadLook(Val.V(() => this.MarkerHead.position)),
            ST_PlayConversationAnims(user, myAnims, userAnims),
            Node_HeadLookStop(),
            user.Node_HeadLookStop()
        );
    }

    protected Node ST_PlainConverse(
        SmartCharacter user,
        AnimationDescription[] myAnims,
        AnimationDescription[] userAnims) {
        return new Sequence(
                Node_HeadLook(Val.V(() => user.MarkerHead.position)),
                user.Node_HeadLook(Val.V(() => this.MarkerHead.position)),
            ST_PlayConversationAnims(user, myAnims, userAnims),
            Node_HeadLookStop(),
            user.Node_HeadLookStop()
        );
    }

    protected Node ST_ApproachConverse(
        SmartCharacter user,
        AnimationDescription[] myAnims,
        AnimationDescription[] userAnims) {
        return new Sequence(
                Node_GoToUpToRadius(Val.V(() => user.transform.position), 1.3f),
                Node_HeadLook(Val.V(() => user.MarkerHead.position)),
                user.Node_HeadLook(Val.V(() => this.MarkerHead.position)),
                user.Node_HeadLook(Val.V(() => this.MarkerHead.position)),
            ST_PlayConversationAnims(user, myAnims, userAnims),
            Node_HeadLookStop(),
            user.Node_HeadLookStop()
        );
    }

    /// <summary>
    /// ABAS
    /// </summary>
    /// <param name="userA"></param>
    /// <param name="userB"></param>
    /// <param name="myAnims"></param>
    /// <param name="userAnims"></param>
    /// <returns></returns>
    public Node ST_ConverseThree(
        SmartCharacter userA,
        SmartCharacter userB) {
        return new Sequence(
            new SequenceParallel(
                Node_CallForAttention(userA),
                userA.Node_GoToUpToRadius(Val.V(() => this.transform.position), 1.2f),
                userB.Node_GoToUpToRadius(Val.V(() => this.transform.position), 1.2f)),
                userB.Node_HeadLook(Val.V(() => this.MarkerHead.position)),
                userB.Node_PlayHandGesture(Val<string>.V(() => "Dismiss"), Val<long>.V(() => 2000L)),
            new Sequence(
                    ST_TalkHappily(userA)),
                    new SequenceParallel(
                        userA.Node_OrientTowards(Val.V(() => userB.transform.position)),
                        userB.ST_TalkHappily(this),
                        ST_TalkHappily(userA))
        );
    }

    /// <summary>
    /// ABAS
    /// </summary>
    /// <param name="userA"></param>
    /// <param name="userB"></param>
    /// <param name="myAnims"></param>
    /// <param name="userAnims"></param>
    /// <returns></returns>
    public Node ST_DistractAndSteal(
        SmartCharacter userA,
        SmartCharacter userB) {
            Vector3 origPosition = userB.transform.position;
        return new Sequence(
            userA.Node_WaveTo(this),
            this.Node_OrientTowards(Val.V(() => userA.transform.position)),
            this.Node_GoToUpToRadius(Val.V(() => userA.transform.position), 1.2f),
            new SequenceParallel(
                userB.Node_HeadLook(Val.V(() => this.MarkerHead.position)),
            new SequenceParallel(
                    userA.ST_JustTalk(this),
                    new Sequence(
                        userB.Node_OrientTowards(Val.V(() => this.transform.position)),
                        new LeafWait(2000),
                        this.Steal(userB),
                        new LeafWait(1800),
                        userB.Node_GoTo(Val.V(() => origPosition)),
                        userB.Node_WaveTo(userA),
                        userA.Node_GoToUpToRadius(Val.V(() => userB.transform.position), 1.2f),
                        userB.ST_TalkHappily(userA))))
        
        );
    }

	/// <summary>
	/// ABAS
	/// </summary>
	/// <returns>The call for attention.</returns>
	/// <param name="pTarg">P targ.</param>
	public Node Node_ApproachUser(SmartCharacter user) {
		return new Sequence(
			Node_GoToUpToRadius(Val.V (() => user.transform.position), 1.2f));
	}

	/// <summary>
	/// ABAS
	/// </summary>
	/// <returns>The call for attention.</returns>
	/// <param name="pTarg">P targ.</param>
	public Node Node_CallForAttention(SmartCharacter pTarg) {
		return new SequenceParallel(
				Node_OrientTowards(Val<Vector3>.V (() => pTarg.transform.position)),
			    Node_PlayHandGesture(Val<string>.V (() => "Wave"), Val<long>.V (() => 2000L)));
	}

    protected Node ST_FaceEachother(
        SmartCharacter user)
    {
        Quaternion thisLook = Quaternion.identity;
        Quaternion userLook = Quaternion.identity;

        return new Sequence(
            new LeafInvoke(() => thisLook = Quaternion.LookRotation(user.transform.position - this.transform.position, Vector3.up)),
            new LeafInvoke(() => userLook = Quaternion.LookRotation(this.transform.position - user.transform.position, Vector3.up)),
            new Race(
                new LeafWait(3000),
                new SequenceParallel(
                    this.Node_Orient(thisLook),
                    user.Node_Orient(userLook))),
            new SequenceParallel(
                new LeafInvoke(() => this.transform.rotation = thisLook),
                new LeafInvoke(() => user.transform.rotation = userLook)));
    }

    protected Node ST_PlayConversationAnims(
        SmartCharacter user, 
        AnimationDescription[] myAnims, 
        AnimationDescription[] userAnims)
    {
        BehaviorMecanim myBehavior = 
            this.GetComponent<BehaviorMecanim>();
        BehaviorMecanim userBehavior =
            user.GetComponent<BehaviorMecanim>();

        List<Node> animations = new List<Node>();
        for (int i = 0; i < myAnims.Length; i++)
        {
            animations.Add(
                myBehavior.ST_PlayGesture(
                    myAnims[i].Name, myAnims[i].Layer, 1500));
            animations.Add(
                userBehavior.ST_PlayGesture(
                    userAnims[i].Name, userAnims[i].Layer, 1500));
        }

        return new Sequence(animations.ToArray());
    }

    /// <summary>
    /// Subtree for Give. Here holdingState should be the StateName corresponding
    /// to the state for holding the object exchanged, e.g. HoldingWallet, HoldingBall.
    /// beforeGive and afterGive can be used to specify actions to be done both before the give starts
    /// (but after they have met and oriented towards each other) and to be done after the give ends.
    /// </summary>
    protected Node ST_Give(SmartCharacter user, StateName holdingState, Node beforeGive, Node afterGive)
    {
        return new Sequence(
            user.Node_Require(StateName.RoleActor, StateName.IsStanding, holdingState, ~StateName.IsIncapacitated),
            this.Node_Require(StateName.RoleActor, StateName.IsStanding, ~holdingState, ~StateName.IsIncapacitated),
            new SequenceParallel(
                user.Node_GoToUpToRadius(new Val<Vector3>(this.transform.position), 1.0f),
                this.Node_OrientTowards(new Val<Vector3>(user.transform.position)),
            user.Node_OrientTowards(new Val<Vector3>(this.transform.position))),
            beforeGive,
            new SequenceParallel(
                user.Node_StartInteraction(FullBodyBipedEffector.RightHand, this.InteractionGive),
                new Sequence(
                    new LeafWait(1000),
                    this.Node_StartInteraction(FullBodyBipedEffector.RightHand, this.InteractionTake)),
                user.Node_WaitForTrigger(FullBodyBipedEffector.RightHand),
                this.Node_WaitForTrigger(FullBodyBipedEffector.RightHand)),
            new LeafInvoke(() => this.HoldPropRightHand.Attach(user.HoldPropRightHand.Release())),
            user.Node_Set(~holdingState),
            afterGive,
            this.Node_Set(holdingState),
            new SequenceParallel(
                user.Node_StopInteraction(FullBodyBipedEffector.RightHand),
                this.Node_StartInteraction(FullBodyBipedEffector.RightHand, this.InteractionHoldRight)));
    }

    /// <summary>
    /// Subtree for the character to put the wallet he is holding in his hand into its pocket.
    /// Does not change/check state.
    /// </summary>
    public Node ST_PutWalletInPocket()
    {
        return new Sequence(
            this.Node_StartInteraction(FullBodyBipedEffector.RightHand, this.InteractionStoreWallet),
            this.Node_WaitForTrigger(FullBodyBipedEffector.RightHand),
            new LeafInvoke(() => this.HoldPropPocket.Attach(this.HoldPropRightHand.Release())),
            this.Node_ResumeInteraction(FullBodyBipedEffector.RightHand));
    }
    
    /// <summary>
    /// Subtree for the character to take its wallet out of the pocket. Does not change/check state.
    /// </summary>
    /// <returns></returns>
    public Node ST_TakeWalletFromPocket()
    {
        return new Sequence(
            this.Node_StartInteraction(FullBodyBipedEffector.RightHand, this.InteractionStealWallet),
            this.Node_WaitForTrigger(FullBodyBipedEffector.RightHand),
            new LeafInvoke(() => this.HoldPropRightHand.Attach(this.HoldPropPocket.Release())));
    }

    /// <summary>
    /// Subtree to pickup an object from the given PropHolder, using the given InteractionObject
    /// for the IK. This subtree will not change state. Also, this subtree will not let the character
    /// approach the PropHolder, which should be done in another step.
    /// </summary>
    public Node ST_Pickup(PropHolder holder, InteractionObject pickup)
    {
        return new DecoratorCatch(
            () => { this.Character.StopInteraction(FullBodyBipedEffector.RightHand); this.Character.HeadLookStop(); },
            new Sequence(
                this.ST_OrientAndReach(holder.transform.position, pickup),
                new LeafInvoke(() => this.HoldPropRightHand.Attach(holder.Release())),
                this.Node_StopInteraction(FullBodyBipedEffector.RightHand),
                this.Node_HeadLookStop()));
    }

    /// <summary>
    /// Subtree to put an object to the given PropHolder, using the given InteractionObject
    /// for the IK. This subtree will not change state. Also, this subtree will not let the character
    /// approach the PropHolder, which should be done in another step.
    /// </summary>
    public Node ST_Put(PropHolder holder, InteractionObject put)
    {
        return new DecoratorCatch(
            () => { this.Character.StopInteraction(FullBodyBipedEffector.RightHand); this.Character.HeadLookStop(); },
            new Sequence(
                this.ST_OrientAndReach(holder.transform.position, put),
                new LeafInvoke(() => holder.Attach(this.HoldPropRightHand.Release())),
                this.Node_HeadLookStop()));
    }

    /// <summary>
    /// Subtree to drop and destroy the held object at the given InteractionObject. This subtree will not change state.
    /// Also, this subtree will not let the character approach the PropHolder, which should be done in another step.
    /// </summary>
    public Node ST_DropAndDestroy(InteractionObject drop)
    {
        return new DecoratorCatch(
            () => { this.Character.StopInteraction(FullBodyBipedEffector.RightHand); this.Character.HeadLookStop(); },
            new Sequence(
                this.ST_OrientAndReach(drop.transform.position, drop),
                new LeafInvoke(() => this.GetRightProp().FadeOut()),
                new LeafWait(500),
                new LeafInvoke(() => GameObject.Destroy(this.HoldPropRightHand.CurrentProp.gameObject)),
                this.Node_StopInteraction(FullBodyBipedEffector.RightHand),
                this.Node_HeadLookStop()));
    }

    /// <summary>
    /// Subtree for orientation/lookat towards the lookAndOrient position, and reaching with
    /// the right hand for the given interaction object.
    /// </summary>
    protected Node ST_OrientAndReach(Val<Vector3> lookAndOrient, InteractionObject interact)
    {
        return new Sequence(
            this.Node_StartInteraction(FullBodyBipedEffector.RightHand, interact),
            this.Node_WaitForTrigger(FullBodyBipedEffector.RightHand));
    }
    #endregion

    #region Behavior Nodes
    public Node Node_GoTo(Val<Vector3> position)
    {
        return this.behavior.Node_GoTo(position);
    }

    public Node Node_NudgeTo(Val<Vector3> position)
    {
        return this.behavior.Node_NudgeTo(position);
    }

    public Node Node_OrientTowards(Val<Vector3> position)
    {
        return this.behavior.Node_OrientTowards(position);
    }

    public Node Node_Orient(Val<Quaternion> direction)
    {
        return this.behavior.Node_Orient(direction);
    }

    public Node Node_HeadLook(Val<Vector3> position)
    {
        return this.behavior.Node_HeadLook(position);
    }

    public Node Node_HeadLookStop()
    {
        return this.behavior.Node_HeadLookStop();
    }

    public Node Node_PlayHandGesture(Val<string> name, Val<long> miliseconds)
    {
        return this.behavior.ST_PlayHandGesture(name, miliseconds);
    }

    public Node Node_StartInteraction(
        Val<FullBodyBipedEffector> effector,
        Val<InteractionObject> obj)
    {
        return this.behavior.Node_StartInteraction(effector, obj);
    }

    public Node Node_ResumeInteraction(
        Val<FullBodyBipedEffector> effector)
    {
        return this.behavior.Node_ResumeInteraction(effector);
    }

    public Node Node_StopInteraction(
        Val<FullBodyBipedEffector> effector)
    {
        return this.behavior.Node_StopInteraction(effector);
    }

    public Node Node_WaitForTrigger(
        Val<FullBodyBipedEffector> effector)
    {
        return this.behavior.Node_WaitForTrigger(effector);
    }

    public Node Node_WaitForFinish(
        Val<FullBodyBipedEffector> effector)
    {
        return this.behavior.Node_WaitForFinish(effector);
    }

    public Node Node_GoToUpToRadius(Val<Vector3> targ, Val<float> dist)
    {
        return this.behavior.Node_GoToUpToRadius(targ, dist);
    }

	/// <summary>
	/// ABAS addition
	/// </summary>
	/// <returns>The approach user.</returns>
	/// <param name="user">User.</param>
	public Node Node_Reprehend(SmartCharacter user) {
		return new Sequence(
			Node_GoToUpToRadius(Val.V (() => user.transform.position), 1.2f),
            Node_HeadLook(Val.V(() => user.MarkerHead.position)),
			Node_PlayHandGesture(Val<string>.V (() => "Dismiss"), Val<long>.V (() => 2000L)),
            Node_HeadLookStop()
            );
	}

    /// <summary>
    /// ABAS addition
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public Node Node_WaveTo(SmartCharacter user) {
        return new Sequence(
            Node_OrientTowards(Val<Vector3>.V(() => user.transform.position)),
            new SequenceParallel(
                Node_HeadLook(Val.V(() => user.MarkerHead.position)),
                Node_PlayHandGesture(Val<string>.V(() => "Wave"), Val<long>.V(() => 2000L))),
            Node_HeadLookStop()
            );
    }

    /// <summary>
    /// ABAS addition
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public Node Node_PointTo(SmartCharacter user) {
        return new Sequence(
            Node_HeadLook(Val.V(() => user.MarkerHead.position)),
            Node_PlayHandGesture(Val<string>.V(() => "CallOver"), Val<long>.V(() => 2000L)),
            Node_HeadLookStop()
            );
    }

	/// <summary>
	/// ABAS
	/// </summary>
	/// <returns>The get scared.</returns>
	/// <param name="user">User.</param>
	public Node Node_GetScared(SmartCharacter user) {
		return new SequenceParallel(
            Node_HeadLook(Val.V(() => user.MarkerHead.position)),
            Node_PlayHandGesture(Val.V(() => "Surprised"), Val.V(() => 2000L)),
            Node_HeadLookStop());
	}

    /// <summary>
    /// ABAS
    /// </summary>
    /// <returns>The get scared.</returns>
    /// <param name="user">User.</param>
    public Node Node_GetTerrified(SmartCharacter user) {
        return new SequenceParallel(
            Node_HeadLook(Val.V(() => user.MarkerHead.position)),
            Node_PlayHandGesture(Val.V(() => "Cry"), Val.V(() => 2000L)),
            Node_HeadLookStop());
    }

    /// <summary>
    /// ABAS
    /// </summary>
    /// <returns>The get scared.</returns>
    /// <param name="user">User.</param>
    public Node Node_Clap(SmartCharacter user) {
        return new Sequence(
            Node_OrientTowards(Val.V(() => user.transform.position)),
            Node_HeadLook(Val.V(() => user.MarkerHead.position)),
            Node_PlayHandGesture(Val.V(() => "Clap"), Val.V(() => 2000L)),
            Node_HeadLookStop()
            );
    }

    /// <summary>
    /// ABAS
    /// </summary>
    /// <returns>The get scared.</returns>
    /// <param name="user">User.</param>
    public Node Node_LoserTo(SmartCharacter user) {
        return new Sequence(
            Node_OrientTowards(Val.V(() => user.transform.position)),
            Node_HeadLook(Val.V(() => user.MarkerHead.position)),
            Node_PlayHandGesture(Val.V(() => "Loser"), Val.V(() => 2000L)),
            Node_HeadLookStop()
            );
    }

    #endregion
}
