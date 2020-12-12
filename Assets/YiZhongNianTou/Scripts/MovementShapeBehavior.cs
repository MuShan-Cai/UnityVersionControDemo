using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//为了防止扩展了本类的其他类型将实例添加到ShapeBehaviorPool池中
//而不是自身的池中，添加sealed关键字来使其他类型无法继承自本类；
public sealed class MovementShapeBehavior : ShapeBehavior
{

    public override ShapeBehaviorType BehaviorType
    {
        get
        {
            return ShapeBehaviorType.Movement;
        }
    }


    public Vector3 Velocity
    {
        get;
        set;
    }

    public override bool GameUpdate(Shape shape)
    {
        shape.transform.localPosition += Velocity * Time.deltaTime;
        return true;

    }

    public override void Save(GameDataWrite writer)
    {
        writer.Write(Velocity);
    }

    public override void Load(GameDataReader reader)
    {
        Velocity = reader.ReadVector3();
    }

    public override void Recycle()
    {
        ShapeBehaviorPool<MovementShapeBehavior>.Reclaim(this);
    }

}
