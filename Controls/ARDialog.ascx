<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ARDialog.ascx.cs" Inherits="cs_websuite.Controls.ARDialog" %>
<style>
    .ar-dialog-status {
        width: 100%;
        background-color: green;
        color: white;
        font-size: 18px;
        text-shadow: 2px 2px;
    }

    .ar-dialog-script {
        float: left;
        width: 315px;
        font-size: 14px;
    }

    .ar-dialog-invoices {
        float: right;
        width: 350px;
    }

    .ar-dialog-payment {
        clear: both;
        width: 100%;
    }

    .ar-dialog-invoices tfoot th {
        text-align: right;
    }

    .blue #ar-dialog .OTable th {
        background-image: none;
        background-color: #336699;
        border-color: #000;
    }

    .blue #ar-dialog .OTable {
        border-color: #000;
    }

    .blue .ui-dialog-titlebar {
        background-image: none;
        background-color: #336699;
        border-color: black;
        color: white;
    }

    .green #ar-dialog .OTable th {
        background-image: none;
        background-color: #3ba31d;
        border-color: #000;
    }

    .green #ar-dialog .OTable {
        border-color: green;
    }

    .green .ui-dialog-titlebar {
        background-image: none;
        background-color: #3ba31d;
        border-color: black;
        color: white;
    }

    .yellow #ar-dialog .OTable th {
        color: #000;
        text-shadow: 1px 1px white;
        background-image: none;
        background-color: yellow;
        border-color: goldenrod;
    }

    .yellow #ar-dialog .OTable {
        border-color: goldenrod;
    }

    .yellow .ui-dialog-titlebar {
        background-image: none;
        background-color: yellow;
        border-color: goldenrod;
        color: black;
    }

    .red #ar-dialog .OTable th {
        background-image: none;
        background-color: #ae0303;
        border-color: #530606;
    }

    .red #ar-dialog .OTable {
        border-color: #530606;
    }

    .red .ui-dialog-titlebar {
        background-image: none;
        background-color: #ae0303;
        border-color: #530606;
        color: white;
    }

    .dialog-body .Label {
        vertical-align: top;
        padding-top: 3px;
    }

    .heading {
        font-style: italic;
        font-weight: bold;
    }

    tr.must-pay {
        background-color: #ffcbcb;
    }

    tr .invoice-checkbox {
        text-align: center;
    }

    #ar-dialog ul li {
        padding-bottom: 10px;
    }

    #common-alert-message ul li {
        margin-bottom: 5px;
    }
</style>
<div class="dialog-body">
    <div id="payment-error">
        <div class="FormError" style="display: none;">
            <p>
                <span class="MessageIcon"></span><strong>Messages:</strong>
                <span class="Message" id="Span1"></span>
            </p>
        </div>
    </div>
    <div class="ar-dialog-status">
    </div>
    <div class="ar-dialog-script">
        <p id="script-main" style="margin-top: 0;"></p>
        <div id="script-keep-on-file" style="display: none;">
            <div id="cc-error" class="ar-dialog-payment" style="padding: 5px 0 10px 0;">
                <div id="errorForm" class="FormError" style="display: none;">
                    <p>
                        <span class="MessageIcon"></span><strong>Messages:</strong>
                        <span class="Message" id="lblErrorMessage"></span>
                    </p>
                </div>
                <table class="OForm">
                    <tr>
                        <td class="Label">Name On Card<span class="Required">*</span>:</td>
                        <td>
                            <input type="text" id="payment-card-name" maxlength="30" style="width: 175px;" /></td>
                    </tr>
                    <tr>
                        <td class="Label">Card Number<span class="Required">*</span>:</td>
                        <td>
                            <input type="text" id="payment-card-number" style="width: 120px;" maxlength="16" /></td>
                    </tr>
                    <tr>
                        <td class="Label">CVV/CVC<span class="Required">*</span>:</td>
                        <td>
                            <input type="text" id="payment-card-cvv" style="width: 50px;" maxlength="4" /></td>
                    </tr>
                    <tr style="white-space:nowrap">
                        <td class="Label">Expires On<span class="Required">*</span>:</td>
                        <td>
                            <select id="payment-card-exp-month">
                                <option selected="selected" value="1">01 January</option>
                                <option value="2">02 February</option>
                                <option value="3">03 March</option>
                                <option value="4">04 April</option>
                                <option value="5">05 May</option>
                                <option value="6">06 June</option>
                                <option value="7">07 July</option>
                                <option value="8">08 August</option>
                                <option value="9">09 September</option>
                                <option value="10">10 October</option>
                                <option value="11">11 November</option>
                                <option value="12">12 December</option>
                            </select>
                            &nbsp;
                    <select id="payment-card-exp-year">
                    </select>
                        </td>
                    </tr>
                    <tr>
                        <td class="Label">Email Receipt To<span class="Required">*</span>:</td>
                        <td>
                            <input type="text" id="payment-email" style="width: 175px;" />
                        </td>
                    </tr>
                </table>
            </div>
            <span id="keepOnFile">Would you like to keep this credit card on file for future payments?
            <input type="checkbox" id="payment-card-keep" /><label for="payment-card-keep" style="font-weight: bold;">Yes</label>
            </span>
        </div>
        <p id="script-no-payment" style="display: none;"></p>
    </div>
    <div class="ar-dialog-invoices" style="float: right;">
        <div class="instruction non-enterprise">
            Select invoices to pay by clicking the checkbox.<br />
            A '<strong>P</strong>' in the first column is for pending payments.
        </div>
        <table class="OTable" style="width: 300px;">
            <thead>
                <tr>
                    <th class="invoice-checkbox">
                        <input type="checkbox" title="Select All Invoices" /></th>
                    <th>Invoice #</th>
                    <th>Invoiced</th>
                    <th>Balance <span class="ar-currency">USD</span></th>
                    <th style="display: none;">Days Past Due</th>
                </tr>
            </thead>
            <tbody></tbody>
            <tfoot>
                <tr>
                    <th class="invoice-checkbox"></th>
                    <th colspan="2">Total:</th>
                    <th>0.00</th>
                    <th style="display: none;"></th>
                </tr>
            </tfoot>
        </table>
        <div class="Hidden must-pay-note instruction" style="padding-bottom: 30px;">To unlock the account pre-selected invoices must be paid.  Once payment is posted (2-3 days) www.instadose.com access will be unlocked.</div>
        <div style="text-align: center; vertical-align: middle; float: right; margin-right: 120px; clear: both;">
            <button id="btnARPayInvoice" class="btn btn-large btn-info" disabled="disabled" style="font-size: 18px; display: none;" value="Pay">
                Pay Invoices<br />
                <br />
                <span>0.00</span></button>

            <div style="padding-top: 20px; display: none;">
                <button id="btnARAccountOverride" class="btn btn-danger" style="font-size: 15px; display: none;" value="Override">Override Account</button>
            </div>
        </div>
    </div>
</div>
<script>
    $(function () {
        var currentYear = (new Date()).getFullYear();
        var dd = $("#payment-card-exp-year");
        for (i = 0; i < 10; i++) {
            dd.append("<option value='" + currentYear + "'>" + currentYear + "</option>");
            currentYear++;
        }
        var today = new Date();
        $("#payment-card-exp-month").val(today.getMonth() + 1);
    });
</script>
