update Accounting_LU_LineItems set name='Surplus Lines', Code='SurplusLines' where LineItem_Id=5 and status=1
go

update Accounting_LU_BillingType_LineItems_Directions set status=1 where BillingType_LineItems_Directions_Id=4 and LineItems_Directions_Id=5
go

insert into Accounting_LU_LineItems(Name,LineItemType_Id,Description,Code)
values('House Commission',2,'House Commission received from Agency/Broker in case of Agency Bill','HouseCommission');
go

insert into Accounting_LU_LineItems_Directions(LineItem_Id,Direction_Id)
values
(15,1);
go

insert into Accounting_LU_BillingType_LineItems_Directions(BillingType_Id,LineItems_Directions_Id)
values
(1,19);
go
