using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

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
    private void Start()
    {
       TraitorBoids(allBoids).ToList();
    }
    //El lider enemigo infecto la conciencia de ciertos boids, por lo tanto todos sus lideres (sus primeros 3) y todos sus ultimos reclutas
    //(los ultimos 3) seran secuestrados!

    IEnumerable<Boid> TraitorBoids(IEnumerable<Boid> allBoids)
    {
        var myCol = allBoids.Take(3).Concat(allBoids.Skip(allBoids.Count() - 3))
        //uNA VEZ Hecha mi primer lista, que son los que son enemigos, lo que hago es ahcer un aggregate y a cada current, osea
        //cada boid enemigo le hago la variable de enemy true y le defino su color
        .Aggregate(new List<Boid>(), (acum, current) =>
        {
            current.isEnemy = true;
            current.rend.material = current.matEnemy;
            acum.Add(current);
            return acum;
        });
        return myCol;
    }
}
