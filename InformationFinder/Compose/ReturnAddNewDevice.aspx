<%@ Page Title="" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InformationFinder_Compose_ReturnAddNewDevice" Codebehind="ReturnAddNewDevice.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
  <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

  <div style="width:750px" >

    <asp:UpdatePanel ID="UpdatePanelReturn" runat="server" >
        <ContentTemplate>
               
              <%--<div style="padding: 4px"><asp:Label ID="lblError" runat="server" CssClass="error2" ></asp:Label></div>--%>

            <div class="FormError" id="errors" runat="server" visible="false">
	            <p><span class="MessageIcon"></span>
	            <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
	        </div>

            <div class="OForm">

                <div class="Row">
                    <div class="Label ">Account #:</div>
                    <div class="LabelValue"><asp:Label ID="lblAccountNo" runat="server" ></asp:Label> (<asp:Label ID="lblAccountName" runat="server" ></asp:Label>)</div>
                    <div class="Clear"></div>
                </div>

                    <div class="Row">
                    <div class="Label ">RMA #:</div>
                    <div class="LabelValue"><asp:Label ID="lblReturnID" runat="server" ></asp:Label> </div>
                    <div class="Clear"></div>
                </div>

                <div class="Row">
                    <div class="Label ">Return Type:</div>
                    <div class="LabelValue">
                        <asp:Label ID="lblReturnType" runat="server" ></asp:Label>
                    </div>
                    <div class="Clear"></div>
                </div>

                <div class="Row">
                    <div class="Label ">Reason for Return:</div>
                    <div class="LabelValue">
                    <asp:Label ID="lblReturnReason" runat="server" ></asp:Label>
                    </div>
                    <div class="Clear"></div>
                </div>

                <div class="Row">
                    <div class="Label ">Notes<span class="Required">*</span>:</div>
                    <div class="Control">
                    <asp:TextBox ID="txtNotes" TextMode="MultiLine" CssClass = "Size XLarge2" runat="server"></asp:TextBox>
                    <span class="InlineError" id="lblNoteValidate" runat="server" visible="false"></span>
                    </div>
                    <div class="Clear"></div>
                </div>

                <div class="Row">
                    <div class="Label ">Added By:</div>
                    <div class="LabelValue"><asp:Label ID="lblCreatedBy" runat="server" ></asp:Label></div>
                    <div class="Clear"></div>
                </div>

                <div class="Row">
                    <div class="Label ">Serial #<span class="Required">*</span>:</div>
                    <div class="Control">
                        <asp:CheckBoxList ID="ckblSerialNumber" 
                            CellPadding="1"
                            CellSpacing="0"
                            RepeatColumns="8"
                            RepeatDirection="Horizontal"
                            RepeatLayout="Table"
                            TextAlign="Right"
                            ToolTip ="Check Serial#"
                            runat="server"  ForeColor="Black">
                        </asp:CheckBoxList>
                        <span class="InlineError" id="lblSerialNoValidate" runat="server" visible="false"></span>
                    </div>
                    <div class="Clear"></div>
                </div>

               
            </div>
            

            <div class="Buttons">
                <div class="RequiredIndicator"><span class="Required">*</span> Indicates a required field.</div>
                <div class="ButtonHolder">
                    <asp:Button ID="btnContinue" CssClass="OButton" runat="server" Text="Continue" onclick="btnContinue_Click" />
                    <asp:Button ID="btnCancel" CssClass="OButton" runat="server" Text="Back" onclick="btnCancel_Click" />
                </div>
                <div class="Clear"> </div>
            </div>


        </ContentTemplate>
    </asp:UpdatePanel>
  </div>

</asp:Content>

