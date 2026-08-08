using System;
using System.Collections.Generic;

namespace ScrabbleMini
{
    public enum WordDirection
    {
        Horizontal,
        Vertical
    }

    [Serializable]
    public class WordPlacementData
    {
        public string word;
        public int startRow;
        public int startColumn;
        public WordDirection direction;
    }

    [Serializable]
    public class ScrabbleLevelData
    {
        public int levelId;

        public string letters;

        public int rows = 6;
        public int columns = 6;

        public List<WordPlacementData> words =
            new List<WordPlacementData>();
    }

    [Serializable]
    public class ScrabbleLevelCollection
    {
        public List<ScrabbleLevelData> levels =
            new List<ScrabbleLevelData>();
    }
}