using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

namespace Assets.Scripts.Solver
{

    public class Solver
    {
        private Random random;
        private Slot[,,] grid;
        private List<Slot> slots = new List<Slot>();
        private List<WFCPrototype> prototypes;

        public Solver(Slot[,,] grid, List<WFCPrototype> prototypes)
        {
            this.slots = new List<Slot>(grid.Cast<Slot>());
            this.grid = grid;
            this.random = new Random();
            this.prototypes = prototypes;

            InitializeGrid(grid);
        }

        public void SetSeed(int seed)
        {
            this.random = new Random(seed);
        }

        private void InitializeGrid(Slot[,,] grid)
        {
            var directions = new (int, int, int, int, int)[]
            {
                (0,1,0, SlotDirection.UP, SlotDirection.DOWN),
                (0,-1,0, SlotDirection.DOWN, SlotDirection.UP),
                (-1,0,0, SlotDirection.LEFT, SlotDirection.RIGHT),
                (1,0,0, SlotDirection.RIGHT, SlotDirection.LEFT),
                (0,0,1, SlotDirection.FORWARD, SlotDirection.BACK),
                (0,0,-1, SlotDirection.BACK, SlotDirection.FORWARD),
            };

            for (int x = 0; x < grid.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    for (int z = 0; z < grid.GetLength(2); z++)
                    {
                        foreach (var (dx, dy, dz, dir, counterdir) in directions)
                        {
                            var (nx, ny, nz) = (x + dx, y + dy, z + dz);
                            if (nx < 0 || ny < 0 || nz < 0) continue;
                            if (nx >= grid.GetLength(0) || ny >= grid.GetLength(1) || nz >= grid.GetLength(2)) continue;

                            grid[x, y, z].AddNeighbour(dir, grid[nx, ny, nz]);
                        }
                    }
                }
            }
        }

        public Slot[,,] Solve()
        {
            List<List<Slot>> history = new List<List<Slot>>();
            do
            {
                Slot candidate = FindSmallestDomain();
                candidate.Intantiate(random);
                var result = Propagate(candidate, out var changeSet);

                if (result == PropagationState.Propagated)
                {
                    history.Add(changeSet);
                }
                else if (result == PropagationState.Violated)
                {

                    throw new NotImplementedException("Violation is not accepted");

                    int wrong = candidate.Value;
                    foreach (var x in changeSet)
                    {
                        x.Backtrack();
                    }
                    candidate.RemoveCandidate(wrong);

                    Propagate(candidate, out changeSet);
                }
                else
                {
                    throw new Exception("Unreachable state");
                }
                // now propagate
            } while (!Check());

            return grid;
        }

        private bool Check() => this.slots.All(x => x.IsInstantiated);
        private Slot FindSmallestDomain()
        {
            Slot temp = slots.First(x => !x.IsInstantiated);
            foreach (var x in this.slots.Where(x => !x.IsInstantiated))
            {
                if (x.DomainSize < temp.DomainSize)
                {
                    temp = x;
                }
            }
            return temp;
        }

        /// <summary>
        /// Propage slot through the grid
        /// </summary>
        /// <param name="slot"></param>
        private PropagationState Propagate(Slot start, out List<Slot> changeSet)
        {
            Stack<Slot> stack = new Stack<Slot>();
            List<Slot> changed = new List<Slot>();
            stack.Push(start);
            changed.Add(start);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                foreach (var direction in SlotDirection.Directions)
                {
                    foreach (var neighbour in node.Neighbours[direction].Where(n => !n.IsInstantiated))
                    {
                        var result = neighbour.LessenDomain(node.GetPossible(direction, prototypes));
                        if (result == PropagationState.Propagated || result == PropagationState.Instantiated)
                        {
                            stack.Push(neighbour);
                            changed.Add(neighbour);
                        }
                        else if (result == PropagationState.Violated)
                        {
                            changeSet = changed;
                            return PropagationState.Violated;
                        }
                    }
                }
            }
            changeSet = changed;
            return PropagationState.Propagated;
        }
    }

    public enum PropagationState
    {
        Propagated, Instantiated, Violated, Unchanged
    }
}
