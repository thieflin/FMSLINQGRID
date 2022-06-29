using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hunter : MonoBehaviour
{
    [Header(" Variables de movimiento")]
    public Vector3 _velocity;
    public float waypointSpeed;
    public float maxForce;
    public bool rightWay;
    public bool provisionaryWaypointTurnaround;

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
    

    public Vector3 GetVelocity()
    {
        return _velocity;
    }


    public void ApplyForce(Vector3 force) //Funcion apply force
    {
        _velocity += force;
    }


    public List<Transform> allWaypoints = new List<Transform>(); //Lista de waypoints en los cuales se va a mover la IA
    private StateMachine _fsm;


    void Start()
    {
        _desiredVectors = new List<Vector3>();
        _magnitudes = new List<float>();
        auxiliarList = new List<Boid>();
        rightWay = true;
        provisionaryWaypointTurnaround = true;
        _fsm = new StateMachine();

        

        _fsm.AddState(PlayerStatesEnum.Patrol, new WaypointState(_fsm, this)); //Agrego todos los estados
        _fsm.AddState(PlayerStatesEnum.Idle, new IdleState(_fsm, this));
        _fsm.AddState(PlayerStatesEnum.Chase, new ChaseState(_fsm, this));
        _fsm.ChangeState(PlayerStatesEnum.Idle); //Lo hago arrancar con Idle
        _fsm.OnStart(); //Starteo la FSM

    }

    void Update()
    {
        _fsm.OnUpdate();
    }

    

}
