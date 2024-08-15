-- Tạo cơ sở dữ liệu
CREATE DATABASE PK_SHOP COLLATE SQL_Latin1_General_CP1_CI_AS;

GO

-- Sử dụng cơ sở dữ liệu PKSHOP
USE PK_SHOP
GO

-- Tạo bảng Users
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(100) UNIQUE NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    FullName NVARCHAR(255),
    PhoneNumber NVARCHAR(20),
    Address NVARCHAR(255),
    Role NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE());
GO
-- Tạo bảng Categories
CREATE TABLE Categories (
    CategoryId INT PRIMARY KEY IDENTITY,
    CategoryName NVARCHAR(100) NOT NULL
);
GO

-- Tạo bảng Products
CREATE TABLE Products (
    ProductId INT PRIMARY KEY IDENTITY,
    ProductName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(18, 2) NOT NULL,
    StockQuantity INT NOT NULL,
    CategoryId INT NOT NULL,
    Branch NVARCHAR(100),
    ImageUrl NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId)
);
GO

-- Tạo bảng Orders
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY IDENTITY,
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    OrderDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(18, 2) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
);
GO

-- Tạo bảng OrderDetails
CREATE TABLE OrderDetails (
    OrderDetailId INT PRIMARY KEY IDENTITY,
    OrderId INT FOREIGN KEY REFERENCES Orders(OrderId),
    ProductId INT FOREIGN KEY REFERENCES Products(ProductId),
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18, 2) NOT NULL,
    TotalPrice DECIMAL(18, 2) NOT NULL,
);
GO

-- Tạo bảng Cart
CREATE TABLE Cart (
    CartId INT PRIMARY KEY IDENTITY,
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
);
GO

-- Tạo bảng CartItems
CREATE TABLE CartItems (
    CartItemId INT PRIMARY KEY IDENTITY,
    CartId INT FOREIGN KEY REFERENCES Cart(CartId),
    ProductId INT FOREIGN KEY REFERENCES Products(ProductId),
    Quantity INT NOT NULL,
);
GO

-- Tạo bảng Reviews
CREATE TABLE Reviews (
    ReviewId INT PRIMARY KEY IDENTITY,
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    ProductId INT FOREIGN KEY REFERENCES Products(ProductId),
    Rating INT NOT NULL,
    Comment NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
);
GO

-- Tạo bảng Messages
CREATE TABLE Messages (
    MessageId INT PRIMARY KEY IDENTITY,
    SenderId INT FOREIGN KEY REFERENCES Users(UserId),
    ReceiverId INT FOREIGN KEY REFERENCES Users(UserId),
    Content NVARCHAR(MAX) NOT NULL,
    SentAt DATETIME DEFAULT GETDATE(),
    ReadAt DATETIME
);
GO
CREATE TABLE ProductSale (
    SaleId INT PRIMARY KEY IDENTITY,
    ProductId INT FOREIGN KEY REFERENCES Products(ProductId),
    OriginalPrice DECIMAL(18, 2) NOT NULL, -- Lưu giá gốc từ bảng Products
    DiscountPercentage DECIMAL(5, 2) NOT NULL, -- Ví dụ: 10.00 cho 10% giảm giá
    SalePrice DECIMAL(18, 2) NOT NULL, -- Giá sau giảm giá
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO

-- Chèn dữ liệu mẫu vào bảng Categories
INSERT INTO Categories (CategoryName)
VALUES 
(N'Adapter, cáp sạc'),
(N'Tai nghe'),
(N'Loa Bluetooth'),
(N'Ốp lưng');
GO

-- Chèn dữ liệu mới vào bảng Products với các chuỗi Unicode
INSERT INTO Products (ProductName, Description, Price, StockQuantity, CategoryId, Branch, ImageUrl, CreatedAt, UpdatedAt) VALUES 
(N'Adapter Sạc Type C PD 20W', N'Adapter Sạc Type C PD 20W Anker PowerPort III Nano A2633', 400000, 600, 1, N'Anker', N'./img/Anker/Adapter_capsac/Adapter Sạc Type C PD 20W Anker PowerPort III Nano A2633.jpg', GETDATE(), GETDATE()),
(N'Tai Nghe Bluetooth True Wireless', N'Tai Nghe Bluetooth True Wireless Anker SoundCore Liberty 2 Pro A3909', 3500000, 200, 2, N'Anker', N'./img/Anker/Tai_Nghe/Tai Nghe Bluetooth True Wireless Anker SoundCore Liberty 2 Pro A3909.jpg', GETDATE(), GETDATE()),
(N'Loa Bluetooth Sony SRS-XP500', N'Loa Bluetooth Sony SRS-XP500', 1600000, 150, 3, N'Sony', N'./img/Sony/Loa/Loa Bluetooth Sony SRS-XP500.jpg', GETDATE(), GETDATE()),
(N'Loa thanh Sony HT-S100F 120W', N'Loa thanh Sony HT-S100F 120W', 1750000, 150, 3, N'Sony', N'./img/Sony/Loa/Loa thanh Sony HT-S100F 120W.jpg', GETDATE(), GETDATE()),
(N'Tai nghe Bluetooth True Wireless', N'Tai nghe Bluetooth True Wireless Sony WF-C500', 2100000, 300, 2, N'Sony', N'./img/Sony/Tai_Nghe/Tai nghe Bluetooth True Wireless Sony WF-C500.jpeg', GETDATE(), GETDATE()),
(N'Tai nghe Bluetooth True Wireless Pro', N'Tai nghe Bluetooth True Wireless Sony WF-C700N', 3900000, 150, 2, N'Sony', N'./img/Sony/Tai_Nghe/Tai nghe Bluetooth True Wireless Sony WF-C700N.jpg', GETDATE(), GETDATE()),
(N'Tai nghe Chụp Tai Sony MDR', N'Tai nghe Chụp Tai Sony MDR - ZX110AP', 1140000, 150, 2, N'Sony', N'./img/Sony/Tai_Nghe/Tai nghe Chụp Tai Sony MDR - ZX110AP.jpg', GETDATE(), GETDATE()),
(N'Tai nghe Có Dây Sony MDR', N'Tai nghe Có Dây Sony MDR-EX15AP', 352000, 320, 2, N'Sony', N'./img/Sony/Tai_Nghe/Tai nghe Có Dây Sony MDR-EX15AP.jpg', GETDATE(), GETDATE()),
(N'Tai nghe Có Dây Sony DMX', N'Tai nghe Có Dây Sony MDR-EX155AP', 200000, 320, 2, N'Sony', N'./img/Sony/Tai_Nghe/Tai nghe Có Dây Sony MDR-EX155AP.jpg', GETDATE(), GETDATE()),
(N'Adapter Sạc USB 18W OPPO', N'Adapter Sạc USB 18W OPPO OP92KAUH', 300000, 100, 1, N'OPPO', N'./img/Oppo/Adapter_capsac/Adapter Sạc USB 18W OPPO OP92KAUH.jpg', GETDATE(), GETDATE()),
(N'Tai nghe Bluetooth True Wireless', N'Tai nghe Bluetooth True Wireless OPPO ENCO Air 3 ETE31', 500000, 300, 2, N'OPPO', N'./img/Oppo/Tai_nghe/Tai nghe Bluetooth True Wireless OPPO ENCO Air 3 ETE31.jpg', GETDATE(), GETDATE()),
(N'Tai nghe Bluetooth True Wireless Pro', N'Tai nghe Bluetooth True Wireless OPPO ENCO Buds 2 ETE41', 1750000, 150, 2, N'OPPO', N'./img/Oppo/Tai_nghe/Tai nghe Bluetooth True Wireless OPPO ENCO Buds 2 ETE41.jpg', GETDATE(), GETDATE()),
(N'Tai nghe Có Dây OPPO MH151', N'Tai nghe Có Dây OPPO MH151', 250000, 170, 2, N'OPPO', N'./img/Oppo/Tai_nghe/Tai nghe Có Dây OPPO MH151.jpg', GETDATE(), GETDATE()),
(N'Tai nghe Có Dây OPPO MH320', N'Tai nghe Có Dây OPPO MH320', 250000, 170, 2, N'OPPO', N'./img/Oppo/Tai_nghe/Tai nghe Có Dây OPPO MH320.jpg', GETDATE(), GETDATE()),
(N'Adapter chuyển đổi Type C', N'Adapter chuyển đổi Type C 4 in 1 Xmobile DS122F', 500000, 170, 1, N'Xmobile', N'./img/XMobile/Adapter_capsac/Adapter chuyển đổi Type C 4 in 1 Xmobile DS122F.jpg', GETDATE(), GETDATE()),
(N'Adapter Sạc Type C PD GaN 30W', N'Adapter Sạc Type C PD GaN 30W Xmobile DS230', 600000, 200, 1, N'Xmobile', N'./img/XMobile/Adapter_capsac/Adapter Sạc Type C PD GaN 30W Xmobile DS230.jpg', GETDATE(), GETDATE()),
(N'Loa Bluetooth JBL Flip 6', N'Loa Bluetooth JBL Flip 6', 2599000, 200, 3, N'JBL', N'./img/JBL/Loa/Loa Bluetooth JBL Flip 6.jpg', GETDATE(), GETDATE()),
(N'Loa Bluetooth JBL Go Essential', N'Loa Bluetooth JBL Go Essential', 690000, 100, 3, N'JBL', N'./img/JBL/Loa/Loa Bluetooth JBL Go Essential.jpg', GETDATE(), GETDATE()),
(N'Loa Bluetooth JBL Go 3', N'Loa Bluetooth JBL Go 3', 249000, 150, 3, N'JBL', N'./img/JBL/Loa/Loa Bluetooth JBL Go 3.jpg', GETDATE(), GETDATE()),
(N'Loa Bluetooth JBL Partybox Encore', N'Loa Bluetooth JBL Partybox Encore', 5900000, 100, 3, N'JBL', N'./img/JBL/Loa/Loa Bluetooth JBL Partybox Encore 2Mic.jpg', GETDATE(), GETDATE()),
(N'Pin sạc dự phòng Polymer 5000mAh', N'Pin sạc dự phòng Polymer 5000mAh Không dây Magnetic Type C 12W Anker MagGo A1611', 1150000, 400, 1, N'Anker', N'./img/Anker/Sac_Du_Phong/Pin sạc dự phòng Polymer 5000mAh Không dây Magnetic Type C 12W Anker MagGo A1611.jpg', GETDATE(), GETDATE()),
(N'Loa Bluetooth JBL Partybox On', N'Loa Bluetooth JBL Partybox On The Go', 6390000, 55, 3, N'JBL', N'./img/JBL/Loa/Loa Bluetooth JBL Partybox On The Go.jpg', GETDATE(), GETDATE()),
(N'Loa Bluetooth JBL Partybox 110', N'Loa Bluetooth JBL Partybox 110', 9350000, 99, 3, N'JBL', N'./img/JBL/Loa/Loa Bluetooth JBL Partybox 110.jpg', GETDATE(), GETDATE()),
(N'Loa Bluetooth JBL Pulse 5', N'Loa Bluetooth JBL Pulse 5', 6150000, 200, 3, N'JBL', N'./img/JBL/Loa/Loa Bluetooth JBL Pulse 5.jpg', GETDATE(), GETDATE()),
(N'Loa Bluetooth JBL Xtreme 3', N'Loa Bluetooth JBL Xtreme 3', 5700000, 300, 3, N'JBL', N'./img/JBL/Loa/Loa Bluetooth JBL Xtreme 3.jpg', GETDATE(), GETDATE()),
(N'Ốp lưng Redmi 9C Nhựa dẻo', N'Ốp lưng Redmi 9C Nhựa dẻo', 40000, 250, 4, N'Xiaomi', N'./img/Xiaomi/Op_lung/Ốp lưng Redmi 9C Nhựa dẻo.jpg', GETDATE(), GETDATE()),
(N'Ốp lưng Xiaomi Poco C40 Nhựa', N'Ốp lưng Xiaomi Poco C40 Nhựa', 80000, 300, 4, N'Xiaomi', N'./img/Xiaomi/Op_lung/Ốp lưng Xiaomi Poco C40 Nhựa.jpg', GETDATE(), GETDATE()),
(N'Ốp lưng Xiaomi Redmi A2+ Nhựa dẻo', N'Ốp lưng Xiaomi Redmi A2+ Nhựa dẻo', 30000, 150, 4, N'Xiaomi', N'./img/Xiaomi/Op_lung/Ốp lưng Xiaomi Redmi A2+ Nhựa dẻo.jpg', GETDATE(), GETDATE()),
(N'Ốp lưng Xiaomi Redmi A2 Nhựa dẻo TPU', N'Ốp lưng Xiaomi Redmi A2 Nhựa dẻo TPU JM Cons Shock', 90000, 200, 4, N'Xiaomi', N'./img/Xiaomi/Op_lung/Ốp lưng Xiaomi Redmi A2 Nhựa dẻo TPU JM Cons Shock.jpg', GETDATE(), GETDATE()),
(N'Ốp lưng Xiaomi Redmi 10A Nhựa dẻo', N'Ốp lưng Xiaomi Redmi 10A Nhựa dẻo', 50000, 250, 4, N'Xiaomi', N'./img/Xiaomi/Op_lung/Ốp lưng Xiaomi Redmi 10A Nhựa dẻo.jpg', GETDATE(), GETDATE()),
(N'Ốp lưng Xiaomi Redmi 12 Silicone', N'Ốp lưng Xiaomi Redmi 12 Silicone JM Candy', 70000, 350, 4, N'Xiaomi', N'./img/Xiaomi/Op_lung/Ốp lưng Xiaomi Redmi 12 Silicone JM Candy.jpg', GETDATE(), GETDATE());
GO

--thêm dữ liệu mẫu cho productSale
SET IDENTITY_INSERT [dbo].[ProductSale] ON
INSERT INTO [dbo].[ProductSale] ([SaleId], [ProductId], [OriginalPrice], [DiscountPercentage], [SalePrice], [StartDate], [EndDate], [CreatedAt], [UpdatedAt]) VALUES (2, 1, CAST(400000.00 AS Decimal(18, 2)), CAST(20.00 AS Decimal(5, 2)), CAST(320000.00 AS Decimal(18, 2)), N'2024-08-06 22:36:00', N'2024-08-30 22:36:00', N'2024-08-10 22:36:11', N'2024-08-10 22:36:11')
INSERT INTO [dbo].[ProductSale] ([SaleId], [ProductId], [OriginalPrice], [DiscountPercentage], [SalePrice], [StartDate], [EndDate], [CreatedAt], [UpdatedAt]) VALUES (3, 9, CAST(200000.00 AS Decimal(18, 2)), CAST(10.00 AS Decimal(5, 2)), CAST(180000.00 AS Decimal(18, 2)), N'2024-08-10 22:37:00', N'2024-08-25 22:37:00', N'2024-08-10 22:37:20', N'2024-08-10 22:37:20')
INSERT INTO [dbo].[ProductSale] ([SaleId], [ProductId], [OriginalPrice], [DiscountPercentage], [SalePrice], [StartDate], [EndDate], [CreatedAt], [UpdatedAt]) VALUES (4, 30, CAST(30000.00 AS Decimal(18, 2)), CAST(15.00 AS Decimal(5, 2)), CAST(25500.00 AS Decimal(18, 2)), N'2024-08-14 22:37:00', N'2024-09-08 22:37:00', N'2024-08-10 22:37:34', N'2024-08-10 22:37:34')
INSERT INTO [dbo].[ProductSale] ([SaleId], [ProductId], [OriginalPrice], [DiscountPercentage], [SalePrice], [StartDate], [EndDate], [CreatedAt], [UpdatedAt]) VALUES (5, 21, CAST(1150000.00 AS Decimal(18, 2)), CAST(15.00 AS Decimal(5, 2)), CAST(977500.00 AS Decimal(18, 2)), N'2024-08-08 22:38:00', N'2024-09-08 22:38:00', N'2024-08-10 22:38:18', N'2024-08-10 22:38:18')
INSERT INTO [dbo].[ProductSale] ([SaleId], [ProductId], [OriginalPrice], [DiscountPercentage], [SalePrice], [StartDate], [EndDate], [CreatedAt], [UpdatedAt]) VALUES (6, 14, CAST(250000.00 AS Decimal(18, 2)), CAST(10.00 AS Decimal(5, 2)), CAST(225000.00 AS Decimal(18, 2)), N'2024-08-08 22:38:00', N'2024-09-01 22:38:00', N'2024-08-10 22:38:36', N'2024-08-10 22:38:36')
SET IDENTITY_INSERT [dbo].[ProductSale] OFF