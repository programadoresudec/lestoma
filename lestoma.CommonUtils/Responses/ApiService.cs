using lestoma.CommonUtils.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace lestoma.CommonUtils.Responses
{
    public class ApiService : IApiService
    {
        public async Task<Response> GetListAsync<T>(string urlBase, string servicePrefix, string controller)
        {
            try
            {
                HttpClient client = new HttpClient
                {
                    BaseAddress = new Uri(urlBase),
                };

                string url = $"{servicePrefix}{controller}";
                HttpResponseMessage response = await client.GetAsync(url);
                string jsonString = await response.Content.ReadAsStringAsync();
                Response respuesta = JsonConvert.DeserializeObject<Response>(jsonString);
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

        public async Task<Response> PostAsync<T>(string urlBase, string servicePrefix, string controller, string json)
        {
            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpClient client = new HttpClient
                {
                    BaseAddress = new Uri(urlBase),
                };

                string url = $"{servicePrefix}{controller}";
                HttpResponseMessage response = await client.PostAsync(url, content);
                string jsonString = await response.Content.ReadAsStringAsync();
                Response respuesta = JsonConvert.DeserializeObject<Response>(jsonString);
                if (!response.IsSuccessStatusCode)
                {
                    return respuesta;
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
    }
}
