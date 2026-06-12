-- ============================================================
--  MIGRATION: Thêm bảng Orders và OrderDetails
--  Chạy trong SSMS sau khi đã có database NoiThatCaoCap
-- ============================================================

USE NoiThatCaoCap;
GO

-- Bảng Orders
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Orders')
BEGIN
    CREATE TABLE Orders (
        Id               INT IDENTITY(1,1) PRIMARY KEY,
        UserId           NVARCHAR(450)     NOT NULL,
        OrderDate        DATETIME2         NOT NULL DEFAULT GETDATE(),
        ShippingAddress  NVARCHAR(MAX)     NOT NULL,
        ReceiverName     NVARCHAR(256)     NOT NULL,
        ReceiverPhone    NVARCHAR(50)      NOT NULL,
        Note             NVARCHAR(MAX)     NULL,
        TotalPrice       DECIMAL(18,2)     NOT NULL,
        Status           INT               NOT NULL DEFAULT 0,  -- 0=Pending,1=Confirmed,2=Shipping,3=Delivered,4=Cancelled

        CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId)
            REFERENCES AspNetUsers(Id) ON DELETE CASCADE
    );
    PRINT 'Tạo bảng Orders thành công';
END
ELSE
    PRINT 'Bảng Orders đã tồn tại, bỏ qua.';
GO

-- Bảng OrderDetails
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OrderDetails')
BEGIN
    CREATE TABLE OrderDetails (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        OrderId         INT               NOT NULL,
        ProductId       INT               NOT NULL,
        ProductName     NVARCHAR(256)     NOT NULL,
        ProductImageUrl NVARCHAR(MAX)     NULL,
        UnitPrice       DECIMAL(18,2)     NOT NULL,
        Quantity        INT               NOT NULL,

        CONSTRAINT FK_OrderDetails_Orders   FOREIGN KEY (OrderId)
            REFERENCES Orders(Id) ON DELETE CASCADE,
        CONSTRAINT FK_OrderDetails_Products FOREIGN KEY (ProductId)
            REFERENCES Products(Id) ON DELETE NO ACTION
    );
    PRINT 'Tạo bảng OrderDetails thành công';
END
ELSE
    PRINT 'Bảng OrderDetails đã tồn tại, bỏ qua.';
GO

PRINT '✓ Migration hoàn tất!';
GO
