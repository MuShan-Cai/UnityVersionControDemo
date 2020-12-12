using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeZone : MonoBehaviour
{
    [SerializeField]
    private float dyingDuration;

    private void OnTriggerExit(Collider other)
    {
        //调用GetComponent会发生内存分配，这种内存分配只存在Unity编辑器中
        //因为它动态创建了一个错误消息字符串，即时没有被使用，这在构建（Build版本）中不会发生
        var shape = other.GetComponent<Shape>();
        if (shape)
        {
            if (dyingDuration <= 0f)
            {
                shape.Die();
            }
            else if (!shape.IsMarkedAsDying)
            {
                shape.AddBehavior<DyingShapeBehavior>().Initialize(shape, dyingDuration);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        var c = GetComponent<Collider>();
        var b = c as BoxCollider;
        if (b != null)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.DrawWireCube(b.center, b.size);
            return;
        }
        var s = c as SphereCollider;
        if (s != null)
        {
            //lossyScale:有损缩放 ———— 它是世界空间中物体尺度的近似值，因为该对象可以是在非均匀缩放范围内旋转的对象层次结构中的子对象，
            //这会使该对象变形。这不能仅仅用一个尺度来表示，因此wold-space尺度被定义为有损的。
            Vector3 scale = transform.lossyScale;
            scale = Vector3.one * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, scale);
            Gizmos.DrawWireSphere(s.center, s.radius);
            return;
        }
    }

}
