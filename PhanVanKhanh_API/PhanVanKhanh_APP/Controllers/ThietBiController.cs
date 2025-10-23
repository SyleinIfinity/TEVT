using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using PhanVanKhanh_APP.Models; // Namespace đúng

namespace PhanVanKhanh_APP.Controllers // Namespace đúng
{
    public class ThietBiController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
        private HttpClient GetHttpClient() => new HttpClient { BaseAddress = new System.Uri(_apiBaseUrl) };

        public async Task<ActionResult> Index(int maNhom)
        {
            IEnumerable<ThietBiViewModel> thietBiList;
            using (var client = GetHttpClient())
            {
                HttpResponseMessage result = await client.GetAsync($"api/ThietBi/TheoNhom/{maNhom}");
                if (result.IsSuccessStatusCode)
                {
                    thietBiList = await result.Content.ReadAsAsync<IEnumerable<ThietBiViewModel>>();
                }
                else { thietBiList = new List<ThietBiViewModel>(); }
            }
            return View(thietBiList);
        }

        public async Task<ActionResult> Detail(int id)
        {
            ThietBiViewModel thietBi;
            using (var client = GetHttpClient())
            {
                HttpResponseMessage result = await client.GetAsync($"api/ThietBi/{id}");
                if (result.IsSuccessStatusCode)
                {
                    thietBi = await result.Content.ReadAsAsync<ThietBiViewModel>();
                }
                else { thietBi = null; }
            }
            if (thietBi == null) return HttpNotFound();
            return View(thietBi);
        }
    }
}