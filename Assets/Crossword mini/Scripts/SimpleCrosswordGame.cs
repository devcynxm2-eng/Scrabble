//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class SimpleCrosswordGame : MonoBehaviour
//{
//    private enum ClueType
//    {
//        Cat,
//        Tree,
//        Red
//    }

//    [Header("Grid References")]
//    [SerializeField] private Transform gridParent;
//    [SerializeField] private GridLayoutGroup gridLayout;
//    [SerializeField] private SimpleCrosswordCell cellPrefab;

//    [Header("Letter Bank References")]
//    [SerializeField] private Transform letterBankParent;
//    [SerializeField] private Button letterButtonPrefab;

//    [Header("Letter Options")]
//    [Range(5, 6)]
//    [SerializeField] private int totalLetterOptions = 6;

//    [Tooltip("Wrong letters will be selected from this alphabet.")]
//    [SerializeField]
//    private string availableAlphabet =
//        "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

//    [Header("Clue Images")]
//    [SerializeField] private Sprite catSprite;
//    [SerializeField] private Sprite treeSprite;
//    [SerializeField] private Sprite redSprite;

//    private SimpleCrosswordCell selectedCell;

//    private readonly List<SimpleCrosswordCell> blankCells =
//        new List<SimpleCrosswordCell>();

//    /*
//        Final layout:

//                      [TREE IMAGE]
//        [CAT IMAGE]   C   A   T
//        [RED IMAGE]           R   E   D
//                              E
//                              E

//        Words:
//        CAT  = Horizontal
//        TREE = Vertical
//        RED  = Horizontal
//    */

//    private readonly string[] solutionGrid =
//    {
//        "......",
//        ".CAT..",
//        "...RED",
//        "...E..",
//        "...E.."
//    };

//    /*
//        X = Column
//        Y = Row

//        Missing letters:
//        A in CAT
//        E in RED
//        Last E in TREE
//    */

//    private readonly HashSet<Vector2Int> blankPositions =
//        new HashSet<Vector2Int>
//        {
//            new Vector2Int(2, 1), // A in CAT
//            new Vector2Int(4, 2), // E in RED
//            new Vector2Int(3, 4)  // Last E in TREE
//        };

//    /*
//        Image clue positions:
//        Cat image before CAT
//        Tree image above TREE
//        Red image before RED
//    */

//    private readonly Dictionary<Vector2Int, ClueType> cluePositions =
//        new Dictionary<Vector2Int, ClueType>
//        {
//            {
//                new Vector2Int(0, 1),
//                ClueType.Cat
//            },
//            {
//                new Vector2Int(3, 0),
//                ClueType.Tree
//            },
//            {
//                new Vector2Int(2, 2),
//                ClueType.Red
//            }
//        };

//    private void Awake()
//    {
//        if (!ValidateReferences())
//        {
//            enabled = false;
//        }
//    }

//    private void Start()
//    {
//        CreateLevel();
//    }

//    private bool ValidateReferences()
//    {
//        bool valid = true;

//        if (gridParent == null)
//        {
//            Debug.LogError(
//                "Grid Parent is not assigned.",
//                this
//            );

//            valid = false;
//        }

//        if (gridLayout == null)
//        {
//            Debug.LogError(
//                "Grid Layout is not assigned.",
//                this
//            );

//            valid = false;
//        }

//        if (cellPrefab == null)
//        {
//            Debug.LogError(
//                "Cell Prefab is not assigned.",
//                this
//            );

//            valid = false;
//        }

//        if (letterBankParent == null)
//        {
//            Debug.LogError(
//                "Letter Bank Parent is not assigned.",
//                this
//            );

//            valid = false;
//        }

//        if (letterButtonPrefab == null)
//        {
//            Debug.LogError(
//                "Letter Button Prefab is not assigned.",
//                this
//            );

//            valid = false;
//        }

//        return valid;
//    }

//    public void CreateLevel()
//    {
//        ClearParent(gridParent);
//        ClearParent(letterBankParent);

//        blankCells.Clear();
//        selectedCell = null;

//        CreateGrid();
//        CreateLetterBank();
//    }

//    private void CreateGrid()
//    {
//        int rows = solutionGrid.Length;
//        int columns = solutionGrid[0].Length;

//        gridLayout.constraint =
//            GridLayoutGroup.Constraint.FixedColumnCount;

//        gridLayout.constraintCount = columns;

//        for (int row = 0; row < rows; row++)
//        {
//            for (int column = 0; column < columns; column++)
//            {
//                Vector2Int position =
//                    new Vector2Int(column, row);

//                SimpleCrosswordCell newCell =
//                    Instantiate(cellPrefab, gridParent);

//                newCell.gameObject.name =
//                    $"Cell_{column}_{row}";

//                /*
//                    Check picture clue cell.
//                */

//                if (cluePositions.TryGetValue(
//                    position,
//                    out ClueType clueType))
//                {
//                    Sprite clueSprite =
//                        GetClueSprite(clueType);

//                    newCell.InitializeClue(
//                        position,
//                        clueSprite
//                    );

//                    continue;
//                }

//                char character =
//                    solutionGrid[row][column];

//                /*
//                    Dot means inactive position.
//                */

//                if (character == '.')
//                {
//                    newCell.InitializeBlocked(position);
//                    continue;
//                }

//                /*
//                    Blank/missing letter.
//                */

//                if (blankPositions.Contains(position))
//                {
//                    newCell.InitializeBlank(
//                        position,
//                        character,
//                        SelectCell
//                    );

//                    blankCells.Add(newCell);
//                }
//                else
//                {
//                    /*
//                        Visible letter.
//                    */

//                    newCell.InitializeFixed(
//                        position,
//                        character
//                    );
//                }
//            }
//        }
//    }

//    private Sprite GetClueSprite(ClueType clueType)
//    {
//        switch (clueType)
//        {
//            case ClueType.Cat:
//                return catSprite;

//            case ClueType.Tree:
//                return treeSprite;

//            case ClueType.Red:
//                return redSprite;

//            default:
//                return null;
//        }
//    }

//    /*
//        Creates 5 or 6 unique letter options.

//        All correct letters are added first.
//        Remaining slots receive random wrong letters.
//    */

//    private void CreateLetterBank()
//    {
//        List<char> letterOptions =
//            GenerateLetterOptions();

//        ShuffleLetters(letterOptions);

//        foreach (char letter in letterOptions)
//        {
//            char currentLetter = letter;

//            Button newButton =
//                Instantiate(
//                    letterButtonPrefab,
//                    letterBankParent
//                );

//            newButton.gameObject.name =
//                $"LetterOption_{currentLetter}";

//            TMP_Text buttonText =
//                newButton.GetComponentInChildren<TMP_Text>(true);

//            if (buttonText != null)
//            {
//                buttonText.text =
//                    currentLetter.ToString();
//            }

//            newButton.onClick.RemoveAllListeners();

//            newButton.onClick.AddListener(() =>
//            {
//                TryPlaceLetter(currentLetter);
//            });
//        }
//    }

//    /*
//        Correct letters are always included.

//        Example:
//        Correct letters = A and E
//        Options = A, E, B, O, S, U
//    */

//    private List<char> GenerateLetterOptions()
//    {
//        List<char> options =
//            new List<char>();

//        /*
//            Add unique correct letters.
//            There are two E blanks, but only one E
//            button is needed because options are reusable.
//        */

//        foreach (Vector2Int position in blankPositions)
//        {
//            char correctLetter =
//                char.ToUpper(
//                    solutionGrid[position.y][position.x]
//                );

//            if (!options.Contains(correctLetter))
//            {
//                options.Add(correctLetter);
//            }
//        }

//        /*
//            Ensure total option count is not smaller
//            than the number of required unique letters.
//        */

//        int requiredOptionCount =
//            Mathf.Max(totalLetterOptions, options.Count);

//        /*
//            Add random distractor letters.
//        */

//        List<char> distractorPool =
//            new List<char>();

//        foreach (char character in availableAlphabet)
//        {
//            char upperCharacter =
//                char.ToUpper(character);

//            if (!char.IsLetter(upperCharacter))
//            {
//                continue;
//            }

//            if (options.Contains(upperCharacter))
//            {
//                continue;
//            }

//            if (!distractorPool.Contains(upperCharacter))
//            {
//                distractorPool.Add(upperCharacter);
//            }
//        }

//        ShuffleLetters(distractorPool);

//        int poolIndex = 0;

//        while (
//            options.Count < requiredOptionCount &&
//            poolIndex < distractorPool.Count)
//        {
//            options.Add(distractorPool[poolIndex]);
//            poolIndex++;
//        }

//        return options;
//    }

//    private void SelectCell(
//        SimpleCrosswordCell cell)
//    {
//        if (cell == null)
//        {
//            return;
//        }

//        /*
//            Remove previous selection.
//        */

//        if (selectedCell != null)
//        {
//            selectedCell.SetSelected(false);
//        }

//        selectedCell = cell;
//        selectedCell.SetSelected(true);
//    }

//    private void TryPlaceLetter(char letter)
//    {
//        /*
//            Child must first select an empty cell.
//        */

//        if (selectedCell == null)
//        {
//            Debug.Log(
//                "First select an empty cell."
//            );

//            return;
//        }

//        bool isCorrect =
//            selectedCell.TryFillLetter(letter);

//        /*
//            Wrong option does not disappear.
//            Selected cell also remains selected.
//        */

//        if (!isCorrect)
//        {
//            Debug.Log(
//                $"Wrong letter selected: {letter}"
//            );

//            return;
//        }

//        /*
//            Correct letter placed.

//            Alphabet button remains available,
//            because the same letter can be used
//            for another blank.
//        */

//        selectedCell = null;

//        CheckLevelComplete();
//    }

//    private void CheckLevelComplete()
//    {
//        foreach (
//            SimpleCrosswordCell cell in blankCells)
//        {
//            if (!cell.IsCompleted)
//            {
//                return;
//            }
//        }

//        Debug.Log("Level Complete!");
//    }

//    private void ShuffleLetters(List<char> letters)
//    {
//        for (int i = 0; i < letters.Count; i++)
//        {
//            int randomIndex =
//                Random.Range(i, letters.Count);

//            char temporaryLetter =
//                letters[i];

//            letters[i] =
//                letters[randomIndex];

//            letters[randomIndex] =
//                temporaryLetter;
//        }
//    }

//    private void ClearParent(Transform parent)
//    {
//        if (parent == null)
//        {
//            return;
//        }

//        for (
//            int i = parent.childCount - 1;
//            i >= 0;
//            i--)
//        {
//            Destroy(
//                parent.GetChild(i).gameObject
//            );
//        }
//    }
//}
















