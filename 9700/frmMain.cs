//using Oracle.DataAccess.Client;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Utils;

namespace _Simphony
{
    public partial class FrmMain : Form
    {
        #region Declarations
        Logger _log = new Logger();
        ConfigurationReader _configReader = new ConfigurationReader();
        Configuration _configuration;
        Dictionary<string, string> _mapeoItems;
        #endregion

        #region Constructor
        public FrmMain()
        {
            InitializeComponent();
            lblStatus.Visible = false;
            pbProgreso.Visible = false;
        }
        #endregion

        #region Event methods
        private void btnGenerar_Click(object sender, EventArgs e)
        {
            lblStatus.Visible = true;
            lblStatus.Text = "Procesando datos de ventas...";
            using (var connection = ConnDb.GetDBConnection(_configuration))
            {
                GenerarArchivo(connection);
            }
            Cursor.Current = Cursors.Default;
            dtpDesde.Enabled = true;
            btnGenerar.Enabled = true;
            btnSalir.Enabled = true;
        }
        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

        #region Private methods
        private void GenerarArchivo(SqlConnection connection)
        {
            var listaHeader = new List<BoletaVenta>();
            _log.W("Inicia procesamiento para la fecha: " + dtpDesde.Value.ToString("dd-MM-yyyy"));

            if (connection != null)
            {
                dtpDesde.Enabled = false;
                btnGenerar.Enabled = false;
                btnSalir.Enabled = false;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    connection.CreateCommand();

                    var query = "SELECT CASE CD.REVCTRID WHEN 1 THEN 'FATTORIA' WHEN 22 THEN 'QUEEN' WHEN 23 THEN 'ROOMSERVICE' WHEN 24 THEN 'BANQUETES' " +
                                "WHEN 25 THEN 'MEDPRO' WHEN 46 THEN 'CAFETERIA' ELSE 'NODEFINIDO' END REVCENT, CD.CHECKID, CONVERT(VARCHAR(10), CD.DETAILPOSTINGTIME, 105), " +
                                "CD.CHECKDETAILID, CD.DETAILINDEX, CD.DETAILTYPE, CD.EMPLOYEEID, CONVERT(INT, ROUND(ISNULL(CD.TOTAL, 0), 0)) TOTAL, " +
                                "CONVERT(INT, ROUND((ISNULL(CD.TOTAL, 0) - TMDET.CHARGETIP) / 1.19, 0)) NETO, CONVERT(INT, ROUND(((ISNULL(CD.TOTAL, 0) - TMDET.CHARGETIP) / 1.19) * 0.19, 0)) IVA, ST.STRINGTEXT DESCRIPCION, " +
                                "TMDET.CHARGETIP PROPINA, TMDET.TendMedID, CASE TMDET.TENDMEDID WHEN 111 THEN '11-04-015' WHEN 128 THEN '11-04-015' WHEN 110 THEN '11-04-016' WHEN 127 THEN '11-04-016' WHEN 109 THEN '11-04-017' WHEN 126 THEN '11-04-017' " +
                                "WHEN 108 THEN '11-04-018' WHEN 125 THEN '11-04-018' WHEN 112 THEN '11-04-018' WHEN 129 THEN '11-04-018' WHEN 97 THEN '11-01-009' WHEN 130 THEN '11-01-009' WHEN 131 THEN '11-01-007' " +
                                "WHEN 103 THEN '11-01-007' ELSE 'CUENTANODEFINIDA' END CUENTA, ISNULL(UPPER(EMP.CHECKNAME), '') USERNAME FROM CHECK_DETAIL CD INNER JOIN TENDER_MEDIA_DETAIL TMDET ON TMDET.CHECKDETAILID = CD.CHECKDETAILID " +
                                "INNER JOIN TENDER_MEDIA TM ON TM.TENDMEDID = TMDET.TENDMEDID INNER JOIN STRING_TABLE ST ON ST.STRINGNUMBERID = TM.NAMEID INNER JOIN EMPLOYEE EMP ON EMP.EMPLOYEEID = CD.EMPLOYEEID " +
                                " WHERE CONVERT(VARCHAR(10), CD.DETAILPOSTINGTIME, 105) " +
                                "= '" + dtpDesde.Value.ToString("dd-MM-yyyy") + "' AND TMDET.TENDMEDID IN (97, 98, 103, 108, 109, 110, 111, 112, 125, 126, 127, 128, 129, 130, 131)";

                    command.CommandText = query;
                    var reader = command.ExecuteReader();

                    var path = "HRPSC_BOF_Regal_" + dtpDesde.Value.ToString("yyyyMMdd") + ".txt";
                    if (File.Exists(path)) File.Delete(path);

                    var i = 0;
                    while (reader.Read())
                    {
                        i++;

                        var bveh = new BoletaVenta()
                        {

                            Cuenta = reader[13].ToString(),
                            Username = reader[14].ToString(),
                            Debe = Convert.ToInt32(reader[7]),
                            Haber = 0,
                            Glosa = "BL/" + reader[1] + " " + (reader[0].ToString().Length > 7 ? reader[0].ToString().Substring(0,7) : reader[0].ToString()),
                            Fecha = reader[2].ToString(),
                            NroBoleta = reader[1].ToString(),
                            CodAutTbnk = "",
                            Auxiliar = "BOL" + (reader[0].ToString().Length > 7 ? reader[0].ToString().Substring(0, 7) : reader[0].ToString()),
                            CentroCosto = "",
                            TipoDoc = "BL",
                            Total = Convert.ToInt32(reader[7]),
                            MontoNeto = Convert.ToInt32(reader[8]),
                            Iva = Convert.ToInt32(reader[9]),
                            Propina = Convert.ToInt32(reader[11])
                        };

                        listaHeader.Add(bveh);
                    }

                    if (i == 0)
                        _log.W("No hay datos para procesar en la fecha seleccionada.");
                    
                    pbProgreso.Visible = true;
                    pbProgreso.Step = 1;
                    pbProgreso.Value = 0;
                    pbProgreso.Maximum = listaHeader.Count;

                    reader.Close();
                    
                    foreach (var h in listaHeader)
                    {
                        var nroBoleta = string.Empty;
                        var glosa = string.Empty;
                        pbProgreso.Value += 1;

                        // Descuentos ---------------------------------------------------------->
                        //IMPRIMIR DESCUENTOS PARA LA CUENTA 22-22-222
                        var sqlDisc =
                            "SELECT CD.REVCTRID, CD.CHECKID, CD.CHECKDETAILID, CD.DETAILINDEX, CD.DETAILTYPE, " +
							"CD.REVCTRID, CD.EMPLOYEEID, ISNULL(CD.TOTAL, 0), ST.STRINGTEXT FROM CHECK_DETAIL CD INNER JOIN " +
							"DISCOUNT_DETAIL DDET ON DDET.CHECKDETAILID = CD.CHECKDETAILID INNER JOIN DISCOUNT DDEF " +
							"ON DDEF.DSCNTID = DDET.DSCNTID INNER JOIN STRING_TABLE ST ON ST.STRINGNUMBERID = DDEF.NAMEID " +
							"WHERE CHECKID = '" + h.NroBoleta + "' ";
                        //_log.W(sqlDisc);
                        var subCommandDisc = connection.CreateCommand();
                        subCommandDisc.CommandText = sqlDisc;
                        var subReaderDisc = subCommandDisc.ExecuteReader();

                        while (subReaderDisc.Read())
                        {
                            var totalDescSinIva = Convert.ToInt32(Math.Round(Convert.ToInt32(subReaderDisc[7]) / 1.19));
                            var bvedd = new BoletaVenta()
                            {
                                Cuenta = "22-22-222",
                                Debe = totalDescSinIva,
                                Haber = 0,
                                Glosa = h.Glosa,
                                Fecha = h.Fecha,
                                NroBoleta = h.NroBoleta,
                                CodAutTbnk = "",
                                Auxiliar = "",
                                TipoDoc = "BL",
                                Total = 0,
                                CentroCosto = "",
                                MontoNeto = 0,
                                Iva = 0
                            };

                            if (bvedd.Debe < 0) TxtFormatter.PrintDetailElements(path, bvedd, i);
                        }

                        subReaderDisc.Close();
                        // Detalle ------------------------------------------------------------->
                        var sqlDetail =
                            "SELECT CD.REVCTRID, CD.CHECKID, CD.CHECKDETAILID, CD.DETAILINDEX, CD.DETAILTYPE, CD.REVCTRID, CD.EMPLOYEEID, ISNULL(CD.TOTAL, 0), ST.STRINGTEXT, " +
                            "CASE WHEN MIDEF.MENUITEMCLASSOBJNUM BETWEEN 2000 AND 2010 THEN '031' WHEN MIDEF.MENUITEMCLASSOBJNUM BETWEEN 3000 AND 3001 THEN '062' " +
                            "WHEN MIDEF.MENUITEMCLASSOBJNUM BETWEEN 7000 AND 7006 THEN '031' ELSE '022' END CCOSTO, J.JOURNALTEXT FROM CHECK_DETAIL CD " +
                            "INNER JOIN MENU_ITEM_DETAIL MIDET ON MIDET.CHECKDETAILID = CD.CHECKDETAILID INNER JOIN MENU_ITEM_DEFINITION MIDEF ON MIDEF.MENUITEMDEFID = MIDET.MENUITEMDEFID " +
                            "INNER JOIN STRING_TABLE ST ON ST.STRINGNUMBERID = MIDEF.NAME1ID INNER JOIN CHECKS C ON C.CHECKID = CD.CHECKID OUTER APPLY (SELECT TOP 1 * FROM POS_JOURNAL_LOG " +
                            "WHERE TYPE = 1 AND CHECKNUM = C.CHECKNUMBER ORDER BY POSJOURNALLOGID) J WHERE CD.CHECKID = '" + h.NroBoleta + "' AND DETAILTYPE <> 15";

                        var subCommand = connection.CreateCommand();
                        subCommand.CommandText = sqlDetail;
                        var subReaderChecks = subCommand.ExecuteReader();
                        
                        while (subReaderChecks.Read())
                        {
                            var journalText = subReaderChecks[10].ToString();
                            var journalTextItems = journalText.Split('\n').ToList();

                            foreach (var linea in journalTextItems)
                            {
                                _log.W(linea);
                            }

                            var nroBoletaIndex = journalTextItems.FindIndex(s => s.ToUpper().Contains("NROBOLELEC"));
                            var codAutTransbankIndex = journalTextItems.FindIndex(s => s.ToUpper().Contains("CODAUTTBK"));
                            var totalSinIva = Convert.ToInt32(Math.Round(Convert.ToInt32(subReaderChecks[7]) / 1.19));

                            nroBoleta = nroBoletaIndex > 0 ? journalTextItems[nroBoletaIndex + 1].Trim() : string.Empty;
                            glosa = "BL/" + nroBoleta + " " + h.Glosa.Split(' ')[1];
                            var bved = new BoletaVenta()
                            {
                                Cuenta = "40-01-001",
                                Debe = 0,
                                Haber = totalSinIva,
                                Glosa = glosa,
                                Fecha = h.Fecha,
                                NroBoleta = nroBoleta,
                                CodAutTbnk = codAutTransbankIndex > 0 ? journalTextItems[codAutTransbankIndex + 1].Trim() : string.Empty,
                                Auxiliar = "",
                                TipoDoc = "BL",
                                Total = Convert.ToInt32(subReaderChecks[7]),
                                CentroCosto = subReaderChecks[9].ToString(),
                                MontoNeto = 0,
                                Iva = 0
                            };

                            if (bved.Haber > 0)
                            {
                                TxtFormatter.PrintDetailElements(path, bved, i);
                                var bvedIva = new BoletaVenta()
                                {
                                    Cuenta = "21-06-001",
                                    Debe = 0,
                                    Haber = Math.Abs(Convert.ToInt32(subReaderChecks[7])) - totalSinIva,
                                    Glosa = glosa,
                                    Fecha = h.Fecha,
                                    NroBoleta = nroBoleta,
                                    CodAutTbnk = codAutTransbankIndex > 0 ? journalTextItems[codAutTransbankIndex + 1].Trim() : string.Empty,
                                    Auxiliar = "",
                                    TipoDoc = "BL",
                                    Total = Convert.ToInt32(subReaderChecks[7]) - totalSinIva,
                                    CentroCosto = subReaderChecks[9].ToString(),
                                    MontoNeto = 0,
                                    Iva = 0
                                };
                            }
                        }

                        // IVA ----------------------------------------------------------------->
                        if (h.Iva > 0)
                        {
                            var bvedi = new BoletaVenta()
                            {
                                Cuenta = "21-06-001",
                                Debe = 0,
                                Haber = h.Iva,
                                Glosa = glosa,
                                Fecha = h.Fecha,
                                NroBoleta = nroBoleta,
                                CodAutTbnk = "",
                                Auxiliar = "",
                                CentroCosto = "",
                                TipoDoc = "BL",
                                Total = 0,
                                MontoNeto = 0,
                                Iva = 0,
                                RevCen = h.RevCen
                            };
                            TxtFormatter.PrintDetailElements(path, bvedi, i);
                        }

                        // Propinas ------------------------------------------------------------>
                        if (h.Propina > 0)
                        {
                            var bvedp = new BoletaVenta()
                            {
                                Cuenta = "40-02-005",
                                Debe = 0,
                                Haber = h.Propina,
                                Glosa = glosa,
                                Fecha = h.Fecha,
                                NroBoleta = nroBoleta,
                                CodAutTbnk = "",
                                Auxiliar = "",
                                CentroCosto = "",
                                TipoDoc = "BL",
                                Total = h.Total,
                                MontoNeto = 0,
                                Iva = 0,
                                RevCen = h.RevCen
                            };

                            TxtFormatter.PrintDetailElements(path, bvedp, i);
                        }

                        subReaderChecks.Close();
                        h.NroBoleta = nroBoleta;
                        h.Glosa = glosa;
                        TxtFormatter.PrintHeaderElements(path, h, i);
                    }

                    _log.W("Finaliza procesamiento para la fecha: " + dtpDesde.Value.ToString("dd/MM/yyyy"));
                    lblStatus.Text = "Datos de ventas procesados correctamente.";
                }
                catch (Exception ex)
                {
                    _log.W(ex.Message + "|" + (ex.InnerException?.Message ?? ""));
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Archivo de configuración no encontrado, contacte a su proveedor.");
                _log.W("Archivo de configuración no encontrado");
            }
        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                _configuration = _configReader.Read("Config.ini");
                _log.W("Inicia exportador Simphony");
            }
            catch (Exception ex)
            {
                _log.W(ex.Message);
                MessageBox.Show(ex.Message);
                Application.Exit();
            }
        }
        #endregion
    }
}
