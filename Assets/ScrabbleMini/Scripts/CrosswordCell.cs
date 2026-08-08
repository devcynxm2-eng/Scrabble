using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScrabbleMini
{
    public class CrosswordCell : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text letterText;

        [Header("Colors")]
        [SerializeField] private Color hiddenColor = Color.white;
        [SerializeField]
        private Color revealedColor =
            new Color(0.25f, 0.75f, 0.35f, 1f);

        private char correctLetter;
        private int row;
        private int column;
        private bool isActiveCell;
        private bool isRevealed;

        public char CorrectLetter => correctLetter;
        public int Row => row;
        public int Column => column;
        public bool IsActiveCell => isActiveCell;
        public bool IsRevealed => isRevealed;

        public void InitializeEmpty(int newRow, int newColumn)
        {
            row = newRow;
            column = newColumn;

            correctLetter = '\0';
            isActiveCell = false;
            isRevealed = false;

            gameObject.SetActive(true);

            if (backgroundImage != null)
            {
                backgroundImage.enabled = false;
            }

            if (letterText != null)
            {
                letterText.text = string.Empty;
                letterText.enabled = false;
            }
        }

        public void SetAsWordCell(char letter)
        {
            correctLetter = char.ToUpperInvariant(letter);
            isActiveCell = true;
            isRevealed = false;

            gameObject.SetActive(true);

            if (backgroundImage != null)
            {
                backgroundImage.enabled = true;
                backgroundImage.color = hiddenColor;
            }

            if (letterText != null)
            {
                letterText.enabled = true;
                letterText.text = string.Empty;
            }
        }

        public void Reveal()
        {
            if (!isActiveCell)
                return;

            isRevealed = true;

            if (backgroundImage != null)
            {
                backgroundImage.enabled = true;
                backgroundImage.color = revealedColor;
            }

            if (letterText != null)
            {
                letterText.enabled = true;
                letterText.text = correctLetter.ToString();
            }
        }

        public void HideLetter()
        {
            if (!isActiveCell)
                return;

            isRevealed = false;

            if (backgroundImage != null)
            {
                backgroundImage.enabled = true;
                backgroundImage.color = hiddenColor;
            }

            if (letterText != null)
            {
                letterText.enabled = true;
                letterText.text = string.Empty;
            }
        }
    }
}