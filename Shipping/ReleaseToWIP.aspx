<%@ Page Title="Release Instadose Request" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="Shipping_ReleaseToWIP" Codebehind="ReleaseToWIP.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></act:ToolkitScriptManager> 

    <div>

		<%--<asp:UpdatePanel ID="udpn" runat="server"  UpdateMode="Conditional">
			
			<ContentTemplate>                
                								
			</ContentTemplate>
		</asp:UpdatePanel>--%>   

        <div class="FormError" id="errors" runat="server" visible="false" style="margin:10px" >
	        <p><span class="MessageIcon"></span>
	        <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
        </div>
        <div class="FormMessage" id="messages" runat="server" visible="false" style="margin:10px" > 
	        <p><span class="MessageIcon"></span>
	        <strong>Messages:</strong> <span id="submitMsg" runat ="server" >Ready to search.</span></p>
        </div> 
        
        <div class="OToolbar JoinTable" id="ReleaseToolBar" runat ="server">
			<ul>
                                            
                <li>
                    <asp:LinkButton ID="btn_refreshPickSheet" runat="server" 
						 CssClass="Icon Refresh" onclick="btnRefresh_Click" >Refresh Page</asp:LinkButton>                   
                </li>
                        
			</ul>
		</div>   				

        <asp:GridView ID="gvQADevice" CssClass="OTable"  runat="server" Visible ="true"
            AllowPaging="true" PageSize = "20" AllowSorting="true"
            AutoGenerateColumns="False" DataKeyNames="QARequestID" DataSourceID="GetQARequest" 
            OnRowDataBound="gvQADevice_RowDataBound" >

            <Columns>                                                                                                              
                            
                    <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  HeaderText="">
                        <ItemTemplate>
                            <asp:CheckBox ID="cbRow" runat="server" />
                            <asp:HiddenField ID="HidQARequestID" runat="server" Value='<%# Eval("QARequestID") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField DataField="ReceiptID" HeaderText="Receipt#" SortExpression="ReceiptID" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />                           
                    <asp:BoundField DataField="RequestItemNumber" HeaderText="Item#" SortExpression="RequestItemNumber" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />   
                    <asp:BoundField DataField="RequestTotal" HeaderText="Total" SortExpression="RequestTotal" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />   
                    <asp:BoundField DataField="RequestBy" HeaderText="Request By" SortExpression="RequestBy" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />   
                    <asp:BoundField DataField="RequestDate" HeaderText="Request Date" SortExpression="RequestDate" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  DataFormatString="{0:d}" />   
                    <asp:BoundField DataField="ReleaseBy" HeaderText="Release By" SortExpression="ReleaseBy" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />   
                    <asp:BoundField DataField="ReleaseDate" HeaderText="Release Date" SortExpression="ReleaseDate" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  DataFormatString="{0:d}" />                                                                                                       

            </Columns>

            <EmptyDataTemplate>
				<div class="NoData">
					No request from Tech-Ops has been made!
				</div>
			</EmptyDataTemplate>                                
            <AlternatingRowStyle CssClass="Alt" />
			<PagerStyle CssClass="Footer" />
                        
        </asp:GridView>	

        <div class="Buttons">
            <%--<div class="RequiredIndicator"><span class="Required">*</span> Indicates a required field.</div>--%>
            <div class="ButtonHolder">                    
                <asp:Button ID="btnRelease" CssClass="OButton" runat="server" Text="Release" onclick="btnRelease_Click" />                                             
            </div>
            <div class="Clear"> </div>
        </div> 
					
	</div>

    <asp:SqlDataSource ID="GetQARequest" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand=" 
                        SELECT *
                        FROM QARequest
                        Order By RequestDate desc, ReceiptID desc
                      ">
    </asp:SqlDataSource> 

</asp:Content>

