using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScrabbleMini
{
    public class LetterNode : MonoBehaviour,
        IPointerDownHandler,
        IPointerEnterHandler
    {
        [Header("References")]
        [SerializeField] private TMP_Text letterText;
        [SerializeField] private Image backgroundImage;

        [Header("Selection")]
        [SerializeField] private float selectedScale = 1.15f;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField]
        private Color selectedColor =
            new Color(1f, 0.75f, 0.15f, 1f);

        private WordWheelController wheelController;

        private char letter;
        private int nodeIndex;
        private bool isSelected;

        public char Letter => letter;
        public int NodeIndex => nodeIndex;
        public bool IsSelected => isSelected;

        public RectTransform RectTransform =>
            transform as RectTransform;

        public void Initialize(
            WordWheelController controller,
            char newLetter,
            int index)
        {
            wheelController = controller;
            letter = char.ToUpperInvariant(newLetter);
            nodeIndex = index;

            if (letterText != null)
                letterText.text = letter.ToString();

            SetSelected(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (wheelController == null)
                return;

            wheelController.BeginSelection(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (wheelController == null)
                return;

            if (!wheelController.IsSelecting)
                return;

            wheelController.TrySelectLetter(this);
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;

            transform.localScale = selected
                ? Vector3.one * selectedScale
                : Vector3.one;

            if (backgroundImage != null)
            {
                backgroundImage.color = selected
                    ? selectedColor
                    : normalColor;
            }
        }
    }
}