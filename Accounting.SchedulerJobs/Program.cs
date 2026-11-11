using Accounting.SchedulerJobs.Abstract;
using Accounting.SchedulerJobs.BackgroundServices;
using Accounting.SchedulerJobs.Jobs;
using Accounting.SchedulerJobs.RepositoryServices;
using Accounting.SchedulerJobs.Services;
using Accounting.SchedulerJobs.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddScoped<CourtesyReminderJob>();
builder.Services.AddScoped<IntentToCancelJob>();
builder.Services.AddScoped<NoticeOfCancellationJob>();
builder.Services.AddScoped<InvoiceProductionReportJob>();
builder.Services.AddScoped<MakePaymentBillProcessJob>();
builder.Services.AddScoped<MakePaymentBillProcessJobThreadOne>();
builder.Services.AddScoped<AccountsReceivableReportJob>();
builder.Services.AddScoped<AccountsPayableReportJob>();
builder.Services.AddHostedService<SchedulerService>();
builder.Services.AddScoped<IJobFactory, JobFactory>();
builder.Services.AddScoped<ISchedulerRepositoryService, SchedulerRepositoryService>();
builder.Services.AddScoped<ICommonRepositoryService, CommonRepositoryService>();
builder.Services.AddScoped<IInvoiceNoticeRepositoryService, InvoiceNoticeRepositoryService>();
builder.Services.AddScoped<IEmailAttachmentService, EmailAttachmentService>();
builder.Services.AddScoped<IMakePaymentRepositoryService, MakePaymentRepositoryService>();
builder.Services.AddScoped<IMakePaymentService, MakePaymentService>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var repository = scope.ServiceProvider.GetRequiredService<ISchedulerRepositoryService>();
    await ConfigurationHelper.Initialize(builder.Configuration, builder.Environment, repository);
}

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
