
insert into Accounting_ClientSettings(Client_Id,SettingKey,SettingValue)
values
(
10007,
'Invoice.DetachAndRemitInfo',
'<div class="payment-instructions-table">
<table width="100%" cellpadding="0" cellspacing="0">
<tr>
<td colspan="2" style="padding-bottom:15px;">
<strong style="font-size:15px;">Payment Instructions</strong><br />
												This is an agency billed policy. Premium collection and remittance are the responsibility 
												of the retail agent. Timely payment is required to ensure continuous coverage. 
												Failure to remit premium may result in policy cancellation and collection of earned premium.
												<br />&nbsp;
</td>
</tr>
<tr>
<td width="50%" valign="top" style="padding-right:20px;">
<strong>Please remit payment to:</strong><br />
												Account # 800110144938<br />
												ABA # 064008637
</td>
<td width="50%" valign="top">
<strong>Remittance Address:</strong><br />
												{{CorporationName}}<br />
												{{CorporationAddress}} <br />
												{{CorporationCity}}, {{CorporationState}} {{CorporationZipCode}}
</td>
</tr>
</table>
</div>
'
);