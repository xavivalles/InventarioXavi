using SistemaInventario.AccesoDatos.Data;
using SistemaInventario.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventario.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventario.AccesoDatos.Repositorio.Repositorio
{
    public class OrdenRepositorio : Repositorio<Orden>, IOrdenRepositorio
    {
        private readonly ApplicationDbContext _db;
        public OrdenRepositorio(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Actualizar(Orden orden)
        {
            _db.Update(orden);
        }

        public void ActualizarEstado(int id, string ordenEstado, string pagoEstado)
        {
            var orderBD = _db.Ordenes.FirstOrDefault(o => o.Id == id);
            if (orderBD != null)
            {
                orderBD.EstadoOrden = ordenEstado;
                orderBD.EstadoPago = pagoEstado;
            }
        }

        public void ActualizarPagoStripeId(int id, string sessionId, string transaccionId)
        {
            var orderBD = _db.Ordenes.FirstOrDefault(o => o.Id == id);
            if (orderBD != null)
            {
                if (!String.IsNullOrEmpty(sessionId))
                {
                    orderBD.SessionId = sessionId;
                }
                if (!String.IsNullOrEmpty(transaccionId))
                {
                    orderBD.TransaccionId = transaccionId;
                    orderBD.FechaPago = DateTime.Now;
                }                
            }
        }
    }
}
