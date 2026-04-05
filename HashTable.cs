using System;

namespace DoAnCuoiKy
{
    public class HashEntry
    {
        private readonly string _key;
        private readonly BlockNode _value;
        private HashEntry _next;

        public string Key { get { return _key; } }
        public BlockNode Value { get { return _value; } }

        public HashEntry Next
        {
            get { return _next; }
            set { _next = value; }
        }

        /// Tạo một phần tử trong bucket để lưu cặp key và node tương ứng.
        public HashEntry(string key, BlockNode value)
        {
            _key = key;
            _value = value;
            _next = null;
        }
    }

    public class HashTable
    {
        private readonly int _capacity;
        private readonly HashEntry[] _buckets;
        private int _count;

        public int Capacity { get { return _capacity; } }
        public int Count { get { return _count; } }

        /// Khởi tạo bảng băm với số lượng bucket cố định.
        public HashTable(int capacity = 100)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException("Capacity must be greater than zero.");
            }

            _capacity = capacity;
            _buckets = new HashEntry[_capacity];
            _count = 0;
        }

        /// Ánh xạ key vào bucket index bằng mã băm nội bộ của .NET.
        private int GetBucketIndex(string key)
        {
            if (key == null)
            {
                return 0;
            }

            return Math.Abs(key.GetHashCode()) % _capacity;
        }

        /// Chèn node vào bảng băm để hỗ trợ tra cứu block theo hash.
        public void Insert(string key, BlockNode node)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty.");
            }

            int index = GetBucketIndex(key);
            HashEntry newEntry = new HashEntry(key, node);

            if (_buckets[index] == null)
            {
                _buckets[index] = newEntry;
            }
            else
            {
                // Xử lý va chạm bằng chained bucket.
                newEntry.Next = _buckets[index];
                _buckets[index] = newEntry;
            }

            _count++;
        }

        /// Tìm block node theo key hash trong bucket tương ứng.
        public BlockNode Search(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            int index = GetBucketIndex(key);
            HashEntry current = _buckets[index];

            while (current != null)
            {
                if (current.Key == key)
                {
                    return current.Value;
                }

                current = current.Next;
            }

            return null;
        }

        // Collision được tính bằng số phần tử vượt quá số bucket đang được sử dụng.
        public int GetCollisionCount()
        {
            int occupiedBuckets = 0;

            for (int i = 0; i < _capacity; i++)
            {
                if (_buckets[i] != null)
                {
                    occupiedBuckets++;
                }
            }

            return _count - occupiedBuckets;
        }
    }
}
