using System;
using UnityEngine;

[Serializable]
public class EgyptQuestion
{
    [TextArea(2, 4)]
    public string questionText;

    public string[] answers = new string[4];

    [Range(0, 3)]
    public int correctAnswerIndex;
}