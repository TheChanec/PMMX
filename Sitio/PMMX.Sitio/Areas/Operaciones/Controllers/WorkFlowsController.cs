﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PMMX.Infraestructura.Contexto;
using PMMX.Modelo.Entidades;

namespace Sitio.Areas.Operaciones
{
    public class WorkFlowsController : Controller
    {
        private PMMXContext db = new PMMXContext();

        // GET: Operaciones/WorkFlows
        public ActionResult Index()
        {
            var workFlows = db.WorkFlows.Include(w => w.Categoria).Include(w => w.EstatusAnterior).Include(w => w.EstatusCancelado).Include(w => w.EstatusInicial).Include(w => w.EstatusSiguiente);
            return View(workFlows.ToList());
        }

        // GET: Operaciones/WorkFlows/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WorkFlow workFlow = db.WorkFlows.Find(id);
            if (workFlow == null)
            {
                return HttpNotFound();
            }
            return View(workFlow);
        }

        // GET: Operaciones/WorkFlows/Create
        public ActionResult Create()
        {
            ViewBag.IdCategoria = new SelectList(db.Categoria, "Id", "Nombre");
            ViewBag.Anterior = new SelectList(db.Estatus, "Id", "Nombre");
            ViewBag.Cancelado = new SelectList(db.Estatus, "Id", "Nombre");
            ViewBag.Inicial = new SelectList(db.Estatus, "Id", "Nombre");
            ViewBag.Siguiente = new SelectList(db.Estatus, "Id", "Nombre");
            return View();
        }

        // POST: Operaciones/WorkFlows/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,IdCategoria,Inicial,Anterior,Siguiente,Cancelado,Activo")] WorkFlow workFlow)
        {
            if (ModelState.IsValid)
            {
                db.WorkFlows.Add(workFlow);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdCategoria = new SelectList(db.Categoria, "Id", "Nombre", workFlow.IdCategoria);
            ViewBag.Anterior = new SelectList(db.Estatus, "Id", "Nombre", workFlow.Anterior);
            ViewBag.Cancelado = new SelectList(db.Estatus, "Id", "Nombre", workFlow.Cancelado);
            ViewBag.Inicial = new SelectList(db.Estatus, "Id", "Nombre", workFlow.Inicial);
            ViewBag.Siguiente = new SelectList(db.Estatus, "Id", "Nombre", workFlow.Siguiente);
            return View(workFlow);
        }

        // GET: Operaciones/WorkFlows/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WorkFlow workFlow = db.WorkFlows.Find(id);
            if (workFlow == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdCategoria = new SelectList(db.Categoria, "Id", "Nombre", workFlow.IdCategoria);
            ViewBag.Anterior = new SelectList(db.Estatus, "Id", "Nombre", workFlow.Anterior);
            ViewBag.Cancelado = new SelectList(db.Estatus, "Id", "Nombre", workFlow.Cancelado);
            ViewBag.Inicial = new SelectList(db.Estatus, "Id", "Nombre", workFlow.Inicial);
            ViewBag.Siguiente = new SelectList(db.Estatus, "Id", "Nombre", workFlow.Siguiente);
            return View(workFlow);
        }

        // POST: Operaciones/WorkFlows/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,IdCategoria,Inicial,Anterior,Siguiente,Cancelado,Activo")] WorkFlow workFlow)
        {
            if (ModelState.IsValid)
            {
                db.Entry(workFlow).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdCategoria = new SelectList(db.Categoria, "Id", "Nombre", workFlow.IdCategoria);
            ViewBag.Anterior = new SelectList(db.Estatus, "Id", "Nombre", workFlow.Anterior);
            ViewBag.Cancelado = new SelectList(db.Estatus, "Id", "Nombre", workFlow.Cancelado);
            ViewBag.Inicial = new SelectList(db.Estatus, "Id", "Nombre", workFlow.Inicial);
            ViewBag.Siguiente = new SelectList(db.Estatus, "Id", "Nombre", workFlow.Siguiente);
            return View(workFlow);
        }

        // GET: Operaciones/WorkFlows/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WorkFlow workFlow = db.WorkFlows.Find(id);
            if (workFlow == null)
            {
                return HttpNotFound();
            }
            return View(workFlow);
        }

        // POST: Operaciones/WorkFlows/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            WorkFlow workFlow = db.WorkFlows.Find(id);
            db.WorkFlows.Remove(workFlow);
            db.SaveChanges();
            return RedirectToAction("Index");
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