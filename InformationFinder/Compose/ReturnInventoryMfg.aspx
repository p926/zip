<%@ Page Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="Instadose_InformationFinder_Compose_ReturnInventoryMfg" Title="Return Receiving" Codebehind="ReturnInventoryMfg.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <script type="text/javascript">
        $(document).ready(function() {
  
          if($('#ctl00_primaryHolder_txtSerialNos').text() == "") 
          {
              // Disable the continue button
              $('#ctl00_primaryHolder_btnContinue').attr("disabled", "true");
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
                $('#ctl00_primaryHolder_btnContinue').attr("disabled", "true");
              else
                $('#ctl00_primaryHolder_btnContinue').removeAttr('disabled');
  
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
      <div style="width: 240px;" class="ui-widget ui-widget-content" >          

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
              <td class="mt-itm-bold" style="padding-left:10px"  >Please enter serial numbers below:</td>
            </tr>
            <tr>
              <td class="mt-itm "  style="padding-left:10px" > 
                <asp:TextBox ID="txtSerialNos" TextMode="MultiLine" CssClass="Notes" runat="server" Height="300px" Width="200px"></asp:TextBox>
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

              <asp:GridView ID="gvReview" runat="server" CssClass="OTable" AutoGenerateColumns="false" >
                  <Columns>

                      <asp:BoundField DataField="SerialNo" HeaderText="Serial#" ItemStyle-CssClass="mt-itm rightalign" HeaderStyle-CssClass="mt-hd" />

                      <asp:TemplateField HeaderText="Account#" ItemStyle-CssClass="mt-itm rightalign" HeaderStyle-CssClass="mt-hd" >
                        <ItemTemplate >   
                            <asp:Label ID="lbl_GvAccountNo" runat="server" Text='<%# DataBinder.Eval(Container.DataItem,"AccountNo","" ) %>' 
                            BackColor='<%#FuncCheckAccountNo(DataBinder.Eval(Container.DataItem,"AccountNo","" ))%>' > </asp:Label>
                        </ItemTemplate>
                      </asp:TemplateField>
                      
                      <asp:TemplateField HeaderText="Request#" ItemStyle-CssClass="mt-itm rightalign" HeaderStyle-CssClass="mt-hd" >
                        <ItemTemplate >   
                           <asp:Label ID="lbl_GvRequestNo" runat="server" Text='<%# Eval("RequestNo") %>' 
                           BackColor='<%#FuncCheckRequestNo(DataBinder.Eval(Container.DataItem,"RequestNo","" ))%>' > </asp:Label>
                        </ItemTemplate>
                      </asp:TemplateField>

                      <asp:BoundField DataField="DepartmentRequest" HeaderText="Department" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                      
                      <asp:BoundField DataField="Action" HeaderText="Action" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />

                      <asp:TemplateField HeaderText="Notes/Comment" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" >
                        <ItemTemplate >   
                           <asp:TextBox ID="txtNotes" runat="server" Text='Good Condition'  SkinID="none" Width="275" TextMode="MultiLine" Height="35" />  
                        </ItemTemplate>
                      </asp:TemplateField>
                                           
                  </Columns>

                  <EmptyDataTemplate>
					    <div class="NoData">
						    There are no Return devices in this request!
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
                <asp:Button ID="btnFinish" runat="server"  CssClass="OButton"
                      onclick="btnFinish_Click" Text="Receive Devices" />
                  <asp:Button ID="btnBack" runat="server" CssClass="OButton" 
                      onclick="btnBack_Click" Text="Back" />
                 
             </div>

             <div class="Clear"></div>
          
      </div>
  </asp:Panel>

    
</asp:Content>

