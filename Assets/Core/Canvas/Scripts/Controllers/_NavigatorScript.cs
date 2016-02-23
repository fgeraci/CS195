using UnityEngine;
using TreeSharpPlus;
using System.Collections;

public class _NavigatorScript : MonoBehaviour
{	
	private NavMeshAgent agent;
	private Animator animator;
    public GameObject gameBall;
	private float angleDiff;
    public bool IsInitialized;
    public Transform lefthand;
    public bool HasHandshaked;
    public bool Tackle;
    public bool IsHoldingBall;
    public Ball_Script ballscript;
    public enum TEAM { RED = 0, BLUE = 1 };
    public TEAM team;
    public Material redTeam;
    public Material blueTeam;


    [HideInInspector]
	public Quaternion desiredOrientation{ get; set; }

	/*put together with locomotion*/
	protected LocomotionController locomotion;


	void Start() { this.Initialize(); }

	public void Initialize() 
	{
        ballscript = gameBall.GetComponent<Ball_Script>();
		agent = this.GetComponent<NavMeshAgent> ();
		animator = this.GetComponent<Animator> ();
        desiredOrientation = transform.rotation;
        IsInitialized = false;
        HasHandshaked = false;
		/*put together with locomotion*/
		locomotion = new LocomotionController(animator);
        ColorByTeam();
    }

    private void ColorByTeam()
    {
        for (int i = 0; i<gameObject.transform.GetChildCount(); i++)
        {
            Transform child = gameObject.transform.GetChild(i);
            if (child.name.Equals("Tops") || child.name.Equals("Bottoms"))
            {
                switch (team)
                {
                    case TEAM.RED:
                        child.GetComponent<Renderer>().material = redTeam;
                        break;
                    case TEAM.BLUE:
                        child.GetComponent<Renderer>().material = blueTeam;
                        break;
                }
            }
        }
    }

    protected void SetupAgentLocomotion()
    {
        /*if (AgentDone())
        {
            //TODO Resetting path here, otherwise e.g. stepback animation not working properly - CS 03.09.2014
            //Is there a better solution?
            agent.ResetPath();
            locomotion.Do(0, angleDiff);
        }
        else
        {*/
            float speed = agent.desiredVelocity.magnitude;

			Vector3 velocity = Quaternion.Inverse(transform.rotation) * agent.desiredVelocity;

            float angle = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / 3.14159f;

            locomotion.Do(speed, angle);
        //}
    }

    void OnAnimatorMove()
    {
        agent.velocity = animator.deltaPosition / Time.deltaTime;
        transform.rotation = animator.rootRotation;
        // get a "forward vector" for each rotation
        var forwardA = transform.rotation * Vector3.forward;
        var forwardB = desiredOrientation * Vector3.forward;
        // get a numeric angle for each vector, on the X-Z plane (relative to world forward)
        var angleA = Mathf.Atan2(forwardA.x, forwardA.z) * Mathf.Rad2Deg;
        var angleB = Mathf.Atan2(forwardB.x, forwardB.z) * Mathf.Rad2Deg;
        // get the signed difference in these angles
        angleDiff = Mathf.DeltaAngle(angleA, angleB);
    }

	public bool AgentDone()
	{
		return !agent.pathPending && AgentStopping();
	}

    protected bool AgentStopping()
    {
        return agent.remainingDistance <= agent.stoppingDistance;
    }
	// Update is called once per frame
	void Update () 
	{
        SetupAgentLocomotion();
        
        if(IsInitialized == false)
        { 
            agent.SetDestination(new Vector3(0,0,0)); // walk to center to handshake
            if(agent.transform.position == agent.destination)
            {
				//wait for second guy to come here
                IsInitialized = true;
            }
            else
            {
                return;
            }
        }
        if(HasHandshaked == false)
        {
                //code here for affordance
                HasHandshaked = true;       
        }        
        if(ballscript.isPickedUp == false)
        {
            ResetBools();
            agent.SetDestination(gameBall.transform.position);
			Vector3 distance = agent.transform.position - agent.destination;
			if(distance.magnitude<1)
            {
                animator.SetTrigger("B_PickupLeft");
                gameBall.GetComponent<Rigidbody>().isKinematic = true;            
                ballscript.isPickedUp = true;
                gameBall.transform.parent = lefthand;
                gameBall.transform.position = lefthand.position;
                IsHoldingBall = true;
            }
            return;
        }
        if(IsHoldingBall == true)
        {
            agent.SetDestination(new Vector3(-18f, 0.2f, agent.transform.position.z)); // the goal
            if(agent.transform.position == agent.destination)
            {
                animator.SetBool("B_Breakdance", true);
                //end game
                return;
            }
        }
        else
        {
            agent.speed = 3.5f;
            if(true)//whatever bool is checked to end game
            {
                //cry
                //end game
            }
            //Tackle = true;
			agent.SetDestination(gameBall.transform.position);
			Vector3 threshold = agent.transform.position - gameBall.transform.position;
			if(threshold.magnitude<1)
            {
                //tackle
				animator.SetTrigger("H_Tackle");
				Debug.Log ("Tackle!");
				gameBall.GetComponent<Rigidbody>().isKinematic = false;
				gameBall.transform.parent = null;
				ballscript.isPickedUp = false;
                
                return;
            }
        }        
	}
    void ResetBools()
    {
        IsHoldingBall = false;
        Tackle = false;
        agent.speed = 2.2f;
        return;
    }
}
