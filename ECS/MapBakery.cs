using UnityEngine;
using Unity.Entities;
using UnityEngine.Tilemaps;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

public static class MapBakery
{
    private static readonly int2[] OddNeighbors =
    {
        new(+1, 0), new(+1, +1), new( 0, +1),
        new(-1, 0), new( 0, -1), new(+1, -1)
    };
    
    private static readonly int2[] EvenNeighbors =
    {
        new(+1, 0), new( 0, +1), new(-1, +1),
        new(-1, 0), new(-1, -1), new( 0, -1)
    };

    public static void BakeMap(EntityManager manager, Tilemap map, RuleTileExtension[] ruleTiles)
    {
        var entities = new List<Entity>();
        var mapLookup = new NativeParallelHashMap<int2, Entity>(map.cellBounds.size.x * map.cellBounds.size.y, Allocator.Persistent);

        foreach (var hex in map.cellBounds.allPositionsWithin)
        {
            if (!map.HasTile(hex))
                continue;

            var tile = map.GetTile(hex);
            
            foreach (var ruleTile in ruleTiles)
            {
                if (tile != ruleTile)
                    continue;
                
                var entity = manager.CreateEntity(
                    typeof(HexData),
                    typeof(HexNeighborElement),
                    typeof(HexSelected)
                );
                
                manager.SetComponentEnabled<HexSelected>(entity, false);
                
                var position = new int2(hex.x, hex.y);
                var center = map.GetCellCenterWorld(hex);
                
                manager.SetComponentData(entity, new HexData
                {
                    TerrainType = ruleTile.terrainType,
                    Height = ruleTile.height,
                    Position = position,
                    Center = new float2(center.x, center.y),
                });
                
                entities.Add(entity);
                mapLookup.TryAdd(position, entity);
                
                break;
            }
        }

        var lookup = manager.CreateEntity(
            typeof(MapLookup));
        
        manager.SetComponentData(lookup, new MapLookup
        {
            Lookup = mapLookup
        });
        
        foreach (var entity in entities)
        {
            var data = manager.GetComponentData<HexData>(entity);
            
            var offsets = (data.Position.y & 1) == 0 
                ? EvenNeighbors 
                : OddNeighbors;
            
            var neighbors =  manager.GetBuffer<HexNeighborElement>(entity);

            foreach (var offset in offsets)
            {
                var neighborPos = data.Position + offset;

                if (!mapLookup.TryGetValue(neighborPos, out var neighbor))
                    continue;
                
                var neighborData = manager.GetComponentData<HexData>(neighbor);
                
                neighbors.Add(new HexNeighborElement
                {
                    Entity = neighbor,
                    IsPassable = math.abs(data.Height - neighborData.Height) <= 1
                });
            }
        }
    }
}
