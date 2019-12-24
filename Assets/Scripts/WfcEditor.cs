using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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


		GUILayout.Label("Debug item");
		wfc.debug_spawner = (GameObject)EditorGUILayout.ObjectField("ds2", wfc.debug_spawner, typeof(GameObject), true);
		wfc.debug_spawner_2 = (GameObject)EditorGUILayout.ObjectField("ds1", wfc.debug_spawner_2, typeof(GameObject), true);

		if (GUILayout.Button("Generate map"))
		{
			Debug.Log("Removing children");
			wfc.ClearChildren();
			Debug.Log("Start generate map");
			wfc.StartGeneration();
			//wfc.GenerateMap();
		}
	}
}
