# DoAnCuoiKy

Ung dung WinForms mo phong mot blockchain co ban, ket hop:

- `Block` va lien ket `PrevHash`
- `Proof of Work` don gian voi `Nonce`
- `LinkedList` tu cai dat de luu chuoi block
- `HashTable` tu cai dat de tim block theo hash
- giao dien WinForms dung Krypton Toolkit de minh hoa truc quan

Project phu hop cho muc dich hoc tap, demo mon Cau truc du lieu va Giai thuat, va thuyet trinh ve cach blockchain hoat dong o muc do nen tang.

## Chuc nang hien co

- Tao block moi tu du lieu nguoi dung nhap
- Tu dong mine block theo `difficulty`
- Hien thi tung block tren giao dien
- Tim block theo `Hash`
- Validate toan bo chuoi de kiem tra tinh toan ven
- Mo phong tampering bang cach sua `Data` cua block tren UI
- Import du lieu tu file CSV
- Export blockchain ra file CSV
- Reset du lieu da nap ve trang thai ban dau voi Genesis Block
- Hien thi thong ke `Capacity`, `Total Blocks`, `Collision Count`

## Cong nghe va phu thuoc

- .NET Framework `4.7.2`
- WinForms
- `ComponentFactory.Krypton.Toolkit`

Project dang tham chieu bo DLL Krypton bang `HintPath` trong [`DoAnCuoiKy.csproj`](d:/UEH/Algorithm/DoAnCuoiKy/DoAnCuoiKy.csproj), nen khi clone sang may khac ban can:

- cap nhat lai `HintPath`, hoac
- cai dat / copy dung bo DLL Krypton vao vi tri phu hop

## Cau truc project

### Core model

- [`Block.cs`](d:/UEH/Algorithm/DoAnCuoiKy/Block.cs): mo hinh 1 block, chua `Index`, `Timestamp`, `Data`, `PrevHash`, `Hash`, `Nonce`
- [`LinkedList.cs`](d:/UEH/Algorithm/DoAnCuoiKy/LinkedList.cs): danh sach lien ket don tu cai dat de luu blockchain
- [`HashTable.cs`](d:/UEH/Algorithm/DoAnCuoiKy/HashTable.cs): bang bam tu cai dat de tra cuu block theo hash
- [`HashHelper.cs`](d:/UEH/Algorithm/DoAnCuoiKy/HashHelper.cs): helper tinh SHA-256

### UI

- [`MainForm.cs`](d:/UEH/Algorithm/DoAnCuoiKy/MainForm.cs): form chinh, dieu phoi nghiep vu
- [`MainForm.Designer.cs`](d:/UEH/Algorithm/DoAnCuoiKy/MainForm.Designer.cs): layout giao dien
- [`BlockControl.cs`](d:/UEH/Algorithm/DoAnCuoiKy/BlockControl.cs): user control dai dien 1 block
- [`DesignHelper.cs`](d:/UEH/Algorithm/DoAnCuoiKy/DesignHelper.cs): helper cho bo goc, placeholder, va xu ly UI
- [`Program.cs`](d:/UEH/Algorithm/DoAnCuoiKy/Program.cs): diem khoi dong ung dung

## Cach he thong hoat dong

### 1. Khoi tao

Khi mo app:

1. `Program.Main()` chay `MainForm`.
2. `MainForm` thiet lap placeholder va kich thuoc ban dau.
3. He thong tao moi:
   - `LinkedList`
   - `HashTable`
   - `Genesis Block`
4. Genesis Block duoc dua vao chuoi va render len giao dien.

### 2. Them block

Khi nguoi dung nhap du lieu va nhan nut them:

1. Lay noi dung tu `txtBlockData`
2. Lay `Hash` cua block cuoi lam `PrevHash`
3. Tao `Block` moi
4. Mine block voi `difficulty` hien tai
5. Them block vao cuoi `LinkedList`
6. Chen vao `HashTable`
7. Tao `BlockControl` de hien thi
8. Cap nhat thong ke va status

### 3. Tim block theo hash

`MainForm` goi `_hashTable.Search(hash)` de tim nhanh `BlockNode`, sau do:

- hien thong tin block tim thay
- highlight block tren UI
- cuon den block tuong ung

### 4. Validate chain

App duyet tu dau den cuoi chuoi va kiem tra:

- `block.Hash == block.CalculateHash()`
- `block.PrevHash == expectedPrevHash`

Neu sai, block loi se bi danh dau tren giao dien.

### 5. Mo phong tampering

Trong [`BlockControl.cs`](d:/UEH/Algorithm/DoAnCuoiKy/BlockControl.cs), nguoi dung co the double-click vao o data de sua noi dung block. Phan nay dung reflection de thay doi field private `_data` trong [`Block.cs`](d:/UEH/Algorithm/DoAnCuoiKy/Block.cs) ma khong tinh lai hash, nham minh hoa viec du lieu bi can thiep se lam vo tinh toan ven cua blockchain.

## CSV workflow

### Import

- Chon file CSV bang `OpenFileDialog`
- Bo qua dong header
- Moi dong hop le lay cot dau tien lam du lieu block
- Moi block moi duoc mine va them vao blockchain
- Gioi han `MaxImportRows = 50` de tranh UI bi cham

### Export

App ghi ra cac cot:

- `Index`
- `Timestamp`
- `Data`
- `PrevHash`
- `Hash`

### Reset

Nut `Reset` khong xoa file CSV tren o dia. No reset du lieu da nap trong ung dung ve trang thai ban dau:

- clear UI
- tao lai `LinkedList`
- tao lai `HashTable`
- tao lai Genesis Block

## Do phuc tap co ban

- Them block vao cuoi chuoi: `O(1)` cho phan luu tru, chua tinh chi phi mining
- Tim block theo hash: trung binh `O(1)`, xau nhat `O(n)` khi collision cao
- Validate chain: `O(n)`
- Export CSV: `O(n)`
- Import CSV: `O(k)` voi `k` la so dong duoc import

## Cach build va chay

### Bang Visual Studio

1. Mo file solution [`DoAnCuoiKy.sln`](d:/UEH/Algorithm/DoAnCuoiKy/DoAnCuoiKy.sln)
2. Dam bao tham chieu Krypton hop le
3. Chon `Debug` hoac `Release`
4. Run project

### Bang command line

Neu dung `dotnet build`, cau hinh da duoc kiem tra trong moi truong hien tai voi:

```powershell
dotnet build -p:GenerateResourceMSBuildArchitecture=CurrentArchitecture
```

File output mac dinh:

- `bin\Debug\DoAnCuoiKy.exe`

## Mot so gioi han hien tai

- Import CSV dang dung `Split(',')`, chua xu ly day du CSV phuc tap co dau phay trong chuoi
- `HashTable` co `capacity` co dinh, chua co resize / rehash
- Tampering dang dung reflection, phu hop de demo nhung khong phai cach thiet ke nghiep vu cho he thong that
- Project phu thuoc vao duong dan DLL Krypton cu the trong file csproj

## Goi y demo project

Mot flow demo gon:

1. Mo app va giai thich Genesis Block
2. Them vai block moi
3. Tim block theo hash
4. Sua data cua mot block tren UI
5. Chay validate de thay chuoi bi loi
6. Import them du lieu tu CSV
7. Export blockchain ra CSV
8. Reset de quay ve trang thai ban dau

## Tac gia va muc dich

Day la project hoc thuat mo phong blockchain o muc co ban, trong tam la:

- hieu cau truc block va lien ket hash
- thay ro vai tro cua Proof of Work
- ap dung `LinkedList` va `HashTable` tu cai dat
- trinh bay truc quan bang WinForms
