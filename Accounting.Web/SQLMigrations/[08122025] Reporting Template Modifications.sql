
--update Accounting_LU_ReportTemplates set Template='<!DOCTYPE html>  
--<html lang="en">
--   <head>
--      <meta charset="UTF-8" />
--      {{fonts}}           
--      <style> 
--	  body,
--		td,
--		table,
--		h1,
--		h2 {
--			font-family: "Poppins", sans-serif;
--		}
		
--		.agency-billed-table {
--			width: 100%;
--		}
		
--		.fw-semibold {
--			font-weight: 600;
--		}
		
--		.fw-medium {
--			font-weight: 500;
--		}
		
--		.fs-13 {
--			font-size: 13px;
--		}
		
--		.fs-12 {
--			font-size: 12px;
--		}
		
--		.lh-20 {
--			line-height: 20px;
--		}
		
--		.lh-18 {
--			line-height: 18px;
--		}
--		.pc-left {
--		background-color: #F0F0F0;
--		padding: 10px;
--		width: 240px;
--		color: #085284;
--		vertical-align: top;
--        }
--		.bg-dark {
--			background-color: #085284;
--		}
		
--		.ac-header {
--			background-color: #085284;
--			font-size: 13px;
--			color: #fff;
--			font-weight: 500;
--			padding: 5px 0;
--			line-height: 20px;
--			border: 1px dotted white;
--			box-sizing: border-box;
--		}
		
--		.due-date-alert {
--			border: 1px dotted #FF0000;
--			color: #FF0000;
--			font-weight: 600;
--			text-transform: uppercase;
--			padding: 4px 0;
--		}
		
--		.agency-billed-table .t-head td {
--			font-weight: 600;
--		}
		
--		.td-bg-light {
--			background-color: #F0F0F0;
--		}
		
--		.mb-0 {
--			margin-bottom: 0px;
--		}
		
--		.mt-0 {
--			margin-top: 0px;
--		}
		
--		.px-8 {
--			padding-left: 8px;
--			padding-right: 8px;
--		}
		
--		.border-top-1 {
--			border-top: 1px solid #CCCCCC;
--		}
		
--		.border-right-1 {
--			border-right: 1px solid #CCCCCC;
--		}
		
--		.border-1 {
--			border: 1px solid #ccc;
--		}
		
--		.border-right-0 {
--			border-right: 1px solid #ccc;
--		}
		
--		.pt-0 {
--			padding-top: 0 !important;
--		}
		
--		.pd-table td {
--			padding: 12px 8px 12px 8px;
--			color: #000;
--		}
		
--		.ps-35 {
--			padding-left: 30px;
--		}
		
--		.fw-400 {
--			font-weight: 400;
--		}
		
--		.border-top-dark {
--			border-top: 1px solid #000000;
--		}
		
--		.text-dark {
--			color: #000;
--		}
		
--		.ps-50 {
--			padding-left: 50px;
--		}
		
--		.sepearator {
--			border-bottom: 3px solid #085284;
--			margin: 0;
--			width: 100%;
--		}
		
--		.pt-8 {
--			padding-top: 12px;
--		}
		
--		.pb-8 {
--			padding-bottom: 12px;
--		}
		
--		.page-break {
--			page-break-after: always;
--			break-after: page;
--		}
		
--		.fs-14 {
--			font-size: 14px;
--		}
		
--		.pb-0 {
--			padding-bottom: 0;
--		}
		
--</style>
--   </head>
--   <body>
--      <table width="100%" cellpadding="0" cellspacing="0" border="0">
--         <tr>
--            <td style="padding: 20px 0; font-family: ''Poppins'', sans-serif; color: #333; font-size: 15px; line-height: 23px;">
--               <table border="0" cellspacing="0" cellpadding="0" width="1050" align="center" style="border-color:rgba(0,0,0,.09);">
--                  <tr>
--                     <td>
--                        <table cellpadding="0" border="0" cellspacing="0" width="100%">
--                           <tr>
--                              <td height="20" class="p-0"></td>
--                           </tr>
--                        </table>
--                        <table class="agency-billed-table">
--                           <tr>
--                              <td>{{clientLogo}}</td>
--                              <td align="right" class="fs-13 lh-20">                                          <span class="fw-semibold">{{clientCompanyName}}</span><br />                                          {{clientAddress}},<br />                                          {{clientCityStateZip}}<br />                                      </td>
--                           </tr>
--                        </table>
--                        <table cellpadding="0" border="0" cellspacing="0" width="100%">
--                           <tr>
--                              <td height="20" class="p-0"></td>
--                           </tr>
--                        </table>
--                        <table class="agency-billed-table">
--                           <tr>
--                              <td align="center">
--                                 <div class="bg-dark ac-header">Account Statement</div>
--                              </td>
--                           </tr>
--                        </table>
--                        <table cellpadding="0" border="0" cellspacing="0" width="100%">
--                           <tr>
--                              <td height="20" class="p-0"></td>
--                           </tr>
--                        </table>
--                        {{dynamicAgentloopingLogic}} 
--                        <table class="agency-billed-table" cellpadding="0" border="0" cellspacing="0" width="100%">
--                           <tr>
--                              <td align="center">
--                                 <div class="due-date-alert">Audits are due upon Receipt</div>
--                              </td>
--                           </tr>
--                        </table>
--                        <table cellpadding="0" border="0" cellspacing="0" width="100%">
--                           <tr>
--                              <td height="10" class="p-0"></td>
--                           </tr>
--                        </table>
--                        <table class="agency-billed-table fs-12 lh-18 pd-table" cellpadding="0" border="0" cellspacing="0">
--                           <tbody>
--                              <tr>
--                                 <td rowspan="2" style="width:200px;">
--                                    <div><span class="fw-semibold fs-14">Account Total</span></div>
--                                 </td>
--                                 <td colspan="6"></td>
--                                 <td class="fw-semibold fs-14" align="right">NET DUE</td>
--                                 <td></td>
--                              </tr>
--                              <tr>
--                                 <td class="fw-400" colspan="6"></td>
--                                 <td class="fw-semibold fs-14 pt-0 pb-0" align="right">
--                                    <div class="pt-8 pb-8" style="border-top:4px solid #000; border-bottom:4px solid #000; width:100px;">                                                  {{TotalNetDue}}                                              </div>
--                                 </td>
--                                 <td align="right"></td>
--                              </tr>
--                           </tbody>
--                        </table>
--                        <table cellpadding="0" border="0" cellspacing="0" width="100%">
--                           <tr>
--                              <td height="20" class="p-0"></td>
--                           </tr>
--                        </table>
--                        <table class="agency-billed-table" cellpadding="0" border="0" cellspacing="0" width="100%">
--                           <tr>
--                              <td align="center">
--                                 <div class="due-date-alert">                                              ITEMS NOT PAID BY DUE DATE MAY BE CANCELLED BY THE CARRIER WITHOUT FURTHER WARNING                                          </div>
--                              </td>
--                           </tr>
--                        </table>
--                        <table cellpadding="0" border="0" cellspacing="0" width="100%">
--                           <tr>
--                              <td height="10" class="p-0"></td>
--                           </tr>
--                        </table>
--                     </td>
--                  </tr>
--               </table>
--            </td>
--         </tr>
--      </table>
--   </body>
--</html>' Where Report_Id=2


--update Accounting_LU_ReportTemplates set Template='<!DOCTYPE html>    
--<html lang="en">
--   <head>
--      <meta charset="UTF-8" />
--      {{fonts}}                  
--      <style>.border-top-1 {
--	border-top: 1px solid #CCCCCC;
--}

--.border-bottom-1 {
--	border-bottom: 1px solid #CCCCCC;
--}

--body {
--	font-family: "Poppins", sans-serif;
--	font-size: 13px;
--	color: #333;
--	font-weight: 400;
--	line-height: normal;
--}

--.mt-0 {
--	margin-top: 0;
--}

--.mb-0 {
--	margin-bottom: 0;
--}

--.pb-10 {
--	padding-bottom: 10px;
--}

--.statement-date {
--	color: #289FD7;
--	font-weight: 600;
--	font-size: 24px;
--	padding: 8px 0px;
--	text-align: center;
--	border-bottom: solid 1px #085284;
--	border-top: solid 1px #085284;
--}

--.agency-name {
--	background-color: #085284;
--	padding: 6px 12px;
--	color: #FFF;
--	font-size: 16px;
--}

--.pc-left {
--	background-color: #F0F0F0;
--	padding: 10px;
--	width: 240px;
--	color: #085284;
--	vertical-align: top;
--}

--.pc-right {
--	padding: 0px 10px;
--	border: solid 1px #CCCCCC;
--	border-left: none;
--	position: relative;
--	vertical-align: top;
--}

--.pc-right table thead th,
--.pc-right table tbody td {
--	padding: 12px 4px;
--}

--.pc-right table tfoot td {
--	padding: 12px 4px;
--}

--.pc-right table tbody {
--	vertical-align: top;
--}

--</style>
--</head>
--   <body>
--      <table width="100%" cellpadding="0" cellspacing="0" border="0">
--         <tr>
--            <td style="padding: 20px 0; color: #333; font-size: 15px; line-height:23px;">
--               <table border="0" cellspacing="0" cellpadding="0" width="1050"  align="center" style="border-color:rgba(0,0,0,.09); margin-bottom:100px">
--                  <tr>
--                     <td>
--                        <table cellpadding="0" cellspacing="0" width="100%">
--                           <tr>
--                              <td>
--                                 <table cellpadding="0" cellspacing="0" width="100%">
--                                    <tr>
--                                       <td align="left">                                                      {{clientLogo}}                                                  </td>
--                                       <td style="text-align:right;" width="80%">
--                                          <div style="font-weight: bold; font-size: 13px;">{{clientCompanyName}}</div>
--                                          <div style="font-size: 13px;">                                                          {{clientAddress}}<br />                                                          {{clientCityStateZip}}<br />                                                      </div>
--                                       </td>
--                                    </tr>
--                                 </table>
--                              </td>
--                           </tr>
--                        </table>
--                        <table cellpadding="0" cellspacing="0" style="width:100%;">
--                           <tr>
--                              <td style="font-weight:700; padding:10px 0; text-align:center; font-size: 24px; color:#289FD7; border-top:1px solid #085284; border-bottom: 1px solid #085284;">                                          Commission Statements as of {{statementDate}}                                      </td>
--                           </tr>
--                        </table>
--                        <table cellpadding="0" cellspacing="0" class="body-table">
--						{{carrierBlocks}}
--						</table>
--                     </td>
--                  </tr>
--               </table>
--            </td>
--         </tr>
--      </table>
--   </body>
--</html>' Where Report_Id=3