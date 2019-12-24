using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class Slot
{
	public HashSet<WFCPrototype> PossiblePrototypes;
	public Slot(IEnumerable<WFCPrototype> possibleProtoTypes)
	{
		this.PossiblePrototypes = new HashSet<WFCPrototype>(possibleProtoTypes);
	}

}

