using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

static class SlotDirection
{
	public const int UP = 0;
	public const int DOWN = 1;
	public const int LEFT = 2;
	public const int RIGHT = 3;
	public const int BACK = 4;
	public const int FORWARD = 5;

	public static readonly int[] Directions =  { UP, DOWN, LEFT, RIGHT, BACK, FORWARD };
    public static readonly int[] CounterDirections = { DOWN, UP, RIGHT, LEFT, FORWARD, BACK };
    public static readonly int[] Flipped = { UP, DOWN, LEFT, RIGHT, FORWARD, BACK};

    public static readonly string[] Names = { "Up", "Down", "Left", "Right", "Back", "Forward" };
    public static readonly Vector3[] Transforms = {
        new Vector3(0, 1, 0),
        new Vector3(0, -1, 0),
        new Vector3(-1, 0, 0),
        new Vector3(1,0, 0),
        new Vector3(0, 0, -1),
        new Vector3(0, 0, 1),
    };

    public static readonly int[][] Rotation = {
        new [] { UP, DOWN, LEFT, RIGHT, BACK, FORWARD }, // 0 = 0 degrees
        new [] { UP, DOWN, BACK, FORWARD, RIGHT, LEFT}, // 1 = 90 degrees
        new [] { UP, DOWN, RIGHT, LEFT, FORWARD,  BACK}, // 2 = 180 degrees
        new [] { UP, DOWN, FORWARD, BACK, LEFT, RIGHT }, // 3 = 270 degrees
    };
}

