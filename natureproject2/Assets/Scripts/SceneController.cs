
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public void GoToMenu()
    {
        GlobalSceneManager.instance.ChangeScene("Menu");
    }

    public void GoToEscena1()
    {
        GlobalSceneManager.instance.ChangeScene("Escena1");
    }

    public void GoToQuestions()
    {
        GlobalSceneManager.instance.ChangeScene("Questions");
    }

    public void GoToTestEscena1()
    {
        GlobalSceneManager.instance.ChangeScene("VAObservatorio6");
    }

    public void GoToTestQuestions()
    {
        GlobalSceneManager.instance.ChangeScene("TestQuestions");
    }

    public void GoToScene2()
    {
        GlobalSceneManager.instance.ChangeScene("Scene 2");
    }

    public void GoToTest1()
    {
        GlobalSceneManager.instance.ChangeScene("Test1");
    }

    public void GoToTestDecimate()
    {
        GlobalSceneManager.instance.ChangeScene("VAObservatorio6 1");
    }

    public void GoToTestDissolve()
    {
        GlobalSceneManager.instance.ChangeScene("VAObservatorio6 2");
    }

    public void GoToTestAmbas()
    {
        GlobalSceneManager.instance.ChangeScene("VAObservatorio6 3");
    }
}
