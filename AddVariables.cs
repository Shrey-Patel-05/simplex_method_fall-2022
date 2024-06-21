
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class AddVariables : MonoBehaviour
{
    public static int VariableCount = 0;
    public TextMeshProUGUI variableIn;
    public TextMeshProUGUI output;
    private string VariableName;
    public static int position = 0;
    private bool validInput;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddButtonVariable()
    {
        validInput = true;
        string Position = Convert.ToString(position);
        string temp = Convert.ToString(0);
        if (PlayerPrefs.GetString(temp) == Convert.ToString(variableIn.text) || variableIn.text == "" || variableIn.text[0] == '0' || variableIn.text[0] == '1' || variableIn.text[0] == '2' || variableIn.text[0] == '3' || variableIn.text[0] == '4' || variableIn.text[0] == '5' || variableIn.text[0] == '6' || variableIn.text[0] == '7' || variableIn.text[0] == '8' || variableIn.text[0] == '9')
        {
            validInput = false;
        }
        for (int i = 0; i < position; i++)
        {
            temp = Convert.ToString(i);
            if (PlayerPrefs.GetString(temp) == Convert.ToString(variableIn.text) || variableIn.text == "" || variableIn.text[0] == '0' || variableIn.text[0] == '1' || variableIn.text[0] == '2' || variableIn.text[0] == '3' || variableIn.text[0] == '4' || variableIn.text[0] == '5' || variableIn.text[0] == '6' || variableIn.text[0] == '7' || variableIn.text[0] == '8' || variableIn.text[0] == '9')
            {
                validInput = false;
            }
        }

        if (validInput == true)
        {
            VariableName = Convert.ToString(variableIn.text);
            PlayerPrefs.SetString(Position, VariableName);
            position += 1;
            VariableCount += 1;
            output.text = "New Variable Created: " + VariableName;
            variableIn.text = "";
        }
    }

}



