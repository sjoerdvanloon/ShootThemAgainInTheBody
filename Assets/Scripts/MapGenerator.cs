using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Transform TilePrefab;
    public Vector2 MapSize;
    [Range(0, 1)]
    public float OutlinePercent;

    const string HOLDER_NAME = "Generated Map";

    void Start()
    {
        GeneratorMap();
    }

    public void GeneratorMap()
    {

        Transform mapHolder = transform.Find(HOLDER_NAME); // FindChild was gone
        if (mapHolder)
        {
            DestroyImmediate(mapHolder.gameObject); // DestroyImmediate because we want to use it from the editor (instead of Destroy)
        }

        mapHolder = new GameObject(HOLDER_NAME).transform;
        mapHolder.parent = transform;
        
        for (int x = 0; x < MapSize.x; x++)
        {
            for (int y = 0; y < MapSize.y; y++)
            {
                Vector3 tilePosition = new Vector3(-MapSize.x / 2 + 0.5f + x, 0, -MapSize.y/2 + 0.5f + y);

                Transform newTile = Instantiate(TilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - OutlinePercent);
                newTile.parent = mapHolder;
            }
        }
    }
}
