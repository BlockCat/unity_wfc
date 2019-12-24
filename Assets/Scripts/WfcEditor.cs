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
		wfc.debug_spawner = (GameObject)EditorGUILayout.ObjectField("es", wfc.debug_spawner, typeof(GameObject), true);

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
