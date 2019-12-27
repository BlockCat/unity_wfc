using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class WFCSchema : MonoBehaviour
{
	// Start is called before the first frame update
	public bool[] editorCollapsed = new bool[6];


	public SchemaConnection[][] connections => new[] { up, down, left, right, back, forward };

	[SerializeField]
	public SchemaConnection[] up;
	public SchemaConnection[] down;
	public SchemaConnection[] left;
	public SchemaConnection[] right;
	public SchemaConnection[] back;
	public SchemaConnection[] forward;

	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	private void OnDrawGizmos()
	{
		var colours = new[] { Color.green, Color.green, Color.blue, Color.blue, Color.red, Color.red };
		foreach (var direction in SlotDirection.Directions)
		{
			var pos = transform.TransformPoint(SlotDirection.Transforms[direction] * 0.2f);
			var name = SlotDirection.Names[direction];
			var k = string.Concat(connections[direction].Select(x => x.Connector + (x.Flipped ? "f" : "") + (x.Symmetric ? "s," : ",")));

			Handles.color = colours[direction];
			Handles.Label(pos, $"{name}\n[{k}]");
		}

	}
}

[Serializable]
public class SchemaConnection
{
	public int Connector;
	public bool Flipped;
	public bool Symmetric;

	public static bool Connects(SchemaConnection a, SchemaConnection b)
	{
		if (a.Connector != b.Connector)
		{
			return false;
		}

		return a.Symmetric || b.Symmetric || (a.Flipped != b.Flipped);
	}
}