using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Accounting.SchedulerJobs.Abstract;
using Accounting.SchedulerJobs.Enum;
using Accounting.SchedulerJobs.Models;
using Accounting.SchedulerJobs.Models.MakePayments;
using Accounting.SchedulerJobs.Utilities;
using Newtonsoft.Json;

namespace Accounting.SchedulerJobs.Jobs
{
    public class MakePaymentBillProcessJobThreadOne : ISchedulerJob
    {
        private readonly ISchedulerRepositoryService schedulerRepositoryService;
        private readonly ICommonRepositoryService commonRepositoryService;
        private readonly IMakePaymentService makePaymentService;
        private readonly HttpClient httpClient;
        private readonly string processedBy = "MakePaymentBillProcessJobThreadOne";

        public MakePaymentBillProcessJobThreadOne(ISchedulerRepositoryService schedulerRepositoryService, ICommonRepositoryService commonRepositoryService, IMakePaymentService makePaymentService, HttpClient httpClient)
        {
            this.schedulerRepositoryService = schedulerRepositoryService;
            this.commonRepositoryService = commonRepositoryService;
            this.makePaymentService = makePaymentService;
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task ExecuteAsync(SchedulerClientJobs jobConfig, CancellationToken cancellationToken, string inputField, string createdOrUpdatedBy)
        {
            try
            {
                FileLogger.LogInformation($"MakePaymentBillProcessJobThreadOne: ExecuteAsync Start: SchedulerClientJob_Id: {jobConfig?.SchedulerClientJob_Id}: jobName: {jobConfig?.JobName}");
                var pendingQueues = await makePaymentService.GetPendingBillProcessQueue();
                if (pendingQueues?.Count > 0)
                {
                    foreach (var queue in pendingQueues)
                    {
                        await makePaymentService.UpdateBillProcessQueue(queue.BillProcess_Id, queue.Client_Id, (byte)Enum_BillProcessWorkFlowStatus.Running, queue.CurrentWorkFlowEventType_Id, null, processedBy);
                        var workflowEvents = await makePaymentService.GetPendingBillProcessQueueEvents(queue.BillProcess_Id, queue.Client_Id, queue.TransactionDate);
                        if (workflowEvents?.Count > 0)
                        {
                            foreach (var workflowEvent in workflowEvents)
                            {
                                await makePaymentService.UpdateBillProcessWorkFlowEvent(queue.BillProcess_Id, queue.Client_Id, (byte)Enum_BillProcessWorkFlowStatus.Running,
                                workflowEvent.WorkFlowEvent_Id, workflowEvent.WorkFlowEventType_Id, null, processedBy);

                                var isWorkFlowCompleted = await ProcessWorkflowEventAsync(jobConfig, queue, workflowEvent);
                                if (!isWorkFlowCompleted)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentBillProcessJobThreadOne: ExecuteAsync: Client_Id: {jobConfig?.Client_Id}: SchedulerClientJob_Id: {jobConfig?.SchedulerClientJob_Id}: jobName: {jobConfig?.JobName} Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }

        private async Task<bool> ProcessWorkflowEventAsync(SchedulerClientJobs jobConfig, BillProcessQueue queue, BillProcessQueueEvents workflowEvent)
        {
            try
            {
                if (queue == null || workflowEvent == null)
                {
                    FileLogger.LogInformation($"No pending workflow event found for BillProcess_Id: {queue?.BillProcess_Id}, Client_Id: {queue?.Client_Id}");
                    return false;
                }

                ServiceResponse response;
                var selectedInvoices = string.IsNullOrWhiteSpace(queue.SelectedInvoicesJson) ? new List<SelectedInvoice>() : JsonConvert.DeserializeObject<List<SelectedInvoice>>(queue.SelectedInvoicesJson) ?? new List<SelectedInvoice>();
                var billDetail = string.IsNullOrWhiteSpace(queue.BillDetailJson) ? null : JsonConvert.DeserializeObject<Bill>(queue.BillDetailJson);
                var completedWorkflowEvents = await makePaymentService.GetCompletedBillProcessQueueEvents(queue.BillProcess_Id, queue.Client_Id, queue.TransactionDate);
                BillProcessQueueEvents createBillAndPayment = null;
                ServiceResponse createBillAndPaymentResponse = null;
                if (queue != null && workflowEvent != null && billDetail != null && selectedInvoices?.Count > 0)
                {
                    switch (workflowEvent?.WorkFlowEventType?.ToUpper())
                    {
                        case "CHECKDUPLICATEMAPPING":
                            response = await makePaymentService.CheckDuplicateMapping(queue.Client_Id, queue.Direction_Id, selectedInvoices, billDetail);
                            break;

                        case "GENERATEANDUPLOADCOMMISSIONDOCUMENTS":
                            response = await makePaymentService.GenerateAndUploadCommissionDocuments(queue.Report_Id, queue.FileType_Id, queue.Client_Id, selectedInvoices, billDetail, httpClient);
                            break;

                        case "CREATEBILLANDPAYMENT":
                            response = await makePaymentService.CreateBillAndPayment(queue.Client_Id, queue.Direction_Id, selectedInvoices, billDetail, queue.BillProcess_Id);
                            break;

                        case "APPLYPAYMENT":
                            createBillAndPayment = completedWorkflowEvents?.FirstOrDefault(x => x.WorkFlowEventType.Equals("CREATEBILLANDPAYMENT", StringComparison.OrdinalIgnoreCase));
                            if (createBillAndPayment == null)
                            {
                                response = new ServiceResponse
                                {
                                    Status = false,
                                    Message = "CREATEBILLANDPAYMENT workflow event was not completed."
                                };
                                break;
                            }

                            createBillAndPaymentResponse = JsonConvert.DeserializeObject<ServiceResponse>(createBillAndPayment.ServiceResponseMessage);
                            response = await makePaymentService.ApplyPayment(queue.Client_Id, queue.Direction_Id, selectedInvoices, billDetail, createBillAndPaymentResponse);
                            break;

                        case "CREATEVENDORANDEXTERNALBILL":
                            createBillAndPayment = completedWorkflowEvents?.FirstOrDefault(x => x.WorkFlowEventType.Equals("CREATEBILLANDPAYMENT", StringComparison.OrdinalIgnoreCase));
                            var applyPayment = completedWorkflowEvents?.FirstOrDefault(x => x.WorkFlowEventType.Equals("APPLYPAYMENT", StringComparison.OrdinalIgnoreCase));

                            if (createBillAndPayment == null)
                            {
                                response = new ServiceResponse
                                {
                                    Status = false,
                                    Message = "CREATEBILLANDPAYMENT workflow event was not completed."
                                };
                                break;
                            }

                            if (applyPayment == null)
                            {
                                response = new ServiceResponse
                                {
                                    Status = false,
                                    Message = "APPLYPAYMENT workflow event was not completed."
                                };
                                break;
                            }

                            createBillAndPaymentResponse = JsonConvert.DeserializeObject<ServiceResponse>(createBillAndPayment.ServiceResponseMessage);
                            var applyPaymentResponse = JsonConvert.DeserializeObject<ServiceResponse>(applyPayment.ServiceResponseMessage);

                            if (createBillAndPaymentResponse == null)
                            {
                                response = new ServiceResponse
                                {
                                    Status = false,
                                    Message = "Unable to retrieve CREATEBILLANDPAYMENT response."
                                };
                                break;
                            }
                            if (applyPaymentResponse == null)
                            {
                                response = new ServiceResponse
                                {
                                    Status = false,
                                    Message = "Unable to retrieve APPLYPAYMENT response."
                                };
                                break;
                            }
                            response = await makePaymentService.CreateVendorAndExternalBill(queue.Client_Id, queue.CreatedBy, queue.Direction_Id, billDetail, createBillAndPaymentResponse, applyPaymentResponse, httpClient);

                            break;

                        case "COMPLETED":
                            response = new ServiceResponse
                            {
                                Status = true,
                                Message = "Bill creation workflow completed successfully."
                            };
                            break;

                        default:
                            response = new ServiceResponse
                            {
                                Status = false,
                                Message = $"Unknown workflow event : {workflowEvent?.WorkFlowEventType}"
                            };
                            break;
                    }

                    if (response != null)
                    {
                        var handleWorkflowResult = await HandleWorkflowResult(jobConfig, queue, workflowEvent, response);
                        return handleWorkflowResult;
                    }

                    return false;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentBillProcessJobThreadOne: ProcessWorkflowEventAsync: Client_Id: {jobConfig?.Client_Id}: SchedulerClientJob_Id: {jobConfig?.SchedulerClientJob_Id}: jobName: {jobConfig?.JobName} Exception: {ex} Message: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> HandleWorkflowResult(SchedulerClientJobs jobConfig, BillProcessQueue queue, BillProcessQueueEvents workflowEvent, ServiceResponse response)
        {
            try
            {
                var completedWorkflowEvents = await makePaymentService.GetCompletedBillProcessQueueEvents(queue.BillProcess_Id, queue.Client_Id, queue.TransactionDate);
                var billDetail = string.IsNullOrWhiteSpace(queue.BillDetailJson) ? null : JsonConvert.DeserializeObject<Bill>(queue.BillDetailJson);
                var serviceResponseMessage = JsonConvert.SerializeObject(response);
                if (response.Status)
                {
                    // Event Completed
                    await makePaymentService.UpdateBillProcessWorkFlowEvent(queue.BillProcess_Id, queue.Client_Id, (byte)Enum_BillProcessWorkFlowStatus.Completed,
                        workflowEvent.WorkFlowEvent_Id, workflowEvent.WorkFlowEventType_Id, serviceResponseMessage, processedBy);
                    long? payment_Id = null;
                    var createBillAndPayment = completedWorkflowEvents?.FirstOrDefault(x => x.WorkFlowEventType.Equals("CREATEBILLANDPAYMENT", StringComparison.OrdinalIgnoreCase));
                    if (createBillAndPayment != null)
                    {
                        var createBillAndPaymentResponse = JsonConvert.DeserializeObject<ServiceResponse>(createBillAndPayment.ServiceResponseMessage);
                        if (createBillAndPaymentResponse?.Data != null)
                        {
                            var paymentData = JsonConvert.DeserializeObject<Payments>(createBillAndPaymentResponse.Data.ToString());
                            payment_Id = paymentData?.Payment_Id ?? 0;
                        }
                    }
                    // Move Queue
                    var result = await makePaymentService.MoveToNextWorkFlowEvent(queue.BillProcess_Id, queue.Client_Id, payment_Id, processedBy);

                    return response.Status;
                }

                // Failure

                var retryConfig = await makePaymentService.GetBillProcessRetryConfiguration();
                var retry = retryConfig.FirstOrDefault(x => x.RetryNumber == workflowEvent.RetryCount + 1);
                if (retry != null)
                {
                    // Retry
                    var retryResult = await makePaymentService.SetBillProcessEventRetry(queue.BillProcess_Id, queue.Client_Id, (byte)Enum_BillProcessWorkFlowStatus.Retry,
                        workflowEvent.WorkFlowEvent_Id, workflowEvent.WorkFlowEventType_Id, retry.DelayInMinutes, serviceResponseMessage, processedBy);

                    if (retryResult == "S")
                    {
                        var queueResult = await makePaymentService.UpdateBillProcessQueue(queue.BillProcess_Id, queue.Client_Id, (byte)Enum_BillProcessWorkFlowStatus.Retry, workflowEvent.WorkFlowEventType_Id, serviceResponseMessage, processedBy);
                        if (queueResult != "S")
                        {
                            FileLogger.LogError($"Unable to mark Queue Running. BillProcess_Id: {queue.BillProcess_Id} Client_Id: {queue.Client_Id}");
                        }

                        return response.Status;
                    }

                    if (retryResult == "MAX_RETRY_EXCEEDED")
                    {
                        var failQueueResult = await makePaymentService.FailBillProcess(queue.BillProcess_Id, queue.Client_Id, workflowEvent.WorkFlowEvent_Id, workflowEvent.WorkFlowEventType_Id, serviceResponseMessage, processedBy);
                        if (failQueueResult != "S")
                        {
                            FileLogger.LogError($"Unable to mark Queue Running. BillProcess_Id: {queue.BillProcess_Id} Client_Id: {queue.Client_Id}");
                        }
                        else
                        {
                            // await FailedBillUnApplyPayment(jobConfig, queue, workflowEvent, completedWorkflowEvents);
                            await Utils.SendBatchFailEmail(jobConfig, queue, billDetail.EntityName);
                        }
                        return response.Status;
                    }
                }

                // Max Retry Exceeded
                var failQueueResult_2 = await makePaymentService.FailBillProcess(queue.BillProcess_Id, queue.Client_Id, workflowEvent.WorkFlowEvent_Id, workflowEvent.WorkFlowEventType_Id, serviceResponseMessage, processedBy);
                if (failQueueResult_2 != "S")
                {
                    FileLogger.LogError($"Unable to mark Queue Running. BillProcess_Id: {queue.BillProcess_Id} Client_Id: {queue.Client_Id}");
                }
                else
                {
                    // await FailedBillUnApplyPayment(jobConfig, queue, workflowEvent, completedWorkflowEvents);
                    await Utils.SendBatchFailEmail(jobConfig, queue, billDetail.EntityName);
                }

                return response.Status;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentBillProcessJobThreadOne: HandleWorkflowResult: Client_Id: {jobConfig?.Client_Id}: SchedulerClientJob_Id: {jobConfig?.SchedulerClientJob_Id}: jobName: {jobConfig?.JobName} Exception: {ex} Message: {ex.Message}");
                return response.Status;
            }
        }
        private async Task FailedBillUnApplyPayment(SchedulerClientJobs jobConfig, BillProcessQueue queue, BillProcessQueueEvents workflowEvent, List<BillProcessQueueEvents> completedWorkflowEvents)
        {
            try
            {
                if (workflowEvent?.WorkFlowEventType?.ToUpper() == "CREATEVENDORANDEXTERNALBILL")
                {
                    long payment_Id = 0;
                    var createBillAndPayment = completedWorkflowEvents?.FirstOrDefault(x => x.WorkFlowEventType.Equals("CREATEBILLANDPAYMENT", StringComparison.OrdinalIgnoreCase));
                    var createBillAndPaymentResponse = JsonConvert.DeserializeObject<ServiceResponse>(createBillAndPayment.ServiceResponseMessage);
                    if (createBillAndPaymentResponse?.Data != null)
                    {
                        var paymentData = JsonConvert.DeserializeObject<Payments>(createBillAndPaymentResponse.Data.ToString());
                        payment_Id = paymentData?.Payment_Id ?? 0;
                    }

                    var response = new ServiceResponse
                    {
                        Status = false,
                        Message = $"External Bill Creation Failed for the Payment_Id: {payment_Id} so the Payment was UnApplied. "
                    };

                    var applyPaymentWorkFlowEvent = completedWorkflowEvents?.FirstOrDefault(x => x.WorkFlowEventType.Equals("APPLYPAYMENT", StringComparison.OrdinalIgnoreCase));
                    var unApplyPaymentResponse = await makePaymentService.UnapplyPayment(payment_Id, queue.Client_Id, long.Parse(queue.CreatedBy), httpClient);
                    if (!unApplyPaymentResponse.Status)
                    {
                        response.Message += unApplyPaymentResponse.Message;
                    }
                    var serviceResponseMessage = JsonConvert.SerializeObject(response);
                    await makePaymentService.FailBillProcess(queue.BillProcess_Id, queue.Client_Id, applyPaymentWorkFlowEvent.WorkFlowEvent_Id, applyPaymentWorkFlowEvent.WorkFlowEventType_Id, serviceResponseMessage, processedBy);
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentBillProcessJob: FailedBillUnApplyPayment: Client_Id: {jobConfig?.Client_Id}: SchedulerClientJob_Id: {jobConfig?.SchedulerClientJob_Id}: jobName: {jobConfig?.JobName} Exception: {ex} Message: {ex.Message}");
            }
        }
    }
}
