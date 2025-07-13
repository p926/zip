<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="True" Inherits="IT_ID1ClearLabel" Codebehind="ID1ClearLabel.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="server">
    <script type="text/javascript">

        //on page load
        $(document).ready(function () {
            
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(ajaxLoad);
            ajaxLoad();

        });        

          function ajaxLoad() {              

              // selected serialno boxes.
              $('.chkbxHeadercc').click(function () {
                  var ischecked = ($('#ctl00_primaryHolder_grdUsers_ctl01_chkbxHeader').is(":checked"));
                  // add attribute 
                  $('#ctl00_primaryHolder_grdUsers input:checkbox').attr("checked", function () {
                      this.checked = ischecked;
                  });
              });

          }

    </script> 
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="server" >
    <act:toolkitscriptmanager ID="ToolkitScriptManager1" runat="server">
    </act:toolkitscriptmanager>

    <div style="color: #0000FF">This page is used to print a custom label with your inputs. Please enter your input for a label print-out.</div>
    <br />        

    <div >
        <asp:UpdatePanel ID="upnlPrinting" runat="server" UpdateMode="Conditional">
            <ContentTemplate>                            
                
                <div class="FormError" id="dialogErrors" runat="server" visible="false">
					<p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="dialogErrorMsg" runat="server" >An error was encountered.</span></p>
				</div>
                <div class="FormMessage" id="dialogSuccess" runat="server" visible="false" style="margin:10px" > 
	                <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="dialogSuccessMsg" runat ="server" >Ready to search.</span></p>
                </div> 

                <div class="OForm" >                                                  
                    
                    <div class="Row">
                        <div class="Label Medium2">Acc #:</div>
                        <div class="Control">
                            <asp:TextBox runat="server" ID="txtAccountID" AutoPostBack ="true" 
                               CssClass="Size Small " ValidationGroup="form" OnTextChanged="txtAccountID_TextChanged" />                            
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Medium2">Company Name:</div>
                        <div class="Control"><asp:TextBox runat="server" ID="txtAccountName" 
                                CssClass="Size Large " ValidationGroup="form" /></div>
                        <div class="Clear"></div>
                    </div>   
                    
                    <div class="Row">
                        <div class="Label Medium2">Locations:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlUserLocation" runat="server" AutoPostBack="true"                                
                                onselectedindexchanged="ddlUserLocation_SelectedIndexChanged" />
                        </div>
                        <div class="Clear"></div>
                    </div>   
                    
                    <div class="Row">
                        <div class="Label Medium2">Users:</div>
                        <div class="Control">
                            <asp:GridView ID="grdUsers" runat="server" AutoGenerateColumns="False" 
                                DataSourceID="sqlUserList" CssClass="OTable"  DataKeyNames="UserID"
                                AllowSorting="true" >
                                <Columns>
                                    <asp:TemplateField >
                                        <HeaderTemplate>
                                            <asp:CheckBox runat="server" ID="chkbxHeader" ToolTip="Select All Users" class="chkbxHeadercc" />
                                        </HeaderTemplate>
                                        <HeaderStyle Width="20px" Wrap="false" />
                                        <ItemTemplate>
                                            <asp:CheckBox id="chkbxSelect" runat="server" class="chkbxRow" />
                                            <asp:HiddenField runat="server" ID="HidUserID"  Value='<%# Eval("UserID") %>' />                                                                                
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    
                                    <asp:BoundField DataField="FirstName" HeaderText="First Name" SortExpression="FirstName" />
                                    <asp:BoundField DataField="MiddleName" HeaderText="Middle Name" SortExpression="MiddleName"  />
                                    <asp:BoundField DataField="LastName" HeaderText="Last Name" SortExpression="LastName"  />
                                    <asp:BoundField DataField="UserName" HeaderText="UserName" SortExpression="UserName" />
                                    <asp:BoundField DataField="UserID" HeaderText="UserID" SortExpression="UserID" Visible="false" />        
                                                                                
                                </Columns>
                                <EmptyDataTemplate>
                                    <div class = "NoData">
                                        There are no users for the selected location.
                                    </div>
                                </EmptyDataTemplate>
                                <AlternatingRowStyle CssClass="Alt" />
                                <PagerStyle CssClass="Footer" />
                            </asp:GridView>
                        </div>
                        <div class="Clear"></div>
                    </div>   
                    
                    <div class="Row">
                        <div class="Label Medium2">Label Format:</div>
                        <asp:DropDownList ID="ddlLabelFormat" runat="server" >
                            <asp:ListItem Text="First+Last" Value="1" />
                            <asp:ListItem Text="Company+FirstLast" Value="2" /> 
                            <asp:ListItem Text="Company Only" Value="3" />                           
                        </asp:DropDownList> 
                        <div class="Clear"></div>
                    </div>             

                </div>                    
                           
                <div class="Row">
                    <div class="Label Medium2">&nbsp;</div>
                    <div class="Control">
                        <asp:Button Text="Print" ID="btnPrint" runat="server" 
                               ValidationGroup="form" cssClass="OButton" 
                            onclick="btnPrint_Click" /> &nbsp;&nbsp;&nbsp;
                        <asp:Button Text="Reset" ID="btnReset" runat="server" 
                               ValidationGroup="form" cssClass="OButton" 
                            onclick="btnReset_Click" />
                    </div>
                    <div class="Clear"></div>
                </div> 
                                  
                
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>


    <asp:SqlDataSource ID="sqlUserList" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="sp_GetUsersByLocation" 
        SelectCommandType="StoredProcedure">
        <SelectParameters>            
            <asp:ControlParameter ControlID="txtAccountID" DefaultValue="0" 
                        Name="AccountID" PropertyName="Text" Type="Int32" /> 
            <asp:ControlParameter ControlID="ddlUserLocation" DefaultValue="0" 
                        Name="LocationID" PropertyName="SelectedItem.Value" Type="Int32" /> 
        </SelectParameters>
    </asp:SqlDataSource>

</asp:Content>

