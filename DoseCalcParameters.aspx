<%@ Page Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="True" Inherits="TechOps_DoseCalcParameters" Title="Dose Calculation Parameters" Codebehind="DoseCalcParameters.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">

  <act:toolkitscriptmanager ID="ToolkitScriptManager1" runat="server" />   
  
    <!-- Set Dose Parameters  -->   
        <asp:UpdatePanel ID="UpdatePnl" runat="server" UpdateMode="Conditional">
                <ContentTemplate>

                    <asp:UpdateProgress id="UpdateProgress" runat="server" DynamicLayout="true" DisplayAfter="0" >
                        <ProgressTemplate>
                            <div style="width: 850px" align="center">
                                <img src="../images/loading11.gif" alt=""/>
                            </div>
                        </ProgressTemplate>
                    </asp:UpdateProgress>                    

                    <div class="FormError" id="Errors" runat="server" visible="false" style="margin:10px" >
	                    <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="ErrorMsg" runat="server" >An error was encountered.</span></p>
                    </div>
                    <div class="FormMessage" id="Success" runat="server" visible="false" style="margin:10px" > 
	                    <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="SuccessMsg" runat ="server" >Ready to search.</span></p>
                    </div>                      

                    <%--Parameters INFORMATION--%>

                    <table style="width:100%;border:1;" cellpadding="0" cellspacing="0" class="OTable">
                        <tr id="ID1_ID2" runat ="server">
                            <td style="width: 50%">
                                <table style="width:100%;border:1;" cellpadding="0" cellspacing="0" class="OTable">
                                    <tr>
                                        <th class="mt-hd" colspan ="2">
                                            ID1 Parameters
                                        </th>                            
                                    </tr> 
                                    <tr class="Alt">
                                        <td style="width: 12%" class="mt-hd">MRD:</td>
                                        <td style="width: 38%" class="mt-itm">
                                            <asp:TextBox runat="server" ID="txtMRD_ID1" Width ="80"
                                                CssClass="Size XXSmall" ValidationGroup="form_Tab" />                                        
                                        </td>                                        
                                    </tr>
                                    <tr class="">
                                        <td style="width: 12%" class="mt-hd">MRD Incr:</td>
                                        <td style="width: 38%" class="mt-itm">
                                            <asp:TextBox runat="server" ID="txtMRDIncr_ID1" Width ="80"
                                                CssClass="Size XXSmall" ValidationGroup="form_Tab" />                                        
                                        </td>                                        
                                    </tr>
                                    <tr class="Alt">
                                        <td style="width: 12%" class="mt-hd">Noise:</td>
                                        <td style="width: 38%" class="mt-itm">
                                            <asp:TextBox runat="server" ID="txtNoise_ID1" Width ="80"
                                                CssClass="Size XXSmall" ValidationGroup="form_Tab" />                                        
                                        </td>                                        
                                    </tr>                                     
                                    <tr class="">
                                        <td style="width: 12%" class="mt-hd">Coefficient:</td>
                                        <td style="width: 38%" class="mt-itm">
                                            <asp:TextBox runat="server" ID="txtCoefficient_ID1" Width ="80"
                                                CssClass="Size XXSmall" ValidationGroup="form_Tab" />                                        
                                        </td>                                        
                                    </tr> 
                                    <tr class="Alt">
                                        <td style="width: 12%" class="mt-hd">LLD:</td>
                                        <td style="width: 38%" class="mt-itm">
                                            <asp:TextBox runat="server" ID="txtLLD_ID1" Width ="80"
                                                CssClass="Size XXSmall" ValidationGroup="form_Tab" />                                        
                                        </td>                                        
                                    </tr> 
                                    <tr class="">
                                        <td style="width: 12%" class="mt-hd">Fading Limit:</td>
                                        <td style="width: 38%" class="mt-itm">
                                            <asp:TextBox runat="server" ID="txtFadingLimit_ID1" Width ="80"
                                                CssClass="Size XXSmall" ValidationGroup="form_Tab" />                                        
                                        </td>                                        
                                    </tr> 
                                    <tr class="Alt">
                                        <td style="width: 12%" class="mt-hd">Read Limit:</td>
                                        <td style="width: 38%" class="mt-itm">
                                            <asp:TextBox runat="server" ID="txtReadLimit_ID1" Width ="80"
                                                CssClass="Size XXSmall" ValidationGroup="form_Tab" />                                        
                                        </td>                                        
                                    </tr> 
                                    <tr class="">
                                        <td style="width: 12%" class="mt-hd">Day Since Init:</td>
                                        <td style="width: 38%" class="mt-itm">
                                            <asp:TextBox runat="server" ID="txtDaySinceInit_ID1" Width ="80"
                                                CssClass="Size XXSmall" ValidationGroup="form_Tab" />                                        
                                        </td>                                        
                                    </tr> 
                                    <tr class="">
                                        <td style="width: 12%" class="mt-hd"></td>
                                        <td style="width: 38%" class="mt-itm">
                                            <asp:Button ID="btnSave_ID1" CssClass="OButton" runat="server" Text="Save" ValidationGroup="form_Tab"
                                                OnClick="btnSave_ID1_Click"  OnClientClick="return confirm('Are you sure you want to update the change of ID1 parameters?');"    />                                      
                                        </td>                                        
                                    </tr>  
                                                                                                                      
                                </table> 
                            </td>

                            <td>
                                <table style="width:100%;border:1;" cellpadding="0" cellspacing="0" class="OTable">
                                    <tr>
                                        <th class="mt-hd" colspan="2">
                                            ID2 Parameters
                                        </th>                            
                                    </tr> 
                                    <tr class="Alt">
                                        <td style="width: 12%" class="mt-hd">MRD:</td>
                                        <td style="width: 38%" class="mt-itm">
                                            <asp:TextBox runat="server" ID="txtMRD_ID2" Width ="80"
                                                CssClass="Size XXSmall" ValidationGroup="form2_Tab" />                                        
                                        </td>                                        
                                    </tr>
                                    <tr class="">
                                        <td style="width: 12%" class="mt-hd">MRD Incr:</td>
                                        <td style="width: 38%" class="mt-itm">
                                            <asp:TextBox runat="server" ID="txtMRDIncr_ID2" Width ="80"
                                                CssClass="Size XXSmall" ValidationGroup="form2_Tab" />                                        
                                        </td>                                        
                                    </tr>
                                    <tr class="Alt">
                                        <td style="width: 12%" class="mt-hd">DL Noise:</td>
                                        <td style="width: 38%" class="mt-itm">
                                            <asp:TextBox runat="server" ID="txtDLNoise_ID2" Width ="80"
                                                CssClass="Size XXSmall" ValidationGroup="form2_Tab" />                                        
                                        </td>                                        
                                    </tr>
                                    <tr class="">
                                        <td style="width: 12%" class="mt-hd">DH Noise:</td>
                                        <td style="width: 38%" class="mt-itm">
                                            <asp:TextBox runat="server" ID="txtDHNoise_ID2" Width ="80"
                                                CssClass="Size XXSmall" ValidationGroup="form2_Tab" />                                        
                                        </td>                                        
                                    </tr>
                                    <tr class="Alt">
                                        <td style="width: 12%" class="mt-hd">SL Noise:</td>
                                        <td style="width: 38%" class="mt-itm">
                                            <asp:TextBox runat="server" ID="txtSLNoise_ID2" Width ="80"
                                                CssClass="Size XXSmall" ValidationGroup="form2_Tab" />                                        
                                        </td>                                        
                                    </tr>
                                    <tr class="">
                                        <td style="width: 12%" class="mt-hd">SH Noise:</td>
                                        <td style="width: 38%" class="mt-itm">
                                            <asp:TextBox runat="server" ID="txtSHNoise_ID2" Width ="80"
                                                CssClass="Size XXSmall" ValidationGroup="form2_Tab" />                                        
                                        </td>                                        
                                    </tr>
                                    <tr class="Alt">
                                        <td style="width: 12%" class="mt-hd">Algorithm Factor:</td>
                                        <td style="width: 38%" class="mt-itm">
                                            <asp:TextBox runat="server" ID="txtAlgorithmFactor_ID2" Width ="80"
                                                CssClass="Size XXSmall" ValidationGroup="form2_Tab" /> 
                                            (G: Gamma; BG: Beta Gamma)                                       
                                        </td>                                        
                                    </tr>
                                    <tr class="">
                                        <td style="width: 12%" class="mt-hd">Day Since Init:</td>
                                        <td style="width: 38%" class="mt-itm">
                                            <asp:TextBox runat="server" ID="txtDaySinceInit_ID2" Width ="80"
                                                CssClass="Size XXSmall" ValidationGroup="form2_Tab" />                                        
                                        </td>                                        
                                    </tr>  
                                    <tr class="">
                                        <td style="width: 12%" class="mt-hd"></td>
                                        <td style="width: 38%" class="mt-itm">
                                            <asp:Button ID="btnSave_ID2" CssClass="OButton" runat="server" Text="Save" ValidationGroup="form2_Tab"
                                                OnClick="btnSave_ID2_Click"  OnClientClick="return confirm('Are you sure you want to update the change of ID2 parameters?');"    />                                         
                                        </td>                                        
                                    </tr>  
                                                                                           
                                </table> 
                            </td>
                        </tr>
                    </table>
                    
                    <%--END--%>                                                               
                                                                                         
                </ContentTemplate>
            </asp:UpdatePanel>
        <!-- Set Dose Parameters  -->      

</asp:Content>


