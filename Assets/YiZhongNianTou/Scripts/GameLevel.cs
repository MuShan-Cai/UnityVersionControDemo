using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 关键字partial —— 局部类，将类或结构定义拆分为多个部分存储在不同文件中的一种方法
/// 唯一的目的是组织代码，典型的用例是将自动生成的代码与手动编写的代码分开。
/// </summary>
public partial class GameLevel : PersistableObject
{
    public int PopulationLimit
    {
        get
        {
            return populationLimit;
        }
    }

    [SerializeField]
    private SpawnZone spawnZone;

    //为了防止重构字段导致场景丢失数据，有必要告诉Unity我们希望它使用旧数据，如果它仍然存在于场景资产中。
    //通过FormerlySerializedAs属性序列化名称空间，以旧名称作为字符串参数
    //需要保留FormerlySerializedAs属性多长时间？
    //一旦确定没有旧的场景留下时，便可以删除它，需要打开场景并做一些修改之后保存，编辑器会决定是否需要重新编写场景资产文件。
    [UnityEngine.Serialization.FormerlySerializedAs("persistentObjects")]
    [SerializeField]
    private GameLevelObject[] levelObjects;

    [SerializeField]
    private int populationLimit;
    public static GameLevel Current
    {
        get;
        private set;
    }



    public void GameUpdate()
    {
        for(int i=0;i<levelObjects.Length;i++)
        {
            levelObjects[i].GameUpdate();
        }
    }

    public void SpawnShapes()
    {
        spawnZone.SpawnShapes();
    }

    public override void Save(GameDataWrite writer)
    {
        /*

         * 必须确保放入数组的内容保持在同一索引下，即不能更改持久对象在数组中的位置
         * 否则将破坏与较早保存文件的向后兼容性，因为更改持久对象的索引位置后导致加载
         * 较早保存文件时持久对象与读取到的数据不匹配。
         
         * 写入要保存的持久对象的数目
         * 因为在将来添加更多内容时，加载旧文件可以将新对象跳过，保留它们在场景中的保存方式。
        */
        writer.Write(levelObjects.Length);
        for(int i=0;i< levelObjects.Length;i++)
        {
            levelObjects[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        int savedCount = reader.ReadInt();
        for(int i=0;i<savedCount;i++)
        {
            levelObjects[i].Load(reader);
        }
    }

    private void OnEnable()
    {
        Current = this;
        if (levelObjects == null)
        {
            levelObjects = new GameLevelObject[0];
        }
    }






}
