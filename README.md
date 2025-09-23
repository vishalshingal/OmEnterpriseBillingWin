CREATE TABLE [dbo].[Customers](
	[ID] [int] NOT NULL,
	[Name] [varchar](50) NULL,
	[Mobile] [numeric](10, 0) NULL,
	[DiscountRate] [int] NULL,
	[Remarks] [varchar](50) NULL,
 CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Items](
	[ItemId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[SalePrice] [decimal](18, 2) NOT NULL,
	[StockQty] [int] NOT NULL,
	[MinStockLevel] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Purchases](
	[PurchaseID] [int] IDENTITY(1,1) NOT NULL,
	[ItemID] [int] NULL,
	[Quantity] [int] NULL,
	[Price] [decimal](10, 2) NULL,
	[Seller] [nvarchar](100) NULL,
	[Date] [datetime] NULL,
	[StakeholderId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[PurchaseID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Purchases]  WITH CHECK ADD  CONSTRAINT [FK_Purchases_Stakeholders] FOREIGN KEY([StakeholderId])
REFERENCES [dbo].[Stakeholders] ([StakeholderId])
GO

ALTER TABLE [dbo].[Purchases] CHECK CONSTRAINT [FK_Purchases_Stakeholders]
GO


CREATE TABLE [dbo].[Sales](
	[SaleID] [int] IDENTITY(1,1) NOT NULL,
	[ItemID] [int] NULL,
	[Quantity] [int] NULL,
	[Price] [decimal](10, 2) NULL,
	[Buyer] [nvarchar](100) NULL,
	[Date] [datetime] NULL,
	[StakeholderId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[SaleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Sales]  WITH CHECK ADD  CONSTRAINT [FK_Sales_Stakeholders] FOREIGN KEY([StakeholderId])
REFERENCES [dbo].[Stakeholders] ([StakeholderId])
GO

ALTER TABLE [dbo].[Sales] CHECK CONSTRAINT [FK_Sales_Stakeholders]
GO


CREATE TABLE [dbo].[Stakeholders](
	[StakeholderId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[ContactNumber] [nvarchar](20) NOT NULL,
	[Address] [nvarchar](200) NOT NULL,
	[Type] [int] NOT NULL,
	[DiscountPercentage] [decimal](5, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[StakeholderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Stakeholders] ADD  DEFAULT ((0)) FOR [DiscountPercentage]
GO


CREATE TABLE [dbo].[Supplier](
	[ID] [int] NOT NULL,
	[Name] [varchar](50) NULL,
	[Remarks] [varchar](100) NULL,
 CONSTRAINT [PK_Supplier] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
