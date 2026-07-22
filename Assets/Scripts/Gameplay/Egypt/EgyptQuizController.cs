using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EgyptQuizController : MonoBehaviour
{
    [Header("Texts")]
    public TMP_Text speechText;
    public TMP_Text questionText;
    public TMP_Text livesText;
    public TMP_Text progressText;
    public TMP_Text feedbackText;

    [Header("Buttons")]
    public Button nextButton;
    public Button[] answerButtons;

    [Header("Panels")]
    public GameObject speechBubblePanel;
    public GameObject questionPanel;
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Door")]
    public PyramidDoorController pyramidDoor;

    [Header("Character Reaction")]
    public AnubisReactionController anubisReaction;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip doorSound;
    public AudioClip winSound;
    public AudioClip loseSound;

    [Header("Game Settings")]
    public int maxLives = 3;
    public int requiredCorrectAnswers = 7;
    public float nextQuestionDelay = 1.2f;

    [Header("Button Colors")]
    public Color normalButtonColor = Color.white;
    public Color correctButtonColor = new Color(0.3f, 0.85f, 0.35f);
    public Color wrongButtonColor = new Color(0.9f, 0.25f, 0.25f);

    [Header("Questions")]
    public List<EgyptQuestion> questionPool = new List<EgyptQuestion>();

    private List<EgyptQuestion> selectedQuestions = new List<EgyptQuestion>();

    private int currentQuestionIndex;
    private int lives;
    private int correctAnswers;

    private bool gameFinished;
    private bool waitingForNextQuestion;

    private Coroutine nextStepCoroutine;

    private void Start()
    {
        SetupButtons();
        StartIntro();
    }

    private void SetupButtons()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(StartQuiz);
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int answerIndex = i;

            if (answerButtons[i] != null)
            {
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(answerIndex));
            }
        }
    }

    private void StartIntro()
    {
        StopRunningCoroutine();

        lives = maxLives;
        correctAnswers = 0;
        currentQuestionIndex = 0;

        gameFinished = false;
        waitingForNextQuestion = false;

        selectedQuestions.Clear();

        if (pyramidDoor != null)
        {
            pyramidDoor.CloseDoor();
        }

        if (anubisReaction != null)
        {
            anubisReaction.ResetAnubis();
        }

        if (speechBubblePanel != null) speechBubblePanel.SetActive(true);
        if (questionPanel != null) questionPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        ResetAnswerButtonColors();
        SetAnswerButtonsClickable(true);

        if (feedbackText != null)
        {
            feedbackText.text = "";
        }

        if (speechText != null)
        {
            speechText.text = "Willkommen bei den ägyptischen Pyramiden! " +
                              "Ich werde dir nun einige Fragen stellen. " +
                              "Wenn du richtig antwortest, öffnen sich die Tore der Geschichte!";
        }

        UpdateUI();
    }

    public void RestartGame()
    {
        PlaySound(clickSound);
        StopRunningCoroutine();

        lives = maxLives;
        correctAnswers = 0;
        currentQuestionIndex = 0;

        gameFinished = false;
        waitingForNextQuestion = false;

        selectedQuestions.Clear();

        if (pyramidDoor != null)
        {
            pyramidDoor.CloseDoor();
        }

        if (anubisReaction != null)
        {
            anubisReaction.ResetAnubis();
        }

        if (speechBubblePanel != null) speechBubblePanel.SetActive(true);
        if (questionPanel != null) questionPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        ResetAnswerButtonColors();
        SetAnswerButtonsClickable(true);

        if (feedbackText != null)
        {
            feedbackText.text = "";
        }

        if (speechText != null)
        {
            speechText.text = "Willkommen zurück bei den ägyptischen Pyramiden! " +
                              "Anubis wird dir neue Fragen stellen. Wenn du bereit bist, drücke auf Weiter!";
        }

        UpdateUI();

        Debug.Log("Egypt quiz restarted.");
    }

    private void StartQuiz()
    {
        PlaySound(clickSound);
        if (questionPool.Count < requiredCorrectAnswers)
        {
            Debug.LogError("Yeterli soru yok! En az " + requiredCorrectAnswers + " soru eklemelisin.");
            return;
        }

        StopRunningCoroutine();

        selectedQuestions = GetRandomQuestions(questionPool, questionPool.Count);

        currentQuestionIndex = 0;
        correctAnswers = 0;
        lives = maxLives;

        gameFinished = false;
        waitingForNextQuestion = false;

        if (speechBubblePanel != null) speechBubblePanel.SetActive(false);
        if (questionPanel != null) questionPanel.SetActive(true);
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        ResetAnswerButtonColors();
        SetAnswerButtonsClickable(true);
        ShowCurrentQuestion();
    }

    private void ShowCurrentQuestion()
    {
        if (gameFinished)
        {
            return;
        }

        waitingForNextQuestion = false;

        ResetAnswerButtonColors();
        SetAnswerButtonsClickable(true);

        if (currentQuestionIndex >= selectedQuestions.Count)
        {
            LoseGame();
            return;
        }

        EgyptQuestion currentQuestion = selectedQuestions[currentQuestionIndex];

        if (questionText != null)
        {
            questionText.text = currentQuestion.questionText;
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null)
            {
                continue;
            }

            TMP_Text buttonText = answerButtons[i].GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                if (currentQuestion.answers != null && i < currentQuestion.answers.Length)
                {
                    buttonText.text = currentQuestion.answers[i];
                }
                else
                {
                    buttonText.text = "";
                }
            }
        }

        if (feedbackText != null)
        {
            feedbackText.text = "";
        }

        UpdateUI();

        Debug.Log("Yeni soru gösteriliyor. Index: " + currentQuestionIndex);
    }

    private void OnAnswerSelected(int selectedIndex)
    {
        if (gameFinished || waitingForNextQuestion)
        {
            return;
        }

        if (currentQuestionIndex >= selectedQuestions.Count)
        {
            LoseGame();
            return;
        }

        EgyptQuestion currentQuestion = selectedQuestions[currentQuestionIndex];

        if (selectedIndex == currentQuestion.correctAnswerIndex)
        {
            CorrectAnswer(selectedIndex);
        }
        else
        {
            WrongAnswer(selectedIndex);
        }
    }

    private void CorrectAnswer(int selectedIndex)
    {
        waitingForNextQuestion = true;

        SetAnswerButtonColor(selectedIndex, correctButtonColor);
        SetAnswerButtonsClickable(false);

        PlaySound(correctSound);
        PlaySound(doorSound);

        if (anubisReaction != null)
        {
            anubisReaction.PlayCorrectReaction();
        }

        correctAnswers++;
        currentQuestionIndex++;

        float doorProgress = (float)correctAnswers / requiredCorrectAnswers;

        if (pyramidDoor != null)
        {
            pyramidDoor.SetDoorProgress(doorProgress);
        }

        if (feedbackText != null)
        {
            feedbackText.text = "Richtige Antwort! Das Tor hat sich ein Stück weiter geöffnet.";
        }

        UpdateUI();

        Debug.Log("Doğru cevap. Doğru sayısı: " + correctAnswers + " / " + requiredCorrectAnswers);

        if (correctAnswers >= requiredCorrectAnswers)
        {
            nextStepCoroutine = StartCoroutine(WinAfterDelay());
        }
        else
        {
            nextStepCoroutine = StartCoroutine(NextQuestionAfterDelay());
        }
    }

    private void WrongAnswer(int selectedIndex)
    {
        waitingForNextQuestion = true;

        EgyptQuestion currentQuestion = selectedQuestions[currentQuestionIndex];

        SetAnswerButtonColor(selectedIndex, wrongButtonColor);
        SetAnswerButtonColor(currentQuestion.correctAnswerIndex, correctButtonColor);
        SetAnswerButtonsClickable(false);

        PlaySound(wrongSound);
        if (anubisReaction != null)
        {
            anubisReaction.PlayWrongReaction();
        }
        lives--;
        currentQuestionIndex++;

        if (feedbackText != null)
        {
            feedbackText.text = "Falsche Antwort! Du hast ein Leben verloren.";
        }

        UpdateUI();

        Debug.Log("Falsche Antwort! Das verbleibende: " + lives);

        if (lives <= 0)
        {
            nextStepCoroutine = StartCoroutine(LoseAfterDelay());
        }
        else
        {
            nextStepCoroutine = StartCoroutine(NextQuestionAfterDelay());
        }
    }

    private IEnumerator NextQuestionAfterDelay()
    {
        yield return new WaitForSecondsRealtime(nextQuestionDelay);

        if (gameFinished)
        {
            yield break;
        }

        if (currentQuestionIndex >= selectedQuestions.Count)
        {
            LoseGame();
            yield break;
        }

        ShowCurrentQuestion();
    }

    private IEnumerator WinAfterDelay()
    {
        yield return new WaitForSecondsRealtime(nextQuestionDelay);
        WinGame();
    }

    private IEnumerator LoseAfterDelay()
    {
        yield return new WaitForSecondsRealtime(nextQuestionDelay);
        LoseGame();
    }

    private void WinGame()
    {
        gameFinished = true;
        waitingForNextQuestion = false;

        if (questionPanel != null) questionPanel.SetActive(false);
        if (speechBubblePanel != null) speechBubblePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(true);
        if (losePanel != null) losePanel.SetActive(false);

        if (pyramidDoor != null)
        {
            pyramidDoor.OpenCompletely();
        }
        PlaySound(winSound);
        if (anubisReaction != null)
        {
            anubisReaction.PlayWinReaction();
        }
        Debug.Log("Das Spiel wurde gewonnen.");
    }

    private void LoseGame()
    {
        gameFinished = true;
        waitingForNextQuestion = false;

        if (questionPanel != null) questionPanel.SetActive(false);
        if (speechBubblePanel != null) speechBubblePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(true);

        PlaySound(loseSound);
        if (anubisReaction != null)
        {
            anubisReaction.PlayLoseReaction();
        }
        Debug.Log("Oyun kaybedildi.");
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    private void UpdateUI()
    {
        if (livesText != null)
        {
            string hearts = "";

            for (int i = 0; i < maxLives; i++)
            {
                if (i < lives)
                {
                    hearts += "♥ ";
                }
                else
                {
                    hearts += "♡ ";
                }
            }

            livesText.text = hearts;
        }

        if (progressText != null)
        {
            progressText.text = "Richtig: " + correctAnswers + " / " + requiredCorrectAnswers;
        }
    }

    private void SetAnswerButtonsClickable(bool state)
    {
        foreach (Button button in answerButtons)
        {
            if (button != null)
            {
                // Button componentini kapatıyoruz ama görsel rengini bozmuyoruz.
                // Interactable = false yaparsak Unity butonu griye çekebilir.
                button.enabled = state;
                button.interactable = true;
            }
        }
    }

    private void ResetAnswerButtonColors()
    {
        foreach (Button button in answerButtons)
        {
            if (button == null) continue;

            Image buttonImage = button.GetComponent<Image>();

            if (buttonImage != null)
            {
                buttonImage.color = normalButtonColor;
            }
        }
    }

    private void SetAnswerButtonColor(int buttonIndex, Color color)
    {
        if (buttonIndex < 0 || buttonIndex >= answerButtons.Length)
        {
            return;
        }

        Button button = answerButtons[buttonIndex];

        if (button == null)
        {
            return;
        }

        Image buttonImage = button.GetComponent<Image>();

        if (buttonImage != null)
        {
            buttonImage.color = color;
        }
    }

    private void StopRunningCoroutine()
    {
        if (nextStepCoroutine != null)
        {
            StopCoroutine(nextStepCoroutine);
            nextStepCoroutine = null;
        }
    }

    private List<EgyptQuestion> GetRandomQuestions(List<EgyptQuestion> source, int amount)
    {
        List<EgyptQuestion> tempList = new List<EgyptQuestion>(source);
        List<EgyptQuestion> result = new List<EgyptQuestion>();

        int targetAmount = Mathf.Min(amount, tempList.Count);

        for (int i = 0; i < targetAmount; i++)
        {
            int randomIndex = Random.Range(0, tempList.Count);
            result.Add(tempList[randomIndex]);
            tempList.RemoveAt(randomIndex);
        }

        return result;
    }
}