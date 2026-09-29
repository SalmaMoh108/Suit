using UnityEngine;

[CreateAssetMenu(fileName = "New Candidate", menuName = "Candidate")]  //create asset from unity menu
public class Candidate : ScriptableObject
{
    [Header("Visuals")]
    public Sprite character;

    [Header("Logic")]
    [Tooltip("Enter 1 for AIM, 2 for Means-to-end, 3 for Alignment")]  //hint box
    public int correctQuestion;
    public bool shouldHire;

    [Header("Answers")]
    [TextArea(2, 4)] public string answer1;
    [TextArea(2, 4)] public string answer2;
    [TextArea(2, 4)] public string answer3;

    [Header("Feedback Text")]
    [TextArea(2, 4)] public string correctFeedback;
    [TextArea(2, 4)] public string wrongFeedback;

    
}
