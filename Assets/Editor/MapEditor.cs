using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(MapGenerator))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var updated = DrawDefaultInspector();

            MapGenerator mapGenerator = target as MapGenerator;
        if (updated)
        {
            mapGenerator.GeneratorMap();
        }

        if (GUILayout.Button("Generate map"))
        {
            mapGenerator.GeneratorMap();
        }
    }
}
