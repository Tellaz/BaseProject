using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace BaseProject.Util
{
    public enum EnumRequestMethod
    {
        [Display(Name = "GET")]
        GET = 1,
        [Display(Name = "POST")]
        POST = 2,
        [Display(Name = "PUT")]
        PUT = 3,
        [Display(Name = "DELETE")]
        DELETE = 4,
    }

    public enum EnumUploadTipo
    {
        [Display(Name = "Usuário")]
        Usuario = 1,
    }

    public enum EnumDownloadTipo
    {
        [Display(Name = "Relatório Geral")]
        RelatorioGeral = 1,
    }

    public enum EnumTaskStatus
    {
        [Display(Name = "Processando", Description = "info")]
        Processando = 1,
        [Display(Name = "Incompleto", Description = "warning")]
        Incompleto = 2,
        [Display(Name = "Completo", Description = "success")]
        Completo = 3,
        [Display(Name = "Erro", Description = "danger")]
        Erro = 4,
    }

    public enum EnumAzureServiceBusQueue
    {
        [Display(Name = "carregarusuarios")]
        CarregarUsuarios = 1,        
    }

}
