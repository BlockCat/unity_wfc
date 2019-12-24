/*
  Copyright © Iain McDonald 2010-2019
  
  This file is part of Decider.
*/
using Decider.Csp.Integer;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Decider.Csp.BaseTypes
{
	public class ConstrainedArray2D : List<ConstrainedArray>
	{
		private VariableInteger Index { get; set; }

		public MetaExpressionInteger this[VariableInteger index] {
			get {
				Index = index;
				UnityEngine.Debug.Log($"{Index}");
				return new MetaExpressionInteger(GetVariableInteger(), this.Evaluate, this.EvaluateBounds, this.Propagator, new[] { Index });


			}
		}

		public ConstrainedArray2D(IEnumerable<IEnumerable<int>> elements)
		{
			this.AddRange(elements.Select(x =>
			{
				var index = new VariableInteger($"2d-{this.ToString()}-{x}", Enumerable.Range(0, x.Count()).ToList());
				return new ConstrainedArray(x, index);
			}));
		}

		// VariableInteger with domain all possible numbers in the 2d array
		private VariableInteger GetVariableInteger()
		{
			return new VariableInteger(Index.Name + this.ToString(), Elements());
		}

		// All domains combined
		private List<int> Elements()
		{
			return Enumerable.Range(Index.Domain.LowerBound, Index.Domain.UpperBound - Index.Domain.LowerBound + 1)
				.Where(i => Index.Domain.Contains(i))
				.SelectMany(i => this[i].Elements())
				.ToList();
		}

		private int Evaluate(ExpressionInteger left, ExpressionInteger right)
		{
			return this[Index.Value].Evaluate(left, right);
		}

		private Bounds<int> EvaluateBounds(ExpressionInteger left, ExpressionInteger right)
		{
			var elements = Elements();

			return new Bounds<int>(elements.Min(), elements.Max());
		}

		// propagate,
		private ConstraintOperationResult Propagator(ExpressionInteger left, ExpressionInteger right, Bounds<int> enforce)
		{
			var result = ConstraintOperationResult.Undecided;
			var elements = Elements();

			// TODO: I think this can go wrong
			if (enforce.UpperBound < elements.Min() || enforce.LowerBound > elements.Max())
				return ConstraintOperationResult.Violated;

			// Get the things to be removed.

			// uuuuuh... I guess Index needs to remove for every domain that contains no elements...
			var remove = this.Select((x, index) => (x.Propagator(left, right, enforce) == ConstraintOperationResult.Violated, index)).Where((x, index) =>
			{
				Debug.Assert(index == x.Item2);
				return x.Item1;
			});
			/*var remove = elements.
				TakeWhile(v => v.Key < enforce.LowerBound).
				Select(v => v.Value).
				Concat(elements.
					Reverse().
					TakeWhile(v => v.Key > enforce.UpperBound).
					Select(v => v.Value)).
				SelectMany(i => i.ToList()).
				ToList();
				*/
			if (remove.Any())
			{
				result = ConstraintOperationResult.Propagated;

				foreach (var value in remove)
				{
					((IVariable<int>)Index).Remove(value.Item2, out DomainOperationResult domainOperation);

					if (domainOperation == DomainOperationResult.EmptyDomain)
						return ConstraintOperationResult.Violated;
				}

				left.Bounds = EvaluateBounds(left, null);
			}

			return result;
		}

	}
}
