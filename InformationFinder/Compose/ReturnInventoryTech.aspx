<%@ Page Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="Instadose_InformationFinder_Compose_ReturnInventoryTech" Title="Receipt of Devices" Codebehind="ReturnInventoryTech.aspx.cs" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<script type="text/javascript">
$(document).ready(function() {
  
  if($('#ctl00_primaryHolder_txtSerialNos').text() == "") 
  {
      // Disable the continue button
      $('#ctl00_primaryHolder_btnContinue').attr("disabled", "true");
      
       // Disable the Edit Comment button
      $('#ctl00_primaryHolder_btnEdit').attr("disabled", "true");
  }
  else
  {
      // A new item has been scanned.
      var countDevices = countLines('ctl00_primaryHolder_txtSerialNos');
      $('#count').text("Devices scanned: " + countDevices);
  }
  // Set up the serial number validation
  $('#ctl00_primaryHolder_txtSerialNos').keyup(function(event) {
      
      // A new item has been scanned.
      var countDevices = countLines('ctl00_primaryHolder_txtSerialNos');
      $('#count').text("Devices scanned: " + countDevices);
      
      // Enable the continue button if the serials numbers counter is greater than 0.
      if(countDevices <= 0)
      {
        $('#ctl00_primaryHolder_btnContinue').attr("disabled", "true");
        $('#ctl00_primaryHolder_btnEdit').attr("disabled", "true");
      }
      else
      {
        $('#ctl00_primaryHolder_btnContinue').removeAttr('disabled');
        $('#ctl00_primaryHolder_btnEdit').removeAttr('disabled');
      }
      
  }).keypress(function(event) {
  
    // Validate the keys being pressed to ensure only numbers are entered.
    if (("0123456789").indexOf(String.fromCharCode(event.keyCode)) <= -1 && event.keyCode != '13')
    {
      // If not, cancel the event.
      event.preventDefault();
    }
  });
});

/// Count lines of a text box.
function countLines(id)
{
  var area = document.getElementById(id)
  if(area.value == "") return 0;
  // trim trailing return char if exists
  var text = area.value.replace(/\s+$/g,"")
  var split = text.split("\n")
  return split.length
}
</script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
  
  <asp:Panel ID="plAdd" runat="server">
      <div style="width: 240px" class="ui-widget ui-widget-content">
          
          <div class="FormError" id="errors" runat="server" visible="false" style="margin:10px" >
	           <p><span class="MessageIcon"></span>
	           <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
           </div>
            <div class="FormMessage" id="messages" runat="server" visible="false" style="margin:10px" > 
	            <p><span class="MessageIcon"></span>
	            <strong>Messages:</strong> <span id="submitMsg" runat ="server" >Ready to search.</span></p>
            </div> 

            <table class="OTable" style="width: 100%; border: 0; margin-bottom:10px;" >
            <tr>
              <td class="mt-itm" style="padding-left:10px"  >Please enter serial numbers below:</td>
            </tr>
            <tr>
              <td class="mt-itm "  style="padding-left:10px" > 
                <asp:TextBox ID="txtSerialNos" TextMode="MultiLine" CssClass="Notes" runat="server" Height="325px" Width="200px"></asp:TextBox>
              </td>
            </tr>
            <tr>
              <td class="mt-itm " style="padding-left:10px" >
                <div id="count">Devices scanned: 0</div>
              </td>
            </tr>
            <tr>
                <td class="mt-itm rightalign " style="padding-right:10px; " >
                    <asp:Button ID="btnContinue" CssClass="OButton" runat="server" Text="Add Devices" onclick="btnContinue_Click" />                           
                    <asp:Button ID="btnEdit" CssClass="OButton" runat="server" Text="Edit Comment" onclick="btnEdit_Click"  />
                </td>
            </tr>
          </table>

      </div>
  </asp:Panel>

  <asp:Panel ID="plReview" runat="server" Visible="false">
      <div >
          
            <div class="FormError" id="errorReview" runat="server" visible="false">
	            <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="errorReviewMsg" runat="server" >An error was encountered.</span>
                </p>
            </div>

            <asp:GridView ID="gvReview" runat="server" CssClass="OTable" 
                AutoGenerateColumns="false" DataKeyNames="SerialNo,AccountNo,RequestNo"  >

                <Columns>
                    <asp:BoundField DataField="SerialNo" HeaderText="Serial#" ItemStyle-CssClass="mt-itm rightalign" HeaderStyle-CssClass="mt-hd" />
                    <asp:BoundField DataField="AccountNo" HeaderText="Account#" ItemStyle-CssClass="mt-itm rightalign" HeaderStyle-CssClass="mt-hd" />
                    <asp:BoundField DataField="RequestNo" HeaderText="Request#" ItemStyle-CssClass="mt-itm rightalign" HeaderStyle-CssClass="mt-hd" />
                    <asp:BoundField DataField="RequestType" HeaderText="Request Type" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                    <asp:BoundField DataField="DepartmentRequest" HeaderText="Department" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                    <asp:BoundField DataField="DeviveStatus" HeaderText="Status" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />
                    <asp:BoundField DataField="Action" HeaderText="Action" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />
                    <asp:TemplateField HeaderText="Visual Inspect"  ItemStyle-CssClass="mt-itm centeralign" HeaderStyle-CssClass="mt-hd centeralign" >
                    <ItemTemplate>   
                        <asp:CheckBox ID="ChkBxVisualInspect" Checked="true" runat="server" />
                    </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Data Inspect"  ItemStyle-CssClass="mt-itm centeralign" HeaderStyle-CssClass="mt-hd centeralign" >
                    <ItemTemplate>   
                        <asp:CheckBox ID="ChkBxDataInspect" Checked="true" runat="server" />
                    </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Approve?"  ItemStyle-CssClass="mt-itm centeralign" HeaderStyle-CssClass="mt-hd centeralign" >
                    <ItemTemplate>   
                        <asp:CheckBox ID="ChkBxApprove" Checked="true" runat="server" />
                    </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Tech Notes" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" >
                    <ItemTemplate >   
                        <asp:TextBox ID="txtNotes" runat="server" Text='Good Condition'  SkinID="none" Width="205" TextMode="MultiLine" Height="65" />  
                    </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
					    <div class="NoData">
						    No records found!
					    </div>
				    </EmptyDataTemplate>  
                                                  
                    <AlternatingRowStyle CssClass="Alt" />
				    <PagerStyle CssClass="Footer" />
            </asp:GridView>

            <table class="OTable">

                <tr>
                    <th  class="mt-hd" >
                        Upload Attachment to All Request#
                    </th>
                </tr>
            
                <tr>
                    <td  class="mt-itm" >
                        Attachment 1: <asp:FileUpload ID="FileUpload1" runat="server" Width="350px" />
                    </td>                                                                    
                </tr>
                    
                <tr>
                    <td  class="mt-itm" >
                        Attachment 2: <asp:FileUpload ID="FileUpload2" runat="server" Width="350px" />
                    </td>                                                                    
                </tr>
                

            </table> 

            <div class="Left">
                User Name: <asp:Label ID="lblusername" runat="server" ></asp:Label>
            </div>
            

            <div class="Right"> 
                  <asp:Button ID="btnFinish" runat="server" CssClass="OButton" 
                      onclick="btnFinish_Click" Text="Acknowledge Receipt of Devices" />
                  <asp:Button ID="btnBack" runat="server" CssClass="OButton" 
                      onclick="btnBack_Click" Text="Back" />
                 
            </div>  
            
            <div class="Clear"></div> 
                    
      </div>
  </asp:Panel>
  
  <asp:Panel ID="plReviewEdit" runat="server" Visible="false">
      <div >          

          <div class="FormError" id="errorReviewEdit" runat="server" visible="false">
	            <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="errorReviewEditMsg" runat="server" >An error was encountered.</span>
                </p>
            </div>

            <asp:GridView ID="gvReviewEdit" runat="server" CssClass="OTable" 
                AutoGenerateColumns="false" DataKeyNames="ReturnInventoryID"  >
                <Columns>
                    <asp:BoundField DataField="ReturnInventoryID" HeaderText="ID" ItemStyle-CssClass="mt-itm rightalign" HeaderStyle-CssClass="mt-hd"  />
                    <asp:BoundField DataField="SerialNo" HeaderText="Serial#" ItemStyle-CssClass="mt-itm rightalign" HeaderStyle-CssClass="mt-hd" />
                    <asp:BoundField DataField="AccountNo" HeaderText="Account#" ItemStyle-CssClass="mt-itm rightalign" HeaderStyle-CssClass="mt-hd" />
                    <asp:BoundField DataField="RequestNo" HeaderText="Request#" ItemStyle-CssClass="mt-itm rightalign" HeaderStyle-CssClass="mt-hd" />
                    <asp:BoundField DataField="RequestType" HeaderText="Request Type" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                    <asp:BoundField DataField="Reviewer" HeaderText="ReviewBy" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                    <asp:BoundField DataField="ReviewDate" HeaderText="Last Review Date" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                    <asp:TemplateField HeaderText="Tech Notes" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" >
                    <ItemTemplate >   
                        <asp:TextBox ID="txtNotes" runat="server" Text='<%# Eval("TechNotes") %>'   SkinID="none" Width="235" TextMode="MultiLine" Height="55" />  
                    </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
					<div class="NoData">
						No records found!
					</div>
				</EmptyDataTemplate>  
                                                  
                <AlternatingRowStyle CssClass="Alt" />
				<PagerStyle CssClass="Footer" />
            </asp:GridView>

              
            <div class="Left">
                User Name: <asp:Label ID="lblusernameEdit" runat="server" ></asp:Label>
               </div>

            <div class="Right"> 
                  <asp:Button ID="btnFinishEdit" runat="server" CssClass="OButton" 
                      onclick="btnFinishEdit_Click"  Text="Update Device Comment" />
                  <asp:Button ID="btnBackEdit" runat="server" CssClass="OButton" 
                      onclick="btnBack_Click" Text="Back" />
                 
             </div>

             <div class="Clear"></div>
             
      </div>
  </asp:Panel>
</asp:Content>


