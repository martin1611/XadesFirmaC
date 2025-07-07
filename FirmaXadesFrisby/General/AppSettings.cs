using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmaXadesFrisby.General
{
    public static class AppSettings
    {
        #region Variables

        public static string UrlPoliticaFirma => "https://facturaelectronica.dian.gov.co/politicadefirma/v2/politicadefirmav2.pdf";
        public static string DescripcionPolitica => "Política de firma para facturas electrónicas de la República de Colombia";
        public static string HashValuePolitica => "dMoMvtcG5aIzgYo0tIsSQeVJBDnUnfSOfBpxXrmor0Y=";
        public static int Emisor => 0;
        public static int ProveedorTecnologico => 1;

        #endregion
    }
}
