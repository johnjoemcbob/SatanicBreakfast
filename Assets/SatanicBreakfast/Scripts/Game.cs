using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
	#region Structures
	public enum State
	{
        INIT,
        MENU,
        PLAY,
	}
	#endregion

	#region Variables - Private
	private State CurrentState;
	#endregion

	#region MonoBehaviour
	void Start()
    {
		SetState( State.INIT );
    }

    void Update()
    {
		UpdateState( CurrentState );
    }
	#endregion

	#region States
	public void SwitchState( State state )
	{
        FinishState( CurrentState );
        SetState( state );
	}

    void SetState( State state )
	{
        CurrentState = state;
        StartState( CurrentState );
	}

    void StartState( State state )
	{
		switch ( state )
		{
			case State.INIT:
				break;
			case State.MENU:
				break;
			case State.PLAY:
				break;
			default:
				break;
		}
	}

    void UpdateState( State state )
	{
		switch ( state )
		{
			case State.INIT:
				break;
			case State.MENU:
				break;
			case State.PLAY:
				break;
			default:
				break;
		}
	}

    void FinishState( State state )
	{
		switch ( state )
		{
			case State.INIT:
				break;
			case State.MENU:
				break;
			case State.PLAY:
				break;
			default:
				break;
		}
	}
	#endregion
}
