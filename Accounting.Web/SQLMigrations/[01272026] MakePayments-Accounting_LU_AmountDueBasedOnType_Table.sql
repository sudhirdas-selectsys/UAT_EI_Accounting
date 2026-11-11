
CREATE TABLE Accounting_LU_AmountDueBasedOnType
(
    AmountDueBasedOnType_Id TINYINT IDENTITY(1,1) NOT NULL,
    Name VARCHAR(250),
    Status BIT CONSTRAINT DF_Accounting_LU_AmountDueBasedOnType_Status DEFAULT 1,
    CreatedOn DATETIME CONSTRAINT DF_Accounting_LU_AmountDueBasedOnType_CreatedOn DEFAULT GETDATE(),
    UpdatedOn DATETIME,

    CONSTRAINT PK_Accounting_LU_AmountDueBasedOnType 
        PRIMARY KEY (AmountDueBasedOnType_Id),

    CONSTRAINT UK_Accounting_LU_AmountDueBasedOnType_AmountDueBasedOnType_Id 
        UNIQUE (AmountDueBasedOnType_Id)

);
CREATE CLUSTERED INDEX IX_Accounting_LU_AmountDueBasedOnType_AmountDueBasedOnType_Id_Status
ON Accounting_LU_AmountDueBasedOnType
(
    AmountDueBasedOnType_Id ASC,
    Status DESC
);

insert into Accounting_LU_AmountDueBasedOnType (Name)values('Collected'),('Gross')