
CREATE TABLE Accounting_BillLineItems
(
    BillLineItems_Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT UK_Accounting_BillLineItems_BillLineItems_Id UNIQUE,  
    Bill_Id BIGINT CONSTRAINT FK_Accounting_BillLineItems_Bill_Id REFERENCES Accounting_Bill (Bill_Id),    
    Invoice_Id BIGINT CONSTRAINT FK_Accounting_BillLineItems_Invoice_Id REFERENCES Accounting_Invoices (Id), 
    LineItem_Id TINYINT CONSTRAINT FK_Accounting_BillLineItems_LineItem_Id REFERENCES Accounting_LU_LineItems (LineItem_Id),  
	InvoiceLineItem_Id BIGINT CONSTRAINT FK_Accounting_BillLineItems_InvoiceLineItem_Id REFERENCES Accounting_InvoiceDetails(Id),
    AppliedAmount FLOAT NOT NULL,      
    Status BIT NOT NULL CONSTRAINT DF_Accounting_BillLineItems_Status DEFAULT (1),
    CreatedOn DATETIME NOT NULL CONSTRAINT DF_Accounting_BillLineItems_CreatedOn DEFAULT (GETDATE()),
    UpdatedOn DATETIME
);

CREATE Clustered INDEX CLU_Accounting_BillLineItems_BillLineItems_Id_Status ON Accounting_BillLineItems (Status desc,BillLineItems_Id asc);
