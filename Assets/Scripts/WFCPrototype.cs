using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WFCPrototype
{

    public int Id { get; }
    public int Rotation { get; }
    public WFCSchema Schema { get; }

    public int[][] Possible = new int[6][];

    public WFCPrototype(int id, int rotation, WFCSchema schema)
    {
        this.Id = id;
        this.Schema = schema;
        this.Rotation = rotation;
        
    }
    public int[] GetPossible(int direction)
    {
        return Possible[direction];
    }

    public Quaternion GetRotation() => Quaternion.Euler(0, Rotation * 90, 0);
    public GameObject GetGameObjectPrototype() => Schema.gameObject;
    
}

