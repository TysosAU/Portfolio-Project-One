using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        public HomeController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> HeroLoadouts()
        {
            List<HeroLoadout> loadouts = new List<HeroLoadout>();
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                ViewData["Error"] = "You are not authenticated";
                return View(); 
            }
 
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync("https://localhost:7232/api/HeroLoadout");

                if (!response.IsSuccessStatusCode)
                {
                    ViewData["Error"] = $"Error retrieving all hero loadouts, Status Code: {response.StatusCode}";
                    return View(); 
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(jsonResponse))
                {
                    ViewData["Error"] = "no data.";
                    return View(); 
                }

                loadouts = JsonConvert.DeserializeObject<List<HeroLoadout>>(jsonResponse);
                if (loadouts == null)
                {
                    ViewData["Error"] = "null data.";
                    return View(); 
                }
            }
            catch (JsonException ex)
            {
                ViewData["Error"] = $"an error occured whilst deserializing the hero loadout data: {ex.Message}";
                return View(); 
            }
            catch (HttpRequestException ex)
            {
                ViewData["Error"] = $"an error occured in the http request: {ex.Message}";
                return View(); 
            }
            return View(loadouts); 
        }

        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> LoadoutsByCommander(string commander)
        {
            if (string.IsNullOrEmpty(commander))
            {
                ViewData["Error"] = "Commander name is required.";
                return View(); 
            }

            if (!Regex.IsMatch(commander, "^[A-Za-z ]+$"))
            {
                ViewData["Error"] = "Commander name can only contain letters and spaces.";
                return View(); 
            }

            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                ViewData["Error"] = "You are not authenticated";
                return View(); 
            }

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                try
                {
                    var response = await httpClient.GetAsync($"https://localhost:7232/api/HeroLoadout/commander/{commander}");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();

                        if (string.IsNullOrEmpty(jsonResponse))
                        {
                            ViewData["Error"] = "the data returned from the response is either null or empty.";
                            return View(); 
                        }

                        var loadouts = JsonConvert.DeserializeObject<List<HeroLoadout>>(jsonResponse);
                        if (loadouts == null)
                        {
                            ViewData["Error"] = "an error occured whislt deserializing the hero loadout data.";
                            return View(); 
                        }

                        return View(loadouts);
                    }
                    else
                    {
                        ViewData["Error"] = $"an error occured whilst retrieving hero loadouts for commander '{commander}', Status Code: {response.StatusCode}";
                        return View(); 
                    }
                }
                catch (HttpRequestException ex)
                {
                    ViewData["Error"] = $"an error occured in the http request: {ex.Message}";
                    return View(); 
                }
                catch (Exception ex)
                {
                    ViewData["Error"] = $"an exception occurred: {ex.Message}";
                    return View(); 
                }
            }
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateHeroLoadout(HeroLoadoutDTO heroLoadout)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Error"] = "Invalid inputs, Please review the loadout";
                return View(heroLoadout); 
            }

            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                ViewData["Error"] = "You are not authenticated.";
                return View(heroLoadout); 
            }

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var jsonContent = JsonConvert.SerializeObject(heroLoadout);
                var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://localhost:7232/api/HeroLoadout", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Hero loadout created successfully!";
                    return RedirectToAction("CreateHeroLoadout"); 
                }
                else
                {
                    ViewData["Error"] = $"an error occured, Status Code: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}";
                    return View(heroLoadout);
                }
            }
            catch (HttpRequestException ex)
            {
                ViewData["Error"] = $"an error occured in the http request: {ex.Message}";
                return View(heroLoadout);
            }
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteLoadout(int id)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                ViewData["Error"] = "You are not authenticated.";
                return View(); 
            }

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.DeleteAsync($"https://localhost:7232/api/HeroLoadout/{id}");

                if (response.IsSuccessStatusCode)
                {
                    ViewData["Success"] = "Hero loadout deleted successfully!";
                }
                else
                {
                    ViewData["Error"] = $"an error occured whilst deleting loadout with ID {id}, Status Code: {response.StatusCode}";
                }

                return View(); 
            }
            catch (HttpRequestException ex)
            {
                ViewData["Error"] = $"an error occured in the http request: {ex.Message}";
                return View();
            }
        }
    }
}

