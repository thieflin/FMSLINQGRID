using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//[ExecuteInEditMode]
public class GridEntity : MonoBehaviour
{
    public event Action<GridEntity> OnMove = delegate { };
    public Vector3 velocity = new Vector3(0, 0, 0);
    public bool onGrid;

    public Tuple<int, int> myPos = Tuple.Create(0, 0);

    public List<GridEntity> entityInSameCell;


    private void Awake()
    {
       
    }

    void Update()
    {

        //Optimization: Hacer esto solo cuando realmente se mueve y no en el update

        if (entityInSameCell.Count > 0)                                                                               //IA2-P1
        {                                                                                                             //IA2-P1
            for (int i = 0; i < entityInSameCell.Count; i++)                                                          //IA2-P1
            {                                                                                                         //IA2-P1
                var neighbor = entityInSameCell[i];                                                                   //IA2-P1
                                                                                                                      //IA2-P1
                if (myPos.Item1 != neighbor.myPos.Item1 && myPos.Item2 != neighbor.myPos.Item2)                       //IA2-P1
                {                                                                                                     //IA2-P1
                    entityInSameCell.Remove(neighbor);                                                                //IA2-P1
                }                                                                                                     //IA2-P1
            }                                                                                                         //IA2-P1
        }                                                                                                             //IA2-P1

        transform.position += velocity * Time.deltaTime;
        OnMove(this);
    }

}
