using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Match3Game
{
    public class GridSystem : MonoBehaviour
    {
        private bool colliderMovedAtOne = false;
        private bool slowed = false;
        private bool soruSoruldu = false;
        public SoruPopUp soruPopUp;
        public bool? isMovetimeOver;
        [SerializeField] public TextMeshProUGUI attempText;
        private bool isSpawningGem = false;
        public InputDisabler inputDisabler;
        [SerializeField] public int Default_moveAttempts = 5;
       public int moveAttempts;
        private float moveTimer = 15f; // 10 saniye s�resi
        private float currentMoveTime;
        private GameState currentState = GameState.WaitingForMove;

        public enum GemType
        {
            NORMAL,
            EMPTY,
            OBSTACLE
        };

        [System.Serializable]
        public struct GemPrefab
        {
            public GemType type;
            public GameObject prefab;
        };

        [SerializeField] public int xDim;
        [SerializeField] public int yDim;
        public float fillTime;

        [SerializeField] GemPrefab[] gemPrefabs;
        [SerializeField] GameObject[] backgroundPrefab;

        // Grid boyutu i�in �l�ek fakt�r�
        [SerializeField] private float gridScale = 0.5f;

        private Dictionary<GemType, GameObject> gemPrefabDic;

        private Gem[,] gems;

        private bool inverse = false;

        // Y�n tabanl� hareket i�in yeni de�i�kenler
        public Gem selectedGem;
        private bool isGemSelected = false;

        // Drag ve Swipe i�in yeni de�i�kenler
        private Vector2 startMousePos;
        private Vector2 currentMousePos;
        private bool isDragging = false;
        private Gem draggedGem = null;
        [SerializeField] private float minSwipeDistance = 0.5f; // Minimum swipe mesafesi

        // Y�n enum'u
        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }
        public enum GameState
        {
            WaitingForMove,   // Oyuncu hamle yapabilir
            MoveTimeExpired,  // 10 saniye ge�ti
            NoMovesLeft,      // Hamle hakk� bitti
            AskingQuestion    // Soru soruluyor
        }

        private void Start()
        {
            inputDisabler.EnableInput();
            currentMoveTime = moveTimer;
            moveAttempts = Default_moveAttempts;
            attempText.text = moveAttempts.ToString();
            gemPrefabDic = new Dictionary<GemType, GameObject>();
            for (int i = 0; i < gemPrefabs.Length; i++)
            {
                if (!gemPrefabDic.ContainsKey(gemPrefabs[i].type))
                {
                    gemPrefabDic.Add(gemPrefabs[i].type, gemPrefabs[i].prefab);
                }
            }

            if (backgroundPrefab.Length > 0)
            {
                GameObject backGroundParent = new GameObject("BackgroundPrefabs");
                backGroundParent.transform.SetParent(transform);

                for (int x = 0; x < xDim; x++)
                {
                    for (int y = 0; y < yDim; y++)
                    {
                        Vector3 spawnPosition = GetWorldPosition(x, y);
                        GameObject background = Instantiate(backgroundPrefab[0], spawnPosition, Quaternion.identity);
                        background.transform.parent = backGroundParent.transform;

                        // Background prefab�n� grid �l�e�ine g�re boyutland�r
                        background.transform.localScale = Vector3.one * gridScale;
                    }
                }
            }

            gems = new Gem[xDim, yDim];
            for (int x = 0; x < xDim; x++)
            {
                for (int y = 0; y < yDim; y++)
                {
                    SpawnNewGem(x, y, GemType.EMPTY);
                }
            }

            StartCoroutine(Fill());
        }

        private void Update()
        {
            HandleDragAndSwipe();
            ColliderController(); 
            SlowDownController();
           
            // Zaman� �al��t�r
            if (currentState == GameState.WaitingForMove)
            {
                currentMoveTime -= Time.deltaTime;
                if (currentMoveTime <= 0f)
                {
                    currentState = GameState.MoveTimeExpired;
                    moveAttempts = 0;
                    attempText.text = moveAttempts.ToString();
                }
            }

            // Hamle hakk� bitti�inde soru sorulmas�
            if ((moveAttempts == 0 && !soruSoruldu) &&
                (currentState == GameState.MoveTimeExpired || currentState == GameState.NoMovesLeft))
            {
                soruSoruldu = true;
                currentState = GameState.AskingQuestion;
                StartCoroutine(AskQuestionAndResetTimer());
            }

            // Hamle yeniden verilirse zaman s�f�rlanmal�
            if (moveAttempts > 0 && soruSoruldu)
            {
                soruSoruldu = false;
                currentState = GameState.WaitingForMove;
                currentMoveTime = moveTimer;
            }
        }


        private IEnumerator AskQuestionAndResetTimer()
        {
            yield return StartCoroutine(soruPopUp.Soru());

            // Do�ru cevap verildiyse zaman yeniden ba�lat�l�r
            currentState = GameState.WaitingForMove;
            currentMoveTime = moveTimer;
            moveAttempts = Default_moveAttempts;
            attempText.text = moveAttempts.ToString();
        }

        private void HandleDragAndSwipe()
        {
            // Mouse/Touch ba�lang�c�
            if (Input.GetMouseButtonDown(0))
            {
                StartDrag();
            }
            // Mouse/Touch hareket halinde
            else if (Input.GetMouseButton(0) && isDragging)
            {
                UpdateDrag();
            }
            // Mouse/Touch b�rak�ld���nda
            else if (Input.GetMouseButtonUp(0))
            {
                EndDrag();
            }
        }

        private void StartDrag()
        {
            startMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentMousePos = startMousePos;

            // Hangi gem'e t�kland���n� bul
            RaycastHit2D hit = Physics2D.Raycast(startMousePos, Vector2.zero);
            if (hit.collider != null)
            {
                Gem clickedGem = hit.collider.GetComponent<Gem>();
                if (clickedGem != null && clickedGem.IsMovable() && clickedGem.GemType != GemType.EMPTY)
                {
                    draggedGem = clickedGem;
                    isDragging = true;
                    SelectGem(clickedGem);
                    Debug.Log($"Started dragging gem at ({clickedGem.X}, {clickedGem.Y})");
                }
            }
        }

        private void UpdateDrag()
        {
            if (draggedGem == null) return;

            currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Drag mesafesini hesapla
            Vector2 dragVector = currentMousePos - startMousePos;

            // Minimum mesafeyi ge�tiyse y�n belirle
            if (dragVector.magnitude >= minSwipeDistance)
            {
                Direction swipeDirection = GetDirectionFromDragVector(dragVector);

                // G�rsel feedback i�in gem'in hangi y�ne hareket edece�ini belirt
                ShowDragPreview(draggedGem, swipeDirection);
            }
        }

        private void EndDrag()
        {
            if (!isDragging || draggedGem == null)
            {
                isDragging = false;
                draggedGem = null;
                return;
            }

            Vector2 dragVector = currentMousePos - startMousePos;

            // Yeterli mesafe kayd�r�ld�ysa hareketi ger�ekle�tir
            if (dragVector.magnitude >= minSwipeDistance)
            {
                Direction swipeDirection = GetDirectionFromDragVector(dragVector);
                MoveGemInDirection(draggedGem, swipeDirection);
                Debug.Log($"Swiped {swipeDirection} with distance: {dragVector.magnitude}");
            }
            else
            {
                // Yeterli mesafe kayd�r�lmad�ysa sadece gem se�imini koru
                Debug.Log("Swipe distance too short, gem remains selected");
            }

            // Drag durumunu s�f�rla
            isDragging = false;
            draggedGem = null;
            HideDragPreview();
        }

        private Direction GetDirectionFromDragVector(Vector2 dragVector)
        {
            // En bask�n y�n� belirle
            if (Mathf.Abs(dragVector.x) > Mathf.Abs(dragVector.y))
            {
                // Yatay hareket
                return dragVector.x > 0 ? Direction.Right : Direction.Left;
            }
            else
            {
                // Dikey hareket - Unity'de Y ekseni yukar� do�ru pozitif
                return dragVector.y > 0 ? Direction.Up : Direction.Down;
            }
        }

        private void ShowDragPreview(Gem gem, Direction direction)
        {
            // Hangi gem'e hareket edece�ini g�ster (iste�e ba�l� g�rsel feedback)
            Gem targetGem = GetAdjacentGemInDirection(gem, direction);
            if (targetGem != null)
            {
                // Hedef gem'i hafif�e highlight et
                SpriteRenderer targetRenderer = targetGem.GetComponent<SpriteRenderer>();
                if (targetRenderer != null)
                {
                    targetRenderer.color = Color.cyan;
                }
            }
        }
        
        private void HideDragPreview()
        {
            // T�m gem'lerin renklerini normale d�nd�r
            for (int x = 0; x < xDim; x++)
            {
                for (int y = 0; y < yDim; y++)
                {
                    if (gems[x, y] != null && gems[x, y] != selectedGem)
                    {
                        SpriteRenderer renderer = gems[x, y].GetComponent<SpriteRenderer>();
                        if (renderer != null)
                        {
                            renderer.color = Color.white;
                        }
                    }
                }
            }
        }

        private void HandleMouseInput()
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                Gem clickedGem = hit.collider.GetComponent<Gem>();
                if (clickedGem != null && clickedGem.IsMovable())
                {
                    SelectGem(clickedGem);
                }
            }
        }

        private Direction GetDirectionFromVector(Vector2 vector)
        {
            if (vector.y > 0) return Direction.Up;
            if (vector.y < 0) return Direction.Down;
            if (vector.x < 0) return Direction.Left;
            if (vector.x > 0) return Direction.Right;

            return Direction.Up; // Default
        }

        public void SelectGem(Gem gem)
        {
            // �nceki se�imi temizle
            if (selectedGem != null)
            {
                DeselectGem(selectedGem);
            }

            selectedGem = gem;
            isGemSelected = true;

            // G�rsel feedback (gem'i highlight et)
            HighlightGem(gem, true);

            Debug.Log($"Gem selected at ({gem.X}, {gem.Y})");
        }

        public void DeselectAllGems()
        {
            if (selectedGem != null)
            {
                DeselectGem(selectedGem);
            }
            selectedGem = null;
            isGemSelected = false;
        }

        private void DeselectGem(Gem gem)
        {
            HighlightGem(gem, false);
        }

        private void HighlightGem(Gem gem, bool highlight)
        {
            // Gem'in g�rsel feedback'ini ayarla
            SpriteRenderer spriteRenderer = gem.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                if (highlight)
                {
                    spriteRenderer.color = Color.yellow; // Se�ili gem sar�
                }
                else
                {
                    spriteRenderer.color = Color.white; // Normal renk
                }
            }
        }

        public void MoveGemInDirection(Gem gem, Direction direction)
        {
            if (gem == null || !gem.IsMovable()) return;

            Gem targetGem = GetAdjacentGemInDirection(gem, direction);

            if (targetGem != null)
            {
                Debug.Log($"Moving gem from ({gem.X}, {gem.Y}) to ({targetGem.X}, {targetGem.Y})");
                SwapGems(gem, targetGem);

                // Hareket sonras� se�imi temizle
                DeselectAllGems();
            }
            else
            {
                Debug.Log($"No valid gem found in {direction} direction from ({gem.X}, {gem.Y})");
            }
        }

        private Gem GetAdjacentGemInDirection(Gem gem, Direction direction)
        {
            int newX = gem.X;
            int newY = gem.Y;

            // Y�nlere g�re koordinat de�i�imi
            switch (direction)
            {
                case Direction.Up:
                    newY -= 1; // Grid'de Y ekseninde yukar� -1
                    break;
                case Direction.Down:
                    newY += 1; // Grid'de Y ekseninde a�a�� +1
                    break;
                case Direction.Left:
                    newX -= 1;
                    break;
                case Direction.Right:
                    newX += 1;
                    break;
            }

            // S�n�r kontrol�
            if (IsWithinBounds(newX, newY))
            {
                Gem adjacentGem = GetGemAt(newX, newY);

                // Bo� gem de�ilse ve hareket ettirilebilirse
                if (adjacentGem != null && adjacentGem.GemType != GemType.EMPTY &&
                    adjacentGem.GemType != GemType.OBSTACLE)
                {
                    return adjacentGem;
                }
            }

            return null;
        }

        // Keyboard shortcuts i�in gem se�imi
        public void SelectGemByCoordinates(int x, int y)
        {
            if (IsWithinBounds(x, y))
            {
                Gem gem = GetGemAt(x, y);
                if (gem != null && gem.IsMovable())
                {
                    SelectGem(gem);
                }
            }
        }

        // Grid'de rastgele bir gem se� (oyun ba�lang�c� i�in)
        public void SelectRandomGem()
        {
            List<Gem> movableGems = new List<Gem>();

            for (int x = 0; x < xDim; x++)
            {
                for (int y = 0; y < yDim; y++)
                {
                    Gem gem = GetGemAt(x, y);
                    if (gem != null && gem.IsMovable() && gem.GemType != GemType.EMPTY)
                    {
                        movableGems.Add(gem);
                    }
                }
            }

            if (movableGems.Count > 0)
            {
                int randomIndex = Random.Range(0, movableGems.Count);
                SelectGem(movableGems[randomIndex]);
            }
        }

        public bool IsWithinBounds(int x, int y)
        {
            return x >= 0 && x < xDim && y >= 0 && y < yDim;
        }

        public Gem GetGemAt(int x, int y)
        {
            if (IsWithinBounds(x, y))
            {
                return gems[x, y];
            }
            return null;
        }

        public IEnumerator Fill()
        {
            if (inputDisabler != null)
                inputDisabler.DisableInput();

            bool needsRefill = true;

            while (needsRefill)
            {
                yield return new WaitForSeconds(fillTime);
                while (FillStep())
                {
                    inverse = !inverse;
                    yield return new WaitForSeconds(fillTime);
                }

                needsRefill = ClearAllValidMatches();
            }

            if (inputDisabler != null)
                inputDisabler.EnableInput();
        }

        public bool FillStep()
        {
            bool movedGem = false;
            // Move gems down from top to bottom
            for (int y = yDim - 2; y >= 0; y--)
            {
                for (int loopX = 0; loopX < xDim; loopX++)
                {
                    int x = loopX;

                    if (inverse)
                    {
                        x = xDim - 1 - loopX;
                    }

                    Gem gem = gems[x, y]; // Get the gem at the current position

                    if (gem.IsMovable()) // Check if the gem can move
                    {
                        Gem gemBelow = gems[x, y + 1]; // Get the gem directly below the current one

                        if (gemBelow.GemType == GemType.EMPTY) // If the spot below is empty
                        {
                            Destroy(gemBelow.gameObject);
                            gem.MovableComponent.Move(x, y + 1, fillTime); // Move the gem down
                            gems[x, y + 1] = gem; // Update the grid to reflect the new position
                            SpawnNewGem(x, y, GemType.EMPTY); // Replace the old spot with an empty gem
                            movedGem = true;
                        }
                        else
                        {
                            for (int diag = -1; diag <= 1; diag++)
                            {
                                if (diag != 0)
                                {
                                    int diagX = x + diag;

                                    if (inverse)
                                    {
                                        diagX = x - diag;
                                    }

                                    if (diagX >= 0 && diagX < xDim)
                                    {
                                        Gem diagonalGem = gems[diagX, y + 1];

                                        if (diagonalGem.GemType == GemType.EMPTY)
                                        {
                                            bool hasGemAbove = true;

                                            for (int aboveY = y; aboveY >= 0; aboveY--)
                                            {
                                                Gem gemAbove = gems[diagX, aboveY];

                                                if (gemAbove.IsMovable())
                                                {
                                                    break;
                                                }
                                                else if (!gemAbove.IsMovable() && gemAbove.GemType != GemType.EMPTY)
                                                {
                                                    hasGemAbove = false;
                                                    break;
                                                }
                                            }

                                            if (!hasGemAbove)
                                            {
                                                Destroy(diagonalGem.gameObject);
                                                gem.MovableComponent.Move(diagX, y + 1, fillTime);
                                                gems[diagX, y + 1] = gem;
                                                SpawnNewGem(x, y, GemType.EMPTY);
                                                movedGem = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            for (int x = 0; x < xDim; x++) // Fill bottom row with new gems where empty
            {
                Gem gemBelow = gems[x, 0];  // Get the gem at the bottom row

                if (gemBelow.GemType == GemType.EMPTY) // If the spot is empty
                {
                    // Create a new gem above the grid (at y = -1 for animation purposes)
                    GameObject newGem = Instantiate(gemPrefabDic[GemType.NORMAL], GetWorldPosition(x, -1), Quaternion.identity);
                    newGem.transform.parent = this.transform;

                    // Gem boyutunu grid �l�e�ine g�re ayarla
                    newGem.transform.localScale = Vector3.one * gridScale;

                    // Initialize the new gem and assign it to the grid
                    gems[x, 0] = newGem.GetComponent<Gem>();
                    gems[x, 0].Init(x, -1, this, GemType.NORMAL);
                    gems[x, 0].MovableComponent.Move(x, 0, fillTime); // Move it to the correct position on the grid
                    gems[x, 0].ColorComponent.SetColor((ColorGem.ColorType)Random.Range(0, gems[x, 0].ColorComponent.NumColors));
                    movedGem = true;
                }
            }

            return movedGem;
        }

        public Vector2 GetWorldPosition(int x, int y)
        {
            // Grid �l�e�i ile d�zeltilmi� pozisyon hesaplama
            float spacing = gridScale; // Gemler aras� mesafe grid �l�e�i ile orant�l�
            return new Vector2(transform.position.x - (xDim * spacing) / 2.0f + x * spacing,
                transform.position.y + (yDim * spacing) / 2.0f - y * spacing);
        }

        public Gem SpawnNewGem(int x, int y, GemType type)
        {
            isSpawningGem = true; // Spawn ba�l�yor

            Vector3 spawnPosition = GetWorldPosition(x, y);
            GameObject newGem = Instantiate(gemPrefabDic[type], spawnPosition, Quaternion.identity);
            newGem.transform.parent = transform;
            newGem.transform.localScale = Vector3.one * gridScale;

            gems[x, y] = newGem.GetComponent<Gem>();
            gems[x, y].Init(x, y, this, type);

            isSpawningGem = false; // Spawn bitti

            return gems[x, y];
        }
        public void SlowDownController()
        {
            
            if (moveAttempts ==3 && !slowed)
            {
                StartCoroutine(soruPopUp.SlowDown());
                slowed = true;
            }
            if (moveAttempts != 3) 
            {
                slowed = false;
            }

        }
        public void ColliderController()
        {
            if (moveAttempts == 1 && !colliderMovedAtOne)
            {
                colliderMovedAtOne = true;
                soruPopUp.MoveColliderTemporarily();
            }

            // moveAttempts tekrar 1 d���na ��karsa flag s�f�rlans�n
            if (moveAttempts != 1)
            {
                colliderMovedAtOne = false;
            }
        }
        public bool IsAdjacent(Gem gem1, Gem gem2)
        {
            return (gem1.X == gem2.X && (int)Mathf.Abs(gem1.Y - gem2.Y) == 1) || (gem1.Y == gem2.Y && (int)Mathf.Abs(gem1.X - gem2.X) == 1);
        }

        public void SwapGems(Gem gem1, Gem gem2)
        {
            if (currentState != GameState.WaitingForMove) return;
            if (gem1.IsMovable() && gem2.IsMovable() && moveAttempts > 0)
            {
                gems[gem1.X, gem1.Y] = gem2;
                gems[gem2.X, gem2.Y] = gem1;

                List<Gem> match1 = GetMatch(gem1, gem2.X, gem2.Y);
                List<Gem> match2 = GetMatch(gem2, gem1.X, gem1.Y);

                if (match1 != null || match2 != null)
                {
                    List<Gem> mainMatch = match1 != null ? match1 : match2;
                    Gem centerGem = mainMatch[0];
                    lastMatchColor = centerGem.ColorComponent.Color;

                    int gem1X = gem1.X;
                    int gem1Y = gem1.Y;

                    gem1.MovableComponent.Move(gem2.X, gem2.Y, fillTime);
                    gem2.MovableComponent.Move(gem1X, gem1Y, fillTime);
                    moveAttempts -= 1;
                    attempText.text = moveAttempts.ToString();
                    ClearAllValidMatches();
                    StartCoroutine(Fill());
                    Debug.Log("Color" + lastMatchColor);
                }
                else
                {
                    SimulateSwapAndReturn(gem1, gem2, fillTime);
                }
            }
        }

        public void SimulateSwapAndReturn(Gem gem1, Gem gem2, float fillTime)
        {
            int gem1X = gem1.X;
            int gem1Y = gem1.Y;
            int gem2X = gem2.X;
            int gem2Y = gem2.Y;

            gem1.MovableComponent.Move(gem2.X, gem2.Y, fillTime);
            gem2.MovableComponent.Move(gem1X, gem1Y, fillTime);

            gems[gem1.X, gem1.Y] = gem2;
            gems[gem2.X, gem2.Y] = gem1;

            StartCoroutine(ReturnToOriginalPositionAfterDelay(gem1, gem2, gem1X, gem1Y, gem2X, gem2Y, fillTime));
        }

        private IEnumerator ReturnToOriginalPositionAfterDelay(Gem gem1, Gem gem2, int gem1X, int gem1Y, int gem2X, int gem2Y, float fillTime)
        {
            yield return new WaitForSeconds(fillTime);

            gem1.MovableComponent.Move(gem1X, gem1Y, fillTime);
            gem2.MovableComponent.Move(gem2X, gem2Y, fillTime);

            gems[gem1X, gem1Y] = gem1;
            gems[gem2X, gem2Y] = gem2;
        }

        #region Legacy Mouse Input (Backward Compatibility)
        public void PressGem(Gem gem)
        {
            SelectGem(gem);
        }

        public void EnterGem(Gem gem)
        {
            // Art�k kullan�lm�yor ama geriye uyumluluk i�in
        }

        public void ReleaseGem()
        {
            // Art�k kullan�lm�yor ama geriye uyumluluk i�in
        }
        #endregion

        #region Match Gems

        public List<Gem> GetMatch(Gem gem, int newX, int newY)
        {
            if (gem.IsColored())
            {
                ColorGem.ColorType color = gem.ColorComponent.Color;
                List<Gem> horizontalGems = new List<Gem>();
                List<Gem> verticalGems = new List<Gem>();
                List<Gem> matchingGems = new List<Gem>();

                horizontalGems.Add(gem);
                for (int dir = 0; dir <= 1; dir++)
                {
                    for (int xOffset = 1; xOffset < xDim; xOffset++)
                    {
                        int x;
                        if (dir == 0) // Left
                        {
                            x = newX - xOffset;
                        }
                        else // Right
                        {
                            x = newX + xOffset;
                        }

                        if (x < 0 || x >= xDim) { break; }

                        if (gems[x, newY].IsColored() && gems[x, newY].ColorComponent.Color == color)
                        {
                            horizontalGems.Add(gems[x, newY]);
                        }
                        else { break; }
                    }
                }

                if (horizontalGems.Count >= 3)
                {
                    for (int i = 0; i < horizontalGems.Count; i++)
                    {
                        matchingGems.Add(horizontalGems[i]);
                    }
                }

                //Traverse vertically if we found a match (for T and L shape)
                if (horizontalGems.Count >= 3)
                {
                    for (int i = 0; i < horizontalGems.Count; i++)
                    {
                        for (int dir = 0; dir <= 1; dir++)
                        {
                            for (int yOffset = 1; yOffset < yDim; yOffset++)
                            {
                                int y;
                                if (dir == 0)
                                { y = newY - yOffset; }
                                else { y = newY + yOffset; }

                                if (y < 0 || y >= yDim) { break; }

                                if (gems[horizontalGems[i].X, y].IsColored() && gems[horizontalGems[i].X, y].ColorComponent.Color == color)
                                {
                                    verticalGems.Add(gems[horizontalGems[i].X, y]);
                                }
                                else { break; }
                            }
                        }

                        if (verticalGems.Count < 2)
                        {
                            verticalGems.Clear();
                        }
                        else
                        {
                            for (int j = 0; j < verticalGems.Count; j++)
                            {
                                matchingGems.Add(verticalGems[j]);
                            }
                            break;
                        }
                    }
                }

                if (matchingGems.Count >= 3)
                {
                    return matchingGems;
                }

                horizontalGems.Clear();
                verticalGems.Clear();
                // Check vertically
                verticalGems.Add(gem);
                for (int dir = 0; dir <= 1; dir++)
                {
                    for (int yOffset = 1; yOffset < yDim; yOffset++)
                    {
                        int y;
                        if (dir == 0) // Up
                        {
                            y = newY - yOffset;
                        }
                        else // Down
                        {
                            y = newY + yOffset;
                        }

                        if (y < 0 || y >= yDim) { break; }

                        if (gems[newX, y].IsColored() && gems[newX, y].ColorComponent.Color == color)
                        {
                            verticalGems.Add(gems[newX, y]);
                        }
                        else { break; }
                    }
                }

                if (verticalGems.Count >= 3)
                {
                    for (int i = 0; i < verticalGems.Count; i++)
                    {
                        matchingGems.Add(verticalGems[i]);
                    }
                }

                //Traverse horizontally if we found a match (for T and L shape)
                if (verticalGems.Count >= 3)
                {
                    for (int i = 0; i < verticalGems.Count; i++)
                    {
                        for (int dir = 0; dir <= 1; dir++)
                        {
                            for (int xOffset = 1; xOffset < xDim; xOffset++)
                            {
                                int x;
                                if (dir == 0) // Left
                                { x = newX - xOffset; }
                                else { x = newX + xOffset; } // Right

                                if (x < 0 || x >= xDim) { break; }

                                if (gems[x, verticalGems[i].Y].IsColored() && gems[x, verticalGems[i].Y].ColorComponent.Color == color)
                                {
                                    horizontalGems.Add(gems[x, verticalGems[i].Y]);
                                }
                                else { break; }
                            }
                        }

                        if (horizontalGems.Count < 2)
                        {
                            horizontalGems.Clear();
                        }
                        else
                        {
                            for (int j = 0; j < horizontalGems.Count; j++)
                            {
                                matchingGems.Add(horizontalGems[j]);
                            }
                            break;
                        }
                    }
                }

                // 2x2 kare kontrol� - grid �l�e�i dikkate al�narak
                int[,] offsets = new int[,] {
                    { 0, 0 },
                    { -1, 0 },
                    { 0, -1 },
                    { -1, -1 }
                };

                for (int i = 0; i < 4; i++)
                {
                    int startX = newX + offsets[i, 0];
                    int startY = newY + offsets[i, 1];

                    // Tahta s�n�rlar�n� kontrol et
                    if (startX < 0 || startX >= xDim - 1 || startY < 0 || startY >= yDim - 1)
                        continue;

                    Gem g1 = gems[startX, startY];
                    Gem g2 = gems[startX + 1, startY];
                    Gem g3 = gems[startX, startY + 1];
                    Gem g4 = gems[startX + 1, startY + 1];

                    if (g1.IsColored() && g2.IsColored() && g3.IsColored() && g4.IsColored())
                    {
                        if (g1.ColorComponent.Color == g2.ColorComponent.Color &&
                            g1.ColorComponent.Color == g3.ColorComponent.Color &&
                            g1.ColorComponent.Color == g4.ColorComponent.Color)
                        {
                            if (!matchingGems.Contains(g1)) matchingGems.Add(g1);
                            if (!matchingGems.Contains(g2)) matchingGems.Add(g2);
                            if (!matchingGems.Contains(g3)) matchingGems.Add(g3);
                            if (!matchingGems.Contains(g4)) matchingGems.Add(g4);
                        }
                    }
                }

                if (matchingGems.Count >= 3)
                {
                    return matchingGems;
                }
            }

            return null;
        }

        #endregion
        private ColorGem.ColorType? lastMatchColor = null;

        public bool ClearAllValidMatches()
        {
            bool needsRefill = false;

            for (int y = 0; y < yDim; y++)
            {
                for (int x = 0; x < xDim; x++)
                {
                    if (gems[x, y].IsClearable())
                    {
                        List<Gem> match = GetMatch(gems[x, y], x, y);
                        if (match != null)
                        {
                            for (int i = 0; i < match.Count; i++)
                            {
                                StartCoroutine(ClearGemRoutine(match[i].X, match[i].Y));
                                needsRefill = true;
                            }
                        }
                    }
                }
            }

            return needsRefill;
        }

        private IEnumerator ClearGemRoutine(int x, int y)
        {
            if (gems[x, y].IsClearable() && !gems[x, y].ClearableComponent.IsBeingCleared)
            {
                gems[x, y].ClearableComponent.Clear();
                yield return new WaitForSeconds(fillTime);
                SpawnNewGem(x, y, GemType.EMPTY);
            }
        }
    }
}