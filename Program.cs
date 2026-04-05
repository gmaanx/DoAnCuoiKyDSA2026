using System;
using System.Windows.Forms;

namespace DoAnCuoiKy
{
    internal static class Program
    {
        /// Điểm khởi tạo ứng dụng WinForms.
        [STAThread]
        static void Main()
        {
            // Thiết lập cấu hình hiển thị chuẩn trước khi mở form chính.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
