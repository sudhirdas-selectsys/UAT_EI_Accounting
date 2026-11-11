create PROC USP_Accounting_GetAllAmountDueBasedOnTypes      
AS      
BEGIN      
SELECT * FROM Accounting_LU_AmountDueBasedOnType WHERE Status = 1      
END