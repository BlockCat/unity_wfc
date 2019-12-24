using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



static class SlotDirection
{
	public const int UP = 0;
	public const int DOWN = 1;
	public const int LEFT = 2;
	public const int RIGHT = 3;
	public const int BACK = 4;
	public const int FORWARD = 5;

	public static readonly int[] Directions =  { UP, DOWN, LEFT, RIGHT, BACK, FORWARD };
}

