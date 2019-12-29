using Assets.Scripts.Solver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("WFC/Mapper")]
public class WFC : MonoBehaviour
{
	// Start is called before the first frame update

	public Vector3Int Size = new Vector3Int(3, 4, 3);
	public int SlotSize = 1;

	public GameObject debug_spawner;
	public GameObject debug_spawner_2;

	public bool DrawDebug = true;
	public long DebugSpeed = 200;

	public int[] HorizontalConnector;
	public int[] UpConnector;
	public int[] DownConnector;


	public WFCPrototype[] prototypes;

	private Slot[,,] grid;

	public GameObject SceneLoader;

	public void ClearChildren()
	{
		for (int i = transform.childCount - 1; i >= 0; --i)
		{
			DestroyImmediate(transform.GetChild(i).gameObject);
		}
	}
	public void StartGeneration()
	{
		Debug.Assert(prototypes != null, "Prototypes was not generated");
		grid = new Slot[Size.x, Size.y, Size.z];

		for (int x = 0; x < Size.x; x++)
		{
			for (int y = 0; y < Size.y; y++)
			{
				for (int z = 0; z < Size.z; z++)
				{
					grid[x, y, z] = new Slot(prototypes.Select(p => p.Id));
				}
			}
		}

		// Back and forward sides
		for (int x = 0; x < Size.x; x++)
		{
			for (int y = 0; y < Size.y; y++)
			{
				grid[x, y, 0].SetLockedDirection(SlotDirection.BACK, HorizontalConnector, prototypes);
				grid[x, y, Size.z - 1].SetLockedDirection(SlotDirection.FORWARD, HorizontalConnector, prototypes);
			}
		}
		
		for (int y = 0; y < Size.y; y++)
		{
			for (int z = 0; z < Size.z; z++)
			{
				grid[0, y, z].SetLockedDirection(SlotDirection.LEFT, HorizontalConnector, prototypes);
				grid[Size.x - 1, y, z].SetLockedDirection(SlotDirection.RIGHT, HorizontalConnector, prototypes);
				Debug.Assert(grid[0, y, z].DomainSize > 0, "Instantiated L, new domain size is now 0");
				Debug.Assert(grid[Size.x - 1, y, z].DomainSize > 0, "Instantiated R, new domain size is now 0");
			}
		}
		for (int z = 0; z < Size.z; z++)
		{
			for (int x = 0; x < Size.x; x++)
			{
				grid[x, 0, z].SetLockedDirection(SlotDirection.DOWN, DownConnector, prototypes);
				grid[x, Size.y - 1, z].SetLockedDirection(SlotDirection.UP, UpConnector, prototypes);
				Debug.Assert(grid[x, 0, z].DomainSize > 0, "Instantiated D, new domain size is now 0");
				Debug.Assert(grid[x, Size.y - 1, z].DomainSize > 0, "Instantiated U, new domain size is now 0");
			}
		}

		Solver solver = new Solver(grid, prototypes.ToList());
		solver.Completed += (_, result) =>
		{
			Debug.Log($"Completed with {result.Backtracks} backtracks");
			for (int x = 0; x < Size.x; x++)
			{
				for (int y = 0; y < Size.y; y++)
				{
					for (int z = 0; z < Size.z; z++)
					{
						var v = result.Grid[x, y, z].Value;
						var pos = transform.TransformPoint(new Vector3(x, y, z) + new Vector3(0.5f, 0, 0.5f));
						var rotation = prototypes[v].GetRotation();
						var scale = prototypes[v].GetScale();
						var go = Instantiate(prototypes[v].GetGameObjectPrototype(), pos, rotation, this.transform);
						go.transform.localScale = scale;

					}
				}
			}
		};
		solver.Failed += (_, backtracks) =>
		{
			Debug.LogError($"No solution could be found with {backtracks} backtracks.");
		};

		solver.Solve();

		grid = null;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Vector3 position = transform.position + (Vector3)(Size * SlotSize) * 0.5f;
		Gizmos.DrawWireCube(position, Size * SlotSize);
		Gizmos.color = Color.red;
		for (int y = 0; y < Size.y; y++)
		{
			for (int z = 0; z < Size.z; z++)
			{
				Gizmos.DrawRay(transform.TransformPoint(new Vector3(0, y, z) + Vector3.one * 0.5f), Vector3.left);
				Gizmos.DrawRay(transform.TransformPoint(new Vector3(Size.x - 1, y, z) + Vector3.one * 0.5f), Vector3.right);
			}
		}
		for (int z = 0; z < Size.z; z++)
		{
			for (int x = 0; x < Size.x; x++)
			{
				Gizmos.DrawRay(transform.TransformPoint(new Vector3(x, 0, z) + Vector3.one * 0.5f), Vector3.down);
				Gizmos.DrawRay(transform.TransformPoint(new Vector3(x, Size.y - 1, z) + Vector3.one * 0.5f), Vector3.up);
			}
		}
		for (int x = 0; x < Size.x; x++)
		{
			for (int y = 0; y < Size.y; y++)
			{
				Gizmos.DrawRay(transform.TransformPoint(new Vector3(x, y, 0) + Vector3.one * 0.5f), Vector3.back);
				Gizmos.DrawRay(transform.TransformPoint(new Vector3(x, y, Size.z - 1) + Vector3.one * 0.5f), Vector3.forward);
			}
		}
	}
}
