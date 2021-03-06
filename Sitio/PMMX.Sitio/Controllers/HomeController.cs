﻿using Sitio.Helpers;
using PMMX.Modelo.Vistas;
using PMMX.Seguridad.Servicios;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;


namespace Sitio.Controllers
{

    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {

            ViewBag.Title = "Home Page";
            
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            NotificationService notify = new NotificationService();
            UsuarioServicio usuarioServicio = new UsuarioServicio();

            List<DispositivoView> dispositivos = usuarioServicio.GetMecanicosPorOrigen(63);
            List<string> llaves = dispositivos.Select(x => x.Llave).ToList();

            foreach (string notificacion in llaves)
            {
                notify.SendPushNotification(notificacion, "Nuevo defecto reportado en " + 63, "Necesitan de ti D:");
            }

            return View();
        }

        public ActionResult Contact()
        {

            ViewBag.Message = "Your contact page.";
            string folderName = @"c:\FotosPersonales";
            string pathString = System.IO.Path.Combine(folderName, "SubFolder");
            System.IO.Directory.CreateDirectory(pathString);
            string fileName = System.IO.Path.GetRandomFileName();
            pathString = System.IO.Path.Combine(pathString, fileName);
            Console.WriteLine("Path to my file: {0}\n", pathString);
            if (!System.IO.File.Exists(pathString))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(pathString))
                {
                    for (byte i = 0; i < 100; i++)
                    {
                        fs.WriteByte(i);
                    }
                }
            }
            else
            {
                Console.WriteLine("File \"{0}\" already exists.", fileName);

            }
            try
            {
                byte[] readBuffer = System.IO.File.ReadAllBytes(pathString);
                foreach (byte b in readBuffer)
                {
                    Console.Write(b + " ");
                }
                Console.WriteLine();
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine(e.Message);
            }

            return View();

        }
        
        [AllowAnonymous]
        public ActionResult Video()
        {
            ViewBag.Title = "Home Page";

            return View();
        }


        public ActionResult Controles()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        [AllowAnonymous]
        public ActionResult StreamingVideo()
        {


            return new VideoResult();

        }



        public Boolean numeros(List<int> arreglo, int  detectar, int cantidad) {
            Boolean respuesta= false;
            int contador = 0;

            var algo = arreglo.Where(x => x == detectar).Count();


            foreach (var item in arreglo)
            {
                if (item == detectar) {
                    
                    contador++;
                }
            }
            if (contador == cantidad)
            {
                respuesta = true;
            }


            return respuesta;
        }


        public Boolean Palindromo(String palabra)
        {
            Boolean respuesta = false;
            palabra = palabra.Replace(" ", "");
            String palindromo="";


            foreach (var item in palabra)
            {
                palindromo = item + "" + palindromo;   
            }

            if (palindromo == palabra) {
                respuesta = true;
            }


            return respuesta;
        }


        public Boolean ComparacionDeLaSumaNumerosParesEImpares(List<int> arreglo)
        {
            Boolean respuesta = false;
            int sumaPares = 0;
            int sumaInpares=0;

            foreach (var item in arreglo)
            {
                if ((item % 2) == 0)
                {
                    sumaPares = sumaPares + item;
                }
                else {
                    sumaInpares = sumaInpares + item;
                }
            }

            if (sumaPares == sumaInpares)
            {
                respuesta = true;
            }
            return respuesta;
        }
    }
}
