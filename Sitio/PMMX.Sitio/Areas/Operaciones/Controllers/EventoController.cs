﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PMMX.Infraestructura.Contexto;
using PMMX.Modelo.Entidades.Operaciones;
using PMMX.Modelo.Vistas;
using Sitio.Helpers;
using PMMX.Seguridad.Servicios;
using PMMX.Modelo.RespuestaGenerica;
using PMMX.Modelo.Entidades;
using Microsoft.AspNet.Identity;
using PMMX.Modelo.Entidades.Warehouse;
using PMMX.Operaciones.Servicios;
using OfficeOpenXml;
using System.Globalization;

namespace Sitio.Areas.Operaciones.Controllers
{
    [Authorize]
    public class EventoController : Controller
    {
        private PMMXContext db = new PMMXContext();

        // GET: Eventos/Evento
        public ActionResult Index()
        {
            var evento = db.Evento.Include(e => e.Asignador).Include(e => e.Categoria);
            return View(evento.Where(e => e.Activo == true).ToList());
        }

        //[RenderAjaxPartialScripts]
        public ActionResult GetEvents(DateTime date)
        {
            if (ModelState.IsValid)
            {
                var lastDay = DateTime.DaysInMonth(date.Year, date.Month);
                var LastDate = date.AddDays(lastDay);
                PersonaServicio personaServicio = new PersonaServicio();
                IRespuestaServicio<Persona> persona = personaServicio.GetPersona(User.Identity.GetUserId());

                if (persona.EjecucionCorrecta)
                {
                    List<EventoView> events = ListEvents(date);
                    var _puesto = db.Puestos.Where(p => p.Id == persona.Respuesta.IdPuesto).Select(p => p.Nombre).FirstOrDefault();

                    switch (_puesto)
                    {
                        case "Supplier":
                            var listID = db.EventoResponsable
                                .Where(r => (r.IdResponsable == persona.Respuesta.Id) && (r.Evento.FechaInicio >= date && r.Evento.FechaFin <= LastDate))
                                .Select(r => r.IdEvento)
                                .ToList();

                            events = events.Where(e=> (listID.Contains(e.Id))).ToList();

                            return Json(new { events }, JsonRequestBehavior.AllowGet);
                        //case "Developer":
                        //    var listIDs = db.EventoResponsable
                        //        .Where(r => (r.IdResponsable == persona.Respuesta.Id) && (r.Evento.FechaInicio >= date && r.Evento.FechaFin <= LastDate))
                        //        .Select(r => r.IdEvento)
                        //        .ToList();

                        //    events = events.Where(e => (listIDs.Contains(e.Id))).ToList();

                        //    return Json(new { events }, JsonRequestBehavior.AllowGet);
                        default:
                            return Json(new { events }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { status = 200 }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = 400 }, JsonRequestBehavior.AllowGet);
            }
        }

        //[RenderAjaxPartialScripts]
        public ActionResult GetEventsBySubCategoria(int IdSubCategoria, DateTime date)
        {
            if (ModelState.IsValid)
            {
                var lastDay = DateTime.DaysInMonth(date.Year, date.Month);
                var LastDate = date.AddDays(lastDay - 1);

                PersonaServicio personaServicio = new PersonaServicio();
                IRespuestaServicio<Persona> persona = personaServicio.GetPersona(User.Identity.GetUserId());

                if (persona.EjecucionCorrecta)
                {
                    List<EventoView> tmp = ListEvents(date);
                    List<EventoView> events = new List<EventoView>();

                    var _puesto = db.Puestos.Where(p => p.Id == persona.Respuesta.IdPuesto)
                                    .Select(p => p.Nombre)
                                    .FirstOrDefault();

                    switch (_puesto)
                    {
                        case "Supplier":
                            var listR = db.EventoResponsable
                                  .Where(r => (r.IdResponsable == persona.Respuesta.Id) 
                                         && (r.Evento.FechaInicio >= date && r.Evento.FechaFin <= LastDate))
                                  .Select(r => r.IdEvento)
                                  .ToList();

                            if(IdSubCategoria == 1)
                            {
                                events = tmp.Where(e => (listR.Contains(e.Id)) && (e.SubCategoria.Tipo == "Local")).ToList();
                            }
                            else
                            {
                                events = tmp.Where(e => (listR.Contains(e.Id)) && (e.IdSubCategoria == IdSubCategoria)).ToList();
                            }
                            return Json(new { events }, JsonRequestBehavior.AllowGet);
                        default:
                            if(IdSubCategoria == 1)
                            {
                                events = tmp.Where(e => (e.SubCategoria.Tipo == "Local")).ToList();
                            }
                            else
                            {
                                events = tmp.Where(e => (e.IdSubCategoria == IdSubCategoria)).ToList();
                            }
                            return Json(new { events }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { status = 200 }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = 400 }, JsonRequestBehavior.AllowGet);
            }
        }

        //[RenderAjaxPartialScripts]
        public string GetColorStatus(int idEvento)
        {
            var colors = db.StatusVentana.OrderByDescending(s => s.Fecha).Where(s => s.Ventana.IdEvento == idEvento)
                          .Select(s => s.Status.Color)
                          .FirstOrDefault();

            if (colors == null) colors = "#95A5A6";

            return colors;
        }

        //[RenderAjaxPartialScripts]
        public string GetClasificacion(int idEvento)
        {
            var clasificacion = db.Ventana.Where(s => s.IdEvento == idEvento)
                          .Select(s => s.SubCategoria.Nombre + "-" + (s.Evento.FechaInicio.Hour < 12 ? "Mañana" : "Tarde"))
                          .FirstOrDefault();

            if (clasificacion == null) clasificacion = "d";

            return clasificacion;
        }

        public List<EventoView> ListEvents(DateTime date)
        {
            List<EventoView> events             = new List<EventoView>();
            var lastDay                         = DateTime.DaysInMonth(date.Year, date.Month);
            var LastDate                        = date.AddDays(lastDay);

            events = db.Evento
                .Include(e => e.SubCategoria)
                .Where(e => (e.FechaInicio >= date && e.FechaFin <= LastDate) && e.Activo == true)
                .Select(e => new EventoView
                {
                    Id = e.Id,
                    Descripcion = e.Descripcion,
                    FechaInicio = e.FechaInicio,
                    FechaFin = e.FechaFin,
                    Nota = e.Nota,
                    IdSubCategoria = e.IdSubCategoria,
                    SubCategoria = new SubCategoriaView
                    {
                        Tipo =e.SubCategoria.Tipo
                    }
                }).ToList();

             foreach (var item in events)
             {
                item.Color = GetColorStatus(item.Id);
                item.Clasificacion = GetClasificacion(item.Id);
             }

            return events;
        }

        // GET: Eventos/Evento/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var evento = db.Evento
                .Where(e => e.Id == id)
                .Select(e => new EventoView
                {
                    Id = e.Id,
                    Descripcion = e.Descripcion,
                    IdAsignador = e.IdAsignador,
                    IdCategoria = e.IdCategoria,
                    IdSubCategoria = e.IdSubCategoria,
                    FechaInicio = e.FechaInicio,
                    FechaFin = e.FechaFin,
                    Nota = e.Nota,
                    Activo = e.Activo
                }).FirstOrDefault();

            if (evento == null)
            {
                return HttpNotFound();
            }

            ViewBag.NombreAsignador = db.Personas.Where(x => x.Id == evento.IdAsignador).Select(x => x.Nombre + " " + x.Apellido1 + " " + x.Apellido2).FirstOrDefault().ToString();
            ViewBag.IdCategoria = db.Categoria.Where(x => x.Id == evento.IdCategoria).Select(x => x.Nombre).FirstOrDefault().ToString();
            ViewBag.IdSubCategoria = db.SubCategoria.Where(x => x.Id == evento.IdSubCategoria).Select(x => x.Nombre).FirstOrDefault() == null 
                ? " " 
                : db.SubCategoria.Where(x => x.Id == evento.IdSubCategoria).Select(x => x.Nombre).FirstOrDefault().ToString();
            return View(evento);
        }

        public ActionResult GetArea()
        {
            if (ModelState.IsValid)
            {
                var areas = db.Area.Select(w => new { Id = w.Id, NombreCorto = w.NombreCorto }).OrderBy(w => w.Id).ToList();
                return Json(new { areas }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = 400 }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetWorkCenterbyArea(int idArea)
        {
            if (ModelState.IsValid)
            {
                var wlist = db.WorkCenters.Where(w => w.BussinesUnit.IdArea == idArea).Select(w => new { Id = w.Id, NombreCorto = w.NombreCorto }).OrderBy(w => w.Id).ToList();
                return Json(new { wlist }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = 400 }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetModulobyWorkCenter(int idWorkCenter)
        {
            if (ModelState.IsValid)
            {
                var modulo = db.Origens.Where(o => (o.IdWorkCenter == idWorkCenter) && (o.IdModulo != null)).Select(o => new { Id = o.Modulo.Id, NombreCorto = o.Modulo.NombreCorto }).ToList();
                return Json(new { modulo }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = 400 }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetIdOrigen(int idWorkCenter, int idModulo)
        {
            if (ModelState.IsValid)
            {
                var origen = db.Origens.Where(o => (o.IdWorkCenter == idWorkCenter) && (o.IdModulo == idModulo)).Select(o => new { Id = o.Id }).FirstOrDefault();
                return Json(new { origen }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = 400 }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetResponsables()
        {
            if (ModelState.IsValid)
            {
                var responsables = db.Personas.Select(w => new { Id = w.Id, Nombre = w.Nombre + " " + w.Apellido1 + " " + w.Apellido2 }).OrderBy(w => w.Id).ToList();
                return Json(new { responsables }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = 400 }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetListaDistribucion()
        {
            if (ModelState.IsValid)
            {
                var lista = db.SubArea.Where(w=> w.Activo == true).Select(w => new { Id = w.Id, Nombre = w.Nombre }).OrderBy(w => w.Id).ToList();
                return Json(new { lista }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = 400 }, JsonRequestBehavior.AllowGet);
            }
        }

        public PartialViewResult Responsables()
        {
            return PartialView("_Responsables");
        }

        public PartialViewResult Ubicaciones()
        {
            var areas = db.Area.Select(w => new { Id = w.Id, NombreCorto = w.NombreCorto }).OrderBy(w => w.Id).ToList();
            return PartialView("_Origen", new { areas = areas });
        }

        public PartialViewResult ListaDistribucion()
        {
            return PartialView("_ListaDistribucion");
        }

        // GET: Eventos/Evento/Create
        public ActionResult Create()
        {
            ViewBag.IdAsignador = new SelectList(db.Personas.Select(x => new { Id = x.Id, Nombre = x.Nombre + " " + x.Apellido1 + " " + x.Apellido2 }).OrderBy(x => x.Nombre), "Id", "Nombre");
            return View();
        }

        // POST: Eventos/Evento/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Evento evento, int IdOrigen, string IdResponsables)
        {
            if (ModelState.IsValid)
            {
                if (evento.Nota == null)
                    evento.Nota = " ";

                PersonaServicio personaServicio = new PersonaServicio();
                IRespuestaServicio<Persona> persona = personaServicio.GetPersona(User.Identity.GetUserId());

                if (persona.EjecucionCorrecta)
                {
                    evento.IdAsignador = persona.Respuesta.Id;
                    db.Evento.Add(evento);
                    db.SaveChanges();
                }

                if (IdOrigen > 0)
                {
                    EventoOrigen eOrigen = new EventoOrigen();
                    eOrigen.IdEvento = evento.Id;
                    eOrigen.IdOrigen = IdOrigen;
                    db.EventoOrigen.Add(eOrigen);
                    db.SaveChanges();
                }

                if (IdResponsables != null && IdResponsables != "")
                {
                    Char delimiter = ',';
                    String[] substrings = IdResponsables.Split(delimiter);
                    foreach (var substring in substrings)
                    {
                        if (substring != "")
                        {
                            EventoResponsable eResponsable = new EventoResponsable();
                            eResponsable.IdEvento = evento.Id;
                            eResponsable.IdResponsable = int.Parse(substring);
                            db.EventoResponsable.Add(eResponsable);
                            db.SaveChanges();
                        }
                    }
                    SendNotification(evento, "New Event: ");
                }

                return RedirectToAction("Index");
            }
                        
            return View(evento);
        }

        public bool SendNotification(Evento evento, String mensaje)
        {
            NotificationService notify = new NotificationService();
            UsuarioServicio usuarioServicio = new UsuarioServicio();

            List<DispositivoView> dispositivos = usuarioServicio.GetDispositivoByEvento(evento.Id);
            List<string> llaves = dispositivos.Select(x => x.Llave).ToList();

            foreach (string notificacion in llaves)
            {
                notify.SendPushNotification(notificacion, mensaje + evento.Descripcion + ". ", "");
            }

            string senders = usuarioServicio.GetEmailByEvento(evento.Id);
            EmailService emailService = new EmailService();
            emailService.SendMail(senders, evento);

            return true;
        }

        public bool changeStatus(Evento evento)
        {
            Ventana ventana = db.Ventana
                                 .Include(v => v.StatusVentana)
                                 .Include(v => v.StatusVentana.Select(s => s.Status))
                                 .Include(v => v.BitacoraVentana)
                                 .Include(v => v.BitacoraVentana.Select(b => b.Estatus))
                                 .Include(v => v.BitacoraVentana.Select(b => b.Rechazo))
                                 .Include(v => v.Evento)
                                 .Include(v => v.Proveedor)
                                 .SingleOrDefault(x => x.IdEvento == evento.Id);
            if (ventana != null)
            {
                try
                {
                    WorkFlowServicio workflowServicio = new WorkFlowServicio();
                    IRespuestaServicio<WorkFlowView> workFlow = workflowServicio.nextEstatus(ventana.IdSubCategoria, ventana.StatusVentana.OrderByDescending(x => x.Fecha).Select(x => x.IdStatus).FirstOrDefault(), false);

                    StatusVentana statusVentana = new StatusVentana();
                    statusVentana.IdResponsable = evento.IdAsignador;
                    statusVentana.IdStatus = workFlow.Respuesta.EstatusSiguiente.Id;
                    statusVentana.IdVentana = ventana.Id;
                    statusVentana.Fecha = DateTime.Now;
                    statusVentana.Comentarios = "Se reagenda ventana";
                    db.StatusVentana.Add(statusVentana);
                    db.SaveChanges();

                    UsuarioServicio usuarioServicio = new UsuarioServicio();
                    NotificationService notify = new NotificationService();

                    string senders = usuarioServicio.GetEmailByStatus(ventana);
                    EmailService emailService = new EmailService();
                    emailService.SendMail(senders, ventana);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return false;
        }

        // GET: Eventos/Evento/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Evento evento = db.Evento.Find(id);
            if (evento == null)
            {
                return HttpNotFound();
            }

            ViewBag.IdAsignador = new SelectList(db.Personas.Select(x => new { Id = x.Id, Nombre = x.Nombre + " " + x.Apellido1 + " " + x.Apellido2 }).OrderBy(x => x.Nombre), "Id", "Nombre", evento.IdAsignador);
            ViewBag.IdCategoria = new SelectList(db.Categoria.Select(x => new { Id = x.Id, Nombre = x.Nombre }).OrderBy(x => x.Nombre), "Id", "Nombre", evento.IdCategoria);
            ViewBag.IdSubCategoria = new SelectList(db.SubCategoria.Where(x => x.IdCategoria == evento.IdCategoria).Select(x => new { Id = x.Id, Nombre = x.Nombre }).OrderBy(x => x.Nombre), "Id", "Nombre", evento.IdSubCategoria);
            return View(evento);
        }

        // POST: Eventos/Evento/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Evento evento, int IdListaDistribucion)
        {
            if (ModelState.IsValid)
            {
                evento.Activo = true;
                db.Entry(evento).State = EntityState.Modified;
                changeStatus(evento);
                db.SaveChanges();

                //Modify IdSubcategoria inside Ventana object
                var ventana = db.Ventana.Where(v => v.IdEvento == evento.Id).FirstOrDefault();

                if (ventana != null)
                {
                    ventana.IdSubCategoria = evento.IdSubCategoria;
                    db.Entry(ventana).State = EntityState.Modified;
                    db.SaveChanges();
                }

                if (IdListaDistribucion != 0)
                {
                    // Remove the old asignation
                    var responsables = db.EventoResponsable
                                       .Where(d => d.IdEvento == evento.Id)
                                       .ToList();

                    foreach (var responsable in responsables)
                    {
                        db.EventoResponsable.Remove(responsable);
                    }

                    //Add new asignation
                    var lista = db.ListaDistribucion
                                  .Where(x => x.Id == IdListaDistribucion)
                                  .Select(x => x.IdPersona)
                                  .ToList();

                    foreach(var item in lista)
                    {
                        EventoResponsable eResponsable = new EventoResponsable();
                        eResponsable.IdEvento = evento.Id;
                        eResponsable.IdResponsable = item;
                        db.EventoResponsable.Add(eResponsable);
                        db.SaveChanges();
                    }
                }

                return RedirectToAction("Index");
            }

            ViewBag.IdAsignador = new SelectList(db.Personas.Select(x => new { Id = x.Id, Nombre = x.Nombre + " " + x.Apellido1 + " " + x.Apellido2 }).OrderBy(x => x.Nombre), "Id", "Nombre");
            ViewBag.IdCategoria = new SelectList(db.Categoria.Select(x => new { Id = x.Id, Nombre = x.Nombre }).OrderBy(x => x.Nombre), "Id", "Nombre", evento.IdCategoria);
            ViewBag.IdSubCategoria = new SelectList(db.SubCategoria.Select(x => new { Id = x.Id, Nombre = x.Nombre }).OrderBy(x => x.Nombre), "Id", "Nombre", evento.IdSubCategoria);
            return View(evento);
        }

        // GET: Eventos/Evento/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Evento evento = db.Evento.Find(id);
            if (evento == null)
            {
                return HttpNotFound();
            }

            ViewBag.NombreAsignador = db.Personas.Where(x => x.Id == evento.IdAsignador).Select(x => x.Nombre + " " + x.Apellido1 + " " + x.Apellido2).FirstOrDefault().ToString();
            ViewBag.IdCategoria = db.Categoria.Where(x => x.Id == evento.IdCategoria).Select(x => x.Nombre).FirstOrDefault().ToString();
            return View(evento);
        }

        // POST: Eventos/Evento/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Evento evento = db.Evento.Find(id);
            //db.Evento.Remove(evento);
            evento.Activo = false;
            db.SaveChanges();

            SendNotification(evento,"The event has been deleted: ");

            return RedirectToAction("Index");
        }

        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                string fileName = file.FileName;
                string fileContentType = file.ContentType;
                byte[] fileBytes = new byte[file.ContentLength];
                var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                using (var package = new ExcelPackage(file.InputStream))
                {
                    PersonaServicio personaServicio = new PersonaServicio();
                    IRespuestaServicio<Persona> persona = personaServicio.GetPersona(User.Identity.GetUserId());

                    var currentSheet = package.Workbook.Worksheets;

                    foreach (var workSheet in currentSheet)
                    {
                        var noOfCol = workSheet.Dimension.End.Column;
                        var noOfRow = workSheet.Dimension.End.Row;
                        
                        for(var row = 2; row <= noOfRow; row++)
                        {
                            if (workSheet.Cells[row, 1].Value != null)
                            {
                                var numSubCategoria = workSheet.Cells[row, 3].Value.ToString().Trim();
                                var fecha = DateTime.ParseExact(workSheet.Cells[row, 2].Value.ToString().Trim(), "dd/MM/yyyy hh:mm", null);
                                Evento evento = new Evento();

                                evento.Descripcion = workSheet.Cells[row, 1].Value == null ? string.Empty : workSheet.Cells[row, 1].Value.ToString().Trim();
                                evento.FechaInicio = workSheet.Cells[row, 2].Value == null ? DateTime.Now : fecha;
                                evento.FechaFin = evento.FechaFin.AddHours(1);
                                evento.IdAsignador = persona.Respuesta.Id;
                                evento.IdCategoria = db.Categoria.Where(c => c.NombreCorto == "Ventana").Select(c => c.Id).FirstOrDefault();
                                evento.IdSubCategoria = db.SubCategoria.Where(c => c.NombreCorto == numSubCategoria).Select(c => c.Id).FirstOrDefault() == 0
                                    ? db.Carrier.Where(c => c.NombreCorto == "DIMS Nacional").Select(c => c.Id).FirstOrDefault()
                                    : db.SubCategoria.Where(c => c.NombreCorto == numSubCategoria).Select(c => c.Id).FirstOrDefault();
                                evento.Nota = " ";
                                evento.Activo = true;

                                db.Evento.Add(evento);
                                db.SaveChanges();

                                var proveedor = workSheet.Cells[row, 4].Value.ToString().Trim();
                                var idSubArea = db.SubArea.Where(f=> f.NombreCorto == proveedor ).Select(f=> f.Id).FirstOrDefault();
                                var listaDistribucion = db.ListaDistribucion.Where(w => (w.IdSubarea == idSubArea)).Select(w => new { IdPersona = w.IdPersona }).ToList();

                                if (listaDistribucion.Count > 0)
                                {
                                    foreach (var item in listaDistribucion)
                                    {
                                        EventoResponsable eResponsable = new EventoResponsable();
                                        eResponsable.IdEvento = evento.Id;
                                        eResponsable.IdResponsable = item.IdPersona;
                                        db.EventoResponsable.Add(eResponsable);
                                        db.SaveChanges();
                                    }
                                    SendNotification(evento, "New Event: ");
                                }
                            }
                        }
                        break;
                    }
                    return RedirectToAction("Index", "Evento", new { Area = "Operaciones" });
                }
            }
            return View("Index");
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
