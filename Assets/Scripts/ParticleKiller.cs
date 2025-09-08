using Obi;
using UnityEngine;

public class ParticleKiller : MonoBehaviour
{
    public ObiSolver solver;
    public Collider killZoneCollider;  // �rne�in kap�n�n d���ndaki collider
    public float killY = -1000f;       // Partik�lleri g�nderilecek uzak yer

    private void Update()
    {
        if (solver == null || solver.positions == null)
            return;

        for (int i = 0; i < solver.positions.count; i++)
        {
            Vector3 pos = (Vector3)solver.positions[i];

            // E�er partik�l killZoneCollider'�n i�inde de�ilse temizle
            if (!killZoneCollider.bounds.Contains(pos) || pos.y < killY + 1f)
            {
                solver.velocities[i] = Vector4.zero;
                solver.invMasses[i] = 0f;
                solver.positions[i] = new Vector4(0, killY, 0, 1);
            }
        }
    }
}


