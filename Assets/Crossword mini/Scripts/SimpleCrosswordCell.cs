//using System;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class SimpleCrosswordCell : MonoBehaviour
//{
//    [Header("UI References")]
//    [SerializeField] private Image backgroundImage;
//    [SerializeField] private TMP_Text letterText;
//    [SerializeField] private Button cellButton;
//    [SerializeField] private Image clueImage;

//    [Header("Cell Colors")]
//    [SerializeField] private Color normalColor = Color.white;
//    [SerializeField]
//    private Color selectedColor =
//        new Color(1f, 0.85f, 0.35f, 1f);

//    [SerializeField]
//    private Color completedColor =
//        new Color(0.55f, 0.95f, 0.55f, 1f);

//    [SerializeField]
//    private Color clueCellColor =
//        new Color(1f, 0.95f, 0.75f, 1f);

//    private char correctLetter;

//    private bool isBlank;
//    private bool isCompleted;

//    private Action<SimpleCrosswordCell> onCellClicked;

//    public Vector2Int GridPosition { get; private set; }

//    public bool IsBlank => isBlank;
//    public bool IsCompleted => isCompleted;

//    /// <summary>
//    /// Cell ko default state par reset karta hai.
//    /// </summary>
//    private void ResetCell()
//    {
//        isBlank = false;
//        isCompleted = false;
//        correctLetter = '\0';
//        onCellClicked = null;

//        if (cellButton != null)
//        {
//            cellButton.onClick.RemoveAllListeners();
//            cellButton.interactable = false;
//        }

//        if (backgroundImage != null)
//        {
//            backgroundImage.enabled = true;
//            backgroundImage.color = normalColor;
//        }

//        if (letterText != null)
//        {
//            letterText.gameObject.SetActive(true);
//            letterText.text = string.Empty;
//        }

//        if (clueImage != null)
//        {
//            clueImage.sprite = null;
//            clueImage.gameObject.SetActive(false);
//        }
//    }

//    /// <summary>
//    /// Empty/inactive grid position.
//    /// </summary>
//    public void InitializeBlocked(Vector2Int position)
//    {
//        ResetCell();

//        GridPosition = position;

//        if (backgroundImage != null)
//        {
//            backgroundImage.enabled = false;
//        }

//        if (letterText != null)
//        {
//            letterText.gameObject.SetActive(false);
//        }

//        if (clueImage != null)
//        {
//            clueImage.gameObject.SetActive(false);
//        }

//        if (cellButton != null)
//        {
//            cellButton.interactable = false;
//        }
//    }

//    /// <summary>
//    /// Already visible/fixed letter.
//    /// </summary>
//    public void InitializeFixed(
//        Vector2Int position,
//        char letter)
//    {
//        ResetCell();

//        GridPosition = position;
//        correctLetter = char.ToUpper(letter);

//        if (letterText != null)
//        {
//            letterText.gameObject.SetActive(true);
//            letterText.text = correctLetter.ToString();
//        }

//        if (cellButton != null)
//        {
//            cellButton.interactable = false;
//        }
//    }

//    /// <summary>
//    /// Missing letter wali selectable cell.
//    /// </summary>
//    public void InitializeBlank(
//        Vector2Int position,
//        char answer,
//        Action<SimpleCrosswordCell> clickCallback)
//    {
//        ResetCell();

//        GridPosition = position;
//        correctLetter = char.ToUpper(answer);

//        isBlank = true;
//        isCompleted = false;

//        onCellClicked = clickCallback;

//        if (letterText != null)
//        {
//            letterText.gameObject.SetActive(true);
//            letterText.text = string.Empty;
//        }

//        if (cellButton != null)
//        {
//            cellButton.interactable = true;
//            cellButton.onClick.AddListener(HandleCellClicked);
//        }
//    }

//    /// <summary>
//    /// Cat, tree ya red ki image wali clue cell.
//    /// </summary>
//    public void InitializeClue(
//        Vector2Int position,
//        Sprite clueSprite)
//    {
//        ResetCell();

//        GridPosition = position;

//        if (backgroundImage != null)
//        {
//            backgroundImage.enabled = true;
//            backgroundImage.color = clueCellColor;
//        }

//        if (letterText != null)
//        {
//            letterText.gameObject.SetActive(false);
//        }

//        if (clueImage != null)
//        {
//            clueImage.gameObject.SetActive(true);
//            clueImage.sprite = clueSprite;
//            clueImage.preserveAspect = true;
//        }

//        if (cellButton != null)
//        {
//            cellButton.interactable = false;
//        }
//    }

//    private void HandleCellClicked()
//    {
//        if (!isBlank || isCompleted)
//        {
//            return;
//        }

//        onCellClicked?.Invoke(this);
//    }

//    /// <summary>
//    /// Selected cell ko yellow karta hai.
//    /// </summary>
//    public void SetSelected(bool selected)
//    {
//        if (!isBlank || isCompleted)
//        {
//            return;
//        }

//        if (backgroundImage != null)
//        {
//            backgroundImage.color =
//                selected ? selectedColor : normalColor;
//        }
//    }

//    /// <summary>
//    /// Letter correct ho to cell fill karta hai.
//    /// </summary>
//    public bool TryFillLetter(char enteredLetter)
//    {
//        if (!isBlank || isCompleted)
//        {
//            return false;
//        }

//        enteredLetter = char.ToUpper(enteredLetter);

//        if (enteredLetter != correctLetter)
//        {
//            return false;
//        }

//        if (letterText != null)
//        {
//            letterText.text = correctLetter.ToString();
//        }

//        if (backgroundImage != null)
//        {
//            backgroundImage.color = completedColor;
//        }

//        isCompleted = true;

//        if (cellButton != null)
//        {
//            cellButton.interactable = false;
//        }

//        return true;
//    }
//}






//using System;
//using TMPro;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//public class SimpleCrosswordCell :
//    MonoBehaviour,
//    IDropHandler
//{
//    [Header("UI References")]
//    [SerializeField] private Image backgroundImage;
//    [SerializeField] private TMP_Text letterText;
//    [SerializeField] private Button cellButton;
//    [SerializeField] private Image clueImage;

//    [Tooltip(
//        "Optional. Leave empty to settle the tile " +
//        "inside the cell root."
//    )]
//    [SerializeField] private RectTransform letterDropTarget;

//    [Header("Cell Colors")]
//    [SerializeField]
//    private Color normalColor =
//        Color.white;

//    [SerializeField]
//    private Color completedColor =
//        new Color(0.55f, 0.95f, 0.55f, 1f);

//    [SerializeField]
//    private Color clueCellColor =
//        new Color(1f, 0.95f, 0.75f, 1f);

//    private char correctLetter;
//    private bool isBlank;
//    private bool isCompleted;

//    private Action<SimpleCrosswordCell>
//        onCellCompleted;

//    public Vector2Int GridPosition
//    {
//        get;
//        private set;
//    }

//    public bool IsBlank => isBlank;
//    public bool IsCompleted => isCompleted;

//    private void Awake()
//    {
//        if (letterDropTarget == null)
//        {
//            letterDropTarget =
//                transform as RectTransform;
//        }
//    }

//    private void ResetCell()
//    {
//        correctLetter = '\0';
//        isBlank = false;
//        isCompleted = false;
//        onCellCompleted = null;

//        if (cellButton != null)
//        {
//            cellButton.onClick.RemoveAllListeners();
//            cellButton.interactable = false;
//        }

//        if (backgroundImage != null)
//        {
//            backgroundImage.enabled = true;
//            backgroundImage.color = normalColor;

//            // Required so the cell can receive drop events.
//            backgroundImage.raycastTarget = true;
//        }

//        if (letterText != null)
//        {
//            letterText.gameObject.SetActive(true);
//            letterText.text = string.Empty;
//            letterText.raycastTarget = false;
//        }

//        if (clueImage != null)
//        {
//            clueImage.sprite = null;
//            clueImage.preserveAspect = true;
//            clueImage.raycastTarget = false;
//            clueImage.gameObject.SetActive(false);
//        }
//    }

//    public void InitializeBlocked(
//        Vector2Int position)
//    {
//        ResetCell();

//        GridPosition = position;

//        if (backgroundImage != null)
//        {
//            backgroundImage.enabled = false;
//            backgroundImage.raycastTarget = false;
//        }

//        if (letterText != null)
//        {
//            letterText.gameObject.SetActive(false);
//        }
//    }

//    public void InitializeFixed(
//        Vector2Int position,
//        char letter)
//    {
//        ResetCell();

//        GridPosition = position;
//        correctLetter =
//            char.ToUpperInvariant(letter);

//        if (letterText != null)
//        {
//            letterText.text =
//                correctLetter.ToString();
//        }

//        // Fixed cells do not accept a drop.
//        if (backgroundImage != null)
//        {
//            backgroundImage.raycastTarget = false;
//        }
//    }

//    public void InitializeBlank(
//        Vector2Int position,
//        char answer,
//        Action<SimpleCrosswordCell>
//            completionCallback)
//    {
//        ResetCell();

//        GridPosition = position;
//        correctLetter =
//            char.ToUpperInvariant(answer);

//        isBlank = true;
//        isCompleted = false;

//        onCellCompleted =
//            completionCallback;

//        if (letterText != null)
//        {
//            letterText.text = string.Empty;
//        }

//        if (backgroundImage != null)
//        {
//            backgroundImage.raycastTarget = true;
//        }
//    }

//    public void InitializeClue(
//        Vector2Int position,
//        Sprite sprite)
//    {
//        ResetCell();

//        GridPosition = position;

//        if (backgroundImage != null)
//        {
//            backgroundImage.enabled = true;
//            backgroundImage.color =
//                clueCellColor;

//            backgroundImage.raycastTarget =
//                false;
//        }

//        if (letterText != null)
//        {
//            letterText.gameObject.SetActive(false);
//        }

//        if (clueImage != null)
//        {
//            clueImage.sprite = sprite;
//            clueImage.preserveAspect = true;
//            clueImage.gameObject.SetActive(true);
//        }
//    }

//    public void OnDrop(
//        PointerEventData eventData)
//    {
//        if (!isBlank || isCompleted)
//        {
//            return;
//        }

//        if (eventData.pointerDrag == null)
//        {
//            return;
//        }

//        DraggableLetter draggableLetter =
//            eventData.pointerDrag
//                .GetComponent<DraggableLetter>();

//        if (draggableLetter == null)
//        {
//            return;
//        }

//        char droppedLetter =
//            char.ToUpperInvariant(
//                draggableLetter.Letter
//            );

//        if (droppedLetter != correctLetter)
//        {
//            // Wrong letter goes back to its
//            // original LetterBank position.
//            draggableLetter.ReturnToHome();
//            return;
//        }

//        CompleteWithLetter(
//            draggableLetter
//        );
//    }

//    private void CompleteWithLetter(
//        DraggableLetter draggableLetter)
//    {
//        isCompleted = true;

//        if (backgroundImage != null)
//        {
//            backgroundImage.color =
//                completedColor;

//            backgroundImage.raycastTarget =
//                false;
//        }

//        if (letterText != null)
//        {
//            // The draggable tile itself now shows
//            // the letter inside the cell.
//            letterText.text = string.Empty;
//            letterText.gameObject.SetActive(false);
//        }

//        draggableLetter.SettleInto(
//            letterDropTarget
//        );

//        onCellCompleted?.Invoke(this);
//    }
//}







using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SimpleCrosswordCell :
    MonoBehaviour,
    IDropHandler
{
    [Header("UI References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text letterText;
    [SerializeField] private Button cellButton;
    [SerializeField] private Image clueImage;

    [Tooltip(
        "Optional. Leave empty to settle the draggable tile " +
        "inside the root of this cell."
    )]
    [SerializeField] private RectTransform letterDropTarget;

    [Header("Cell Colors")]
    [SerializeField]
    private Color normalColor =
        Color.white;

    [SerializeField]
    private Color completedColor =
        new Color(0.55f, 0.95f, 0.55f, 1f);

    private char correctLetter;

    private bool isBlank;
    private bool isCompleted;

    private Action<SimpleCrosswordCell>
        onCellCompleted;

    public Vector2Int GridPosition
    {
        get;
        private set;
    }

    public bool IsBlank => isBlank;
    public bool IsCompleted => isCompleted;

    private void Awake()
    {
        /*
         * Agar separate drop target assign nahi kiya,
         * to draggable letter cell ke root mein settle hoga.
         */

        if (letterDropTarget == null)
        {
            letterDropTarget =
                transform as RectTransform;
        }
    }

    /// <summary>
    /// Cell ko default state mein reset karta hai.
    /// </summary>
    private void ResetCell()
    {
        correctLetter = '\0';

        isBlank = false;
        isCompleted = false;

        onCellCompleted = null;

        if (cellButton != null)
        {
            cellButton.onClick.RemoveAllListeners();
            cellButton.interactable = false;
        }

        if (backgroundImage != null)
        {
            backgroundImage.enabled = true;
            backgroundImage.color = normalColor;

            /*
             * Blank cells ko drop event receive karne
             * ke liye raycast enabled chahiye.
             */

            backgroundImage.raycastTarget = true;
        }

        if (letterText != null)
        {
            letterText.gameObject.SetActive(true);
            letterText.text = string.Empty;
            letterText.raycastTarget = false;
        }

        if (clueImage != null)
        {
            clueImage.sprite = null;
            clueImage.preserveAspect = true;
            clueImage.raycastTarget = false;
            clueImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Grid ki inactive position.
    /// Is cell mein na background show hoga,
    /// na letter aur na drop accept hoga.
    /// </summary>
    public void InitializeBlocked(
        Vector2Int position)
    {
        ResetCell();

        GridPosition = position;

        if (backgroundImage != null)
        {
            backgroundImage.enabled = false;
            backgroundImage.raycastTarget = false;
        }

        if (letterText != null)
        {
            letterText.gameObject.SetActive(false);
        }

        if (clueImage != null)
        {
            clueImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Already visible fixed letter cell.
    /// </summary>
    public void InitializeFixed(
        Vector2Int position,
        char letter)
    {
        ResetCell();

        GridPosition = position;

        correctLetter =
            char.ToUpperInvariant(letter);

        if (letterText != null)
        {
            letterText.gameObject.SetActive(true);
            letterText.text =
                correctLetter.ToString();
        }

        /*
         * Fixed letter cells draggable tile
         * accept nahi karengi.
         */

        if (backgroundImage != null)
        {
            backgroundImage.raycastTarget = false;
        }
    }

    /// <summary>
    /// Missing letter wali blank drop cell.
    /// </summary>
    public void InitializeBlank(
        Vector2Int position,
        char answer,
        Action<SimpleCrosswordCell>
            completionCallback)
    {
        ResetCell();

        GridPosition = position;

        correctLetter =
            char.ToUpperInvariant(answer);

        isBlank = true;
        isCompleted = false;

        onCellCompleted =
            completionCallback;

        if (letterText != null)
        {
            letterText.gameObject.SetActive(true);
            letterText.text = string.Empty;
        }

        /*
         * Blank cell ko raycast receive karna hai,
         * taa-ke OnDrop call ho.
         */

        if (backgroundImage != null)
        {
            backgroundImage.enabled = true;
            backgroundImage.color = normalColor;
            backgroundImage.raycastTarget = true;
        }
    }

    /// <summary>
    /// Picture clue cell.
    /// Is cell mein sirf clue image nazar aayegi.
    /// Background tile completely hidden rahegi.
    /// </summary>
    public void InitializeClue(
        Vector2Int position,
        Sprite sprite)
    {
        ResetCell();

        GridPosition = position;

        /*
         * Clue image ke peeche tile/background
         * completely hide kar di gayi hai.
         */

        if (backgroundImage != null)
        {
            backgroundImage.enabled = false;
            backgroundImage.raycastTarget = false;
        }

        if (letterText != null)
        {
            letterText.text = string.Empty;
            letterText.gameObject.SetActive(false);
        }

        if (clueImage != null)
        {
            clueImage.sprite = sprite;
            clueImage.preserveAspect = true;
            clueImage.raycastTarget = false;
            clueImage.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Draggable letter blank cell par drop hone
    /// ke baad call hota hai.
    /// </summary>
    public void OnDrop(
        PointerEventData eventData)
    {
        /*
         * Sirf uncompleted blank cells
         * drop accept karengi.
         */

        if (!isBlank || isCompleted)
        {
            return;
        }

        if (eventData.pointerDrag == null)
        {
            return;
        }

        DraggableLetter draggableLetter =
            eventData.pointerDrag
                .GetComponent<DraggableLetter>();

        if (draggableLetter == null)
        {
            return;
        }

        char droppedLetter =
            char.ToUpperInvariant(
                draggableLetter.Letter
            );

        /*
         * Wrong letter:
         * Tile apni original LetterBank
         * position par wapas jayegi.
         */

        if (droppedLetter != correctLetter)
        {
            draggableLetter.ReturnToHome();
            return;
        }

        /*
         * Correct letter:
         * Tile is cell ke andar settle hogi.
         */

        CompleteWithLetter(
            draggableLetter
        );
    }

    /// <summary>
    /// Correct draggable letter ko blank cell
    /// ke andar settle aur lock karta hai.
    /// </summary>
    private void CompleteWithLetter(
        DraggableLetter draggableLetter)
    {
        isCompleted = true;

        if (backgroundImage != null)
        {
            backgroundImage.enabled = true;
            backgroundImage.color =
                completedColor;

            backgroundImage.raycastTarget =
                false;
        }

        if (letterText != null)
        {
            /*
             * Letter draggable tile par already
             * visible hai, isliye cell ka internal
             * text hide rahega.
             */

            letterText.text = string.Empty;
            letterText.gameObject.SetActive(false);
        }

        draggableLetter.SettleInto(
            letterDropTarget
        );

        onCellCompleted?.Invoke(this);
    }
}



