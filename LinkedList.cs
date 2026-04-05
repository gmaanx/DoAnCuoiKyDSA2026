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

        public BlockNode Head { get { return _head; } }
        public BlockNode Tail { get { return _tail; } }
        public int Count { get { return _count; } }

        /// Khởi tạo danh sách liên kết rỗng để lưu chuỗi block.
        public LinkedList()
        {
            _head = null;
            _tail = null;
            _count = 0;
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
    }
}
