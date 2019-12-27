
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(WFCSchema))]
public class WFCSchemaEditor : Editor
{

	private WFCSchema prototype;

	private SerializedProperty[] p_directions;

	public void OnEnable()
	{
		prototype = (WFCSchema)target;
		p_directions = new[] {
			serializedObject.FindProperty("up"),
			serializedObject.FindProperty("down"),
			serializedObject.FindProperty("left"),
			serializedObject.FindProperty("right"),
			serializedObject.FindProperty("back"),
			serializedObject.FindProperty("forward")
		};
	}


	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		GUIStyle s = new GUIStyle();
		s.alignment = TextAnchor.MiddleLeft;
		foreach (var direction in SlotDirection.Directions)
		{

			EditorGUILayout.BeginFoldoutHeaderGroup(true, SlotDirection.Names[direction]);
			p_directions[direction].arraySize = EditorGUILayout.IntField("Size", p_directions[direction].arraySize);
			for (int i = 0; i < p_directions[direction].arraySize; i++)
			{
				var d = p_directions[direction].GetArrayElementAtIndex(i);
				var p_connector = d.FindPropertyRelative("Connector");
				var p_flipper = d.FindPropertyRelative("Flipped");
				var p_symmetric = d.FindPropertyRelative("Symmetric");
				
				EditorGUILayout.PropertyField(p_connector, new GUIContent("C"));
				EditorGUILayout.PropertyField(p_flipper, new GUIContent("F"));
				EditorGUILayout.PropertyField(p_symmetric, new GUIContent("S"));
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button("Layout"))
		{
			var components = this.prototype.transform.parent.gameObject.GetComponentsInChildren<WFCSchema>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].gameObject.transform.position = new Vector3(0, 0, i * 1.5f);
			}
		}
		EditorGUILayout.EndHorizontal();

		if (EditorGUI.EndChangeCheck())
		{
			Debug.Log("Changes made");
			serializedObject.ApplyModifiedProperties();
			//PrefabUtility.SavePrefabAsset(this.prototype.transform.parent.gameObject);
		}
	}
}
