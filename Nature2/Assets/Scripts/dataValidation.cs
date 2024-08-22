using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;


public class dataValidation : MonoBehaviour
{
    public GameObject panel;
    public TMP_Dropdown age;
    public TMP_Dropdown nationality;

    // Start is called before the first frame update
    private string sex = "";
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void maleSex(){
        sex = "male";
    }

    public void femaleSex(){
        sex = "female";
    }

    public void otherSex(){
        sex = "other";
    }


    
    public void validation(){

        string ageData = age.options[age.value].text;
        string nationalityData = nationality.options[nationality.value].text;

        if (ageData == "" || nationalityData == "" || sex == ""){
            panel.SetActive(true);
        }else{
            SceneManager.LoadScene("experience");
        }
    }

    public void validationEspañol(){

        string ageData = age.options[age.value].text;
        string nationalityData = nationality.options[nationality.value].text;

        if (ageData == "" || nationalityData == "" || sex == ""){
            panel.SetActive(true);
        }else{
            SceneManager.LoadScene("experienceEspañol");
        }
    }

    public void okValidation(){
        panel.SetActive(false);
    }

}
