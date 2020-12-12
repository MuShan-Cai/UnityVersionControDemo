using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 工厂的唯一责任是交付形状实例。它不需要位置旋转或缩放，也不需要Update方法来更改状态。
/// 因此它不必是组件，不需要附加到游戏对象上，它可以单独存在，不是作为特定场景的一部分，
/// 而是作为项目的一部分，它是一种资产，因此继承自ScriptableObject 
/// </summary>
[CreateAssetMenu]
public class ShapeFactory : ScriptableObject
{
    [SerializeField]
    Shape[] prefabs;

    [SerializeField]
    Material[] materials;

    [SerializeField]
    bool recycle;

    //Unity在重新编译后不会对 ScriptableObject 类型的私有字段进行序列化
    //故在重新编译后池列表pools将丢失
    List<Shape>[] pools;

    //池场景保存所有可以回收的形状实例
    Scene poolScene;

    public int FactoryId
    {
        get
        {
            return factoryId;
        }
        set
        {
            if(factoryId == int.MinValue && value != int.MinValue)
            {
                factoryId = value;
            }
            else
            {
                Debug.LogError("Not allowed to change factoryId.");
            }
        }

    }
    //为什么不能对factoryId进行序列化？
    /*Unity不会保存未标记为序列化的可编写脚本对象的私有字段。
    但是，可编写脚本的对象实例本身可以在单个编辑器会话期间的播放会话之间保留下来。
    只要打开编辑器，私有字段的值就会保留，但是下次你打开Unity编辑器时，私有字段的值将被重置。
    通过复制创建新的工厂资产时，这会造成混乱并混淆对象，因此最好确保该字段永不持久。这确实意味着在热重载
    （播放模式下的重新编译）期间数据也会丢失*/
    [System.NonSerialized]
    private int factoryId = int.MinValue;

    public Shape Get(int shapeId = 0,int materialId = 0)
    {
        Shape instance;
        if (recycle) 
        {
            if(pools == null)
            {
                CreatePools();
            }
            List<Shape> pool = pools[shapeId];
            int lastIndex = pool.Count - 1;
            if (lastIndex >= 0)
            {
                instance = pool[lastIndex];
                instance.gameObject.SetActive(true);
                pool.RemoveAt(lastIndex);
            }
            else
            {
                instance = Instantiate(prefabs[shapeId]);
                instance.ShapeId = shapeId;
                instance.OriginFactory = this;
                SceneManager.MoveGameObjectToScene(instance.gameObject, poolScene);
            }
        }
        else
        {
            instance = Instantiate(prefabs[shapeId]);
            instance.ShapeId = shapeId;
        }
        instance.SetMaterial(materials[materialId], materialId);
        Game.Instance.AddShape(instance);
        return instance;
    }

    public void Reclaim(Shape shapeToRecycle)
    {
        if(shapeToRecycle.OriginFactory != this)
        {
            Debug.LogError("Tried to reclaim shape with wrong factory.");
            return;
        }

        if(recycle)
        {
            if(pools == null)
            {
                CreatePools();
            }
            pools[shapeToRecycle.ShapeId].Add(shapeToRecycle);
            shapeToRecycle.gameObject.SetActive(false);
        }
        else
        {
            Destroy(shapeToRecycle.gameObject);
        }
    }

    public Shape GetRandom()
    {
        return Get(
            Random.Range(0, prefabs.Length),
            Random.Range(0,materials.Length)
            );
    }

    void CreatePools()
    {
        pools = new List<Shape>[prefabs.Length];
        for(int i=0;i<pools.Length;i++)
        {
            pools[i] = new List<Shape>();
        }
        //重编译出现在编辑器模式下，判断是否处于编辑器模式
        if(Application.isEditor)
        {
            //重编译后池列表丢失将导致重复加载池场景，故加载前先判断池场景是否已存在
            //重编译后poolScene作为一个不可序列化的结构体将被重置为默认值表示一个已卸载的场景，
            //而不是对实际场景的直接引用，因此需要使用GetSceneByName()请求一个新的连接
            poolScene = SceneManager.GetSceneByName(name);
            if (poolScene.isLoaded)
            {
                //因为重编译丢失了非活动状态下的形状列表，其中的形状实例将永远不会被重用
                //故需要重新填充列表 
                
                GameObject[] rootObjects = poolScene.GetRootGameObjects();//获得池场景中所有根游戏对象
                for(int i=0;i<rootObjects.Length;i++)
                {
                    Shape pooledShape = rootObjects[i].GetComponent<Shape>();
                    if(!pooledShape.gameObject.activeSelf)//检查是否处于活动状态，由于是根对象不需要使用activeInHierarchy
                    {
                        pools[pooledShape.ShapeId].Add(pooledShape);//重新添加回池列表
                    }
                }
                return;
            }
        } 
        //创建池场景，使用工厂名称作为场景名称
        poolScene = SceneManager.CreateScene(name);
    }
}
