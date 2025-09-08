using UnityEngine;

public class Rock : MonoBehaviour
{
    private Rigidbody rb;  // Rigidbody referansý
    private bool isActive = false;  // Çakýl taþý aktif mi?
  
    private Vector3 targetPosition;  // Çakýl taþýnýn hedef pozisyonu

    void Awake()
    {
        rb = GetComponent<Rigidbody>();  // Rigidbody bileþenini alýyoruz
        rb.useGravity = true;  // Yerçekimini aktif hale getiriyoruz
        rb.isKinematic = true;  
       
    }

    void Update()
    {
        // Taþ aktifse, hareket etmeye baþlasýn
        if (isActive)
        {
            MoveRockDown();
        }
    }

    // Çakýl taþýný BoxCollider'ýn üst sýnýrýna yerleþtir
    private void StartFalling(BoxCollider area)
    {
        // BoxCollider'ýn üst sýnýrýna yerleþtiriyoruz (y ekseni)
        Vector3 startPosition = new Vector3(
            Random.Range(-area.size.x / 2, area.size.x / 2), // x koordinatý rastgele
            area.transform.position.y + area.size.y / 2 + 5f, // y koordinatý BoxCollider'ýn üst sýnýrýndan
            Random.Range(-area.size.z / 2, area.size.z / 2) // z koordinatý rastgele
        );

        transform.position = startPosition; // Baþlangýç pozisyonunu ayarla

        // Çakýl taþýný aktif hale getir
        ActivateRock(area);
    }

    // Çakýl taþýný aktif hale getiren method
    public void ActivateRock(BoxCollider area)
    {
        rb.isKinematic = false;  // Fiziksel hareketi aktif et
        isActive = true;  // Taþý aktif hale getir
        StartFalling(area);  // Taþ düþmeye baþlasýn
    }

    // Çakýl taþýný sadece aþaðý doðru hareket ettir (sadece y ekseninde hareket)
    private void MoveRockDown()
    {
        if (transform.position.y > targetPosition.y)
        {
            // Aþaðý doðru hareket et
            rb.isKinematic = false;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, 2f * Time.deltaTime);  // Yavaþça aþaðýya doðru hareket ettir
        }
        else
        {
            // Taþ hedef pozisyona ulaþtýðýnda durur
         //   isActive = false;  // Taþý durdur
          //  rb.isKinematic = true;  // Fiziksel etkileþim durur
        }
    }

    // Çakýl taþý gem'le çarpýþtýðýnda
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Gem"))
        {
            // Çakýl taþý gem'in üstüne yerleþecek
            PositionRockOnGem(collision.gameObject);
        }
    }

    // Çakýl taþýný gem'in üstüne yerleþtir
    private void PositionRockOnGem(GameObject gem)
    {
        Vector3 gemPosition = gem.transform.position;
        transform.position = new Vector3(gemPosition.x, gemPosition.y + 0.5f, gemPosition.z);  // Yüksekliði gem'in üstüne ayarla
        rb.isKinematic = true;  // Çakýl taþýnýn hareketini durdur
    }
}
