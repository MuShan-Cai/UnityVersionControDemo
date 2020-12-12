[System.Serializable]
public struct ShapeInstance
{
    public Shape Shape { get; private set; }

    private int instanceIdOrSaveIndex;

    public ShapeInstance(Shape shape)
    {
        Shape = shape;
        instanceIdOrSaveIndex = shape.InstanceId;
    }

    public ShapeInstance(int saveIndex)
    {
        Shape = null;
        instanceIdOrSaveIndex = saveIndex;
    }

    public bool IsValid
    {
        //由于结构体存在一个默认的构造函数，例如在创建ShapeInstance数组时将使用，这将导致
        //Shape为空引用，因此应检查Shape是否有形状引用
        get { return Shape && instanceIdOrSaveIndex == Shape.InstanceId; }
    }

    public void Resolve()
    {
        Shape = Game.Instance.GetShape(instanceIdOrSaveIndex);
        instanceIdOrSaveIndex = Shape.InstanceId;
    }

}