using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Responses;
using Newtonsoft.Json;
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
                    respuesta.IsExito = false;
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
    }
}
