using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ListClientMVC.Models;
using ListClientMVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ListClientMVC.Controllers
{
    public class GraphController : Controller
    { 

        private IEasyAuthProxy _easyAuthProxy;
        private IConfiguration _configuration;

        public GraphController(IEasyAuthProxy easyproxy, IConfiguration config) {
            _easyAuthProxy = easyproxy;
            _configuration = config;
        }

        public async Task<IActionResult> Index()
        {
            //Get a token for the Graph API
            string id_token = _easyAuthProxy.Headers["x-ms-token-aad-id-token"];
            string client_id = _configuration["AADClientID"];
            string client_secret = _configuration["AADClientSecret"];
            string aad_instance = _configuration["AADInstance"];

            var client = new HttpClient();

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
                new KeyValuePair<string, string>("assertion", id_token),
                new KeyValuePair<string, string>("requested_token_use", "on_behalf_of"),
                new KeyValuePair<string, string>("scope", "User.Read"),
                new KeyValuePair<string, string>("client_id", client_id),
                new KeyValuePair<string, string>("client_secret", client_secret),
                new KeyValuePair<string, string>("resource", "https://graph.microsoft.com"),
                                
            });

            var result = await client.PostAsync(aad_instance + "oauth2/token", content);
            string resultContent = await result.Content.ReadAsStringAsync();

            if (result.IsSuccessStatusCode) {
                //Call Graph API to get some information about the user
                TokenResponse tokenResponse= JsonConvert.DeserializeObject<TokenResponse>(resultContent);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.access_token);
                var response = await client.GetAsync("https://graph.microsoft.com/v1.0/me");
                var cont = await response.Content.ReadAsStringAsync();
                ViewData["me"] = cont;
            } else {
                ViewData["me"] = "Failed to access MS Graph";
            }

            return View();
        }
    }
}