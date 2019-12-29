using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WFCPrototype
{

	public int Id { get; }
	public int Rotation { get; }
	public bool Flipped { get; }
	public WFCSchema Schema { get; }


	public SchemaConnection[] GetSchemaConnectors(int direction)
	{
		if (Flipped)
		{
			var flipped_transform = SlotDirection.Flipped;
			var rotation_transform = SlotDirection.Rotation[Rotation];
			return Schema.connections[flipped_transform[rotation_transform[direction]]];
		}
		else
		{
			return Schema.connections[SlotDirection.Rotation[Rotation][direction]];
		}
	}

	public int[][] Possible = new int[6][];

	public WFCPrototype(int id, int rotation, bool flipped, WFCSchema schema)
	{
		this.Id = id;
		this.Schema = schema;
		this.Rotation = rotation;
		this.Flipped = flipped;

	}
	public int[] GetPossible(int direction)
	{
		return Possible[direction];
	}

	public Quaternion GetRotation() => Quaternion.Euler(0, Rotation * 90, 0);
	public Vector3 GetScale() => Flipped ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
	public GameObject GetGameObjectPrototype() => Schema.gameObject;

}

