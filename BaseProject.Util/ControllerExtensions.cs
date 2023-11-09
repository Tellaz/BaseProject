using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Net;

namespace BaseProject.Util
{
    public class SessionObject
    {
        public string jsonData { get; set; }

    }

    public class ResponseObject<T>
    {
        public bool IsRequestSuccessful { get; set; }
        
        public int StatusCode { get; set; }

        private string _message = "A requisição foi finalizada com sucesso!";
        
        private string _errorMessage = "Ocorreu um erro ao processar essa requisição, tente novamente mais tarde! Se o eror persistir, entre em contato com o administrador do sistema!";

        public bool ShowSuccessMessage { get; set; }

        public string Message
        {
            get
            {
                return _message;
            }
            set => _message = string.IsNullOrEmpty(value) ? _message : value;
        }

        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set => _errorMessage = string.IsNullOrEmpty(value) ? _errorMessage : value;
        }

        public T Payload { get; set; }

	}

    public static class ControllerExtensions
    {
        public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model, bool partial = false)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;
            }

            controller.ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, viewName, !partial);

                if (viewResult.Success == false)
                {
                    return $"A view with the name {viewName} could not be found";
                }

                ViewContext viewContext = new ViewContext(
                    controller.ControllerContext,
                    viewResult.View,
                    controller.ViewData,
                    controller.TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }

        public static T CreateObjectFromSession<T>(this Controller controller)
        {
            var requestObjectSession = controller.HttpContext.Session.GetString("requestObject");
            return JsonConvert.DeserializeObject<T>(requestObjectSession);
        }

        public static T CreateVariableFromSession<T>(this Controller controller, string name)
        {
            var requestObjectSession = JObject.Parse(controller.HttpContext.Session.GetString("requestObject"));
            return requestObjectSession.GetValue(name).ToObject<T>();
        }

        public static ResponseObject<string> CreateResponseObject(bool responseSuccessfulValue, string successMessage = "", string errorMessage = "")
        {
            return new ResponseObject<string>
            {
                IsRequestSuccessful = responseSuccessfulValue,
                ShowSuccessMessage = responseSuccessfulValue && !string.IsNullOrEmpty(successMessage),
                Message = successMessage,
                ErrorMessage = errorMessage,
                Payload = ""
            };
        }

        public static ResponseObject<T> CreateResponseObject<T>(bool responseSuccessfulValue, T payload, string successMessage = "", string errorMessage = "")
        {
            return new ResponseObject<T>
            {
                IsRequestSuccessful = responseSuccessfulValue,
                ShowSuccessMessage = responseSuccessfulValue && !string.IsNullOrEmpty(successMessage),
                Message = successMessage,
                ErrorMessage = errorMessage,
                Payload = payload
            };
        }

        public static ResponseObject<string> CreateResponseObject(this Controller controller, bool responseSuccessfulValue, string successMessage = "", string errorMessage = "")
        {
            return new ResponseObject<string>
            {
                IsRequestSuccessful = responseSuccessfulValue,
                ShowSuccessMessage = responseSuccessfulValue && !string.IsNullOrEmpty(successMessage),
                Message = successMessage,
                ErrorMessage = errorMessage,
                Payload = ""
			};
        }

        public static ResponseObject<T> CreateResponseObject<T>(this Controller controller, bool responseSuccessfulValue, T payload, string successMessage = "", string errorMessage = "")
        {
            return new ResponseObject<T>
            {
                IsRequestSuccessful = responseSuccessfulValue,
                ShowSuccessMessage = responseSuccessfulValue && !string.IsNullOrEmpty(successMessage),
                Message = successMessage,
                ErrorMessage = errorMessage,
                Payload = payload
			};
        }

        public static ResponseObject<T> GetResponseObject<T>(object actionResult)
        {
            var jsonString = JsonConvert.SerializeObject(actionResult, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            var data = (JObject)JsonConvert.DeserializeObject(jsonString);

            return data["Value"].ToObject<ResponseObject<T>>();
        }

    }

    public class GlobalModelStateValidatorAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new JsonResult(new ResponseObject<string>
                {
                    IsRequestSuccessful = false,
                    ShowSuccessMessage = false,
                    Message = "",
                    ErrorMessage = "Dados inválidos!",
                    Payload = ""
                });
            }

            base.OnActionExecuting(context);
        }
    }
}
