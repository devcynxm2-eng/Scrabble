using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ScrabbleMini
{
    public class WordWheelController : MonoBehaviour
    {
        [Header("Level Letters")]
        [SerializeField] private string letters = "GAME";

        [Header("Wheel")]
        [SerializeField] private RectTransform wheelPanel;
        [SerializeField] private LetterNode letterButtonPrefab;
        [SerializeField] private float wheelRadius = 220f;

        [Header("UI")]
        [SerializeField] private TMP_Text currentWordText;

        [Header("Crossword Grid")]
        [SerializeField] private CrosswordGridController crosswordGrid;

        [Header("Selection Trail")]
        [SerializeField] private WordWheelTrail wordWheelTrail;

        [Header("Submission Timing")]
        [Min(0f)]
        [SerializeField] private float correctWordHoldDuration = 0.6f;

        [Min(0f)]
        [SerializeField] private float wrongWordHoldDuration = 0.3f;

        [SerializeField] private bool blockInputDuringSubmission = true;

        private readonly List<LetterNode> spawnedNodes = new();
        private readonly List<LetterNode> selectedNodes = new();

        private bool isSelecting;
        private bool isSubmitting;

        public bool IsSelecting => isSelecting;
        public bool IsSubmitting => isSubmitting;

        private void Start()
        {
            GenerateWheel(letters);
        }

        private void Update()
        {
            if (blockInputDuringSubmission && isSubmitting)
                return;

            if (!isSelecting)
                return;

            UpdateLiveTrail();

            if (Pointer.current == null)
                return;

            if (Pointer.current.press.wasReleasedThisFrame)
            {
                EndSelection();
            }
        }

        #region Wheel Generation

        public void GenerateWheel(string newLetters)
        {
            StopAllCoroutines();

            isSubmitting = false;
            isSelecting = false;

            ClearWheel();

            if (string.IsNullOrWhiteSpace(newLetters))
            {
                Debug.LogError(
                    "Wheel letters are empty.",
                    gameObject
                );

                return;
            }

            letters = NormalizeLetters(newLetters);

            if (letters.Length == 0)
            {
                Debug.LogError(
                    "No valid alphabetic letters were found.",
                    gameObject
                );

                return;
            }

            if (wheelPanel == null)
            {
                Debug.LogError(
                    "Wheel Panel reference is missing.",
                    gameObject
                );

                return;
            }

            if (letterButtonPrefab == null)
            {
                Debug.LogError(
                    "Letter Button Prefab reference is missing.",
                    gameObject
                );

                return;
            }

            int letterCount = letters.Length;

            for (int index = 0;
                 index < letterCount;
                 index++)
            {
                LetterNode node = Instantiate(
                    letterButtonPrefab,
                    wheelPanel
                );

                node.gameObject.SetActive(true);

                node.name =
                    $"Letter_{letters[index]}_{index}";

                node.Initialize(
                    this,
                    letters[index],
                    index
                );

                PositionNodeInCircle(
                    node.RectTransform,
                    index,
                    letterCount
                );

                spawnedNodes.Add(node);
            }

            ClearCurrentSelection();
        }

        private void PositionNodeInCircle(
            RectTransform node,
            int index,
            int totalNodes)
        {
            if (node == null || totalNodes <= 0)
                return;

            float angleStep = 360f / totalNodes;

            float angle =
                90f - angleStep * index;

            float angleRadians =
                angle * Mathf.Deg2Rad;

            Vector2 position = new Vector2(
                Mathf.Cos(angleRadians),
                Mathf.Sin(angleRadians)
            ) * wheelRadius;

            node.anchorMin = new Vector2(0.5f, 0.5f);
            node.anchorMax = new Vector2(0.5f, 0.5f);
            node.pivot = new Vector2(0.5f, 0.5f);

            node.anchoredPosition = position;
            node.localRotation = Quaternion.identity;
            node.localScale = Vector3.one;
        }

        #endregion

        #region Selection

        public void BeginSelection(LetterNode firstNode)
        {
            if (firstNode == null)
                return;

            if (blockInputDuringSubmission && isSubmitting)
                return;

            ClearCurrentSelection();

            isSelecting = true;

            AddNode(firstNode);
        }

        public void TrySelectLetter(LetterNode node)
        {
            if (!isSelecting || node == null)
                return;

            if (blockInputDuringSubmission && isSubmitting)
                return;

            if (selectedNodes.Count == 0)
            {
                AddNode(node);
                return;
            }

            LetterNode lastNode =
                selectedNodes[selectedNodes.Count - 1];

            // Same last letter ko ignore karein.
            if (node == lastNode)
                return;

            /*
             * Backtracking:
             *
             * G → A → M selected hai.
             * Player dobara A par aaye,
             * to M remove ho jayega.
             */
            if (selectedNodes.Count >= 2)
            {
                LetterNode previousNode =
                    selectedNodes[selectedNodes.Count - 2];

                if (node == previousNode)
                {
                    RemoveLastNode();
                    return;
                }
            }

            // Already-selected letter ko dobara use na karein.
            if (node.IsSelected)
                return;

            AddNode(node);
        }

        private void AddNode(LetterNode node)
        {
            if (node == null)
                return;

            selectedNodes.Add(node);

            node.SetSelected(true);

            RefreshCurrentWord();
            RefreshTrail();
        }

        private void RemoveLastNode()
        {
            if (selectedNodes.Count == 0)
                return;

            int lastIndex =
                selectedNodes.Count - 1;

            LetterNode lastNode =
                selectedNodes[lastIndex];

            selectedNodes.RemoveAt(lastIndex);

            if (lastNode != null)
            {
                lastNode.SetSelected(false);
            }

            RefreshCurrentWord();
            RefreshTrail();
        }

        private void EndSelection()
        {
            if (!isSelecting)
                return;

            if (isSubmitting)
                return;

            isSelecting = false;

            if (wordWheelTrail != null)
            {
                wordWheelTrail.HidePointerTrail();
            }

            string submittedWord =
                NormalizeWord(GetCurrentWord());

            if (string.IsNullOrEmpty(submittedWord))
            {
                ClearCurrentSelection();
                return;
            }

            StartCoroutine(
                SubmitWordRoutine(submittedWord)
            );
        }

        #endregion

        #region Trail

        private void UpdateLiveTrail()
        {
            if (wordWheelTrail == null)
                return;

            if (Pointer.current == null ||
                selectedNodes.Count == 0)
            {
                wordWheelTrail.HidePointerTrail();
                return;
            }

            LetterNode lastSelectedNode =
                selectedNodes[selectedNodes.Count - 1];

            Vector2 pointerScreenPosition =
                Pointer.current.position.ReadValue();

            wordWheelTrail.UpdatePointerTrail(
                lastSelectedNode,
                pointerScreenPosition
            );
        }

        private void RefreshTrail()
        {
            if (wordWheelTrail == null)
                return;

            wordWheelTrail.RefreshFixedTrail(
                selectedNodes
            );
        }

        #endregion

        #region Submission

        private IEnumerator SubmitWordRoutine(
            string submittedWord)
        {
            isSubmitting = true;

            Debug.Log(
                $"Submitted word: {submittedWord}",
                gameObject
            );

            if (currentWordText != null)
            {
                currentWordText.text = submittedWord;
            }

            if (crosswordGrid == null)
            {
                Debug.LogError(
                    "Crossword Grid reference is missing.",
                    gameObject
                );

                yield return new WaitForSeconds(
                    wrongWordHoldDuration
                );

                ClearCurrentSelection();

                isSubmitting = false;

                yield break;
            }

            bool alreadySolved =
                crosswordGrid.IsWordSolved(
                    submittedWord
                );

            if (alreadySolved)
            {
                Debug.Log(
                    $"Word already solved: {submittedWord}",
                    gameObject
                );

                yield return new WaitForSeconds(
                    wrongWordHoldDuration
                );

                ClearCurrentSelection();

                isSubmitting = false;

                yield break;
            }

            bool isTargetWord =
                crosswordGrid.IsTargetWord(
                    submittedWord
                );

            if (!isTargetWord)
            {
                Debug.Log(
                    $"Not a target word: {submittedWord}",
                    gameObject
                );

                yield return new WaitForSeconds(
                    wrongWordHoldDuration
                );

                ClearCurrentSelection();

                isSubmitting = false;

                yield break;
            }

            /*
             * Correct word ke case mein selected letters,
             * current word aur trail kuch der visible rahengi.
             */
            yield return new WaitForSeconds(
                correctWordHoldDuration
            );

            bool revealed =
                crosswordGrid.TryRevealWord(
                    submittedWord
                );

            if (revealed)
            {
                Debug.Log(
                    $"Correct word revealed: {submittedWord}",
                    gameObject
                );
            }

            ClearCurrentSelection();

            isSubmitting = false;
        }

        #endregion

        #region Current Word

        private void RefreshCurrentWord()
        {
            if (currentWordText == null)
                return;

            currentWordText.text =
                GetCurrentWord();
        }

        public string GetCurrentWord()
        {
            StringBuilder builder = new();

            foreach (LetterNode node in selectedNodes)
            {
                if (node != null)
                {
                    builder.Append(node.Letter);
                }
            }

            return builder.ToString();
        }

        private void ClearCurrentSelection()
        {
            foreach (LetterNode node in selectedNodes)
            {
                if (node != null)
                {
                    node.SetSelected(false);
                }
            }

            selectedNodes.Clear();

            isSelecting = false;

            if (currentWordText != null)
            {
                currentWordText.text =
                    string.Empty;
            }

            if (wordWheelTrail != null)
            {
                wordWheelTrail.ClearTrail();
            }
        }

        #endregion

        #region Clear Wheel

        private void ClearWheel()
        {
            ClearCurrentSelection();

            foreach (LetterNode node in spawnedNodes)
            {
                if (node != null)
                {
                    Destroy(node.gameObject);
                }
            }

            spawnedNodes.Clear();
        }

        #endregion

        #region Utility

        private static string NormalizeLetters(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            StringBuilder builder = new();

            foreach (char character in value)
            {
                if (char.IsLetter(character))
                {
                    builder.Append(
                        char.ToUpperInvariant(character)
                    );
                }
            }

            return builder.ToString();
        }

        private static string NormalizeWord(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            StringBuilder builder = new();

            foreach (char character in value)
            {
                if (char.IsLetter(character))
                {
                    builder.Append(
                        char.ToUpperInvariant(character)
                    );
                }
            }

            return builder.ToString();
        }

        public void SetLetters(string newLetters)
        {
            GenerateWheel(newLetters);
        }

        #endregion
    }
}