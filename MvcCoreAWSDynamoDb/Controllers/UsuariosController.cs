using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MvcCoreAWSDynamoDb.Models;
using MvcCoreAWSDynamoDb.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MvcCoreAWSDynamoDb.Controllers
{
    public class UsuariosController : Controller
    {
        ServiceAWSDynamoDb ServiceDynamo;
        public ServiceAWSS3 serviceS3;

        public UsuariosController(ServiceAWSDynamoDb service, ServiceAWSS3 services3)
        {
            this.ServiceDynamo = service;
            this.serviceS3 = services3;
        }

        public async Task<IActionResult> Index()
        {
            return View(await this.ServiceDynamo.GetUsuarios());
        }

        public async Task<IActionResult> Details(int id)
        {
            return View(await this.ServiceDynamo.FindUsuario(id));
        }
        public async Task<IActionResult> FileAWS(string filename)
        {
            Stream stream = await this.serviceS3.GetFileAsync(filename);
            return File(stream, "image/jpg");
        }
        public async Task<IActionResult> Delete(int id)
        {
            Usuario usuario = await this.ServiceDynamo.GetUsuarioId(id);
            await this.ServiceDynamo.DeleteUsuario(id);
            if (usuario.Fotos != null && usuario.Fotos.Count != 0)
            {
                foreach (Fotos imagen in usuario.Fotos)
                {
                    await this.serviceS3.DeleteFileAsync(imagen.Imagen);
                }

            }
            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Usuario usuario
            , String incluirfotos, String titulo
            , List<IFormFile> files)
        {
            if (incluirfotos != null)
            {
                usuario.Fotos = new List<Fotos>();
                foreach (IFormFile file in files)
                {
                    Fotos fotos = new Fotos();

                    fotos.Titulo = titulo;

                    using (MemoryStream memory = new MemoryStream())
                    {
                        fotos.Imagen = file.FileName;
                        file.CopyTo(memory);
                        await this.serviceS3.UploadFileAsync(memory, file.FileName);
                    }

                    usuario.Fotos.Add(fotos);

                }

            }
            await this.ServiceDynamo.CreateUsuario(usuario);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Update(int iduser)
        {
            Usuario usuario = await this.ServiceDynamo.GetUsuarioId(iduser);
            List<Fotos> fotos = usuario.Fotos;
            ViewData["FOTO"] = fotos;
            return View(usuario);
        }
        [HttpPost]
        public async Task<IActionResult> Update(Usuario user, String accion, String titulo, List<IFormFile> files, List<String> fotoseliminar)
        {
            Usuario usuario = await this.ServiceDynamo.GetUsuarioId(user.IdUsuario);
            if (accion == "delete")
            {

                if (usuario.Fotos != null && usuario.Fotos.Count != 0)
                {
                    foreach (String fotodelete in fotoseliminar)
                    {
                        await this.serviceS3.DeleteFileAsync(fotodelete);
                    }

                    foreach (Fotos foto in usuario.Fotos)
                    {
                        int post = 0;
                        foreach (String fotosdelete in fotoseliminar)
                        {
                            if (foto.Imagen == fotosdelete)
                            {
                                post = usuario.Fotos.IndexOf(foto);
                                usuario.Fotos.RemoveAt(post);
                            }
                        }
                    }
                }


            }

            if (files != null)
            {
                user.Fotos = new List<Fotos>();
                user.Fotos = usuario.Fotos;

                foreach (IFormFile file in files)
                {
                    Fotos fotosnew = new Fotos();

                    fotosnew.Titulo = titulo;

                    using (MemoryStream m = new MemoryStream())
                    {
                        fotosnew.Imagen = file.FileName;
                        file.CopyTo(m);
                        await this.serviceS3.UploadFileAsync(m, file.FileName);
                    }
                    user.Fotos.Add(fotosnew);
                }
            }

            await this.ServiceDynamo.CreateUsuario(user);
            return RedirectToAction("Index");
        }
    }
}
