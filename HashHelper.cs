using System;
using System.Security.Cryptography;
using System.Text;

namespace DoAnCuoiKy
{
    public static class HashHelper
    {
        // Sinh chuỗi hash SHA-256 dạng hex viết thường từ dữ liệu đầu vào.
        public static string CalculateSHA256(string rawData)
        {
            if (string.IsNullOrEmpty(rawData))
            {
                throw new ArgumentNullException(nameof(rawData));
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();

                // Chuyển từng byte sang định dạng hex 2 ký tự để tạo chuỗi hash hoàn chỉnh.
                foreach (byte value in bytes)
                {
                    builder.Append(value.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
