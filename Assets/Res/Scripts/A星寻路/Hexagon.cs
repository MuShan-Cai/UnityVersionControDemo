using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hexagon : MonoBehaviour
{
    Hexagon father = null;
    public Hexagon[] neighbors = new Hexagon[6];
    public bool passFlag = true;
    float gValue = 999f;
    float hValue = 999f;

    public Material redMat;
    public Material greenMat;

    void Start()
    {
    }

    public void reset()
    {

    }

    public Hexagon[] getNeighborList()
    {
        return neighbors;
    }

    public void setFatherHexagon(Hexagon f)
    {
        father = f;
    }

    public Hexagon getFatherHexagon()
    {
        return father;
    }

    public void setCanPass(bool f)
    {
        passFlag = f;
    }

    public bool canPass()
    {
        return passFlag;
    }

    public float computeGValue(Hexagon hex)
    {
        return 1f;
    }

    public void setgValue(float v)
    {
        gValue = v;
    }

    public float getgValue()
    {
        return gValue;
    }

    public void sethValue(float v)
    {
        hValue = v;
    }

    public float gethValue()
    {
        return hValue;
    }

    public float computeHValue(Hexagon hex)
    {
        return Vector3.Distance(transform.position, hex.transform.position);
    }
}
