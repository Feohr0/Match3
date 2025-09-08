using UnityEngine;

public class Trigger : MonoBehaviour
{
    public InputDisabler dsbl;
    public GameObject gameOverPanel;
    public GameObject waterCollider;

    // 2D için
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger detected: " + other.name + " with tag: " + other.tag);

        if (other.CompareTag("water"))
        {
            GameOver();
        }
    }

    // 3D için (alternatif)
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger detected: " + other.name + " with tag: " + other.tag);

        if (other.CompareTag("water"))
        {
            GameOver();
        }
    }

    void GameOver()
    {
        Debug.Log("Water Overflow - Game Over!");
        Time.timeScale = 0f;
        dsbl.DisableInput();
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }
}