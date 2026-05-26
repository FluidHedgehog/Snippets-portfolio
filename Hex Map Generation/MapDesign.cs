using UnityEngine;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine.Tilemaps;
using Unity.Mathematics;
using UnityEditorInternal;
using System.Collections.Generic;

public class MapDesign : EditorWindow
{
    [MenuItem("Tools/Map Design")]
    private static void CreateMap() => GetWindow<MapDesign>("MapDesign");

    public Tilemap tilemap;
    public MapTerrainData data;
    
    //Generator
    public Vector3Int mapSize;
    public Vector2 offset;
    public Vector2 scale;
    public bool randomOffset;
    public bool randomScale;
    
    public RuleTileExtension defaultTerrain;
    public RuleTileExtension[] terrains;
    public Vector2[] vectors;
    
    //Bakery
    public List<TileData> terrainDatas = new();

    public bool3 mode;
    public bool usePerlin;
    
    public SerializedObject terrainDatasObject;
    public SerializedProperty terrainDatasProperty;

    public SerializedObject terrainsObject;
    public SerializedProperty terrainsProperty;

    public SerializedObject vectorObject;
    public SerializedProperty vectorProperty;

    void OnEnable()
    { 
        terrainDatasObject = new SerializedObject(this);
        terrainDatasProperty = terrainDatasObject.FindProperty("terrainDatas");
        
        terrainsObject = new SerializedObject(this);
        terrainsProperty = terrainsObject.FindProperty("terrains");
        
        vectorObject = new SerializedObject(this);
        vectorProperty = vectorObject.FindProperty("vectors");
    }
    
    private void OnGUI()
    {
        tilemap = (Tilemap)EditorGUILayout.ObjectField("Tilemap", tilemap, typeof(Tilemap), true);
        data = (MapTerrainData)EditorGUILayout.ObjectField("Data", data, typeof(MapTerrainData), false);
        
        EditorGUILayout.LabelField("Select Mode", EditorStyles.largeLabel);

        mode.x = EditorGUILayout.Toggle("Generator",  mode.x);
        
        mode.y = EditorGUILayout.Toggle( "Bakery", mode.y);

        if (mode.x)
        {
            EditorGUILayout.LabelField("Generator!", EditorStyles.largeLabel);
            usePerlin = EditorGUILayout.Toggle("Perlin?", usePerlin);
            
            if (usePerlin)
            {
                randomOffset = EditorGUILayout.Toggle("Random Offset?", randomOffset);
                randomScale = EditorGUILayout.Toggle("Random Scale?", randomScale);
                
                offset = EditorGUILayout.Vector2Field("Offset", offset);
                scale = EditorGUILayout.Vector2Field("Scale", scale);
                
                EditorGUILayout.PropertyField(terrainsProperty, true);
                terrainsObject.ApplyModifiedProperties();
                
                EditorGUILayout.PropertyField(vectorProperty, true);
                vectorObject.ApplyModifiedProperties();
            }
            
            if (GUILayout.Button("Clear Map"))
                tilemap.ClearAllTiles();
            
            defaultTerrain = (RuleTileExtension)EditorGUILayout.ObjectField(defaultTerrain, typeof(RuleTileExtension), false);
            mapSize =  EditorGUILayout.Vector3IntField("Map Size", mapSize);

            if (GUILayout.Button("Generate Map"))
                GenerateMap();
        }

        if (mode.y)
        {
            EditorGUILayout.LabelField("Bakery!", EditorStyles.largeLabel);
            
            EditorGUILayout.PropertyField(terrainDatasProperty, true);
            terrainDatasObject.ApplyModifiedProperties();
        
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Bakery", EditorStyles.largeLabel);
            if (GUILayout.Button("Bake Terrain"))
                data.BakeMe(tilemap, terrainDatas);
        }
    }


    private void GenerateMap()
    {
        UnityEngine.Assertions.Assert.IsNotNull(tilemap);
        UnityEngine.Assertions.Assert.IsNotNull(defaultTerrain);
        
        BoundsInt bounds = new BoundsInt(new Vector3Int(-mapSize.x/2,-mapSize.y/2,0), mapSize);
        
        Dictionary<Vector3Int, RuleTileExtension> tilemapGeneration = new();
        
        if (usePerlin)
        {
            if (randomOffset)
            {
                offset.x = UnityEngine.Random.Range(0f, 9999f);
                offset.y = UnityEngine.Random.Range(0f, 9999f);
            }

            if (randomScale)
            {
                var scaleVal = UnityEngine.Random.Range(0f, 0.99f);
                
                scale.x = scaleVal;
                scale.y = scaleVal;
            }
            
            foreach (var pos in bounds.allPositionsWithin)
            {
                var perlinValue = Mathf.PerlinNoise(
                    pos.x * scale.x + offset.x, 
                    pos.y * scale.y + offset.y
                    );
                
                for (int i = 0; i < vectors.Length; i++)
                    if (vectors[i].x <= perlinValue && vectors[i].y >= perlinValue)
                    {
                        if (tilemapGeneration.ContainsKey(pos)) continue;
                            tilemapGeneration[pos] = terrains[i];
                    }
                        
            }
        }
        else
            foreach (var pos in bounds.allPositionsWithin)
                tilemapGeneration[pos] = defaultTerrain;
        
        foreach (var tile in tilemapGeneration.Keys)
            tilemap.SetTile(tile, tilemapGeneration[tile]);
    }
}
