﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class WFCPrototype
{

    public int Id;
    public int[][] Possible = new int[6][];

    public WFCPrototype(int id)
    {
        this.Id = id;
        foreach (var direction in SlotDirection.Directions)
        {
            if (direction == SlotDirection.UP || direction == SlotDirection.DOWN)
            {
                Possible[direction] = new int[] { (Id + 1)%2 };                 
            } else
            {
                Possible[direction] = new int[] { Id };
            }
            //Possible[direction] = new int[] { 0 };
        }
    }
    public int[] GetPossible(int direction)
    {
        return Possible[direction];
    }
}

