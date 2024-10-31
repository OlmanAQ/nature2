using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using System.Threading.Tasks;
using System.Collections;

public class QuestionManager : MonoBehaviour
{
    public Text questionTextUI;  // Text UI element to display the question.
    public Button[] optionButtons;  // Array of buttons for the answer options.
    public Text feedbackTextUI;  // Text UI element to display feedback.

    private FirebaseFirestore db;
    private Dictionary<string, object> loadedQuestions;
    private string currentCorrectAnswer;

    private int maxRetries = 10;  // Número de intentos para reintentar cargar las preguntas si falla.

    void Start()
    {
        // Inicializar Firebase Firestore.
        db = FirebaseFirestore.DefaultInstance;

        // Obtener el nombre de la última escena desde GlobalSceneManager.
        string lastScene = GlobalSceneManager.instance.GetLastScene();

        // Verificar si hay una escena anterior válida.
        if (string.IsNullOrEmpty(lastScene))
        {
            Debug.LogError("No se pudo obtener el nombre de la última escena.");
            return;
        }

        // Iniciar la coroutine para cargar preguntas desde Firestore utilizando el nombre de la última escena.
        StartCoroutine(TryLoadQuestionsFromFirestore(lastScene, maxRetries));
    }

    // Coroutine que intenta cargar las preguntas con reintentos.
    IEnumerator TryLoadQuestionsFromFirestore(string sceneDocumentName, int retries)
    {
        while (retries > 0)
        {
            // Coroutine para llamar a la función de cargar preguntas.
            yield return StartCoroutine(LoadQuestionsFromFirestore(sceneDocumentName));

            if (loadedQuestions != null && loadedQuestions.Count > 0)
            {
                // Si las preguntas se cargaron correctamente, mostramos una pregunta aleatoria.
                ShowRandomQuestion();
                yield break;  // Salimos del bucle ya que las preguntas se cargaron bien.
            }

            retries--;
            Debug.LogWarning("Intento fallido, reintentando... Restantes: " + retries);
            yield return new WaitForSeconds(1);  // Esperar 1 segundo antes de volver a intentar.
        }

        Debug.LogError("No se pudieron cargar las preguntas después de varios intentos.");
    }

    // Coroutine para cargar las preguntas desde Firestore.
    IEnumerator LoadQuestionsFromFirestore(string sceneDocumentName)
    {
        Task<DocumentSnapshot> task = db.Collection("Preguntas").Document(sceneDocumentName).GetSnapshotAsync();

        // Esperar hasta que la tarea esté completa.
        while (!task.IsCompleted)
        {
            yield return null;  // Dejar que la coroutine espere hasta que Firebase responda.
        }

        if (task.IsFaulted)
        {
            Debug.LogError("Error al obtener el documento: " + task.Exception);
            yield break;
        }

        DocumentSnapshot snapshot = task.Result;
        if (snapshot.Exists)
        {
            // Obtener todas las preguntas como un diccionario.
            loadedQuestions = snapshot.ToDictionary();

            Debug.Log("Preguntas cargadas correctamente");
        }
        else
        {
            Debug.LogError("No se encontró el documento: " + sceneDocumentName);
        }
    }

    // Función para mostrar una pregunta aleatoria.
    void ShowRandomQuestion()
    {
        if (loadedQuestions == null || loadedQuestions.Count == 0)
        {
            Debug.LogError("No hay preguntas cargadas.");
            return;
        }

        // Elegir una pregunta aleatoria de las preguntas cargadas.
        int randomIndex = Random.Range(1, loadedQuestions.Count + 1);  // Índice aleatorio para "PreguntaX".
        string randomQuestionKey = "Pregunta" + randomIndex;

        Debug.Log("Mostrando: " + randomQuestionKey);

        if (loadedQuestions.ContainsKey(randomQuestionKey))
        {
            // Verificar que los datos de la pregunta sean válidos.
            if (!(loadedQuestions[randomQuestionKey] is Dictionary<string, object> questionData))
            {
                Debug.LogError("Los datos de la pregunta no están formateados correctamente.");
                return;
            }

            // Verificar si el campo 'pregunta' existe y es un string.
            if (!questionData.ContainsKey("pregunta") || !(questionData["pregunta"] is string questionText))
            {
                Debug.LogError("El campo 'pregunta' no se encuentra o no está correctamente formateado.");
                return;
            }

            // Verificar si el campo 'respuestas' existe y es una lista.
            if (!questionData.ContainsKey("respuestas") || !(questionData["respuestas"] is List<object> respuestasList))
            {
                Debug.LogError("El campo 'respuestas' no se encuentra o no está correctamente formateado.");
                return;
            }

            // Verificar si el campo 'correcta' existe y es un número entero.
            if (!questionData.ContainsKey("correcta") || !(questionData["correcta"] is long correctAnswerIndexLong))
            {
                Debug.LogError("El campo 'correcta' no se encuentra o no está correctamente formateado.");
                return;
            }

            // Convertir 'correcta' a int.
            int correctAnswerIndex = (int)correctAnswerIndexLong;

            // Validar que el índice de la respuesta correcta esté dentro del rango de la lista.
            if (correctAnswerIndex < 0 || correctAnswerIndex >= respuestasList.Count)
            {
                Debug.LogError("El índice de la respuesta correcta está fuera de rango.");
                return;
            }

            // Asignar la respuesta correcta.
            currentCorrectAnswer = respuestasList[correctAnswerIndex].ToString();

            // Mostrar el texto de la pregunta en la UI.
            questionTextUI.text = questionText;

            // Asignar las respuestas a los botones.
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (i < respuestasList.Count)
                {
                    // Asignar texto a cada botón.
                    string respuestaTexto = respuestasList[i].ToString();
                    optionButtons[i].GetComponentInChildren<Text>().text = respuestaTexto;
                    optionButtons[i].gameObject.SetActive(true);

                    // Eliminar oyentes anteriores para evitar errores.
                    optionButtons[i].onClick.RemoveAllListeners();

                    // Agregar oyente para verificar la respuesta al hacer clic.
                    int index = i;  // Necesario para la captura de la variable en el closure.
                    optionButtons[i].onClick.AddListener(() => CheckAnswer(respuestaTexto));
                }
                else
                {
                    optionButtons[i].gameObject.SetActive(false); // Desactivar botones que no tienen respuestas.
                }
            }
        }
        else
        {
            Debug.LogError("Pregunta no encontrada: " + randomQuestionKey);
        }
    }

    // Función para verificar si la respuesta seleccionada es correcta.
    void CheckAnswer(string selectedAnswer)
    {
        // Verificar si la respuesta seleccionada es correcta.
        if (selectedAnswer == currentCorrectAnswer)
        {
            feedbackTextUI.text = "¡Correcto!";
            Debug.Log("Respuesta correcta");
        }
        else
        {
            feedbackTextUI.text = "Respuesta incorrecta. Intenta de nuevo.";
            Debug.Log("Respuesta incorrecta");
        }
    }
}
