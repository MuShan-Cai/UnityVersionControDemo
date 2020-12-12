public enum ShapeBehaviorType
{
    Movement,
    Rotation,
    Oscillation,
    Satellite,
    Growing,
    Dying,
    Lifecycle
}

public static class ShapeBehaviorTypeMethods
{
    /// <summary>
    /// ShapeBehaviorType的扩展方法
    /// 扩展方法是静态类中的静态方法，行为类似于某种类型的实例方法，该类型可以是类、接口、结构、原始值或枚举。
    /// 扩展方法的第一个参数定义了该方法将要操作的类型和实例值。    
    /// 
    /// 要将某静态类的静态方法转换为扩展方法，只需在该静态方法的第一个参数前添加this关键字
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static ShapeBehavior GetInstance(this ShapeBehaviorType type)
    {
        switch (type)
        {
            case ShapeBehaviorType.Movement:
                return ShapeBehaviorPool<MovementShapeBehavior>.Get();
            case ShapeBehaviorType.Rotation:
                return ShapeBehaviorPool<RotationShapeBehavior>.Get();
            case ShapeBehaviorType.Oscillation:
                return ShapeBehaviorPool<OscillationShapeBehavior>.Get();
            case ShapeBehaviorType.Satellite:
                return ShapeBehaviorPool<SatelliteShapeBehavior>.Get();
            case ShapeBehaviorType.Growing:
                return ShapeBehaviorPool<GrowingShapeBehavior>.Get();
            case ShapeBehaviorType.Dying:
                return ShapeBehaviorPool<DyingShapeBehavior>.Get();
            case ShapeBehaviorType.Lifecycle:
                return ShapeBehaviorPool<LifecycleShapeBehavior>.Get();
        }
        UnityEngine.Debug.LogError("Forgot to support" + type);
        return null;
    }
}
