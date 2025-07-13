<%@ Page Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="Instadose_InformationFinder_Details_ReturnList" Title="Return Request List" Codebehind="ReturnList.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <asp:Button ID="btnPrint" runat="server" Text="Print Selected RMA" 
        onclick="btnPrint_Click" />
   <asp:GridView CssClass="gv1" AlternatingRowStyle-CssClass="alt" ID="gvReturnDetails"
      AutoGenerateColumns="False" runat="server" DataSourceID="sqlReturn" 
      DataKeyNames="ReturnID"   AllowSorting="true" >
       <Columns>
       
       <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" 
          FooterStyle-HorizontalAlign="center"  ItemStyle-HorizontalAlign="center" HeaderText="">
           <ItemTemplate >   
            <asp:CheckBox ID="cbx" Checked="false" runat="server" />
           </ItemTemplate>
       </asp:TemplateField>
        <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="returnid"
        HeaderText="Return ID" SortExpression="returnid" />
        
       <asp:BoundField ItemStyle-CssClass="mt-itm" ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="mt-hd" DataField="CreatedDate"
           DataFormatString="{0:d}" HeaderText="Created Date" SortExpression="CreatedDate" />
       <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="CreatedBy"
        HeaderText="Created By" SortExpression="CreatedBy" />
       <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="Type"
        HeaderText="Request Type" SortExpression="Type" />
       <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="Reason"
        HeaderText="Reason" SortExpression="Reason" />
       <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="Status"
        HeaderText="Stage" ReadOnly="True" SortExpression="Status" />
       <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" 
        HeaderText="Serial#">
        <ItemTemplate>
        <asp:Label ID="lblSerialNoString" runat="server" 
          Text='<%#FuncTrimSerialNo(DataBinder.Eval(Container.DataItem,"MYserialnoString","" ))%>' > </asp:Label>
        </ItemTemplate>
       </asp:TemplateField>
       <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="DeviceCount"
        HeaderText="Devices" SortExpression="DeviceCount" />   
       
      </Columns>
    </asp:GridView>
    
    
    <asp:SqlDataSource ID="sqlReturn" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.AppConnectionString %>"
    SelectCommand="sp_if_GetReturnListByDate" SelectCommandType="StoredProcedure">
    <SelectParameters>
        <asp:Parameter Name="FromDate"  DefaultValue="1/1/1990" Type="String" />
        <asp:Parameter Name="ToDate" Type="String" DefaultValue="1/1/1990" />
    </SelectParameters>
  </asp:SqlDataSource>
    
    
</asp:Content>

