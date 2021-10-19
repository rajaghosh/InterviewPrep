using System;
using System.Collections.Generic;
using System.Text;

namespace DesignPatterns
{
    class TemplateMethodDesignPattern
    {
        public TemplateMethodDesignPattern()
        {
            ExcelToWord _ew = new ExcelToWord();
            _ew.Execute();
        }
    }

    //Here we have the template
    public abstract class Etl
    {
        public abstract void Extract();
        public abstract void Transform();
        public abstract void Load();
        public void Execute()
        {
            Extract();
            Transform();
            Load();
        }
    }

    //Child class will determine the specific steps of the algorithm
    public class ExcelToWord : Etl
    {
        public override void Extract()
        {
            Console.WriteLine("Extraction is completed");
        }

        public override void Load()
        {
            Console.WriteLine("Loading is completed");
        }

        public override void Transform()
        {
            Console.WriteLine("Tranformation is completed");
        }
    }
}
/*
 * Template Method is a behavioral design pattern that defines the skeleton / template of an algorithm in the superclass 
 * but lets subclasses override specific steps of the algorithm without changing its structure.
 * 
 * Difference from Factory Design Pattern is - Factory Pattern Creates the Interfaces and let Sub-Class implement the Interface but 
 * in Template Method its the Algorithm needed to be followed.
 * 
 * 
 * 
 * 
 */