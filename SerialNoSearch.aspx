<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="TechOps_SerialNoSearch" Codebehind="SerialNoSearch.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style type="text/css">
    /* CSS/Override for PageContent Class */
    .PageContent
    {
        width: 320px; /* Forces Search Box to LEFT. */
    }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></act:ToolkitScriptManager>
    <asp:UpdatePanel ID="updtpnlSerialNumberSearch" runat="server">
        <ContentTemplate>
            <div class="FormError" id="divErrorMessage" runat="server" visible="false">
	            <p><span class="MessageIcon"></span>
	            <strong>Messages:</strong>&nbsp;<span id="spnErrorMessage" runat="server">An error was encountered.</span></p>
            </div>
            <asp:UpdatePanel ID="upnlSearchTable" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div id="divSearchBySerailNumber" style="margin: 10px auto; width: 320px;">
                        <table class="OTable">
                                <tr>
                                    <th>Find Badge Information By:</th>
                                </tr>
                                <tr>
                                    <td>
                                        <div class="OForm"> 
                                            <div class="Row">
                                                <div class="Label Small">Serial #:</div>
                                                <div class="Control">
                                                    <asp:TextBox ID="txtSerialNumber" runat="server" CssClass="Size Small" ></asp:TextBox>
                                                    <asp:Button ID="btnGo" runat="server" CssClass="OButton" OnClick="btnGo_Click" Text="Go" />
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
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

