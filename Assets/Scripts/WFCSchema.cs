using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WFCSchema : MonoBehaviour
{
    // Start is called before the first frame update
    public bool[] editorCollapsed = new bool[6];

    [SerializeField]
    public List<SchemaConnection>[] connections;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        foreach (var direction in SlotDirection.Directions)
        {
            var pos = transform.TransformPoint(SlotDirection.Transforms[direction]);
            Handles.Label(pos, $"{SlotDirection.Names[direction]}");
        }

    }
}

[Serializable]
public class SchemaConnection
{
    public int Connector;
    public bool Flipped;
    public bool Symmetric;
}