﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMMX.Modelo.Vistas
{
    public class CarrierView
    {
        #region Propiedades
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string NombreCorto { get; set; }
        public bool Activo { get; set; }
        #endregion

        #region Navegacion
        public ICollection<VentanaView> Ventanas { get; set; }
        #endregion
    }
}