
using System.Collections.Generic;
using UnityEngine;

public class GlobalSceneManager : MonoBehaviour
{
    public static GlobalSceneManager instance;

    // Lista para guardar el nombre de las escenas visitadas
    public List<string> sceneHistory = new List<string>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Hace que este objeto persista entre escenas
        }
        else
        {
            Destroy(gameObject); // Destruye duplicados de GlobalSceneManager
        }
    }

    // Método para cambiar de escena
    public void ChangeScene(string sceneName)
    {
        // Agregar el nombre de la escena actual a la lista de historial
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        sceneHistory.Add(currentSceneName);

        // Cambiar a la nueva escena
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    // Obtener el nombre de la última escena visitada (excepto la de preguntas)
    public string GetLastScene()
    {
        if (sceneHistory.Count > 0)
        {
            // Devolver la última escena visitada antes de la escena actual
            return sceneHistory[sceneHistory.Count - 1];
        }
        else
        {
            Debug.LogError("No hay escenas en el historial.");
            return null;
        }
    }
}
