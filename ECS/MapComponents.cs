using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

public struct HexData : IComponentData
{
    public TerrainType TerrainType;
    public byte Height;
    public int2 Position;
    public float2 Center;
}

public struct MapLookup : IComponentData, IDisposable
{
    public NativeParallelHashMap<int2, Entity> Lookup;
    
    public Entity GetHex(int2 position) => Lookup[position];

    public void Dispose()
    {
        if (Lookup.IsCreated)
            Lookup.Dispose();
    }
}

[InternalBufferCapacity(6)]
public struct HexNeighborElement : IBufferElementData
{
    public Entity Entity;
    public bool IsPassable;
}

public struct HexSelected : IComponentData, IEnableableComponent {}