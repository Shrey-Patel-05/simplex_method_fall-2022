using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using static System.Net.Mime.MediaTypeNames;

public class AddConstraints : MonoBehaviour
{

    public TMP_Dropdown dropdownVariables;
    public TextMeshProUGUI LHSRHS;
    public TextMeshProUGUI SignFlip;
    public TextMeshProUGUI OutputLHS;
    public TextMeshProUGUI OutputRHS;
    public TextMeshProUGUI ConstraintName;
    public TextMeshProUGUI sign;
    public List<string> variables = new List<string>() { };
    private List<string> variablesforconstraints = new List<string>() { };
    public static List<string> constraintcontents = new List<string>();
    private List<string> contents = new List<string>();
    public static int CurrentConstraint = 0;
    private string VariableName;
    private bool LHS = true;
    private int count = -1;
    public static List<int> constraintswithSurplus = new List<int>();
    // Start is called before the first frame update
    void Start()
    {
        PopulateList();
        if (CurrentConstraint == 0)
        {
            ConstraintName.text = "Objective Function";
            OutputRHS.text = "P";
            sign.text = "=";
        }
        else if (CurrentConstraint > 0)
        {
            ConstraintName.text = "Constraint " + CurrentConstraint;
            OutputRHS.text = "Constant";
            sign.text = "≤";
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ButtonNumbers(TextMeshProUGUI value)
    {
        if (LHS == true)
        {
            if (OutputLHS.text == "Coefficient")
            {
                OutputLHS.text = "";
            }
            OutputLHS.text = OutputLHS.text + value.text;
        }
        if (LHS == false)
        {
            if (OutputRHS.text == "Constant")
            {
                OutputRHS.text = "";
            }
            OutputRHS.text = OutputRHS.text + value.text;
        }
    }

    public void ButtonBack()
    {
        if (LHS == true && OutputLHS.text.Length > 0)
        {
            if (OutputLHS.text == "Coefficient")
            {
                OutputLHS.text = "";
            }
            else
            {
                OutputLHS.text = OutputLHS.text.Substring(0, OutputLHS.text.Length - 1);
            }
        }
        if (LHS == false && OutputRHS.text.Length > 0)
        {
            if (OutputRHS.text == "Constant")
            {
                OutputRHS.text = "";
            }
            else
            {
                OutputRHS.text = OutputRHS.text.Substring(0, OutputRHS.text.Length - 1);
            }
        }
    }
    public void ButtonNegative()
    {
        if (LHS == true)
        {
            if (OutputLHS.text == "Coefficient")
            {
                OutputLHS.text = "-";
            }
            else if (OutputLHS.text.Length == 0)
            {
                OutputLHS.text = "-";
            }
        }
        if (LHS == false)
        {
            if (OutputRHS.text == "Constant")
            {
                OutputRHS.text = "-";
            }
            else if (OutputRHS.text.Length == 0)
            {
                OutputRHS.text = "-";
            }
        }
    }
    public void ButtonPoint()
    {
        if (LHS == true && OutputLHS.text.Length > 0)
        {
            if (OutputLHS.text == "Coefficient")
            {
                OutputLHS.text = "0.";
            }
            else
            {
                if (OutputLHS.text.Contains('.') == false)
                {
                    OutputLHS.text = OutputLHS.text + ".";
                }
                if (OutputLHS.text == "-.")
                {
                    OutputLHS.text = "-0.";
                }
            }
        }
        if (LHS == false && OutputRHS.text.Length > 0)
        {
            if (OutputRHS.text == "Constant")
            {
                OutputRHS.text = "0.";
            }
            else
            {
                if (OutputRHS.text.Contains('.') == false)
                {
                    OutputRHS.text = OutputRHS.text + ".";
                }
                if (OutputRHS.text == "-.")
                {
                    OutputRHS.text = "-0.";
                }
            }
        }
    }
    public void Selected()
    {
        int index = dropdownVariables.value;
        VariableName = dropdownVariables.options[index].text;

    }
    public void ButtonAdd()
    {
        if (dropdownVariables.options.Count != 0)
        {
            if (OutputLHS.text == "Coefficient")
            {
                OutputLHS.text = "0";
            }
            if (VariableName == null)
            {
                VariableName = dropdownVariables.options[0].text;
            }
            variables.Remove(VariableName);

            dropdownVariables.ClearOptions();
            dropdownVariables.AddOptions(variables);
            string variabletoadd = OutputLHS.text + VariableName;
            variablesforconstraints.Add(variabletoadd);


            VariableName = null;
            OutputLHS.text = "Coefficient";
        }
    }

    public void ButtonDone()
    {
        for (int i = 0; i < variablesforconstraints.Count; i++)
        {
            contents.Add(variablesforconstraints[i]);
        }
        variablesforconstraints.Clear();
        if (OutputRHS.text == "Constant" || OutputRHS.text == "P")
        {
            OutputRHS.text = "0";
        }

        contents.Add(OutputRHS.text);
        foreach (var item in contents)
        {
            constraintcontents.Add(item);
        }
        contents.Clear();
        count += 1;
        if (sign.text == "≥")
        {
            constraintswithSurplus.Add(count);
        }
        if (sign.text == "=" || sign.text == "≥")
        {
            sign.text = "≤";
        }
        SignFlip.text = "≥";
        CurrentConstraint += 1;
        ConstraintName.text = "Constraint " + CurrentConstraint;
        OutputRHS.text = "Constant";
        OutputLHS.text = "Coefficient";
        variables.Clear();
        dropdownVariables.ClearOptions();
        PopulateList();
        LHSRHS.text = "RHS";
        LHS = true;


    }
    public void SwitchLHSandRHS()
    {
        if (OutputRHS.text != "P")
        {
            string LHSRHSstring = Convert.ToString(LHSRHS.text);
            if (LHSRHSstring == "RHS")
            {
                LHSRHS.text = "LHS";
                LHS = false;
            }
            if (LHSRHSstring == "LHS")
            {
                LHSRHS.text = "RHS";
                LHS = true;
            }
        }
    }
    public void SwitchSign()
    {
        if (OutputRHS.text != "P")
        {
            if (sign.text == "≥")
            {
                sign.text = "≤";
                SignFlip.text = "≥";
            }
            else if (sign.text == "≤")
            {
                sign.text = "≥";
                SignFlip.text = "≤";
            }
        }
    }
    public void PopulateList()
    {
        variables.Clear();
        dropdownVariables.ClearOptions();
        for (int i = 0; i < AddVariables.position; i++)
        {
            string Position = Convert.ToString(i);
            variables.Add(PlayerPrefs.GetString(Position));
        }
        dropdownVariables.AddOptions(variables);
    }
}



