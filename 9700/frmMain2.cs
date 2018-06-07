using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

namespace Exportador9700
{
    public partial class frmMain : Form
    {
        Logger log = new Logger();
        public frmMain()
        {
            InitializeComponent();
        }

        #region Event methods
        private void Form1_Load(object sender, EventArgs e)
        {
            dtpHasta.Value = dtpDesde.Value.AddDays(1);
            dtpHasta.Enabled = false;
        }
        private void cbRango_CheckedChanged(object sender, EventArgs e)
        {
            if (cbRango.Checked)
                dtpHasta.Enabled = true;
            else
                dtpHasta.Enabled = false;
        }
        private void btnGenerar_Click(object sender, EventArgs e)
        {
            using (OracleConnection connection = TestDb.GetDBConnection())
            {
                GenerarArchivo(connection);
            }
            Cursor.Current = Cursors.Default;
            btnGenerar.Enabled = true;
            btnSalir.Enabled = true;
        }
        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void cbRango_CheckedChanged_1(object sender, EventArgs e)
        {
            if (cbRango.Checked)
                dtpHasta.Enabled = true;
            else
                dtpHasta.Enabled = false;
        }
        #endregion
        private void GenerarArchivo(OracleConnection connection)
        {
            string tipoDocumento = "BOLETA DE VENTA";

            if (connection != null)
            {
                btnGenerar.Enabled = false;
                btnSalir.Enabled = false;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    connection.Open();
                    OracleCommand command = connection.CreateCommand();
                    OracleCommand subCommand = connection.CreateCommand();

                    string query;

                    if (!cbRango.Checked)
                        query = "SELECT TO_CHAR(FCRBSNZDATE, 'dd/MM/yyyy') FECHA, TO_CHAR(FCRBSNZDATE, 'yyyyMMdd') FECHA2, TO_CHAR(FCRBSNZDATE, 'yyyyMM') PERIODO, " +
                              "TO_CHAR(FCRBSNZDATE, 'dd/MM/yyyy') HORA, FCRINVNUMBER FCRINVNUMBER, FCRINVNUMBER INVNUM, " +
                              "MICROSCHKNUM CHECKNUM, SUBTOTAL1 NETO, SUBTOTAL2 DESCUENTOS, SUBTOTAL3 SERVICIOS, SUBTOTAL5 EXENTO, " +
                              "SUBTOTAL8 TOTAL, SUBTOTAL9 PROPINA, TAXTTL1 IVA, PCWSID TERMINAL " +
                              "FROM FCR_INVOICE_DATA WHERE TO_CHAR(FCRBSNZDATE, 'yyyyMMdd') = '" + dtpDesde.Value.ToString("yyyyMMdd") + "'" +
                              "ORDER BY FCRBSNZDATE DESC";
                        else
                        query = "SELECT TO_CHAR(FCRBSNZDATE, 'dd/MM/yyyy') FECHA, TO_CHAR(FCRBSNZDATE, 'yyyyMMdd') FECHA2, TO_CHAR(FCRBSNZDATE, 'yyyyMM') PERIODO, " +
                              "TO_CHAR(FCRBSNZDATE, 'dd/MM/yyyy') HORA, FCRINVNUMBER FCRINVNUMBER, FCRINVNUMBER INVNUM, " +
                              "MICROSCHKNUM CHECKNUM, SUBTOTAL1 NETO, SUBTOTAL2 DESCUENTOS, SUBTOTAL3 SERVICIOS, SUBTOTAL5 EXENTO, " +
                              "SUBTOTAL8 TOTAL, SUBTOTAL9 PROPINA, TAXTTL1 IVA, PCWSID TERMINAL " +
                              "FROM FCR_INVOICE_DATA WHERE TO_CHAR(FCRBSNZDATE, 'yyyyMMdd') = '" + dtpDesde.Value.ToString("yyyyMMdd") + "'" +
                              "ORDER BY FCRBSNZDATE DESC";

                    command.CommandText = query;
                    OracleDataReader reader = command.ExecuteReader();

                    string path = dtpDesde.Value.ToString("yyyyMMdd") + "_BoletaDeVenta.xml";
                    var xDoc = XmlFormatter.OpenFile(path);

                    int i = 0;
                    while (reader.Read())
                    {
                        i++;

                        var bveh = new BoletaVentaHeader()
                        {
                            Correlativo = i,
                            InvNumber = Convert.ToInt32(reader["INVNUM"]),
                            Fecha = reader["FECHA"].ToString(),
                            Hora = reader["HORA"].ToString(),
                            Usuario = reader["TERMINAL"].ToString(),
                            Periodo = reader["PERIODO"].ToString(),
                            Neto = Convert.ToDecimal(reader["NETO"]),
                            Subtotal = Convert.ToDecimal(reader["NETO"]),
                            Total = Convert.ToDecimal(reader["TOTAL"])
                        };

                        XmlFormatter.PrintDocument(path, xDoc, i);
                        XmlFormatter.PrintHeaderElements(xDoc, path, tipoDocumento, bveh, i);

                        //DETALLE ---------------------------------------------------------------------
                        var sqlChecks = "SELECT CHECKID, CHECKNUMBER, EMPLOYEEID, CASHIERID, WORKSTATIONID, CHECKOPEN, " +
                                        "TABLEOPEN, CHECKCLOSE, SPLITFROMCHECKNUM, SUBTOTAL, TAX, OTHER, PAYMENT, DUE FROM CHECKS " +
                                        "WHERE TO_CHAR(CHECKPOSTINGTIME, 'YYYYMMDD') = '" + reader["FECHA2"].ToString() + "' and " +
                                        "CHECKNUMBER = " + reader["CHECKNUM"].ToString();

                        command = connection.CreateCommand();
                        command.CommandText = sqlChecks;
                        OracleDataReader subReaderChecks = command.ExecuteReader();

                        int j = 0;
                        while (subReaderChecks.Read())
                        {
                            //ITEMS ---------------------------------------------------------------------
                            var sqlCheckDetail = "SELECT CHECK_DETAIL.CHECKDETAILID, CHECK_DETAIL.CHECKID, CHECK_DETAIL.DETAILINDEX, " +
                                                 "CHECK_DETAIL.DETAILTYPE, CHECK_DETAIL.TOTAL, CHECK_DETAIL.SALESCOUNT, STRING_TABLE.STRINGTEXT, MENU_ITEM_DEFINITION.MENUITEMDEFID MENUITEMDEFID " +
                                                 "FROM CHECK_DETAIL INNER JOIN MENU_ITEM_DETAIL ON CHECK_DETAIL.CHECKDETAILID = " +
                                                 "MENU_ITEM_DETAIL.CHECKDETAILID INNER JOIN MENU_ITEM_DEFINITION ON MENU_ITEM_DEFINITION.MENUITEMDEFID = " +
                                                 "MENU_ITEM_DETAIL.MENUITEMDEFID INNER JOIN STRING_TABLE ON STRING_TABLE.STRINGNUMBERID = " +
                                                 "MENU_ITEM_DEFINITION.NAME1ID WHERE CHECKID = " + subReaderChecks["CHECKID"] + "ORDER BY CHECK_DETAIL.DETAILINDEX";

                            command = connection.CreateCommand();
                            command.CommandText = sqlCheckDetail;
                            OracleDataReader subReaderDetail = command.ExecuteReader();

                            int k = 0;
                            while (subReaderDetail.Read())
                            {
                                k++;
                                var bved = new BoletaVentaDetalle()
                                {
                                    Correlativo = i,
                                    Secuencia = k,
                                    Articulo = subReaderDetail["MENUITEMDEFID"].ToString(),
                                    Cantidad = Convert.ToInt32(subReaderDetail["SALESCOUNT"]),
                                    Precio = Convert.ToInt32(subReaderDetail["TOTAL"]) / Convert.ToInt32(subReaderDetail["SALESCOUNT"]),
                                    Subtotal = decimal.Round(Convert.ToDecimal(subReaderDetail["TOTAL"]) / 1.19m, 2),
                                    Iva = decimal.Round(Convert.ToDecimal(subReaderDetail["TOTAL"]) * 0.19m, 2),
                                    Fecha = reader["FECHA"].ToString(),
                                    Total = Convert.ToInt32(subReaderDetail["TOTAL"])
                                };
                                XmlFormatter.PrintDetail(xDoc, path, i, k);
                                XmlFormatter.PrintDetailElements(xDoc, path, tipoDocumento, bved, i, k);
                            }
                            //PAGOS ---------------------------------------------------------------------
                            var sqlCheckPayment = "SELECT CHECK_DETAIL.CHECKDETAILID, CHECK_DETAIL.CHECKID, CHECK_DETAIL.DETAILINDEX, " +
                                                  "CHECK_DETAIL.DETAILTYPE, CHECK_DETAIL.TOTAL, CHECK_DETAIL.SALESCOUNT, TENDER_MEDIA_DETAIL.TENDMEDID " +
                                                  "FROM CHECK_DETAIL INNER JOIN TENDER_MEDIA_DETAIL ON CHECK_DETAIL.CHECKDETAILID = TENDER_MEDIA_DETAIL.CHECKDETAILID " +
                                                  "WHERE CHECKID = " + subReaderChecks["CHECKID"] + " AND TOTAL > 0 ORDER BY CHECK_DETAIL.DETAILINDEX";
                            decimal transbank = 0;
                            command = connection.CreateCommand();
                            command.CommandText = sqlCheckPayment;
                            OracleDataReader subReaderPayment = command.ExecuteReader();

                            int l = 0;
                            while (subReaderPayment.Read())
                            {
                                l++;
                                if (subReaderPayment["TENDMEDID"].ToString() == "21")
                                    transbank += Convert.ToDecimal(subReaderPayment["TOTAL"]);

                                var bvep = new BoletaVentaPago()
                                {
                                    Correlativo = i,
                                    BillNo = Convert.ToInt32(reader["INVNUM"]),
                                    Linea = l,
                                    Monto = Convert.ToDecimal(subReaderPayment["TOTAL"]),
                                    TrxCode = subReaderPayment["TENDMEDID"].ToString(),
                                    Fecha = reader["FECHA"].ToString()
                                };
                                XmlFormatter.PrintPayment(xDoc, path, i, l);
                                XmlFormatter.PrintPaymentElements(xDoc, path, tipoDocumento, bvep, i, l);
                            }

                            //VALORES ---------------------------------------------------------------------
                            //DESCUENTO -------------------------------------------------------------------
                            var sqlCheckDiscount = "SELECT ABS(CHECK_DETAIL.TOTAL) TOTAL " +
                                                   "FROM CHECK_DETAIL WHERE CHECKID = " + subReaderChecks["CHECKID"] +
                                                   "AND DETAILTYPE = 2 ORDER BY CHECK_DETAIL.DETAILINDEX";

                            command = connection.CreateCommand();
                            command.CommandText = sqlCheckDiscount;
                            OracleDataReader subReaderDiscount = command.ExecuteReader();

                            int m = 0;
                            while (subReaderDiscount.Read())
                            {
                                m++;
                                var bvev = new BoletaVentaValores()
                                {
                                    Correlativo = i,
                                    Nombre = "DESCUENTO",
                                    Factor = -1,
                                    Orden = 2,
                                    Monto = Convert.ToDecimal(subReaderDiscount["TOTAL"]),
                                    Porcentaje = 0
                                };
                                XmlFormatter.PrintValues(xDoc, path, i, 1);
                                XmlFormatter.PrintValueElements(xDoc, path, tipoDocumento, bvev, i, 1);
                            }

                            //No hubo descuentos, grabo en cero
                            if(m == 0)
                            {
                                var bvev = new BoletaVentaValores()
                                {
                                    Correlativo = i,
                                    Nombre = "DESCUENTO",
                                    Factor = -1,
                                    Orden = 2,
                                    Monto = 0,
                                    Porcentaje = 0
                                };
                                XmlFormatter.PrintValues(xDoc, path, i, 1);
                                XmlFormatter.PrintValueElements(xDoc, path, tipoDocumento, bvev, i, 1);
                            }

                            //VALORES ------------------------------------------------------------------
                            //EXENTO -------------------------------------------------------------------
                            var bvev2 = new BoletaVentaValores()
                            {
                                Correlativo = i,
                                Nombre = "EXENTO",
                                Factor = 1,
                                Orden = 3,
                                Monto = 0,
                                Porcentaje = 0
                            };
                            XmlFormatter.PrintValues(xDoc, path, i, 2);
                            XmlFormatter.PrintValueElements(xDoc, path, tipoDocumento, bvev2, i, 2);

                            //VALORES ---------------------------------------------------------------
                            //IVA -------------------------------------------------------------------
                            string iva = "0";

                            if (reader["IVA"].ToString() != string.Empty)
                                iva = reader["IVA"].ToString();

                            bvev2 = new BoletaVentaValores()
                            {
                                Correlativo = i,
                                Nombre = "IVA",
                                Factor = 1,
                                Orden = 3,
                                Monto = Convert.ToDecimal(iva),
                                Porcentaje = 0
                            };
                            XmlFormatter.PrintValues(xDoc, path, i, 3);
                            XmlFormatter.PrintValueElements(xDoc, path, tipoDocumento, bvev2, i, 3);

                            //VALORES ---------------------------------------------------------------
                            //NBASE -----------------------------------------------------------------
                            bvev2 = new BoletaVentaValores()
                            {
                                Correlativo = i,
                                Nombre = "IVA",
                                Factor = 0,
                                Orden = 6,
                                Monto = Convert.ToDecimal(reader["NETO"]),
                                Porcentaje = 0
                            };
                            XmlFormatter.PrintValues(xDoc, path, i, 4);
                            XmlFormatter.PrintValueElements(xDoc, path, tipoDocumento, bvev2, i, 4);
                            //VALORES ---------------------------------------------------------------
                            //NETO ------------------------------------------------------------------
                            bvev2 = new BoletaVentaValores()
                            {
                                Correlativo = i,
                                Nombre = "IVA",
                                Factor = 0,
                                Orden = 6,
                                Monto = Convert.ToDecimal(reader["NETO"]),
                                Porcentaje = 0
                            };
                            XmlFormatter.PrintValues(xDoc, path, i, 5);
                            XmlFormatter.PrintValueElements(xDoc, path, tipoDocumento, bvev2, i, 5);
                            //VALORES ---------------------------------------------------------------
                            //PROPINA ---------------------------------------------------------------
                            string propina = "0";

                            if (reader["PROPINA"].ToString() != string.Empty)
                                propina = reader["PROPINA"].ToString();

                            bvev2 = new BoletaVentaValores()
                            {
                                Correlativo = i,
                                Nombre = "PROPINA",
                                Factor = 1,
                                Orden = 1,
                                Monto = Convert.ToDecimal(propina),
                                Porcentaje = 0
                            };
                            XmlFormatter.PrintValues(xDoc, path, i, 6);
                            XmlFormatter.PrintValueElements(xDoc, path, tipoDocumento, bvev2, i, 6);
                            //VALORES ---------------------------------------------------------------
                            //SPA -------------------------------------------------------------------
                            bvev2 = new BoletaVentaValores()
                            {
                                Correlativo = i,
                                Nombre = "SPA",
                                Factor = 1,
                                Orden = 7,
                                Monto = 0,
                                Porcentaje = 0
                            };
                            XmlFormatter.PrintValues(xDoc, path, i, 7);
                            XmlFormatter.PrintValueElements(xDoc, path, tipoDocumento, bvev2, i, 7);
                            //VALORES ---------------------------------------------------------------
                            //TOTAL -----------------------------------------------------------------
                            bvev2 = new BoletaVentaValores()
                            {
                                Correlativo = i,
                                Nombre = "TOTAL",
                                Factor = 0,
                                Orden = 6,
                                Monto = Convert.ToDecimal(reader["NETO"]),
                                Porcentaje = 0
                            };
                            XmlFormatter.PrintValues(xDoc, path, i, 8);
                            XmlFormatter.PrintValueElements(xDoc, path, tipoDocumento, bvev2, i, 8);
                            //VALORES ---------------------------------------------------------------
                            //TRANSBANK -------------------------------------------------------------
                            bvev2 = new BoletaVentaValores()
                            {
                                Correlativo = i,
                                Nombre = "TRANSBANK",
                                Factor = 0,
                                Orden = 6,
                                Monto = transbank,
                                Porcentaje = 0
                            };
                            XmlFormatter.PrintValues(xDoc, path, i, 9);
                            XmlFormatter.PrintValueElements(xDoc, path, tipoDocumento, bvev2, i, 9);
                            //VALORES ---------------------------------------------------------------
                            //TRANSIVA --------------------------------------------------------------
                            bvev2 = new BoletaVentaValores()
                            {
                                Correlativo = i,
                                Nombre = "TRANSIVA",
                                Factor = 0,
                                Orden = 6,
                                Monto = transbank - decimal.Round(transbank / 1.19m),
                                Porcentaje = 0
                            };
                            XmlFormatter.PrintValues(xDoc, path, i, 10);
                            XmlFormatter.PrintValueElements(xDoc, path, tipoDocumento, bvev2, i, 10);
                            //VALORES ---------------------------------------------------------------
                            //TRANSNETO -------------------------------------------------------------
                            bvev2 = new BoletaVentaValores()
                            {
                                Correlativo = i,
                                Nombre = "TRANSNETO",
                                Factor = 0,
                                Orden = 6,
                                Monto = decimal.Round(transbank / 1.19m),
                                Porcentaje = 0
                            };
                            XmlFormatter.PrintValues(xDoc, path, i, 11);
                            XmlFormatter.PrintValueElements(xDoc, path, tipoDocumento, bvev2, i, 11);
                        }
                    }
                    XmlFormatter.RenameXmlNodes(xDoc, path);
                }
                catch (Exception ex)
                {
                    log.W(ex.Message + "|" + ex.InnerException.Message);
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Archivo de configuración no encontrado, contacte a su proveedor.");
                log.W("Archivo de configuración no encontrado");
            }
        }
    }
}
