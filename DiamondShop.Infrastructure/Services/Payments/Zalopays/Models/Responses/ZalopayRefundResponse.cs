using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Infrastructure.Services.Payments.Zalopays.Models.Responses
{
    public class ZalopayRefundResponse
    {
        public int return_code { get; set; }           // 1: Hoàn tiền giao dịch thành công
                                                       // 2: Hoàn tiền thất bại, cần thực hiện lại giao dịch
                                                       // 3: Đang hoàn tiền, gọi query_refund api để lấy trạng thái cuối cùng
        public string return_message { get; set; }      // Thông tin trạng thái
        public int sub_return_code { get; set; }       // Thông tin chi tiết mã lỗi hoàn tiền
        public string sub_return_message { get; set; }  // Thông tin chi tiết trạng thái
        public long refund_id { get; set; }            // Mã giao dịch hoàn tiền của ZaloPay, cần lưu lại để đối chiếu
    }
}
