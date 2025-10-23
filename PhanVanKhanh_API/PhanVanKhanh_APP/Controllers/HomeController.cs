using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Web.Mvc;
using PhanVanKhanh_APP.Models; // Namespace đúng

namespace PhanVanKhanh_APP.Controllers // Namespace đúng
{
    public class HomeController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
        public ActionResult Index() { return View(); }

        [ChildActionOnly]
        public ActionResult DanhMucMenu()
        {
            IEnumerable<NhomViewModel> danhMucList;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new System.Uri(_apiBaseUrl);
                var responseTask = client.GetAsync("api/Nhom");
                responseTask.Wait();
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<IEnumerable<NhomViewModel>>();
                    readTask.Wait();
                    danhMucList = readTask.Result;
                }
                else { danhMucList = new List<NhomViewModel>(); }
            }
            return PartialView("_DanhMucMenu", danhMucList);
        }
    }
}