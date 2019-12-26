using Assets.Scripts.Solver;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class WFC : MonoBehaviour
{
    // Start is called before the first frame update

    public Vector3Int Size = new Vector3Int(3, 4, 3);
    public int SlotSize = 1;

    public GameObject debug_spawner;
    public GameObject debug_spawner_2;

    public bool DrawDebug = true;
    public long DebugSpeed = 200;


    public List<WFCPrototype> prototypes;

    private Slot[,,] grid;
    
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

        //prototypes.Add(new WFCPrototype(2));

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

        Solver solver = new Solver(grid, prototypes);
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
                        var pos = new Vector3(x, y, z);
                        if (v == 0)
                        {
                            Instantiate(this.debug_spawner, pos, Quaternion.identity, this.transform);
                        }
                        else
                        {
                            Instantiate(this.debug_spawner_2, pos, Quaternion.identity, this.transform);
                        }
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

    }
}
