
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WFCSchema))]
public class WFCSchemaEditor : Editor
{

    private WFCSchema prototype;

    public void OnEnable()
    {
        prototype = (WFCSchema)target;

        if (prototype.connections == null)
        {
            prototype.connections = new List<SchemaConnection>[] {
                new List<SchemaConnection>(),
                new List<SchemaConnection>(),
                new List<SchemaConnection>(),
                new List<SchemaConnection>(),
                new List<SchemaConnection>(),
                new List<SchemaConnection>()
            };            
        }
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        foreach (var direction in SlotDirection.Directions)
        {
            prototype.editorCollapsed[direction] = EditorGUILayout.Foldout(prototype.editorCollapsed[direction], $"{SlotDirection.Names[direction]} - {direction} - {prototype.connections[direction].Count}", true);

            if (prototype.editorCollapsed[direction])
            {
                if (GUILayout.Button("Add"))
                {
                    prototype.connections[direction].Add(new SchemaConnection());
                }
                // connectors
                List<SchemaConnection> toRemove = new List<SchemaConnection>();
                foreach (var x in prototype.connections[direction])
                {
                    EditorGUILayout.BeginHorizontal();
                    x.Connector = EditorGUILayout.IntField(x.Connector);
                    if (GUILayout.Button($"Flip: {x.Flipped}"))
                    {
                        x.Flipped = !x.Flipped;
                    }
                    if (GUILayout.Button($"Symmetric: {x.Symmetric}"))
                    {
                        x.Symmetric = !x.Symmetric;
                    }
                    if (GUILayout.Button("x"))
                    {
                        toRemove.Add(x);
                    }

                    EditorGUILayout.EndHorizontal();
                }
                foreach (var remove in toRemove)
                {
                    prototype.connections[direction].Remove(remove);
                }
            }
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
            PrefabUtility.SavePrefabAsset(this.prototype.transform.parent.gameObject);
        }
    }
}
