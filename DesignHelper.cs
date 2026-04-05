using ComponentFactory.Krypton.Toolkit;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DoAnCuoiKy
{
    public static class DesignHelper
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        /// Áp dụng vùng hiển thị bo góc cho control và tự cập nhật khi kích thước thay đổi.
        public static void ApplyRoundedCorners(Control control, int radius)
        {
            // Local function giúp đồng bộ lại Region khi control thay đổi kích thước.
            void UpdateRegion(object sender, EventArgs e)
            {
                IntPtr regionHandle = CreateRoundRectRgn(0, 0, control.Width, control.Height, radius, radius);

                try
                {
                    using (Region roundedRegion = Region.FromHrgn(regionHandle))
                    {
                        Region previousRegion = control.Region;
                        control.Region = roundedRegion.Clone();
                        previousRegion?.Dispose();
                    }
                }
                finally
                {
                    DeleteObject(regionHandle);
                }
            }

            UpdateRegion(control, EventArgs.Empty);
            control.Resize += UpdateRegion;
        }

        /// Đăng ký cơ chế placeholder đầy đủ cho một KryptonTextBox.
        public static void AddPlaceholder(this KryptonTextBox textBox, string placeholderText)
        {
            ApplyPlaceholder(textBox, placeholderText);

            textBox.Enter += (sender, e) => RemovePlaceholder(textBox, placeholderText);
            textBox.Leave += (sender, e) => ApplyPlaceholder(textBox, placeholderText);
        }

        /// Gán placeholder khi textbox đang trống hoặc vừa quay lại trạng thái mặc định.
        private static void ApplyPlaceholder(KryptonTextBox textBox, string placeholderText)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text) || textBox.Text == placeholderText)
            {
                textBox.Text = placeholderText;
                textBox.StateCommon.Content.Color1 = Color.Gray;
            }
        }

        /// Gỡ placeholder khi người dùng bắt đầu nhập liệu.
        private static void RemovePlaceholder(KryptonTextBox textBox, string placeholderText)
        {
            if (textBox.Text == placeholderText)
            {
                textBox.Text = string.Empty;
                textBox.StateCommon.Content.Color1 = Color.Black;
            }
        }

        // Kiểm tra textbox hiện đang ở trạng thái placeholder.
        public static bool IsPlaceholderActive(this KryptonTextBox textBox, string placeholderText)
        {
            return textBox.Text == placeholderText;
        }

        /// Xử lý placeholder theo từng ngữ cảnh Enter hoặc Leave để tái sử dụng trong form.
        public static void HandlePlaceholder(KryptonTextBox textBox, string placeholderText, bool isEntering)
        {
            if (isEntering)
            {
                if (textBox.Text == placeholderText)
                {
                    textBox.Text = string.Empty;
                    textBox.StateCommon.Content.Color1 = Color.Black;
                }
            }
            else if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = placeholderText;
                textBox.StateCommon.Content.Color1 = Color.Gray;
            }
        }
    }
}
