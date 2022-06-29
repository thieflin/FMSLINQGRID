using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Boid : MonoBehaviour
{
    [Header("Valores iniciales")]
    private Vector3 _velocity;
    public float maxSpeed;
    public float force;

    [Header("Arrival")]
    public GameObject foodTarget;//Elemento el cual buscaran y recogeran los voids.
    public float slowingRadio; //Radio en el cual empieza a arrivar;
    public float detectionRadio; //Radio en el cual reconocen a la comida
    public float nukeRadio; //Radio en el cual me nukean la comida porque la agarraron
    public float foodTimer; //Timer de la comida


    [Header("Evade")]
    public GameObject hunterTarget; //El NPC Hunter
    public float evadeRadio; //El radio de evasion
    public float futureTime; //Future time


    [Header("Separation")]
    public float viewDistance;
    public float separationDistance;

    [Header ("Floats Sep, Coh, Alig")]
    public float separationWeight;
    public float cohesionWeight;
    public float alignWeight;

    void Start()
    {
        BoidManager.instance.AddBoid(this);

        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));

        randomDirection.Normalize();
        randomDirection *= maxSpeed;
        AddForce(randomDirection);
    }

    
    void Update()
    {
        CheckBounds();
        AddForce(Separation() * separationWeight + Align() * alignWeight + Cohesion() * cohesionWeight);
        CourseOfAction();
        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity;
        
    }
    public void CourseOfAction()
    {
        Vector3 desiredArrival = transform.position - foodTarget.transform.position;
        Vector3 desiredEvade = transform.position - hunterTarget.transform.position;
        if (desiredEvade.magnitude < desiredArrival.magnitude)
        {
            Evade();
        }
        else
        {
            Arrive();
        }

    }

    #region Arrive de la comida


    public void Arrive()
    {
        
        Vector3 desired = foodTarget.transform.position - transform.position; //Desired es el vector que se da entre el target y el boid

        if (desired.magnitude < detectionRadio) //Si entro en el radio de deteccion, ahi activo arrive, sino me mantengo fuera de ello, solo lo targeteo cuando estoy lo suficientemnete creca
        {
            if (desired.magnitude < slowingRadio)
            {
                float speed = maxSpeed * (desired.magnitude / 10);
                desired.Normalize();
                desired *= speed;
            }
            else
            {
                desired.Normalize();
                desired *= maxSpeed;
            }

            if (desired.magnitude < nukeRadio) //Cuando agarra a la comida ejecuta una corrutina
            {
                foodTimer = Random.Range(5, 11); //Seteo para que la comida no se spawnee cada un mismmo tiempo siempre
                StartCoroutine(RestoreFood());
                
            }


            desired.Normalize();
            desired *= maxSpeed;

            Vector3 steering = desired - _velocity;

            steering = Vector3.ClampMagnitude(steering, force);

            steering.Normalize();
            steering *= force;


            AddForce(steering);

        }
        
    }

    IEnumerator RestoreFood() //La corrutina que ejecuta lo que hace es acomodar en un lugar random a la comida, y desacvitarla, esperar y luego activarla
    {
        foodTarget.transform.position = new Vector3(Random.Range(-3, 3), 0, Random.Range(-8, 8)); //La posicion randomizada
        foodTarget.SetActive(false); 
        yield return new WaitForSeconds(foodTimer); //Paso food timer, cada vez que me como la comida, hago que el tiempo para que vuelva a respawnear sea distinto
        foodTarget.SetActive(true); 
    }

    #endregion


    #region Evadir al NPC Cazador
    void Evade()
    {
        Hunter hunter = hunterTarget.GetComponent<Hunter>();
        if (hunterTarget == null) return;
        Vector3 futurePos = hunterTarget.transform.position + hunterTarget.GetComponent<Hunter>().GetVelocity() * futureTime;

        Vector3 desired = futurePos - transform.position;

        if(desired.magnitude < evadeRadio)
        {
            desired.Normalize();
            desired *= maxSpeed;
            desired = -desired;

            Vector3 steering = desired - _velocity;
            steering = Vector3.ClampMagnitude(steering, force);

            AddForce(steering);
        }

        
    }
    #endregion

    public Vector3 GetVelocity()
    {
        return _velocity;
    }



    Vector3 Cohesion()
    {
        Vector3 desired = new Vector3();
        int nearbyBoids = 0;

        foreach (var boid in BoidManager.instance.allBoids)
        {
            if (boid != this && Vector3.Distance(boid.transform.position, transform.position) < viewDistance)
            {
                desired += boid.transform.position;
                nearbyBoids++;
            }
        }
        if (nearbyBoids == 0) return desired;
        desired /= nearbyBoids;
        desired = desired - transform.position;
        desired.Normalize();
        desired *= maxSpeed;

        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, force);
        return steering;
    }

    Vector3 Align()
    {
        Vector3 desired = new Vector3();
        int nearbyBoids = 0;
        foreach (var boid in BoidManager.instance.allBoids)
        {
            if (boid != this && Vector3.Distance(boid.transform.position, transform.position) < viewDistance)
            {
                desired += boid._velocity;
                nearbyBoids++;
            }
        }
        if (nearbyBoids == 0) return Vector3.zero;
        desired /= nearbyBoids;
        desired.Normalize();
        desired *= maxSpeed;

        Vector3 steering = Vector3.ClampMagnitude(desired - _velocity, force);

        return steering;
    }


    Vector3 Separation()
    {
        Vector3 desired = new Vector3();
        int nearbyBoids = 0;

        var myCol = BoidManager.instance.allBoids.Select(x => x);

        foreach (var boid in myCol /*BoidManager.instance.allBoids*/) //Reviso en cada elemento de la lista constituida por mis boids
        {
            Vector3 distance = boid.transform.position - transform.position;

            if (boid != this && distance.magnitude < separationDistance)
            {
                desired.x += distance.x;
                desired.z += distance.z;
                nearbyBoids++;
            }
        }
        if (nearbyBoids == 0) return desired;
        desired /= nearbyBoids;
        desired.Normalize();
        desired *= maxSpeed;
        desired = -desired;

        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, force);
        return steering;
    }

    void AddForce(Vector3 force)
    {
        _velocity += force;
        _velocity = Vector3.ClampMagnitude(_velocity, maxSpeed);

    }

    void CheckBounds()
    {
        if (transform.position.z > 9) transform.position = new Vector3(transform.position.x, transform.position.y, -9);
        if (transform.position.z < -9) transform.position = new Vector3(transform.position.x, transform.position.y, 9);
        if (transform.position.x < -5.2f) transform.position = new Vector3(5.2f, transform.position.y, transform.position.z);
        if (transform.position.x > 5.2f) transform.position = new Vector3(-5.2f, transform.position.y, transform.position.z);
    }





}
