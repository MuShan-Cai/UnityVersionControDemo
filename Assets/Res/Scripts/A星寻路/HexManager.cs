using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class HexManager : MonoBehaviour
{
    Dictionary<string,Hexagon> name2Hex = new Dictionary<string, Hexagon>();

    static List<Hexagon> openList = new List<Hexagon>();
    static List<Hexagon> closeList = new List<Hexagon>();


    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform child in transform)
        {
            name2Hex.Add(child.name, child.GetComponent<Hexagon>());
        }
    }

    public Hexagon GetHexByName(string i)
    {
        Hexagon v = new Hexagon();
        name2Hex.TryGetValue(i, out v);
        return v;
    }

    public Dictionary<string, Hexagon> GetAllHex()
    {
        return name2Hex;
    }

    public static List<Hexagon> searchRoute(Hexagon thisHexagon,Hexagon targetHexagon)
    {
        Hexagon nowHexagon = thisHexagon;
        nowHexagon.reset();

        openList.Add(nowHexagon);
        bool finded = false;

        while(!finded)
        {
            openList.Remove(nowHexagon);
            closeList.Add(nowHexagon);
            Hexagon[] neighbors = nowHexagon.getNeighborList();
            foreach(Hexagon neighbor in neighbors)
            {
                if (neighbor == null) continue;

                if(neighbor == targetHexagon)
                {
                    finded = true;
                    neighbor.setFatherHexagon(nowHexagon);
                }
                if(closeList.Contains(neighbor) || !neighbor.canPass())
                {
                    continue;
                }

                if(openList.Contains(neighbor))
                {
                    float assueGValue = neighbor.computeGValue(nowHexagon) + nowHexagon.getgValue();
                    if(assueGValue < neighbor.getgValue())
                    {
                        openList.Remove(neighbor);
                        neighbor.setgValue(assueGValue);
                        openList.Add(neighbor);
                    }
                }
                else
                {
                    neighbor.sethValue(neighbor.computeHValue(targetHexagon));
                    neighbor.setgValue(neighbor.computeHValue(nowHexagon) + nowHexagon.getgValue());
                    openList.Add(neighbor);
                    neighbor.setFatherHexagon(nowHexagon);
                }
            }

            if(openList.Count<= 0)
            {
                Debug.Log("无法到达目标");
                break;
            }
            else
            {
                nowHexagon = openList[0];
            }
        }

        openList.Clear();
        closeList.Clear();

        List<Hexagon> route = new List<Hexagon>();
        if(finded)
        {
            Hexagon hex = targetHexagon;
            while(hex != thisHexagon)
            {
                route.Add(hex);
                Hexagon fatherHex = hex.getFatherHexagon();
                hex = fatherHex;
            }
            route.Add(hex);
        }
        route.Reverse();
        return route;
    }

    public static int GetRouteDis(Hexagon thisHexagon,Hexagon targetHexagon)
    {
        Hexagon nowHexagon = thisHexagon;
        nowHexagon.reset();

        openList.Add(nowHexagon);
        bool finded = false;
        while(!finded)
        {
            openList.Remove(nowHexagon);
            closeList.Add(nowHexagon);
            Hexagon[] neighbors = nowHexagon.getNeighborList();

            foreach(Hexagon neighbor in neighbors)
            {
                if (neighbor == null) continue;

                if(neighbor == targetHexagon)
                {
                    finded = true;
                    neighbor.setFatherHexagon(nowHexagon);
                }

                if(closeList.Contains(neighbor))
                {
                    continue;
                }

                if(openList.Contains(neighbor))
                {
                    float assueGValue = nowHexagon.getgValue() + neighbor.computeGValue(nowHexagon);
                    if(assueGValue < neighbor.getgValue())
                    {
                        openList.Remove(neighbor);
                        neighbor.setgValue(assueGValue);
                        openList.Add(neighbor);
                    }
                }
                else
                {
                    neighbor.sethValue(neighbor.computeHValue(targetHexagon));
                    neighbor.setgValue(neighbor.computeGValue(targetHexagon));
                    openList.Add(neighbor);
                    neighbor.setFatherHexagon(nowHexagon);
                }
            }

            if(openList.Count<=0)
            {
                break;
            }
            else
            {
                nowHexagon = openList[0];
            }
        }
        openList.Clear();
        closeList.Clear();

        List<Hexagon> route = new List<Hexagon>();

        if(finded)
        {
            Hexagon hex = targetHexagon;
            while (hex != thisHexagon)
            {
                route.Add(hex);
                Hexagon father = hex.getFatherHexagon();
                hex = father;
            }
            route.Add(hex);
        }
        return route.Count - 1;
    }
}
