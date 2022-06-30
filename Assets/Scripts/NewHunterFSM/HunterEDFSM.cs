using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA2;
using System.Linq;
using System;

public class HunterEDFSM : MonoBehaviour
{
    public enum PlayerInputs { MOVE, IDLE, CHASE, ATTACK, REST, CONQUER }
    private EventFSM<PlayerInputs> _myFsm;
    public Renderer _myRen;

    [Header(" Variables de movimiento")]
    public float waypointSpeed;

    private int _currentWaypoint;

    [Header("Chase")]
 
    public Boid target; //El boid que voy a targetear

    public float chaseDistance; //La distancia en la cual lo voy a focusear
    public float attackDistance; //La distancia en la que vuelvo a patrol si no lo encuentro

    public List<Boid> myEnemyBoids = new List<Boid>();
    public WaypointsSafety wpSafety;

    public BoidManager bm;

    public List<Tuple<Vector3, float, Boid>> boids = new List<Tuple<Vector3, float, Boid>>();//Lista de tuplas para saber a que boid sigo


    [Header("Idle state")]
    public float staminaBar;
    public bool isResting; //Stamina bar


    [Header("IdleState")]
    public float restingTime; //Descanso luego de comer

    [Header("StatusMaterial")]
    public Material matIdle;
    public Material matChase;
    public Material matPatrol;
    public Material matRestingAfterAttack;
    public Material matConquer;


    public List<Transform> allWaypoints = new List<Transform>(); //Lista de waypoints en los cuales se va a mover la IA
    public List<Tuple<Waypoint, bool>> safeWaypoints = new List<Tuple<Waypoint, bool>>();

    private void Awake()
    {
        //_myRb = gameObject.GetComponent<Rigidbody>();
        _myRen = GetComponent<Renderer>();
        //PARTE 1: SETEO INICIAL

        //Creo los estados
        var idle = new State<PlayerInputs>("Idle");
        var moving = new State<PlayerInputs>("WaypointState");
        var chasing = new State<PlayerInputs>("Chase");
        var attacking = new State<PlayerInputs>("Attack");
        var resting = new State<PlayerInputs>("Resting");
        var conquered = new State<PlayerInputs>("Conquer");

        //creo las transiciones
        //Idle puede ir a moverse
        StateConfigurer.Create(idle)
            .SetTransition(PlayerInputs.MOVE, moving)
            .Done(); //aplico y asigno

        //Moving puede ir a idle si se cansa o a chase si puede atacar
        StateConfigurer.Create(moving)
            .SetTransition(PlayerInputs.IDLE, idle)
            .SetTransition(PlayerInputs.CHASE, chasing)
            .Done();

        //Chase puede ir a idle si se cansa, a moving si pierde el target o a attacking si esta en rango
        StateConfigurer.Create(chasing)
            .SetTransition(PlayerInputs.IDLE, idle)
            .SetTransition(PlayerInputs.MOVE, moving)
            .SetTransition(PlayerInputs.ATTACK, attacking)
            .Done();

        //Despues de attacar va a resting si o si para descansar un tiempo
        StateConfigurer.Create(attacking)
            .SetTransition(PlayerInputs.REST, resting)
            .Done();

        //Luego de descansar vuelve directamente a patrullar
        StateConfigurer.Create(resting)
            .SetTransition(PlayerInputs.MOVE, moving)
            .SetTransition(PlayerInputs.CONQUER, conquered)
            .Done();

        //Una vez que los agarra a todos, de conquer pasa a resting
        StateConfigurer.Create(conquered)
            .SetTransition(PlayerInputs.REST, resting)
            .Done();

        //Empiezo a setear las variables como antes en la fsm comun

        idle.OnEnter += x =>
        {
            _myRen.material = matIdle;

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
            _myRen.material = matPatrol;
            target = null;

            //Seteo las listas de boids
            myEnemyBoids = EnemyBoids(bm.allBoids).ToList();
            Debug.Log("entre a fsm moving");
        };

        //En el update manejo lo que seria la stamina para ver cuanto se puede mover
        moving.OnUpdate += () =>
        {



            staminaBar -= Time.deltaTime * 0.5f; //Cuando patruya pierde stamina

            if (staminaBar <= 0) //Si llega a 0 de stamina va a Idle y recarga
                SendInputToFSM(PlayerInputs.IDLE);

            //Busco de todos los boids el que me sirve
            boids = BoidSearcher(myEnemyBoids).ToList();


            var closestBoid = boids.FirstOrDefault(); //Elijo el boid que mas cerca mio esta


            //Debug.Log(closestBoid.Item2);
            //Si la lista de boids cercanos esta vacia, no ingreso mas aca

            if (closestBoid.Item2 < chaseDistance) //Aca miro si estoy en chase distance, de estarlo, voy derecho a chase, y le ASIGNO un target a mi hunter, en este caso, el que mas cerca este
            {
                target = closestBoid.Item3; //Hago que el target sea el boid (current previo seleccionado que cumpla con la condicion)
                SendInputToFSM(PlayerInputs.CHASE);

            }

        };


        moving.OnFixedUpdate += () =>
        {
            //Esto es el comportamiento de waypoints
            Vector3 dir = safeWaypoints[_currentWaypoint].Item1.transform.position - transform.position;
            transform.forward = dir;
            transform.position += transform.forward * waypointSpeed * Time.deltaTime;

            if (dir.magnitude < 0.15f)
            {
                _currentWaypoint++;
                if (_currentWaypoint > allWaypoints.Count - 1)
                {
                    _currentWaypoint = 0;

                }
            }
        };


        moving.OnExit += x =>
        {
            Debug.Log("sali");
        };

        chasing.OnEnter += x =>
        {
            _myRen.material = matChase;
            Debug.Log("entre a chase");
        };

        chasing.OnUpdate += () =>
        {
            Debug.Log("tuki");

            staminaBar -= Time.deltaTime;

            //Si la stamina llega a 0 se vuelve a idle a descansar (podria hacer un estado de rest honestamente)
            if (staminaBar < 0)
                SendInputToFSM(PlayerInputs.IDLE);


            Vector3 dir = target.transform.position - transform.position;
            transform.forward = dir;
            //Si es menor a la chase distance, entonces lo chasea
            if (dir.magnitude < chaseDistance)
            {
                transform.position += transform.forward * waypointSpeed * 1.2f * Time.deltaTime;

                //Si estoy en condicion de atacarlo, entonces hago que el target cambie su color, vuelve a move
                if (dir.magnitude < attackDistance)
                {
                    SendInputToFSM(PlayerInputs.ATTACK);
                }

            }
            //Sino directamnete lo esquiva
            else
                SendInputToFSM(PlayerInputs.MOVE);

        };

        chasing.OnExit += x =>
        {
            Debug.Log("sali de aki");
        };


        attacking.OnEnter += x =>
        {
            target.rend.material = target.matAlly;
            target.isEnemy = false;
            SendInputToFSM(PlayerInputs.REST);
        };

        attacking.OnUpdate += () =>
        {
            Debug.Log("en attacking");
        };


        attacking.OnExit += x =>
        {
            //Cada vez que salgo de comerme a uno actualizo la lista de posibles targets
            myEnemyBoids = EnemyBoids(bm.allBoids).ToList();
            Debug.Log("sali de attack");
        };


        resting.OnEnter += x =>
        {
            _myRen.material = matRestingAfterAttack;
            restingTime = 5;
            Debug.Log("ingrese a descansar despues de comer exitosamente");
        };

        resting.OnUpdate += () =>
        {
            restingTime -= Time.deltaTime;
            //Una vez que termina de descansar, sale a pistear
            if (restingTime <= 0)
                SendInputToFSM(PlayerInputs.MOVE);

            if (myEnemyBoids.Count() == 0)
                SendInputToFSM(PlayerInputs.CONQUER);
        };

        resting.OnExit += x =>
        {
            Debug.Log("sali");
        };

        conquered.OnEnter += x =>
        {
            _myRen.material = matConquer;
        };

        conquered.OnUpdate += () =>
        {
            Debug.Log("TRIUNFO EL PODER DE LA NUEVA FSM");
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


    //Armo la lista de enemigos
    public void Start()
    {

        safeWaypoints = SafeWaypoint(wpSafety).ToList();
    }



    private void SendInputToFSM(PlayerInputs inp)
    {
        _myFsm.SendInput(inp);
    }

    private void Update()
    {
        _myFsm.Update();

        //Prov

    }

    private void FixedUpdate()
    {
        _myFsm.FixedUpdate();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //SendInputToFSM(PlayerInputs.IDLE);
    }


    //Buscado de boid cercano IA TP2 - P1 - PROGRAMACION FUNCIONAL
    IEnumerable<Tuple<Vector3, float, Boid>> BoidSearcher(List<Boid> enemyBoids)
    {
        var myCol = enemyBoids.Aggregate(new List<Tuple<Vector3, float, Boid>>(), (acum, current) =>
        {
            var dir = current.transform.position - transform.position;
            var tuple = Tuple.Create(dir, dir.magnitude, current);
            acum.Add(tuple);


            return acum;

        }).OrderBy(x => x.Item2);

        return myCol;

    }

    //Filtro a los boids aliados de los boids enemigos y que tengan una condicion de vida especifica
    // IA-TP2-PROGRAMACIONFUNCIONAL - 
    IEnumerable<Boid> EnemyBoids(List<Boid> allBoids)
    {
        //Si son enemigos y y tienen mas de 50 de vida la idea es que los ataque y los transforme a aliados
        var myCol = allBoids.Where(x => x.isEnemy && x.currentHp >= 50)
                            .Select(x => x);

        return myCol;
    }


    //La idea es que cada waypoint sea SAFE o no sea SAFE, es decir la variable en el array de booleans, si la variable es true,
    //entonces va a ese waypoint, si la variable no es true, no va a ese waypoint

    // IA TP-2 Programacion Funcional (ZIP)
    IEnumerable<Tuple<Waypoint, bool>> SafeWaypoint(WaypointsSafety wpList)
    {
        var myCol = wpList.waypoints.Zip(wpList.canEnter, (booleans, waypoints) => Tuple.Create(booleans, waypoints))
              .Where(x => x.Item2);

        return myCol;
    }
}
