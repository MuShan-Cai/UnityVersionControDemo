using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ShapeBehavior
#if UNITY_EDITOR
    : ScriptableObject
#endif
{

    public abstract ShapeBehaviorType BehaviorType { get; }

    public abstract bool GameUpdate(Shape shape);

    public abstract void Save(GameDataWrite writer);

    public abstract void Load(GameDataReader reader);

    public abstract void Recycle();

    public virtual void ResolveShapeInstances() { }

#if UNITY_EDITOR
    public bool IsReclaimed { get; set; }

    private void OnEnable()
    {
        if (IsReclaimed)
        {
            Recycle();
        }
    }
#endif
}
