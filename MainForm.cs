using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;

namespace DoAnCuoiKy
{
    public partial class MainForm : KryptonForm
    {
        private const string GenesisPrevHash = "0";
        private const int MaxImportRows = 50;
        private const string DATA_PLACEHOLDER = "Nhập dữ liệu cho khối mới...";
        private const string SEARCH_PLACEHOLDER = "Nhập mã Hash cần tìm...";

        private LinkedList _blockchain;
        private HashTable _hashTable;
        private int _difficulty;

        /// Khởi tạo form chính, chuẩn bị giao diện và dữ liệu blockchain ban đầu.
        public MainForm()
        {
            InitializeComponent();
            SetupPlaceholders();
            InitializeBlockchainSystem();
        }

        /// Khởi tạo placeholder và trạng thái hiển thị ban đầu cho các ô nhập liệu.
        private void SetupPlaceholders()
        {
            txtBlockData.Text = DATA_PLACEHOLDER;
            txtBlockData.StateCommon.Content.Color1 = Color.Gray;

            txtSearchHash.Text = SEARCH_PLACEHOLDER;
            txtSearchHash.StateCommon.Content.Color1 = Color.Gray;

            txtBlockData.Enter += (s, e) => DesignHelper.HandlePlaceholder(txtBlockData, DATA_PLACEHOLDER, true);
            txtBlockData.Leave += (s, e) => DesignHelper.HandlePlaceholder(txtBlockData, DATA_PLACEHOLDER, false);

            txtSearchHash.Enter += (s, e) => DesignHelper.HandlePlaceholder(txtSearchHash, SEARCH_PLACEHOLDER, true);
            txtSearchHash.Leave += (s, e) => DesignHelper.HandlePlaceholder(txtSearchHash, SEARCH_PLACEHOLDER, false);
        }

        /// Cập nhật thống kê hiện tại của bảng băm lên giao diện.
        private void UpdateHashStats()
        {
            if (_hashTable == null)
            {
                return;
            }

            int collisionCount = _hashTable.GetCollisionCount();

            lblCapacity.Text = $"{_hashTable.Capacity}";
            lblTotalBlocks.Text = $"{_hashTable.Count}";
            lblCollisionCount.Text = $"{collisionCount}";
            lblCollisionCount.ForeColor = collisionCount > 10 ? Color.Red : Color.White;
        }

        /// Khởi tạo blockchain rỗng, hash table và thêm Genesis Block.
        private void InitializeBlockchainSystem()
        {
            _blockchain = new LinkedList();
            _hashTable = new HashTable(100);

            AddNewBlockToSystem("Genesis Block", GenesisPrevHash);
            UpdateHashStats();
        }

        /// Tạo, đào, lưu và hiển thị một block mới trong toàn hệ thống.
        private void AddNewBlockToSystem(string data, string prevHash)
        {
            int newIndex = _blockchain.Count;
            Block newBlock = new Block(newIndex, data, prevHash);

            _difficulty = (int)difficultyUpDown.Value;
            Stopwatch stopwatch = Stopwatch.StartNew();

            newBlock.MineBlock(_difficulty);
            stopwatch.Stop();

            _blockchain.AddLast(newBlock);
            _hashTable.Insert(newBlock.Hash, _blockchain.Tail);

            RenderBlockToUI(newBlock);
            UpdateStatus($"Đã đào Block #{newBlock.Index} thành công! Thời gian: {stopwatch.ElapsedMilliseconds} ms | Nonce: {newBlock.Nonce}", Color.Green);
            UpdateHashStats();
        }

        /// Xử lý thao tác thêm block từ dữ liệu do người dùng nhập.
        private void btnAddBlock_Click(object sender, EventArgs e)
        {
            string dataInput = txtBlockData.Text.Trim();

            if (string.IsNullOrEmpty(dataInput) || dataInput == DATA_PLACEHOLDER)
            {
                UpdateStatus("Dữ liệu khởi tạo khối không được để trống.", Color.Red);
                return;
            }

            AddNewBlockToSystem(dataInput, GetLatestBlockHash());
            txtBlockData.Text = string.Empty;
            ActiveControl = null;
        }

        /// Khôi phục trạng thái hiển thị mặc định trước khi thực hiện thao tác mới.
        private void ResetAllBlocksHighlight()
        {
            foreach (Control ctrl in flpBlockchain.Controls)
            {
                if (ctrl is BlockControl blockCtrl)
                {
                    blockCtrl.ResetSearchHighlight();
                }
            }
        }

        /// Xử lý tìm kiếm block theo hash và điều hướng giao diện tới block tương ứng.
        private void btnSearch_Click(object sender, EventArgs e)
        {
            ResetAllBlocksHighlight();

            string targetHash = txtSearchHash.Text.Trim();
            if (string.IsNullOrEmpty(targetHash) || targetHash == SEARCH_PLACEHOLDER)
            {
                UpdateStatus("Đã làm mới kết quả hiển thị.", Color.Black);
                return;
            }

            BlockNode resultNode = _hashTable.Search(targetHash);
            if (resultNode != null)
            {
                Block foundBlock = resultNode.Data;
                UpdateStatus($"Tìm thấy: Block #{foundBlock.Index} | Data: {foundBlock.Data}", Color.Blue);

                if (flpBlockchain.Controls.Count > foundBlock.Index)
                {
                    BlockControl targetCtrl = (BlockControl)flpBlockchain.Controls[foundBlock.Index];
                    targetCtrl.HighlightSearch();
                    flpBlockchain.ScrollControlIntoView(targetCtrl);
                }
            }
            else
            {
                UpdateStatus("Mã Hash không tồn tại trong hệ thống.", Color.Red);
            }
        }

        /// Duyệt toàn bộ chuỗi để kiểm tra hash nội tại và liên kết PrevHash giữa các block.
        private void btnValidateChain_Click(object sender, EventArgs e)
        {
            bool isValid = true;
            BlockNode current = _blockchain.Head;
            int errorIndex = -1;
            string expectedPrevHash = GenesisPrevHash;

            while (current != null)
            {
                Block block = current.Data;

                // Nếu dữ liệu bên trong block thay đổi mà hash không được sinh lại, chuỗi sẽ mất toàn vẹn.
                if (block.Hash != block.CalculateHash())
                {
                    isValid = false;
                    errorIndex = block.Index;
                    break;
                }

                // Mỗi block phải tham chiếu đúng hash của block đứng ngay trước nó.
                if (block.PrevHash != expectedPrevHash)
                {
                    isValid = false;
                    errorIndex = block.Index;
                    break;
                }

                expectedPrevHash = block.Hash;
                current = current.Next;
            }

            if (isValid)
            {
                UpdateStatus("Xác thực thành công. Toàn vẹn chuỗi được đảm bảo.", Color.Green);

                foreach (Control control in flpBlockchain.Controls)
                {
                    if (control is BlockControl blockControl)
                    {
                        blockControl.ResetSearchHighlight();
                    }
                }
            }
            else
            {
                UpdateStatus($"Phát hiện bất thường dữ liệu tại Block #{errorIndex}.", Color.Red);

                if (errorIndex >= 0 && errorIndex < flpBlockchain.Controls.Count)
                {
                    BlockControl errorControl = (BlockControl)flpBlockchain.Controls[errorIndex];
                    errorControl.MarkAsInvalid();
                    flpBlockchain.ScrollControlIntoView(errorControl);
                }
            }
        }

        /// Tạo control hiển thị cho block mới và thêm vào danh sách trên giao diện.
        private void RenderBlockToUI(Block block)
        {
            BlockControl newBlockCard = new BlockControl();
            newBlockCard.BindData(block);

            flpBlockchain.Controls.Add(newBlockCard);
            flpBlockchain.ScrollControlIntoView(newBlockCard);
        }

        /// Cập nhật thông điệp trạng thái chung cho người dùng.
        private void UpdateStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }

        /// Xuất toàn bộ blockchain hiện tại ra file CSV theo thứ tự từ đầu đến cuối chuỗi.
        private void btnExport_Click(object sender, EventArgs e)
        {
            if (_blockchain.Count == 0)
            {
                UpdateStatus("Chuỗi rỗng, không có dữ liệu để xuất!", Color.Orange);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "CSV file (*.csv)|*.csv", FileName = "MyBlockchain.csv" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(sfd.FileName))
                        {
                            sw.WriteLine("Index,Timestamp,Data,PrevHash,Hash");

                            BlockNode current = _blockchain.Head;
                            while (current != null)
                            {
                                Block b = current.Data;
                                sw.WriteLine($"{b.Index},{b.Timestamp:yyyy-MM-dd HH:mm:ss},{b.Data},{b.PrevHash},{b.Hash}");
                                current = current.Next;
                            }
                        }

                        UpdateStatus($"Đã xuất dữ liệu thành công ra file {Path.GetFileName(sfd.FileName)}", Color.Green);
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus($"Lỗi khi xuất file: {ex.Message}", Color.Red);
                    }
                }
            }
        }

        /// Nhập dữ liệu từ CSV, mỗi dòng hợp lệ sẽ được chuyển thành một block mới.
        private void btnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "CSV file (*.csv)|*.csv", Title = "Chọn Dataset từ Kaggle" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        flpBlockchain.SuspendLayout();
                        int importCount = 0;

                        using (StreamReader sr = new StreamReader(ofd.FileName))
                        {
                            // Bỏ qua dòng header của file CSV.
                            sr.ReadLine();
                            string line;

                            while ((line = sr.ReadLine()) != null)
                            {
                                string[] columns = line.Split(',');
                                if (columns.Length > 0)
                                {
                                    string kaggleData = columns[0].Trim();
                                    AddNewBlockToSystem(kaggleData, GetLatestBlockHash());
                                    importCount++;

                                    // Giới hạn số block import để giữ giao diện phản hồi tốt trong WinForms.
                                    if (importCount >= MaxImportRows)
                                    {
                                        UpdateStatus($"Đã đạt giới hạn {MaxImportRows} dòng import để bảo vệ UI.", Color.Orange);
                                        break;
                                    }
                                }
                            }
                        }

                        flpBlockchain.ResumeLayout();
                        if (flpBlockchain.Controls.Count > 0)
                        {
                            flpBlockchain.ScrollControlIntoView(flpBlockchain.Controls[flpBlockchain.Controls.Count - 1]);
                        }

                        UpdateStatus($"Đã Import thành công {importCount} dữ liệu từ file CSV", Color.Green);
                    }
                    catch (Exception ex)
                    {
                        flpBlockchain.ResumeLayout();
                        UpdateStatus($"Lỗi khi đọc file: {ex.Message}", Color.Red);
                    }
                }
            }
        }

        /// Lấy hash của block cuối chuỗi để gắn cho block kế tiếp.
        private string GetLatestBlockHash()
        {
            return _blockchain.Tail != null ? _blockchain.Tail.Data.Hash : GenesisPrevHash;
        }
    }
}
