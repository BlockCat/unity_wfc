using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

namespace Assets.Scripts.Solver
{

    public class Solver
    {
        private Random random;
        private Slot[,,] grid;
        private List<WFCPrototype> prototypes;

        private int Depth = 0;
        private List<Slot> unassignedSlots;
        private Stack<Slot> instantiatedSlots;

        public int Backtracks { get; private set; }

        public event EventHandler<CompletedEventArgs> Completed;
        public event EventHandler<int> Failed;

        public Solver(Slot[,,] grid, List<WFCPrototype> prototypes)
        {
            this.grid = grid;
            this.random = new Random();
            this.prototypes = prototypes;

            this.unassignedSlots = new List<Slot>(grid.Cast<Slot>());
            this.instantiatedSlots = new Stack<Slot>();

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

        public void Solve()
        {

            while (true)
            {
                if (this.unassignedSlots.Count == 0)
                {
                    this.Completed?.Invoke(this, new CompletedEventArgs
                    {
                        Grid = this.grid,
                        Backtracks = this.Backtracks

                    }); ;
                    return;
                }

                var smallestDomain = FindSmallestDomain();

                instantiatedSlots.Push(smallestDomain);
                unassignedSlots.Remove(smallestDomain);
                instantiatedSlots.Peek().Instantiate(this.Depth, random);

                if (ConstraintsViolated(instantiatedSlots.Peek()) || unassignedSlots.Any(x => x.DomainSize == 0))
                {
                    if (Backtrack(unassignedSlots, instantiatedSlots) == PropagationState.Violated)
                    {
                        this.Failed?.Invoke(this, Backtracks);
                    }
                }

                ++Depth;
            }
        }

        private PropagationState Backtrack(List<Slot> unassignedSlots, Stack<Slot> instantiatedSlots)
        {
            DomainOperationResult result;
            do
            {
                if (this.Depth < 0)
                {
                    return PropagationState.Violated;
                }
                var unassign = instantiatedSlots.Pop();
                unassignedSlots.Insert(0, unassign);
                result = BacktrackVariable(unassign);
            } while (result == DomainOperationResult.EmptyDomain);
            return PropagationState.Propagated;
        }

        private DomainOperationResult BacktrackVariable(Slot unassign)
        {
            ++this.Backtracks;
            var v = unassign.Value;

            foreach (var s in instantiatedSlots)
            {
                s.Backtrack(this.Depth);
            }
            foreach (var s in unassignedSlots)
            {
                s.Backtrack(this.Depth);
            }

            --this.Depth;

            return unassign.RemoveCandidate(this.Depth, v);
        }

        private bool ConstraintsViolated(Slot changed)
        {
            var state = Propagate(changed, out var _);
            if (state == PropagationState.Violated)
            {
                return true;
            }

            return false;
        }
        private Slot FindSmallestDomain()
        {
            Slot temp = unassignedSlots.First();
            foreach (var x in this.unassignedSlots)
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
                        var result = neighbour.LessenDomain(this.Depth, node.GetPossible(direction, prototypes));
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

    public enum DomainOperationResult
    {
        EmptyDomain,
        FilledDomain
    }

    public struct CompletedEventArgs
    {
        public Slot[,,] Grid;
        public int Backtracks;
    }
}
