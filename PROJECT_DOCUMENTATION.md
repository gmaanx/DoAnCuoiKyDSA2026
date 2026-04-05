# Tai lieu mo ta project `DoAnCuoiKy`

## 1. Muc tieu project

Project mo phong mot he thong blockchain don gian bang WinForms. Ung dung cho phep:

- Tao block moi tu du lieu nguoi dung nhap.
- Dao block bang co che `Proof of Work` don gian.
- Luu chuoi block bang danh sach lien ket tu cai dat.
- Tra cuu block theo `Hash` bang bang bam tu cai dat.
- Kiem tra tinh toan ven cua toan bo chuoi.
- Mo phong truong hop du lieu bi thay doi trai phep.
- Xuat va nhap du lieu qua file CSV.

Project phu hop de trinh bay cac khai niem:

- Blockchain co ban.
- Hash va lien ket giua cac block.
- Proof of Work.
- Linked List.
- Hash Table.
- Tinh toan ven du lieu.

## 2. Tong quan kien truc

Project duoc chia thanh 3 nhom thanh phan chinh:

### 2.1. Lop mo hinh du lieu

- `Block`: dai dien cho mot khoi trong blockchain.
- `BlockNode`: node cua danh sach lien ket, dung de luu `Block`.
- `LinkedList`: danh sach lien ket don tu cai dat de luu toan bo chuoi block.
- `HashEntry`: phan tu trong bucket cua bang bam.
- `HashTable`: bang bam tu cai dat de tra cuu nhanh block theo `Hash`.

### 2.2. Lop tro giup

- `HashHelper`: sinh ma bam SHA-256.
- `DesignHelper`: chua cac helper phuc vu giao dien, placeholder va bo goc control.

### 2.3. Lop giao dien

- `MainForm`: man hinh chinh, dieu phoi toan bo nghiep vu.
- `BlockControl`: the hien thi mot block tren giao dien.
- `Program`: diem khoi dong ung dung.

## 3. Luong xu ly chinh

### 3.1. Khoi tao he thong

Khi ung dung chay:

1. `Program.Main()` mo `MainForm`.
2. `MainForm` goi `SetupPlaceholders()` de khoi tao placeholder cho cac o nhap.
3. `MainForm` goi `InitializeBlockchainSystem()`.
4. He thong tao:
   - mot `LinkedList` rong,
   - mot `HashTable` rong,
   - mot `Genesis Block`.
5. `Genesis Block` duoc dua vao danh sach lien ket va bang bam.

### 3.2. Them block moi

Khi nguoi dung nhan nut them block:

1. Doc du lieu tu `txtBlockData`.
2. Kiem tra du lieu rong hoac dang la placeholder.
3. Lay `Hash` cua block cuoi lam `PrevHash`.
4. Tao `Block` moi.
5. Goi `MineBlock(difficulty)` de dao block.
6. Them block vao cuoi `LinkedList`.
7. Dua `Hash` cua block vao `HashTable`.
8. Tao `BlockControl` moi de hien thi len giao dien.
9. Cap nhat thong tin thong ke va trang thai.

### 3.3. Tim block theo hash

Khi nguoi dung nhan nut tim kiem:

1. Xoa cac highlight cu.
2. Lay chuoi hash can tim.
3. Goi `_hashTable.Search(hash)`.
4. Neu tim thay:
   - lay `BlockNode`,
   - truy cap `Block`,
   - highlight `BlockControl` tuong ung,
   - cuon giao dien toi block do.
5. Neu khong tim thay, hien thi thong bao loi.

### 3.4. Kiem tra tinh toan ven chuoi

Khi nguoi dung nhan nut validate:

1. Duyet tu `Head` den `Tail` cua `LinkedList`.
2. Voi moi block, kiem tra:
   - `block.Hash == block.CalculateHash()`
   - `block.PrevHash == expectedPrevHash`
3. Neu co block sai:
   - danh dau block loi,
   - hien thi thong bao bat thuong.
4. Neu toan bo hop le:
   - hien thi thong bao thanh cong,
   - dua giao dien ve trang thai binh thuong.

### 3.5. Mo phong du lieu bi sua

Trong `BlockControl`, nguoi dung co the double click vao o data de mo khoa chinh sua. Khi roi khoi o nhap:

- du lieu `_data` cua `Block` goc bi thay doi qua reflection,
- `Hash` khong duoc tinh lai,
- lan `Validate Chain` tiep theo se phat hien block bi sai.

Day la co che mo phong tampering de minh hoa tinh chat "du lieu bi doi se lam vo tinh toan ven cua chuoi".

### 3.6. Import va export CSV

#### Export

- Duyet toan bo blockchain tu `Head`.
- Ghi cac cot: `Index, Timestamp, Data, PrevHash, Hash`.

#### Import

- Doc file CSV bo qua dong dau.
- Lay du lieu o cot dau tien cua moi dong.
- Moi dong hop le se tao thanh mot block moi.
- He thong gioi han so dong import de tranh tai giao dien WinForms.

## 4. Mo ta chi tiet tung class

## 4.1. `Block`

File: `Block.cs`

### Trach nhiem

Dai dien cho mot block trong chuoi blockchain.

### Thuoc tinh chinh

- `Index`: vi tri cua block trong chuoi.
- `Timestamp`: thoi diem tao block.
- `Data`: noi dung du lieu cua block.
- `PrevHash`: hash cua block truoc do.
- `Hash`: hash hien tai cua block.
- `Nonce`: gia tri thu dung trong qua trinh dao block.

### Phuong thuc chinh

- `CalculateHash()`
  - Tao chuoi raw data tu cac truong cua block.
  - Goi `HashHelper.CalculateSHA256(...)`.
  - Tra ve hash SHA-256 dang chuoi hex.

- `MineBlock(int difficulty)`
  - Tao chuoi muc tieu gom `difficulty` ky tu `0`.
  - Lap tang `Nonce` cho toi khi `Hash` bat dau bang chuoi muc tieu.
  - Mo phong co che `Proof of Work`.

### Y nghia

Day la lop trung tam cua bai toan blockchain. Moi thay doi trong du lieu cua block se dan den thay doi hash.

## 4.2. `HashHelper`

File: `HashHelper.cs`

### Trach nhiem

Dong vai tro utility de sinh ma bam SHA-256.

### Phuong thuc chinh

- `CalculateSHA256(string rawData)`
  - Nhan du lieu dang chuoi.
  - Chuyen sang bytes UTF-8.
  - Tinh hash SHA-256.
  - Chuyen ket qua sang chuoi hex viet thuong.

### Y nghia

Tat ca hash trong he thong deu phu thuoc vao helper nay. Neu ham nay thay doi, toan bo co che hash cua he thong se thay doi theo.

## 4.3. `BlockNode`

File: `LinkedList.cs`

### Trach nhiem

La node trong danh sach lien ket, dung de bao goi mot doi tuong `Block`.

### Thuoc tinh chinh

- `Data`: block hien tai.
- `Next`: node ke tiep.

### Y nghia

Cho phep cai dat `LinkedList` thu cong thay vi dung `System.Collections.Generic.LinkedList<T>`.

## 4.4. `LinkedList`

File: `LinkedList.cs`

### Trach nhiem

Quan ly chuoi block theo dung thu tu hinh thanh.

### Thuoc tinh chinh

- `Head`: block dau chuoi.
- `Tail`: block cuoi chuoi.
- `Count`: so block hien co.

### Phuong thuc chinh

- `AddLast(Block newBlock)`
  - Tao `BlockNode` moi.
  - Neu danh sach rong: gan cho ca `Head` va `Tail`.
  - Neu khong rong: noi vao cuoi danh sach va cap nhat `Tail`.
  - Tang `Count`.

### Ly do su dung Linked List

- Them block moi vao cuoi danh sach nhanh.
- Mo phong cau truc chuoi theo thu tu tu nhien.
- De trinh bay ky thuat cai dat cau truc du lieu nen khong phu thuoc collection co san.

## 4.5. `HashEntry`

File: `HashTable.cs`

### Trach nhiem

La phan tu luu trong moi bucket cua bang bam.

### Thuoc tinh chinh

- `Key`: hash cua block.
- `Value`: `BlockNode` tuong ung trong `LinkedList`.
- `Next`: phan tu tiep theo trong cung bucket khi xay ra collision.

### Y nghia

Lop nay phuc vu co che chaining de xu ly va cham trong bang bam.

## 4.6. `HashTable`

File: `HashTable.cs`

### Trach nhiem

Cung cap co che tim block theo `Hash` voi do phuc tap trung binh gan `O(1)`.

### Cau truc noi bo

- Mang `_buckets` luu cac bucket.
- Moi bucket co the la:
  - `null`, neu chua co phan tu,
  - hoac mot chuoi lien ket `HashEntry`, neu co va cham.

### Phuong thuc chinh

- `Insert(string key, BlockNode node)`
  - Tinh chi so bucket qua `GetBucketIndex(key)`.
  - Neu bucket rong thi gan truc tiep.
  - Neu bucket da co du lieu thi chen dau danh sach chaining.

- `Search(string key)`
  - Tinh bucket index.
  - Duyet chuoi `HashEntry` trong bucket.
  - Neu tim thay key trung khop thi tra ve `BlockNode`.

- `GetCollisionCount()`
  - Dem so bucket dang duoc su dung.
  - Collision = tong phan tu - tong bucket da su dung.

### Vi sao `HashTable` luu `BlockNode` thay vi luu `Block`

Vi `BlockNode` la doi tuong dang ton tai trong `LinkedList`. Cach nay giup:

- lien ket truc tiep giua bang bam va chuoi block,
- tranh sao chep doi tuong,
- truy xuat block nhanh qua `node.Data`.

### Y nghia trong project

Neu chi dung `LinkedList`, thao tac tim mot block theo hash se ton `O(n)`.
Khi them `HashTable`, thao tac tim kiem hash co the dat trung binh `O(1)`.

## 4.7. `DesignHelper`

File: `DesignHelper.cs`

### Trach nhiem

Tap trung cac tien ich cho giao dien:

- bo goc control,
- xu ly placeholder cho `KryptonTextBox`.

### Phuong thuc chinh

- `ApplyRoundedCorners(Control control, int radius)`
  - Goi Windows API `CreateRoundRectRgn`.
  - Tao `Region` bo goc cho control.
  - Tu dong cap nhat lai khi control thay doi kich thuoc.

- `AddPlaceholder(...)`, `HandlePlaceholder(...)`
  - Quan ly placeholder cho textbox.
  - Doi mau chu va noi dung theo trang thai focus.

## 4.8. `BlockControl`

File: `BlockControl.cs`

### Trach nhiem

La `UserControl` dai dien cho mot block tren giao dien.

### Chuc nang chinh

- `BindData(Block block)`
  - Dua du lieu block len cac label/textbox.

- `MarkAsInvalid()`
  - To mau block loi khi validate that bai.

- `HighlightSearch()`
  - To mau block duoc tim thay.

- `ResetSearchHighlight()`
  - Dua block ve trang thai hien thi mac dinh.

- `txtData_DoubleClick(...)`
  - Mo khoa o data de cho phep sua.

- `txtData_Leave(...)`
  - Cap nhat truc tiep field `_data` cua `Block` qua reflection.
  - Co chu y: day la hanh vi phuc vu mo phong, khong phai cach cap nhat du lieu cho he thong thuc te.

## 4.9. `MainForm`

File: `MainForm.cs`

### Trach nhiem

La lop dieu phoi trung tam cua ung dung.

### Nhung cong viec chinh

- Khoi tao blockchain va hash table.
- Quan ly su kien giao dien.
- Them, tim, validate, import, export block.
- Dong bo du lieu len giao dien.
- Cap nhat thong tin thong ke.

### Cac phuong thuc quan trong

- `SetupPlaceholders()`
- `InitializeBlockchainSystem()`
- `AddNewBlockToSystem(...)`
- `ResetAllBlocksHighlight()`
- `btnAddBlock_Click(...)`
- `btnSearch_Click(...)`
- `btnValidateChain_Click(...)`
- `btnExport_Click(...)`
- `btnImport_Click(...)`
- `GetLatestBlockHash()`

## 4.10. `Program`

File: `Program.cs`

### Trach nhiem

La diem vao cua ung dung WinForms.

### Cong viec

- Bat visual styles.
- Cau hinh text rendering.
- Khoi chay `MainForm`.

## 5. Quan he giua cac class

Mo hinh phu thuoc chinh:

- `MainForm` su dung `LinkedList` de luu blockchain.
- `MainForm` su dung `HashTable` de tim block theo hash.
- `LinkedList` luu `BlockNode`.
- `BlockNode` bao boc `Block`.
- `HashTable` luu `HashEntry`.
- `HashEntry` tham chieu toi `BlockNode`.
- `Block` su dung `HashHelper` de tinh hash.
- `MainForm` tao `BlockControl` de hien thi tung `Block`.
- `BlockControl` su dung `DesignHelper` de ap dung giao dien.

## 6. Do phuc tap thoi gian

### Them block

- Tao block: `O(1)`
- Dao block: phu thuoc `difficulty`, khong co gioi han co dinh
- Them vao linked list: `O(1)`
- Chen vao hash table: trung binh `O(1)`

### Tim block theo hash

- Trung binh: `O(1)`
- Xau nhat: `O(n)` neu collision rat cao

### Validate chain

- `O(n)` vi phai duyet toan bo danh sach

### Export CSV

- `O(n)`

### Import CSV`

- `O(k)` voi `k` la so dong duoc import thuc te

## 7. Diem can luu y ky thuat

### 7.1. Reflection trong `BlockControl`

Project dang dung reflection de thay doi field private `_data` cua `Block`.
Dieu nay hop ly cho muc tieu mo phong "du lieu bi can thiep".
Trong ung dung thuc te, cach nay khong nen dung lam co che cap nhat nghiep vu.

### 7.2. Import CSV don gian

Phan import dang tach cot bang `Split(',')`.
Cach nay du cho demo, nhung se khong xu ly day du cac truong hop CSV phuc tap nhu:

- gia tri co dau phay ben trong dau ngoac kep,
- du lieu nhieu cot dac biet,
- ky tu xuong dong trong mot o.

Neu sau nay can nang cap, nen dung thu vien parser CSV chuyen dung.

### 7.3. HashTable khong co resize

`HashTable` hien tai co capacity co dinh.
Neu so block tang lon, collision se tang theo.
Day la lua chon hop ly cho bai tap cau truc du lieu, nhung neu mo rong thuc te thi nen bo sung co che rehash / resize.

### 7.4. Timestamp dung `DateTime.Now`

Moi block lay thoi gian tao theo may cuc bo.
Neu can tinh dong nhat cao hon, co the can chuyen sang `DateTime.UtcNow`.
Trong pham vi demo hien tai, cach dang dung la du.

## 8. Goi y cach thuyet trinh project

Neu ban can trinh bay project, co the di theo thu tu sau:

1. Giai thich blockchain gom nhung gi.
2. Trinh bay `Block` va vai tro cua `Hash`, `PrevHash`, `Nonce`.
3. Giai thich vi sao dung `LinkedList` de luu chuoi.
4. Giai thich vi sao them `HashTable` de tim kiem nhanh theo hash.
5. Demo them block va dao block.
6. Demo tim kiem theo hash.
7. Demo sua data truc tiep trong `BlockControl`.
8. Chay `Validate Chain` de cho thay chuoi bi vo toan ven.
9. Demo export/import CSV.

## 9. Ket luan

Project nay la mot mo hinh blockchain mang tinh giao duc, ket hop:

- mo phong blockchain co ban,
- cai dat thu cong cau truc du lieu,
- giao dien WinForms de minh hoa truc quan.

Gia tri chinh cua project nam o viec giup nguoi hoc nhin thay moi lien he giua:

- du lieu,
- hash,
- lien ket chuoi,
- proof of work,
- va tra cuu nhanh bang bang bam.
