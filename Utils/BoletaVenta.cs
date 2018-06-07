using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    public class BoletaVenta
    {
        public string Cuenta { get; set; }
        public int Debe { get; set; }
        public int Haber { get; set; }
        public string Glosa { get; set; }
        public string Fecha { get; set; }
        public string NroBoleta { get; set; }
        public string CodAutTbnk { get; set; }
        public string Auxiliar { get; set; }
        public string CentroCosto { get; set; }
        public string TipoDoc { get; set; }
        public int MontoNeto { get; set; }
        public int Iva { get; set; }
        public int Total { get; set; }
        public int Propina { get; set; }
        public string RevCen { get; set; }
        public string Username { get; set; }
    }
}
