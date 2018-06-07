using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utils;
using Oracle.DataAccess.Client;
using System.Xml.Linq;

namespace Tester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OracleConnection connection = TestDb.GetDBConnection())
            {
                Cursor.Current = Cursors.WaitCursor;
                Logger log = new Logger();

                try
                {
                    connection.Open();
                    OracleCommand command = connection.CreateCommand();
                    OracleCommand subCommand = connection.CreateCommand();

                    string sql = "SELECT 'ENCABEZADO' TIPO,	to_char(FID.MICROSBSNZDATE, 'dd/MM/yyyy') FECHA," +
                                 "to_char(FID.MICROSBSNZDATE, 'hh:mm:ss') HORA,	FID.FCRINVNUMBER INVNUM, to_char(FID.MICROSBSNZDATE, 'yyyyMM') PERIODO," +
                                 "CAST(FID.SUBTOTAL1 AS INT) SUBTOTAL, CAST(FID.SUBTOTAL2 AS INT) DESCUENTO, CAST(FID.SUBTOTAL3 AS INT) TIPS," +
                                 "CAST(FID.TAXTTL1 AS INT) IVA, CAST(FID.SUBTOTAL8 AS INT) TOTAL, FID.MICROSCHKNUM CHECKNUM, FID.PCWSID TERMINAL" +
                                 "FROM FCR_INVOICE_DATA FID" +
                                 "WHERE to_char(MICROSBSNZDATE, 'yyyyMMdd') = '20151128'" +
                                 "ORDER BY FID.MICROSBSNZDATE DESC";

                    command.CommandText = sql;

                    OracleDataReader reader = command.ExecuteReader();

                    string path = "20151128_BoletaDeVenta.xml";
                    var xDoc = XmlFormatter.OpenFile(path);

                    int i = 0;
                    while (reader.Read())
                    {
                        i++;
                        var documento = new BoletaDeVenta();
                        var bvh = new BoletaDeVentaHeader()
                        {
                            Correlativo = i,
                            InvNumber = Convert.ToInt32(reader["INVNUM"]),
                            //CheckNumber = Convert.ToInt32(reader["CHECKNUM"]),
                            Fecha = reader["FECHA"].ToString(),
                            Periodo = reader["PERIODO"].ToString(),
                            Neto = Convert.ToInt32(reader["SUBTOTAL"]),
                            Total = Convert.ToInt32(reader["TOTAL"]),
                            Usuario = reader["TERMINAL"].ToString()
                        };
                        //XmlFormatter.PrintDocument(path, xDoc, i);
                        //XmlFormatter.PrintHeaderElementsBoletVentaEL(xDoc, path, bvh, i);

                        subSql = "";    //PRINT DETALLES
                        //PRINT PAGOS

                        //Print VALORES(DESCUENTO)
                        //PRINT VALORES(EXENTO)
                        //PRINT VALORES(IVA)
                        //PRINT VALORES(NBASE)
                        //PRINT VALORES(NETO)
                        //PRINT VALORES(PROPINA)
                        //PRINT VALORES(SPA)
                        //PRINT VALORES(TOTAL)
                        //PRINT VALORES(TRANSBANK)
                        //PRINT VALORES(TRANSIVA)
                        //PRINT VALORES(TRANSNETO)
                    }
                    XmlFormatter.RenameXmlNodes(xDoc, path);
                }
                catch (Exception ex)
                {
                    log.W(ex.Message);
                    MessageBox.Show(ex.Message);
                }
            }
            Cursor.Current = Cursors.Default;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Logger log = new Logger();

            log.W("Iniciando log");

            string path = "test.xml";
            XDocument doc = XmlFormatter.OpenFile(path);
            //XmlFormatter.PrintHeaderElementsBoletaDeVenta(doc, path, "cadenaDeParametros");
            //XmlFormatter.PrintDetailElementsBoletaDeVenta(doc, path, "cadenaDeParametros");
            //XmlFormatter.PrintPaymentElementsBoletaDeVenta(doc, path, "cadenaDeParametros");
            //XmlFormatter.PrintValueElementsBoletaDeVenta(doc, path, "cadenaDeParametros");

            log.W("Finalizando log");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            XDocument xDoc = XDocument.Load("20151128_BoletaDeVenta.xml");
            XmlFormatter.RenameXmlNodes(xDoc, "20151128_BoletaDeVenta.xml");
        }
    }
}
