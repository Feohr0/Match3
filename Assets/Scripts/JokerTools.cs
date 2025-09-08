using Match3Game;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JokerTools : MonoBehaviour
{
    [SerializeField] public AudioSource button;
    [SerializeField] public Button hammer;
    [SerializeField] public Button vertical;
    [SerializeField] public Button horizontal;
    [SerializeField] public Button colorBomb;
    [SerializeField] private GridSystem gridSystem;
    [SerializeField] public TextMeshProUGUI hammertext;
    [SerializeField] public TextMeshProUGUI verticaltext;
    [SerializeField] public TextMeshProUGUI horizontaltext;
    [SerializeField] public TextMeshProUGUI colorBombtext;
    public int hammercount = 0;
    public int horizontalcount = 0;
    public int verticalcount = 0;
    public int colorbombcount = 0;
    private ButtonState currentButtonState = ButtonState.Ready;
    private ToolType activeToolType = ToolType.None;

    public enum ButtonState
    {
        Ready,
        WaitingForGem,
        Deactive,
    }

    public enum ToolType
    {
        None,
        Hammer,
        Vertical,
        Horizontal,
        ColorBomb
    }

    private void Start()
    {
        hammertext.text = hammercount.ToString();
        horizontaltext.text = horizontalcount.ToString();
        verticaltext.text = verticalcount.ToString();
        colorBombtext.text = colorbombcount.ToString();
        // GridSystem referansýný bul
        if (gridSystem == null)
            gridSystem = FindObjectOfType<GridSystem>();

        // Button event'lerini ayarla
        hammer.onClick.AddListener(() => ActivateTool(ToolType.Hammer));
        vertical.onClick.AddListener(() => ActivateTool(ToolType.Vertical));
        horizontal.onClick.AddListener(() => ActivateTool(ToolType.Horizontal));
        colorBomb.onClick.AddListener(() => ActivateTool(ToolType.ColorBomb));
        UpdateButtonVisuals();
    }

    public ToolType GetRandomToolType()
    {
        // Enum deðerlerini al (None hariç)
        ToolType[] values = { ToolType.Hammer, ToolType.Vertical, ToolType.Horizontal , ToolType.ColorBomb };

        // Rastgele bir index seç
        int randomIndex = Random.Range(0, values.Length);

        // O indexteki deðeri döndür
        return values[randomIndex];
    }

    private void Update()
    {
        // Gem seçimini dinle
        if (currentButtonState == ButtonState.WaitingForGem)
        {
            HandleGemSelection();
        }

        // ESC tuþu ile iptal
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelTool();
        }
    }

    public void ActivateTool(ToolType toolType)
    {
        // Kullaným sayýsýný kontrol et
        if (!CanUseTool(toolType))
            return;

        // Önceki tool'u iptal et
        if (currentButtonState == ButtonState.WaitingForGem)
        {
            CancelTool();
        }

        activeToolType = toolType;
        currentButtonState = ButtonState.WaitingForGem;

        // Görsel feedback
        HighlightActiveButton(toolType);

        Debug.Log($"{toolType} tool activated. Click on a gem!");
    }

    private bool CanUseTool(ToolType toolType)
    {
        switch (toolType)
        {
            case ToolType.Hammer:
                return hammercount > 0;
            case ToolType.Vertical:
                return verticalcount > 0;
            case ToolType.Horizontal:
                return horizontalcount > 0;
            case ToolType.ColorBomb:
                return colorbombcount > 0;
            default:
                return false;
        }
    }

    private void HandleGemSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                Gem targetGem = hit.collider.GetComponent<Gem>();
                Debug.Log("Gem");
                if (targetGem != null)
                {
                    ExecuteTool(targetGem);
                }
            }
        }
    }

    private void ExecuteTool(Gem targetGem)
    {
        if (gridSystem.moveAttempts <= 0)
            return;
        // Her durumda 1 hamle harcansýn
        if (gridSystem.moveAttempts > 0)
        {
            gridSystem.moveAttempts--;
            gridSystem.attempText.text = gridSystem.moveAttempts.ToString(); // UI güncelle
        }

        switch (activeToolType)
        {
            case ToolType.Hammer:
                HammerEffect(targetGem);
                hammercount--;
                hammertext.text = hammercount.ToString();
                button.Play();
                break;

            case ToolType.Vertical:
                VerticalEffect(targetGem);
                verticalcount--;
                verticaltext.text = verticalcount.ToString();
                button.Play();
                break;

            case ToolType.Horizontal:
                HorizontalEffect(targetGem);
                horizontalcount--;
                horizontaltext.text = horizontalcount.ToString();
                button.Play();
                break;
            case ToolType.ColorBomb:
                ColorbombEffect(targetGem);
                colorbombcount--;
                colorBombtext.text = colorbombcount.ToString();
                button.Play();
                break;
        }

        // Tool'u sýfýrla
        ResetTool();
    }

    private void ColorbombEffect(Gem targetGem)
    {
        List<Gem> gemsToDestroy = new List<Gem>();

        if (targetGem.IsColored())
        {
            ColorGem.ColorType targetColor = targetGem.ColorComponent.Color;

            // Ayný renkteki tüm gem'leri bul
            for (int x = 0; x < gridSystem.xDim; x++)
            {
                for (int y = 0; y < gridSystem.yDim; y++)
                {
                    Gem gem = GetGemAt(x, y);

                    // Gem varsa, renkli ise, temizlenebilir ise ve ayný renkte ise
                    if (gem != null && gem.IsColored() && gem.IsClearable() &&
                        gem.ColorComponent.Color == targetColor)
                    {
                        gemsToDestroy.Add(gem); // Gem objesini ekle, rengi deðil
                    }
                }
            }

            // Bulunan gem'leri yok et
            StartCoroutine(ClearGemList(gemsToDestroy));

            Debug.Log($"Color bomb used! Destroyed {gemsToDestroy.Count} {targetColor} gems");
        }
    }
    private void HammerEffect(Gem targetGem)
    {
        // Tek gem'i yok et
        if (targetGem.IsClearable())
        {
            StartCoroutine(ClearSurroundingGems(targetGem));
        }

        Debug.Log($"Hammer used on gem at ({targetGem.X}, {targetGem.Y})");
    }

    private void VerticalEffect(Gem targetGem)
    {
        // Dikey sütunu yok et
        int targetX = targetGem.X;

        List<Gem> gemsToDestroy = new List<Gem>();

        // Sütundaki tüm gem'leri bul
        for (int y = 0; y < gridSystem.yDim; y++)
        {
            Gem gem = GetGemAt(targetX, y);
            if (gem != null && gem.IsClearable())
            {
                gemsToDestroy.Add(gem);
            }
        }

        // Gem'leri yok et
        StartCoroutine(ClearGemList(gemsToDestroy));

        Debug.Log($"Vertical tool used on column {targetX}");
    }

    private void HorizontalEffect(Gem targetGem)
    {
        // Yatay satýrý yok et
        int targetY = targetGem.Y;

        List<Gem> gemsToDestroy = new List<Gem>();

        // Satýrdaki tüm gem'leri bul
        for (int x = 0; x < gridSystem.xDim; x++)
        {
            Gem gem = GetGemAt(x, targetY);
            if (gem != null && gem.IsClearable())
            {
                gemsToDestroy.Add(gem);
            }
        }

        // Gem'leri yok et
        StartCoroutine(ClearGemList(gemsToDestroy));

        Debug.Log($"Horizontal tool used on row {targetY}");
    }

    private Gem GetGemAt(int x, int y)
    {
        // GridSystem'den gem'i al (reflection kullanarak private field'a eriþim)
        var gemsField = typeof(GridSystem).GetField("gems",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (gemsField != null)
        {
            Gem[,] gems = (Gem[,])gemsField.GetValue(gridSystem);

            if (x >= 0 && x < gems.GetLength(0) && y >= 0 && y < gems.GetLength(1))
            {
                return gems[x, y];
            }
        }

        return null;
    }

    private IEnumerator ClearSurroundingGems(Gem centerGem)
    {
        int centerX = centerGem.X;
        int centerY = centerGem.Y;

        // 3x3 alaný tarar
        for (int x = centerX - 1; x <= centerX + 1; x++)
        {
            for (int y = centerY - 1; y <= centerY + 1; y++)
            {
                // Grid sýnýrlarý içinde mi kontrol et
                if (gridSystem.IsWithinBounds(x, y))
                {
                    Gem gemToClear = gridSystem.GetGemAt(x, y);

                    if (gemToClear != null && gemToClear.ClearableComponent != null)
                    {
                        gemToClear.ClearableComponent.Clear();
                    }
                }
            }
        }

        // Bekleme süresi
        yield return new WaitForSeconds(gridSystem.fillTime);

        // Boþ gemleri oluþtur
        for (int x = centerX - 1; x <= centerX + 1; x++)
        {
            for (int y = centerY - 1; y <= centerY + 1; y++)
            {
                if (gridSystem.IsWithinBounds(x, y))
                {
                    gridSystem.SpawnNewGem(x, y, GridSystem.GemType.EMPTY);
                }
            }
        }

        // Grid'i doldur
        yield return StartCoroutine(gridSystem.Fill());
    }

    private IEnumerator ClearGemList(List<Gem> gemsToDestroy)
    {
        foreach (Gem gem in gemsToDestroy)
        {
            if (gem.ClearableComponent != null)
            {
                gem.ClearableComponent.Clear();
            }
        }

        yield return new WaitForSeconds(gridSystem.fillTime);

        // Boþ gem'leri spawn et
        foreach (Gem gem in gemsToDestroy)
        {
            gridSystem.SpawnNewGem(gem.X, gem.Y, GridSystem.GemType.EMPTY);
        }

        // Grid'i doldur
        StartCoroutine(gridSystem.Fill());
    }

    private void CancelTool()
    {
        currentButtonState = ButtonState.Ready;
        activeToolType = ToolType.None;
        UpdateButtonVisuals();

        Debug.Log("Tool cancelled");
    }

    private void ResetTool()
    {
        currentButtonState = ButtonState.Ready;
        activeToolType = ToolType.None;
        UpdateButtonVisuals();
    }

    private void HighlightActiveButton(ToolType toolType)
    {
        // Tüm butonlarý normal renge döndür
        ResetButtonColors();

        // Aktif butonu highlight et
        switch (toolType)
        {
            case ToolType.Hammer:
                hammer.image.color = Color.yellow;
                break;
            case ToolType.Vertical:
                vertical.image.color = Color.yellow;
                break;
            case ToolType.Horizontal:
                horizontal.image.color = Color.yellow;
                break;
        }
    }

    private void ResetButtonColors()
    {
        // Her buton için ayrý ayrý kontrol et
        SetButtonColor(ToolType.Hammer);
        SetButtonColor(ToolType.Vertical);
        SetButtonColor(ToolType.Horizontal);
    }

    private void SetButtonColor(ToolType toolType)
    {
        Button targetButton = null;
        int count = 0;

        switch (toolType)
        {
            case ToolType.Hammer:
                targetButton = hammer;
                count = hammercount;
                break;
            case ToolType.Vertical:
                targetButton = vertical;
                count = verticalcount;
                break;
            case ToolType.Horizontal:
                targetButton = horizontal;
                count = horizontalcount;
                break;
        }

        if (targetButton != null)
        {
            if (count <= 0)
            {
                targetButton.image.color = Color.gray;
            }
            else
            {
                targetButton.image.color = Color.white;
            }
        }
    }

    private void UpdateButtonVisuals()
    {
        // Interactable durumlarýný güncelle
        hammer.interactable = hammercount > 0 && currentButtonState != ButtonState.WaitingForGem;
        vertical.interactable = verticalcount > 0 && currentButtonState != ButtonState.WaitingForGem;
        horizontal.interactable = horizontalcount > 0 && currentButtonState != ButtonState.WaitingForGem;

        // Sadece aktif tool yoksa renkleri güncelle
        if (currentButtonState != ButtonState.WaitingForGem)
        {
            ResetButtonColors();
        }
    }

    // Public metodlar - UI'dan kullaným sayýlarýný göstermek için
    public int GetHammerCount() => hammercount;
    public int GetVerticalCount() => verticalcount;
    public int GetHorizontalCount() => horizontalcount;

    // Tool sayýlarýný artýrmak için (satýn alma sistemi için)
    public void AddHammer(int count)
    {
        hammercount += count;
        hammertext.text = hammercount.ToString();
        UpdateButtonVisuals();
    }

    public void AddVertical(int count)
    {
        verticalcount += count;
        verticaltext.text = verticalcount.ToString();
        UpdateButtonVisuals();
    }

    public void AddHorizontal(int count)
    {
        horizontalcount += count;
        horizontaltext.text = horizontalcount.ToString();
        UpdateButtonVisuals();
    }
}