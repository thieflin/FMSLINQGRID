using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    public static BoidManager instance;
    public List<Boid> allBoids = new List<Boid>(); //Lista de los boids

    private void Awake()
    {
        //Si la instancia es nula entonces isntance es esto, sino la destruyo para quen o haya muchas
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    public void AddBoid(Boid b) //Funcion para agregar boids a ala lista
    {
        if (!allBoids.Contains(b))
            allBoids.Add(b);
    }
}
