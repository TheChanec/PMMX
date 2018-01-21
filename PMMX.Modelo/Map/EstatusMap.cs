﻿using PMMX.Modelo.Entidades.Operaciones;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace PMMX.Modelo.Entidades.Operaciones
{
    public class EstatusMap : EntityTypeConfiguration<Estatus>
    {
        public EstatusMap()
        {
            #region Propiedades
            ToTable("Estatus");
            HasKey(c => c.Id);
            Property(c => c.Id).HasColumnName("Id");
            Property(c => c.Nombre).HasColumnName("Nombre");
            Property(c => c.IdCategoria).HasColumnName("IdCategoria");
            Property(c => c.Activo).HasColumnName("Activo");
            #endregion

            #region HasMany
            this.HasMany(c => c.StatusVentana).WithRequired(x => x.Status).HasForeignKey(c => c.IdStatus);
            this.HasMany(c => c.Rechazo).WithRequired(x => x.Estatus).HasForeignKey(c => c.IdStatus);
            #endregion

            #region HasOptional
            #endregion

            #region HasRequired
            this.HasRequired(c => c.Categoria).WithMany(x => x.Estatus);
            #endregion
        }
    }
}
