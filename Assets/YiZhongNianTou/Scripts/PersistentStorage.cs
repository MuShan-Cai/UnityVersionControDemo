using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class PersistentStorage : MonoBehaviour
{
    string savePath;
    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "saveFile");
    }

    public void Save(PersistableObject o,int version)
    {        
        //使用using关键字将变量writer限制仅在using的代码块内可用。
        //这确保在代码执行退出该块后，无论如何都将正确处理所有writer引用。
        using (var writer = new BinaryWriter(File.Open(savePath, FileMode.Create)))
        {
            writer.Write(-version);
            o.Save(new GameDataWrite(writer));
        }
    }

    public void Load(PersistableObject o)
    {
        //不可用代码
        //using (var reader = new BinaryReader(File.Open(savePath, FileMode.Open)))
        //{
        //由于使用了using，在o.Load调用完之后会自动释放reader，之后通过协程读取关卡数据时会失败
        //    o.Load(new GameDataReader(reader, reader.ReadInt32()));
        //}

        //一次性读取整个文件对其进行缓冲，然后再从缓冲区中读取，所以不必担心释放文件，
        //只需将其全部内容存储在内存中一段时间。
        byte[] data = File.ReadAllBytes(savePath);
        var reader = new BinaryReader(new MemoryStream(data));
        o.Load(new GameDataReader(reader,-reader.ReadInt32()));
    }

}
