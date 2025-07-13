<%@ Page Title="Factors Search Page" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="TechOps_FactorsSearch" Codebehind="FactorsSearch.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style type="text/css">
        /* CSS/Override for PageContent Class */
        .PageContent
        {
            width: 450px; /* Forces Search Box to LEFT. */
        }
    </style>
    <script type="text/javascript">
        // Allows User to type in numeric values only (no decimal allowed).
        function checkForWholeNumber(evt) {
            var x = document.getElementById("<%=rdobtnInstaAcct.ClientID%>"); 
            
            if (x.checked == true) {
                var charCode = (evt.which) ? evt.which : event.keyCode;
                if (charCode > 31 && (charCode < 48 || charCode > 57))
                    return false;
            }
            
            return true;
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <asp:UpdatePanel ID="updtpnlFormErrorMessage" runat="server">
        <ContentTemplate>
            <div class="FormError" id="divErrorMessage" runat="server" visible="false">
	            <p><span class="MessageIcon"></span>
	            <strong>Messages:</strong>&nbsp;<span id="spnErrorMessage" runat="server">An error was encountered.</span></p>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="updtpnlSearchByAccountNumber" runat="server">
        <ContentTemplate>
            <div id="divSearchByAccountNumber" style="margin: 10px auto; width: 450px;">
                <table class="OTable">
                    <tr>
                        <th>Find Factor Settings By:</th>
                    </tr>
                    <tr>
                        <td>
                            <div class="OForm">                                 
                                <div class="Row">                                    
                                    <div class="Control">                                        
                                        <asp:RadioButton ID="rdobtnInstaAcct" runat="server" Text="Instadose Acct#" Checked="true" GroupName="Account"/>
                                        <asp:RadioButton ID="rdobtnGDSAcct" runat="server" Text="GDS Acct#" GroupName="Account" />    
                                        <asp:TextBox ID="txtAccountNumberSearch" runat="server" CssClass="Size Small" MaxLength="10" onkeypress= "return checkForWholeNumber(event)"></asp:TextBox>
                                        <asp:Button ID="btnFind" runat="server" CssClass="OButton" OnClick="btnFind_Click" Text="Find" />
                                    </div>
                                    <div class="Clear"></div>
                                </div>
                            </div>       
                        </td>
                    </tr>
                </table>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

