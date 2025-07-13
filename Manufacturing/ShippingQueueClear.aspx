<%@ Page Title="Clear Shipping Queue" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="CustomerService_ShippingQueueClear" Codebehind="ShippingQueueClear.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <script type="text/javascript">
        $(document).ready(function () {

            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
            JQueryControlsLoad();

            if ($('#ctl00_primaryHolder_txtMMultiPackageID').text() == "") {
                // Disable the continue button
                $('#ctl00_primaryHolder_btnMSave').attr("disabled", "true");
            }
            else {
                // A new item has been scanned.
                var countDevices = countLines('ctl00_primaryHolder_txtMMultiPackageID');
                //$('#count').text("Devices scanned: " + countDevices);
            }
            // Set up the serial number validation
            $('#ctl00_primaryHolder_txtMMultiPackageID').keyup(function (event) {

                // A new item has been scanned.
                var countDevices = countLines('ctl00_primaryHolder_txtMMultiPackageID');
                //$('#count').text("Devices scanned: " + countDevices);

                // Enable the continue button if the serials numbers counter is greater than 0.
                if (countDevices <= 0)
                    $('#ctl00_primaryHolder_btnMSave').attr("disabled", "true");
                else
                    $('#ctl00_primaryHolder_btnMSave').removeAttr('disabled');

            }).keypress(function (event) {

                // Validate the keys being pressed to ensure only numbers are entered.
                if (("0123456789").indexOf(String.fromCharCode(event.keyCode)) <= -1 && event.keyCode != '13') {
                    // If not, cancel the event.
                    event.preventDefault();
                }
            });
        });

        function JQueryControlsLoad() {

            $("#TabContainer1").tabs();

            // UserBirthDate Datepicker
            $('#<%=txtShipDate.ClientID %>').datepicker();
            
        }

        
        /// Count lines of a text box.
        function countLines(id) {
            var area = document.getElementById(id)
            if (area.value == "") return 0;
            // trim trailing return char if exists
            var text = area.value.replace(/\s+$/g, "")
            var split = text.split("\n")
            return split.length
        }

    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server">
    </act:ToolkitScriptManager>

    <%--Start TabsContainer Section--%>
    <div id="TabContainer1" >

	    <ul>				
            <li><a href="#TabPanel1" id="TabPanel1Header" runat ="server">Merge Shipped Packages</a></li>	
            <li><a href="#TabPanel2" id="TabPanel2Header" runat ="server">Insert Tracking#</a></li>						               
	    </ul>

        <!-- Merge Packages  -->   
        <div id="TabPanel1">

            <asp:UpdatePanel ID="UpdateTabPanel1" runat="server" UpdateMode="Conditional">
                <ContentTemplate>

                    <asp:UpdateProgress id="UpdateProgressMerge" runat="server" DynamicLayout="true" DisplayAfter="0" >
                        <ProgressTemplate>
                            <div style="width: 850px" align="center">
                                <img src="../images/loading11.gif" alt=""/>
                            </div>
                        </ProgressTemplate>
                    </asp:UpdateProgress>                    

                    <div class="FormError" id="mergeError" runat="server" visible="false" style="margin:10px" >
	                    <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="mergeErrorMsg" runat="server" >An error was encountered.</span></p>
                    </div>
                    <div class="FormMessage" id="mergeSuccess" runat="server" visible="false" style="margin:10px" > 
	                    <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="mergeSuccessMsg" runat ="server" >Ready to search.</span></p>
                    </div>      

                    <table class="OTable" >

                        <tr>
                            <th class="mt-hd" >
                                Shipped Package Information
                            </th>
                        </tr>      
                        <tr>
                            <td>
                                <div class="OForm" > 
                             
                                    <div class="Row">
                                        <div class="Label Small">Package #<span class="Required">*</span>:</div>
                                        <div class="Control">
                                            <asp:TextBox runat="server" ID="txtMPackageID" 
                                                MaxLength="40" CssClass="Medium3" ValidationGroup="form" />
                                        </div>
                                        <div class="Clear"></div>
                                    </div>

                                    <div class="Row">
                                        <div class="Label Small">Tracking #<span class="Required">*</span>:</div>
                                        <div class="Control">
                                            <asp:TextBox runat="server" ID="txtMTrackingNo" MaxLength="50" 
                                                CssClass="Large1" ValidationGroup="form"/>
                                        </div>
                                        <div class="Clear"></div>
                                    </div>

                                </div>
                            </td>                            
                        </tr>                        
                
                        <tr>
                            <th  class="mt-hd">Merged Packages</th>
                        </tr>

                        <tr>
                            <td>
                                <div class="OForm" > 
                             
                                    <div class="Row">
                                        <div class="Label Small">Package #<span class="Required">*</span>:</div>
                                        <div class="Control">
                                            <asp:TextBox runat="server" ID="txtMMultiPackageID" TextMode="MultiLine" Height="100px"  
                                                CssClass="Medium3" ValidationGroup="form" />
                                        </div>
                                        <div class="Clear"></div>
                                    </div>                                    

                                </div>
                            </td>                            
                        </tr>
                                
                    </table>  
                    
                    <div class="Left">
                        <span class="Required">* Indicate required fields.</span>
                    </div>
            
                    <div class="Right"> 
                          <asp:Button Text="Merge" ID="btnMSave" runat="server" OnClick="btnMSave_Click" 
                               ValidationGroup="form" style="margin-left: 0px" cssClass="OButton" />                  
                    </div>  
            
                    <div class="Clear"></div>  
                   
                </ContentTemplate>
            </asp:UpdatePanel>
                                                                   
        </div>
        <!-- Merge Packages  -->   

        <!-- Insert Packages  -->   
	    <div id="TabPanel2">

            <asp:UpdatePanel ID="UpdateTabPanel2" runat="server"  UpdateMode="Conditional">
                
                <ContentTemplate>

                    <asp:UpdateProgress id="UpdateProgressInsert" runat="server" DynamicLayout="true" DisplayAfter="0" >
                        <ProgressTemplate>
                            <div style="width: 850px" align="center">
                                <img src="../images/loading11.gif" alt=""/>
                            </div>
                        </ProgressTemplate>
                    </asp:UpdateProgress>                                      

                    <div class="FormError" id="insertError" runat="server" visible="false" style="margin:10px" >
	                    <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="insertErrorMsg" runat="server" >An error was encountered.</span></p>
                    </div>
                    <div class="FormMessage" id="insertSuccess" runat="server" visible="false" style="margin:10px" > 
	                    <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="insertSuccessMsg" runat ="server" >Ready to search.</span></p>
                    </div>     

                    <table class="OTable" >

                        <tr>
                            <th class="mt-hd" >
                                Shipping Information
                            </th>
                        </tr>      
                        <tr>
                            <td>
                                <div class="OForm" > 
                             
                                    <div class="Row">
                                        <div class="Label Small">Package #<span class="Required">*</span>:</div>
                                        <div class="Control">
                                            <asp:TextBox runat="server" ID="txtPackageID" 
                                                MaxLength="40" CssClass="Medium3" ValidationGroup="form" />
                                        </div>
                                        <div class="Clear"></div>
                                    </div>  
                                    
                                    <div class="Row">
                                        <div class="Label Small">Tracking #<span class="Required">*</span>:</div>
                                        <div class="Control">
                                            <asp:TextBox runat="server" ID="txtTrackingNo" MaxLength="50" 
                                                CssClass="Large1" ValidationGroup="form"/>
                                        </div>
                                        <div class="Clear"></div>
                                    </div>   
                                    
                                    <div class="Row">
                                        <div class="Label Small">Ship Date<span class="Required">*</span>:</div>
                                        <div class="Control">
                                            <asp:TextBox ID="txtShipDate" runat="server" Style="width: 100px" 
                                                CssClass="Medium1" ValidationGroup="form" />
                                        </div>
                                        <div class="Clear"></div>
                                    </div>   
                                    
                                    <div class="Row">
                                        <div class="Label Small">Shipping Cost:</div>
                                        <div class="Control">
                                            <asp:TextBox runat="server" ID="txtShippingCost" 
                                                MaxLength="40" CssClass="Small3" ValidationGroup="form" />
                                        </div>
                                        <div class="Clear"></div>
                                    </div>                                     

                                </div>
                            </td>
                            
                        </tr>                                                                               
        
                        <tr>
                            <th  class="mt-hd">Shipping To</th>
                        </tr>
                        <tr>
                            <td>
                                <div class="OForm" > 
                             
                                    <div class="Row">
                                        <div class="Label Small">Company Name:</div>
                                        <div class="Control">
                                            <asp:TextBox runat="server" ID="txtCompanyName" 
                                                MaxLength="250" CssClass="Large3" ValidationGroup="form" />
                                        </div>
                                        <div class="Clear"></div>
                                    </div>  
                                    
                                    <div class="Row">
                                        <div class="Label Small">Contact Name:</div>
                                        <div class="Control">
                                            <asp:TextBox runat="server" ID="txtContactName" MaxLength="250" 
                                                CssClass="Large3" ValidationGroup="form"/>
                                        </div>
                                        <div class="Clear"></div>
                                    </div>   
                                    
                                    <div class="Row">
                                        <div class="Label Small">Address 1:</div>
                                        <div class="Control">
                                            <asp:TextBox runat="server" ID="txtUserAddress1" 
                                                MaxLength="250" CssClass="Large3" ValidationGroup="form" />
                                        </div>
                                        <div class="Clear"></div>
                                    </div>   
                                    
                                    <div class="Row">
                                        <div class="Label Small">Address 2:</div>
                                        <div class="Control">
                                            <asp:TextBox runat="server" ID="txtUserAddress2" 
                                                MaxLength="250" CssClass="Large3" ValidationGroup="form" />
                                        </div>
                                        <div class="Clear"></div>
                                    </div>  
                                    
                                    <div class="Row">
                                        <div class="Label Small">Country:</div>
                                        <div class="Control">
                                            <asp:DropDownList ID="ddlUserCountryID" runat="server"  DataValueField="CountryID"
                                                DataTextField="CountryName" DataSourceID="GetCountries" 
                                                AutoPostBack="True" 
                                                onselectedindexchanged="ddlUserCountryID_SelectedIndexChanged" >
                                            </asp:DropDownList>
                                        </div>
                                        <div class="Clear"></div>
                                    </div>  
                                    
                                    <div class="Row">
                                        <div class="Label Small">City:</div>
                                        <div class="Control">
                                            <asp:TextBox runat="server" ID="txtUserCity" MaxLength="100" 
                                                CssClass="Large1" ValidationGroup="form" />
                                        </div>
                                        <div class="Clear"></div>
                                    </div>   
                                    
                                    <div class="Row">
                                        <div class="Label Small">State:</div>
                                        <div class="Control">
                                            <asp:DropDownList ID="ddlUserStateID" runat="server" DataSourceID="GetStatesByCountry" 
                                                DataValueField="StateID" DataTextField="StateAbbrev" />
                                        </div>
                                        <div class="Clear"></div>
                                    </div>   
                                    
                                    <div class="Row">
                                        <div class="Label Small">Zip Code:</div>
                                        <div class="Control">
                                            <asp:TextBox runat="server" ID="txtUserPostalCode" 
                                                MaxLength="50" CssClass="Medium2" ValidationGroup="form" />
                                        </div>
                                        <div class="Clear"></div>
                                    </div>                                           

                                </div>
                            </td>
                            
                        </tr>                                                                                                                                                                       
                                
                    </table>

                    <div class="Left">
                        <span class="Required">* Indicate required fields.</span>
                    </div>
            
                    <div class="Right"> 
                          <asp:Button Text="Save" ID="btnSave" runat="server" OnClick="btnSave_Click" 
                               ValidationGroup="form" style="margin-left: 0px" cssClass="OButton" />                     
                    </div>  
            
                    <div class="Clear"></div> 
                     
                </ContentTemplate>
            </asp:UpdatePanel>   
                    
        </div>
        <!-- Insert Packages  -->

    </div>
    <%--End TabsContainer Section--%>


    <asp:SqlDataSource ID="GetCountries" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="SELECT [CountryID], [CountryName] FROM [Countries] Where [Active] = 1 Order By CountryName">
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="GetStatesByCountry" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="SELECT [StateID], [StateAbbrev] FROM [States] Where [CountryID] = @CountryID Order By StateAbbrev">
        <SelectParameters>
            <asp:ControlParameter ControlID="ddlUserCountryID" DefaultValue="" 
                    Name="CountryID" PropertyName="SelectedItem.Value" Type="String" /> 
        </SelectParameters>
    </asp:SqlDataSource>

</asp:Content>



