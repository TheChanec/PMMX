﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PMMX.Infraestructura.Contexto;
using PMMX.Modelo.Entidades;
using PMMX.Modelo.Entidades.SeguridadFisica;
using PMMX.Modelo.RespuestaGenerica;
using PMMX.Seguridad.Servicios;

namespace Sitio.Areas.SeguridadFisica.Controllers
{
    public class RegistroUnidadController : Controller
    {
        private PMMXContext db = new PMMXContext();

        // GET: SeguridadFisica/RegistroUnidad
        public ActionResult Index(string Codigo)
        {
            var registroUnidad = db.RegistroUnidad
                .OrderByDescending(r=> r.Id)
                .Where(r=> r.Formato.Codigo == Codigo)
                .Include(r => r.Formato)
                .Include(r => r.Datos)
                .Include(r => r.Bitacora)
                .Include(r => r.Bitacora.Select(b => b.Persona));

            ViewBag.CodigoFormato = Codigo;
            return View(registroUnidad.ToList());
        }

        // GET: SeguridadFisica/RegistroUnidad/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            RegistroUnidad registroUnidad = db.RegistroUnidad
                .Where(v=> v.Id == id)
                .Include(v=> v.Formato)
                .Include(v => v.Datos)
                .Include(v => v.Bitacora)
                .Include(r => r.Bitacora.Select(b => b.Persona))
                .FirstOrDefault();

            if (registroUnidad == null)
            {
                return HttpNotFound();
            }
            return View(registroUnidad);
        }

        // GET: SeguridadFisica/RegistroUnidad/Create
        public ActionResult Create(string Codigo)
        {
            if(Codigo != null)
            {
                ViewBag.IdFormato = new SelectList(db.Formato
                    .Where(x=> x.Codigo == Codigo)
                    .Select(x => new { Id = x.Id, NombreCorto = x.Descripcion })
                    .OrderBy(x => x.NombreCorto), "Id", "NombreCorto");
            }
            else
            {
                ViewBag.IdFormato = new SelectList(db.Formato
                    .Select(x => new { Id = x.Id, NombreCorto = x.Descripcion })
                    .OrderBy(x => x.NombreCorto), "Id", "NombreCorto");
            }
            ViewBag.CodigoFormato = Codigo;
            return View();
        }

        // POST: SeguridadFisica/RegistroUnidad/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RegistroUnidad registroUnidad, DatosUnidad datos, DateTime date, string NombreGuardia, string Puerta)
        {
            if (ModelState.IsValid)
            {
                db.RegistroUnidad.Add(registroUnidad);
                db.SaveChanges();

                datos.PlacasTractor = datos.PlacasTractor == null ? " " : datos.PlacasTractor;
                datos.NoEcoTractor = datos.NoEcoTractor == null ? " " : datos.NoEcoTractor;
                datos.PlacasRemolque = datos.PlacasRemolque == null ? " " : datos.PlacasRemolque;
                datos.NoEcoRemolque = datos.NoEcoRemolque == null ? " " : datos.NoEcoRemolque;
                datos.TipoRemolque = datos.TipoRemolque == null ? " " : datos.TipoRemolque;
                datos.NoCaja = datos.NoCaja == null ? "" : datos.NoCaja;
                datos.IdRegistroUnidad = registroUnidad.Id;
                db.DatosUnidad.Add(datos);
                
                PersonaServicio personaServicio = new PersonaServicio();
                IRespuestaServicio<Persona> persona = personaServicio.GetPersona(User.Identity.GetUserId());

                if (persona.EjecucionCorrecta)
                {
                    BitacoraUnidad bitacora = new BitacoraUnidad();
                    bitacora.IdPersona = persona.Respuesta.Id;
                    bitacora.NombreGuardia = NombreGuardia;
                    bitacora.Puerta = Puerta == null 
                        ? db.Formato.Where(f => f.Id == registroUnidad.IdFormato).Select(f=> f.Puerta).FirstOrDefault() 
                        : Puerta;
                    bitacora.Fecha = date;
                    bitacora.IdRegistroUnidad = registroUnidad.Id;
                    bitacora.TipoMovimiento = "Entrada";
                    db.BitacoraUnidad.Add(bitacora);
                }

                db.SaveChanges();
                return RedirectToAction("Index", new { Codigo = db.Formato.Where(f => f.Id == registroUnidad.IdFormato).Select(f => f.Codigo).FirstOrDefault() });
            }

            ViewBag.IdFormato = new SelectList(db.Formato, "Id", "Codigo", registroUnidad.IdFormato);
            return View(registroUnidad);
        }

        // GET: SeguridadFisica/RegistroUnidad/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            RegistroUnidad registroUnidad = db.RegistroUnidad
                .Where(v => v.Id == id)
                .Include(v => v.Formato)
                .Include(v => v.Datos)
                .Include(v => v.Bitacora)
                .Include(r => r.Bitacora.Select(b => b.Persona))
                .FirstOrDefault();

            if (registroUnidad == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdFormato = new SelectList(db.Formato, "Id", "Codigo", registroUnidad.IdFormato);
            return View(registroUnidad);
        }

        // POST: SeguridadFisica/RegistroUnidad/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RegistroUnidad registroUnidad, DatosUnidad datos, DateTime date, string NombreGuardia, string Puerta)
        {
            if (ModelState.IsValid)
            {
                db.Entry(registroUnidad).State = EntityState.Modified;
                db.SaveChanges();

                // Editar otra Entidad
                /*var _datos = db.DatosUnidad.Where(d => d.IdRegistroUnidad == registroUnidad.Id).FirstOrDefault();
                _datos.NombreConductor = datos.NombreConductor;
                _datos.PlacasTractor = datos.PlacasTractor;
                _datos.NoEcoTractor = datos.NoEcoTractor;
                _datos.NoCaja = datos.NoCaja;
                _datos.TipoRemolque = datos.TipoRemolque;
                _datos.PlacasRemolque = datos.PlacasRemolque;
                _datos.NoEcoRemolque = datos.NoEcoRemolque;
                db.Entry(_datos).State = EntityState.Modified;
                db.SaveChanges();*/

                datos.PlacasTractor = datos.PlacasTractor == null ? " " : datos.PlacasTractor;
                datos.NoEcoTractor = datos.NoEcoTractor == null ? " " : datos.NoEcoTractor;
                datos.PlacasRemolque = datos.PlacasRemolque == null ? " " : datos.PlacasRemolque;
                datos.NoEcoRemolque = datos.NoEcoRemolque == null ? " " : datos.NoEcoRemolque;
                datos.TipoRemolque = datos.TipoRemolque == null ? " " : datos.TipoRemolque;
                datos.NoCaja = datos.NoCaja == null ? "" : datos.NoCaja;
                datos.IdRegistroUnidad = registroUnidad.Id;
                db.DatosUnidad.Add(datos);
                db.SaveChanges();

                PersonaServicio personaServicio = new PersonaServicio();
                IRespuestaServicio<Persona> persona = personaServicio.GetPersona(User.Identity.GetUserId());

                if (persona.EjecucionCorrecta)
                {
                    BitacoraUnidad bitacora = new BitacoraUnidad();
                    bitacora.IdPersona = persona.Respuesta.Id;
                    bitacora.NombreGuardia = NombreGuardia;
                    bitacora.Puerta = Puerta == null
                        ? db.Formato.Where(f => f.Id == registroUnidad.IdFormato).Select(f => f.Puerta).FirstOrDefault()
                        : Puerta;
                    bitacora.Fecha = date;
                    bitacora.IdRegistroUnidad = registroUnidad.Id;
                    bitacora.TipoMovimiento = "Salida";
                    db.BitacoraUnidad.Add(bitacora);
                    db.SaveChanges();
                }

                return RedirectToAction("Index", new { Codigo = db.Formato.Where(f=> f.Id == registroUnidad.IdFormato).Select(f=> f.Codigo).FirstOrDefault() });
            }
            ViewBag.IdFormato = new SelectList(db.Formato, "Id", "Codigo", registroUnidad.IdFormato);
            return View(registroUnidad);
        }

        // GET: SeguridadFisica/RegistroUnidad/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            RegistroUnidad registroUnidad = db.RegistroUnidad
                .Where(v => v.Id == id)
                .Include(v => v.Formato)
                .Include(v => v.Datos)
                .Include(v => v.Bitacora)
                .Include(r => r.Bitacora.Select(b => b.Persona))
                .FirstOrDefault();

            if (registroUnidad == null)
            {
                return HttpNotFound();
            }
            return View(registroUnidad);
        }

        // POST: SeguridadFisica/RegistroUnidad/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            RegistroUnidad registroUnidad = db.RegistroUnidad.Find(id);
            db.RegistroUnidad.Remove(registroUnidad);

            List<DatosUnidad> datos = db.DatosUnidad.Where(d => d.IdRegistroUnidad == id).ToList();
            List<BitacoraUnidad> bitacoras = db.BitacoraUnidad.Where(d => d.IdRegistroUnidad == id).ToList();

            foreach(var dato in datos)
            {
                db.DatosUnidad.Remove(dato);
            }

            foreach (var bitacora in bitacoras)
            {
                db.BitacoraUnidad.Remove(bitacora);
            }

            db.SaveChanges();
            return RedirectToAction("Index", new { Codigo = db.Formato.Where(f => f.Id == registroUnidad.IdFormato).Select(f => f.Codigo).FirstOrDefault() });
        }

        public ActionResult downloadReport(DateTime Inicio, DateTime Fin, string Codigo)
        {
            List<RegistroUnidad> registros = db.RegistroUnidad
                            .Include(v => v.Formato)
                            .Include(v => v.Bitacora)
                            .Include(v => v.Bitacora.Select(s => s.Persona))
                            .Include(v => v.Datos)
                            .Where(v => v.Formato.Codigo.Equals(Codigo))
                            .ToList();

            var fileName = "Bitacora_" + DateTime.Now.ToString("yyyy-MM-dd--hh-mm-ss") + ".xlsx";

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == "");
                worksheet = package.Workbook.Worksheets.Add(Codigo);
                worksheet.Row(1).Height = 20;

                worksheet.TabColor = Color.Gold;
                worksheet.DefaultRowHeight = 12;
                worksheet.Row(1).Height = 20;
                //Header
                worksheet.Cells[4, 1].Value = "Nombre";
                worksheet.Cells[4, 2].Value = "Empresa";
                worksheet.Cells[4, 3].Value = "Placas Tractor";
                worksheet.Cells[4, 4].Value = "#Economico Tractor";
                worksheet.Cells[4, 5].Value = "Placas Remolque";
                worksheet.Cells[4, 6].Value = "#Economico Remolque";
                worksheet.Cells[4, 7].Value = "Asunto";
                worksheet.Cells[4, 8].Value = "Nombre Autoriza";
                worksheet.Cells[4, 9].Value = "Nombre Guardia";
                worksheet.Cells[4, 10].Value = "#Gafette";
                worksheet.Cells[4, 11].Value = "Hora Entrada";
                worksheet.Cells[4, 12].Value = "Hora Salida";

                var fila = 5;

                foreach (var registro in registros)
                {
                    //Header
                    if(worksheet.Cells[1, 1].Value == null)
                    {
                        worksheet.Cells[1, 1].Value = registro.Formato.Descripcion.ToUpper();
                        worksheet.Cells["A1:I1"].Merge = true;
                        worksheet.Cells["A1:I1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[2, 1].Value = "Codigo del Formato: ";
                        worksheet.Cells[2, 2].Value =  registro.Formato.Codigo.ToUpper();
                        worksheet.Cells[2, 3].Value = "Fecha de Efectividad: ";
                        worksheet.Cells[2, 4].Value =  registro.Formato.FechaEfectividad.ToString();
                        worksheet.Cells[2, 5].Value = "Tiempo de Retención: ";
                        worksheet.Cells[2, 6].Value =  registro.Formato.TiempoRetencion.ToString() + " Año";
                        worksheet.Cells[2, 7].Value = "Versión: " ;
                        worksheet.Cells[2, 8].Value =  registro.Formato.Version.ToString();
                        worksheet.Cells[2, 9].Value = "Pagina: 1";
                    }
                    
                    //Content
                    worksheet.Cells[fila, 1].Value = registro.Datos.Select(d => d.NombreConductor).LastOrDefault().ToUpper();
                    worksheet.Cells[fila, 2].Value = registro.Empresa.ToUpper();
                    worksheet.Cells[fila, 3].Value = registro.Datos.Select(d => d.PlacasTractor).LastOrDefault().ToUpper();
                    worksheet.Cells[fila, 4].Value = registro.Datos.Select(d => d.NoEcoTractor).LastOrDefault().ToUpper();
                    worksheet.Cells[fila, 5].Value = registro.Datos.Select(d => d.PlacasRemolque).LastOrDefault().ToUpper();
                    worksheet.Cells[fila, 6].Value = registro.Datos.Select(d => d.NoEcoRemolque).LastOrDefault().ToUpper();
                    worksheet.Cells[fila, 7].Value = registro.Asunto.ToUpper();
                    worksheet.Cells[fila, 8].Value = registro.NombreAutoriza.ToUpper();
                    worksheet.Cells[fila, 9].Value = registro.Bitacora.OrderByDescending(b => b.Fecha).Select(d => d.NombreGuardia).FirstOrDefault().ToString();
                    worksheet.Cells[fila, 10].Value = registro.NoGafette;
                    worksheet.Cells[fila, 11].Value = registro.Bitacora.OrderByDescending(b=> b.Fecha).Select(d => d.Fecha).LastOrDefault().ToString();
                    worksheet.Cells[fila, 12].Value = registro.Bitacora.OrderByDescending(b => b.Fecha).Select(d => d.Fecha).FirstOrDefault().ToString();
                    fila++;
                }

                worksheet.Cells.AutoFitColumns();

                for (var i = 0; i < 12; i++)
                {
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MidnightBlue);

                    worksheet.Cells[2, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[2, i + 1].Style.Font.Color.SetColor(Color.White);
                    worksheet.Cells[2, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MidnightBlue);

                    worksheet.Cells[4, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[4, i + 1].Style.Font.Color.SetColor(Color.Black);
                    worksheet.Cells[4, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                package.Workbook.Properties.Title = "Bitacora_";
                this.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                this.Response.AddHeader("content-disposition", string.Format("attachment;  filename={0}", fileName));
                this.Response.BinaryWrite(package.GetAsByteArray());
                this.Response.Flush();
                this.Response.Close();
                this.Response.End();
            }

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
