using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompositeSpawnZone : SpawnZone
{
    [SerializeField]
    private SpawnZone[] spawnZones;

    [SerializeField]
    private bool sequential;

    [SerializeField]
    private bool overrideConfig;

    private int nextSequentialIndex = 0;
    public override Vector3 SpawnPoint
    {
        get
        {
            int index;

            if(sequential)
            {
                index = nextSequentialIndex;
                nextSequentialIndex = (nextSequentialIndex + 1) % spawnZones.Length;
            }
            else
            {
                index = Random.Range(0, spawnZones.Length);
            }
            return spawnZones[index].SpawnPoint;
        }
    }

    public override void SpawnShapes()
    {
        if(overrideConfig)
        {
            base.SpawnShapes();
        }
        else
        {
            int index;

            if (sequential)
            {
                index = nextSequentialIndex;
                nextSequentialIndex = (nextSequentialIndex + 1) % spawnZones.Length;
            }
            else
            {
                index = Random.Range(0, spawnZones.Length);
            }
            spawnZones[index].SpawnShapes();
        }
    }

    public override void Save(GameDataWrite writer)
    {
        base.Save(writer);
        writer.Write(nextSequentialIndex);
        foreach(SpawnZone zone in spawnZones)
        {
            writer.Write(zone.transform.position);
        }
    }

    public override void Load(GameDataReader reader)
    {
        if(reader.Version >= 7)
        {
            base.Load(reader);
        }
        nextSequentialIndex = reader.ReadInt();
        for(int i=0;i<spawnZones.Length;i++)
        {
            spawnZones[i].transform.position = reader.ReadVector3();
        }
    }


}
