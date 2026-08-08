public enum WordDirection
{
    Horizontal,
    Vertical
}

[System.Serializable]
public class WordPlacementData
{
    public string word;
    public int startRow;
    public int startColumn;
    public WordDirection direction;
}