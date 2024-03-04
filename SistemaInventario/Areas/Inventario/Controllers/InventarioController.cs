﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaInventario.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventario.Modelos;
using SistemaInventario.Modelos.ViewModels;
using SistemaInventario.Utilidades;
using System.Security.Claims;

namespace SistemaInventario.Areas.Inventario.Controllers
{
    [Area("Inventario")]
    [Authorize(Roles = DS.Role_Admin + "," + DS.Role_Inventario)]
    public class InventarioController : Controller
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        [BindProperty]
        private InventarioVM inventarioVM { get; set; }
        public InventarioController(IUnidadTrabajo unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult NuevoInventario()
        {
            inventarioVM = new InventarioVM()
            {
                Inventario = new Modelos.Inventario(),
                BodegaLista = _unidadTrabajo.Inventario.ObtenerTodosDropdownLista("Bodega")
            };
            inventarioVM.Inventario.Estado = false;

            //obtener id del ususaruio desde la sesion
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            inventarioVM.Inventario.UsuarioAplicacionId = claim.Value;
            inventarioVM.Inventario.FechaInicial = DateTime.Now;
            inventarioVM.Inventario.FechaFinal = DateTime.Now;

            return View(inventarioVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NuevoInventario(InventarioVM inventarioVM)
        {
            if (ModelState.IsValid)
            {
                inventarioVM.Inventario.FechaInicial = DateTime.Now;
                inventarioVM.Inventario.FechaFinal = DateTime.Now;
                await _unidadTrabajo.Inventario.Agregar(inventarioVM.Inventario);
                await _unidadTrabajo.Guardar();
                return RedirectToAction("DetalleInventario", new { id = inventarioVM.Inventario.Id });
            }
            inventarioVM.BodegaLista = _unidadTrabajo.Inventario.ObtenerTodosDropdownLista("Bodega");
            return View(inventarioVM);
        }

        public async Task<IActionResult> DetalleInventario(int id)
        {
            inventarioVM = new InventarioVM();
            inventarioVM.Inventario = await _unidadTrabajo.Inventario.ObtenerPrimero(x => x.Id == id, incluirPropiedades: "Bodega");
            inventarioVM.InventarioDetalles = await _unidadTrabajo.InventarioDetalle.ObtenerTodos(d => d.InventarioId == id, incluirPropiedades: "Producto,Producto.Marca");
            return View(inventarioVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DetalleInventario(int inventarioId, int productoId, int cantidadId)
        {
            inventarioVM = new InventarioVM();
            inventarioVM.Inventario = await _unidadTrabajo.Inventario.ObtenerPrimero(x => x.Id == inventarioId);
            var bodegaProducto = await _unidadTrabajo.BodegaProducto.ObtenerPrimero(b => b.ProductoId == productoId &&
                                                                                        b.BodegaId == inventarioVM.Inventario.BodegaId);
            var detalle = await _unidadTrabajo.InventarioDetalle.ObtenerPrimero(x => x.InventarioId == inventarioId &&
                                                                                    x.ProductoId == productoId);
            if (detalle == null)
            {
                inventarioVM.InventarioDetalle = new InventarioDetalle();
                inventarioVM.InventarioDetalle.ProductoId = productoId;
                inventarioVM.InventarioDetalle.InventarioId = inventarioId;
                if (bodegaProducto != null)
                {
                    inventarioVM.InventarioDetalle.StockAnterior = bodegaProducto.Cantidad;
                }
                else
                {
                    inventarioVM.InventarioDetalle.StockAnterior = 0;
                }
                inventarioVM.InventarioDetalle.Cantidad = cantidadId;
                await _unidadTrabajo.InventarioDetalle.Agregar(inventarioVM.InventarioDetalle);
                await _unidadTrabajo.Guardar();
            }
            else
            {
                detalle.Cantidad += cantidadId;
                await _unidadTrabajo.Guardar();
            }
            return RedirectToAction("DetalleInventario", new { id = inventarioId });
        }

        public async Task<IActionResult> Mas(int id) //id de detall
        {
            inventarioVM = new InventarioVM();
            var detalle = await _unidadTrabajo.InventarioDetalle.Obtener(id);
            inventarioVM.Inventario = await _unidadTrabajo.Inventario.Obtener(detalle.InventarioId);
            detalle.Cantidad += 1;
            await _unidadTrabajo.Guardar();
            return RedirectToAction("DetalleInventario", new { id = inventarioVM.Inventario.Id });
        }

        public async Task<IActionResult> Menos(int id) //id de detall
        {
            inventarioVM = new InventarioVM();
            var detalle = await _unidadTrabajo.InventarioDetalle.Obtener(id);
            inventarioVM.Inventario = await _unidadTrabajo.Inventario.Obtener(detalle.InventarioId);
            if (detalle.Cantidad == 1)
            {
                _unidadTrabajo.InventarioDetalle.Remover(detalle);
                await _unidadTrabajo.Guardar();
            }
            else
            {
                detalle.Cantidad -= 1;
                await _unidadTrabajo.Guardar();
            }
            return RedirectToAction("DetalleInventario", new { id = inventarioVM.Inventario.Id });
        }

        public async Task<IActionResult> GenerarStock(int id)
        {
            var inventario = await _unidadTrabajo.Inventario.Obtener(id);
            var detalleLista = await _unidadTrabajo.InventarioDetalle.ObtenerTodos(d => d.InventarioId == id);

            //obtener id del ususaruio desde la sesion
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            foreach (var item in detalleLista)
            {
                var bodegaProducto = new BodegaProducto();
                bodegaProducto = await _unidadTrabajo.BodegaProducto.ObtenerPrimero(b => b.ProductoId == item.ProductoId &&
                                                                                         b.BodegaId == inventario.BodegaId);
                if (bodegaProducto != null) // registro existe
                {
                    await _unidadTrabajo.KardexInventario.RegistrarKardex(bodegaProducto.Id, "Entrada", "Registro de Inventario",
                                                                          bodegaProducto.Cantidad, item.Cantidad, claim.Value);
                    bodegaProducto.Cantidad += item.Cantidad;
                    await _unidadTrabajo.Guardar();
                }
                else // registro nuevo
                {
                    bodegaProducto = new BodegaProducto();
                    bodegaProducto.BodegaId = inventario.BodegaId;
                    bodegaProducto.ProductoId = item.ProductoId;
                    bodegaProducto.Cantidad = item.Cantidad;
                    await _unidadTrabajo.BodegaProducto.Agregar(bodegaProducto);
                    await _unidadTrabajo.Guardar();
                    await _unidadTrabajo.KardexInventario.RegistrarKardex(bodegaProducto.Id, "Entrada", "Inventario inicial",
                                                                          0, item.Cantidad, claim.Value);
                }
            }
            inventario.Estado = true;
            inventario.FechaFinal = DateTime.Now;
            await _unidadTrabajo.Guardar();
            TempData[DS.Exitosa] = "Stock Generado correctamente";
            return RedirectToAction("Index");
        }

        public IActionResult KardexProducto()
        {
            return View();
        }

        [HttpPost]
        public IActionResult KardexProducto(string fechaInicioId, string fechaFinalId, int productoId)
        {
            return RedirectToAction("kardexProductoResultado", new { fechaInicioId, fechaFinalId, productoId });
        }

        public async Task<IActionResult> KardexProductoResultado(string fechaInicioId, string fechaFinalId, int productoId)
        {
            KardexInventarioVM kardexInventarioVM = new KardexInventarioVM();
            kardexInventarioVM.Producto = new Producto();
            kardexInventarioVM.Producto = await _unidadTrabajo.Producto.Obtener(productoId);

            kardexInventarioVM.FechaInicio = DateTime.Parse(fechaInicioId);
            kardexInventarioVM.FechaFinal = DateTime.Parse(fechaFinalId).AddHours(23).AddMinutes(59);

            kardexInventarioVM.KardexInventarioLista = await _unidadTrabajo.KardexInventario.ObtenerTodos(
                k => k.BodegaProducto.ProductoId == productoId &&
                (k.FechaRegistro >= kardexInventarioVM.FechaInicio &&
                k.FechaRegistro <= kardexInventarioVM.FechaFinal),
                incluirPropiedades: "BodegaProducto,BodegaProducto.Producto,BodegaProducto.Bodega",
                orderBy: o => o.OrderBy(o => o.FechaRegistro)
                );
            return View(kardexInventarioVM);
        }

        #region API

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var todos = await _unidadTrabajo.BodegaProducto.ObtenerTodos(incluirPropiedades: "Bodega,Producto");
            return Json(new { data = todos });
        }

        [HttpGet]
        public async Task<IActionResult> BuscarProducto(string term)
        {
            if (!string.IsNullOrEmpty(term))
            {
                var listaProductos = await _unidadTrabajo.Producto.ObtenerTodos(p => p.Estado == true);
                var data = listaProductos.Where(x => x.NumeroSerie.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                                     x.Descripcion.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
                return Ok(data);
            }
            return Ok();
        }
        #endregion

    }
}
