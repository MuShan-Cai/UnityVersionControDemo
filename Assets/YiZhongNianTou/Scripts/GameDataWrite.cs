using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class GameDataWrite
{
    BinaryWriter writer;

    public GameDataWrite (BinaryWriter writer)
    {
        this.writer = writer;
    }

    public void Write(int value)
    {
        writer.Write(value);
    }

    public void Write(float value)
    {
        writer.Write(value);
    }

    public void Write(Quaternion value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
        writer.Write(value.w);
    }

    public void Write(Vector3 value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
    }

    public void Write(Color value)
    {
        writer.Write(value.r);
        writer.Write(value.g);
        writer.Write(value.b);
        writer.Write(value.a);
    }

    //用于写入随机状态
    public void Write(Random.State value)
    {
        //Random.State是可序列化类型，ToJson方法可将其转换为相同数据的字符串表示形式。
        writer.Write(JsonUtility.ToJson(value));
    }

    public void Write(ShapeInstance value)
    {
        writer.Write(value.Shape.SaveIndex);
    }

}
