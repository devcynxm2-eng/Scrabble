using System.Collections;
using TMPro;
using UnityEngine;

namespace ScrabbleMini
{
    public class ScrabbleLevelManager : MonoBehaviour
    {
        private const string SavedLevelKey =
            "ScrabbleMini_CurrentLevel";

        [Header("References")]
        [SerializeField]
        private ScrabbleLevelDatabase levelDatabase;

        [SerializeField]
        private WordWheelController wordWheelController;

        [SerializeField]
        private CrosswordGridController crosswordGridController;

        [Header("UI")]
        [SerializeField]
        private TMP_Text levelText;

        [Header("Progression")]
        [SerializeField]
        private bool loadSavedLevel = true;

        [SerializeField]
        private bool loopAfterLastLevel = true;

        [Min(0f)]
        [SerializeField]
        private float nextLevelDelay = 1f;

        private int currentLevelIndex;
        private bool isChangingLevel;

        public int CurrentLevelIndex =>
            currentLevelIndex;

        private void OnEnable()
        {
            if (crosswordGridController != null)
            {
                crosswordGridController
                    .OnLevelCompleted +=
                    HandleLevelComplete;
            }
        }

        private void OnDisable()
        {
            if (crosswordGridController != null)
            {
                crosswordGridController
                    .OnLevelCompleted -=
                    HandleLevelComplete;
            }
        }

        private void Start()
        {
            if (!ValidateReferences())
                return;

            if (loadSavedLevel)
            {
                currentLevelIndex =
                    PlayerPrefs.GetInt(
                        SavedLevelKey,
                        0
                    );
            }
            else
            {
                currentLevelIndex = 0;
            }

            currentLevelIndex =
                Mathf.Clamp(
                    currentLevelIndex,
                    0,
                    Mathf.Max(
                        0,
                        levelDatabase.LevelCount - 1
                    )
                );

            LoadCurrentLevel();
        }

        public void LoadCurrentLevel()
        {
            if (isChangingLevel)
                return;

            ScrabbleLevelData level =
                levelDatabase.GetLevelByIndex(
                    currentLevelIndex
                );

            if (level == null)
                return;

            wordWheelController.SetLetters(
                level.letters
            );

            crosswordGridController.LoadLevel(
                level
            );

            if (levelText != null)
            {
                levelText.text =
                    $"LEVEL {level.levelId}";
            }

            Debug.Log(
                $"Playing Level {level.levelId}",
                gameObject
            );
        }

        private void HandleLevelComplete()
        {
            if (!isChangingLevel)
            {
                StartCoroutine(
                    NextLevelRoutine()
                );
            }
        }

        private IEnumerator NextLevelRoutine()
        {
            isChangingLevel = true;

            if (nextLevelDelay > 0f)
            {
                yield return
                    new WaitForSeconds(
                        nextLevelDelay
                    );
            }

            currentLevelIndex++;

            if (currentLevelIndex >=
                levelDatabase.LevelCount)
            {
                if (loopAfterLastLevel)
                {
                    currentLevelIndex = 0;
                }
                else
                {
                    currentLevelIndex =
                        levelDatabase.LevelCount - 1;

                    Debug.Log(
                        "All levels completed.",
                        gameObject
                    );

                    isChangingLevel = false;

                    yield break;
                }
            }

            PlayerPrefs.SetInt(
                SavedLevelKey,
                currentLevelIndex
            );

            PlayerPrefs.Save();

            isChangingLevel = false;

            LoadCurrentLevel();
        }

        public void RestartCurrentLevel()
        {
            if (isChangingLevel)
                return;

            LoadCurrentLevel();
        }

        public void LoadNextLevel()
        {
            if (isChangingLevel)
                return;

            HandleLevelComplete();
        }

        public void ResetProgress()
        {
            StopAllCoroutines();

            isChangingLevel = false;
            currentLevelIndex = 0;

            PlayerPrefs.DeleteKey(
                SavedLevelKey
            );

            PlayerPrefs.Save();

            LoadCurrentLevel();
        }

        private bool ValidateReferences()
        {
            if (levelDatabase == null)
            {
                Debug.LogError(
                    "Level Database reference is missing.",
                    gameObject
                );

                return false;
            }

            if (wordWheelController == null)
            {
                Debug.LogError(
                    "Word Wheel Controller is missing.",
                    gameObject
                );

                return false;
            }

            if (crosswordGridController == null)
            {
                Debug.LogError(
                    "Crossword Grid Controller is missing.",
                    gameObject
                );

                return false;
            }

            if (levelDatabase.LevelCount == 0)
            {
                Debug.LogError(
                    "Level database has no levels.",
                    gameObject
                );

                return false;
            }

            return true;
        }
    }
}