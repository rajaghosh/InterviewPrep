using System;
using System.Collections.Generic;
using System.Text;

namespace DesignPatterns
{
    class AdapterDesignPattern
    {
        public AdapterDesignPattern()
        {
            IReport _report = new EmailReport();
            _report.Export();
            _report = new WordReport();
            _report.Export();
            _report = new ThirdPartyPDF();
            _report.Export();
        }
    }

    public interface IReport
    {
        public void SetValueForReport(object a);
        public void Export();
    }

    public class EmailReport : IReport
    {
        private Excel _e1;
        public void Export()
        {
            Console.WriteLine($"Excel Is Exported with data {_e1.ExcelId} {_e1.ExcelName}");
        }

        public void SetValueForReport(object a1)
        {
            a1 = new Excel() { ExcelId = 1, ExcelName = "Excel 1" };
            _e1 = (Excel)a1;
        }

        public EmailReport()
        {
            SetValueForReport(new Excel());
        }
    }

    public class WordReport : IReport
    {
        private Word _w1;
        public void Export()
        {
            Console.WriteLine($"Word Is Exported with data {_w1.WordId} {_w1.WordName}");
        }

        public void SetValueForReport(object a2)
        {
            a2 = new Word() { WordId = 1, WordName = "Word 1" };
            _w1 = (Word)a2;
        }

        public WordReport()
        {
            SetValueForReport(new Word());
        }
    }

    //To Accomodate "ThirdPartyPDF" we would wrap the implementation inside another compatible class that will implement IReport.
    //Wrapper Class is a class which wraps some logic such that the logic stays inside class and other members can access it with some definite access level only.
    //This will be done so that we have no issues with conn. end points. [Here we are implementing using methods as we just need to make sure interface names are correct]
    public class ThirdPartyPDF : IReport
    {
        private ThirdPartyPdf _tp;

        public void Export()
        {
            Console.WriteLine($"PDF Is Exported with data {_tp.pdfType} {_tp.pdfOwner}");
        }

        public void SetValueForReport(object a3)
        {
            //Suppose Third Party Pdf has some method we can accomodate that inside this method or any other method inside this Wrapper class only
            //Such that any new method should not affect any other methods outside wrapper class
            //This is very important to implement 3rd party integrations
            //Because if any issue occurs we can directly debug one particular wrapper class and this will solve the issue.
            a3 = new ThirdPartyPdf { pdfType = "Important", pdfOwner = "Owner 1" };
            _tp = (ThirdPartyPdf)a3;
        }

        public ThirdPartyPDF()
        {
            SetValueForReport(new ThirdPartyPdf());
        }
    }

    public class Excel
    {
        public int ExcelId { get; set; }
        public string ExcelName { get; set; }
    }

    public class Word
    {
        public int WordId { get; set; }
        public string WordName { get; set; }
    }

    //This structure is different
    public class ThirdPartyPdf
    {
        public string pdfType { get; set; }
        public string pdfOwner { get; set; }
    }

}
/*
 * 
 * Adapter Design Pattern - Also called Wrapper Class Pattern.
 * This design pattern allows 2 dissimilar classes to interact using 2 different "Interface" having similar communication endpoints.
 * This comes under "Structural Design Pattern" as this combines the capability of 2 independent interfaces.
 * 
 * 
 * 
 * 
 * 
 * 
 */
