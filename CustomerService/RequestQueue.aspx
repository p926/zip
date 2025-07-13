<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="CustomerService_RequestQueue" Codebehind="RequestQueue.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<style type="text/css">
    .SmallerTextSize
    {
        font-size: 10px;
        font-weight: normal;
        font-style: normal;   
    }
    .noWrapHeader th
    {
        white-space: nowrap !important;
    }
</style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
<act:toolkitscriptmanager id="ToolkitScriptManager1" runat="server"></act:toolkitscriptmanager>
    <div class="FormError" id="formErrors" runat="server" visible="false">
		<p><span class="MessageIcon"></span>
		<strong>Messages:</strong>&nbsp;<span id="errorMsg" runat="server" >An error was encountered.</span></p>
    </div>
    <div class="FormMessage" id="messages" runat="server" visible="false"> 
	    <p><span class="MessageIcon"></span>
	    <strong>Messages:</strong> <span id="submitMsg" runat ="server"></span></p>
    </div>
    <div id="mainFormArea" runat="server">
        <asp:UpdatePanel ID="updtpnlMainContentArea" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <%--Toolbar Component for Dealer ID Search.--%>
                <div id="divDealerIDSearchToolbar" class="OToolbar JoinTable" runat="server">
                    <ul>
                        <li>
                            <asp:LinkButton ID="btnNewRequest" CssClass="Add Icon" runat="server"
                            CommandName="NewCSRequest" CommandArgument="" 
                            onclick="btnNewRequest_Click" Visible="false">Create CS Request</asp:LinkButton>
                        </li>
                        <li>
                            <asp:LinkButton ID="btnShowAllRequest" CssClass=" PageGo Icon" runat="server"
                            CommandName="ListCSRequests" CommandArgument=""  
                            onclick="btnShowAllRequest_Click" Text="Turn Paging Off"></asp:LinkButton>     
                        </li>
                        <li>List Case Status:</li>
                        <li>
                            <asp:DropDownList ID="ddlCaseStatus" runat="server" AutoPostBack="true" AppendDataBoundItems="true">
                                <asp:ListItem Text="-All Status-" Value="0" />
                            </asp:DropDownList>
                        </li>
                        <li><asp:Label ID="lblTotalCases" runat="server" Font-Bold="true"></asp:Label></li>
                    </ul>
                </div>
                <%--End--%>
                <%--Customer Service Request Queque GridView--%>
                <asp:UpdatePanel ID="updtpnlSalesRepDistributorToolbarAndGridView" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:GridView ID="gvRequestCases" runat="server" AutoGenerateColumns="False" 
                        DataSourceID="sqlRequestCases" CssClass="OTable noWrapHeader" AllowSorting="True"
                        AlternatingRowStyle-CssClass="Alt" DataKeyNames="CaseID" AllowPaging="True" 
                        PageSize="20" Width="100%" OnRowCommand="gvRequestCases_RowCommand">
                            <AlternatingRowStyle CssClass="Alt" />
                            <HeaderStyle CssClass="" />
                            <Columns>
                                <asp:TemplateField HeaderText="Case #" ShowHeader="true">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lnkbtnCSRequestCase" runat="server" CommandArgument='<%# DataBinder.Eval(Container.DataItem,"CaseID","") %>'
                                        CommandName="EditCSRequestCase" Text='<%# DataBinder.Eval(Container.DataItem,"CaseID","") %>'></asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="CaseID" ShowHeader="false" ReadOnly="true" Visible="false" />
                                <asp:BoundField DataField="AccountID" HeaderText="Acct. #" SortExpression="AccountID" />
                                <asp:BoundField DataField="AccountName" HeaderText="Acct. Name" SortExpression="AccountName" />
                                <asp:BoundField DataField="RequestedBy" HeaderText="CS Rep." SortExpression="RequestedBy" Visible="false" />
                                <asp:BoundField DataField="RequestDate" HeaderText="Requested"  SortExpression="RequestDate" />
                                <asp:BoundField DataField="DaysOpen" HeaderText="Days Open" SortExpression="DaysOpen" />
                                <asp:BoundField DataField="LastUpdatedDate" HeaderText="Last Updated" SortExpression="LastUpdatedDate" />
                                <asp:BoundField DataField="CaseStatusDesc" HeaderText="Status" SortExpression="CaseStatusDesc" />
                                <asp:BoundField DataField="RequestDesc" HeaderText="Description" SortExpression="RequestDesc" />
                                <asp:BoundField DataField="ResolvedBy" HeaderText="Resolved By" SortExpression="ResolvedBy" />
                                <asp:TemplateField >
                                    <HeaderTemplate>Case Notes</HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="lblCaseNotes" runat="server" Width="175" Text='<%# LimitDisplayedCharacters(Eval("CaseNote"), 100) %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                <div>There are no records found!</div>
                            </EmptyDataTemplate>
                            <PagerSettings PageButtonCount="20" />
                        </asp:GridView>  
                    </ContentTemplate>
                </asp:UpdatePanel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div> 
    <asp:SqlDataSource ID="sqlRequestCases" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
    SelectCommand="sp_if_GetCaseRequestByCaseStatus" SelectCommandType="StoredProcedure">
        <SelectParameters>
            <asp:ControlParameter ControlID="ddlCaseStatus" DefaultValue="1" Name="txtSearch" PropertyName="SelectedValue" Type="String" />
        </SelectParameters>
    </asp:SqlDataSource>  
</asp:Content>

