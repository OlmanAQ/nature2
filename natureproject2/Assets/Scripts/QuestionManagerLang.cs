using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class QuestionManagerLang : MonoBehaviour
{
    public Text questionTextUI;
    public Button[] optionButtons;
    public Text feedbackTextUI;

    private FirebaseFirestore db;
    private Dictionary<string, object> loadedQuestions;
    private string currentCorrectAnswer;

    private string languageCode;
    private int maxRetries = 10;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        string lastScene = GlobalSceneManager.instance.GetLastScene();
        if (string.IsNullOrEmpty(lastScene))
        {
            Debug.LogError("No se pudo obtener el nombre de la última escena.");
            return;
        }

        // Obtener el código del idioma actual del sistema de localización
        languageCode = LocalizationSettings.SelectedLocale.Identifier.Code;

        // Inicia la coroutine para cargar preguntas en el idioma actual
        StartCoroutine(TryLoadQuestionsFromFirestore(lastScene, languageCode, maxRetries));
    }

    IEnumerator TryLoadQuestionsFromFirestore(string sceneDocumentName, string languageCode, int retries)
    {
        while (retries > 0)
        {
            yield return StartCoroutine(LoadQuestionsFromFirestore(sceneDocumentName, languageCode));

            if (loadedQuestions != null && loadedQuestions.Count > 0)
            {
                ShowRandomQuestion();
                yield break;
            }

            retries--;
            Debug.LogWarning("Intento fallido, reintentando... Restantes: " + retries);
            yield return new WaitForSeconds(1);
        }

        Debug.LogError("No se pudieron cargar las preguntas después de varios intentos.");
    }

    IEnumerator LoadQuestionsFromFirestore(string sceneDocumentName, string languageCode)
    {
        Task<DocumentSnapshot> task = db.Collection("Questions").Document(sceneDocumentName).GetSnapshotAsync();

        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.IsFaulted)
        {
            Debug.LogError("Error al obtener el documento: " + task.Exception);
            yield break;
        }

        DocumentSnapshot snapshot = task.Result;
        if (snapshot.Exists)
        {
            loadedQuestions = snapshot.ToDictionary();
            Debug.Log("Preguntas cargadas correctamente para el idioma: " + languageCode);
        }
        else
        {
            Debug.LogError("No se encontró el documento: " + sceneDocumentName);
        }
    }

    void ShowRandomQuestion()
    {
        if (loadedQuestions == null || loadedQuestions.Count == 0)
        {
            Debug.LogError("No hay preguntas cargadas.");
            return;
        }

        int randomIndex = Random.Range(1, loadedQuestions.Count + 1);
        string randomQuestionKey = "Pregunta" + randomIndex;

        if (loadedQuestions.ContainsKey(randomQuestionKey))
        {
            if (loadedQuestions[randomQuestionKey] is Dictionary<string, object> questionData &&
                questionData.ContainsKey(languageCode) && questionData[languageCode] is Dictionary<string, object> languageData)
            {
                string questionText = languageData.ContainsKey("pregunta") ? languageData["pregunta"].ToString() : "";
                List<object> respuestasList = languageData.ContainsKey("respuestas") ? languageData["respuestas"] as List<object> : null;
                int correctAnswerIndex = languageData.ContainsKey("correcta") ? (int)(long)languageData["correcta"] : -1;

                if (string.IsNullOrEmpty(questionText) || respuestasList == null || correctAnswerIndex == -1)
                {
                    Debug.LogError("Los datos de la pregunta no están correctamente formateados.");
                    return;
                }

                currentCorrectAnswer = respuestasList[correctAnswerIndex].ToString();
                questionTextUI.text = questionText;

                for (int i = 0; i < optionButtons.Length; i++)
                {
                    if (i < respuestasList.Count)
                    {
                        string respuestaTexto = respuestasList[i].ToString();
                        optionButtons[i].GetComponentInChildren<Text>().text = respuestaTexto;
                        optionButtons[i].gameObject.SetActive(true);
                        optionButtons[i].onClick.RemoveAllListeners();
                        optionButtons[i].onClick.AddListener(() => CheckAnswer(respuestaTexto));
                    }
                    else
                    {
                        optionButtons[i].gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                Debug.LogError("Pregunta o datos de idioma no encontrados para: " + randomQuestionKey);
            }
        }
        else
        {
            Debug.LogError("Pregunta no encontrada: " + randomQuestionKey);
        }
    }

    void CheckAnswer(string selectedAnswer)
    {
        if (selectedAnswer == currentCorrectAnswer)
        {
            // Verificar el idioma para el mensaje de éxito
            if (languageCode == "es")
            {
                feedbackTextUI.text = "¡Correcto!";
            }
            else if (languageCode == "en")
            {
                feedbackTextUI.text = "Correct Answer!";
            }
            else
            {
                feedbackTextUI.text = "Correct!"; // Mensaje por defecto
            }

            Debug.Log("Respuesta correcta");
        }
        else
        {
            // Verificar el idioma para el mensaje de error
            if (languageCode == "es")
            {
                feedbackTextUI.text = "Respuesta incorrecta. Intenta de nuevo.";
            }
            else if (languageCode == "en")
            {
                feedbackTextUI.text = "Incorrect answer. Try again.";
            }
            else
            {
                feedbackTextUI.text = "Incorrect. Try again."; // Mensaje por defecto
            }

            Debug.Log("Respuesta incorrecta");
        }
    }

    public void GoToMap()
    {
        Debug.Log("GoTomap funciotn called");
        string lastScene = GlobalSceneManager.instance.GetLastScene();
        //si la escena contiene "observatorio" envialo al mapa
        if (string.IsNullOrEmpty(lastScene))
        {
            Debug.Log("escena vacia");
        }
        Debug.Log("escnea ultima vaciua" + lastScene) ;
        GlobalSceneManager.instance.ChangeScene(lastScene);

    }
}


