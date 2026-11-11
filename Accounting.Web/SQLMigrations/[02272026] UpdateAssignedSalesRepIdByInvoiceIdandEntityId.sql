
CREATE PROCEDURE USP_Accounting_UpdateAssignedSalesRepIdByInvoiceIdandEntityId
(
    @Invoice_Id BIGINT = NULL,
    @Entity_Id  BIGINT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE agd SET agd.AssignedSalesRep_Id = @Entity_Id FROM Accounting_Invoices INV
    INNER JOIN Accounting_PolicyDetails PD ON INV.PolicyDetails_Id = PD.Id AND PD.Status = 1
    INNER JOIN Accounting_AgentDetails agd ON agd.AgentDetail_Id = PD.AgentDetail_Id AND agd.Status = 1
    WHERE INV.Id = @Invoice_Id AND INV.Status = 1;

END
GO