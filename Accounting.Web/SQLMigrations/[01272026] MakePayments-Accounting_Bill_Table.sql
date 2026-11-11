CREATE TABLE Accounting_Bill
(
    Bill_Id BIGINT IDENTITY(1,1) NOT NULL  CONSTRAINT UK_Accounting_Bill_Bill_Id UNIQUE,
    EntityType_Id TINYINT CONSTRAINT FK_Accounting_Bill_EntityType_Id REFERENCES Accounting_LU_EntityTypes (EntityType_Id),
	[Entity_Id] BIGINT,
    Due FLOAT NULL,
    Amount_Applied FLOAT NOT NULL,
	ExternalBillIdentifier BIGINT null,
    Status BIT NOT NULL CONSTRAINT DF_Accounting_Bill_Status DEFAULT (1),
    CreatedOn DATETIME NOT NULL CONSTRAINT DF_Accounting_Bill_CreatedOn DEFAULT (GETDATE()),
    UpdatedOn DATETIME
);
Create Clustered Index CLU_Accounting_Bill_Bill_Id_Status on Accounting_Bill(Status desc,Bill_Id asc)
