using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine.InputSystem;

public class DataCollector : MonoBehaviour
{
    private FirebaseFirestore firestore;
    private float timeSpent = 0f;
    private int clickCount = 0;
    private int doubleClickCount = 0;

    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.3f; // Tiempo máximo entre clics para contar como doble clic

    void Start()
    {
        Debug.Log("Comenzando la inicialización de Firebase...");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                firestore = FirebaseFirestore.DefaultInstance;

                if (firestore != null)
                {
                    Debug.Log("Firestore inicializado correctamente.");
                }
                else
                {
                    Debug.LogError("Error al inicializar Firestore.");
                }

                StartCoroutine(TimeCounter()); // Comienza a contar el tiempo
            }
            else
            {
                Debug.LogError("Firebase no pudo inicializarse. Error: " + task.Result);
            }
        });
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            clickCount++;

            // Detección de doble clic
            float currentTime = Time.time;
            if (currentTime - lastClickTime <= doubleClickThreshold)
            {
                doubleClickCount++;
                Debug.Log("Doble clic detectado");
            }
            lastClickTime = currentTime;
        }
    }

    IEnumerator TimeCounter()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            timeSpent += 1f;
        }
    }

    public void SaveData()
    {
        if (firestore == null)
        {
            Debug.LogError("Firestore aún no está inicializado.");
            return; // Evita que el método continúe si `firestore` es null
        }

        Debug.Log("Guardando datos en Firestore...");

        // Crea un diccionario con los datos
        var data = new Dictionary<string, object>
        {
            { "TimeSpent", timeSpent },
            { "ClickCount", clickCount },
            { "DoubleClickCount", doubleClickCount }
        };

        // Guarda los datos en la colección "UserData"
        firestore.Collection("UserData").AddAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Datos guardados exitosamente en Firestore.");
            }
            else
            {
                Debug.LogError("Error al guardar datos en Firestore: " + task.Exception);
            }
        });
    }
}
