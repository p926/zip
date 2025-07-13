<%@ Page Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="TechOps_AssignGroupColor" Title="Assign Group" Codebehind="AssignGroupColor.aspx.cs" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
	<script type="text/javascript">
		$(document).ready(function() {
  
		  if($('#ctl00_primaryHolder_txtSerialNos').text() == "") 
		  {
			  // Disable the continue button
			  $('#ctl00_primaryHolder_btnAssign').attr("disabled", "true");                   
		  }
		  else
		  {
			  // A new item has been scanned.
			  var countDevices = countLines('ctl00_primaryHolder_txtSerialNos');
			  $('#count').text(countDevices);
		  }
		  // Set up the serial number validation
		  $('#ctl00_primaryHolder_txtSerialNos').keyup(function(event) {
	  
			  // A new item has been scanned.
			  var countDevices = countLines('ctl00_primaryHolder_txtSerialNos');
			  $('#count').text(countDevices);
	  
			  // Enable the continue button if the serials numbers counter is greater than 0.
			  if(countDevices <= 0)
			  {
				$('#ctl00_primaryHolder_btnAssign').attr("disabled", "true");               
			  }
			  else
			  {
				$('#ctl00_primaryHolder_btnAssign').removeAttr('disabled');               
			  }
	  
		  }).keypress(function(event) {
			  // Get the keycode between FireFox and other browsers.
			  var key = (event.keyCode ? event.keyCode : event.which);

			  // Allow: backspace, delete, tab, escape, and enter
			  if (key == 46 || key == 8 || key == 9 || key == 27 || key == 13 ||
				  // Allow: Ctrl+A
				  (key == 65 && event.ctrlKey === true) ||
				  // Allow: home, end, left, right
				  (key >= 35 && key <= 39)) {
				  // let it happen, don't do anything
				  return;
			  }
			  else {
				  // Ensure that it is a number and stop the keypress
				  if (event.shiftKey || (key < 48 || key > 57)) {

					  event.preventDefault();
				  }
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

  <act:toolkitscriptmanager ID="ToolkitScriptManager1" runat="server" />   
  
  <asp:Panel ID="plAdd" runat="server">

		<div class="FormError" id="errors" runat="server" visible="false" style="margin:10px" >
			<p><span class="MessageIcon"></span>
			<strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
		</div>
		<div class="FormMessage" id="messages" runat="server" visible="false" style="margin:10px" > 
			<p><span class="MessageIcon"></span>
			<strong>Messages:</strong> <span id="submitMsg" runat ="server" >Ready to search.</span></p>
		</div>             

		 <div class="OForm" id = "mainForm" runat = "server">                                          
			 
			<div class="Row" >
				<div class="Label Small">Group Name<span class="Required">*</span>:</div>
				<div class="Control"  ><asp:TextBox ID="txtGroup" runat="server" CssClass="Size Small" /> 
					<span style="display:inline" >(Enter a group name if you like to assign all the badges to this new group name)</span>               
				</div>
				<div class="Clear"></div>
			</div>
					  
			<div class="Row">
				<div class="Label Small">Group Name<span class="Required">*</span>:</div>
				<div class="Control"><asp:DropDownList ID="ddlGroup" runat="server" CssClass="Size Small"  />                 
					<span style="display:inline">(Select a group name if you like to assign all the badges to the selected group name)</span>              
				</div>
				<div class="Clear"></div>
			</div>
			<%--UNCHECKED BY DEFAULT (ON PAGE LOAD)--%>
			<div class="Row" >
				<div class="Label Small"></div>
				<div class="Control"><asp:CheckBox ID="chkBoxDeleteReads" runat="server" Text = "Delete Reads" Checked="false" />
					<span style="display:inline" >(By checking the box, you decide to delete all reads of every device belong to selected group)</span>               
				</div>
				<div class="Clear"></div>
			</div>
			<%--END--%>
			<div class="Row">
				<div class="Label Small">Device Profile<span class="Required">*</span>:</div>
				<div class="Control"><asp:DropDownList ID="ddlDeviceProfile" runat="server"   />                                                   
				</div>
				<div class="Clear"></div>
			</div>

			<div class="Row">
				<div class="Label Small">Badge S/N<span class="Required">*</span>:</div>
				<div class="Control"><asp:TextBox runat="server" ID="txtSerialNos" TextMode="MultiLine"
						CssClass="Size Medium2" Style="height: 520px;" />                                              
				</div>
				<div class="Clear"></div>
			</div> 

			<div class="Row">
				<div class="Label Small">Badge#:</div>
				<div class="Control"><span id="count" style="padding-top: 5px;">0</span>                                         
				</div>
				<div class="Clear"></div>
			</div> 
							 
			<div class="Row">
				<div class="Label Small">&nbsp;</div>
				<div class="Control">
					<asp:Button Text="Assign Badge" ID="btnAssign" runat="server" 
							cssClass="OButton" onclick="btnAssign_Click"  />
				</div>
				<div class="Clear"></div>
			</div>                                        

		</div>


  </asp:Panel>

</asp:Content>


