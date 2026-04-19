using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace DoAnCuoiKy
{
    public partial class BlockControl : UserControl
    {
        private static readonly FieldInfo BlockDataField = typeof(Block).GetField("_data", BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly Color _blockFound = Color.FromArgb(127, 183, 126);
        private readonly Color _titleFound = Color.FromArgb(47, 107, 63);
        private readonly Color _validBlock = Color.FromArgb(198, 232, 197);
        private readonly Color _validTitle = Color.FromArgb(39, 92, 54);
        private readonly Color _invalidBlock = Color.FromArgb(217, 104, 104);
        private readonly Color _invalidAffectedBlock = Color.FromArgb(241, 192, 192);
        private readonly Color _invalidTitle = Color.FromArgb(126, 37, 37);

        private Block _boundBlock;

        /// Khởi tạo control hiển thị block và áp dụng cấu hình giao diện dùng chung.
        public BlockControl()
        {
            InitializeComponent();
            DesignHelper.ApplyRoundedCorners(this, 20);
        }

        /// Đồng bộ dữ liệu của block vào các thành phần hiển thị.
        public void BindData(Block block)
        {
            if (block == null)
            {
                return;
            }

            _boundBlock = block;
            lblIndex.Text = $"Block #{_boundBlock.Index}";
            lblTimestamp.Text = _boundBlock.Timestamp.ToString("dd/MM/yyyy HH:mm:ss");
            txtData.Text = _boundBlock.Data;
            txtHash.Text = _boundBlock.Hash;
            txtPrevHash.Text = _boundBlock.PrevHash;

            ResetSearchHighlight();
        }

        /// Đánh dấu block hiện tại là không hợp lệ trong kết quả kiểm tra chuỗi.
        public void MarkAsInvalid()
        {
            BackColor = _invalidBlock;
            lblIndex.ForeColor = _invalidTitle;
        }

        /// Đánh dấu block bị ảnh hưởng gián tiếp do liên kết chuỗi phía trước đã sai.
        public void MarkAsAffectedInvalid()
        {
            BackColor = _invalidAffectedBlock;
            lblIndex.ForeColor = _invalidTitle;
        }

        /// Đánh dấu block đang ở trạng thái hợp lệ trong lần kiểm tra hiện tại.
        public void MarkAsValid()
        {
            BackColor = _validBlock;
            lblIndex.ForeColor = _validTitle;
        }

        /// Cho phép mô phỏng thay đổi dữ liệu trực tiếp để phục vụ bài toán kiểm tra tính toàn vẹn.
        private void txtData_Leave(object sender, EventArgs e)
        {
            if (!txtData.ReadOnly && _boundBlock != null)
            {
                if (BlockDataField != null)
                {
                    BlockDataField.SetValue(_boundBlock, txtData.Text);
                }

                txtData.ReadOnly = true;
            }
        }

        /// Mở chế độ chỉnh sửa dữ liệu để mô phỏng hành vi tamper.
        private void txtData_DoubleClick(object sender, EventArgs e)
        {
            txtData.ReadOnly = false;
            MessageBox.Show("Đã mở khóa Data. Hãy sửa nội dung và nhấn nút Validate Chain để xem kết quả!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// Tô sáng block khi kết quả tìm kiếm trùng với hash cần tra cứu.
        public void HighlightSearch()
        {
            BackColor = _blockFound;
            lblIndex.ForeColor = _titleFound;
        }

        /// Trả block về trạng thái hiển thị mặc định.
        public void ResetSearchHighlight()
        {
            BackColor = Color.White;
            lblIndex.ForeColor = Color.Black;
        }
    }
}
