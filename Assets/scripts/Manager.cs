using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class Manager : MonoBehaviour
{
    private Pause pauseAction;
    private bool isPaused = false;
    [Header("UI Elements")]
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject questionPanel;
    [SerializeField] GameObject dialoguePanel;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject pausePanel;
    
    [Header("Feedback UI")]
    [SerializeField] GameObject feedbackPanel;
    [SerializeField] Image stampImage;
    [SerializeField] TextMeshProUGUI feedbackText;
    [SerializeField]  Sprite stampCorrect; 
    [SerializeField]  Sprite stampWrong;   

    [Header("Card Animation")]
    [SerializeField] Image candImage;
    [SerializeField] RectTransform cardTransform;

    [Header("Game Logic")]
    [SerializeField] List<Candidate> allCandidates;
    private int currentIndex=0;
    private Candidate currentCandidate;
    private bool askedRight=false;

    [Header("Scoreboard")]
    [SerializeField] TextMeshProUGUI scoreText;
    private int currentScore = 0;
    private int totalEvaluated = 0; 

    [Header("End Game UI")]
    [SerializeField] GameObject endGamePanel;
    [SerializeField] TextMeshProUGUI finalScoreText;
    [SerializeField] TextMeshProUGUI gradeSentenceText;
    [SerializeField] Image gradeImage;

    [Header("Grade Custom Sentences")]
    [TextArea(2, 3)] [SerializeField]  string sentenceA = "Promoted to CEO! You actually know how to read people.";
    [TextArea(2, 3)] [SerializeField]  string sentenceB = "Decent job. You hired a few psychos, but survived.";
    [TextArea(2, 3)] [SerializeField]  string sentenceC = "Yikes. The workplace culture is officially toxic.";
    [TextArea(2, 3)] [SerializeField]  string sentenceF = "WTF is this grade?! Did you just close your eyes and click?";
    
    [Header("Grade Sprites")]
    [SerializeField]  Sprite gradeA;
    [SerializeField]  Sprite gradeB;
    [SerializeField]  Sprite gradeC;
    [SerializeField]  Sprite gradeF;

    [Header("Audio Sources")]
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioClip correctSound;
    [SerializeField] AudioClip wrongSound;
    [SerializeField] AudioClip smashSound;
    

    [Header("Typing Effect")]
    [SerializeField] float typingSpeed = 0.02f; // Lower is faster
    private Coroutine typingCoroutine;


    void Start()
    {
        mainMenuPanel.SetActive(true);
        endGamePanel.SetActive(false);
        questionPanel.SetActive(false);
        dialoguePanel.SetActive(false);
        feedbackPanel.SetActive(false);
        pausePanel.SetActive(false);
    }

    public void Backmain()
    {
        mainMenuPanel.SetActive(true);
        questionPanel.SetActive(false);
        dialoguePanel.SetActive(false);
        feedbackPanel.SetActive(false);
        endGamePanel.SetActive(false);
        pausePanel.SetActive(false);
        currentIndex = 0;
        currentScore = 0;
        totalEvaluated = 0;
    }
    void Awake()
    {
        pauseAction = new Pause();
    }
    void OnEnable()
    {
        pauseAction.Enable();
        pauseAction.PauseMenu.pause.performed += OnPause;
    }
    void OnDisable()
    {
        pauseAction.Disable();
        pauseAction.PauseMenu.pause.performed -= OnPause;
    }
    private void OnPause(InputAction.CallbackContext context)
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
        
    }

    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        LoadNextCandidate();
    }
    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f; 
    }
    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f; 
    }

    public void QuitGame()
    {
      Application.Quit(); 
    }   
    private void LoadNextCandidate()
    {
        scoreText.text = "HR Accuracy: " + currentScore + " / " + totalEvaluated;
        if (currentIndex < allCandidates.Count)
        {
            currentCandidate = allCandidates[currentIndex];
            candImage.sprite = currentCandidate.character;   //assign the character sprite to the UI image
            
            cardTransform.anchoredPosition =  Vector2.zero;  // Reset the card position to center
            questionPanel.SetActive(true);
            dialoguePanel.SetActive(false);
            feedbackPanel.SetActive(false);
            askedRight = false;
            endGamePanel.SetActive(false);
        }
        else
        {
            StartCoroutine(AnimateEndScreen());  //coroutine is used so that we can have a delay and animate the end screen
        }   //normal methods cant have delays
    }

    private IEnumerator AnimateEndScreen()
    {
        finalScoreText.text = "Final Accuracy: " + currentScore + " / " + allCandidates.Count;

        if (currentScore >= 8)
        {
            gradeImage.sprite = gradeA;
            gradeSentenceText.text = sentenceA;
        }
        else if (currentScore >= 5)
        {
            gradeImage.sprite = gradeB;
            gradeSentenceText.text = sentenceB;
        }
        else if (currentScore == 4)
        {
            gradeImage.sprite = gradeC;
            gradeSentenceText.text = sentenceC;
        }
        else
        {
            gradeImage.sprite = gradeF;
            gradeSentenceText.text = sentenceF;
        }

        //Hide the elements so we can smash them in one by one
        finalScoreText.gameObject.SetActive(false);
        gradeSentenceText.gameObject.SetActive(false);
        gradeImage.gameObject.SetActive(false);

        // Turn on the blank paper background
        endGamePanel.SetActive(true);
        yield return new WaitForSeconds(0.3f);  // Brief pause to let the panel appear

        //Smash in the final score
        finalScoreText.gameObject.SetActive(true);
        yield return StartCoroutine(SmashEffect(finalScoreText.rectTransform));
        
        //Smash in the Sentence
        gradeSentenceText.gameObject.SetActive(true);
        yield return StartCoroutine(SmashEffect(gradeSentenceText.rectTransform));

        //Letter grade at the end
        yield return new WaitForSeconds(0.6f); 
        
        gradeImage.gameObject.SetActive(true);
        yield return StartCoroutine(SmashEffect(gradeImage.rectTransform));
    }

    private IEnumerator SmashEffect(RectTransform target)
    {
        float duration = 0.15f; //how long the animation will take
        float elapsed = 0f;     //timer 
        if (sfxSource != null && smashSound != null)
        sfxSource.PlayOneShot(smashSound);
        
        //Remember the exact scale set in unity 
        Vector3 originalScale = target.localScale;
        
        //Start bigger than the original size
        Vector3 startScale = originalScale * 4f;
        target.localScale = startScale;
        
        while (elapsed < duration) //run till animation time is done
        {
            //shrink back to normal size over time
            target.localScale = Vector3.Lerp(startScale, originalScale, elapsed / duration); //smooth transition from start to end scale
            elapsed += Time.deltaTime;
            yield return null;    //wait for next frame
        }
        
        //Ensure it ends perfectly sized
        target.localScale = originalScale; 
    }

    public void questionAsked(int qNum)
    {
        askedRight = (qNum == currentCandidate.correctQuestion);
        dialoguePanel.SetActive(true);
        string answerToType = "";
        if (qNum == 1) answerToType = currentCandidate.answer1;
        else if (qNum == 2) answerToType = currentCandidate.answer2;
        else if (qNum == 3) answerToType = currentCandidate.answer3;
        if (typingCoroutine != null) // If a typing coroutine is already running, stop it
    {
        StopCoroutine(typingCoroutine);
    }

        // Start typing the new answer
        typingCoroutine = StartCoroutine(TypeText(answerToType));
        
    }

    private IEnumerator TypeText(string textToType)
    {
        dialogueText.text = textToType;
        dialogueText.maxVisibleCharacters = 0; // Start with no characters visible

        // Reveal them one by one visually
        int totalCharacters = textToType.Length;
        for (int i = 0; i <= totalCharacters; i++)
        {
           dialogueText.maxVisibleCharacters = i;
           yield return new WaitForSeconds(typingSpeed);
        }
    }

    private IEnumerator ShakeUI(RectTransform target, float duration = 0.35f, float magnitude = 18f)
    {
        Vector2 originalPos = target.anchoredPosition; // Store the original position of the UI element
        float elapsed = 0f;  // Timer to track how long the shake has been happening

        while (elapsed < duration)
        {
            // Generate a random jitter offset for both X and Y axes
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            target.anchoredPosition = originalPos + new Vector2(offsetX, offsetY);  // Apply the jitter to the original position

            elapsed += Time.deltaTime;
            yield return null;
        }

        //back to the starting position
        target.anchoredPosition = originalPos;
    }
    public void Decision(bool hired)
    {
        bool isCorrect=(hired == currentCandidate.shouldHire);  // Check if the player's decision matches the candidate's actual suitability
        totalEvaluated++;
        if (isCorrect) 
        {
            currentScore++;
            if (sfxSource != null && correctSound != null)
            sfxSource.PlayOneShot(correctSound);
    }
    else
    {
        if (sfxSource != null && wrongSound != null)
            sfxSource.PlayOneShot(wrongSound);
    }
        dialoguePanel.SetActive(false);
        StartCoroutine(SwipeCardOffScreen(isCorrect, hired));
    }

    private IEnumerator SwipeCardOffScreen(bool isCorrect, bool isHired)
    {
        float duration = 0.4f;
        float elapsed = 0f;
        Vector2 startPos = cardTransform.anchoredPosition;
        Vector2 targetPos = isHired ? new Vector2(2000f, startPos.y) : new Vector2(startPos.x, 2000f);  //move off screen to the right if hired, otherwise up

        while (elapsed < duration)
        {
            cardTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        ShowFeedback(isCorrect);
    }
    private void ShowFeedback(bool isFullyCorrect)
    {
        feedbackPanel.SetActive(true);
        stampImage.sprite = isFullyCorrect ? stampCorrect : stampWrong;
        feedbackText.text = isFullyCorrect ? currentCandidate.correctFeedback : currentCandidate.wrongFeedback;
        if (!isFullyCorrect)
        {
        RectTransform feedbackRect = feedbackPanel.GetComponent<RectTransform>();
        StartCoroutine(ShakeUI(feedbackRect));
        }
    }

    public void OnNextButtonClicked()
    {
        currentIndex++;
        LoadNextCandidate();
    }
}
