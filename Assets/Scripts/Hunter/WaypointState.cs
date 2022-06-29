using System.Collections.Generic;
using UnityEngine;

public class WaypointState : MonoBehaviour, IState
{
    private int _currentWaypoint = 0;
    
    Hunter _hunter;
    StateMachine _fsm;


    public WaypointState(StateMachine fsm, Hunter h) //Constreuctor state machine
    {
        _fsm = fsm;
        _hunter = h;
    }


    public void OnExit()
    {
        
    }

    public void OnStart()
    {
        _hunter._desiredVectors = new List<Vector3>();
        _hunter._magnitudes = new List<float>();
        _hunter.auxiliarList = new List<Boid>();



        foreach (var boid in BoidManager.instance.allBoids) //Relleno las listas auxiliares para saber a que boid targetear
        {
            _hunter._desiredVectors.Add(boid.transform.position);
            _hunter._magnitudes.Add(_currentWaypoint);
            _hunter.auxiliarList.Add(boid);
        }

    }
    
    public void OnUpdate() //OnUpdate arranco con esto
    {

        _hunter.staminaBar -= Time.deltaTime * 0.5f; //Cuando patruya pierde stamina

        if (_hunter.staminaBar <= 0) //Si llega a 0 de stamina va a Idle y recarga
            _fsm.ChangeState(PlayerStatesEnum.Idle);

        #region Comportamiento waypoints

        //Esto es el comportamiento de waypoints
        Vector3 dir = _hunter.allWaypoints[_currentWaypoint].transform.position - _hunter.transform.position;
        _hunter.transform.forward = dir;
        _hunter.transform.position += _hunter.transform.forward * _hunter.waypointSpeed * Time.deltaTime;

        if (dir.magnitude < 0.15f)
        {
            if (_hunter.rightWay) //Hago un sentido correcto, y ejecuto los waypoints
            {
                _currentWaypoint++;
                if (_currentWaypoint > _hunter.allWaypoints.Count - 1)
                {
                    _currentWaypoint = 0;
                    _hunter.rightWay = false;
                    
                }
                    
            }
            else //Le quito el sentido correcto, como cambia al 0 le armor un bool y le digo
            {
                if (_currentWaypoint == 0 && _hunter.provisionaryWaypointTurnaround) //Si sos 0 y acabas de cambiar
                {
                    _currentWaypoint = 4; //Tu proximo waypoint es el count total +1, cosa que vaya al ultimo
                    _hunter.provisionaryWaypointTurnaround = false; //Le hago false el turn around, asi aca solo entro cuando cambio
                }
                _currentWaypoint--;
                if (_currentWaypoint < 0) //Cuando es menor que 0
                {
                    _currentWaypoint = 1; //Lo hago focusear el primer waypoint
                    _hunter.rightWay = true; //Cambio al sentido correcto
                    _hunter.provisionaryWaypointTurnaround = true; //Le vuelvo a hacer true el primer caso de si es 0 y cambia, asi elige el ultimo waypoint

                }

            }

        }

        #endregion

        #region Buscador de Boids
        //Esto es el comportamiento para poder invocar a chase
        for (int i = 0; i < _hunter.auxiliarList.Count; i++) //Armo un vector desired entre mi hunter y todos los boids
        {

            _hunter._desiredVectors[i] = _hunter.auxiliarList[i].transform.position - _hunter.transform.position;
            _hunter._magnitudes[i] = _hunter._desiredVectors[i].magnitude; //Saco las magnitud del desired
        }


        Min(); //Busco el de menor magnitud


        if (_hunter.minValue < _hunter.chaseDistance) //Aca miro si estoy en chase distance, de estarlo, voy derecho a chase, y le ASIGNO un target a mi hunter, en este caso, el que mas cerca este
        {
            _hunter.target = _hunter.auxiliarList[(int)_hunter.minValueIndex]; //Aca le asigno el target, como el que este en la lista auxiliar, y tenga el indice que tiene menor magnitud de desired
            _fsm.ChangeState(PlayerStatesEnum.Chase); /// Voy hacia chase

        }

        #endregion

    }
    public float Min() //Busco el valor minimo de magnitud y su respectivo indice dentro de la lista
    {
        _hunter.minValue = _hunter._magnitudes[0];
        _hunter.minValueIndex = 0;

        for (int i = 0; i < _hunter._magnitudes.Count; i++)
        {
            float number = _hunter._magnitudes[i];

            if (number < _hunter.minValue)
            {
                _hunter.minValue = number;
                _hunter.minValueIndex = i;
            }
        }

        return _hunter.minValue;
    }

}
