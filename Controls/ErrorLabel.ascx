<%@ Control Language="C#" AutoEventWireup="true" Inherits="Controls_ErrorLabel" Codebehind="ErrorLabel.ascx.cs" %>
<asp:Panel ID="Message" CssClass="ui-widget" style="padding: 10px 0;" 
    Visible="False" runat="server" meta:resourcekey="MessageResource1">
	<div class="ui-state-error ui-corner-all" style="padding: 1px 8px;"> 
		<p><span class="ui-icon ui-icon-alert" style="float: left; margin-right: 4px; margin-top: 3px;"></span> 
		<strong><asp:Localize runat="server" meta:resourcekey="locErrorTitle" Text="Error:" /></strong> 
            <asp:Label runat="server" ID="lblMessage" 
                meta:resourcekey="lblMessageResource1" /></p>
	</div>
</asp:Panel>
