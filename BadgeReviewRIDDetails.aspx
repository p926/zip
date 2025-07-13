<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BadgeReviewRIDDetails.aspx.cs" Inherits="portal_instadose_com_v3.TechOps.BadgeReviewRIDDetails" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<form id="frm" class="read-details-body" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <strong>Read Details</strong>
    <div>
        <telerik:RadGrid ID="rgReads" runat="server"
            SkinID="Default" CssClass="OTable"
            AllowPaging="false"
            Width="1520"
            AllowFilteringByColumn="false" OnNeedDataSource="rgReads_NeedDataSource"
            AutoGenerateColumns="false">
            <MasterTableView TableLayout="Fixed" CssClass="read-details-table" DataKeyNames="AccountID" ShowFooter="false">
                <Columns>
                    <telerik:GridBoundColumn DataField="SerialNo" HeaderText="SerialNo" HeaderStyle-Width="80"></telerik:GridBoundColumn> 
                    <telerik:GridBoundColumn DataField="ReadTypeID" HeaderText="Read Type" UniqueName="ReadTypeID" Visible="false"
                        Aggregate="Count" FooterText="Total: "></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="CreatedDate" HeaderText="Exp. Date" UniqueName="CreatedDate" 
                        DataType="System.DateTime" HeaderStyle-Width="150px" ItemStyle-Width="150px"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="BatteryPercent" HeaderText="Main Battery MV" SortExpression="BatteryPercent" UniqueName="MainBatteryV" AllowFiltering="false"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="DoseBatteryPercent" HeaderText="Dose Battery MV" SortExpression="DoseBatteryPercent" UniqueName="DoseBatteryV" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    
                    <telerik:GridTemplateColumn HeaderText="Initial Read">  
                        <ItemTemplate>  
                            <asp:Label ID="lblInitialRead" runat="server" Text='<%# Convert.ToBoolean(Eval("InitRead")) == true ? "Yes" : "No" %>'></asp:Label>  
                        </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                                                                    
                    <telerik:GridBoundColumn DataField="HasAnomaly" Visible="false"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="ErrorText" HeaderText="Exception" UniqueName="ErrorText"></telerik:GridBoundColumn>
                                                                    
                    <telerik:GridBoundColumn DataField="DeepLowDose" HeaderText="DL Dose" UniqueName="DeepLowDose"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="CumulativeDose" HeaderText="Cum. Dose" UniqueName="CumulativeDose" DataFormatString="{0:0}"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="DL_Rm" HeaderText="DL Rm" HeaderTooltip="DL Raw Dose Digits" UniqueName="DL_Rm"></telerik:GridBoundColumn>
                
                    <telerik:GridBoundColumn DataField="DL_Tm" HeaderText="DL Tm" HeaderTooltip="DL dose temp digits" SortExpression="DL_Tm" UniqueName="DL_Tm"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="DL_Tc" HeaderText="DL Tc" HeaderTooltip="DL capacitance temp dose digits" SortExpression="DL_Tc" UniqueName="DL_Tc"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="DL_DAC" HeaderText="DL DAC" HeaderTooltip="" SortExpression="DL_DAC" UniqueName="DL_DAC"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="TA_Pre" HeaderText="TA Pre" HeaderTooltip="Temp reading before dose measure" SortExpression="TA_Pre" UniqueName="TA_Pre"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="TA_Post" HeaderText="TA Post" HeaderTooltip="Temp reading after dose measure" SortExpression="TA_Post" UniqueName="TA_Post"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="DeepHighDose" HeaderText="DH Dose" HeaderTooltip="" SortExpression="DeepHighDose" UniqueName="DeepHighDose"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="DH_DAC" HeaderText="DH DAC" HeaderTooltip="" SortExpression="DH_DAC" UniqueName="DH_DAC"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="DH_Rm" HeaderText="DH Rm" HeaderTooltip="DH Raw Dose Digits" SortExpression="DH_Rm" UniqueName="DL_Tm"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="DH_Tm" HeaderText="DH Tm" HeaderTooltip="DH dose temp digits" SortExpression="DH_Tm" UniqueName="DL_Tm"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="DH_Tc" HeaderText="DH Tc" HeaderTooltip="DH capacitance temp dose digits" SortExpression="DH_Tc" UniqueName="DL_Tm"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="ActualDeepLowDose" HeaderText="Small Incr" HeaderTooltip="" SortExpression="ActualDeepLowDose" UniqueName="ActualDeepLowDose" DataFormatString="{0:0.00}"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="BackgroundExposure" HeaderText="Bkgd. Exp." HeaderTooltip="" SortExpression="BackgroundExposure" UniqueName="BackgroundExposure" DataFormatString="{0:0.00}"></telerik:GridBoundColumn>
                
                </Columns>
            </MasterTableView>
        </telerik:RadGrid>
    </div>
    
    <br />
    <strong>Exceptions</strong>
    <div>
        <telerik:RadGrid ID="rgExceptions" runat="server"
            SkinID="Default" CssClass="OTable"
            AllowPaging="false"
            Width="1100"
            AllowFilteringByColumn="false" OnNeedDataSource="rgExceptions_NeedDataSource"
            AutoGenerateColumns="false">
            <MasterTableView TableLayout="Fixed" DataKeyNames="SerialNo" CssClass="exceptions-table" ShowFooter="false">
                <Columns>
                    <telerik:GridBoundColumn DataField="SerialNo" HeaderText="SerialNo"></telerik:GridBoundColumn> 
                    <telerik:GridBoundColumn DataField="ExceptionDate" HeaderText="ExceptionDate" UniqueName="ExceptionDate" 
                        DataType="System.DateTime" HeaderStyle-Width="150px" ItemStyle-Width="150px"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="CreatedDate" HeaderText="CreatedDate" UniqueName="CreatedDate" 
                        DataType="System.DateTime" HeaderStyle-Width="150px" ItemStyle-Width="150px"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="Code" HeaderText="Code" UniqueName="Code"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="AdditionalInfo" DataFormatString="{0:#.##}" HeaderText="AdditionalInfo" UniqueName="AdditionalInfo"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="ExceptionDesc" HeaderText="ExceptionDesc" UniqueName="ExceptionDesc"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="InfoDesc" HeaderText="InfoDesc" UniqueName="InfoDesc"></telerik:GridBoundColumn>
<%--                    <telerik:GridBoundColumn DataField="Code" HeaderText="Code" UniqueName="Code"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="Code" HeaderText="Code" UniqueName="Code"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="Code" HeaderText="Code" UniqueName="Code"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="Code" HeaderText="Code" UniqueName="Code"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="Code" HeaderText="Code" UniqueName="Code"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="Code" HeaderText="Code" UniqueName="Code"></telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="Code" HeaderText="Code" UniqueName="Code"></telerik:GridBoundColumn>--%>

                </Columns>
            </MasterTableView>
        </telerik:RadGrid>
    </div>
    <br />
    <strong>Daily Motion</strong>
    <div>
        <telerik:RadGrid ID="rgDailyMotions" runat="server"
            SkinID="Default" CssClass="OTable"
            AllowPaging="false"
            Width="1600"
            AllowFilteringByColumn="false" OnNeedDataSource="rgDailyMotions_NeedDataSource"
            AutoGenerateColumns="false">
            <MasterTableView TableLayout="Fixed" DataKeyNames="SerialNo" CssClass="daily-motion-table" ShowFooter="false">
                <Columns>
                    <telerik:GridBoundColumn DataField="SerialNo" HeaderStyle-Width="80" HeaderText="Serial No"></telerik:GridBoundColumn> 
                    <telerik:GridBoundColumn DataField="MotionDetectedDate" HeaderText="Motion Detected Date" UniqueName="MotionDetectedDate" 
                        DataType="System.DateTime" HeaderStyle-Width="80px"  DataFormatString="{0:MM/dd/yyyy}"></telerik:GridBoundColumn>
                    <telerik:GridTemplateColumn HeaderText="Hour0">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour0")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour1">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour1")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour2">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour2")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour3">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour3")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour4">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour4")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour5">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour5")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour6">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour6")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour7">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour7")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour8">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour8")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour9">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour9")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour10">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour10")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour11">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour11")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour12">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour12")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour13">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour13")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour14">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour14")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour15">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour15")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour16">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour16")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour17">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour17")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour18">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour18")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour19">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour19")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour20">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour20")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour21">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour21")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour22">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour22")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 
                    <telerik:GridTemplateColumn HeaderText="Hour23">  
                        <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour23")) == true ? "1" : "0" %> </ItemTemplate>  
                    </telerik:GridTemplateColumn> 

                    <telerik:GridBoundColumn DataField="CreatedDate" HeaderText="CreatedDate" UniqueName="CreatedDate" 
                        DataType="System.DateTime" HeaderStyle-Width="150px" ItemStyle-Width="150px"></telerik:GridBoundColumn>
                    
                </Columns>
            </MasterTableView>
        </telerik:RadGrid>
    </div>
</form>
