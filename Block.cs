using System;

namespace DoAnCuoiKy
{
    public class Block
    {
        private readonly int _index;
        private readonly DateTime _timestamp;
        private readonly string _data;
        private readonly string _prevHash;
        private string _hash;
        private int _nonce;

        public int Index { get { return _index; } }
        public DateTime Timestamp { get { return _timestamp; } }
        public string Data { get { return _data; } }
        public string PrevHash { get { return _prevHash; } }
        public string Hash { get { return _hash; } }
        public int Nonce { get { return _nonce; } }

        /// Khởi tạo block mới và sinh hash ban đầu từ trạng thái hiện tại.
        public Block(int index, string data, string prevHash)
        {
            _index = index;
            _timestamp = DateTime.Now;
            _data = data;
            _prevHash = prevHash;
            _nonce = 0;
            _hash = CalculateHash();
        }

        // Tái tạo mã băm từ toàn bộ trạng thái hiện tại của block.
        public string CalculateHash()
        {
            string rawData = $"{_index}-{_timestamp:yyyyMMddHHmmss}-{_data}-{_prevHash}-{_nonce}";
            return HashHelper.CalculateSHA256(rawData);
        }

        /// Thực hiện Proof of Work bằng cách tăng nonce cho đến khi hash đạt độ khó yêu cầu.
        public void MineBlock(int difficulty)
        {
            string target = new string('0', difficulty);

            // Tăng nonce cho đến khi hash thỏa điều kiện độ khó.
            while (!_hash.StartsWith(target, StringComparison.Ordinal))
            {
                _nonce++;
                _hash = CalculateHash();
            }
        }
    }
}
