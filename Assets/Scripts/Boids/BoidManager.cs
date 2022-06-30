using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class BoidManager : MonoBehaviour
{
    public static BoidManager instance;
    public List<Boid> allBoids = new List<Boid>(); //Lista de los boids
    public List<Boid> mindWashedBoids = new List<Boid>();

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
       mindWashedBoids = TraitorBoids(allBoids).ToList();
    }
    //El lider enemigo infecto la conciencia de ciertos boids, por lo tanto todos sus lideres (sus primeros 3) y todos sus ultimos reclutas
    //(los ultimos 3) seran secuestrados!

    IEnumerable<Boid> TraitorBoids(IEnumerable<Boid> allBoids)
    {
        //Primero tomo los primeros 3 Boids, luego hago un concat y skipeo todos,
        //Como si tengo 10 voids, origianlmenete tengo del slot 0 al 9, entonces si mi count da 10,
        //y le pongo -3 solo deberia tomarme el 8 y el 9, ya que count va a dar 10 y eso va a dar slot 8 y 9,
        //Por lo tanto hago count -4.
        var myCol = allBoids.Take(3).Concat(allBoids.Skip(allBoids.Count() - 4))
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
