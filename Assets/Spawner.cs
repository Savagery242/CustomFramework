using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{

    [SerializeField]
    GameObject go;
    public int spawnNum;

    // Use this for initialization
    void Start()
    {
        
        for (int i = 0; i < spawnNum; ++i)
        {
            GameObject.Instantiate(go, Vector3.forward * i, Quaternion.identity);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GameObject.Instantiate(go, Vector3.forward, Quaternion.identity);
        }
    }


}
