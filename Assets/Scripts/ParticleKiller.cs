using Obi;
using UnityEngine;

public class ParticleKiller : MonoBehaviour
{
    public ObiSolver solver;
    public Collider killZoneCollider;  // Örneðin kapýnýn dýþýndaki collider
    public float killY = -1000f;       // Partikülleri gönderilecek uzak yer

    private void Update()
    {
        if (solver == null || solver.positions == null)
            return;

        for (int i = 0; i < solver.positions.count; i++)
        {
            Vector3 pos = (Vector3)solver.positions[i];

            // Eðer partikül killZoneCollider'ýn içinde deðilse temizle
            if (!killZoneCollider.bounds.Contains(pos) || pos.y < killY + 1f)
            {
                solver.velocities[i] = Vector4.zero;
                solver.invMasses[i] = 0f;
                solver.positions[i] = new Vector4(0, killY, 0, 1);
            }
        }
    }
}


