using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GenericCrosswordGame :
    MonoBehaviour
{
    private enum WordDirection
    {
        Horizontal,
        Vertical
    }

    [Header("JSON Resources")]
    [Tooltip(
        "Resources path without .json extension."
    )]
    [SerializeField]
    private string
        levelsJsonResourcePath =
            "LevelData/levels";

    [Tooltip(
        "Resources folder containing clue sprites."
    )]
    [SerializeField]
    private string
        clueImagesResourceFolder =
            "ClueImages";

    [Header("Starting Level")]
    [Min(0)]
    [SerializeField]
    private int
        startingLevelIndex = 0;

    [Header("Grid References")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField]
    private
        SimpleCrosswordCell cellPrefab;

    [Header("Letter Bank References")]
    [SerializeField]
    private Transform
        letterBankParent;

    [SerializeField]
    private Button
        letterButtonPrefab;

    [Tooltip(
        "Full-screen RectTransform used while dragging. " +
        "Create DragLayer under Canvas and assign it here."
    )]
    [SerializeField] private RectTransform dragLayer;

    [Header("Optional UI")]
    [SerializeField] private TMP_Text levelText;

    [Header("Level Progression")]
    [SerializeField]
    private bool
        autoLoadNextLevel = true;

    [Min(0f)]
    [SerializeField]
    private float
        nextLevelDelay = 1.5f;

    private CrosswordJsonDatabase database;
    private CrosswordJsonLevel currentLevel;
    private int currentLevelIndex;

    private char[,] solutionLetters;
    private bool[,] blankPositions;

    private readonly
        Dictionary<Vector2Int, Sprite>
        cluePositions =
            new Dictionary<Vector2Int, Sprite>();

    private readonly
        List<SimpleCrosswordCell>
        blankCells =
            new List<SimpleCrosswordCell>();

    private bool isCompletingLevel;

    public int TotalLevels =>
        database?.levels?.Length ?? 0;

    private void Awake()
    {
        if (!ValidateSceneReferences())
        {
            enabled = false;
            return;
        }

        if (!LoadJsonDatabase())
        {
            enabled = false;
        }
    }

    private void Start()
    {
        if (TotalLevels == 0)
        {
            Debug.LogError(
                "JSON does not contain any levels.",
                this
            );
            return;
        }

        int safeIndex =
            Mathf.Clamp(
                startingLevelIndex,
                0,
                TotalLevels - 1
            );

        LoadLevel(safeIndex);
    }

    private bool ValidateSceneReferences()
    {
        bool valid = true;

        if (gridParent == null)
        {
            Debug.LogError(
                "Grid Parent is not assigned.",
                this
            );
            valid = false;
        }

        if (gridLayout == null)
        {
            Debug.LogError(
                "Grid Layout is not assigned.",
                this
            );
            valid = false;
        }

        if (cellPrefab == null)
        {
            Debug.LogError(
                "Cell Prefab is not assigned.",
                this
            );
            valid = false;
        }

        if (letterBankParent == null)
        {
            Debug.LogError(
                "Letter Bank Parent is not assigned.",
                this
            );
            valid = false;
        }

        if (letterButtonPrefab == null)
        {
            Debug.LogError(
                "Letter Button Prefab is not assigned.",
                this
            );
            valid = false;
        }

        if (dragLayer == null)
        {
            Debug.LogWarning(
                "Drag Layer is not assigned. " +
                "The root Canvas will be used.",
                this
            );
        }

        return valid;
    }

    private bool LoadJsonDatabase()
    {
        TextAsset jsonFile =
            Resources.Load<TextAsset>(
                levelsJsonResourcePath
            );

        if (jsonFile == null)
        {
            Debug.LogError(
                "Could not find JSON at Resources/" +
                levelsJsonResourcePath +
                ".json",
                this
            );
            return false;
        }

        try
        {
            database =
                JsonUtility
                    .FromJson<CrosswordJsonDatabase>(
                        jsonFile.text
                    );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "JSON parsing failed:\n" +
                exception.Message,
                this
            );
            return false;
        }

        if (database == null ||
            database.levels == null ||
            database.levels.Length == 0)
        {
            Debug.LogError(
                "JSON levels array is empty.",
                this
            );
            return false;
        }

        Debug.Log(
            $"Loaded {database.levels.Length} levels."
        );

        return true;
    }

    public void LoadLevel(int levelIndex)
    {
        if (database == null ||
            database.levels == null)
        {
            return;
        }

        if (levelIndex < 0 ||
            levelIndex >= database.levels.Length)
        {
            Debug.LogError(
                $"Invalid level index: {levelIndex}",
                this
            );
            return;
        }

        StopAllCoroutines();

        currentLevelIndex = levelIndex;
        currentLevel =
            database.levels[currentLevelIndex];

        isCompletingLevel = false;

        ClearParent(gridParent);
        ClearParent(letterBankParent);

        blankCells.Clear();
        cluePositions.Clear();

        if (!BuildLevelMaps(currentLevel))
        {
            Debug.LogError(
                $"Level {currentLevel.levelNumber} " +
                "is invalid.",
                this
            );
            return;
        }

        CreateGrid();
        CreateLetterBank();
        UpdateLevelText();
        RebuildLayouts();
    }

    private bool BuildLevelMaps(
        CrosswordJsonLevel level)
    {
        if (level.rows <= 0 ||
            level.columns <= 0)
        {
            Debug.LogError(
                "Rows and columns must be above zero.",
                this
            );
            return false;
        }

        if (level.words == null ||
            level.words.Length == 0)
        {
            Debug.LogError(
                $"Level {level.levelNumber} has no words.",
                this
            );
            return false;
        }

        solutionLetters =
            new char[
                level.rows,
                level.columns
            ];

        blankPositions =
            new bool[
                level.rows,
                level.columns
            ];

        // Pass 1: place and validate letters.
        for (int wordIndex = 0;
             wordIndex < level.words.Length;
             wordIndex++)
        {
            CrosswordJsonWord wordData =
                level.words[wordIndex];

            if (wordData == null)
            {
                return false;
            }

            string normalizedWord =
                NormalizeWord(wordData.word);

            if (string.IsNullOrEmpty(
                normalizedWord))
            {
                Debug.LogError(
                    $"Word {wordIndex} is empty.",
                    this
                );
                return false;
            }

            if (!TryParseDirection(
                wordData.direction,
                out WordDirection direction))
            {
                Debug.LogError(
                    $"{normalizedWord}: use Horizontal " +
                    "or Vertical.",
                    this
                );
                return false;
            }

            int startRow =
                wordData.startRow - 1;

            int startColumn =
                wordData.startColumn - 1;

            for (int letterIndex = 0;
                 letterIndex <
                 normalizedWord.Length;
                 letterIndex++)
            {
                GetLetterPosition(
                    direction,
                    startRow,
                    startColumn,
                    letterIndex,
                    out int row,
                    out int column
                );

                if (!IsInsideGrid(
                    row,
                    column,
                    level.rows,
                    level.columns))
                {
                    Debug.LogError(
                        $"{normalizedWord} goes outside " +
                        $"the grid at row {row + 1}, " +
                        $"column {column + 1}.",
                        this
                    );
                    return false;
                }

                char newLetter =
                    normalizedWord[letterIndex];

                char existingLetter =
                    solutionLetters[row, column];

                if (existingLetter != '\0' &&
                    existingLetter != newLetter)
                {
                    Debug.LogError(
                        $"Invalid intersection at row " +
                        $"{row + 1}, column {column + 1}.",
                        this
                    );
                    return false;
                }

                solutionLetters[row, column] =
                    newLetter;
            }

            MarkBlankLetters(
                level,
                wordData,
                normalizedWord,
                direction,
                startRow,
                startColumn
            );
        }

        // Pass 2: place clue images.
        for (int wordIndex = 0;
             wordIndex < level.words.Length;
             wordIndex++)
        {
            CrosswordJsonWord wordData =
                level.words[wordIndex];

            string normalizedWord =
                NormalizeWord(wordData.word);

            TryParseDirection(
                wordData.direction,
                out WordDirection direction
            );

            int startRow =
                wordData.startRow - 1;

            int startColumn =
                wordData.startColumn - 1;

            GetCluePosition(
                direction,
                startRow,
                startColumn,
                out int clueRow,
                out int clueColumn
            );

            if (!IsInsideGrid(
                clueRow,
                clueColumn,
                level.rows,
                level.columns))
            {
                Debug.LogError(
                    $"{normalizedWord} clue is outside grid.",
                    this
                );
                return false;
            }

            if (solutionLetters[
                clueRow,
                clueColumn] != '\0')
            {
                Debug.LogError(
                    $"{normalizedWord} clue overlaps a letter.",
                    this
                );
                return false;
            }

            Vector2Int cluePosition =
                new Vector2Int(
                    clueColumn,
                    clueRow
                );

            if (cluePositions.ContainsKey(
                cluePosition))
            {
                Debug.LogError(
                    "Two clue images use the same cell.",
                    this
                );
                return false;
            }

            Sprite clueSprite =
                LoadClueSprite(
                    wordData.imageName
                );

            cluePositions.Add(
                cluePosition,
                clueSprite
            );
        }

        return true;
    }

    private void MarkBlankLetters(
        CrosswordJsonLevel level,
        CrosswordJsonWord wordData,
        string normalizedWord,
        WordDirection direction,
        int startRow,
        int startColumn)
    {
        if (wordData.blankLetterNumbers == null)
        {
            return;
        }

        foreach (int blankNumber
                 in wordData.blankLetterNumbers)
        {
            int letterIndex =
                blankNumber - 1;

            if (letterIndex < 0 ||
                letterIndex >=
                normalizedWord.Length)
            {
                Debug.LogWarning(
                    $"{normalizedWord}: invalid blank " +
                    $"number {blankNumber}.",
                    this
                );
                continue;
            }

            GetLetterPosition(
                direction,
                startRow,
                startColumn,
                letterIndex,
                out int row,
                out int column
            );

            blankPositions[row, column] =
                true;
        }
    }

    private Sprite LoadClueSprite(
        string imageName)
    {
        if (string.IsNullOrWhiteSpace(
            imageName))
        {
            return null;
        }

        string path =
            $"{clueImagesResourceFolder}/" +
            imageName.Trim();

        Sprite sprite =
            Resources.Load<Sprite>(path);

        if (sprite == null)
        {
            Debug.LogError(
                $"Missing clue sprite: Resources/{path}",
                this
            );
        }

        return sprite;
    }

    private void CreateGrid()
    {
        gridLayout.constraint =
            GridLayoutGroup.Constraint
                .FixedColumnCount;

        gridLayout.constraintCount =
            currentLevel.columns;

        for (int row = 0;
             row < currentLevel.rows;
             row++)
        {
            for (int column = 0;
                 column < currentLevel.columns;
                 column++)
            {
                Vector2Int position =
                    new Vector2Int(
                        column,
                        row
                    );

                SimpleCrosswordCell cell =
                    Instantiate(
                        cellPrefab,
                        gridParent,
                        false
                    );

                cell.gameObject.name =
                    $"Cell_R{row + 1}_C{column + 1}";

                if (cluePositions.TryGetValue(
                    position,
                    out Sprite clueSprite))
                {
                    cell.InitializeClue(
                        position,
                        clueSprite
                    );
                    continue;
                }

                char letter =
                    solutionLetters[
                        row,
                        column
                    ];

                if (letter == '\0')
                {
                    cell.InitializeBlocked(
                        position
                    );
                    continue;
                }

                if (blankPositions[
                    row,
                    column])
                {
                    cell.InitializeBlank(
                        position,
                        letter,
                        OnBlankCellCompleted
                    );

                    blankCells.Add(cell);
                }
                else
                {
                    cell.InitializeFixed(
                        position,
                        letter
                    );
                }
            }
        }
    }

    private void CreateLetterBank()
    {
        List<char> options =
            GenerateDragLetterOptions();

        ShuffleLetters(options);

        foreach (char letter in options)
        {
            char savedLetter = letter;

            Button button =
                Instantiate(
                    letterButtonPrefab,
                    letterBankParent,
                    false
                );

            button.gameObject.name =
                $"LetterOption_{savedLetter}";

            button.onClick.RemoveAllListeners();

            TMP_Text text =
                button
                    .GetComponentInChildren
                    <TMP_Text>(true);

            if (text != null)
            {
                text.text =
                    savedLetter.ToString();

                text.raycastTarget = false;
            }

            DraggableLetter draggable =
                button.GetComponent
                    <DraggableLetter>();

            if (draggable == null)
            {
                Debug.LogError(
                    "Add DraggableLetter to " +
                    "LetterButtonPrefab.",
                    button
                );
                continue;
            }

            draggable.Initialize(
                savedLetter,
                dragLayer
            );
        }
    }

    private List<char>
        GenerateDragLetterOptions()
    {
        List<char> options =
            new List<char>();

        /*
         * Add one tile for every blank cell.
         * Duplicate answers are intentionally kept.
         *
         * Example blanks: A, E, E
         * Generated correct tiles: A, E, E
         */
        for (int row = 0;
             row < currentLevel.rows;
             row++)
        {
            for (int column = 0;
                 column <
                 currentLevel.columns;
                 column++)
            {
                if (!blankPositions[
                    row,
                    column])
                {
                    continue;
                }

                char correctLetter =
                    solutionLetters[
                        row,
                        column
                    ];

                if (correctLetter != '\0')
                {
                    options.Add(
                        correctLetter
                    );
                }
            }
        }

        int requiredCount =
            Mathf.Max(
                currentLevel
                    .totalLetterOptions,
                options.Count
            );

        List<char> distractorPool =
            CreateDistractorPool(
                currentLevel
                    .distractorAlphabet,
                options
            );

        ShuffleLetters(
            distractorPool
        );

        foreach (char distractor
                 in distractorPool)
        {
            if (options.Count >=
                requiredCount)
            {
                break;
            }

            options.Add(distractor);
        }

        const string fallback =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        foreach (char character in fallback)
        {
            if (options.Count >=
                requiredCount)
            {
                break;
            }

            if (!options.Contains(character))
            {
                options.Add(character);
            }
        }

        return options;
    }

    private List<char> CreateDistractorPool(
        string alphabet,
        List<char> correctLetters)
    {
        List<char> pool =
            new List<char>();

        if (string.IsNullOrEmpty(alphabet))
        {
            return pool;
        }

        foreach (char character in alphabet)
        {
            if (!char.IsLetter(character))
            {
                continue;
            }

            char upper =
                char.ToUpperInvariant(
                    character
                );

            // Do not create extra copies of
            // correct answer letters as distractors.
            if (correctLetters.Contains(upper))
            {
                continue;
            }

            if (!pool.Contains(upper))
            {
                pool.Add(upper);
            }
        }

        return pool;
    }

    private void OnBlankCellCompleted(
        SimpleCrosswordCell cell)
    {
        CheckLevelComplete();
    }

    private void CheckLevelComplete()
    {
        if (isCompletingLevel)
        {
            return;
        }

        foreach (SimpleCrosswordCell cell
                 in blankCells)
        {
            if (!cell.IsCompleted)
            {
                return;
            }
        }

        isCompletingLevel = true;

        Debug.Log(
            $"Level {currentLevel.levelNumber} Complete!"
        );

        if (autoLoadNextLevel)
        {
            StartCoroutine(
                LoadNextLevelRoutine()
            );
        }
    }

    private IEnumerator
        LoadNextLevelRoutine()
    {
        yield return new WaitForSeconds(
            nextLevelDelay
        );

        LoadNextLevel();
    }

    public void LoadNextLevel()
    {
        int nextIndex =
            currentLevelIndex + 1;

        if (nextIndex >= TotalLevels)
        {
            Debug.Log(
                "All levels completed!"
            );

            isCompletingLevel = false;
            return;
        }

        LoadLevel(nextIndex);
    }

    public void RestartCurrentLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    private void UpdateLevelText()
    {
        if (levelText == null)
        {
            return;
        }

        levelText.text =
            $"Level {currentLevelIndex + 1} / " +
            $"{TotalLevels}";
    }

    private void RebuildLayouts()
    {
        Canvas.ForceUpdateCanvases();

        if (gridParent is
            RectTransform gridRect)
        {
            LayoutRebuilder
                .ForceRebuildLayoutImmediate(
                    gridRect
                );
        }

        if (letterBankParent is
            RectTransform bankRect)
        {
            LayoutRebuilder
                .ForceRebuildLayoutImmediate(
                    bankRect
                );
        }
    }

    private bool TryParseDirection(
        string directionText,
        out WordDirection direction)
    {
        if (string.Equals(
            directionText,
            "Horizontal",
            StringComparison.OrdinalIgnoreCase))
        {
            direction =
                WordDirection.Horizontal;
            return true;
        }

        if (string.Equals(
            directionText,
            "Vertical",
            StringComparison.OrdinalIgnoreCase))
        {
            direction =
                WordDirection.Vertical;
            return true;
        }

        direction =
            WordDirection.Horizontal;
        return false;
    }

    private void GetLetterPosition(
        WordDirection direction,
        int startRow,
        int startColumn,
        int letterIndex,
        out int row,
        out int column)
    {
        row = startRow;
        column = startColumn;

        if (direction ==
            WordDirection.Horizontal)
        {
            column += letterIndex;
        }
        else
        {
            row += letterIndex;
        }
    }

    private void GetCluePosition(
        WordDirection direction,
        int startRow,
        int startColumn,
        out int clueRow,
        out int clueColumn)
    {
        clueRow = startRow;
        clueColumn = startColumn;

        if (direction ==
            WordDirection.Horizontal)
        {
            clueColumn--;
        }
        else
        {
            clueRow--;
        }
    }

    private bool IsInsideGrid(
        int row,
        int column,
        int totalRows,
        int totalColumns)
    {
        return row >= 0 &&
               row < totalRows &&
               column >= 0 &&
               column < totalColumns;
    }

    private string NormalizeWord(
        string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return string.Empty;
        }

        StringBuilder builder =
            new StringBuilder();

        foreach (char character in word)
        {
            if (char.IsLetter(character))
            {
                builder.Append(
                    char.ToUpperInvariant(
                        character
                    )
                );
            }
        }

        return builder.ToString();
    }

    private void ShuffleLetters(
        List<char> letters)
    {
        for (int i = 0;
             i < letters.Count;
             i++)
        {
            int randomIndex =
                UnityEngine.Random.Range(
                    i,
                    letters.Count
                );

            char temporary =
                letters[i];

            letters[i] =
                letters[randomIndex];

            letters[randomIndex] =
                temporary;
        }
    }

    private void ClearParent(
        Transform parent)
    {
        if (parent == null)
        {
            return;
        }

        for (int i =
                 parent.childCount - 1;
             i >= 0;
             i--)
        {
            GameObject child =
                parent
                    .GetChild(i)
                    .gameObject;

            child.SetActive(false);
            Destroy(child);
        }
    }
}