<%@ Page Title="Request Case" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_InformationFinder_Details_Case" Codebehind="Case.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
   
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">

    <div style="width: 100%" >

        <div >
            <asp:FormView ID="fvCase" runat="server" DataSourceID="sqlCase" Width="100%" >
                <ItemTemplate>
                <table class="OTable" >
                    <tr >
                    <td style="width: 14%" >Case #:</td>
                    <td style="width: 36%" ><asp:Label ID="lblCaseID" runat="server" Text='<%# Bind("CaseID") %>' /></td>
                    <td style="width: 14%" >Days Open:</td>
                    <td  ><asp:Label ID="lblDaysOpen" runat="server" Text='<%# Bind("DaysOpen") %>' /></td>
                    </tr>
                    <%--<tr >
                    <td >Status:</td>
                    <td ><asp:Label ID="Label1" runat="server" Text='<%# Bind("Status") %>' /></td>
                    <td >C/S Rep:</td>
                    <td ><asp:Label ID="Label2" runat="server" Text='' /></td>
                    </tr>--%>
                    <tr class="Alt">
                    <td >Requested By:</td>
                    <td ><asp:Label ID="Label3" runat="server" Text='<%# Bind("RequestedBy") %>' /></td>
                    <td >Account #:</td>
                    <td ><asp:Label ID="lblAccountID" runat="server" Text='<%# Bind("AccountID") %>' /></td>
                    </tr>
                    <tr>
                    <td >Requested Date:</td>
                    <td ><asp:Label ID="Label5" runat="server" Text='<%# Bind("RequestDate", "{0:d}") %>' /></td>
                    <td >Account Name:</td>
                    <td ><asp:HyperLink NavigateUrl='<%# Bind("AccountID","Account.aspx?ID={0}") %>' ID="Label6" runat="server" Text='<%# Bind("AccountName") %>' /></td>
                    </tr>
                    <tr class="Alt">
                    <td >Request Type:</td>
                    <td ><asp:Label ID="Label7" runat="server" Text='<%# Bind("RequestDescription") %>' /></td>
                    <td >Serial #:</td>
                    <td ><asp:HyperLink NavigateUrl='<%# Bind("SerialNo", "Device.aspx?ID={0}") %>' ID="Label12" runat="server" Text='<%# Bind("SerialNo") %>' /></td>
                    </tr>
                    <tr >
                    <td >Forwarded To:</td>
                    <td ><asp:HyperLink NavigateUrl='<%# Bind("FowardedUserNo", "User.aspx?ID={0}") %>' ID="Label11" runat="server" Text='<%# Bind("FowardedName") %>' /></td>
                    <td >Order #:</td>
                    <td ><asp:HyperLink NavigateUrl='<%# Bind("OrderNo", "Order.aspx?ID={0}") %>' ID="Label16" runat="server" Text='<%# Bind("OrderNo") %>' /></td>
                    </tr>
                    <tr class="Alt">
                    <td >Resolved By:</td>
                    <td ><asp:Label ID="Label4" runat="server" Text='<%# Bind("ResolvedBy") %>' /></td>
                    <td >Resolved Date:</td>
                    <td ><asp:Label ID="Label9" runat="server" Text='<%# Bind("ResolvedDate", "{0:d}") %>' /></td>
                    </tr>          
                    <tr >
                    <td >Request Reason:</td>
                    <td  colspan="3"><asp:Label ID="Label15" runat="server" Text='<%# Bind("ReasonText") %>' /></td>
                    </tr>
                </table>
            </ItemTemplate>
            </asp:FormView>
        </div>

        <div  >
        <asp:GridView CssClass="OTable"
            ID="cvCaseNotes" AutoGenerateColumns="false" runat="server" DataSourceID="sqlCaseNotes">
            <Columns>
                <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="CreatedDate" HeaderStyle-Width="80"
                    HeaderText="Date" InsertVisible="False" ReadOnly="True" SortExpression="CreatedDate" DataFormatString="{0:d}" />
                <asp:BoundField ItemStyle-CssClass="mt-itm" ItemStyle-Wrap="false" HeaderStyle-CssClass="mt-hd" DataField="CallerName"
                    HeaderStyle-Width="100" HeaderText="From" InsertVisible="False" ReadOnly="True" SortExpression="CallerName" />
                <asp:BoundField ItemStyle-CssClass="mt-itm" ItemStyle-Wrap="false" HeaderStyle-CssClass="mt-hd" DataField="ForwardTo"
                    HeaderStyle-Width="120" HeaderText="To" InsertVisible="False" ReadOnly="True" SortExpression="ForwardTo" />
                <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="caseNote"
                    HeaderText="Notes" InsertVisible="False" ReadOnly="True" SortExpression="caseNote" />
            </Columns>
            <AlternatingRowStyle CssClass="Alt" />
            <PagerStyle CssClass="Footer" />
        </asp:GridView>
        </div>        

        <div class="Buttons">
           
            <div class="ButtonHolder">
                <asp:Button ID="btnBack" CssClass="OButton" runat="server" Text="Back to Account Page"
                onclick="btnBack_Click" />
            </div>
            <div class="Clear"> </div>
        </div>

    </div>


  <asp:SqlDataSource ID="sqlCase" runat="server" 
     ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
    SelectCommand="sp_if_GetCaseByNo" SelectCommandType="StoredProcedure">
    <SelectParameters>
      <asp:QueryStringParameter DefaultValue="0" Name="CaseNo" QueryStringField="ID" 
        Type="Int32" />
    </SelectParameters>
  </asp:SqlDataSource>

  <asp:SqlDataSource ID="sqlCaseNotes" runat="server" 
   ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
    SelectCommand="sp_if_GetCaseRequestNotesByCaseNo" 
    SelectCommandType="StoredProcedure">
    <SelectParameters>
      <asp:QueryStringParameter DefaultValue="0" Name="CaseNo" QueryStringField="ID" 
        Type="Int32" />
    </SelectParameters>
  </asp:SqlDataSource>

</asp:Content>

