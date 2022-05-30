using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandsController : MonoBehaviour
{
    [Header( "Variables" )]
    public float PICKUP_RADIUS = 0.5f;
    public float PICKUP_DISTANCE = 5;
    public float DROP_RADIUS = 0.5f;

    [Header( "References" )]
    public Transform[] Hands;

    private ItemBase[] CurrentHeld;

    void Start()
    {
        CurrentHeld = new ItemBase[Hands.Length];
    }

    void Update()
    {
        // 1 = Left, 2 = Right
        for ( int i = 1; i <= Hands.Length; i++ )
        {
            int hand = i - 1;
            if ( Input.GetButtonDown( "Fire" + i ) )
            {
                TryPickup( hand );
            }
            else if ( !Input.GetButton( "Fire" + i ) )
            {
                TryDrop( hand );
            }
        }
    }

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
            if ( item != null )
            {
                CurrentHeld[hand] = item;

                // Pickup code
                var body = CurrentHeld[hand].GetComponent<Rigidbody>();
                body.isKinematic = true;

                CurrentHeld[hand].transform.SetParent( Hands[hand].GetChild( 0 ).GetChild( 0 ).GetChild( 0 ) );
                CurrentHeld[hand].transform.localPosition = Vector3.zero;

                CurrentHeld[hand].gameObject.layer = LayerMask.NameToLayer( "HandDisplay" );
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
            Vector3 start = Player.Camera.transform.position - Player.Camera.transform.forward * 1;
            Vector3 end = CurrentHeld[hand].transform.position;
            Vector3 dir = ( start - end ).normalized;
            float dist = Vector3.Distance( start, end );
            bool drop = !Physics.SphereCast( start, DROP_RADIUS, dir, out hit, dist );
            if ( !drop )
            {
                // If it hit itself, its droppable
                if ( hit.transform.GetComponentInParent<ItemBase>() == CurrentHeld[hand] )
                {
                    drop = true;
                }
            }
            if ( drop )
			{
                CurrentHeld[hand].OnDrop();

                var body = CurrentHeld[hand].GetComponent<Rigidbody>();
                body.isKinematic = false;

                CurrentHeld[hand].transform.SetParent( null );

                CurrentHeld[hand].gameObject.layer = LayerMask.NameToLayer( "Item" );

                CurrentHeld[hand] = null;
            }
        }
	}
}
