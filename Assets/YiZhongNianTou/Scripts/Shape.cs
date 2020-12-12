using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : PersistableObject
{

    //隐式转换  将Shape转换为ShapeInstance
    public static implicit operator ShapeInstance(Shape shape)
    {
        return new ShapeInstance(shape);
    }

    public List<ShapeBehavior> behaviorList = new List<ShapeBehavior>();

    public int ColorCount
    {
        get
        {
            return colors.Length;
        }
    }

    public int ShapeId
    {
        get
        {
            return shapeId;
        }
        set
        {
            if(shapeId == int.MinValue && value != int.MinValue)
            {
                shapeId = value;
            }
            else
            {
                Debug.LogError("Not allowed to change shapeId");
            }
        }
    }

    public int SaveIndex { get; set; }

    public ShapeFactory OriginFactory
    {
        get
        {
            return originFactory;
        }
        set
        {
            if(originFactory == null)
            {
                originFactory = value;
            }
            else
            {
                Debug.LogError("Not allowed to change origin factory.");
            }
        }
    }

    public float Age { get; private set; }

    public int InstanceId { get; private set; }

    private ShapeFactory originFactory;

    public int MaterialId { get; private set; }

    public bool IsMarkedAsDying
    {
        get
        {
            return Game.Instance.IsMarkedAsDying(this);
        }
    }

    private int shapeId = int.MinValue;
    private Color color;
    [SerializeField]
    private MeshRenderer[] meshRenderers;
    private Color[] colors;

    //所有形状都是用同一个属性块，当设置渲染器属性时，复制块的内容，为所有形状不断改变相同块的颜色
    static private int colorPropertyId = Shader.PropertyToID("_Color");
    static private MaterialPropertyBlock sharedPropertyBlock;

    private void Awake()
    {
        colors = new Color[meshRenderers.Length];
    }

    public void GameUpdate()
    {
        Age += Time.deltaTime;
        for(int i=0;i<behaviorList.Count;i++)
        {
            if(!behaviorList[i].GameUpdate(this))
            {
                behaviorList[i].Recycle();
                behaviorList.RemoveAt(i--);
            }            
        }
    }

    //在未定义显示构造函数方法的情况下，类仍然具有隐式公共默认构造函数方法
    //但这不能保证它们一定存在，因此必须通过明确要求存在不带参数的构造函数方法来进一步
    //限制模板类型，因此将new()添加到T的约束列表中
    public T AddBehavior<T>() where T : ShapeBehavior,new()
    {
        T behavior = ShapeBehaviorPool<T>.Get();
        behaviorList.Add(behavior);
        return behavior;
    }

    public void SetMaterial(Material material,int materialId)
    {
        for(int i=0;i<meshRenderers.Length;i++)
        {
            meshRenderers[i].material = material;
        }
        MaterialId = materialId;
    }

    public void SetColor(Color color)
    {
        //meshRenderer.material.color = color;
        if (sharedPropertyBlock == null)
        {
            sharedPropertyBlock = new MaterialPropertyBlock();
        }
        sharedPropertyBlock.SetColor(colorPropertyId, color);
        for(int i=0;i<meshRenderers.Length;i++)
        {
            colors[i] = color;
            meshRenderers[i].SetPropertyBlock(sharedPropertyBlock);
        }
    }

    public void SetColor(Color color,int index)
    {
        if (sharedPropertyBlock == null)
        {
            sharedPropertyBlock = new MaterialPropertyBlock();
        }
        sharedPropertyBlock.SetColor(colorPropertyId, color);
        colors[index] = color;
        meshRenderers[index].SetPropertyBlock(sharedPropertyBlock); 
    }

    public override void Save(GameDataWrite writer)
    {
        base.Save(writer);
        //存储保存的颜色数量来兼容旧版本的保存文件
        writer.Write(colors.Length);
        for(int i=0;i<colors.Length;i++)
        {
            writer.Write(colors[i]);
        }
        writer.Write(Age);
        writer.Write(behaviorList.Count);
        for(int i=0;i<behaviorList.Count;i++)
        {
            writer.Write((int)behaviorList[i].BehaviorType);
            behaviorList[i].Save(writer);
        }        
    }

    public override void Load(GameDataReader reader)
    {
        base.Load(reader);
        if(reader.Version >= 5)
        {
            LoadColors(reader);
        }
        else
        {
            SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
        }

        if(reader.Version >= 6)
        {
            Age = reader.ReadFloat();
            int behaviorCount = reader.ReadInt();
            for(int i=0;i<behaviorCount;i++)
            {
                ShapeBehavior behavior = ((ShapeBehaviorType)reader.ReadInt()).GetInstance();
                behaviorList.Add(behavior);
                behavior.Load(reader);
            }
        }
        else if(reader.Version >= 4)
        {
            AddBehavior<RotationShapeBehavior>().AngularVelocity = reader.ReadVector3();
            AddBehavior<MovementShapeBehavior>().Velocity = reader.ReadVector3();
        }

    }

    void LoadColors(GameDataReader reader)
    {
        int count = reader.ReadInt();
        int max = count <= colors.Length ? count : colors.Length;
        int i = 0;
        for(;i<max;i++)
        {
            SetColor(reader.ReadColor(), i);
        }
        //存储的颜色超过了当前的需要，文件保存了更多的颜色，需要将它们读取掉
        if(count > colors.Length)
        {
            for(;i<count;i++)
            {
                reader.ReadColor();
            }
        }
        //存储的颜色少于当前需要的颜色，已读取完所有可用的数据，但仍然需要设置颜色
        //将其余颜色设置为白色
        else if(count < colors.Length)
        {
            for(;i<colors.Length;i++)
            {
                SetColor(Color.white, i);
            }
        }
    }

    public void Recycle()
    {
        Age = 0f;
        InstanceId += 1;
        for(int i=0;i<behaviorList.Count;i++)
        {
            behaviorList[i].Recycle();
        }
        behaviorList.Clear();
        originFactory.Reclaim(this);
    }

    public void ResolveShapeInstances()
    {
        for(int i=0;i<behaviorList.Count;i++)
        {
            behaviorList[i].ResolveShapeInstances();
        }
    }

    public void Die()
    {
        Game.Instance.Kill(this);
    }

    public void MarkAsDying()
    {
        Game.Instance.MarkAsDying(this);
    }
}
