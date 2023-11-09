using Newtonsoft.Json;
using BaseProject.DAO.Data;
using BaseProject.DAO.IService;
using BaseProject.DAO.Models;
using BaseProject.DAO.Models.API.OpenAI;
using BaseProject.Util;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace BaseProject.DAO.Service
{

    public class ServiceOpenAI : IServiceOpenAI
    {

        private readonly IConfigurationRoot _config;
        private readonly string _api_resource_name;
        private readonly string _api_deployment_id;
        private readonly string _api_key;
        private readonly string _api_base_url;
        private readonly string _api_version;
        private readonly int? _idUsuario;

        public ServiceOpenAI(int? idUsuario = null) 
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            _config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            _api_resource_name = _config.GetProperty<string>("AzureOpenAIAPI", "ResourceName");
            _api_deployment_id = _config.GetProperty<string>("AzureOpenAIAPI", "DeploymentId");
            _api_key = _config.GetProperty<string>("AzureOpenAIAPI", "Key");
            _api_version = _config.GetProperty<string>("AzureOpenAIAPI", "Version");
            _api_base_url = $"https://{_api_resource_name}.openai.azure.com/openai/deployments/{_api_deployment_id}";
            _idUsuario = idUsuario;
        }

        private async Task<string[]> InitChat(string prompt)
        {
            var respostas = Array.Empty<string>();

            var chatRequest = new ChatRequest
            {
                messages = new Message[]
                {
                    new Message
                    {
                        role = "user",
                        content = prompt
                    }
                },
                temperature = 0.2M,
                user = _idUsuario.HasValue ? _idUsuario.Value.ToString() : string.Empty,
            };

            var log = await this.Chat(chatRequest);

            if (log.Success)
            {
                try
                {
                    var chatResponse = JsonConvert.DeserializeObject<ChatResponse>(log.RespContent);

                    var mensagem = chatResponse.choices.FirstOrDefault()?.message?.content ?? "";

                    if (!mensagem.Contains("NADA ENCONTRADO"))
                    {
                        respostas = mensagem.TrimStart('\n').Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Select(x => Regex.Replace(x, @"^\d{1,}. ", "")).Select(x => Regex.Replace(x, @"^\d{1,}° ", "")).Select(x => Regex.Replace(x, @"^\d{1,} ?- ", "")).Select(x => x.Replace("  ", " ").Replace("- ", "").Replace(";", "").Replace("\"", "").Replace("'", "").Trim().TrimEnd('.')).Where(x => !string.IsNullOrEmpty(x)).ToArray() ?? Array.Empty<string>();
                    }
                }
                catch (Exception e)
                {

                }
            }

            return respostas;
        }

        public async Task<LogOpenAI> Chat(ChatRequest chatRequest)
        {
            return await this.Request(HttpMethod.Post, $"chat/completions", chatRequest);
        }

        private async Task<LogOpenAI> Request(HttpMethod httpMethod, string endPoint)
        {
            var log = new LogOpenAI
            {
                ReqMethod = (byte)EnumExtensions.GetValueFromName<EnumRequestMethod>(httpMethod.ToString()),
                ReqURL = $"{_api_base_url}/{endPoint}?api-version={_api_version}",
                ReqDate = DateTime.Now.ToBrasiliaTime(),
                IdUsuario = _idUsuario ?? null
            };

            try
            {
                using var httpClient = new HttpClient();

                httpClient.BaseAddress = new Uri(_api_base_url);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var request = new HttpRequestMessage(httpMethod, log.ReqURL)
                {
                    Headers =
                    {
                        {"api-key", _api_key}
                    }
                };

                log.ReqContent = await request.Content.ReadAsStringAsync();

                var response = await httpClient.SendAsync(request);

                log.RespDate = DateTime.Now.ToBrasiliaTime();
                log.RespContent = await response.Content.ReadAsStringAsync();
                log.RespStatusCode = (int)response.StatusCode;

                if (!response.IsSuccessStatusCode) throw new Exception($"Error {log.RespStatusCode} ({response.StatusCode})!");

                log.Success = true;
            }
            catch (Exception e)
            {
                log.Exception = e.InnerException?.Message ?? e.Message;
                log.Success = false;
            }

            SaveLog(log);

            return log;
        }

        private async Task<LogOpenAI> Request<T>(HttpMethod httpMethod, string endPoint, T content)
        {
            var log = new LogOpenAI
            {
                ReqMethod = (byte)EnumExtensions.GetValueFromName<EnumRequestMethod>(httpMethod.ToString()),
                ReqURL = $"{_api_base_url}/{endPoint}?api-version={_api_version}",
                ReqDate = DateTime.Now.ToBrasiliaTime(),
                IdUsuario = _idUsuario ?? null
            };

            try
            {
                using var httpClient = new HttpClient();

                httpClient.BaseAddress = new Uri(_api_base_url);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var request = new HttpRequestMessage(httpMethod, log.ReqURL)
                {
                    Headers =
                    {
                        {"api-key", _api_key}
                    },
                    Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json")
                };

                log.ReqContent = await request.Content.ReadAsStringAsync();

                var response = await httpClient.SendAsync(request);

                log.RespDate = DateTime.Now.ToBrasiliaTime();
                log.RespContent = await response.Content.ReadAsStringAsync();
                log.RespStatusCode = (int)response.StatusCode;

                if (!response.IsSuccessStatusCode) throw new Exception($"Error {log.RespStatusCode} ({response.StatusCode})!");

                log.Success = true;
            }
            catch (Exception e)
            {
                log.Exception = e.InnerException?.Message ?? e.Message;
                log.Success = false;
            }

            SaveLog(log);

            return log;
        }

        private bool SaveLog(LogOpenAI log)
        {
            try
            {
                using var context = new ApplicationDbContext();

                context.LogOpenAI.Add(log);

                context.SaveChanges();

                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

    }
}