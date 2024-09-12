using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{
    // Este método se llama al iniciar el script.
    void Start()
    {
        // Inicializa Firebase y verifica dependencias.
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                // Firebase está correctamente inicializado.
                FirebaseApp app = FirebaseApp.DefaultInstance;
                Debug.Log("Firebase se ha inicializado correctamente.");
            }
            else
            {
                // Error en las dependencias de Firebase.
                Debug.LogError($"Firebase no pudo inicializarse correctamente: {task.Result}");
            }
        });
    }
}

