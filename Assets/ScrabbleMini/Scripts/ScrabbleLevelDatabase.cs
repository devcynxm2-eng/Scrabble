using System.Collections.Generic;
using UnityEngine;

namespace ScrabbleMini
{
    public class ScrabbleLevelDatabase : MonoBehaviour
    {
        [Header("Level JSON")]
        [SerializeField] private TextAsset levelsJson;

        private ScrabbleLevelCollection levelCollection;

        public int LevelCount
        {
            get
            {
                if (levelCollection == null ||
                    levelCollection.levels == null)
                {
                    return 0;
                }

                return levelCollection.levels.Count;
            }
        }

        private void Awake()
        {
            LoadDatabase();
        }

        public void LoadDatabase()
        {
            if (levelsJson == null)
            {
                Debug.LogError(
                    "Levels JSON file is not assigned.",
                    gameObject
                );

                levelCollection =
                    new ScrabbleLevelCollection();

                return;
            }

            try
            {
                levelCollection =
                    JsonUtility.FromJson<ScrabbleLevelCollection>(
                        levelsJson.text
                    );
            }
            catch (System.Exception exception)
            {
                Debug.LogError(
                    $"Could not parse levels JSON: " +
                    $"{exception.Message}",
                    gameObject
                );

                levelCollection =
                    new ScrabbleLevelCollection();

                return;
            }

            if (levelCollection == null)
            {
                levelCollection =
                    new ScrabbleLevelCollection();
            }

            if (levelCollection.levels == null)
            {
                levelCollection.levels =
                    new List<ScrabbleLevelData>();
            }

            Debug.Log(
                $"Loaded {levelCollection.levels.Count} levels.",
                gameObject
            );
        }

        public ScrabbleLevelData GetLevelByIndex(
            int levelIndex)
        {
            if (levelCollection == null ||
                levelCollection.levels == null ||
                levelCollection.levels.Count == 0)
            {
                Debug.LogError(
                    "Level database is empty.",
                    gameObject
                );

                return null;
            }

            if (levelIndex < 0 ||
                levelIndex >= levelCollection.levels.Count)
            {
                Debug.LogError(
                    $"Level index {levelIndex} is outside " +
                    $"the available level range.",
                    gameObject
                );

                return null;
            }

            return levelCollection.levels[levelIndex];
        }

        public ScrabbleLevelData GetLevelById(
            int levelId)
        {
            if (levelCollection == null ||
                levelCollection.levels == null)
            {
                return null;
            }

            foreach (ScrabbleLevelData level
                     in levelCollection.levels)
            {
                if (level != null &&
                    level.levelId == levelId)
                {
                    return level;
                }
            }

            Debug.LogWarning(
                $"Level ID {levelId} was not found.",
                gameObject
            );

            return null;
        }
    }
}