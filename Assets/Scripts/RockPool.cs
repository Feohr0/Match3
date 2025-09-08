using System.Collections.Generic;
using UnityEngine;

public class RockPool : MonoBehaviour
{
    public GameObject rockPrefab;  // �ak�l ta�� prefab'�
    public int initialPoolSize = 10;  // Ba�lang��ta havuzda olacak �ak�l ta�� say�s�
    public int maxPoolSize = 50;     // Havuzun b�y�yebilece�i maksimum boyut
    private Queue<GameObject> rockPool = new Queue<GameObject>();  // �ak�l ta�� havuzu

    public BoxCollider rockArea;  // BoxCollider (d��me alan�)

    void Start()
    {
        InitializePool();
    }

    // Havuzdaki �ak�l ta�lar�n� ba�lat
    void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewRock();  // Havuzu ba�lang��ta olu�tur
        }
    }

    // Yeni bir �ak�l ta�� olu�tur ve havuza ekle
    private void CreateNewRock()
    {
        GameObject rock = Instantiate(rockPrefab);
        rock.SetActive(false);  // Ba�lang��ta pasif yap
        rockPool.Enqueue(rock);  // Havuzda tut
    }

    // Havuzdan bir �ak�l ta�� al
    public GameObject GetRock(Vector3 position)
    {
        if (rockPool.Count > 0)
        {
            GameObject rock = rockPool.Dequeue();  // Havuzdan bir �ak�l ta�� al
            rock.SetActive(true);  // �ak�l ta��n� aktif yap
            rock.transform.position = position;  // Yeni pozisyona yerle�tir
            rock.GetComponent<Rock>().ActivateRock(rockArea);  // Ta�� aktif hale getir ve BoxCollider'� kullan
            return rock;
        }
        else
        {
            // E�er havuzda �ak�l ta�� yoksa, yeni bir tane olu�tur
            if (rockPool.Count < maxPoolSize)  // Havuzun boyutunu kontrol et
            {
                CreateNewRock();  // Yeni �ak�l ta�� olu�tur
            }
            return GetRock(position);  // Yeni ta� al
        }
    }

    // �ak�l ta��n� havuza geri ver
    public void ReturnRock(GameObject rock)
    {
        rock.SetActive(false);  // Nesneyi devre d��� b�rak
        rockPool.Enqueue(rock);  // Nesneyi havuza geri ekle
    }

    // Context Menu ile ta�lar� aktif hale getirme
    [ContextMenu("Activate All Rocks")]
    public void ActivateAllRocks()
    {
        foreach (var rock in rockPool)
        {
            if (rock != null && !rock.activeInHierarchy)
            {
                rock.SetActive(true);  // Havuzdaki t�m ta�lar� aktif yap
                
                rock.GetComponent<Rock>().ActivateRock(rockArea);  // Ta��n fiziksel etkile�ime girmesini sa�la
            }
        }
    }
}
