using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    [SerializeField] GameObject gameOverPanel;
    [SerializeField]  GameManager manager;
    [SerializeField] Timer timer;
    public float speed;
    public float fastspeed = 0.00856f;
    public float normalSpeed = 0.022f;
    public float slowSpeed = 0.03422f;
    [SerializeField] GameObject waterplane;
    private void Start()
    {
        speed = normalSpeed;
    }
    void GameOver()
    {
        Debug.Log("Water Overflow - Game Over!");
        Time.timeScale = 0f;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }
    private void Update()
    {
        WaterMover();
    }
    public void WaterMover()
    {
        // Eðer 3 durumdan herhangi biri saðlanmazsa dur
        if (manager.isPaused || !timer.isCounting || Time.timeScale == 0f)
            return;

        // Hareket
        waterplane.transform.Translate(0, speed * Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == ("water"))
        {
                GameOver();
        }
    }
}
