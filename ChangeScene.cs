using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static System.Net.Mime.MediaTypeNames;

public class ChangeScene : MonoBehaviour
{
    public string SceneName;
    public void ChangeMenu(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }
    public void Clear()
    {
        if (AddConstraints.CurrentConstraint < 3)
        {
            AddConstraints.constraintcontents.Clear();
            AddConstraints.CurrentConstraint = 0;
            AddConstraints.constraintswithSurplus.Clear();
        }
    }
    public void clearAll()
    {
        PlayerPrefs.DeleteAll();
        AddVariables.VariableCount = 0;
        AddVariables.position = 0;
        AddConstraints.constraintcontents.Clear();
        AddConstraints.CurrentConstraint = 0;
        AddConstraints.constraintswithSurplus.Clear();
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}