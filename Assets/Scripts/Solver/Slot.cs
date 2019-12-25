using Assets.Scripts.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Slot
{
    public int DomainSize => DomainStack.Peek().Count;
    public bool IsInstantiated => DomainSize == 1;
    public int Value
    {
        get
        {
            if (IsInstantiated)
                return DomainStack.Peek().First();

            throw new ArgumentException("Tried to take value whilst not instantiated");
        }
    }

    public List<Slot>[] Neighbours { get; }

    private Stack<HashSet<int>> DomainStack;
    public Slot(IEnumerable<int> possibleProtoTypes)
    {
        this.DomainStack = new Stack<HashSet<int>>();
        this.DomainStack.Push(new HashSet<int>(possibleProtoTypes));
        this.Neighbours = new List<Slot>[6] {
            new List<Slot>(),
            new List<Slot>(),
            new List<Slot>(),
            new List<Slot>(),
            new List<Slot>(),
            new List<Slot>()
        };
    }

    internal void Intantiate(Random random)
    {
        if (IsInstantiated)
        {
            throw new ArgumentException("Tried to instantiate whilst already instantiated");
        }

        var oldDomain = new List<int>(this.DomainStack.Peek());
        var newDomain = new HashSet<int>();
        newDomain.Add(oldDomain[random.Next(0, oldDomain.Count)]);

        this.DomainStack.Push(newDomain);
    }

    public PropagationState LessenDomain(HashSet<int> possibles)
    {
        var domain = this.DomainStack.Peek();
        HashSet<int> lessened = new HashSet<int>(possibles.Intersect(domain));
        if (lessened.Count < domain.Count)
        {
            if (lessened.Count == 0)
            {
                return PropagationState.Violated;
            }
            else
            {
                this.DomainStack.Push(lessened);
                if (lessened.Count == 1)
                {
                    return PropagationState.Instantiated;
                } else
                {
                    return PropagationState.Propagated;
                }
            }
        }
        return PropagationState.Unchanged;
    }

    public void Backtrack()
    {
        this.DomainStack.Pop();
    }

    public void AddNeighbour(int dir, Slot slot)
    {
        this.Neighbours[dir].Add(slot);
    }

    public HashSet<int> GetPossible(int direction, List<WFCPrototype> prototypes)
    {
        HashSet<int> possible = new HashSet<int>();

        foreach (int index in this.DomainStack.Peek())
        {
            possible.UnionWith(prototypes[index].GetPossible(direction));
        }

        return possible;
    }

    public void RemoveCandidate(int wrong)
    {
        var set = new HashSet<int>(this.DomainStack.Peek());
        set.Remove(wrong);
        this.DomainStack.Push(set);
    }
}