using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : MonoBehaviour, IState
{


    StateMachine _fms;
    Hunter _hunter;
    bool isResting;

    public IdleState(StateMachine fms, Hunter h)
    {
        _fms = fms;
        _hunter = h;
    }



    public void OnExit()
    {

    }

    public void OnStart()
    {

        if (_hunter.staminaBar <= 0) //Si la stamina es menor que 0 le activo para que descanse
            isResting = true;
        else //Si no, en el start directamente me voy al patrol 
            _fms.ChangeState(PlayerStatesEnum.Patrol);


    }

    public void OnUpdate()
    {

        if (isResting && _hunter.staminaBar <=10) //una vez que me quedo sin stamina vengo aca, si se hizo true resting en el start
        {
            _hunter.waypointSpeed = 0; //Le hago 0 la speed de movimiento
            _hunter.staminaBar += 4*Time.deltaTime; //Lo pongo a recargar energia

        }
        else //Si no estoy en resting, lo paso como false, le doy su speed, y me vuelvo a patrol
        {
            _hunter.waypointSpeed = 4;
            isResting = false;
            _fms.ChangeState(PlayerStatesEnum.Patrol);
        }
    }
}
