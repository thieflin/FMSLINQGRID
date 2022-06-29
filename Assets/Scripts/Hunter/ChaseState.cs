using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : IState
{


    Hunter _hunter;
    StateMachine _fsm;


    public GameObject target;

    private Vector3 _velocity;

    

    public ChaseState(StateMachine fsm, Hunter h) //Constreuctor state machine
    {
        _fsm = fsm;
        _hunter = h;
    }

    public void OnExit()
    {
        
    }

    public void OnStart()
    {
        
    }

    public void OnUpdate()
    {
        _hunter.staminaBar -= Time.deltaTime; //Perdida de Stamina, cuadno chasea pierde una cantidad

        if (_hunter.staminaBar <= 0) //Si chaseando se queda en 0, se cambia a Idle
            _fsm.ChangeState(PlayerStatesEnum.Idle);


        _hunter.transform.position += _velocity * Time.deltaTime;
        _hunter.transform.forward = _velocity;
        Pursuit();


    }

    public void ApplyForce(Vector3 force)
    {
        _velocity += force;
    }

    void Pursuit() //Aplico pursuit
    {
        //Vector3 desired = _hunter.target.transform.position - _hunter.transform.position;

        Boid tarAgent = _hunter.target.GetComponent<Boid>();
        Vector3 futurePos = _hunter.target.transform.position + tarAgent.GetVelocity() * _hunter.futureTime;// * Time.deltaTime;
        _hunter.futurePosObject.transform.position = futurePos;

        Vector3 dist = _hunter.target.transform.position - _hunter.transform.position;

        Vector3 desired;

        if (dist.magnitude < futurePos.magnitude) // o un radio x
            desired = dist;
        else desired = futurePos - _hunter.transform.position;

        if (desired.magnitude < _hunter.chaseDistance)
        {

            if (desired.magnitude < _hunter.killBoidDistance) //Si estoy dentro de mi kill range
            {
                _hunter.target.gameObject.SetActive(false); //Desacctivo el Boid asi se apaga
                _hunter.auxiliarList.Remove(_hunter.target); //Lo elimino de mi lista auxiliar
                desired = Vector3.zero; //Hago el desired nulo asi elimino pursuit del comportamiento
                BoidManager.instance.allBoids.Remove(_hunter.target); //Elimino al boid de mi lista de boids
                _fsm.ChangeState(PlayerStatesEnum.Patrol); //Vuelvo a patrullar
               
                    
            }
                
            desired.Normalize();
            desired *= _hunter.waypointSpeed;

            Vector3 steering = desired - _velocity;
            steering = Vector3.ClampMagnitude(steering, _hunter.maxForce);

            ApplyForce(steering);
        }
        else //Si salgo de el rango del chase, vuelvo a patrol
        {
            _velocity = Vector3.zero; //Hago velocity cero asi remuevo el comportamiento de steering

            _fsm.ChangeState(PlayerStatesEnum.Patrol); //Vuelvo a patrol
        }
        

    }




}
