using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient; // Sử dụng Microsoft.Data.SqlClient để hỗ trợ TVP

namespace QLBH_API.Controllers
{
    // DTOs
    public class OrderItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CreateOrderRequest
    {
        public List<OrderItem> Items { get; set; }
        // public string CartId { get; set; } // Tùy chọn nếu dùng CartId
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; }
    }

    // Model map từ tbSANPHAM để tính giá
    public class SanPhamPrice
    {
        public int MASANPHAM { get; set; }
        public decimal DONGIA { get; set; }
    }

    // Model map từ SP psGetDonHangByParty
    public class OrderHistoryItem
    {
        public int MAHOADON { get; set; }
        public DateTime NGAY { get; set; }
        public decimal TONGTIEN { get; set; }
        public string TRANGTHAI { get; set; }
    }


    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Yêu cầu xác thực cho tất cả các endpoint trong Controller này
    public class OrdersController : ControllerBase
    {
        private readonly string _connectionString;

        public OrdersController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/orders/my
        [HttpGet("my")]
        [Authorize(Roles = "client,admin")] // Cả client và admin đều có thể xem đơn của mình
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                var partyId = GetCurrentPartyId();
                if (partyId == 0)
                {
                    return Unauthorized("Không thể xác định người dùng.");
                }

                using (var connection = new SqlConnection(_connectionString))
                {
                    var orders = await connection.QueryAsync<OrderHistoryItem>(
                        "psGetDonHangByParty",
                        new { PARTY_ID = partyId },
                        commandType: CommandType.StoredProcedure
                    );
                    return Ok(orders);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi máy chủ: " + ex.Message);
            }
        }

        // PUT: api/orders/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "admin")] // Chỉ admin được cập nhật
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Status))
            {
                return BadRequest("Trạng thái mới là bắt buộc.");
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(
                        "psUpdateHOADONStatus",
                        new { MAHOADON = id, TRANGTHAI = request.Status },
                        commandType: CommandType.StoredProcedure
                    );
                    return Ok(new { success = true, message = $"Cập nhật trạng thái cho HD {id} thành công." });
                }
            }
            catch (SqlException sqlEx)
            {
                // Bắt lỗi RAISERROR từ SP
                return BadRequest(new { success = false, message = sqlEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi máy chủ: " + ex.Message);
            }
        }

        // POST: api/orders/create
        [HttpPost("create")]
        [Authorize(Roles = "client")] // Chỉ client được tạo đơn
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (request?.Items == null || !request.Items.Any())
            {
                return BadRequest("Giỏ hàng rỗng.");
            }

            var partyId = GetCurrentPartyId();
            if (partyId == 0)
            {
                return Unauthorized("Không thể xác định người dùng.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Lấy giá sản phẩm từ DB (để đảm bảo tính toàn vẹn giá)
                        var productIds = request.Items.Select(i => i.ProductId).ToList();
                        var prices = (await connection.QueryAsync<SanPhamPrice>(
                            $"SELECT MASANPHAM, DONGIA FROM tbSANPHAM WHERE MASANPHAM IN @Ids",
                            new { Ids = productIds },
                            transaction: transaction
                        )).ToDictionary(p => p.MASANPHAM, p => p.DONGIA);

                        // 2. Tính TONGTIEN và chuẩn bị TVP
                        decimal tongTien = 0;
                        var chiTietTable = new DataTable();
                        chiTietTable.Columns.Add("MASANPHAM", typeof(int));
                        chiTietTable.Columns.Add("SOLUONG", typeof(decimal)); // Kiểu numeric(18,0)
                        chiTietTable.Columns.Add("DONGIA", typeof(decimal)); // Kiểu numeric(18,0)

                        foreach (var item in request.Items)
                        {
                            if (!prices.TryGetValue(item.ProductId, out var donGia))
                            {
                                throw new Exception($"Sản phẩm ID {item.ProductId} không tồn tại hoặc không có giá.");
                            }

                            tongTien += donGia * item.Quantity;
                            chiTietTable.Rows.Add(item.ProductId, (decimal)item.Quantity, donGia);
                        }

                        // 3. Gọi psInsertHOADON (Lưu ý: SP này trả về NewMAHOADON)
                        var hoaDonParams = new DynamicParameters();
                        hoaDonParams.Add("@MAKHACHHANG", partyId);
                        hoaDonParams.Add("@TONGTIEN", tongTien);

                        // Dùng QuerySingleAsync để nhận 1 giá trị trả về
                        var newHoaDonId = await connection.QuerySingleAsync<int>(
                            "psInsertHOADON",
                            hoaDonParams,
                            commandType: CommandType.StoredProcedure,
                            transaction: transaction
                        );

                        if (newHoaDonId <= 0)
                        {
                            throw new Exception("Không thể tạo Hóa đơn, ID trả về không hợp lệ.");
                        }

                        // 4. Gọi psInsertCHITIETHOADON với TVP
                        var chiTietParams = new DynamicParameters();
                        chiTietParams.Add("@MAHOADON", newHoaDonId);
                        chiTietParams.Add("@ChiTietList", chiTietTable.AsTableValuedParameter("ChiTietHoaDonType"));

                        await connection.ExecuteAsync(
                            "psInsertCHITIETHOADON",
                            chiTietParams,
                            commandType: CommandType.StoredProcedure,
                            transaction: transaction
                        );

                        // 5. Hoàn tất
                        await transaction.CommitAsync();

                        return Ok(new { success = true, maHoaDon = newHoaDonId, tongTien = tongTien, trangThai = "chờ xử lý" });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        // Log lỗi
                        return StatusCode(500, $"Lỗi khi tạo đơn hàng: {ex.Message}");
                    }
                }
            }
        }


        // Hàm tiện ích lấy PARTY_ID từ JWT
        private long GetCurrentPartyId()
        {
            var partyIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub");
            if (partyIdClaim != null && long.TryParse(partyIdClaim.Value, out long partyId))
            {
                return partyId;
            }
            return 0; // Không tìm thấy
        }
    }
}