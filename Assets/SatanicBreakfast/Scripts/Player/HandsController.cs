using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandsController : MonoBehaviour
{
    public enum State
	{
        IDLE,
        PICKUP,
        HELD,
	}

    [Header( "Variables" )]
    public float PICKUP_RADIUS = 0.5f;
    public float PICKUP_DISTANCE = 5;
    public float HandLerpSpeed = 5;

    [Header( "References" )]
    public Transform[] Hands;

    private Transform[] HeldPivots;
    private ItemBase[] CurrentHeld;
    private State[] HandState;
    private float[] HandTime;

    void Start()
    {
        HeldPivots = new Transform[Hands.Length];
        CurrentHeld = new ItemBase[Hands.Length];
        HandState = new State[Hands.Length];
        HandTime = new float[Hands.Length];

        // Store pivots to avoid lookup every frame :+1:
        for ( int i = 0; i < Hands.Length; i++ )
        {
            HeldPivots[i] = Hands[i].GetChild( 0 ).GetChild( 0 ).GetChild( 0 ).GetChild( 0 );
        }
    }

    void Update()
    {
        for ( int i = 1; i <= Hands.Length; i++ )
        {
            // Input - 1 = Left, 2 = Right
            int hand = i - 1;
            if ( Input.GetButtonDown( "Fire" + i ) )
            {
                TryPickup( hand );
            }
            else if ( !Input.GetButton( "Fire" + i ) )
            {
                TryDrop( hand );
            }

            // Updates
            UpdateState( hand );

            var auto = Hands[hand].GetComponentInChildren<Autohand.Hand>();
            if ( HandState[hand] != State.PICKUP )
            {
                auto.transform.localPosition = Vector3.zero;
                auto.transform.localEulerAngles = new Vector3( 0, 0, 90 );
            }
            //if ( CurrentHeld[hand] != null && HandState[hand] == State.PICKUP )
            //{
            //    var look = auto.transform.position - ( CurrentHeld[hand].transform.position - auto.transform.position );
            //    auto.transform.LookAt( look );
            //}
        }

        // temp
        //if ( Input.GetKey( KeyCode.K ) )
        //{
        //    FindObjectOfType<Autohand.Hand>().Grab();
        //}
        //else
        //{
        //    FindObjectOfType<Autohand.Hand>().Release();
        //}
    }

	#region Pickup/Drop
	void TryPickup( int hand )
    {
        // Shouldn't happen but here you go
        if ( CurrentHeld[hand] != null ) return;

        // Physics sphere cast forward
        Vector3 sphere = Player.Camera.transform.position + Player.Camera.transform.forward;
        Collider[] hitColliders = Physics.OverlapSphere( sphere, PICKUP_RADIUS * 10 );
        foreach ( var hit in hitColliders )
        {
            // If has ItemBase component
            var item = hit.transform.GetComponentInParent<ItemBase>();
            if ( item != null && !item.GetComponent<Autohand.Grabbable>().beingGrabbed )
            {
                CurrentHeld[hand] = item;

                SwitchState( hand, State.PICKUP );

                break;
            }
        }
    }

    void TryDrop( int hand )
	{
        if ( CurrentHeld[hand] != null )
		{
            // Raycast forward from camera pos to current item pos
            // If it can't reach the position, you can't drop (sorry)
            RaycastHit hit;
            Vector3 start = Player.Camera.transform.position - Player.Camera.transform.forward * 0.2f + Vector3.up * 0.2f;
            Vector3 end = CurrentHeld[hand].transform.position + Player.Camera.transform.forward * CurrentHeld[hand].Radius;
            Vector3 dir = ( end - start ).normalized;
            float dist = Vector3.Distance( start, end );
            int layer = ~((1 << LayerMask.NameToLayer( "HandDisplay" )) | (1 << LayerMask.NameToLayer( "Player" ))| (1 << LayerMask.NameToLayer( "Hand" )));
            bool drop = !Physics.SphereCast( start, CurrentHeld[hand].Radius, dir, out hit, dist, layer );
			if ( drop )
            {
                CurrentHeld[hand].OnDrop();

                CurrentHeld[hand].transform.SetParent( null );
				foreach ( var collider in CurrentHeld[hand].GetComponentsInChildren<Collider>() )
                {
                    collider.gameObject.layer = LayerMask.NameToLayer( "Item" );
                }

                var body = CurrentHeld[hand].GetComponent<Rigidbody>();
                body.isKinematic = false;

                CurrentHeld[hand] = null;

                SwitchState( hand, State.IDLE );
            }
        }
	}
	#endregion

	#region States
    void SwitchState( int hand, State state )
	{
        FinishState( hand, HandState[hand] );
        SetState( hand, state );
	}

    void SetState( int hand, State state )
	{
        HandState[hand] = state;
        StartState( hand, state );
	}

    void StartState( int hand, State state )
    {
        switch ( HandState[hand] )
        {
            case State.IDLE:
                Hands[hand].GetComponentInChildren<Autohand.Hand>().Release();
                break;
            case State.PICKUP:
                HandTime[hand] = 0;
                break;
            case State.HELD:
                HandTime[hand] = 0;
                //CurrentHeld[hand].transform.SetParent( HeldPivots[hand] );
                //CurrentHeld[hand].transform.localPosition = Vector3.zero;
                CurrentHeld[hand].transform.position += Vector3.up * CurrentHeld[hand].GrabUpOffset;

                //Hands[hand].GetComponentInChildren<Autohand.Hand>().TryGrab( CurrentHeld[hand].GetComponentInChildren<Autohand.Grabbable>() );

                //CurrentHeld[hand].gameObject.layer = LayerMask.NameToLayer( "HandDisplay" );
                foreach ( var collider in CurrentHeld[hand].GetComponentsInChildren<Collider>() )
                {
                    collider.gameObject.layer = LayerMask.NameToLayer( "HandDisplay" );
                }

                break;
            default:
                break;
        }
    }

    void UpdateState( int hand )
    {
        Vector3 target;
        var trans = Hands[hand].GetChild( 0 ).GetChild( 0 );
        float dist = 0;

        switch ( HandState[hand] )
		{
			case State.IDLE:
                HandTime[hand] = Mathf.Min( 1, HandTime[hand] + Time.deltaTime * HandLerpSpeed );

                // Move back to normal
                // Find correct position via HeldPivot at 0,0,0
                target = new Vector3( 0, -0.19f, -0.74f );
                trans.localPosition = Vector3.Lerp( trans.localPosition, target, HandTime[hand] );

                break;
			case State.PICKUP:
                HandTime[hand] = Mathf.Min( 1, HandTime[hand] + Time.deltaTime * HandLerpSpeed );

                // Move towards
                // Find correct position via HeldPivot at 0,0,0
                target = CurrentHeld[hand].transform.position;// + ( Hands[hand].position );// - HeldPivots[hand].position );
                    target += Player.Camera.transform.right * Mathf.Cos( ( 1 - HandTime[hand] ) * 180 * 2 ) * 0.5f;
                trans = Hands[hand].GetChild( 0 ).GetChild( 0 );
                trans.position = Vector3.Lerp( trans.position, target, HandTime[hand] );

                dist = Vector3.Distance( trans.position, target );
                if ( dist < 0.1f )
                {
                    var body = CurrentHeld[hand].GetComponent<Rigidbody>();
                    body.isKinematic = true;

                    var grab = CurrentHeld[hand].GetComponentInChildren<Autohand.Grabbable>();
                    Hands[hand].GetComponentInChildren<Autohand.Hand>().TryGrab( grab );
                    if ( grab.IsHeld() )
                    {
                        SwitchState( hand, State.HELD );
                    }
                }

                break;
			case State.HELD:
                HandTime[hand] = Mathf.Min( 1, HandTime[hand] + Time.deltaTime * HandLerpSpeed );

                // Move back to normal
                // Find correct position via HeldPivot at 0,0,0
                target = new Vector3( 0, -0.19f, -0.74f );
                trans.localPosition = Vector3.Lerp( trans.localPosition, target, HandTime[hand] );

                break;
			default:
				break;
		}
	}

    void FinishState( int hand, State state )
    {
        switch ( HandState[hand] )
        {
            case State.IDLE:
                break;
            case State.PICKUP:
                break;
            case State.HELD:
                break;
            default:
                break;
        }
    }
    #endregion
}
