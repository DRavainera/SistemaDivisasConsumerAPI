using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SistemaDivisasConsumerAPI.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Net;

namespace SistemaDivisasConsumerAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClient;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public IActionResult Index()
        {
            var jsonString = GetTokenFromSession();
            var clienteString = GetClientFromSession();
            
            if (string.IsNullOrEmpty(jsonString))
            {
                return RedirectToAction("Login");
            }

            dynamic jsonLogin = JsonConvert.DeserializeObject(jsonString);
            string token = jsonLogin["token"].ToString();

            dynamic jsonCliente = JsonConvert.DeserializeObject(clienteString);
            int clienteId = Convert.ToInt32(jsonCliente["id"]);

            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);

            var responsePeso = client
                .GetAsync("https://localhost:7204/Cuenta/CuentaPeso/Listar?clienteId=" + clienteId.ToString())
                .ConfigureAwait(false).GetAwaiter().GetResult();

            var responseDolar = client
                .GetAsync("https://localhost:7204/Cuenta/CuentaDolar/Listar?clienteId=" + clienteId.ToString())
                .ConfigureAwait(false).GetAwaiter().GetResult();

            var responseCripto = client
                .GetAsync("https://localhost:7204/Cuenta/CuentaCripto/Listar?clienteId=" + clienteId.ToString())
                .ConfigureAwait(false).GetAwaiter().GetResult();

            if (responsePeso.IsSuccessStatusCode || responseDolar.IsSuccessStatusCode || responseCripto.IsSuccessStatusCode)
            {
                var contentPeso = responsePeso.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                var remindersPeso = JsonConvert.DeserializeObject<List<CuentaPeso>>(contentPeso);

                var contentDolar = responseDolar.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                var remindersDolar = JsonConvert.DeserializeObject<List<CuentaDolar>>(contentDolar);

                var contentCripto = responseCripto.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                var remindersCripto = JsonConvert.DeserializeObject<List<CuentaCripto>>(contentCripto);

                ViewBag.CuentasPeso = remindersPeso;
                ViewBag.CuentasDolar = remindersDolar;
                ViewBag.CuentasCripto = remindersCripto;

                return View();
            }
            if (responsePeso.StatusCode == HttpStatusCode.NotFound && responseDolar.StatusCode == HttpStatusCode.NotFound && responseCripto.StatusCode == HttpStatusCode.NotFound)
            {
                return RedirectToAction("Login");
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login(string usuario, string contrasenia)
        {
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(contrasenia))
            {
                return RedirectToAction("Login");
            }

            var jsonString = GetTokenFromSession();

            if (!string.IsNullOrEmpty(jsonString))
            {
                return RedirectToAction("Index");
            }

            try
            {
                var client = _httpClient.CreateClient();

                var uriLogin = "https://localhost:7204/Cliente/login?usuario=" + usuario + "&contrasenia=" + contrasenia;
                var responseLogin = client
                    .GetAsync(uriLogin)
                    .ConfigureAwait(false).GetAwaiter().GetResult();

                if (responseLogin.IsSuccessStatusCode)
                {
                    var token = responseLogin.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    var claims = new List<Claim> { new Claim(ClaimTypes.Name, usuario) };
                    var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    var props = new AuthenticationProperties();

                    HttpContext.SignInAsync(JwtBearerDefaults.AuthenticationScheme, principal, props).Wait();
                    HttpContext.Session.SetString("ExpiryToken", token);

                    dynamic jsonToken = JsonConvert.DeserializeObject(token);
                    string accessToken = jsonToken["token"].ToString();

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);

                    var uriCliente = "https://localhost:7204/Cliente/VerCliente?usuario=" + usuario + "&contrasenia=" + contrasenia;
                    var responseCliente = client
                        .GetAsync(uriCliente)
                        .ConfigureAwait(false).GetAwaiter().GetResult();

                    var cliente = responseCliente.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                    HttpContext.Session.SetString("Client", cliente);

                    return RedirectToAction("Index");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return View();
            }
        }

        public IActionResult VerRegistroMovimientos(int numCuenta)
        {
            var jsonString = GetTokenFromSession();

            if (string.IsNullOrEmpty(jsonString))
            {
                return RedirectToAction("Login");
            }

            dynamic json = JsonConvert.DeserializeObject(jsonString);
            string token = json["token"].ToString();

            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);

            var response = client
                .GetAsync("https://localhost:7204/Cuenta/Movimientos/Ver?numCuenta=" + numCuenta.ToString())
                .ConfigureAwait(false).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                var reminders = JsonConvert.DeserializeObject<List<Movimiento>>(content);

                return View(reminders);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return View(new List<Movimiento>());
            }

            else
            {
                return RedirectToAction("Error");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();

            return RedirectToAction("Index");
        }

        private string? GetTokenFromSession()
        {
            return HttpContext.Session.GetString("ExpiryToken");

        }

        private string? GetClientFromSession()
        {
            return HttpContext.Session.GetString("Client");

        }
    }
}