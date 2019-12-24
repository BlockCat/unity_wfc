using Decider.Csp.BaseTypes;
using Decider.Csp.Integer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WFC : MonoBehaviour
{
	// Start is called before the first frame update

	public Vector3Int Size = new Vector3Int(3, 4, 3);
	public int SlotSize = 1;

	public GameObject debug_spawner;
	public GameObject debug_spawner_2;


	public List<WFCPrototype> prototypes;

	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void ClearChildren()
	{
		for (int i = transform.childCount - 1; i >= 0; --i)
		{
			DestroyImmediate(transform.GetChild(i).gameObject);
		}
	}
	public void StartGeneration()
	{

		prototypes = new List<WFCPrototype>();
		prototypes.Add(new WFCPrototype(0));
		prototypes.Add(new WFCPrototype(1));

		VariableInteger[,,] slotGrid = new VariableInteger[Size.x, Size.y, Size.z];

		ConstrainedArray protoypeVariables = new ConstrainedArray(prototypes.Select(x => x.Id));
		VariableInteger[,,,] protoypeDirectionSelectors = new VariableInteger[Size.x, Size.y, Size.z, 6];
		ConstrainedArray2D[] prototypeDirectionVariables = new ConstrainedArray2D[6];

		List<IVariable<int>> variables = new List<IVariable<int>>();
		List<ConstraintInteger> constraints = new List<ConstraintInteger>();

		// Every slot should get some prototype assigned
		for (int x = 0; x < Size.x; x++)
		{
			for (int y = 0; y < Size.y; y++)
			{
				for (int z = 0; z < Size.z; z++)
				{
					slotGrid[x, y, z] = new VariableInteger($"{x},{y},{z}", prototypes.Select(a => a.Id).ToList());
					variables.Add(slotGrid[x, y, z]);

					foreach (int direction in SlotDirection.Directions)
					{
						protoypeDirectionSelectors[x, y, z, direction] = new VariableInteger($"s_{x}{y}{z}_{direction}", 0, prototypes.Count);
						variables.Add(protoypeDirectionSelectors[x, y, z, direction]);
					}
				}
			}
		}

		// Initialize prototype variabvles
		foreach (int direction in SlotDirection.Directions)
		{
			List<int[]> p = new List<int[]>();
			for (int i = 0; i < prototypes.Count; i++)
			{
				p.Add(prototypes[i].GetPossible(direction));
			}
			prototypeDirectionVariables[direction] = new ConstrainedArray2D(p);
		}




		var directions = new (int, int, int, int, int)[]
		{
			(0,1,0, SlotDirection.UP, SlotDirection.DOWN),
			(0,-1,0, SlotDirection.DOWN, SlotDirection.UP),
			(-1,0,0, SlotDirection.LEFT, SlotDirection.RIGHT),
			(1,0,0, SlotDirection.RIGHT, SlotDirection.LEFT),
			(0,0,1, SlotDirection.FORWARD, SlotDirection.BACK),
			(0,0,-1, SlotDirection.BACK, SlotDirection.FORWARD),
		};

		// Neighbour constraints.		
		for (int x = 1; x < Size.x - 1; x++)
		{
			for (int y = 1; y < Size.y - 1; y++)
			{
				for (int z = 1; z < Size.z - 1; z++)
				{
					// Slot a contains a int that decides the prototype:

					// the left of protoype a should contain b
					// the right of prototype b should contain a
					// aka. ∃x1 | D[b,right,x1] = a
					// aka. ∃x2 | D[a,left,x2] = b

					var currentSlot = slotGrid[x, y, z];

					foreach (var (dx, dy, dz, direction, otherDirection) in directions)
					{
						var nextSlot = slotGrid[x + dx, y + dy, z + dz];

						//var selector1 = protoypeDirectionSelectors[x, y, z, direction];
						var d1 = prototypeDirectionVariables[direction][currentSlot];


						constraints.Add(new ConstraintInteger(d1 == nextSlot));

						//var selector2 = protoypeDirectionSelectors[x + dx, y + dy, z + dz, otherDirection];
						var d2 = prototypeDirectionVariables[direction][nextSlot];
						constraints.Add(new ConstraintInteger(d2 == currentSlot));

					}
				}
			}
		}


		IState<int> state = new StateInteger(variables, constraints);



		state.StartSearch(out StateOperationResult result);

		if (result == StateOperationResult.Solved)
		{
			for (int x = 0; x < Size.x; x++)
			{
				for (int y = 0; y < Size.y; y++)
				{
					for (int z = 0; z < Size.z; z++)
					{
						var value = slotGrid[x, y, z];
						var veci = new Vector3Int(x, y, z);
						if (value.Value == 0)
						{
							var o = Instantiate(debug_spawner, veci, Quaternion.identity, transform);
							o.name = $"spawn_{x}_{y}_{z}_{value}";
							//Debug.Log($"{value} but {value.Integer} or {value.Value}");
						} else
						{
							var o = Instantiate(debug_spawner_2, veci, Quaternion.identity, transform);
							o.name = $"spawn_{x}_{y}_{z}_{value}";
						}
					}
				}
			}
		}
		else
		{
			Debug.LogError("Could not find valid solution");
		}

	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Vector3 position = transform.position + (Vector3)(Size * SlotSize) * 0.5f;
		Gizmos.DrawWireCube(position, Size * SlotSize);
	}
}
