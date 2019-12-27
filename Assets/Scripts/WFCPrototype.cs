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
        foreach (var direction in SlotDirection.Directions)
        {
            if (direction == SlotDirection.UP || direction == SlotDirection.DOWN)
            {
            }
            else
            {
                Possible[direction] = new int[] { Id };
            }
                Possible[direction] = new int[] { (Id + 1) % 2 };
            //Possible[direction] = new int[] { 0 };
        }
    }
    public int[] GetPossible(int direction)
    {
        return Possible[direction];
    }
}

