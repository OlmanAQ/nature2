using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DataCollector : MonoBehaviour
{
    private FirebaseFirestore firestore;
    private float timeSpent = 0f;
    private int clickCount = 0;
    private int doubleClickCount = 0;

    private float doubleClickThreshold = 0.3f; // Tiempo m�ximo entre toques para contar como doble toque
    private float tapCooldown = 0.5f; // Tiempo m�nimo entre toques para evitar m�ltiples registros

    private bool isDoubleTapPending = false; // Si est� esperando el segundo toque para el doble toque
    private Coroutine doubleTapCoroutine; // Coroutine para manejar el temporizador de doble toque

    void Start()
    {
        Debug.Log("Comenzando la inicializaci�n de Firebase...");

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
        // Detecci�n de toques en pantalla
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            HandleTap();
        }
        else if (Touchscreen.current == null)
        {
            Debug.LogWarning("Touchscreen.current es null. Aseg�rate de que el sistema de entrada est� configurado correctamente.");
        }
    }

    private void HandleTap()
    {
        if (isDoubleTapPending)
        {
            // Si est� esperando un segundo toque, se registra como doble toque
            doubleClickCount++;
            Debug.Log("Doble toque detectado. Contador de dobles toques: " + doubleClickCount);

            // Reiniciar la l�gica de doble toque
            isDoubleTapPending = false;
            if (doubleTapCoroutine != null)
            {
                StopCoroutine(doubleTapCoroutine); // Detener el temporizador de espera del segundo toque
            }
        }
        else
        {
            // Si no est� esperando un segundo toque, empieza a contar como un primer toque
            clickCount++;
            Debug.Log("Toque detectado. Contador de toques: " + clickCount);

            // Configura la espera para un posible segundo toque
            isDoubleTapPending = true;
            doubleTapCoroutine = StartCoroutine(DoubleTapTimeout()); // Inicia el temporizador de espera
        }
    }

    private IEnumerator DoubleTapTimeout()
    {
        // Espera el tiempo l�mite para un segundo toque
        yield return new WaitForSeconds(doubleClickThreshold);

        // Si el tiempo se cumple sin recibir un segundo toque, se considera un toque simple
        isDoubleTapPending = false;
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
            Debug.LogError("Firestore a�n no est� inicializado.");
            return;
        }

        Debug.Log("Guardando datos en Firestore...");

        string sceneName = SceneManager.GetActiveScene().name;

        var data = new Dictionary<string, object>
        {
            { "TimeSpent", timeSpent },
            { "ClickCount", clickCount },
            { "DoubleClickCount", doubleClickCount },
            { "SceneName", sceneName }
        };

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