using Obi;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Match3Game;
using static JokerTools;
using Unity.VisualScripting;

public class SoruPopUp : MonoBehaviour
{
    bool questionActive;
    public bool isMovingDown=false;
    public GridSystem grid;
    public BoxCollider wallCollider;
    public float yukariMesafe = 6f;
    public float gecikmeSuresi = 0.04f;
    public GameManager mngr;
    public JokerTools jokerTools; // Inspector'dan atanmalı
    public InputDisabler di;
    [SerializeField] GameObject waterPlane;
    public ObiSolver solver;
    public Timer timer;
    public GameObject hedefPanel;
    public Water water;
    public bool? cevap = null;

    public static bool PanelAçık { get; private set; } = false;
    private float öncekiHız; // Panelden önceki hızı saklar
    public int correctCount;
    [SerializeField] public Button[] buttons;

    void Start()
    {
        correctCount = 0;
        di.EnableInput();
        if (hedefPanel != null)
            hedefPanel.SetActive(false);
        Time.timeScale = 1f;

        Buttonfunction();
    }

    private void Update()
    {
        if (correctCount >= 3)
        {
            mngr.WinPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    void Buttonfunction()
    {
        buttons[0].onClick.AddListener(() =>
        {
            cevap = true;

            ToolType achieved = jokerTools.GetRandomToolType();
            grid.moveAttempts = grid.Default_moveAttempts;
            grid.attempText.text = grid.moveAttempts.ToString();
            correctCount += 1;

            switch (achieved)
            {
                case ToolType.Hammer:
                    jokerTools.AddHammer(1);
                    break;
                case ToolType.Vertical:
                    jokerTools.AddVertical(1);
                    break;
                case ToolType.Horizontal:
                    jokerTools.AddHorizontal(1);
                    break;
            }
        });

        buttons[1].onClick.AddListener(() =>
        {
            cevap = false;
        });
    }

    void PaneliKapat()
    {
        if (hedefPanel != null)
        {
            hedefPanel.SetActive(false);
            PanelAçık = false;
            timer.isCounting = true;
            di.EnableInput();
        }
    }

    public void MoveColliderTemporarily()
    {
        Debug.Log("MoveColliderTemporarily çağrıldı!"); // Debug log ekledik
        StartCoroutine(MoveColliderRoutine());
    }

    private IEnumerator MoveColliderRoutine()
    {
        // Önce Inspector'dan atanan waterPlane'i kullan
        GameObject waterPlaneObj = waterPlane;

        // Eğer Inspector'dan atanmamışsa Find ile bul
        if (waterPlaneObj == null)
        {
            waterPlaneObj = GameObject.Find("waterplane");
            if (waterPlaneObj == null)
            {
                // Farklı isimlerle dene
                waterPlaneObj = GameObject.Find("WaterPlane");
                if (waterPlaneObj == null)
                {
                    waterPlaneObj = GameObject.Find("Waterplane");
                }
            }
        }

        if (waterPlaneObj == null)
        {
            Debug.LogError("Waterplane hiçbir şekilde bulunamadı! Lütfen Inspector'dan waterPlane'i atayın.");
            yield break;
        }

        Debug.Log($"Waterplane bulundu: {waterPlaneObj.name}"); // Debug log

        Vector3 startPos = waterPlaneObj.transform.position;
        Vector3 targetPos = startPos + Vector3.down * 0.1f;

        Debug.Log($"Başlangıç pozisyonu: {startPos}, Hedef pozisyon: {targetPos}"); // Debug log

        float elapsedTime = 0f;
        float duration = 3f; // 3 saniyede insin

        isMovingDown = true; // Hareket başlangıcında true yap

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            Vector3 newPos = Vector3.Lerp(startPos, targetPos, t);
            waterPlaneObj.transform.position = newPos;

            Debug.Log($"Hareket ediyor - Zaman: {elapsedTime:F2}, Pozisyon: {newPos}"); // Debug log

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        waterPlaneObj.transform.position = targetPos; // Son pozisyona sabitle
        isMovingDown = false; // Hareket bittiğinde false yap

        Debug.Log("Waterplane hareketi tamamlandı!"); // Debug log
    }


    public IEnumerator SpeedUp()
    {
        // Normal hıza geri dönmesi için normalSpeed kullan
        water.speed =water.slowSpeed;
        yield return new WaitForSeconds(6f);
        water.speed = water.normalSpeed; // Normal hıza geri dön 
    }

    public IEnumerator SlowDown()
    {
        // Normal hıza geri dönmesi için normalSpeed kullan
        water.speed = water.fastspeed; // 0.18f
        yield return new WaitForSeconds(6f);
        water.speed = water.normalSpeed; // Normal hıza geri dön (0.27f)      
    }

    public IEnumerator Soru()
    {
        PanelAçık = true;
        hedefPanel.SetActive(true);
        Time.timeScale = 0;
        timer.isCounting = false;
        cevap = null;

        // 30 saniye içinde cevap beklenir
        float zaman = 0f;
        while (zaman < 30f && cevap == null)
        {
            zaman += Time.deltaTime;
            yield return null;
        }

        if (cevap == null)
        {
            cevap = false; // zaman dolduysa otomatik olarak yanlış kabul
            Time.timeScale = 1f;
        }

        hedefPanel.SetActive(false);
        timer.isCounting = true;
        Time.timeScale = 1;
        PanelAçık = false;

        if (cevap == false)
        {
            yield return StartCoroutine(SpeedUp());

            // Hızlanma bitti, tekrar soru sor
            StartCoroutine(Soru());
        }
        else
        {
            // doğru cevap verildiyse tekrar soru sormaya gerek yok
            grid.moveAttempts = grid.Default_moveAttempts;
            grid.attempText.text = grid.moveAttempts.ToString();
        }
    }
}