﻿using PMMX.Modelo.Entidades;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMMX.Modelo.Map
{
    public class NoConformidadMap : EntityTypeConfiguration<NoConformidad>
    {
        public NoConformidadMap()
        {
            #region Propiedades

            ToTable("NoConformidades");
            HasKey(c => c.Id);

            Property(c => c.Id).HasColumnName("Id");
            Property(c => c.Calificacion_VQI).HasColumnName("VQI");
            Property(c => c.Code).HasColumnName("Code");
            Property(c => c.CodeDescription).HasColumnName("CodeDescription");
            
            Property(c => c.Fecha).HasColumnName("Fecha");
            Property(c => c.IdPersona).HasColumnName("IdPersona");
            Property(c => c.IdWorkCenter).HasColumnName("IdWorkCenter");
            Property(c => c.IdSeccion).HasColumnName("IdSeccion");

            #endregion

            #region HasMany

            #endregion

            #region HasOptional
            #endregion

            #region HasRequired
            HasRequired(c => c.Persona).WithMany(x => x.NoConformidadesIngresadas).HasForeignKey(c => c.IdPersona);
            HasRequired(c => c.Seccion).WithMany(x => x.NoConformidades).HasForeignKey(c => c.IdSeccion);
            HasRequired(c => c.WorkCenter).WithMany(x => x.NoConformidades).HasForeignKey(c => c.IdWorkCenter);

            #endregion






        }
    }
}
