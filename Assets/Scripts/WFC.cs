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
		ConstrainedArray[,,,] directionGrid = new ConstrainedArray[Size.x, Size.y, Size.z, 6];
		VariableInteger[,,] slotGrid = new VariableInteger[Size.x, Size.y, Size.z];
		//VariableInteger[,,,] selector = new VariableInteger[Size.x, Size.y, Size.z, 6];

		ConstrainedArray protoypeVariables = new ConstrainedArray(prototypes.Select(x => x.Id));
		VariableInteger[,] protoypeDirectionSelectors = new VariableInteger[prototypes.Count, 6];
		VariableInteger[,] prototypeDirectionVariables = new VariableInteger[prototypes.Count, 6];

		List<IVariable<int>> variables = new List<IVariable<int>>();
		List<ConstraintInteger> constraints = new List<ConstraintInteger>();

		// Initialize prototype variabvles
		for (int i = 0; i < prototypes.Count; i++)
		{
			foreach (int direction in SlotDirection.Directions)
			{
				var possibleIds = prototypes[i].GetPossible(direction);
				protoypeDirectionSelectors[i, direction] = new VariableInteger($"s_{i}_{direction}", possibleIds);
				prototypeDirectionVariables[i, direction] = new VariableInteger($"d_{i}_{direction}", possibleIds);

				variables.Add(protoypeDirectionSelectors[i, direction]);
				variables.Add(prototypeDirectionVariables[i, direction]);
			}
		}


		// Every slot should get some prototype assigned
		for (int x = 0; x < Size.x; x++)
		{
			for (int y = 0; y < Size.y; y++)
			{
				for (int z = 0; z < Size.z; z++)
				{
					slotGrid[x, y, z] = new VariableInteger($"{x},{y},{z}", prototypes.Select(a => a.Id).ToList());
					variables.Add(slotGrid[x, y, z]);					
				}
			}
		}

		// Neighbour constraints.		
		for (int x = 1; x < Size.x - 1; x++)
		{
			for (int y = 1; y < Size.y - 1; y++)
			{
				for (int z = 1; z < Size.z - 1; z++)
				{
					// Every slot contains a int
					/*var directedUp = directionGrid[x, y + 1, z, SlotDirection.DOWN];
					var directedDown = directionGrid[x, y - 1, z, SlotDirection.UP];
					var up = slotGrid[x, y + 1, z];
					var down = slotGrid[x, y - 1, z];

					var directedLeft = directionGrid[x - 1, y, z, SlotDirection.RIGHT];
					var directedRight = directionGrid[x + 1, y, z, SlotDirection.LEFT];
					var left = slotGrid[x - 1, y, z];
					var right = slotGrid[x + 1, y, z];

					var directedForward = directionGrid[x, y, z + 1, SlotDirection.BACK];
					var directedBack = directionGrid[x, y, z - 1, SlotDirection.FORWARD];
					var forward = slotGrid[x, y, z + 1];
					var back = slotGrid[x, y, z - 1];*/

					var currentSlot = slotGrid[x, y, z];

					// slots are equal or something.

					/*constraints.Add(new ConstraintInteger(directedUp[selector[x, y + 1, z, SlotDirection.DOWN]] == slotGrid[x, y, z]));
					constraints.Add(new ConstraintInteger(directedDown[selector[x, y - 1, z, SlotDirection.UP]] == slotGrid[x, y, z]));
					constraints.Add(new ConstraintInteger(directedLeft[selector[x - 1, y, z, SlotDirection.RIGHT]] == slotGrid[x, y, z]));
					constraints.Add(new ConstraintInteger(directedRight[selector[x + 1, y, z, SlotDirection.LEFT]] == slotGrid[x, y, z]));
					constraints.Add(new ConstraintInteger(directedBack[selector[x, y, z - 1, SlotDirection.FORWARD]] == slotGrid[x, y, z]));
					constraints.Add(new ConstraintInteger(directedForward[selector[x, y, z + 1, SlotDirection.BACK]] == slotGrid[x, y, z]));

					constraints.Add(new ConstraintInteger(directionGrid[x, y, z, SlotDirection.UP][selector[x, y, z, SlotDirection.UP]] == up));
					constraints.Add(new ConstraintInteger(directionGrid[x, y, z, SlotDirection.DOWN][selector[x, y, z, SlotDirection.DOWN]] == down));

					constraints.Add(new ConstraintInteger(directionGrid[x, y, z, SlotDirection.LEFT][selector[x, y, z, SlotDirection.LEFT]] == left));
					constraints.Add(new ConstraintInteger(directionGrid[x, y, z, SlotDirection.RIGHT][selector[x, y, z, SlotDirection.RIGHT]] == right));

					constraints.Add(new ConstraintInteger(directionGrid[x, y, z, SlotDirection.FORWARD][selector[x, y, z, SlotDirection.FORWARD]] == forward));
					constraints.Add(new ConstraintInteger(directionGrid[x, y, z, SlotDirection.BACK][selector[x, y, z, SlotDirection.BACK]] == back));*/
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
						var o = Instantiate(debug_spawner, veci, Quaternion.identity, transform);
						o.name = $"spawn_{x}_{y}_{z}_{value}";						
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
