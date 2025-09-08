using UnityEngine;

public class Rock : MonoBehaviour
{
    private Rigidbody rb;  // Rigidbody referans�
    private bool isActive = false;  // �ak�l ta�� aktif mi?
  
    private Vector3 targetPosition;  // �ak�l ta��n�n hedef pozisyonu

    void Awake()
    {
        rb = GetComponent<Rigidbody>();  // Rigidbody bile�enini al�yoruz
        rb.useGravity = true;  // Yer�ekimini aktif hale getiriyoruz
        rb.isKinematic = true;  
       
    }

    void Update()
    {
        // Ta� aktifse, hareket etmeye ba�las�n
        if (isActive)
        {
            MoveRockDown();
        }
    }

    // �ak�l ta��n� BoxCollider'�n �st s�n�r�na yerle�tir
    private void StartFalling(BoxCollider area)
    {
        // BoxCollider'�n �st s�n�r�na yerle�tiriyoruz (y ekseni)
        Vector3 startPosition = new Vector3(
            Random.Range(-area.size.x / 2, area.size.x / 2), // x koordinat� rastgele
            area.transform.position.y + area.size.y / 2 + 5f, // y koordinat� BoxCollider'�n �st s�n�r�ndan
            Random.Range(-area.size.z / 2, area.size.z / 2) // z koordinat� rastgele
        );

        transform.position = startPosition; // Ba�lang�� pozisyonunu ayarla

        // �ak�l ta��n� aktif hale getir
        ActivateRock(area);
    }

    // �ak�l ta��n� aktif hale getiren method
    public void ActivateRock(BoxCollider area)
    {
        rb.isKinematic = false;  // Fiziksel hareketi aktif et
        isActive = true;  // Ta�� aktif hale getir
        StartFalling(area);  // Ta� d��meye ba�las�n
    }

    // �ak�l ta��n� sadece a�a�� do�ru hareket ettir (sadece y ekseninde hareket)
    private void MoveRockDown()
    {
        if (transform.position.y > targetPosition.y)
        {
            // A�a�� do�ru hareket et
            rb.isKinematic = false;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, 2f * Time.deltaTime);  // Yava��a a�a��ya do�ru hareket ettir
        }
        else
        {
            // Ta� hedef pozisyona ula�t���nda durur
         //   isActive = false;  // Ta�� durdur
          //  rb.isKinematic = true;  // Fiziksel etkile�im durur
        }
    }

    // �ak�l ta�� gem'le �arp��t���nda
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Gem"))
        {
            // �ak�l ta�� gem'in �st�ne yerle�ecek
            PositionRockOnGem(collision.gameObject);
        }
    }

    // �ak�l ta��n� gem'in �st�ne yerle�tir
    private void PositionRockOnGem(GameObject gem)
    {
        Vector3 gemPosition = gem.transform.position;
        transform.position = new Vector3(gemPosition.x, gemPosition.y + 0.5f, gemPosition.z);  // Y�ksekli�i gem'in �st�ne ayarla
        rb.isKinematic = true;  // �ak�l ta��n�n hareketini durdur
    }
}
