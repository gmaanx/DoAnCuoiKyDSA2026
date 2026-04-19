namespace DoAnCuoiKy
{
    public class BlockNode
    {
        private readonly Block _data;
        private BlockNode _next;

        public Block Data { get { return _data; } }

        public BlockNode Next
        {
            get { return _next; }
            set { _next = value; }
        }

        /// Tạo node bao bọc một block trong danh sách liên kết.
        public BlockNode(Block data)
        {
            _data = data;
            _next = null;
        }
    }

    public class LinkedList
    {
        private BlockNode _head;
        private BlockNode _tail;
        private int _count;
        private int _comparisons;

        public BlockNode Head { get { return _head; } }
        public BlockNode Tail { get { return _tail; } }
        public int Count { get { return _count; } }
        public int Comparisons { get { return _comparisons; } }

        /// Khởi tạo danh sách liên kết rỗng để lưu chuỗi block.
        public LinkedList()
        {
            _head = null;
            _tail = null;
            _count = 0;
            _comparisons = 0;
        }

        // Thêm block vào cuối danh sách để bảo toàn thứ tự hình thành chuỗi.
        public void AddLast(Block newBlock)
        {
            BlockNode newNode = new BlockNode(newBlock);

            if (_head == null)
            {
                _head = newNode;
                _tail = newNode;
            }
            else
            {
                _tail.Next = newNode;
                _tail = newNode;
            }

            _count++;
        }

        /// Đưa bộ đếm số phép so sánh về 0 trước khi thực hiện benchmark.
        public void ResetComparisons()
        {
            _comparisons = 0;
        }

        /// Tìm node theo hash bằng duyệt tuyến tính để phục vụ so sánh với HashTable.
        public BlockNode SearchByHash(string hash)
        {
            if (string.IsNullOrEmpty(hash))
            {
                return null;
            }

            BlockNode current = _head;
            while (current != null)
            {
                _comparisons++;
                if (current.Data.Hash == hash)
                {
                    return current;
                }

                current = current.Next;
            }

            return null;
        }
    }
}
