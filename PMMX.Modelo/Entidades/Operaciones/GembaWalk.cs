﻿using PMMX.Modelo.Entidades.Defectos;
using PMMX.Modelo.Entidades.Maquinaria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PMMX.Modelo.Entidades.Operaciones;
using System.ComponentModel.DataAnnotations;

namespace PMMX.Modelo.Entidades.Operaciones
{
    public class GembaWalk
    {
        #region Propiedades
        public int Id { get; set; }
        public int IdEvento { get; set; }
        public int IdReportador { get; set; }
        public int IdOrigen { get; set; }

        [StringLength(250)]
        public string Descripcion { get; set; }
        public DateTime FechaReporte { get; set; }
        public DateTime FechaEstimada { get; set; }
        public int Prioridad { get; set; }
        public int IdResponsable { get; set; }
        public int IdSubCategoria { get; set; }
        public int Tipo { get; set; }
        public Boolean Activo { get; set; }
        #endregion

        #region Navegacion
        public Persona Reportador { get; set; }
        public Persona Responsable { get; set; }
        public Origen Origen { get; set; }
        public Evento Evento { get; set; }
        public SubCategoria SubCategoria { get; set; }
        public List<Foto> Fotos { get; set; }
        public ICollection<BitacoraGembaWalk> BitacoraGembaWalk { get; set; }
        #endregion
    }
}
