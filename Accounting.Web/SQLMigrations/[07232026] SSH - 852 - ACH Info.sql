USP_Accounting_GetEntitySettings

select * from Accounting_LU_Settings S   
select * from Accounting_EntitySettings ES  
update Accounting_LU_Settings set Code = 'ACHInfo' where Setting_id = 4
select * from Accounting_CarrierDetails where Client_id = 10001 and status = 1 and (name like '%Concert%' or name like '%Merchant%' or name like '%Clear Blue%') 
select * from Accounting_PolicyDetails where CarrierDetail_id in (select carrierDetail_id from Accounting_CarrierDetails where Client_id = 10001 and status = 1 and (name like '%Concert%' or name like '%Merchant%' or name like '%Clear Blue%') ) order by CreatedOn desc
insert into Accounting_LU_Settings(Name,Description)
Values ('ACH Info','ACH Payment Information')
 insert into  Accounting_EntitySettings(Client_id, EntityType_id, Entity_Id, Setting_Id, SettingValue_text)
 values
(10001,3,10005,4,'<center>
<p>
        Please remit premium payments via <strong>ACH</strong> using the banking information below.
        Kindly email the remittance advice to
        <a href="mailto:SZambrana@necspecialty.com">SZambrana@necspecialty.com</a>.
    </p>

    <table cellpadding="8" cellspacing="0" border="1" style="border-collapse: collapse; border-color: #dcdcdc; max-width: 650px;">
        <tr style="background-color: #f5f5f5;">
            <th align="left">Detail</th>
            <th align="left">Information</th>
        </tr>
        <tr>
            <td><strong>Account Name</strong></td>
            <td>NEC Specialty, Inc.</td>
        </tr>
        <tr>
            <td><strong>Bank Name</strong></td>
            <td>Bank of America</td>
        </tr>
        <tr>
            <td><strong>Concert Policies</strong>
Account Number</td>
            <td>483100280910</td>
        </tr>
        <tr>
            <td><strong>Merchant &amp; Clear Blue Policies</strong>
Account Number</td>
            <td>483100280907</td>
        </tr>
        <tr>
            <td><strong>Routing Number (Paper &amp; Electronic)</strong></td>
            <td>021000322</td>
        </tr>
        <tr>
            <td><strong>Routing Number (Wires)</strong></td>
            <td>026009593</td>
        </tr>
    </table>
<p><strong>Questions?</strong> Contact our Accounting Department at <a href="mailto:accountingservices@teamfocusins.com">accountingservices@teamfocusins.com</a>.</p>
</center>'),
(10001,3,10010,4,'<center>
<p>
        Please remit premium payments via <strong>ACH</strong> using the banking information below.
        Kindly email the remittance advice to
        <a href="mailto:SZambrana@necspecialty.com">SZambrana@necspecialty.com</a>.
    </p>

    <table cellpadding="8" cellspacing="0" border="1" style="border-collapse: collapse; border-color: #dcdcdc; max-width: 650px;">
        <tr style="background-color: #f5f5f5;">
            <th align="left">Detail</th>
            <th align="left">Information</th>
        </tr>
        <tr>
            <td><strong>Account Name</strong></td>
            <td>NEC Specialty, Inc.</td>
        </tr>
        <tr>
            <td><strong>Bank Name</strong></td>
            <td>Bank of America</td>
        </tr>
        <tr>
            <td><strong>Concert Policies</strong>
Account Number</td>
            <td>483100280910</td>
        </tr>
        <tr>
            <td><strong>Merchant &amp; Clear Blue Policies</strong>
Account Number</td>
            <td>483100280907</td>
        </tr>
        <tr>
            <td><strong>Routing Number (Paper &amp; Electronic)</strong></td>
            <td>021000322</td>
        </tr>
        <tr>
            <td><strong>Routing Number (Wires)</strong></td>
            <td>026009593</td>
        </tr>
    </table>
<p><strong>Questions?</strong> Contact our Accounting Department at <a href="mailto:accountingservices@teamfocusins.com">accountingservices@teamfocusins.com</a>.</p>
</center>'),
(10001,3,10011,4,'<center>
<p>
        Please remit premium payments via <strong>ACH</strong> using the banking information below.
        Kindly email the remittance advice to
        <a href="mailto:SZambrana@necspecialty.com">SZambrana@necspecialty.com</a>.
    </p>

    <table cellpadding="8" cellspacing="0" border="1" style="border-collapse: collapse; border-color: #dcdcdc; max-width: 650px;">
        <tr style="background-color: #f5f5f5;">
            <th align="left">Detail</th>
            <th align="left">Information</th>
        </tr>
        <tr>
            <td><strong>Account Name</strong></td>
            <td>NEC Specialty, Inc.</td>
        </tr>
        <tr>
            <td><strong>Bank Name</strong></td>
            <td>Bank of America</td>
        </tr>
        <tr>
            <td><strong>Concert Policies</strong>
Account Number</td>
            <td>483100280910</td>
        </tr>
        <tr>
            <td><strong>Merchant &amp; Clear Blue Policies</strong>
Account Number</td>
            <td>483100280907</td>
        </tr>
        <tr>
            <td><strong>Routing Number (Paper &amp; Electronic)</strong></td>
            <td>021000322</td>
        </tr>
        <tr>
            <td><strong>Routing Number (Wires)</strong></td>
            <td>026009593</td>
        </tr>
    </table>
<p><strong>Questions?</strong> Contact our Accounting Department at <a href="mailto:accountingservices@teamfocusins.com">accountingservices@teamfocusins.com</a>.</p>
</center>'),
(10001,3,20008,4,'<center>
<p>
        Please remit premium payments via <strong>ACH</strong> using the banking information below.
        Kindly email the remittance advice to
        <a href="mailto:SZambrana@necspecialty.com">SZambrana@necspecialty.com</a>.
    </p>

    <table cellpadding="8" cellspacing="0" border="1" style="border-collapse: collapse; border-color: #dcdcdc; max-width: 650px;">
        <tr style="background-color: #f5f5f5;">
            <th align="left">Detail</th>
            <th align="left">Information</th>
        </tr>
        <tr>
            <td><strong>Account Name</strong></td>
            <td>NEC Specialty, Inc.</td>
        </tr>
        <tr>
            <td><strong>Bank Name</strong></td>
            <td>Bank of America</td>
        </tr>
        <tr>
            <td><strong>Concert Policies</strong>
Account Number</td>
            <td>483100280910</td>
        </tr>
        <tr>
            <td><strong>Merchant &amp; Clear Blue Policies</strong>
Account Number</td>
            <td>483100280907</td>
        </tr>
        <tr>
            <td><strong>Routing Number (Paper &amp; Electronic)</strong></td>
            <td>021000322</td>
        </tr>
        <tr>
            <td><strong>Routing Number (Wires)</strong></td>
            <td>026009593</td>
        </tr>
    </table>
<p><strong>Questions?</strong> Contact our Accounting Department at <a href="mailto:accountingservices@teamfocusins.com">accountingservices@teamfocusins.com</a>.</p>
</center>'),
(10001,3,20123,4,'<center>
<p>
        Please remit premium payments via <strong>ACH</strong> using the banking information below.
        Kindly email the remittance advice to
        <a href="mailto:SZambrana@necspecialty.com">SZambrana@necspecialty.com</a>.
    </p>

    <table cellpadding="8" cellspacing="0" border="1" style="border-collapse: collapse; border-color: #dcdcdc; max-width: 650px;">
        <tr style="background-color: #f5f5f5;">
            <th align="left">Detail</th>
            <th align="left">Information</th>
        </tr>
        <tr>
            <td><strong>Account Name</strong></td>
            <td>NEC Specialty, Inc.</td>
        </tr>
        <tr>
            <td><strong>Bank Name</strong></td>
            <td>Bank of America</td>
        </tr>
        <tr>
            <td><strong>Concert Policies</strong>
Account Number</td>
            <td>483100280910</td>
        </tr>
        <tr>
            <td><strong>Merchant &amp; Clear Blue Policies</strong>
Account Number</td>
            <td>483100280907</td>
        </tr>
        <tr>
            <td><strong>Routing Number (Paper &amp; Electronic)</strong></td>
            <td>021000322</td>
        </tr>
        <tr>
            <td><strong>Routing Number (Wires)</strong></td>
            <td>026009593</td>
        </tr>
    </table>
<p><strong>Questions?</strong> Contact our Accounting Department at <a href="mailto:accountingservices@teamfocusins.com">accountingservices@teamfocusins.com</a>.</p>
</center>'),
(10001,3,20172,4,'<center>
<p>
        Please remit premium payments via <strong>ACH</strong> using the banking information below.
        Kindly email the remittance advice to
        <a href="mailto:SZambrana@necspecialty.com">SZambrana@necspecialty.com</a>.
    </p>

    <table cellpadding="8" cellspacing="0" border="1" style="border-collapse: collapse; border-color: #dcdcdc; max-width: 650px;">
        <tr style="background-color: #f5f5f5;">
            <th align="left">Detail</th>
            <th align="left">Information</th>
        </tr>
        <tr>
            <td><strong>Account Name</strong></td>
            <td>NEC Specialty, Inc.</td>
        </tr>
        <tr>
            <td><strong>Bank Name</strong></td>
            <td>Bank of America</td>
        </tr>
        <tr>
            <td><strong>Concert Policies</strong>
Account Number</td>
            <td>483100280910</td>
        </tr>
        <tr>
            <td><strong>Merchant &amp; Clear Blue Policies</strong>
Account Number</td>
            <td>483100280907</td>
        </tr>
        <tr>
            <td><strong>Routing Number (Paper &amp; Electronic)</strong></td>
            <td>021000322</td>
        </tr>
        <tr>
            <td><strong>Routing Number (Wires)</strong></td>
            <td>026009593</td>
        </tr>
    </table>
<p><strong>Questions?</strong> Contact our Accounting Department at <a href="mailto:accountingservices@teamfocusins.com">accountingservices@teamfocusins.com</a>.</p>
</center>'),
(10001,3,20844,4,'<center>
<p>
        Please remit premium payments via <strong>ACH</strong> using the banking information below.
        Kindly email the remittance advice to
        <a href="mailto:SZambrana@necspecialty.com">SZambrana@necspecialty.com</a>.
    </p>

    <table cellpadding="8" cellspacing="0" border="1" style="border-collapse: collapse; border-color: #dcdcdc; max-width: 650px;">
        <tr style="background-color: #f5f5f5;">
            <th align="left">Detail</th>
            <th align="left">Information</th>
        </tr>
        <tr>
            <td><strong>Account Name</strong></td>
            <td>NEC Specialty, Inc.</td>
        </tr>
        <tr>
            <td><strong>Bank Name</strong></td>
            <td>Bank of America</td>
        </tr>
        <tr>
            <td><strong>Concert Policies</strong>
Account Number</td>
            <td>483100280910</td>
        </tr>
        <tr>
            <td><strong>Merchant &amp; Clear Blue Policies</strong>
Account Number</td>
            <td>483100280907</td>
        </tr>
        <tr>
            <td><strong>Routing Number (Paper &amp; Electronic)</strong></td>
            <td>021000322</td>
        </tr>
        <tr>
            <td><strong>Routing Number (Wires)</strong></td>
            <td>026009593</td>
        </tr>
    </table>
<p><strong>Questions?</strong> Contact our Accounting Department at <a href="mailto:accountingservices@teamfocusins.com">accountingservices@teamfocusins.com</a>.</p>
</center>')



update Accounting_EntitySettings set SettingValue_Text = '
<div class="payment-info">

    <p class="payment-text">
        Please remit premium payments via <b>ACH</b> using the banking information below.
        Kindly email the remittance advice to
        <a href="mailto:SZambrana@necspecialty.com">SZambrana@necspecialty.com</a>.
    </p>

    <table class="bank-table">
        <tr>
            <th class="detail-col">Detail</th>
            <th class="info-col">Information</th>
        </tr>
        <tr>
            <td>Account Name</td>
            <td>NEC Specialty, Inc.</td>
        </tr>
        <tr>
            <td>Bank Name</td>
            <td>Bank of America</td>
        </tr>
        <tr>
            <td>Concert Policies Account Number</td>
            <td>483100280910</td>
        </tr>
        <tr>
            <td>Merchant &amp; Clear Blue Policies Account Number</td>
            <td>483100280907</td>
        </tr>
        <tr>
            <td>Routing Number (Paper &amp; Electronic)</td>
            <td>021000322</td>
        </tr>
        <tr>
            <td>Routing Number (Wires)</td>
            <td>026009593</td>
        </tr>
    </table>
    <p class="payment-text">
        <strong>Questions?</strong> Contact our Accounting Department at
        <a href="mailto:accountingservices@teamfocusins.com">accountingservices@teamfocusins.com</a>.
    </p>
</div>
' where Setting_Id = 4 and Client_id = 10001 and status = 1