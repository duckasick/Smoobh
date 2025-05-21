using System.Collections.Generic;
using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    public float xLeft;
    public float xRight;
    public float xAmount;
    public float yDown;
    public float yUp;
    public float yAmount;
    public float zFront;
    public float zRear;
    public float zAmount;
    public float moveBack;

    float xSpread;
    float ySpread;
    float zSpread;

    public GameObject cube;

    List<GameObject> cubes;

    void Start()
    {
        xSpread = (xRight - xLeft) / xAmount;
        ySpread = (yUp - yDown) / yAmount;
        zSpread = (zRear - zFront) / zAmount;
        cubes = new List<GameObject>();

        for (int i = 0; i < xAmount; i++)
        {   
            for (int j = 0; j < yAmount; j++)
            {
                for (int k = 0; k < zAmount; k++)
                {
                    cubes.Add(Instantiate(cube, new Vector3(xLeft + xSpread * (i + 1) - xSpread/2, yDown + ySpread * (j + 1) - ySpread / 2, zFront + zSpread * (k + 1) - zSpread / 2 +zSpread+30+moveBack), Quaternion.identity));
                }
            }
            
        }

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            xSpread = (xRight - xLeft) / xAmount;
            ySpread = (yUp - yDown) / yAmount;
            zSpread = (zRear - zFront) / zAmount;
            for (int g = 0; g < cubes.Count; g++)
            {
                Destroy(cubes[g]);
            }
            for (int i = 0; i < xAmount; i++)
            {
                for (int j = 0; j < yAmount; j++)
                {
                    for (int k = 0; k < zAmount; k++)
                    {
                        cubes.Add(Instantiate(cube, new Vector3(xLeft + xSpread * (i + 1) - xSpread / 2, yDown + ySpread * (j + 1) - ySpread / 2, zFront + zSpread * (k + 1) - zSpread / 2 + zSpread + 30 + moveBack), Quaternion.identity));
                    }
                }
            }
        }
    }
}