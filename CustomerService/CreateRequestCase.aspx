<%@ Page Title="Request Case" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="CustomerService_CreateRequestCase" Codebehind="CreateRequestCase.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <script type="text/javascript" charset="utf-8">
        $(document).ready(function () {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(ajaxLoad);
            ajaxLoad();
        });

        function ajaxLoad() {
            // Accordion
            $("#accordion").accordion({ header: "h3",
                autoHeight: false
            });

            // Request Date Datepicker
            $('#<%=txtRequestDate.ClientID %>').datepicker();

            // Resolved Date Datepicker
            $('#<%=txtResolvedDate.ClientID %>').datepicker();
        }
    </script>  
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
<act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></act:ToolkitScriptManager>
<div style="width: 100%" >
    <asp:UpdatePanel ID="upnlCaseDetail" runat="server" >
        <ContentTemplate>
            <div class="FormError" id="errors" runat="server" visible="false">
	            <p><span class="MessageIcon"></span>
	            <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
            </div>
            <div class="FormMessage" id="messages" runat="server" visible="false"> 
	            <p><span class="MessageIcon"></span>
	            <strong>Messages:</strong> <span id="submitMsg" runat ="server" >Ready to search.</span></p>
            </div>
            <div class="OForm">
                <div class="Row">
                    <div class="Label  " >Case #:</div>
                    <div class="LabelValue">
                        <asp:Label runat="server" ID="lblCaseNumber" Font-Bold="true"></asp:Label>   
                    </div>
                    <div class="Clear"></div>
                </div>
                <div class="Row">
                    <div class="Label ">Request Date<span class="Required">*</span>:</div>
                    <div class="Control">
                        <asp:TextBox ID="txtRequestDate" runat="server"  CssClass="Size Small" ></asp:TextBox>
                        <span class="InlineError" id="lblRequestDateValidate" runat="server" visible="false"></span>                                   
                    </div>
                    <div class="Clear"></div>
                </div>
        <div class="Row">
            <div class="Label ">Status:</div>
            <div class="Control">
                <asp:DropDownList ID="ddlCaseStatus" runat="server" 
                    DataValueField="caseStatusID" DataTextField="CaseStatusDesc" />
                <span class="InlineError" id="lblCaseStatusValidate" runat="server" visible="false"></span>                          
            </div>
            <div class="Clear"></div>
        </div>
               
        <div class="Row">
            <div class="Label ">Account #<span class="Required">*</span>:</div>
            <div class="Control">
                <asp:TextBox ID="txtAccountID" runat="server" Text="" AutoPostBack ="true" ></asp:TextBox>
                <span class="InlineError" id="lblAccountIDValidate" runat="server" visible="false"></span>                          
            </div>
            <div class="Clear"></div>
        </div>

        <div class="Row">
            <div class="Label ">Order #:</div>
            <div class="Control">
                <asp:DropDownList ID="ddlOrder" runat="server" 
                        DataValueField="OrderID" DataTextField="OrderID"  />
                <span class="InlineError" id="lblOrderIDValidate" runat="server" visible="false"></span>                          
            </div>
            <div class="Clear"></div>
        </div>

        <div class="Row">
            <div class="Label ">User:</div>
            <div class="Control">
                <asp:DropDownList ID="ddlAcctUserName" runat="server" 
                        DataValueField="userID" DataTextField="FullName" />
                <span class="InlineError" id="lblUserValidate" runat="server" visible="false"></span>                          
            </div>
            <div class="Clear"></div>
        </div>

        <div class="Row">
            <div class="Label ">Badge #:</div>
            <div class="Control">
                <asp:DropDownList ID="ddlSerialno" runat="server" 
                        DataValueField="deviceid" DataTextField="Serialno" />
                <span class="InlineError" id="lblSerialNoValidate" runat="server" visible="false"></span>                          
            </div>
            <div class="Clear"></div>
        </div>
                
        <div class="Row">
            <div class="Label ">Request Type:</div>
            <div class="Control">
                <asp:DropDownList ID="ddlTypeRequest" runat="server"  AutoPostBack="true"
                    DataValueField="RequestID" DataTextField="RequestDesc"  />
                <span class="InlineError" id="lblTypeRequestValidate" runat="server" visible="false"></span>                                                                                                                                              
            </div>
            <div class="Clear"></div>
        </div>

        <div class="Row">
            <div class="Label ">Request Reason:</div>
            <div class="Control">
                <asp:DropDownList ID="ddlRequestReason" runat="server" 
                    DataValueField="ReasonID" DataTextField="ReasonDesc" Visible="true" />
                <span class="InlineError" id="lblRequestReasonValidate" runat="server" visible="false"></span>                                                                                                          
            </div>
            <div class="Clear"></div>
        </div>

        <div class="Row">
            <div class="Label ">Reason:</div>
            <div class="Control">
                <asp:TextBox ID="txtReasonNotes" Width="550" Height="40" runat="server" TextMode="MultiLine"  MaxLength="2000" ></asp:TextBox>              
                <span class="InlineError" id="lblReasonNoteValidate" runat="server" visible="false"></span>                          
            </div>
            <div class="Clear"></div>
        </div>
                       
        <div class="Row">
            <div class="Label ">Forward Request To:</div>
            <div class="Control">
                <asp:DropDownList ID="ddlForwardUser" runat="server" 
                    DataValueField="UserID" DataTextField="FullName"  />
                <asp:CheckBox ID="chkbxSendEmail" runat="server" Checked="true" Text="Send Email"/>               
            </div>
            <div class="Clear"></div>
        </div>

        <div class="Row">
            <div class="Label ">Request Notes:</div>
            <div class="Control">
                <asp:TextBox ID="txtNotes" Width="550" Height="60" runat="server" TextMode="MultiLine" MaxLength="2000" ></asp:TextBox>
                <span class="InlineError" id="lblNoteValidate" runat="server" visible="false"></span>  
            </div>
            <div class="Clear"></div>
        </div>

        <div class="Row">
            <div class="Label ">Resolved Date:</div>
            <div class="Control">
                <asp:TextBox ID="txtResolvedDate" runat="server" Text="" ></asp:TextBox>  
                <span class="InlineError" id="lblResolvedDateValidate" runat="server" visible="false"></span>             
            </div>
            <div class="Clear"></div>
        </div>

        <div class="Row">
            <div class="Label ">Resolved By:</div>
            <div class="Control">
                <asp:DropDownList ID="ddlResolvedBy" runat="server" 
                    DataValueField="UserID" DataTextField="FullName"  />                                  
            </div>
            <div class="Clear"></div>
        </div>

        <div runat="server" id="DivCaseHistory" visible="false">

			<h3>Request Notes History</h3>

            <div >
                 <asp:GridView CssClass="OTable" AllowSorting="true" AllowPaging="true" PageSize="10" Width="100%"
                    ID="cvCaseNotes" AutoGenerateColumns="false" runat="server" DataSourceID="sqlCaseNotes">
                    <Columns>
                        <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="CreatedDate" DataFormatString="{0:d}"
                            HeaderText="Created" InsertVisible="False" ReadOnly="True" SortExpression="CreatedDate" HeaderStyle-Width="100" />
                        <asp:BoundField ItemStyle-CssClass="mt-itm" ItemStyle-Wrap="false" HeaderStyle-CssClass="mt-hd" DataField="CallerName"
                            HeaderText="From" InsertVisible="False" ReadOnly="True" SortExpression="CallerName" HeaderStyle-Width="120" />
                        <asp:BoundField ItemStyle-CssClass="mt-itm" ItemStyle-Wrap="false" HeaderStyle-CssClass="mt-hd" DataField="ForwardTo"
                            HeaderText="Forward To" InsertVisible="False" ReadOnly="True" SortExpression="ForwardTo" HeaderStyle-Width="120" />

                        <asp:TemplateField >
                            <HeaderTemplate>Request Notes</HeaderTemplate>
                            <ItemTemplate>
                                <asp:Label ID="lbl" runat="server" Text='<%# Eval("CaseNote") %>' Width="630"></asp:Label>
                            </ItemTemplate>

                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="NoData">
                            There are no customer service request notes.
                        </div>
                    </EmptyDataTemplate>
                    <AlternatingRowStyle CssClass="Alt" />
                    <PagerStyle CssClass="Footer" />

                </asp:GridView>
            </div>

        </div>
    </div>
        
        
 

    <div class="Buttons">
        <div class="RequiredIndicator"><span class="Required">*</span> Indicates a required field.</div>
        <div class="ButtonHolder">
            <asp:Button ID="btnSaveCase" runat="server" Text="Save" onclick="btnSaveCase_Click" CssClass="OButton" />
            <asp:Button ID="btnCancel" runat="server" CssClass="OButton" Text="Back to All Requests" onclick="btnCancel_Click" />
            <asp:Button ID="btnBacktoAccount" runat="server" CssClass="OButton" Text="Back to Account" onclick="btnBacktoAccount_Click" />
        </div>
        <div class="Clear"> </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>

</div>   


    <asp:SqlDataSource ID="sqlCaseNotes" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="sp_if_GetCaseRequestNotesByCaseNo" 
        SelectCommandType="StoredProcedure">
        <SelectParameters>
            <asp:QueryStringParameter DefaultValue="0" Name="CaseNo" QueryStringField="caseid" 
            Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>

</asp:Content>

