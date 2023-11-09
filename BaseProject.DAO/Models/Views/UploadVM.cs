using Microsoft.EntityFrameworkCore;
using BaseProject.Util;
using System.ComponentModel.DataAnnotations;

namespace BaseProject.DAO.Models.Views
{
	public class UploadVM
	{
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public string MD5 { get; set; }
        public byte Tipo { get; set; }
        public string TipoString { get; set; }
        public byte Status { get; set; }
        public string StatusString { get; set; }
        public string StatusColor { get; set; }
        public DateTime DataInicial { get; set; }
        public string DataInicialString { get; set; }
        public DateTime? DataFinal { get; set; }
        public string DataFinalString { get; set; }

        public UploadVM() { }

        public UploadVM(Upload model)
        {
            Id = model.Id;
            IdUsuario = model.IdUsuario;
            MD5 = model.MD5;
            Tipo = model.Tipo;
            TipoString = ((EnumUploadTipo)model.Tipo).GetEnumDisplayName();
            Status = model.Status;
            StatusString = ((EnumTaskStatus)model.Status).GetEnumDisplayName();
            StatusColor = ((EnumTaskStatus)model.Status).GetEnumDescription();
            DataInicial = model.DataInicial;
            DataInicialString = model.DataInicial.ToString("dd/MM/yyyy HH:mm:ss");
            DataFinal = model.DataFinal;
            DataFinalString = model.DataFinal.HasValue ? model.DataFinal.Value.ToString("dd/MM/yyyy HH:mm:ss") : "";
        }
    }
}