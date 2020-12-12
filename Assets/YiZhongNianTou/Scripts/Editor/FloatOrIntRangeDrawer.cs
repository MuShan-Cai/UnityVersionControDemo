using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(FloatRange)),CustomPropertyDrawer(typeof(IntRange))]
public class FloatOrIntRangeDrawer : PropertyDrawer
{
    //当Unity必须显示FloatRange值的UI时，便会调用OnGUI方法
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        int originalIndentLevel = EditorGUI.indentLevel;
        float originalLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUI.BeginProperty(position, label, property);
        //将变量标签添加进去，调用EditorGUI.PrefixLabel方法，当标签占用完空间后
        //该方法返回一个修改后的剩余区域。
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive),label);
        
        position.width = position.width / 2f;

        //因为Unity标签使用的固定宽度对于min和max字段而言太宽了
        //通过EditorGUIUtility.labelWidth属性覆盖该宽度，设置为每个字段使用宽度的一半。
        EditorGUIUtility.labelWidth = position.width / 2f;
        //设置一个字段绘制之后的缩进
        EditorGUI.indentLevel = 1;
        //显示FloatRange脚本中的min变量UI
        EditorGUI.PropertyField(position, property.FindPropertyRelative("min"));

        position.x += position.width;

        //显示FloatRange脚本中的max变量UI
        EditorGUI.PropertyField(position, property.FindPropertyRelative("max"));

        EditorGUI.EndProperty();

        //将缩进级别和标签宽度恢复为其原始值
        EditorGUI.indentLevel = originalIndentLevel;
        EditorGUIUtility.labelWidth = originalLabelWidth;
    }


}
