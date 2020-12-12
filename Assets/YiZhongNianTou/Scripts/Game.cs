using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class Game : PersistableObject
{
    public static Game Instance { get; private set; }
    
    //const将一个简单值声明为常量，而不是字段，它不能被改变，也不存在与内存中。
    //相反它只是代码的一部分，它的显示值在编译的过程中被引用和替换。
    const int saveVersion = 7;
    [SerializeField] private ShapeFactory[] shapeFactories;
    [SerializeField] private PersistentStorage storage;
    [SerializeField] private int levelCount;

    public KeyCode createKey = KeyCode.C;
    public KeyCode newGameKey = KeyCode.N;
    public KeyCode saveKey = KeyCode.S;
    public KeyCode loadKey = KeyCode.L;
    public KeyCode destroyKey = KeyCode.X;    

    public float CreationSpeed { get; set; }
    public float DestructionSpeed  { get; set; }

    private List<Shape> shapes;
    private List<ShapeInstance> killList,markAsDyingList;
    private string savePath;
    private float creationProgress;
    private float destructionProgress;
    private int loadedLevelBuildIndex;
    private int dyingShapeCount;

    private bool inGameUpdateLoop;
    private Random.State mainRandomState;
    [SerializeField] bool reseedOnLoad;
    [SerializeField] Slider creationSpeedSlider;
    [SerializeField] Slider destructionSpeedSlider;
    [SerializeField] float destroyDuration;
    private void OnEnable()
    {
        //static属性不会在编辑器中处于播放模式的编译之间持久存在，因为它不是Unity游戏状态的一部分。
        //为了从重新编译中恢复过来，我们也可以在OnEnable方法中设置该属性。每次启用组件时，
        //Unity都会调用该方法，每次重新编译后也会发生这种情况。
        Instance = this;
        
        
        //使用OnEnable以便在热重载后重新生成id，但是在游戏加载完成后，
        //也会调用OnEnable，在这种情况下不应重新分配ID，通过检查第一个ID是否正确设置
        if(shapeFactories[0].FactoryId != 0)
        {
            for(int i=0;i<shapeFactories.Length;i++)
            {
                shapeFactories[i].FactoryId = i;
            }
        }
    }

    private void Start()
    {
        mainRandomState = Random.state;
        shapes = new List<Shape>();
        killList = new List<ShapeInstance>();
        markAsDyingList = new List<ShapeInstance>();
        if (Application.isEditor)
        {
            //检查已加载的所有场景中是否加载了名称包含"Level "字符串的场景
            //避免同时加载一个以上的关卡
            for(int i=0;i<SceneManager.sceneCount;i++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(i);
                if(loadedScene.name.Contains("Level "))
                {
                    SceneManager.SetActiveScene(loadedScene);
                    loadedLevelBuildIndex = loadedScene.buildIndex;
                    return;
                }
            }
        }
        BeginNewGame();
        StartCoroutine(LoadLevel(1));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(createKey))
        {
            GameLevel.Current.SpawnShapes();

        }
        else if(Input.GetKeyDown(newGameKey))
        {
            BeginNewGame();
            StartCoroutine(LoadLevel(loadedLevelBuildIndex));
        }else if(Input.GetKeyDown(saveKey))
        {
            storage.Save(this,saveVersion);
        }else if(Input.GetKeyDown(loadKey))
        {
            BeginNewGame();
            storage.Load(this);
        }
        else if(Input.GetKeyDown(destroyKey))
        {
            DestroyShape();
        }
        else
        {
            for(int i=1;i<=levelCount;i++)
            {
                //检查玩家是否按下数字键从而加载相应关卡
                if(Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    BeginNewGame();
                    StartCoroutine(LoadLevel(i));
                    return;     //开始加载之后跳过剩余的update方法
                }
            }
        }
    }

    private void FixedUpdate()
    {
        //更新场景中的shape以及关卡内容
        inGameUpdateLoop = true;
        for (int i=0;i<shapes.Count;i++)
        {
            //更新形状
            shapes[i].GameUpdate();
        }
        //在更新形状完毕之后再更新关卡内容，这样就不会自动更新自动生成的形状
        GameLevel.Current.GameUpdate();
        inGameUpdateLoop = false;

        creationProgress += Time.deltaTime * CreationSpeed;
        while (creationProgress >= 1f)
        {
            creationProgress -= 1f;
            GameLevel.Current.SpawnShapes();
        }

        destructionProgress += Time.deltaTime * DestructionSpeed;
        while (destructionProgress >= 1f)
        {
            destructionProgress -= 1f;
            DestroyShape();
        }

        int limit = GameLevel.Current.PopulationLimit;
        if(limit > 0)
        {
            while(shapes.Count - dyingShapeCount > limit)
            {
                DestroyShape();
            }
        }

        if(killList.Count>0)
        {
            for(int i=0;i<killList.Count;i++)
            {
                if (killList[i].IsValid)
                {
                    KillImmediately(killList[i].Shape);
                }
            }
            killList.Clear();
        }

        if(markAsDyingList.Count>0)
        {
            for(int i=0;i<markAsDyingList.Count;i++)
            {
                if(markAsDyingList[i].IsValid)
                {
                    MarkAsDyingImmediately(markAsDyingList[i].Shape);
                }
            }
            markAsDyingList.Clear();
        }
    }
    
    public void AddShape(Shape shape)
    {
        shape.SaveIndex = shapes.Count;
        shapes.Add(shape);
    }

    void DestroyShape()
    {
        if(shapes.Count - dyingShapeCount > 0)
        {
            Shape shape = shapes[Random.Range(dyingShapeCount, shapes.Count)];
            if(destroyDuration <= 0f)
            {
                KillImmediately(shape);
            }
            else
            {
                shape.AddBehavior<DyingShapeBehavior>().Initialize(shape, destroyDuration);
            }
        }
    }

    void BeginNewGame()
    {
        Random.state = mainRandomState;     //恢复主随机状态
        //在主随机状态中获取一个随机值作为种子
        int seed = Random.Range(0, int.MaxValue) ^ (int)Time.unscaledTime;
        //在使用Random.Range后，Random.State的值会发生变化，因此将变化后的值更新到主随机状态中
        mainRandomState = Random.state;
        //使用种子初始化一个新的伪随机序列
        Random.InitState(seed);

        creationSpeedSlider.value = CreationSpeed = 0;
        destructionSpeedSlider.value = DestructionSpeed = 0;


        for (int i=0;i<shapes.Count;i++)
        {
            shapes[i].Recycle();
        }
        shapes.Clear();
        dyingShapeCount = 0;
    }

    public override void Save(GameDataWrite writer)
    {
        writer.Write(shapes.Count);
        writer.Write(Random.state);
        writer.Write(CreationSpeed);
        writer.Write(creationProgress);
        writer.Write(DestructionSpeed);
        writer.Write(destructionProgress);
        writer.Write(loadedLevelBuildIndex);
        GameLevel.Current.Save(writer);
        for(int i=0;i<shapes.Count;i++)
        {
            writer.Write(shapes[i].OriginFactory.FactoryId);

            //shapeId需要先于形状实例之前保存，因为在读取时它需要先被读取以确定实例化的是哪个形状
            writer.Write(shapes[i].ShapeId);
            writer.Write(shapes[i].MaterialId);
            shapes[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        int version = reader.Version;
        //加载到的版本比我们当前保存的版本高，需要记录一个错误并立即返回
        if(version > saveVersion)
        {
            Debug.LogError("Unsupported future save version " + version);
            return;
        }
        StartCoroutine(LoadGame(reader));
    }

    IEnumerator LoadGame(GameDataReader reader)
    {
        int version = reader.Version;

        int count = version <= 0 ? -version : reader.ReadInt();

        if (version >= 3)
        {
            Random.State state = reader.ReadRandomState();
            if (!reseedOnLoad)  //是否使用可重复的随机性
            {
                //使用文件中保存的随机状态
                Random.state = state;
            }
            creationSpeedSlider.value = CreationSpeed = reader.ReadFloat();
            creationProgress = reader.ReadFloat();
            destructionSpeedSlider.value = DestructionSpeed = reader.ReadFloat();
            destructionProgress = reader.ReadFloat();
        }

        //使用协程加载完场景之后再读取关卡数据
        yield return LoadLevel(version < 2 ? 1 : reader.ReadInt());
        
        if(version >= 3)
        {
            //读取关卡数据
            GameLevel.Current.Load(reader);
        }

        for (int i = 0; i < count; i++)
        {
            int factoryId = version >= 5 ? reader.ReadInt() : 0;
            int shapeId = version > 0 ? reader.ReadInt() : 0;
            int materialId = version > 0 ? reader.ReadInt() : 0;
            Shape instance = shapeFactories[factoryId].Get(shapeId, materialId);
            instance.Load(reader);
        }
        for(int i=0;i<shapes.Count;i++)
        {
            shapes[i].ResolveShapeInstances();
        }
    }

    IEnumerator LoadLevel(int levelBuildIndex)
    {
        //加载场景时防止玩家在场景加载完毕前发出命令
        //在开始加载过程之前禁用自身，加载完毕再次启用自身
        enabled = false;
        if(loadedLevelBuildIndex > 0)
        {
            yield return SceneManager.UnloadSceneAsync(loadedLevelBuildIndex);
        }
        yield return SceneManager.LoadSceneAsync(levelBuildIndex,LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(levelBuildIndex));
        enabled = true;
        loadedLevelBuildIndex = levelBuildIndex;
    }

    public Shape GetShape(int index)
    {
        return shapes[index];
    }

    private void KillImmediately(Shape shape)
    {
        int index = shape.SaveIndex;
        shape.Recycle();

        if(index < dyingShapeCount && index < --dyingShapeCount)
        {
            shapes[dyingShapeCount].SaveIndex = index;
            shapes[index] = shapes[dyingShapeCount];
            index = dyingShapeCount;
        }

        int lastIndex = shapes.Count - 1;
        if(index < lastIndex)
        {
            shapes[lastIndex].SaveIndex = index;
            shapes[index] = shapes[lastIndex];
        }
        shapes.RemoveAt(lastIndex);
    }

    public void Kill(Shape shape)
    {
        if(inGameUpdateLoop)
        {
            killList.Add(shape);
        }
        else
        {
            KillImmediately(shape);
        }
    }

    void MarkAsDyingImmediately(Shape shape)
    {
        int index = shape.SaveIndex;
        if(index<dyingShapeCount)
        {
            return;
        }
        shapes[dyingShapeCount].SaveIndex = index;
        shapes[index] = shapes[dyingShapeCount];
        shape.SaveIndex = dyingShapeCount;
        shapes[dyingShapeCount++] = shape;
    }

    public void MarkAsDying(Shape shape)
    {
        if(inGameUpdateLoop)
        {
            markAsDyingList.Add(shape);
        }
        else
        {
            MarkAsDyingImmediately(shape);
        }
    }

    public bool IsMarkedAsDying(Shape shape)
    {
        return shape.SaveIndex < dyingShapeCount;
    }

    

}
