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
    public float DROP_RADIUS = 0.5f;
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
        }
    }

	#region Pickup/Drop
	void TryPickup( int hand )
    {
        // Shouldn't happen but here you go
        if ( CurrentHeld[hand] != null ) return;

        // Physics sphere cast forward
        RaycastHit hit;
        if ( Physics.SphereCast( Player.Camera.transform.position, PICKUP_RADIUS, Player.Camera.transform.forward, out hit, PICKUP_DISTANCE ) )
        {
            // If has ItemBase component
            var item = hit.transform.GetComponentInParent<ItemBase>();
            if ( item != null && !item.Held )
            {
                CurrentHeld[hand] = item;
                CurrentHeld[hand].Held = true;

                // Pickup code
                var body = CurrentHeld[hand].GetComponent<Rigidbody>();
                body.isKinematic = true;

                CurrentHeld[hand].gameObject.layer = LayerMask.NameToLayer( "HandDisplay" );

                SwitchState( hand, State.PICKUP );
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
            Vector3 end = CurrentHeld[hand].transform.position + Player.Camera.transform.forward * DROP_RADIUS;
            Vector3 dir = ( end - start ).normalized;
            float dist = Vector3.Distance( start, end );
            int layer = ~((1 << LayerMask.NameToLayer( "HandDisplay" )) | (1 << LayerMask.NameToLayer( "Player" )));
            bool drop = !Physics.SphereCast( start, DROP_RADIUS, dir, out hit, dist, layer );
			if ( drop )
            {
                CurrentHeld[hand].OnDrop();

                var body = CurrentHeld[hand].GetComponent<Rigidbody>();
                body.isKinematic = false;

                CurrentHeld[hand].transform.SetParent( null );
                CurrentHeld[hand].gameObject.layer = LayerMask.NameToLayer( "Item" );

                CurrentHeld[hand].Held = false;
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
                break;
            case State.PICKUP:
                HandTime[hand] = 0;
                break;
            case State.HELD:
                HandTime[hand] = 0;
                CurrentHeld[hand].transform.SetParent( HeldPivots[hand] );
                CurrentHeld[hand].transform.localPosition = Vector3.zero;

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
                target = CurrentHeld[hand].transform.position + ( Hands[hand].position - HeldPivots[hand].position );
                    target += Player.Camera.transform.right * Mathf.Cos( ( 1 - HandTime[hand] ) * 180 * 2 ) * 0.5f;
                trans = Hands[hand].GetChild( 0 ).GetChild( 0 );
                trans.position = Vector3.Lerp( trans.position, target, HandTime[hand] );

                dist = Vector3.Distance( trans.position, target );
                if ( dist < 0.1f )
                {
                    // Switch state on arrival
                    SwitchState( hand, State.HELD );
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
