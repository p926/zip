<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="IT_DeviceProfiles" Codebehind="DeviceProfiles.aspx.cs" %>
<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Src="~/Controls/ErrorLabel.ascx" TagName="ErrorLabel" TagPrefix="id" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">

<div>
<!-- Grid View -->
    <p><a href="#" id="dialog_link" class="ui-state-default ui-corner-all OButton"><span class="ui-icon ui-icon-plusthick"></span>Add Profile</a></p>

    <ec:GridViewEx ID="gvProfiles" runat="server" AutoGenerateColumns="false" CssClass="OTable" DataSourceID="sqlDeviceProfiles"
            AlternatingRowStyle-CssClass="Alt" PagerStyle-CssClass="mt-hd" EmptyDataText="There are no device profiles.">           
        <Columns>
           
            <asp:TemplateField HeaderText="Profile Name">
                <ItemTemplate>
                    <asp:HyperLink ID="hlEditDeviceProfile" runat="server" 
                        NavigateUrl='<%# DataBinder.Eval(Container.DataItem,"DeviceProfileID","EditDeviceProfile.aspx?ID={0}" ) %>' 
                        Target="_blank" Text='<%# Bind("DeviceProfileName") %>'></asp:HyperLink>
                
                </ItemTemplate>
            </asp:TemplateField>

            <asp:BoundField DataField="AdvertTime" HeaderText="Advert Time" />
            <asp:BoundField DataField="ConnectionTime" HeaderText="Conn Time" />
            <asp:BoundField DataField="DeviceModeName" HeaderText="Mode" />
            <asp:BoundField DataField="MeasLatency" HeaderText="Meas Lat" />
            <asp:BoundField DataField="KeyLatency" HeaderText="Kat Lat" />
            <asp:BoundField DataField="AdvancedLatency" HeaderText="Advert Lat" />
            <asp:BoundField DataField="CommRetries" HeaderText="Comm Retries" />
            <asp:BoundField DataField="CommInterval" HeaderText="Comm Int" />
            <asp:BoundField DataField="DiagInterval" HeaderText="Diag Int" />
            <asp:BoundField DataField="DiagAdvInterval" HeaderText="Diag Adv Int" />
            <asp:BoundField DataField="ChamberInfo" HeaderText="Chamber" />
                                             
        </Columns>
    </ec:GridViewEx>


</div>



    <asp:SqlDataSource ID="sqlDeviceProfiles" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>" 
        SelectCommand="SELECT DeviceProfiles.*, DeviceModes.OperationModeDesc AS DeviceModeName FROM DeviceProfiles LEFT OUTER JOIN DeviceModes ON DeviceProfiles.DeviceModeID = DeviceModes.DeviceModeID"></asp:SqlDataSource>

</asp:Content>

