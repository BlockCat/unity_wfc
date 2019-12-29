using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(WFC))]
[CanEditMultipleObjects]
public class WFCEditor : Editor
{
	private WFC wfc;

	private SerializedProperty p_UpConnectors;
	private SerializedProperty p_DownConnectors;
	private SerializedProperty p_HorizontalConnectors;

	public void OnEnable()
	{
		wfc = (WFC)target;
		p_UpConnectors = serializedObject.FindProperty("UpConnector");
		p_DownConnectors = serializedObject.FindProperty("DownConnector");
		p_HorizontalConnectors = serializedObject.FindProperty("HorizontalConnector");
	}
	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		GUILayout.Label("Map settings");
		wfc.Size = EditorGUILayout.Vector3IntField("Map size", wfc.Size);
		wfc.SlotSize = EditorGUILayout.IntField("Slot size", wfc.SlotSize);


		EditorGUILayout.BeginFoldoutHeaderGroup(true, "Up");
		p_UpConnectors.arraySize = EditorGUILayout.IntField("size", p_UpConnectors.arraySize);
		for (int i = 0; i < p_UpConnectors.arraySize; i++)
		{
			EditorGUILayout.PropertyField(p_UpConnectors.GetArrayElementAtIndex(i), new GUIContent("connector"));
		}
		EditorGUILayout.EndFoldoutHeaderGroup();
		EditorGUILayout.BeginFoldoutHeaderGroup(true, "Down");
		p_DownConnectors.arraySize = EditorGUILayout.IntField("Down size", p_DownConnectors.arraySize);
		for (int i = 0; i < p_DownConnectors.arraySize; i++)
		{
			EditorGUILayout.PropertyField(p_DownConnectors.GetArrayElementAtIndex(i), new GUIContent("connector"));
		}
		EditorGUILayout.EndFoldoutHeaderGroup();
		EditorGUILayout.BeginFoldoutHeaderGroup(true, "Horizontal");
		p_HorizontalConnectors.arraySize = EditorGUILayout.IntField("Down size", p_HorizontalConnectors.arraySize);
		for (int i = 0; i < p_HorizontalConnectors.arraySize; i++)
		{
			EditorGUILayout.PropertyField(p_HorizontalConnectors.GetArrayElementAtIndex(i), new GUIContent("connector"));
		}
		EditorGUILayout.EndFoldoutHeaderGroup();

		GUILayout.Label("Prototypes");
		wfc.SceneLoader = (GameObject)EditorGUILayout.ObjectField("Prototypes", wfc.SceneLoader, typeof(GameObject), false);
		if (GUILayout.Button("Reload Prototypes"))
		{
			LoadPrototypes();
		}

		if (GUILayout.Button("Generate map"))
		{
			Debug.Log("Removing children");
			wfc.ClearChildren();
			Debug.Log("Start generate map");
			wfc.StartGeneration();
		}

		if (EditorGUI.EndChangeCheck())
		{
			serializedObject.ApplyModifiedProperties();
		}
	}

	private void LoadPrototypes()
	{
		var schemas = wfc.SceneLoader.GetComponentsInChildren<WFCSchema>();
		var prototypes = new List<WFCPrototype>();
		var counter = 0;
		// Check all directions;
		foreach (var schema in schemas)
		{
			if (schema.gameObject.activeSelf)
			{

				for (int i = 0; i < 4; i++)
				{
					var prototype = new WFCPrototype(counter++, i, false, schema);
					prototypes.Add(prototype);
					if (schema.AllowFlipped)
					{
						var flippedPrototype = new WFCPrototype(counter++, i, true, schema);
						prototypes.Add(flippedPrototype);
					}
				}
			}


		}

		// rotation 0 = 0 (forward becomes forward)
		// rotation 1 = 90 (forward becomes right)
		// rotation 2 = 180 (forward becomes back)
		// rotation 3 = 270 (forward becomes left)

		foreach (var prototype in prototypes)
		{
			var directionCollection = new[]
			{
				new List<int>(),
				new List<int>(),
				new List<int>(),
				new List<int>(),
				new List<int>(),
				new List<int>(),
			};

			void handlePrototype(WFCPrototype me, WFCPrototype other, int direction)
			{
				var m = me.GetSchemaConnectors(direction);
				var o = other.GetSchemaConnectors(SlotDirection.CounterDirections[direction]);
				if (m.Any(a => o.Any(b => SchemaConnection.Connects(a, b))))
				{
					directionCollection[direction].Add(other.Id);
				}
			}

			foreach (var o_prototype in prototypes)
			{
				var mu = prototype.GetSchemaConnectors(SlotDirection.UP);
				var md = prototype.GetSchemaConnectors(SlotDirection.DOWN);
				var ou = o_prototype.GetSchemaConnectors(SlotDirection.UP);
				var od = o_prototype.GetSchemaConnectors(SlotDirection.DOWN);

				if (mu.Select(x => x.Connector).Intersect(od.Select(x => x.Connector)).Any())
				{
					directionCollection[SlotDirection.UP].Add(o_prototype.Id);
				}
				if (md.Select(x => x.Connector).Intersect(ou.Select(x => x.Connector)).Any())
				{
					directionCollection[SlotDirection.DOWN].Add(o_prototype.Id);
				}

				handlePrototype(prototype, o_prototype, SlotDirection.LEFT);
				handlePrototype(prototype, o_prototype, SlotDirection.RIGHT);
				handlePrototype(prototype, o_prototype, SlotDirection.FORWARD);
				handlePrototype(prototype, o_prototype, SlotDirection.BACK);
			}
			foreach (var direction in SlotDirection.Directions)
			{
				prototype.Possible[direction] = directionCollection[direction].ToArray();
			}
		}

		wfc.prototypes = prototypes.ToArray();
	}
}
