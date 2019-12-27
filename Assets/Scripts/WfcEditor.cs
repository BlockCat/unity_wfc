using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(WFC))]
public class WFCEditor : Editor
{
	private WFC wfc;
	public void OnEnable()
	{
		wfc = (WFC)target;
	}
	public override void OnInspectorGUI()
	{
		GUILayout.Label("Map settings");
		wfc.Size = EditorGUILayout.Vector3IntField("Map size", wfc.Size);
		wfc.SlotSize = EditorGUILayout.IntField("Slot size", wfc.SlotSize);

		wfc.UpConnector = EditorGUILayout.IntField("Upper connectors", wfc.UpConnector);
		wfc.DownConnector = EditorGUILayout.IntField("Down connectors", wfc.DownConnector);
		wfc.HorizontalConnector = EditorGUILayout.IntField("Horizontal connectors", wfc.HorizontalConnector);

		GUILayout.Label("Prototypes");
		wfc.SceneLoader = (GameObject)EditorGUILayout.ObjectField("Prototypes", wfc.SceneLoader, typeof(GameObject), false);
		if (GUILayout.Button("Reload Prototypes"))
		{
			LoadPrototypes();
		}

		GUILayout.Label("Debug item");
		wfc.debug_spawner = (GameObject)EditorGUILayout.ObjectField("ds2", wfc.debug_spawner, typeof(GameObject), true);
		wfc.debug_spawner_2 = (GameObject)EditorGUILayout.ObjectField("ds1", wfc.debug_spawner_2, typeof(GameObject), true);

		wfc.DrawDebug = EditorGUILayout.Toggle("Debug", wfc.DrawDebug);
		if (wfc.DrawDebug)
		{
			wfc.DebugSpeed = EditorGUILayout.LongField("Debug speed", wfc.DebugSpeed);
		}
		else
		{
			wfc.DebugSpeed = 0;
		}

		if (GUILayout.Button("Generate map"))
		{
			Debug.Log("Removing children");
			wfc.ClearChildren();
			Debug.Log("Start generate map");
			wfc.StartGeneration();
		}
	}

	private void LoadPrototypes()
	{
		var schemas = wfc.SceneLoader.GetComponentsInChildren<WFCSchema>();
		var mapping = new Dictionary<WFCSchema, List<WFCPrototype>>();
		var prototypes = new List<WFCPrototype>();
		var counter = 0;
		// Check all directions;
		foreach (var schema in schemas)
		{
			List<WFCPrototype> rotations = new List<WFCPrototype>();
			for (int i = 0; i < 4; i++)
			{
				var prototype = new WFCPrototype(counter++, i, schema);
				prototypes.Add(prototype);
				rotations.Add(prototype);
			}
			mapping.Add(schema, rotations);
		}

		// rotation 0 = 0 (forward becomes forward)
		// rotation 1 = 90 (forward becomes right)
		// rotation 2 = 180 (forward becomes back)
		// rotation 3 = 270 (forward becomes left)

		foreach (var prototype in prototypes)
		{
			// Check foreach rotated prototype if connectors match
			// Handle up
			var transformed = SlotDirection.Rotation[prototype.Rotation];
			var possibleUp = prototype.Schema.connections[SlotDirection.UP].Select(x => x.Connector);
			var possibleDown = prototype.Schema.connections[SlotDirection.DOWN].Select(x => x.Connector);
			var possibleLeft = prototype.Schema.connections[transformed[SlotDirection.LEFT]];
			var possibleRight = prototype.Schema.connections[transformed[SlotDirection.RIGHT]];
			var possibleForward = prototype.Schema.connections[transformed[SlotDirection.FORWARD]];
			var possibleBack = prototype.Schema.connections[transformed[SlotDirection.BACK]];

			var directionCollection = new[]
			{
				new List<int>(),
				new List<int>(),
				new List<int>(),
				new List<int>(),
				new List<int>(),
				new List<int>(),
			};
			//  f 
			// l0r
			//  b
			//  l 
			// b1f
			//  r			
			foreach (var o_prototype in prototypes)
			{
				var o_transformed = SlotDirection.Rotation[(o_prototype.Rotation)];
				if (o_prototype.Rotation == 1)
				{
					Debug.Assert(o_transformed[SlotDirection.FORWARD] == SlotDirection.LEFT, $"Wrong Direction: f {o_transformed[SlotDirection.FORWARD]} instead of {SlotDirection.LEFT}");
				}
				
				var ou = o_prototype.Schema.connections[SlotDirection.UP].Select(x => x.Connector);
				var od = o_prototype.Schema.connections[SlotDirection.DOWN].Select(x => x.Connector);
				var ol = o_prototype.Schema.connections[o_transformed[SlotDirection.LEFT]];
				var or = o_prototype.Schema.connections[o_transformed[SlotDirection.RIGHT]];
				var of = o_prototype.Schema.connections[o_transformed[SlotDirection.FORWARD]];
				var ob = o_prototype.Schema.connections[o_transformed[SlotDirection.BACK]];

				if (possibleUp.Intersect(od).Any())
				{
					directionCollection[SlotDirection.UP].Add(o_prototype.Id);					
				}
				if (possibleDown.Intersect(ou).Any())
				{
					directionCollection[SlotDirection.DOWN].Add(o_prototype.Id);
				}

				if (possibleLeft.Any(a => or.Any(b => SchemaConnection.Connects(a, b))))
				{
					directionCollection[SlotDirection.LEFT].Add(o_prototype.Id);
				}
				if (possibleRight.Any(a => ol.Any(b => SchemaConnection.Connects(a, b))))
				{
					directionCollection[SlotDirection.RIGHT].Add(o_prototype.Id);
				}
				if (possibleForward.Any(a => ob.Any(b => SchemaConnection.Connects(a, b))))
				{
					directionCollection[SlotDirection.FORWARD].Add(o_prototype.Id);
				}
				if (possibleBack.Any(a => of.Any(b => SchemaConnection.Connects(a, b))))
				{
					directionCollection[SlotDirection.BACK].Add(o_prototype.Id);
				}
			}
			foreach (var direction in SlotDirection.Directions)
			{
				prototype.Possible[direction] = directionCollection[direction].ToArray();
			}
		}

		wfc.prototypes = prototypes.ToArray();
	}
}
