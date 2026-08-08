using System;

[Serializable]
public class CrosswordJsonDatabase
{
    public CrosswordJsonLevel[] levels;
}

[Serializable]
public class CrosswordJsonLevel
{
    public int levelNumber;
    public int rows;
    public int columns;
    public int totalLetterOptions;
    public string distractorAlphabet;
    public CrosswordJsonWord[] words;
}

[Serializable]
public class CrosswordJsonWord
{
    public string word;
    public string imageName;

    // Row and column counting starts from 1 in JSON.
    public int startRow;
    public int startColumn;

    // Use "Horizontal" or "Vertical" in JSON.
    public string direction;

    // Letter counting also starts from 1.
    // Example: CAT -> 1=C, 2=A, 3=T.
    public int[] blankLetterNumbers;
}
