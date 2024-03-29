﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA2;
using System;

public class Personaje : MonoBehaviour
{
    public enum PlayerInputs { MOVE, IDLE, CHASE }
    private EventFSM<PlayerInputs> _myFsm;
    private Rigidbody _myRb;
    public Renderer _myRen;

    [Header(" Variables de movimiento")]
    public Vector3 _velocity;
    public float waypointSpeed;
    public float maxForce;
    public bool rightWay;
    public bool provisionaryWaypointTurnaround;
    private int _currentWaypoint;

    [Header("Vector Desired")]
    public Vector3 _desired;

    [Header("Chase")]
    public float killBoidDistance; //Distancia en la cual mato al boid
    public Boid target; //El boid que voy a targetear
    public float chaseDistance; //La distancia en la cual lo voy a focusear
    public List<Vector3> _desiredVectors; //Una lista para guardar la posicion de el boid que voy a focusear
    public List<float> _magnitudes; //Una lista donde guardo las magnitudes (la magnitud del desired)
    public List<Boid> auxiliarList; //Lista auxiliar de boids
    public float minValue, minValueIndex; //Valor minimo de distancia para que focusee a ese, y el valor de su index en la lista
    public float futureTime;
    public GameObject futurePosObject; //Objeto futuro

    [Header("Idle state")]
    public float staminaBar;
    public bool isResting;


    public List<Transform> allWaypoints = new List<Transform>(); //Lista de waypoints en los cuales se va a mover la IA

    private void Awake()
    {
        //_myRb = gameObject.GetComponent<Rigidbody>();

        //PARTE 1: SETEO INICIAL

        //Creo los estados
        var idle = new State<PlayerInputs>("Idle");
        var moving = new State<PlayerInputs>("WaypointState");
        var chasing = new State<PlayerInputs>("Chase");

        //creo las transiciones
        StateConfigurer.Create(idle)
            .SetTransition(PlayerInputs.MOVE, moving)
            .SetTransition(PlayerInputs.CHASE, chasing)
            .Done(); //aplico y asigno

        StateConfigurer.Create(moving)
            .SetTransition(PlayerInputs.IDLE, idle)
            .SetTransition(PlayerInputs.CHASE, chasing)
            .Done();

        StateConfigurer.Create(chasing)
            .SetTransition(PlayerInputs.IDLE, idle)
            .SetTransition(PlayerInputs.MOVE, moving)
            .Done();

        //Empiezo a setear las variables como antes en la fsm comun

        idle.OnEnter += x =>
        {
            if (staminaBar <= 0) //Si la stamina es menor que 0 le activo para que descanse
                isResting = true;
            else //Si no, en el start directamente me voy al patrol 
                SendInputToFSM(PlayerInputs.MOVE);
        };


        //Parecido a la FSM ANTERIOR, en lugar de hacer el update alla hago aca
        //IDLE
        idle.OnUpdate += () => 
        {
            if (isResting && staminaBar <= 10) //una vez que me quedo sin stamina vengo aca, si se hizo true resting en el start
            {
                waypointSpeed = 0; //Le hago 0 la speed de movimiento
                staminaBar += 4 * Time.deltaTime; //Lo pongo a recargar energia

            }
            else //Si no estoy en resting, lo paso como false, le doy su speed, y me vuelvo a patrol
            {
                waypointSpeed = 4;
                isResting = false;
                SendInputToFSM(PlayerInputs.MOVE);
            }
        };



        //MOVING
        moving.OnEnter += x =>
        {
            Debug.Log("entre a fsm moving");
        };

        //En el update manejo lo que seria la stamina para ver cuanto se puede mover
        moving.OnUpdate += () => 
        {
            staminaBar -= Time.deltaTime * 0.5f; //Cuando patruya pierde stamina

            if (staminaBar <= 0) //Si llega a 0 de stamina va a Idle y recarga
                SendInputToFSM(PlayerInputs.IDLE);
        };


        moving.OnFixedUpdate += () => 
        {
            //Esto es el comportamiento de waypoints
            Vector3 dir = allWaypoints[_currentWaypoint].transform.position - transform.position;
            transform.forward = dir;
            transform.position += transform.forward * waypointSpeed * Time.deltaTime;

            if (dir.magnitude < 0.15f)
            {
                if (rightWay) //Hago un sentido correcto, y ejecuto los waypoints
                {
                    _currentWaypoint++;
                    if (_currentWaypoint > allWaypoints.Count - 1)
                    {
                        _currentWaypoint = 0;
                        rightWay = false;

                    }

                }
                else //Le quito el sentido correcto, como cambia al 0 le armor un bool y le digo
                {
                    if (_currentWaypoint == 0 &&provisionaryWaypointTurnaround) //Si sos 0 y acabas de cambiar
                    {
                        _currentWaypoint = 4; //Tu proximo waypoint es el count total +1, cosa que vaya al ultimo
                        provisionaryWaypointTurnaround = false; //Le hago false el turn around, asi aca solo entro cuando cambio
                    }
                    _currentWaypoint--;
                    if (_currentWaypoint < 0) //Cuando es menor que 0
                    {
                        _currentWaypoint = 1; //Lo hago focusear el primer waypoint
                       rightWay = true; //Cambio al sentido correcto
                       provisionaryWaypointTurnaround = true; //Le vuelvo a hacer true el primer caso de si es 0 y cambia, asi elige el ultimo waypoint

                    }

                }

            }
        };


        moving.OnExit += x => 
        {
            Debug.Log("sali");
        };


        //Action Poisoned = () => { };
        //float currentTime = 0;
        //Poisoned += () =>
        //{
        //    //Debug.Log("Poisoned");
        //    currentTime += Time.deltaTime;
        //    if (currentTime >= 5)
        //        idle.OnUpdate -= Poisoned;
        //};


        //idle.OnUpdate += Poisoned;




        //Dado que nuestras transiciones son una clase en si, le agregamos la funcionalidad de llamar a una accion al momento de hacerse esa transicion en si
        ////Esto es aparte del Exit de los estados!
        ////En este caso si pasamos de el estado "jumping" con el input PlayerInputs.JUMP se ejecuta esto
        //En cambio si estamos en "jumping" y se le pone el input de PlayerInputs.IDLE se ejecutaria esto
        idle.GetTransition(PlayerInputs.MOVE).OnTransition += x =>
        {
            Debug.Log("a moving");
        };

        //En cambio si estamos en "jumping" y se le pone el input de PlayerInputs.IDLE se ejecutaria esto
        moving.GetTransition(PlayerInputs.IDLE).OnTransition += x =>
        {
            Debug.Log("a idle");
        };



        //con todo ya creado, creo la FSM y le asigno el primer estado
        _myFsm = new EventFSM<PlayerInputs>(idle);
    }

    private void SendInputToFSM(PlayerInputs inp)
    {
        _myFsm.SendInput(inp);
    }

    private void Update()
    {
        _myFsm.Update();
    }

    private void FixedUpdate()
    {
        _myFsm.FixedUpdate();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //SendInputToFSM(PlayerInputs.IDLE);
    }
}
