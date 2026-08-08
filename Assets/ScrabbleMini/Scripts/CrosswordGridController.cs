//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//namespace ScrabbleMini
//{
//    public class CrosswordGridController : MonoBehaviour
//    {
//        [Header("Grid References")]
//        [SerializeField] private RectTransform gridContainer;
//        [SerializeField] private CrosswordCell cellPrefab;
//        [SerializeField] private GridLayoutGroup gridLayoutGroup;

//        [Header("Grid Settings")]
//        [Min(1)]
//        [SerializeField] private int rows = 6;

//        [Min(1)]
//        [SerializeField] private int columns = 6;

//        [Header("Level Testing")]
//        [SerializeField] private bool useRandomLayout = true;

//        [Tooltip("Used only when Random Layout is disabled.")]
//        [Min(0)]
//        [SerializeField] private int selectedLayoutIndex;

//        [SerializeField] private bool revealAllLettersForTesting;

//        private CrosswordCell[,] cells;

//        private readonly Dictionary<string, List<CrosswordCell>>
//            wordCells = new();

//        private readonly HashSet<string> solvedWords = new();

//        private int currentLayoutIndex;

//        public int CurrentLayoutIndex => currentLayoutIndex;
//        public int SolvedWordCount => solvedWords.Count;
//        public int TargetWordCount => wordCells.Count;

//        private void Start()
//        {
//            GenerateGrid();
//            CreateLevelOne();
//        }

//        #region Grid Generation

//        public void GenerateGrid()
//        {
//            if (!ValidateReferences())
//                return;

//            ClearGrid();

//            gridLayoutGroup.constraint =
//                GridLayoutGroup.Constraint.FixedColumnCount;

//            gridLayoutGroup.constraintCount = columns;

//            cells = new CrosswordCell[rows, columns];

//            for (int row = 0; row < rows; row++)
//            {
//                for (int column = 0; column < columns; column++)
//                {
//                    CrosswordCell newCell = Instantiate(
//                        cellPrefab,
//                        gridContainer
//                    );

//                    newCell.gameObject.SetActive(true);
//                    newCell.name = $"Cell_{row}_{column}";

//                    newCell.InitializeEmpty(row, column);

//                    cells[row, column] = newCell;
//                }
//            }

//            Debug.Log(
//                $"Crossword grid generated: {rows} × {columns}",
//                gameObject
//            );
//        }

//        private bool ValidateReferences()
//        {
//            if (gridContainer == null)
//            {
//                Debug.LogError(
//                    "Grid Container reference is missing.",
//                    gameObject
//                );

//                return false;
//            }

//            if (cellPrefab == null)
//            {
//                Debug.LogError(
//                    "Crossword Cell prefab reference is missing.",
//                    gameObject
//                );

//                return false;
//            }

//            if (gridLayoutGroup == null)
//            {
//                gridLayoutGroup =
//                    gridContainer.GetComponent<GridLayoutGroup>();
//            }

//            if (gridLayoutGroup == null)
//            {
//                Debug.LogError(
//                    "GridLayoutGroup is missing from GridContainer.",
//                    gridContainer
//                );

//                return false;
//            }

//            if (rows <= 0 || columns <= 0)
//            {
//                Debug.LogError(
//                    "Rows and Columns must be greater than zero.",
//                    gameObject
//                );

//                return false;
//            }

//            return true;
//        }

//        #endregion

//        #region Level One

//        private void CreateLevelOne()
//        {
//            if (cells == null)
//            {
//                Debug.LogError(
//                    "Grid must be generated before creating the level.",
//                    gameObject
//                );

//                return;
//            }

//            wordCells.Clear();
//            solvedWords.Clear();

//            List<List<WordPlacementData>> layouts =
//                GetLevelOneLayouts();

//            if (layouts == null || layouts.Count == 0)
//            {
//                Debug.LogError(
//                    "No Level 1 crossword layouts were found.",
//                    gameObject
//                );

//                return;
//            }

//            if (useRandomLayout)
//            {
//                currentLayoutIndex =
//                    Random.Range(0, layouts.Count);
//            }
//            else
//            {
//                currentLayoutIndex = Mathf.Clamp(
//                    selectedLayoutIndex,
//                    0,
//                    layouts.Count - 1
//                );
//            }

//            List<WordPlacementData> selectedLayout =
//                layouts[currentLayoutIndex];

//            bool completeLayoutPlaced = true;

//            foreach (WordPlacementData placement in selectedLayout)
//            {
//                bool placed = PlaceWord(placement);

//                if (!placed)
//                {
//                    completeLayoutPlaced = false;

//                    Debug.LogError(
//                        $"Could not place word: {placement.word}",
//                        gameObject
//                    );
//                }
//            }

//            if (completeLayoutPlaced)
//            {
//                Debug.Log(
//                    $"Level 1 created with Shape {currentLayoutIndex + 1}.",
//                    gameObject
//                );
//            }

//            if (revealAllLettersForTesting)
//            {
//                RevealAllLetters();
//            }
//        }

//        private List<List<WordPlacementData>> GetLevelOneLayouts()
//        {
//            return new List<List<WordPlacementData>>
//            {
//                /*
//                 * Shape 1
//                 *
//                 * G A M E
//                 * E G
//                 * M E
//                 */
//                new List<WordPlacementData>
//                {
//                    new WordPlacementData(
//                        "GAME",
//                        1,
//                        1,
//                        WordDirection.Horizontal
//                    ),

//                    new WordPlacementData(
//                        "GEM",
//                        1,
//                        1,
//                        WordDirection.Vertical
//                    ),

//                    new WordPlacementData(
//                        "AGE",
//                        1,
//                        2,
//                        WordDirection.Vertical
//                    )
//                },

//                /*
//                 * Shape 2
//                 *
//                 * G E M
//                 * A G E
//                 * M
//                 * E
//                 */
//                new List<WordPlacementData>
//                {
//                    new WordPlacementData(
//                        "GAME",
//                        0,
//                        2,
//                        WordDirection.Vertical
//                    ),

//                    new WordPlacementData(
//                        "GEM",
//                        0,
//                        2,
//                        WordDirection.Horizontal
//                    ),

//                    new WordPlacementData(
//                        "AGE",
//                        1,
//                        2,
//                        WordDirection.Horizontal
//                    )
//                },

//                /*
//                 * Shape 3
//                 *
//                 *     G
//                 * A   E
//                 * G A M E
//                 * E
//                 */
//                new List<WordPlacementData>
//                {
//                    new WordPlacementData(
//                        "GAME",
//                        2,
//                        1,
//                        WordDirection.Horizontal
//                    ),

//                    new WordPlacementData(
//                        "GEM",
//                        0,
//                        3,
//                        WordDirection.Vertical
//                    ),

//                    new WordPlacementData(
//                        "AGE",
//                        1,
//                        1,
//                        WordDirection.Vertical
//                    )
//                },

//                /*
//                 * Shape 4
//                 *
//                 *     G A
//                 *     E G
//                 * G A M E
//                 */
//                new List<WordPlacementData>
//                {
//                    new WordPlacementData(
//                        "GAME",
//                        2,
//                        1,
//                        WordDirection.Horizontal
//                    ),

//                    new WordPlacementData(
//                        "GEM",
//                        0,
//                        3,
//                        WordDirection.Vertical
//                    ),

//                    new WordPlacementData(
//                        "AGE",
//                        0,
//                        4,
//                        WordDirection.Vertical
//                    )
//                },

//                /*
//                 * Shape 5
//                 *
//                 * G
//                 * A G E
//                 * M E
//                 * E M
//                 */
//                new List<WordPlacementData>
//                {
//                    new WordPlacementData(
//                        "AGE",
//                        1,
//                        1,
//                        WordDirection.Horizontal
//                    ),

//                    new WordPlacementData(
//                        "GAME",
//                        0,
//                        1,
//                        WordDirection.Vertical
//                    ),

//                    new WordPlacementData(
//                        "GEM",
//                        1,
//                        2,
//                        WordDirection.Vertical
//                    )
//                },

//                /*
//                 * Shape 6
//                 *
//                 *     E
//                 * A G E
//                 *   A M
//                 *   M
//                 *   E
//                 */
//                new List<WordPlacementData>
//                {
//                    new WordPlacementData(
//                        "AGE",
//                        1,
//                        1,
//                        WordDirection.Horizontal
//                    ),

//                    new WordPlacementData(
//                        "GAME",
//                        1,
//                        2,
//                        WordDirection.Vertical
//                    ),

//                    new WordPlacementData(
//                        "GEM",
//                        0,
//                        3,
//                        WordDirection.Vertical
//                    )
//                }
//            };
//        }

//        #endregion

//        #region Word Placement

//        public bool PlaceWord(WordPlacementData placement)
//        {
//            if (placement == null || cells == null)
//                return false;

//            string normalizedWord =
//                NormalizeWord(placement.word);

//            if (string.IsNullOrEmpty(normalizedWord))
//            {
//                Debug.LogError(
//                    "Cannot place an empty word.",
//                    gameObject
//                );

//                return false;
//            }

//            if (wordCells.ContainsKey(normalizedWord))
//            {
//                Debug.LogWarning(
//                    $"Word is already placed: {normalizedWord}",
//                    gameObject
//                );

//                return false;
//            }

//            bool canPlace = CanPlaceWord(
//                normalizedWord,
//                placement.startRow,
//                placement.startColumn,
//                placement.direction
//            );

//            if (!canPlace)
//                return false;

//            List<CrosswordCell> placedCells = new();

//            for (int letterIndex = 0;
//                 letterIndex < normalizedWord.Length;
//                 letterIndex++)
//            {
//                GetCellCoordinates(
//                    placement.startRow,
//                    placement.startColumn,
//                    placement.direction,
//                    letterIndex,
//                    out int row,
//                    out int column
//                );

//                CrosswordCell cell = cells[row, column];

//                cell.SetAsWordCell(
//                    normalizedWord[letterIndex]
//                );

//                placedCells.Add(cell);
//            }

//            wordCells.Add(
//                normalizedWord,
//                placedCells
//            );

//            Debug.Log(
//                $"Placed word: {normalizedWord}",
//                gameObject
//            );

//            return true;
//        }

//        private bool CanPlaceWord(
//            string word,
//            int startRow,
//            int startColumn,
//            WordDirection direction)
//        {
//            for (int letterIndex = 0;
//                 letterIndex < word.Length;
//                 letterIndex++)
//            {
//                GetCellCoordinates(
//                    startRow,
//                    startColumn,
//                    direction,
//                    letterIndex,
//                    out int row,
//                    out int column
//                );

//                if (!IsInsideGrid(row, column))
//                {
//                    Debug.LogError(
//                        $"Word {word} is outside the grid at " +
//                        $"Row {row}, Column {column}.",
//                        gameObject
//                    );

//                    return false;
//                }

//                CrosswordCell cell = cells[row, column];

//                if (cell == null)
//                {
//                    Debug.LogError(
//                        $"Grid cell is missing at Row {row}, " +
//                        $"Column {column}.",
//                        gameObject
//                    );

//                    return false;
//                }

//                if (cell.IsActiveCell &&
//                    cell.CorrectLetter != word[letterIndex])
//                {
//                    Debug.LogError(
//                        $"Invalid collision while placing {word}. " +
//                        $"Grid has {cell.CorrectLetter}, but word " +
//                        $"requires {word[letterIndex]} at " +
//                        $"Row {row}, Column {column}.",
//                        gameObject
//                    );

//                    return false;
//                }
//            }

//            return true;
//        }

//        private static void GetCellCoordinates(
//            int startRow,
//            int startColumn,
//            WordDirection direction,
//            int letterIndex,
//            out int row,
//            out int column)
//        {
//            row = startRow;
//            column = startColumn;

//            switch (direction)
//            {
//                case WordDirection.Horizontal:
//                    column += letterIndex;
//                    break;

//                case WordDirection.Vertical:
//                    row += letterIndex;
//                    break;
//            }
//        }

//        private bool IsInsideGrid(int row, int column)
//        {
//            return row >= 0 &&
//                   row < rows &&
//                   column >= 0 &&
//                   column < columns;
//        }

//        #endregion

//        #region Word Reveal

//        public bool TryRevealWord(string submittedWord)
//        {
//            string normalizedWord =
//                NormalizeWord(submittedWord);

//            if (string.IsNullOrEmpty(normalizedWord))
//                return false;

//            if (solvedWords.Contains(normalizedWord))
//            {
//                Debug.Log(
//                    $"Word already solved: {normalizedWord}",
//                    gameObject
//                );

//                return false;
//            }

//            if (!wordCells.TryGetValue(
//                    normalizedWord,
//                    out List<CrosswordCell> cellsForWord))
//            {
//                Debug.Log(
//                    $"Not a target word: {normalizedWord}",
//                    gameObject
//                );

//                return false;
//            }

//            foreach (CrosswordCell cell in cellsForWord)
//            {
//                if (cell != null)
//                {
//                    cell.Reveal();
//                }
//            }

//            solvedWords.Add(normalizedWord);

//            Debug.Log(
//                $"Correct word revealed: {normalizedWord}",
//                gameObject
//            );

//            CheckLevelComplete();

//            return true;
//        }

//        public bool IsTargetWord(string word)
//        {
//            string normalizedWord =
//                NormalizeWord(word);

//            return wordCells.ContainsKey(normalizedWord);
//        }

//        public bool IsWordSolved(string word)
//        {
//            string normalizedWord =
//                NormalizeWord(word);

//            return solvedWords.Contains(normalizedWord);
//        }

//        private void CheckLevelComplete()
//        {
//            if (wordCells.Count == 0)
//                return;

//            if (solvedWords.Count != wordCells.Count)
//                return;

//            Debug.Log(
//                "Level Complete!",
//                gameObject
//            );

//            // Next step mein yahan:
//            // Level complete panel
//            // Coins
//            // Next level
//            // Adaptive difficulty
//            // add ki jayegi.
//        }

//        #endregion

//        #region Testing and Reloading

//        [ContextMenu("Reveal All Letters")]
//        public void RevealAllLetters()
//        {
//            if (cells == null)
//                return;

//            foreach (CrosswordCell cell in cells)
//            {
//                if (cell != null && cell.IsActiveCell)
//                {
//                    cell.Reveal();
//                }
//            }
//        }

//        [ContextMenu("Hide All Letters")]
//        public void HideAllLetters()
//        {
//            if (cells == null)
//                return;

//            foreach (CrosswordCell cell in cells)
//            {
//                if (cell != null && cell.IsActiveCell)
//                {
//                    cell.HideLetter();
//                }
//            }

//            solvedWords.Clear();
//        }

//        [ContextMenu("Generate New Random Shape")]
//        public void GenerateNewRandomShape()
//        {
//            GenerateGrid();
//            CreateLevelOne();
//        }

//        #endregion

//        #region Utility

//        private void ClearGrid()
//        {
//            wordCells.Clear();
//            solvedWords.Clear();

//            if (gridContainer == null)
//                return;

//            for (int index = gridContainer.childCount - 1;
//                 index >= 0;
//                 index--)
//            {
//                GameObject child =
//                    gridContainer.GetChild(index).gameObject;

//                if (Application.isPlaying)
//                {
//                    Destroy(child);
//                }
//                else
//                {
//                    DestroyImmediate(child);
//                }
//            }

//            cells = null;
//        }

//        private static string NormalizeWord(string value)
//        {
//            if (string.IsNullOrWhiteSpace(value))
//                return string.Empty;

//            return value
//                .Trim()
//                .ToUpperInvariant();
//        }

//        #endregion
//    }
//}



using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ScrabbleMini
{
    public class CrosswordGridController : MonoBehaviour
    {
        [Header("Grid References")]
        [SerializeField] private RectTransform gridContainer;
        [SerializeField] private CrosswordCell cellPrefab;
        [SerializeField] private GridLayoutGroup gridLayoutGroup;

        [Header("Grid Appearance")]
        [Min(10f)]
        [SerializeField] private float cellSize = 90f;

        [Min(0f)]
        [SerializeField] private float cellSpacing = 10f;

        [Header("Reveal Animation")]
        [Min(0f)]
        [SerializeField] private float letterRevealDelay = 0.08f;

        [Header("Testing")]
        [SerializeField] private bool revealAllForTesting;

        private CrosswordCell[,] cells;

        private readonly Dictionary<string, List<CrosswordCell>>
            wordCells = new();

        private readonly HashSet<string>
            solvedWords = new();

        private ScrabbleLevelData currentLevel;

        private bool isRevealing;

        public int SolvedWordCount => solvedWords.Count;
        public int TargetWordCount => wordCells.Count;
        public bool IsRevealing => isRevealing;

        public event Action OnLevelCompleted;

        public void LoadLevel(
            ScrabbleLevelData levelData)
        {
            if (levelData == null)
            {
                Debug.LogError(
                    "Cannot load a null level.",
                    gameObject
                );

                return;
            }

            currentLevel = levelData;

            int rows = Mathf.Max(1, levelData.rows);
            int columns = Mathf.Max(1, levelData.columns);

            GenerateGrid(rows, columns);

            if (levelData.words == null ||
                levelData.words.Count == 0)
            {
                Debug.LogError(
                    $"Level {levelData.levelId} has no words.",
                    gameObject
                );

                return;
            }

            foreach (WordPlacementData placement
                     in levelData.words)
            {
                PlaceWord(placement);
            }

            if (revealAllForTesting)
            {
                RevealAllLetters();
            }

            Debug.Log(
                $"Loaded Level {levelData.levelId}: " +
                $"{levelData.letters}",
                gameObject
            );
        }

        private void GenerateGrid(
            int rows,
            int columns)
        {
            if (!ValidateReferences())
                return;

            ClearGrid();

            gridLayoutGroup.constraint =
                GridLayoutGroup.Constraint.FixedColumnCount;

            gridLayoutGroup.constraintCount =
                columns;

            gridLayoutGroup.cellSize =
                new Vector2(cellSize, cellSize);

            gridLayoutGroup.spacing =
                new Vector2(
                    cellSpacing,
                    cellSpacing
                );

            cells =
                new CrosswordCell[rows, columns];

            for (int row = 0;
                 row < rows;
                 row++)
            {
                for (int column = 0;
                     column < columns;
                     column++)
                {
                    CrosswordCell newCell =
                        Instantiate(
                            cellPrefab,
                            gridContainer
                        );

                    newCell.gameObject.SetActive(true);

                    newCell.name =
                        $"Cell_{row}_{column}";

                    newCell.InitializeEmpty(
                        row,
                        column
                    );

                    cells[row, column] =
                        newCell;
                }
            }

            ResizeGridContainer(
                rows,
                columns
            );
        }

        private void ResizeGridContainer(
            int rows,
            int columns)
        {
            if (gridContainer == null)
                return;

            float width =
                columns * cellSize +
                Mathf.Max(0, columns - 1) *
                cellSpacing;

            float height =
                rows * cellSize +
                Mathf.Max(0, rows - 1) *
                cellSpacing;

            gridContainer.sizeDelta =
                new Vector2(width, height);
        }

        private bool PlaceWord(
            WordPlacementData placement)
        {
            if (placement == null ||
                cells == null)
            {
                return false;
            }

            string word =
                NormalizeWord(placement.word);

            if (string.IsNullOrEmpty(word))
            {
                Debug.LogError(
                    "Cannot place an empty word.",
                    gameObject
                );

                return false;
            }

            if (wordCells.ContainsKey(word))
            {
                Debug.LogWarning(
                    $"Duplicate target word: {word}",
                    gameObject
                );

                return false;
            }

            if (!CanPlaceWord(word, placement))
            {
                Debug.LogError(
                    $"Invalid placement for word: {word}",
                    gameObject
                );

                return false;
            }

            List<CrosswordCell> placedCells =
                new List<CrosswordCell>();

            for (int index = 0;
                 index < word.Length;
                 index++)
            {
                GetCoordinates(
                    placement,
                    index,
                    out int row,
                    out int column
                );

                CrosswordCell cell =
                    cells[row, column];

                cell.SetAsWordCell(word[index]);

                placedCells.Add(cell);
            }

            wordCells.Add(
                word,
                placedCells
            );

            return true;
        }

        private bool CanPlaceWord(
            string word,
            WordPlacementData placement)
        {
            int totalRows =
                cells.GetLength(0);

            int totalColumns =
                cells.GetLength(1);

            for (int index = 0;
                 index < word.Length;
                 index++)
            {
                GetCoordinates(
                    placement,
                    index,
                    out int row,
                    out int column
                );

                if (row < 0 ||
                    row >= totalRows ||
                    column < 0 ||
                    column >= totalColumns)
                {
                    return false;
                }

                CrosswordCell cell =
                    cells[row, column];

                if (cell.IsActiveCell &&
                    cell.CorrectLetter != word[index])
                {
                    return false;
                }
            }

            return true;
        }

        private static void GetCoordinates(
            WordPlacementData placement,
            int index,
            out int row,
            out int column)
        {
            row = placement.startRow;
            column = placement.startColumn;

            if (placement.direction ==
                WordDirection.Horizontal)
            {
                column += index;
            }
            else
            {
                row += index;
            }
        }

        public bool IsTargetWord(string word)
        {
            return wordCells.ContainsKey(
                NormalizeWord(word)
            );
        }

        public bool IsWordSolved(string word)
        {
            return solvedWords.Contains(
                NormalizeWord(word)
            );
        }

        public bool TryRevealWord(
            string submittedWord)
        {
            string word =
                NormalizeWord(submittedWord);

            if (string.IsNullOrEmpty(word))
                return false;

            if (isRevealing)
                return false;

            if (solvedWords.Contains(word))
                return false;

            if (!wordCells.TryGetValue(
                    word,
                    out List<CrosswordCell> cellsForWord))
            {
                return false;
            }

            StartCoroutine(
                RevealWordRoutine(
                    word,
                    cellsForWord
                )
            );

            return true;
        }

        private IEnumerator RevealWordRoutine(
            string word,
            List<CrosswordCell> cellsForWord)
        {
            isRevealing = true;

            foreach (CrosswordCell cell
                     in cellsForWord)
            {
                if (cell != null &&
                    !cell.IsRevealed)
                {
                    cell.Reveal();

                    if (letterRevealDelay > 0f)
                    {
                        yield return
                            new WaitForSeconds(
                                letterRevealDelay
                            );
                    }
                }
            }

            solvedWords.Add(word);

            isRevealing = false;

            Debug.Log(
                $"Solved word: {word}",
                gameObject
            );

            CheckLevelComplete();
        }

        private void CheckLevelComplete()
        {
            if (wordCells.Count == 0)
                return;

            if (solvedWords.Count <
                wordCells.Count)
            {
                return;
            }

            Debug.Log(
                $"Level {currentLevel.levelId} Complete!",
                gameObject
            );

            OnLevelCompleted?.Invoke();
        }

        [ContextMenu("Reveal All Letters")]
        public void RevealAllLetters()
        {
            if (cells == null)
                return;

            foreach (CrosswordCell cell
                     in cells)
            {
                if (cell != null &&
                    cell.IsActiveCell)
                {
                    cell.Reveal();
                }
            }
        }

        private bool ValidateReferences()
        {
            if (gridContainer == null)
            {
                Debug.LogError(
                    "Grid Container is missing.",
                    gameObject
                );

                return false;
            }

            if (cellPrefab == null)
            {
                Debug.LogError(
                    "Cell Prefab is missing.",
                    gameObject
                );

                return false;
            }

            if (gridLayoutGroup == null)
            {
                gridLayoutGroup =
                    gridContainer.GetComponent<GridLayoutGroup>();
            }

            if (gridLayoutGroup == null)
            {
                Debug.LogError(
                    "GridLayoutGroup is missing.",
                    gameObject
                );

                return false;
            }

            return true;
        }

        private void ClearGrid()
        {
            StopAllCoroutines();

            isRevealing = false;

            wordCells.Clear();
            solvedWords.Clear();

            if (gridContainer != null)
            {
                for (int index =
                         gridContainer.childCount - 1;
                     index >= 0;
                     index--)
                {
                    Destroy(
                        gridContainer
                            .GetChild(index)
                            .gameObject
                    );
                }
            }

            cells = null;
        }

        private static string NormalizeWord(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value
                .Trim()
                .ToUpperInvariant();
        }
    }
}