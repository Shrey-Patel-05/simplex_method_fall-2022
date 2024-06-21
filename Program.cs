using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class SolveProblem : MonoBehaviour
{

    public TextMeshProUGUI Output;
    private bool TableSee = false;
    private List<string> artificialCluster = new List<string>();
    private bool Big = false;
    private int ArrayCounter = 1;
    public TMP_Dropdown dropdownTables;
    List<string> StoreArrays = new List<string>();
    List<string> options = new List<string>();


    void Start()
    {
        if (AddConstraints.CurrentConstraint > 2)
        {
            TableSee = false;
            if (AddConstraints.constraintswithSurplus.Count > 0)
            {
                Big = true;
                TwoStage();
            }
            else
            {
                Table();
            }
            PopulateList();
        }
    }
    public void TwoStage()
    {
        int NumberOfRows = AddConstraints.CurrentConstraint + 1;
        int NumberOfColumns = AddVariables.VariableCount + AddConstraints.CurrentConstraint + 1 + AddConstraints.constraintswithSurplus.Count;
        double[,] table = new double[NumberOfRows, NumberOfColumns];
        string[,] variablenames = new string[AddConstraints.CurrentConstraint + 1, AddVariables.VariableCount + 1];
        int current = 1;
        int indexofvariable = 0;
        bool containsvariable = false;

        for (int i = 0; i < NumberOfRows; i++)
        {
            for (int j = 0; j < NumberOfColumns; j++)
            {
                table[i, j] = 0;
            }
        }
        table[0, 0] = 1;

        for (int i = 0; i < AddVariables.VariableCount; i++)
        {
            string Position = Convert.ToString(i);
            variablenames[0, i] = (PlayerPrefs.GetString(Position));
        }
        variablenames[0, AddVariables.VariableCount] = "RHS";

        for (int i = 0; i < AddConstraints.constraintcontents.Count; i++)
        {
            for (int j = 0; j < AddVariables.VariableCount; j++)
            {
                if (AddConstraints.constraintcontents[i].Contains(variablenames[0, j]))
                {
                    indexofvariable = j;
                    containsvariable = true;
                }
            }
            if (containsvariable == false)
            {
                variablenames[current, AddVariables.VariableCount] = AddConstraints.constraintcontents[i];
                current += 1;
            }
            else if (containsvariable == true)
            {
                variablenames[current, indexofvariable] = AddConstraints.constraintcontents[i].Remove(AddConstraints.constraintcontents[i].IndexOf(variablenames[0, indexofvariable]));
                containsvariable = false;
            }
        }
        for (int i = 0; i < AddConstraints.CurrentConstraint + 1; i++)
        {
            for (int j = 0; j < AddVariables.VariableCount + 1; j++)
            {
                if (variablenames[i, j] == null)
                {
                    variablenames[i, j] = "0";
                }
            }
        }
        for (int i = 0; i < AddVariables.VariableCount; i++)
        {
            table[0, i + 1] = Convert.ToDouble(variablenames[1, i]);
            table[0, i + 1] = -table[0, i + 1];
        }
        table[0, NumberOfColumns - 1] = Convert.ToDouble(variablenames[1, AddVariables.VariableCount]);

        for (int i = 1; i < AddConstraints.CurrentConstraint; i++)
        {
            for (int j = 1; j < AddVariables.VariableCount + 1; j++)
            {
                table[i, j] = Convert.ToDouble(variablenames[i + 1, j - 1]);
            }
            table[i, NumberOfColumns - 1] = Convert.ToDouble(variablenames[i + 1, AddVariables.VariableCount]);
        }




        double RHS = 0;
        for (int i = 2; i < AddConstraints.CurrentConstraint + 1; i++)
        {
            foreach (var item in AddConstraints.constraintswithSurplus)
            {
                if (item + 1 == i)
                {
                    RHS += Convert.ToDouble(variablenames[i, AddVariables.VariableCount]);
                }
            }
        }

        table[NumberOfRows - 1, NumberOfColumns - 1] = RHS;

        double likeTerms = 0;
        for (int i = 0; i < AddVariables.VariableCount; i++)
        {
            for (int j = 2; j < AddConstraints.CurrentConstraint + 1; j++)
            {
                foreach (var item in AddConstraints.constraintswithSurplus)
                {
                    if (item + 1 == j)
                    {
                        likeTerms += Convert.ToDouble(variablenames[j, i]);
                    }
                }
            }
            table[NumberOfRows - 1, i + 1] = likeTerms;
            likeTerms = 0;
        }



        for (int i = 1; i < NumberOfRows - 1; i++)
        {
            table[i, (AddVariables.VariableCount + i)] = 1;
            foreach (var item in AddConstraints.constraintswithSurplus)
            {
                if (i == item)
                {
                    table[i, (AddVariables.VariableCount + i)] = -1;
                    table[NumberOfRows - 1, AddVariables.VariableCount + i] = -1;
                }
            }
        }

        int artificalcounter = 0;
        foreach (var item in AddConstraints.constraintswithSurplus)
        {
            table[item, AddVariables.VariableCount + AddConstraints.CurrentConstraint + artificalcounter] = 1;
            artificalcounter += 1;
        }


        TwoStageSolver(AddVariables.VariableCount, table, NumberOfRows, NumberOfColumns, TableSee);


        Big = false;
        int StandardNumberOfRows = AddConstraints.CurrentConstraint;
        int StandardNumberOfColumns = AddVariables.VariableCount + AddConstraints.CurrentConstraint + 1;
        double[,] Standardtable = new double[StandardNumberOfRows, StandardNumberOfColumns];
        for (int i = 0; i < NumberOfRows - 1; i++)
        {
            for (int j = 0; j < AddVariables.VariableCount + AddConstraints.CurrentConstraint; j++)
            {
                Standardtable[i, j] = table[i, j];
            }
            Standardtable[i, StandardNumberOfColumns - 1] = table[i, NumberOfColumns - 1];
        }
        Solver(AddVariables.VariableCount, Standardtable, StandardNumberOfRows, StandardNumberOfColumns, TableSee);
        if (TableSee == false)
        {
            OptimumSolution(StandardNumberOfRows, StandardNumberOfColumns, AddVariables.VariableCount, Standardtable);
        }


    }


    public void Table()
    {
        int NumberOfRows = AddConstraints.CurrentConstraint;
        int NumberOfColumns = AddVariables.VariableCount + AddConstraints.CurrentConstraint + 1;
        double[,] table = new double[NumberOfRows, NumberOfColumns];
        for (int i = 0; i < NumberOfRows; i++)
        {
            for (int j = 0; j < NumberOfColumns; j++)
            {
                table[i, j] = 0;
            }
        }
        for (int i = 1; i < NumberOfRows; i++)
        {
            table[i, (AddVariables.VariableCount + i)] = 1;
        }
        table[0, 0] = 1;

        string[,] variablenames = new string[AddConstraints.CurrentConstraint + 1, AddVariables.VariableCount + 1];
        int current = 1;
        int indexofvariable = 0;
        bool containsvariable = false;

        for (int i = 0; i < AddVariables.VariableCount; i++)
        {
            string Position = Convert.ToString(i);
            variablenames[0, i] = (PlayerPrefs.GetString(Position));
        }
        variablenames[0, AddVariables.VariableCount] = "RHS";

        for (int i = 0; i < AddConstraints.constraintcontents.Count; i++)
        {
            for (int j = 0; j < AddVariables.VariableCount; j++)
            {
                if (AddConstraints.constraintcontents[i].Contains(variablenames[0, j]))
                {
                    indexofvariable = j;
                    containsvariable = true;
                }
            }
            if (containsvariable == false)
            {
                variablenames[current, AddVariables.VariableCount] = AddConstraints.constraintcontents[i];
                current += 1;
            }
            else if (containsvariable == true)
            {
                variablenames[current, indexofvariable] = AddConstraints.constraintcontents[i].Remove(AddConstraints.constraintcontents[i].IndexOf(variablenames[0, indexofvariable]));
                containsvariable = false;
            }
        }
        for (int i = 0; i < AddConstraints.CurrentConstraint + 1; i++)
        {
            for (int j = 0; j < AddVariables.VariableCount + 1; j++)
            {
                if (variablenames[i, j] == null)
                {
                    variablenames[i, j] = "0";
                }
            }
        }
        for (int i = 0; i < AddVariables.VariableCount; i++)
        {
            table[0, i + 1] = Convert.ToDouble(variablenames[1, i]);
            table[0, i + 1] = -table[0, i + 1];
        }
        table[0, NumberOfColumns - 1] = Convert.ToDouble(variablenames[1, AddVariables.VariableCount]);

        for (int i = 1; i < AddConstraints.CurrentConstraint; i++)
        {
            for (int j = 1; j < AddVariables.VariableCount + 1; j++)
            {
                table[i, j] = Convert.ToDouble(variablenames[i + 1, j - 1]);
            }
            table[i, NumberOfColumns - 1] = Convert.ToDouble(variablenames[i + 1, AddVariables.VariableCount]);
        }
        Solver(AddVariables.VariableCount, table, NumberOfRows, NumberOfColumns, TableSee);
        if (TableSee == false)
        {
            OptimumSolution(NumberOfRows, NumberOfColumns, AddVariables.VariableCount, table);
        }

    }
    public void TwoStageSolver(int NumberOfVariables, double[,] table, int NumberOfRows, int NumberOfColumns, bool Tablesee)
    {
        if (FindLargest(AddVariables.VariableCount, NumberOfRows, table) > 0)
        {
            SaveTable(NumberOfRows, NumberOfColumns, table);
            double[] pivots = FindPivotElementAndDivide(NumberOfRows, NumberOfColumns, NumberOfVariables, table);
            DoOneRound(NumberOfRows, NumberOfColumns, NumberOfVariables, table, pivots);
            SaveTable(NumberOfRows, NumberOfColumns, table);
            while (Math.Round(FindLargest(AddVariables.VariableCount, NumberOfRows, table), 10) > 0)
            {
                pivots = FindPivotElementAndDivide(NumberOfRows, NumberOfColumns, NumberOfVariables, table);
                DoOneRound(NumberOfRows, NumberOfColumns, NumberOfVariables, table, pivots);
                SaveTable(NumberOfRows, NumberOfColumns, table);
            }

        }
    }


    public void Solver(int NumberOfVariables, double[,] table, int NumberOfRows, int NumberOfColumns, bool Tablesee)
    {
        if (Math.Round(FindSmallest(NumberOfVariables, table), 10) < 0)
        {
            SaveTable(NumberOfRows, NumberOfColumns, table);
            double[] pivots = FindPivotElementAndDivide(NumberOfRows, NumberOfColumns, NumberOfVariables, table);
            DoOneRound(NumberOfRows, NumberOfColumns, NumberOfVariables, table, pivots);
            SaveTable(NumberOfRows, NumberOfColumns, table);
            while (Math.Round(FindSmallest(NumberOfVariables, table), 10) < 0)
            {

                pivots = FindPivotElementAndDivide(NumberOfRows, NumberOfColumns, NumberOfVariables, table);
                DoOneRound(NumberOfRows, NumberOfColumns, NumberOfVariables, table, pivots);
                SaveTable(NumberOfRows, NumberOfColumns, table);
            }

        }
    }
    static double FindSmallest(int NumberOfVariables, double[,] table)
    {
        double smallest = 0;
        for (int i = 1; i < NumberOfVariables + AddConstraints.CurrentConstraint; i++)
        {
            if (table[0, i] < smallest)
            {
                smallest = table[0, i];
            }
        }
        return smallest;
    }

    static double FindLargest(int NumberOfVariables, int NumberOfRows, double[,] table)
    {
        double largest = 0;
        for (int i = 1; i < NumberOfVariables + AddConstraints.CurrentConstraint + AddConstraints.constraintswithSurplus.Count; i++)
        {
            if (table[NumberOfRows - 1, i] > largest)
            {
                largest = table[NumberOfRows - 1, i];
            }
        }
        return largest;
    }
    private double[] FindPivotElementAndDivide(int NumberOfRows, int NumberOfColumns, int NumberOfVariables, double[,] table)
    {
        double smallest = 0;
        double[] ElRoSc = { 0, 0, 0 };
        int smallestcolumn = 0;
        int pivotelementrow = 1;
        int InitialEl = 2;
        double pivotelement;
        if (Big == true)
        {
            for (int i = 1; i < NumberOfColumns - 1; i++)
            {
                if (table[NumberOfRows - 1, i] > smallest)
                {
                    smallest = table[NumberOfRows - 1, i];
                    smallestcolumn = i;
                    ElRoSc[2] = i;
                }
            }
        }
        else
        {
            for (int i = 1; i < NumberOfVariables + AddConstraints.CurrentConstraint; i++)
            {
                if (table[0, i] < smallest)
                {
                    smallest = table[0, i];
                    smallestcolumn = i;
                    ElRoSc[2] = i;
                }
            }
        }


        smallest = table[1, NumberOfColumns - 1] / table[1, smallestcolumn];
        pivotelement = table[1, smallestcolumn];
        ElRoSc[0] = table[1, smallestcolumn];
        ElRoSc[1] = 1;
        while (smallest < 0 && InitialEl < NumberOfRows)
        {
            smallest = table[InitialEl, NumberOfColumns - 1] / table[InitialEl, smallestcolumn];
            pivotelement = table[InitialEl, smallestcolumn];
            ElRoSc[0] = table[InitialEl, smallestcolumn];
            ElRoSc[1] = InitialEl;
            InitialEl += 1;
        }


        if (Big == true)
        {
            NumberOfRows -= 1;
        }

        for (int i = 2; i < NumberOfRows; i++)
        {
            if ((table[i, NumberOfColumns - 1] / table[i, smallestcolumn] < smallest) && table[i, smallestcolumn] > 0)
            {
                smallest = table[i, NumberOfColumns - 1] / table[i, smallestcolumn];
                pivotelement = table[i, smallestcolumn];
                ElRoSc[0] = table[i, smallestcolumn];
                pivotelementrow = i;
                ElRoSc[1] = i;
            }

        }
        for (int i = 0; i < NumberOfColumns; i++)
        {
            table[pivotelementrow, i] /= pivotelement;
        }
        return ElRoSc;
    }

    static void DoOneRound(int NumberOfRows, int NumberOfColumns, int NumberOfVariables, double[,] table, double[] ElRoSc)
    {
        double n = 0;
        double pivotelement = ElRoSc[0];
        int pivotelementrow = Convert.ToInt32(ElRoSc[1]);
        int column = Convert.ToInt32(ElRoSc[2]);
        double[,] original = new double[1, NumberOfColumns];

        for (int i = 0; i < NumberOfColumns; i++)
        {
            original[0, i] = table[pivotelementrow, i];
        }
        for (int i = 0; i < NumberOfRows; i++)
        {
            if (i != pivotelementrow)
            {
                n = table[i, column] / table[pivotelementrow, column];
                if (n != 0)
                {
                    for (int j = 0; j < NumberOfColumns; j++)
                    {
                        table[pivotelementrow, j] *= n;
                        table[i, j] -= table[pivotelementrow, j];
                    }
                }
            }
        }
        for (int i = 0; i < NumberOfColumns; i++)
        {
            table[pivotelementrow, i] = original[0, i];
        }
    }
    public void OutputTable(int NumberOfRows, int NumberOfColumns, double[,] table)
    {

        string[,] StringTable = new string[NumberOfRows, NumberOfColumns];
        string Biggest = "";
        int Length = 0;
        for (int i = 0; i < NumberOfRows; i++)
        {
            for (int j = 0; j < NumberOfColumns; j++)
            {
                StringTable[i, j] = Convert.ToString(Math.Round(table[i, j], 2));
                if (StringTable[i, j].Length > Biggest.Length)
                {
                    Biggest = StringTable[i, j];
                    Length = Biggest.Length;
                }
            }
        }
        for (int i = 0; i < NumberOfRows; i++)
        {

            Output.text = Output.text + "\r\n";
            Output.text = Output.text + (" | ");
            for (int j = 0; j < NumberOfColumns; j++)
            {
                Output.text = Output.text + StringTable[i, j].PadRight(Length) + " | ";
            }
            Output.text = Output.text + "\r\n";
        }

    }

    public void OptimumSolution(int NumberOfRows, int NumberOfColumns, int NumberOfVariables, double[,] table)
    {
        int onecount = 0;
        bool basic = true;
        int rownum = 1;
        if (table[0, NumberOfColumns - 1] > 0)
        {
            Output.text = Output.text + ($"\nObejctive Function = {table[0, NumberOfColumns - 1]}");
        }
        else
        {
            Output.text = Output.text + ($"\nObejctive Function = {-table[0, NumberOfColumns - 1]}");
        }
        for (int i = 1; i < NumberOfColumns; i++)
        {
            for (int j = 0; j < NumberOfRows; j++)
            {
                if (table[j, i] == 1)
                {
                    rownum = j;
                    onecount += 1;
                }
                if (Math.Round(table[j, i], 2) != 0 && table[j, i] != 1)
                {
                    basic = false;
                }
            }
            if (onecount == 1 && basic == true && i <= NumberOfVariables)
            {
                Output.text = Output.text + ($"\nVariable {PlayerPrefs.GetString(Convert.ToString(i - 1))} = {table[rownum, NumberOfColumns - 1]}");
            }
            if (onecount == 1 && basic == true && i > NumberOfVariables)
            {
                Output.text = Output.text + ($"\nSurplus Variable {i - NumberOfVariables} = {table[rownum, NumberOfColumns - 1]}");
            }
            if ((onecount != 1 || basic != true) && i <= NumberOfVariables)
            {
                Output.text = Output.text + ($"\nVariable {PlayerPrefs.GetString(Convert.ToString(i - 1))} = 0");
            }
            onecount = 0;
            rownum = 1;
            basic = true;
        }
    }

    public void SaveTable(int NumberOfRows, int NumberOfColumns, double[,] table)
    {
        for (int i = 0; i < NumberOfRows; i++)
        {
            for (int j = 0; j < NumberOfColumns; j++)
            {
                StoreArrays.Add(Convert.ToString(table[i, j]));
            }
            StoreArrays.Add("ROW");
        }
        StoreArrays.Add($"ARRAY{ArrayCounter}");
        ArrayCounter += 1;
    }
    public void ShowTables()
    {
        try
        {
            List<string> TempListOfAll = StoreArrays;
            List<string> CurrentArray = new List<string>();
            if (dropdownTables.options[dropdownTables.value].text.Contains("Table "))
            {
                if (dropdownTables.value > 1 && dropdownTables.value <= ArrayCounter)
                {
                    TempListOfAll.RemoveRange(0, TempListOfAll.IndexOf($"ARRAY{dropdownTables.value - 1}") + 1);
                }


                Output.text = "Table " + dropdownTables.value + "\r\n";
                int columnlength = 0;
                int counter = 0;
                int RowCount = 0;
                int elcount = 0;

                while (elcount < TempListOfAll.IndexOf($"ARRAY{dropdownTables.value}"))
                {
                    if (TempListOfAll[elcount].Contains("ROW"))
                    {
                        RowCount += 1;
                        columnlength = TempListOfAll.IndexOf("ROW");
                        TempListOfAll.RemoveRange(0, (TempListOfAll.IndexOf("ROW") + 1));
                        elcount = 0;
                    }
                    else
                    {
                        CurrentArray.Add(TempListOfAll[elcount]);
                        elcount += 1;
                    }
                }
                TempListOfAll.Remove($"ARRAY{dropdownTables.value}");

                double[,] tempTable = new double[RowCount, columnlength];
                for (int i = 0; i < RowCount; i++)
                {
                    for (int j = 0; j < columnlength; j++)
                    {
                        tempTable[i, j] = Convert.ToDouble(CurrentArray[counter]);
                        counter += 1;
                    }
                }

                OutputTable(RowCount, columnlength, tempTable);
            }
            else
            {
                TableSee = false;
                if (AddConstraints.constraintswithSurplus.Count > 0)
                {
                    Big = true;
                    TwoStage();
                }
                else
                {
                    Table();
                }
            }
        }
        catch (Exception)
        {
            Output.text = "Optimum solution:\r\n";
        }
    }

    public void PopulateList()
    {
        options.Add("Optimum Solution");
        for (int i = 1; i < ArrayCounter; i++)
        {
            options.Add("Table " + i);
        }
        dropdownTables.AddOptions(options);
    }

}

