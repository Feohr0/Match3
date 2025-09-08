using System.Collections.Generic;
using UnityEngine;

public class RockPool : MonoBehaviour
{
    public GameObject rockPrefab;  // Çakýl taþý prefab'ý
    public int initialPoolSize = 10;  // Baþlangýçta havuzda olacak çakýl taþý sayýsý
    public int maxPoolSize = 50;     // Havuzun büyüyebileceði maksimum boyut
    private Queue<GameObject> rockPool = new Queue<GameObject>();  // Çakýl taþý havuzu

    public BoxCollider rockArea;  // BoxCollider (düþme alaný)

    void Start()
    {
        InitializePool();
    }

    // Havuzdaki çakýl taþlarýný baþlat
    void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewRock();  // Havuzu baþlangýçta oluþtur
        }
    }

    // Yeni bir çakýl taþý oluþtur ve havuza ekle
    private void CreateNewRock()
    {
        GameObject rock = Instantiate(rockPrefab);
        rock.SetActive(false);  // Baþlangýçta pasif yap
        rockPool.Enqueue(rock);  // Havuzda tut
    }

    // Havuzdan bir çakýl taþý al
    public GameObject GetRock(Vector3 position)
    {
        if (rockPool.Count > 0)
        {
            GameObject rock = rockPool.Dequeue();  // Havuzdan bir çakýl taþý al
            rock.SetActive(true);  // Çakýl taþýný aktif yap
            rock.transform.position = position;  // Yeni pozisyona yerleþtir
            rock.GetComponent<Rock>().ActivateRock(rockArea);  // Taþý aktif hale getir ve BoxCollider'ý kullan
            return rock;
        }
        else
        {
            // Eðer havuzda çakýl taþý yoksa, yeni bir tane oluþtur
            if (rockPool.Count < maxPoolSize)  // Havuzun boyutunu kontrol et
            {
                CreateNewRock();  // Yeni çakýl taþý oluþtur
            }
            return GetRock(position);  // Yeni taþ al
        }
    }

    // Çakýl taþýný havuza geri ver
    public void ReturnRock(GameObject rock)
    {
        rock.SetActive(false);  // Nesneyi devre dýþý býrak
        rockPool.Enqueue(rock);  // Nesneyi havuza geri ekle
    }

    // Context Menu ile taþlarý aktif hale getirme
    [ContextMenu("Activate All Rocks")]
    public void ActivateAllRocks()
    {
        foreach (var rock in rockPool)
        {
            if (rock != null && !rock.activeInHierarchy)
            {
                rock.SetActive(true);  // Havuzdaki tüm taþlarý aktif yap
                
                rock.GetComponent<Rock>().ActivateRock(rockArea);  // Taþýn fiziksel etkileþime girmesini saðla
            }
        }
    }
}
