using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace Utils
{
    public class TxtFormatter
    {
        #region Constants
        protected const string LISTA_DOCUMENTOS = "DOCUMENTO_LIST";
        protected const string DOCUMENTO = "DOCUMENTO";
        protected const string HEADER = "ENCABEZADO";
        protected const string DETAIL = "DETALLE";
        protected const string PAYMENT = "PAGOS";
        protected const string VALUES = "VALORES";
        #endregion

        #region Document
        /// <summary>
        /// Imprime estructura del documento sin datos con tags de encabezado
        /// </summary>
        /// <param name="path">Ruta del documento a generar</param>
        /// <param name="doc">Objeto del XML</param>
        /// <param name="index">Número de documento a imprimir</param>
        public static void PrintDocument(string path, XDocument doc, int index)
        {
            PrintHeader(doc, path, index);
            doc.Save(path);
        }
        public static void PrintHeaderElements(string filePath, BoletaVenta bv, int index)
        {
            using (var outputFile = new StreamWriter(filePath, append: true))
            {
                outputFile.Write(bv.Cuenta + ";");      //ok
                outputFile.Write(bv.Debe + ";");        //ok
                outputFile.Write(bv.Haber + ";");       //ok
                outputFile.Write(bv.Glosa + ";");
                outputFile.Write(bv.Fecha + ";");       //ok
                outputFile.Write(bv.NroBoleta + ";");   
                outputFile.Write(bv.Username + ";");    //ok
                outputFile.Write("" + ";");
                outputFile.Write(bv.CodAutTbnk + ";");  //Falta cod
                outputFile.Write("" + ";");
                outputFile.Write(bv.Auxiliar + ";");    //ok
                outputFile.Write("" + ";"); //1
                outputFile.Write("" + ";");
                outputFile.Write("" + ";");
                outputFile.Write("" + ";");
                outputFile.Write("" + ";");
                outputFile.Write("" + ";");
                outputFile.Write("" + ";"); //7
                outputFile.Write(bv.Auxiliar + ";");    //ok
                outputFile.Write("BL" + ";");           //ok
                outputFile.Write(bv.NroBoleta + ";");   //Falta nro doc
                outputFile.Write(bv.Fecha + ";");
                outputFile.Write(bv.Fecha + ";");
                outputFile.Write("BL" + ";");
                outputFile.Write(bv.NroBoleta + ";");   //Falta nro doc
                outputFile.Write("" + ";");
                outputFile.Write(bv.MontoNeto + ";");   //ok
                outputFile.Write(bv.Propina + ";");     //Falta propina (CORREGIDO)
                outputFile.Write(bv.Iva + ";");         //ok
                outputFile.Write("" + ";"); //1
                outputFile.Write("" + ";");
                outputFile.Write("" + ";");
                outputFile.Write("" + ";");
                outputFile.Write("" + ";"); //5
                outputFile.WriteLine(bv.Total);         //ok
            }
        }
        public static void PrintDetailElements(string filePath, BoletaVenta bv, int index)
        {
            using (var outputFile = new StreamWriter(filePath, append: true))
            {
                outputFile.Write(bv.Cuenta + ";");
                outputFile.Write(bv.Debe + ";");
                outputFile.Write(bv.Haber + ";");
                outputFile.Write(bv.Glosa + ";");
                outputFile.Write(bv.Fecha + ";");
                outputFile.Write(bv.NroBoleta + ";");
                outputFile.Write("" + ";");
                outputFile.Write("" + ";");
                outputFile.Write(bv.CodAutTbnk + ";");
                outputFile.Write("" + ";"); //1
                outputFile.Write("" + ";");
                outputFile.Write("" + ";");
                outputFile.Write("" + ";");
                outputFile.Write("" + ";"); //5
                outputFile.Write("" + ";");
                outputFile.Write(bv.CentroCosto + ";");
                outputFile.Write("" + ";"); //1
                outputFile.Write("" + ";");
                outputFile.Write("" + ";");
                outputFile.Write("" + ";");
                outputFile.Write("" + ";"); //5
                outputFile.Write("" + ";");
                outputFile.Write("" + ";");
                outputFile.Write("" + ";");
                outputFile.Write("" + ";");
                outputFile.Write("" + ";"); //10
                outputFile.Write("" + ";");
                outputFile.Write("" + ";");
                outputFile.Write("" + ";");
                outputFile.Write("" + ";");
                outputFile.Write("" + ";"); //15
                outputFile.Write("" + ";");
                outputFile.Write("" + ";");
                outputFile.Write("" + ";");
                outputFile.WriteLine(""); //19
            }
        }
        #endregion

        #region Public methods
        public static XDocument OpenFile(string filePath)
        {
            if (!File.Exists(filePath))
                return new XDocument(new XElement(LISTA_DOCUMENTOS));
            else
            {
                File.Delete(filePath);
                return new XDocument(new XElement(LISTA_DOCUMENTOS));
            }
        }
        public static void PrintDetail(XDocument doc, string filePath, int i, int j)
        {
            XElement detail = new XElement(DETAIL + "-" + j);
            doc.Element(LISTA_DOCUMENTOS).Element(DOCUMENTO + "-" + i).Add(detail);
            doc.Save(filePath);
        }
        public static void PrintPayment(XDocument doc, string filePath, int i, int j)
        {
            XElement payment = new XElement(PAYMENT + "-" + j);
            doc.Element(LISTA_DOCUMENTOS).Element(DOCUMENTO + "-" + i).Add(payment);
            doc.Save(filePath);
        }
        public static void PrintValues(XDocument doc, string filePath, int i, int j)
        {
            XElement value = new XElement(VALUES + "-" + j);
            doc.Element(LISTA_DOCUMENTOS).Element(DOCUMENTO + "-" + i).Add(value);
            doc.Save(filePath);
        }
        public static void RenameXmlNodes(XDocument doc, string filePath)
        {
            Logger log = new Logger();
            log.W("Renaming " + filePath);

            foreach(var element in doc.Descendants())
            {
                if (element.Name.LocalName.StartsWith("DOCUMENTO-"))
                    element.Name = DOCUMENTO;
                if (element.Name.LocalName.StartsWith("DETALLE-"))
                    element.Name = DETAIL;
                if (element.Name.LocalName.StartsWith("PAGOS-"))
                    element.Name = PAYMENT;
                if (element.Name.LocalName.StartsWith("VALORES-"))
                    element.Name = VALUES;
            }
            doc.Save(filePath);
        }
        #endregion

        #region Protected methods
        protected static void PrintHeader(XDocument doc, string filePath, int index)
        {
            XElement header = new XElement(HEADER);
            doc.Element(LISTA_DOCUMENTOS).Element(DOCUMENTO + "-" + index).Add(header);
            doc.Save(filePath);
        }
        #endregion
    }
}
