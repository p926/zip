<%@ Control Language="C#" AutoEventWireup="true" Inherits="Controls_QuickMenu" Codebehind="QuickMenu.ascx.cs" %>

<div class="QuickMenu">
    <div class="Title"><asp:Label Text="" ID="lblDepartmentName" runat="server" /></div>
    <asp:BulletedList ID="bulletList" runat="server" DisplayMode="HyperLink" CssClass="QuickMenuList"
        DataTextField="Title" DataValueField="Link">
    </asp:BulletedList>
</div>