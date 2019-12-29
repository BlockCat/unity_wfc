using Assets.Scripts.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
	public bool IsInstantiated => DomainSize == 1;

	private Stack<Domain> DomainStack;
	public int Value {
		get {
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
		Debug.Assert(this.DomainSize > 0, "Domain size is 0");
	}

	public void SetLockedDirection(int direction, int[] possibleConnectors, WFCPrototype[] prototypes)
	{
		Debug.Assert(this.DomainStack.Count <= 1, $"Domainstack is larger than 1, cannot set direction when propagating: {this.DomainStack.Count}");
		Debug.Assert(this.DomainSize > 0, "Domain size is 0");

		if (possibleConnectors.Length == 0)
		{
			return;
		}

		// This domain should just be prototypes whose connector is good

		var neighbours = prototypes.Where(p =>
		{
			return p.GetSchemaConnectors(direction).Any(x => possibleConnectors.Contains(x.Connector));
		}).ToList();

		var result = this.LessenDomain(-1, neighbours.Select(p => p.Id));

		// Debug.Log($"Locking: {DomainSize} left");
		if (result == PropagationState.Violated || DomainSize == 0)
		{
			throw new Exception($"Failed to add lock: {direction}");
		}
	}

	internal void Instantiate(int depth, System.Random random)
	{
		Debug.Assert(DomainSize > 0, "Domain size is 0 whilst trying to instantiate");
		if (!IsInstantiated)
		{
			var oldDomain = new List<int>(this.Domain.Range);
			var newDomain = new Domain(depth, new HashSet<int>(new[] { oldDomain[random.Next(0, oldDomain.Count)] }));

			this.DomainStack.Push(newDomain);
		}
	}

	public PropagationState LessenDomain(int depth, IEnumerable<int> possibles)
	{
		return LessenDomain(depth, new HashSet<int>(possibles));
	}
	public PropagationState LessenDomain(int depth, HashSet<int> possibles)
	{
		Debug.Assert(DomainSize > 0, "Domain size is 0 whilst trying to lessen domain");
		if (Domain.Depth == depth)
		{
			int oldCount = DomainSize;
			Domain.Range.IntersectWith(possibles);

			if (DomainSize == 0)
			{
				return PropagationState.Violated;
			}
			if (oldCount == DomainSize)
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

		if (lessened.Count == DomainSize)
		{
			return PropagationState.Unchanged;
		}
		Domain next = new Domain(depth, lessened);
		DomainStack.Push(next);
		Debug.Assert(DomainSize > 0, "Domain size is 0 whilst trying to finalize lessen domain");
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
		return (DomainSize == 0) ?
			DomainOperationResult.EmptyDomain :
			DomainOperationResult.FilledDomain;
	}

	public override string ToString()
	{
		return $"Slot: {DomainSize}, s: {DomainStack.Count}";
	}
}