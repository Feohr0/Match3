using Match3Game;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.Android;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Vector2 waterFirstLocation = new Vector2();
    public InputDisabler dis;
    [SerializeField] public GameObject Settingspanel;
    [SerializeField] public GameObject GameOverPanel;
    [SerializeField] public GameObject PausePanel;
    [SerializeField] public GameObject WinPanel;
    [SerializeField] public Timer timer;
    [SerializeField] public SoruPopUp soruPopup; // SoruPopup referansý eklendi
    public bool isPaused = false;
    [SerializeField] public AudioSource music;
    [SerializeField] public Button[] buttons;
    public Water water;
    void AssignFunctions()
    {
        buttons[0].onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
        buttons[1].onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
        buttons[2].onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        buttons[3].onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        buttons[4].onClick.AddListener(() => Settingspanel.SetActive(true));
        buttons[5].onClick.AddListener(() => Settingspanel.SetActive(true));
        buttons[6].onClick.AddListener(() => Application.Quit());
        buttons[7].onClick.AddListener(() => Application.Quit());
        buttons[8].onClick.AddListener(() => PauseGame());
        buttons[9].onClick.AddListener(() => ResumeGame());
        buttons[10].onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        buttons[11].onClick.AddListener(() => Settingspanel.SetActive(true));
        buttons[12].onClick.AddListener(() => Application.Quit());
        buttons[13].onClick.AddListener(() => Settingspanel.SetActive(false));
    }


    private void Start()
    {

        AssignFunctions();
        music = GetComponent<AudioSource>();
        music.Play();
        Time.timeScale = 1f;

    }

    private void Update()
    {
        EndGame();
        WaterMover();
    }

    public void EndGame()
    {
        if (timer != null && timer.isOver)
        {
            GameOverPanel.SetActive(true);
            music.Stop();
            Time.timeScale = 0f;
        }
        if (timer == null)
        {
            Debug.Log("Timer Null");
        }
    }

    public void PauseGame()
    {
        if (PausePanel != null && !isPaused)
        {
            PausePanel.SetActive(true);
            isPaused = true;
            dis.DisableInput();
            Time.timeScale = 0;
            buttons[8].gameObject.SetActive(false);

        }
    }

    public void ResumeGame()
    {
        if (PausePanel != null && isPaused)
        {
            PausePanel.SetActive(false);
            isPaused = false;
            dis.EnableInput();
            Time.timeScale = 1;
            buttons[8].gameObject.SetActive(true);




        }
    }
    public void WaterMover()
    {
        // Eðer 3 durumdan herhangi biri saðlanmazsa dur
        if (isPaused || !timer.isCounting || Time.timeScale == 0f || soruPopup.isMovingDown)
            return;

        // Hareket
        transform.Translate(0, 0.022f * Time.deltaTime, 0);
    }

}