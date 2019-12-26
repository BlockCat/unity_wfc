using Assets.Scripts.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct Domain
{
    public int Depth;
    public HashSet<int> Range;

    public Domain(Domain d)
    {
        this.Depth = d.Depth;
        this.Range = new HashSet<int>(d.Range);
    }

    public Domain(int depth, HashSet<int> range)
    {
        this.Depth = depth;
        this.Range = range;
    }
}
public class Slot
{
    public Domain Domain => DomainStack.Peek();
    public int DomainSize => Domain.Range.Count;
    public bool IsInstantiated => Domain.Range.Count == 1;

    private Stack<Domain> DomainStack;
    public int Value
    {
        get
        {
            if (IsInstantiated)
                return Domain.Range.First();

            throw new ArgumentException("Tried to take value whilst not instantiated");
        }
    }

    public List<Slot>[] Neighbours { get; }

    public Slot(IEnumerable<int> possibleProtoTypes)
    {
        this.DomainStack = new Stack<Domain>();
        this.DomainStack.Push(new Domain(-1, new HashSet<int>(possibleProtoTypes)));
        this.Neighbours = new List<Slot>[6] {
            new List<Slot>(),
            new List<Slot>(),
            new List<Slot>(),
            new List<Slot>(),
            new List<Slot>(),
            new List<Slot>()
        };
    }

    internal void Instantiate(int depth, Random random)
    {
        if (!IsInstantiated)
        {
            var oldDomain = new List<int>(this.Domain.Range);
            var newDomain = new Domain(depth, new HashSet<int>(new[] { oldDomain[random.Next(0, oldDomain.Count)] }));

            this.DomainStack.Push(newDomain);
        }
    }

    public PropagationState LessenDomain(int depth, HashSet<int> possibles)
    {

        if (Domain.Depth == depth)
        {
            int oldCount = Domain.Range.Count;
            Domain.Range.IntersectWith(possibles);
            if (Domain.Range.Count == 0)
            {
                return PropagationState.Violated;
            }
            if (oldCount == Domain.Range.Count)
            {
                return PropagationState.Unchanged;
            }
            else
            {
                return PropagationState.Propagated;
            }
        }
        HashSet<int> lessened = new HashSet<int>(possibles.Intersect(Domain.Range));
        if (lessened.Count == 0)
        {
            return PropagationState.Violated;
        }

        if (lessened.Count == Domain.Range.Count)
        {
            return PropagationState.Unchanged;
        }
        Domain next = new Domain(depth, lessened);
        DomainStack.Push(next);
        return PropagationState.Propagated;
    }

    public void Backtrack(int depth)
    {
        // remove the domain of everything deeper than depth
        while (DomainStack.Peek().Depth >= depth)
        {
            DomainStack.Pop();
        }
    }

    public void AddNeighbour(int dir, Slot slot)
    {
        this.Neighbours[dir].Add(slot);
    }

    public HashSet<int> GetPossible(int direction, List<WFCPrototype> prototypes)
    {
        HashSet<int> possible = new HashSet<int>();

        foreach (int index in this.DomainStack.Peek().Range)
        {
            possible.UnionWith(prototypes[index].GetPossible(direction));
        }

        return possible;
    }

    public DomainOperationResult RemoveCandidate(int depth, int wrong)
    {
        if (Domain.Depth == depth)
        {
            Domain.Range.Remove(wrong);
        }
        else
        {
            Domain clone = new Domain(this.Domain);
            clone.Depth = depth;
            clone.Range.Remove(wrong);
            DomainStack.Push(clone);
        }

        return (Domain.Range.Count == 0) ?
            DomainOperationResult.EmptyDomain :
            DomainOperationResult.FilledDomain;
    }
}