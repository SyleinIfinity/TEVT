using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers; // ⬅️ Thêm using này
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using QLBH_WEB_ADMIN.Models;

namespace QLBH_WEB_ADMIN.Controllers
{
    public class QuanLySanPhamController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // Model helper để nhận kết quả từ API upload
        public class ImageUploadResponse
        {
            public string FileName { get; set; }
            public string Url { get; set; }
        }

        #region Helpers (Hàm trợ giúp)

        private HttpClient GetHttpClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new System.Uri(_apiBaseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        /// <summary>
        /// Tải danh sách danh mục từ API và đưa vào ViewBag
        /// </summary>
        private async Task PopulateDanhMucDropDownList(object selectedDanhMuc = null)
        {
            IEnumerable<DanhMucViewModel> danhMucs = new List<DanhMucViewModel>();
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync("api/DanhMuc");
                    if (response.IsSuccessStatusCode)
                    {
                        danhMucs = await response.Content.ReadAsAsync<IEnumerable<DanhMucViewModel>>();
                    }
                }
            }
            catch (Exception ex)
            {
                // Ghi lại log lỗi (nếu cần)
                ModelState.AddModelError(string.Empty, "Không thể tải danh mục từ API. Lỗi: " + ex.Message);
            }

            // Gán vào ViewBag, ngay cả khi bị lỗi (sẽ là danh sách rỗng)
            // Tên ViewBag "MADANHMUC" sẽ được @Html.DropDownListFor sử dụng
            ViewBag.MADANHMUC = new SelectList(danhMucs, "MADANHMUC", "TENDANHMUC", selectedDanhMuc);
        }

        /// <summary>
        /// Gọi API để upload ảnh
        /// </summary>
        private async Task<string> UploadImageToApi(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                return null;

            try
            {
                using (var client = new HttpClient()) // Không cần BaseAddress vì dùng URL đầy đủ
                using (var content = new MultipartFormDataContent())
                {
                    var fileContent = new StreamContent(file.InputStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                    content.Add(fileContent, "file", file.FileName);

                    string apiUrl = ConfigurationManager.AppSettings["ImageUploadApiUrl"];

                    var response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ImageUploadResponse>(jsonString);
                        return result.FileName; // Trả về tên file
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                return null;
            }
        }

        #endregion

        // GET: Hiển thị danh sách sản phẩm
        public async Task<ActionResult> Index()
        {
            IEnumerable<SanPhamViewModel> sanPhamList;
            using (var client = GetHttpClient())
            {
                HttpResponseMessage result = await client.GetAsync("api/SanPham");
                if (result.IsSuccessStatusCode)
                {
                    sanPhamList = await result.Content.ReadAsAsync<IEnumerable<SanPhamViewModel>>();
                }
                else
                {
                    sanPhamList = new List<SanPhamViewModel>();
                    ModelState.AddModelError(string.Empty, "Lỗi khi lấy danh sách sản phẩm từ API.");
                }
            }
            return View(sanPhamList);
        }

        // GET: Hiển thị chi tiết sản phẩm
        public async Task<ActionResult> Details(int id)
        {
            SanPhamViewModel sanPham;
            using (var client = GetHttpClient())
            {
                HttpResponseMessage result = await client.GetAsync($"api/SanPham/{id}");
                if (result.IsSuccessStatusCode)
                {
                    sanPham = await result.Content.ReadAsAsync<SanPhamViewModel>();
                }
                else
                {
                    return HttpNotFound();
                }
            }
            return View(sanPham);
        }

        // GET: Hiển thị form tạo mới VỚI DANH SÁCH DANH MỤC
        public async Task<ActionResult> Create()
        {
            // Tải danh mục cho DropDownList
            await PopulateDanhMucDropDownList();
            return View(new SanPhamViewModel()); // Trả về model rỗng để tránh lỗi null
        }

        // POST: Xử lý tạo mới sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SanPhamViewModel sanPham, HttpPostedFileBase HINHANH_FILE)
        {
            // 1. Xử lý upload ảnh
            if (HINHANH_FILE != null && HINHANH_FILE.ContentLength > 0)
            {
                string uploadedFileName = await UploadImageToApi(HINHANH_FILE);

                if (!string.IsNullOrEmpty(uploadedFileName))
                {
                    sanPham.HINHANH = uploadedFileName; // Lưu TÊN FILE vào model
                }
                else
                {
                    ModelState.AddModelError("", "Lỗi upload ảnh lên server.");
                    sanPham.HINHANH = null;
                }
            }
            else
            {
                sanPham.HINHANH = null; // Không có ảnh
            }

            // 2. Validate model và gọi API
            if (ModelState.IsValid)
            {
                using (var client = GetHttpClient())
                {
                    // Gửi 'sanPham' (với HINHANH = "guid.png") đến API
                    // SỬA: Dùng await, không dùng .Wait()
                    HttpResponseMessage response = await client.PostAsJsonAsync("api/SanPham", sanPham);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Lỗi khi tạo sản phẩm qua API.");
                    }
                }
            }

            // 3. Nếu thất bại (ModelState invalid hoặc API lỗi), tải lại DropDownList
            // SỬA: Gọi lại hàm helper, không dùng 'db'
            await PopulateDanhMucDropDownList(sanPham.MADANHMUC);
            return View(sanPham);
        }

        // GET: Hiển thị form chỉnh sửa
        public async Task<ActionResult> Edit(int id)
        {
            SanPhamViewModel sanPham;
            using (var client = GetHttpClient())
            {
                HttpResponseMessage result = await client.GetAsync($"api/SanPham/{id}");
                if (result.IsSuccessStatusCode)
                {
                    sanPham = await result.Content.ReadAsAsync<SanPhamViewModel>();
                }
                else
                {
                    return HttpNotFound();
                }
            }

            if (sanPham == null)
            {
                return HttpNotFound();
            }

            // Tải danh mục và chọn sẵn giá trị hiện tại
            await PopulateDanhMucDropDownList(sanPham.MADANHMUC);
            return View(sanPham);
        }

        // POST: QuanLySanPham/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(SanPhamViewModel sanPham, HttpPostedFileBase HINHANH_FILE)
        {
            // 1. Xử lý upload ảnh (nếu có file mới)
            if (HINHANH_FILE != null && HINHANH_FILE.ContentLength > 0)
            {
                string uploadedFileName = await UploadImageToApi(HINHANH_FILE);
                if (!string.IsNullOrEmpty(uploadedFileName))
                {
                    sanPham.HINHANH = uploadedFileName; // Gán filename mới
                }
                else
                {
                    ModelState.AddModelError("", "Lỗi upload ảnh mới. Sẽ giữ lại ảnh cũ.");
                    // Nếu lỗi, `sanPham.HINHANH` từ hidden field sẽ được giữ nguyên
                }
            }
            // Nếu không có file mới, `sanPham.HINHANH` (từ HiddenFor) sẽ được giữ nguyên

            // 2. Validate model và gọi API
            if (ModelState.IsValid)
            {
                using (var client = GetHttpClient())
                {
                    // SỬA: Dùng await, không dùng .Wait()
                    // SỬA: Dùng MASANPHAM, không dùng MASP
                    HttpResponseMessage response = await client.PutAsJsonAsync($"api/SanPham/{sanPham.MASANPHAM}", sanPham);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật sản phẩm qua API.");
                    }
                }
            }

            // 3. Nếu thất bại, tải lại DropDownList
            // SỬA: Gọi lại hàm helper, không dùng 'db'
            await PopulateDanhMucDropDownList(sanPham.MADANHMUC);
            return View(sanPham);
        }

        // POST: QuanLySanPham/Delete/5
        [HttpPost] // Chỉ nhận phương thức POST
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id) // Tên action là Delete, nhận id
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    // Gọi thẳng API Delete
                    HttpResponseMessage result = await client.DeleteAsync($"api/SanPham/{id}");

                    if (result.IsSuccessStatusCode)
                    {
                        // Dùng TempData để gửi thông báo thành công về trang Index
                        TempData["SuccessMessage"] = "Đã xóa sản phẩm thành công.";
                    }
                    else
                    {
                        // Dùng TempData để gửi thông báo lỗi về trang Index
                        TempData["ErrorMessage"] = "Lỗi khi xoá sản phẩm qua API.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi: " + ex.Message;
            }

            // Quay trở lại trang Index sau khi xóa
            return RedirectToAction("Index");
        }
    }
}