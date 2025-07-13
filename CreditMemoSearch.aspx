<%@ Page Title="Credit Memo Search" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_Finance_CreditMemoSearch" Codebehind="CreditMemoSearch.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<style type="text/css">
    /* CSS/Override for PageContent class */
    .PageContent
    {
        width: 320px; /* Forces search box to left. */
    }
</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></act:ToolkitScriptManager>
    <div id="divSearchByAccountID" style="margin: 10px auto; width: 320px;">
        
        <asp:UpdatePanel ID="upnlSearchTable" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="FormError" id="errors" runat="server" visible="false">
		            <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
                </div>
                <table class="OTable" cellpadding="0" cellspacing="0">
                        <tr>
                            <th>Find Credit Memo By:</th>
                        </tr>
                        <tr>
                            <td>
                                <div class="OForm" > 
                                    <div class="Row">
                                        <div class="Label Small">Account #:</div>
                                        <div class="Control">
                                            <asp:TextBox ID="txtAccountNumber" runat="server" CssClass="Size Small" ></asp:TextBox>
                                            <asp:Button ID="btnGo" runat="server" CssClass="OButton" OnClick="btnGo_Click" Text="Go" />
                                        </div>
                                        <div class="Clear"></div>
                                    </div>
                                </div>       
                            </td>
                        </tr>
                </table>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>

