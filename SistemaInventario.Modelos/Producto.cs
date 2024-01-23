using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventario.Modelos
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Numero de serie obligatorio")]
        [MaxLength(60)]
        public string NumeroSerie { get; set; }
        [Required(ErrorMessage = "Descripcion obligatoria")]
        [MaxLength(60)]
        public string Descripcion { get; set; }
        [Required(ErrorMessage = "Precio obligatorio")]
        public double Precio { get; set; }
        [Required(ErrorMessage = "Costo obligatorio")]
        public double Costo { get; set; }
        public string ImagenUrl { get; set; }
        [Required(ErrorMessage = "Estado obligatorio")]
        public bool Estado { get; set; }
        [Required(ErrorMessage = "Categoria obligatoria")]
        public int CategoriaId { get; set; }
        [ForeignKey("CategoriaId")]
        public Categoria Categoria { get; set; }
        [Required(ErrorMessage = "Marca obligatoria")]
        public int MarcaId { get; set; }
        [ForeignKey("MarcaId")]
        public Marca Marca { get; set; }
        public int? PadreId { get; set; }
        public virtual Producto Padre { get; set; }
    }
}
