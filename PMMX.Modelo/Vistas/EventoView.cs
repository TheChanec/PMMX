﻿using PMMX.Modelo.Vistas;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PMMX.Modelo.Vistas
{
    public class EventoView
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public int IdOrigen { get; set; }
        public int IdAsignador { get; set; }
        public int IdResponsable { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}")]
        public DateTime FechaInicio { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}")]
        public DateTime FechaFin { get; set; }
        public string Nota { get; set; }
        public bool EsRecurrente { get; set; }
        public bool Activo { get; set; }

        public PersonaView Asignador { get; set; }
        public PersonaView Responsable { get; set; }
        public OrigenView Origen { get; set; }

        public List<JustDoItView> JustDoIt { get; set; }
    }
}
