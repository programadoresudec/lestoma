using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace lestoma.CommonUtils.Services
{
    public class ApiService : IApiService
    {
        #region GetList Api service
        public async Task<Response> GetListAsync<T>(string urlBase, string controller)
        {
            try
            {
                HttpClient client = new HttpClient
                {
                    BaseAddress = new Uri(urlBase),
                };
                HttpResponseMessage response = await client.GetAsync(controller);
                string jsonString = await response.Content.ReadAsStringAsync();
                var respuesta = JsonConvert.DeserializeObject<Response>(jsonString);
                if (!response.IsSuccessStatusCode)
                {
                    return respuesta;
                }

                List<T> lista = JsonConvert.DeserializeObject<List<T>>((string)respuesta.Data);
                return new Response
                {
                    IsExito = respuesta.IsExito,
                    Mensaje = respuesta.Mensaje,
                    Data = lista
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsExito = false,
                    Mensaje = ex.Message
                };
            }
        }

        #endregion

        #region Post Api service
        public async Task<Response> PostAsync<T>(string urlBase, string controller, T model)
        {
            try
            {
                string json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpClient client = new HttpClient
                {
                    BaseAddress = new Uri(urlBase),
                };

                HttpResponseMessage response = await client.PostAsync(controller, content);
                string jsonString = await response.Content.ReadAsStringAsync();
                Response respuesta = JsonConvert.DeserializeObject<Response>(jsonString);
                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsExito = false,
                        Mensaje = mostrarMensajePersonalizadoStatus(response.StatusCode, respuesta.Mensaje)
                    };
                }
            
                return respuesta;
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsExito = false,
                    Mensaje = ex.Message
                };
            }
        }
        #endregion


        private string mostrarMensajePersonalizadoStatus(System.Net.HttpStatusCode statusCode, string mensajeDeLaApi)
        {
            string mensaje = string.Empty;
            if (!string.IsNullOrWhiteSpace(mensajeDeLaApi))
            {
                mensaje = mensajeDeLaApi;
            }
            else
            {
                switch (statusCode)
                {
                    case System.Net.HttpStatusCode.Accepted:
                        break;
                    case System.Net.HttpStatusCode.Ambiguous:
                        break;
                    case System.Net.HttpStatusCode.BadGateway:
                        break;
                    case System.Net.HttpStatusCode.BadRequest:
                        break;
                    case System.Net.HttpStatusCode.Conflict:
                        break;
                    case System.Net.HttpStatusCode.Continue:
                        break;
                    case System.Net.HttpStatusCode.Created:
                        break;
                    case System.Net.HttpStatusCode.ExpectationFailed:
                        break;
                    case System.Net.HttpStatusCode.Forbidden:
                        break;
                    case System.Net.HttpStatusCode.Found:
                        break;
                    case System.Net.HttpStatusCode.GatewayTimeout:
                        break;
                    case System.Net.HttpStatusCode.Gone:
                        break;
                    case System.Net.HttpStatusCode.HttpVersionNotSupported:
                        break;
                    case System.Net.HttpStatusCode.InternalServerError:
                        break;
                    case System.Net.HttpStatusCode.LengthRequired:
                        break;
                    case System.Net.HttpStatusCode.MethodNotAllowed:
                        break;
                    case System.Net.HttpStatusCode.Moved:
                        break;
                    case System.Net.HttpStatusCode.NoContent:
                        break;
                    case System.Net.HttpStatusCode.NonAuthoritativeInformation:
                        break;
                    case System.Net.HttpStatusCode.NotAcceptable:
                        break;
                    case System.Net.HttpStatusCode.NotFound:
                        mensaje = "Petición no encontrada.";
                        break;
                    case System.Net.HttpStatusCode.NotImplemented:
                        break;
                    case System.Net.HttpStatusCode.NotModified:
                        break;
                    case System.Net.HttpStatusCode.OK:
                        break;
                    case System.Net.HttpStatusCode.PartialContent:
                        break;
                    case System.Net.HttpStatusCode.PaymentRequired:
                        break;
                    case System.Net.HttpStatusCode.PreconditionFailed:
                        break;
                    case System.Net.HttpStatusCode.ProxyAuthenticationRequired:
                        break;
                    case System.Net.HttpStatusCode.RedirectKeepVerb:
                        break;
                    case System.Net.HttpStatusCode.RedirectMethod:
                        break;
                    case System.Net.HttpStatusCode.RequestedRangeNotSatisfiable:
                        break;
                    case System.Net.HttpStatusCode.RequestEntityTooLarge:
                        break;
                    case System.Net.HttpStatusCode.RequestTimeout:
                        break;
                    case System.Net.HttpStatusCode.RequestUriTooLong:
                        break;
                    case System.Net.HttpStatusCode.ResetContent:
                        break;
                    case System.Net.HttpStatusCode.ServiceUnavailable:
                        break;
                    case System.Net.HttpStatusCode.SwitchingProtocols:
                        break;
                    case System.Net.HttpStatusCode.Unauthorized:
                        break;
                    case System.Net.HttpStatusCode.UnsupportedMediaType:
                        break;
                    case System.Net.HttpStatusCode.Unused:
                        break;
                    case System.Net.HttpStatusCode.UpgradeRequired:
                        break;
                    case System.Net.HttpStatusCode.UseProxy:
                        break;
                }
            }
            return mensaje;
        }
    }
}
