using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscillationShapeBehavior : ShapeBehavior
{
    public Vector3 offset { get; set; }
    public float Frequency { get; set; }

    private float previousOscillation;
    public override ShapeBehaviorType BehaviorType
    {
        get
        {
            return ShapeBehaviorType.Oscillation;
        }
    }

    public override bool GameUpdate(Shape shape)
    {
        float oscillation = Mathf.Sin(2f * Mathf.PI * Frequency * shape.Age);
        shape.transform.localPosition += (oscillation - previousOscillation) * offset;
        previousOscillation = oscillation;
        return true;
    }

    public override void Load(GameDataReader reader)
    {
        offset = reader.ReadVector3();
        Frequency = reader.ReadFloat();
        previousOscillation = reader.ReadFloat();
    }

    public override void Recycle()
    {
        previousOscillation = 0;
        ShapeBehaviorPool<OscillationShapeBehavior>.Reclaim(this);
    }

    public override void Save(GameDataWrite writer)
    {
        writer.Write(offset);
        writer.Write(Frequency);
        writer.Write(previousOscillation);

    }
}
