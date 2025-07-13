<%@ Page Title="Load Multi Users to Account" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="IT_MultiUsersInsert" Codebehind="MultiUsersInsert.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <div class="FormError" id="errors" runat="server" visible="false" style="margin:10px" >
	    <p><span class="MessageIcon"></span>
	    <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
    </div>
    <div class="FormMessage" id="messages" runat="server" visible="false" style="margin:10px" > 
	    <p><span class="MessageIcon"></span>
	    <strong>Messages:</strong> <span id="submitMsg" runat ="server" >Ready to search.</span></p>
    </div>    

    <div>This tool is used to upload mass users to an account by your prepared CSV file.<br />
    If you do not have a <b>sample CSV file</b>, please click on <a href="SampleUsers.csv">Download</a> to download it.</div>
    <br />
    <div>
         <table >
            <tr>
                <td>
                    CSV File:
                </td>
                <td>
                    <asp:FileUpload ID="FileUpload1" runat="server" Width="300px" />
                </td>
            </tr>
            <tr>
                <td>
                    
               </td>
                <td>
                    <asp:Button ID="Button1" runat="server" Text="Load Users" 
                        onclick="Button1_Click"  />
                </td>
            </tr>
        </table>
    </div>  
   
    <br />
    <br />
    
    <div id="successSection" runat="server" style="width: 850px">
        <table cellpadding="0" cellspacing="0" class="tbl1" style="width: 850px;">
            <tr>
                <td style="color: blue">
                    Success to insert users list:
                </td>
            </tr>
            <tr>
                <td>
                    <asp:GridView ID="GridViewSuccess" runat="server" CssClass="OTable" AutoGenerateColumns="False" >

                         <Columns>
                
                            <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="AccountID"
                                HeaderText="Account #" SortExpression="AccountID" />
                            <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="LocationID"
                                HeaderText="Location #" SortExpression="LocationID" />
                            <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="GroupID"
                                HeaderText="Group #" SortExpression="GroupID" />
                            <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="FirstName"
                                HeaderText="First Name" SortExpression="FirstName" />
                            <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="LastName"
                                HeaderText="Last Name" SortExpression="LastName" />
                            <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="UserName"
                                HeaderText="User Name" SortExpression="UserName" />
                            <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="Password"
                                HeaderText="Password" SortExpression="Password" />
                                       
                        </Columns>

                         <RowStyle Font-Size="Small"  />
                         <HeaderStyle Font-Bold="true" Font-Size="Small" />
                         <AlternatingRowStyle CssClass="Alt" />

                    </asp:GridView>
                </td>
            </tr>           
        </table>        
    </div>
    <br />
    <div id="failSection" runat="server" style="width: 850px">
        <table cellpadding="0" cellspacing="0" class="tbl1" style="width: 850px;">
            <tr>
                <td style="color: red">
                    Failure to insert users list:
                </td>
            </tr>
            <tr>
                <td>
                    <asp:GridView ID="GridViewFail" runat="server" CssClass="OTable" AutoGenerateColumns="False" >

                         <Columns>
                
                            <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="FirstName"
                                HeaderText="First Name" SortExpression="FirstName" />
                            <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="LastName"
                                HeaderText="Last Name" SortExpression="LastName" />
                            <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="ErrorMessage"
                                HeaderText="Error Message" SortExpression="ErrorMessage" />
                                       
                        </Columns>

                         <RowStyle Font-Size="Small"  />
                         <HeaderStyle Font-Bold="true" Font-Size="Small" />
                         <AlternatingRowStyle CssClass="Alt" />

                    </asp:GridView>
                </td>
            </tr>           
        </table>
    </div>
</asp:Content>

