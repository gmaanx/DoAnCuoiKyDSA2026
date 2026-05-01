using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;

namespace DoAnCuoiKy
{
    public partial class MainForm : KryptonForm
    {
        private const bool EnableSearchComparisonBenchmark = true; // Chỉnh thành true để đo và ghi log
        private const string GenesisPrevHash = "0000000000000000000000000000000000000000000000000000000000000000";
        private const int MaxImportRows = 50;
        private static readonly Size InitialClientSize = new Size(1400, 710);
        private const string DATA_PLACEHOLDER = "Nhập dữ liệu cho khối mới...";
        private const string SEARCH_PLACEHOLDER = "Nhập Hash hoặc nội dung block...";
        
        private LinkedList _blockchain;
        private HashTable _hashTable;
        private int _difficulty;
        private bool _isMining;
        private readonly AutoCompleteStringCollection _searchSuggestions = new AutoCompleteStringCollection();

        /// Khởi tạo form chính, chuẩn bị giao diện và dữ liệu blockchain ban đầu.
        public MainForm()
        {
            InitializeComponent();
            ClientSize = InitialClientSize;
            SetupPlaceholders();
            InitializeBlockchainSystem();

            if (EnableSearchComparisonBenchmark)
            {
                RunSearchComparisonBenchmarkSeries();
            }
        }

        /// Khởi tạo placeholder và trạng thái hiển thị ban đầu cho các ô nhập liệu.
        private void SetupPlaceholders()
        {
            txtBlockData.Text = DATA_PLACEHOLDER;
            txtBlockData.StateCommon.Content.Color1 = Color.Gray;

            txtSearchHash.Text = SEARCH_PLACEHOLDER;
            txtSearchHash.StateCommon.Content.Color1 = Color.Gray;
            txtSearchHash.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtSearchHash.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtSearchHash.AutoCompleteCustomSource = _searchSuggestions;

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

        /// Đưa toàn bộ hệ thống về trạng thái ban đầu và chỉ giữ lại Genesis Block.
        private void ResetBlockchainSystem()
        {
            flpBlockchain.SuspendLayout();

            try
            {
                flpBlockchain.Controls.Clear();
                ResetAllBlocksHighlight();

                _blockchain = new LinkedList();
                _hashTable = new HashTable(100);

                AddNewBlockToSystem("Genesis Block", GenesisPrevHash);
                UpdateHashStats();

                txtBlockData.Text = DATA_PLACEHOLDER;
                txtBlockData.StateCommon.Content.Color1 = Color.Gray;
                txtSearchHash.Text = SEARCH_PLACEHOLDER;
                txtSearchHash.StateCommon.Content.Color1 = Color.Gray;
                ActiveControl = null;

                RefreshSearchSuggestions();
                UpdateStatus("Da reset du lieu ve trang thai ban dau.", Color.Green);
            }
            finally
            {
                flpBlockchain.ResumeLayout();
            }
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
            RefreshSearchSuggestions();
            UpdateStatus($"Đã đào Block #{newBlock.Index} thành công! Thời gian: {stopwatch.ElapsedMilliseconds} ms | Nonce: {newBlock.Nonce}", Color.Green);
            UpdateHashStats();
        }

        /// Thực hiện đào block trên luồng nền để giao diện vẫn phản hồi khi difficulty cao.
        private async Task AddNewBlockToSystemAsync(string data, string prevHash)
        {
            int newIndex = _blockchain.Count;
            Block newBlock = new Block(newIndex, data, prevHash);

            _difficulty = (int)difficultyUpDown.Value;
            Stopwatch stopwatch = Stopwatch.StartNew();
            SetMiningUiState(true, $"Đang đào Block #{newIndex}... Vui lòng chờ.");

            try
            {
                await Task.Run(() => newBlock.MineBlock(_difficulty));
            }
            finally
            {
                stopwatch.Stop();
                SetMiningUiState(false);
            }

            _blockchain.AddLast(newBlock);
            _hashTable.Insert(newBlock.Hash, _blockchain.Tail);

            RenderBlockToUI(newBlock);
            RefreshSearchSuggestions();
            UpdateStatus($"Đã đào Block #{newBlock.Index} thành công! Thời gian: {stopwatch.ElapsedMilliseconds} ms | Nonce: {newBlock.Nonce}", Color.Green);
            UpdateHashStats();
        }

        /// Xử lý thao tác thêm block từ dữ liệu do người dùng nhập.
        private async void btnAddBlock_Click(object sender, EventArgs e)
        {
            if (_isMining)
            {
                return;
            }

            string dataInput = txtBlockData.Text.Trim();

            if (string.IsNullOrWhiteSpace(dataInput) || dataInput == DATA_PLACEHOLDER)
            {
                UpdateStatus("Dữ liệu khởi tạo khối không được để trống.", Color.Red);
                return;
            }

            await AddNewBlockToSystemAsync(dataInput, GetLatestBlockHash());
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

            Stopwatch searchStopwatch = Stopwatch.StartNew();
            SearchResult result = FindBlockBySmartQuery(targetHash);
            searchStopwatch.Stop();

            if (result != null)
            {
                UpdateStatus(
                    $"Tìm thấy trong {searchStopwatch.Elapsed.TotalMilliseconds:F2} ms theo {result.MatchMode}: Block #{result.Block.Index} | Data: {result.Block.Data}",
                    Color.Blue);

                if (string.Equals(result.MatchMode, "Hash chính xác", StringComparison.Ordinal))
                {
                    LogSearchComparisons(result.Block.Hash, searchStopwatch.Elapsed);
                }

                if (flpBlockchain.Controls.Count > result.Block.Index)
                {
                    BlockControl targetCtrl = (BlockControl)flpBlockchain.Controls[result.Block.Index];
                    targetCtrl.HighlightSearch();
                    flpBlockchain.ScrollControlIntoView(targetCtrl);
                }
            }
            else
            {
                UpdateStatus($"Không tìm thấy block phù hợp sau {searchStopwatch.Elapsed.TotalMilliseconds:F2} ms.", Color.Red);
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
                        blockControl.MarkAsValid();
                    }
                }
            }
            else
            {
                UpdateStatus($"Chain INVALID at Block {errorIndex}", Color.Red);

                for (int i = 0; i < flpBlockchain.Controls.Count; i++)
                {
                    if (!(flpBlockchain.Controls[i] is BlockControl blockControl))
                    {
                        continue;
                    }

                    if (i < errorIndex)
                    {
                        blockControl.MarkAsValid();
                    }
                    else if (i == errorIndex)
                    {
                        blockControl.MarkAsInvalid();
                    }
                    else
                    {
                        blockControl.MarkAsAffectedInvalid();
                    }
                }

                if (errorIndex >= 0 && errorIndex < flpBlockchain.Controls.Count)
                {
                    BlockControl errorControl = (BlockControl)flpBlockchain.Controls[errorIndex];
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
        private async void btnImport_Click(object sender, EventArgs e)
        {
            if (_isMining)
            {
                UpdateStatus("Vui lòng đợi quá trình đào hiện tại hoàn tất trước khi import.", Color.Orange);
                return;
            }

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
                                if (string.IsNullOrWhiteSpace(line))
                                {
                                    continue;
                                }

                                string[] columns = line.Split(',');
                                if (columns.Length > 0)
                                {
                                    string kaggleData = columns[0].Trim();

                                    if (string.IsNullOrWhiteSpace(kaggleData))
                                    {
                                        continue;
                                    }

                                    await AddNewBlockToSystemAsync(kaggleData, GetLatestBlockHash());
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

        /// Reset du lieu da import tu CSV trong ung dung ve trang thai mac dinh.
        private void resetBtn_Click(object sender, EventArgs e)
        {
            if (_isMining)
            {
                UpdateStatus("Không thể reset trong khi đang đào block.", Color.Orange);
                return;
            }

            ResetBlockchainSystem();
        }

        /// Khóa các thao tác thay đổi dữ liệu trong lúc mining đang chạy trên nền.
        private void SetMiningUiState(bool isMining, string statusMessage = null)
        {
            _isMining = isMining;
            btnAddBlock.Enabled = !isMining;
            btnImport.Enabled = !isMining;
            resetBtn.Enabled = !isMining;
            difficultyUpDown.Enabled = !isMining;
            txtBlockData.Enabled = !isMining;

            if (!string.IsNullOrWhiteSpace(statusMessage))
            {
                UpdateStatus(statusMessage, Color.DarkOrange);
            }
        }

        /// Làm mới danh sách gợi ý tìm kiếm từ toàn bộ hash và data hiện có trong chuỗi.
        private void RefreshSearchSuggestions()
        {
            _searchSuggestions.Clear();

            if (_blockchain == null)
            {
                return;
            }

            HashSet<string> uniqueSuggestions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            BlockNode current = _blockchain.Head;

            while (current != null)
            {
                Block block = current.Data;

                if (!string.IsNullOrWhiteSpace(block.Hash) && uniqueSuggestions.Add(block.Hash))
                {
                    _searchSuggestions.Add(block.Hash);
                }

                if (!string.IsNullOrWhiteSpace(block.Data) && uniqueSuggestions.Add(block.Data))
                {
                    _searchSuggestions.Add(block.Data);
                }

                current = current.Next;
            }
        }

        /// Thực hiện tìm kiếm thông minh: ưu tiên hash chính xác, sau đó hash theo tiền tố, rồi đến data.
        private SearchResult FindBlockBySmartQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return null;
            }

            string normalizedQuery = query.Trim();
            BlockNode exactHashNode = _hashTable.Search(normalizedQuery);
            if (exactHashNode != null)
            {
                return new SearchResult(exactHashNode.Data, "Hash chính xác");
            }

            SearchResult hashPrefixResult = FindBlockByPredicate(
                block => block.Hash.StartsWith(normalizedQuery, StringComparison.OrdinalIgnoreCase),
                "Hash gần đúng");
            if (hashPrefixResult != null)
            {
                return hashPrefixResult;
            }

            SearchResult exactDataResult = FindBlockByPredicate(
                block => string.Equals(block.Data, normalizedQuery, StringComparison.OrdinalIgnoreCase),
                "Data chính xác");
            if (exactDataResult != null)
            {
                return exactDataResult;
            }

            return FindBlockByPredicate(
                block => block.Data.IndexOf(normalizedQuery, StringComparison.OrdinalIgnoreCase) >= 0,
                "Data gần đúng");
        }

        /// Duyệt chuỗi để tìm block đầu tiên thỏa điều kiện lọc.
        private SearchResult FindBlockByPredicate(Func<Block, bool> predicate, string matchMode)
        {
            BlockNode current = _blockchain.Head;
            while (current != null)
            {
                if (predicate(current.Data))
                {
                    return new SearchResult(current.Data, matchMode);
                }

                current = current.Next;
            }

            return null;
        }

        /// Ghi số phép so sánh của LinkedList và HashTable ra Output window cho truy vết.
        private void LogSearchComparisons(string targetHash, TimeSpan elapsed)
        {
            _blockchain.ResetComparisons();
            _hashTable.ResetComparisons();

            BlockNode linkedListResult = _blockchain.SearchByHash(targetHash);
            BlockNode hashTableResult = _hashTable.Search(targetHash);

            double ratio = _hashTable.Comparisons == 0
                ? 0
                : (double)_blockchain.Comparisons / _hashTable.Comparisons;

            Debug.WriteLine(
                $"[SearchComparison] n={_blockchain.Count}, targetHash={targetHash}, " +
                $"LinkedListComparisons={_blockchain.Comparisons}, " +
                $"HashTableComparisons={_hashTable.Comparisons}, Ratio={ratio:F2}, " +
                $"ElapsedMs={elapsed.TotalMilliseconds:F4}, " +
                $"LinkedListFound={(linkedListResult != null)}, HashTableFound={(hashTableResult != null)}");
        }

        /// Chạy nhanh bộ benchmark chuẩn để ghi ra Output window các mốc n được yêu cầu.
        private void RunSearchComparisonBenchmarkSeries()
        {
            int[] benchmarkSizes = { 10, 50, 100, 500, 1000, 5000, 10000 };

            Debug.WriteLine("========== SEARCH COMPARISON BENCHMARK ==========");
            foreach (int blockCount in benchmarkSizes)
            {
                RunSearchComparisonBenchmark(blockCount);
            }
            Debug.WriteLine("=================================================");
        }

        /// Tạo dữ liệu in-memory và đo số phép so sánh khi tìm block ở vị trí n/2.
        private void RunSearchComparisonBenchmark(int blockCount)
        {
            LinkedList benchmarkList = new LinkedList();
            HashTable benchmarkTable = new HashTable(100);
            string prevHash = GenesisPrevHash;
            string targetHash = string.Empty;
            int targetIndex = blockCount / 2;

            for (int i = 0; i < blockCount; i++)
            {
                Block block = new Block(i, $"BENCHMARK-{i}", prevHash);
                benchmarkList.AddLast(block);
                benchmarkTable.Insert(block.Hash, benchmarkList.Tail);

                if (i == targetIndex)
                {
                    targetHash = block.Hash;
                }

                prevHash = block.Hash;
            }

            benchmarkList.ResetComparisons();
            benchmarkTable.ResetComparisons();

            benchmarkList.SearchByHash(targetHash);
            benchmarkTable.Search(targetHash);

            double ratio = benchmarkTable.Comparisons == 0
                ? 0
                : (double)benchmarkList.Comparisons / benchmarkTable.Comparisons;

            Debug.WriteLine(
                $"[Benchmark] n={blockCount}, targetIndex={targetIndex}, " +
                $"LinkedListComparisons={benchmarkList.Comparisons}, " +
                $"HashTableComparisons={benchmarkTable.Comparisons}, Ratio={ratio:F2}, " +
                $"Collisions={benchmarkTable.GetCollisionCount()}");
        }

        /// Lấy hash của block cuối chuỗi để gắn cho block kế tiếp.
        private string GetLatestBlockHash()
        {
            return _blockchain.Tail != null ? _blockchain.Tail.Data.Hash : GenesisPrevHash;
        }

        private sealed class SearchResult
        {
            public SearchResult(Block block, string matchMode)
            {
                Block = block;
                MatchMode = matchMode;
            }

            public Block Block { get; }
            public string MatchMode { get; }
        }
    }
}
