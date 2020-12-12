using UnityEngine;
using UnityEditor;
static class RegisterLevelObjectMenuItem
{
    const string menuItem = "GameObject/Register Level Object";

    //验证方法的属性具有true作为附加参数，并且返回是否应启用菜单项。
    [MenuItem(menuItem,true)]
    static bool ValidateRegisterLevelObject()
    {
        if (Selection.objects.Length == 0)
        {
            return false;
        }

        foreach (Object o in Selection.objects)
        {
            if (!(o is GameObject))
            {
                return false;
            }
        }
        return true;
    }

    [MenuItem(menuItem)]
    static void RegisterLevelObject()
    {
        foreach (Object o in Selection.objects)
        {
            Register(o as GameObject);
        }
    }

    static void Register(GameObject o)
    {
        if (PrefabUtility.GetPrefabType(o) == PrefabType.Prefab)
        {
            //在记录警告时提供对象作为附加参数，以便在编辑器中将其临时突出显示。
            Debug.LogWarning(o.name + " is a prefab asset.", o);
            return;
        }

        var levelObject = o.GetComponent<GameLevelObject>();
        if (levelObject == null)
        {
            Debug.LogWarning(o.name + " isn't a game level object.", o);
        }

        //当foreach循环遍历数组外的其他集合或枚举数（包括List）时，它会创建一个临时迭代器对象，用于分配内存
        //所以经验法则就是不要依赖foreach来获取游戏逻辑。这对数组来说很好，但是如果它们被重构为列表
        //你会在游戏中突然得到临时的内存分配
        //这一步需要找到合适的游戏关卡进行注册，假设关卡对象始终是其场景的根对象，遍历场景中的根对象数组。
        foreach (GameObject rootObject in o.scene.GetRootGameObjects())
        {
            var gamelevel = rootObject.GetComponent<GameLevel>();
            if (gamelevel != null)
            {
                //找到游戏关卡，检查对象是否已被注册
                if (gamelevel.HasLevelObject(levelObject))
                {
                    Debug.LogWarning(o.name + " is already registered.", o);
                    return;
                }
                //记录撤销系统的游戏关卡
                Undo.RecordObject(gamelevel, "Register Level Object.");
                gamelevel.RegisterLevelObject(levelObject);
                Debug.Log(o.name + " registered to game level " + gamelevel.name + " in scene " + o.scene.name + ".", o);

                return;
            }
        }
        Debug.LogWarning(o.name + " isn't part of a game level.", o);
    }


}
