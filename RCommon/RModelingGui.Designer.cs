﻿namespace RCommon
{
    partial class RModelingGui
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.DatasheetTab = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.dsControl1 = new VBCommon.Controls.DatasheetControl();
            this.VariableSelectionTab = new System.Windows.Forms.TabPage();
            this.lblAvailVars = new System.Windows.Forms.Label();
            this.lblDepVars = new System.Windows.Forms.Label();
            this.lblNumObs = new System.Windows.Forms.Label();
            this.btnRemoveInputVariable = new System.Windows.Forms.Button();
            this.btnAddInputVariable = new System.Windows.Forms.Button();
            this.lbDepVarName = new System.Windows.Forms.Label();
            this.lblDepVariable = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.lbIndVariables = new System.Windows.Forms.ListBox();
            this.label9 = new System.Windows.Forms.Label();
            this.lbAvailableVariables = new System.Windows.Forms.ListBox();
            this.ModelingTab = new System.Windows.Forms.TabPage();
            this.pnlThresholdingButtons = new System.Windows.Forms.Panel();
            this.btnRight25 = new System.Windows.Forms.Button();
            this.btnRight1 = new System.Windows.Forms.Button();
            this.btnLeft1 = new System.Windows.Forms.Button();
            this.btnLeft25 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblValidationTitle = new System.Windows.Forms.Label();
            this.lblSpec = new System.Windows.Forms.Label();
            this.chartValidation = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.cbRandomSeed = new System.Windows.Forms.CheckBox();
            this.lblRandomSeed = new System.Windows.Forms.Label();
            this.tbRandomSeed = new System.Windows.Forms.TextBox();
            this.lblDecisionThreshold = new System.Windows.Forms.Label();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.lvValidation = new System.Windows.Forms.ListView();
            this.TruePos = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TrueNeg = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.FalsePos = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.FalseNeg = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Sensitivity = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Specificity = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Accuracy = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.lvModel = new System.Windows.Forms.ListView();
            this.groupBox11 = new System.Windows.Forms.GroupBox();
            this.groupBox12 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbExponent = new System.Windows.Forms.TextBox();
            this.rbPower = new System.Windows.Forms.RadioButton();
            this.rbLoge = new System.Windows.Forms.RadioButton();
            this.rbValue = new System.Windows.Forms.RadioButton();
            this.rbLog10 = new System.Windows.Forms.RadioButton();
            this.groupBox13 = new System.Windows.Forms.GroupBox();
            this.label32 = new System.Windows.Forms.Label();
            this.label33 = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.label35 = new System.Windows.Forms.Label();
            this.label36 = new System.Windows.Forms.Label();
            this.label37 = new System.Windows.Forms.Label();
            this.label39 = new System.Windows.Forms.Label();
            this.tbThreshold = new System.Windows.Forms.TextBox();
            this.DiagnosticTab = new System.Windows.Forms.TabPage();
            this.zgcDiagnostic = new ZedGraph.ZedGraphControl();
            this.tabControl1.SuspendLayout();
            this.DatasheetTab.SuspendLayout();
            this.VariableSelectionTab.SuspendLayout();
            this.ModelingTab.SuspendLayout();
            this.pnlThresholdingButtons.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartValidation)).BeginInit();
            this.groupBox10.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox11.SuspendLayout();
            this.groupBox12.SuspendLayout();
            this.groupBox13.SuspendLayout();
            this.DiagnosticTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.DatasheetTab);
            this.tabControl1.Controls.Add(this.VariableSelectionTab);
            this.tabControl1.Controls.Add(this.ModelingTab);
            this.tabControl1.Controls.Add(this.DiagnosticTab);
            this.tabControl1.Location = new System.Drawing.Point(1, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(631, 610);
            this.tabControl1.TabIndex = 0;
            // 
            // DatasheetTab
            // 
            this.DatasheetTab.BackColor = System.Drawing.SystemColors.Control;
            this.DatasheetTab.Controls.Add(this.label3);
            this.DatasheetTab.Controls.Add(this.dsControl1);
            this.DatasheetTab.Location = new System.Drawing.Point(4, 22);
            this.DatasheetTab.Name = "DatasheetTab";
            this.DatasheetTab.Padding = new System.Windows.Forms.Padding(3);
            this.DatasheetTab.Size = new System.Drawing.Size(623, 584);
            this.DatasheetTab.TabIndex = 0;
            this.DatasheetTab.Text = "Data Manipulation";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(42, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(356, 16);
            this.label3.TabIndex = 1;
            this.label3.Text = "Make further manipulations if desired for modeling.";
            // 
            // dsControl1
            // 
            this.dsControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dsControl1.CurrentSelectedRowIndex = -1;
            this.dsControl1.DependentVariableTransform = VBCommon.Transforms.DependentVariableTransforms.none;
            this.dsControl1.DT = null;
            this.dsControl1.DTCI = null;
            this.dsControl1.DTRI = null;
            this.dsControl1.FileName = "";
            this.dsControl1.Location = new System.Drawing.Point(22, 39);
            this.dsControl1.Name = "dsControl1";
            this.dsControl1.Orientation = 0D;
            this.dsControl1.PowerTransformExponent = double.NaN;
            this.dsControl1.ResponseVarColIndex = 1;
            this.dsControl1.ResponseVarColName = "";
            this.dsControl1.ResponseVarColNameAsImported = "";
            this.dsControl1.SelectColName = "";
            this.dsControl1.SelectedColIndex = -1;
            this.dsControl1.Size = new System.Drawing.Size(663, 494);
            this.dsControl1.State = VBCommon.Controls.DatasheetControl.dtState.dirty;
            this.dsControl1.TabIndex = 0;
            // 
            // VariableSelectionTab
            // 
            this.VariableSelectionTab.BackColor = System.Drawing.SystemColors.Control;
            this.VariableSelectionTab.Controls.Add(this.lblAvailVars);
            this.VariableSelectionTab.Controls.Add(this.lblDepVars);
            this.VariableSelectionTab.Controls.Add(this.lblNumObs);
            this.VariableSelectionTab.Controls.Add(this.btnRemoveInputVariable);
            this.VariableSelectionTab.Controls.Add(this.btnAddInputVariable);
            this.VariableSelectionTab.Controls.Add(this.lbDepVarName);
            this.VariableSelectionTab.Controls.Add(this.lblDepVariable);
            this.VariableSelectionTab.Controls.Add(this.label14);
            this.VariableSelectionTab.Controls.Add(this.lbIndVariables);
            this.VariableSelectionTab.Controls.Add(this.label9);
            this.VariableSelectionTab.Controls.Add(this.lbAvailableVariables);
            this.VariableSelectionTab.Location = new System.Drawing.Point(4, 22);
            this.VariableSelectionTab.Name = "VariableSelectionTab";
            this.VariableSelectionTab.Padding = new System.Windows.Forms.Padding(3);
            this.VariableSelectionTab.Size = new System.Drawing.Size(640, 584);
            this.VariableSelectionTab.TabIndex = 1;
            this.VariableSelectionTab.Text = "Variable Selection";
            // 
            // lblAvailVars
            // 
            this.lblAvailVars.AutoSize = true;
            this.lblAvailVars.Location = new System.Drawing.Point(108, 35);
            this.lblAvailVars.Name = "lblAvailVars";
            this.lblAvailVars.Size = new System.Drawing.Size(19, 13);
            this.lblAvailVars.TabIndex = 83;
            this.lblAvailVars.Text = "    ";
            // 
            // lblDepVars
            // 
            this.lblDepVars.AutoSize = true;
            this.lblDepVars.Location = new System.Drawing.Point(313, 34);
            this.lblDepVars.Name = "lblDepVars";
            this.lblDepVars.Size = new System.Drawing.Size(22, 13);
            this.lblDepVars.TabIndex = 82;
            this.lblDepVars.Text = "     ";
            // 
            // lblNumObs
            // 
            this.lblNumObs.AutoSize = true;
            this.lblNumObs.Location = new System.Drawing.Point(141, 34);
            this.lblNumObs.Name = "lblNumObs";
            this.lblNumObs.Size = new System.Drawing.Size(141, 13);
            this.lblNumObs.TabIndex = 79;
            this.lblNumObs.Text = "Number of Observations: {0}";
            // 
            // btnRemoveInputVariable
            // 
            this.btnRemoveInputVariable.Location = new System.Drawing.Point(164, 216);
            this.btnRemoveInputVariable.Name = "btnRemoveInputVariable";
            this.btnRemoveInputVariable.Size = new System.Drawing.Size(25, 20);
            this.btnRemoveInputVariable.TabIndex = 81;
            this.btnRemoveInputVariable.Text = "<";
            this.btnRemoveInputVariable.Click += new System.EventHandler(this.btnRemoveInputVariable_Click);
            // 
            // btnAddInputVariable
            // 
            this.btnAddInputVariable.Location = new System.Drawing.Point(164, 190);
            this.btnAddInputVariable.Name = "btnAddInputVariable";
            this.btnAddInputVariable.Size = new System.Drawing.Size(25, 20);
            this.btnAddInputVariable.TabIndex = 80;
            this.btnAddInputVariable.Text = ">";
            this.btnAddInputVariable.Click += new System.EventHandler(this.btnAddInputVariable_Click);
            // 
            // lbDepVarName
            // 
            this.lbDepVarName.AutoSize = true;
            this.lbDepVarName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDepVarName.Location = new System.Drawing.Point(117, 7);
            this.lbDepVarName.Name = "lbDepVarName";
            this.lbDepVarName.Size = new System.Drawing.Size(150, 13);
            this.lbDepVarName.TabIndex = 78;
            this.lbDepVarName.Text = "dependent variable name";
            // 
            // lblDepVariable
            // 
            this.lblDepVariable.AutoSize = true;
            this.lblDepVariable.Location = new System.Drawing.Point(4, 7);
            this.lblDepVariable.Name = "lblDepVariable";
            this.lblDepVariable.Size = new System.Drawing.Size(107, 13);
            this.lblDepVariable.TabIndex = 77;
            this.lblDepVariable.Text = "Dependent Variable: ";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(194, 59);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(113, 13);
            this.label14.TabIndex = 76;
            this.label14.Text = "Independent Variables";
            // 
            // lbIndVariables
            // 
            this.lbIndVariables.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbIndVariables.FormattingEnabled = true;
            this.lbIndVariables.HorizontalScrollbar = true;
            this.lbIndVariables.Location = new System.Drawing.Point(192, 75);
            this.lbIndVariables.Name = "lbIndVariables";
            this.lbIndVariables.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbIndVariables.Size = new System.Drawing.Size(160, 446);
            this.lbIndVariables.TabIndex = 75;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 34);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(96, 13);
            this.label9.TabIndex = 74;
            this.label9.Text = "Available Variables";
            // 
            // lbAvailableVariables
            // 
            this.lbAvailableVariables.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbAvailableVariables.FormattingEnabled = true;
            this.lbAvailableVariables.HorizontalScrollbar = true;
            this.lbAvailableVariables.Location = new System.Drawing.Point(3, 75);
            this.lbAvailableVariables.Name = "lbAvailableVariables";
            this.lbAvailableVariables.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbAvailableVariables.Size = new System.Drawing.Size(160, 446);
            this.lbAvailableVariables.TabIndex = 73;
            // 
            // ModelingTab
            // 
            this.ModelingTab.BackColor = System.Drawing.SystemColors.Control;
            this.ModelingTab.Controls.Add(this.pnlThresholdingButtons);
            this.ModelingTab.Controls.Add(this.groupBox1);
            this.ModelingTab.Controls.Add(this.cbRandomSeed);
            this.ModelingTab.Controls.Add(this.lblRandomSeed);
            this.ModelingTab.Controls.Add(this.tbRandomSeed);
            this.ModelingTab.Controls.Add(this.lblDecisionThreshold);
            this.ModelingTab.Controls.Add(this.groupBox10);
            this.ModelingTab.Controls.Add(this.label4);
            this.ModelingTab.Controls.Add(this.groupBox6);
            this.ModelingTab.Controls.Add(this.groupBox11);
            this.ModelingTab.Location = new System.Drawing.Point(4, 22);
            this.ModelingTab.Name = "ModelingTab";
            this.ModelingTab.Padding = new System.Windows.Forms.Padding(3);
            this.ModelingTab.Size = new System.Drawing.Size(623, 584);
            this.ModelingTab.TabIndex = 2;
            this.ModelingTab.Text = "Model";
            // 
            // pnlThresholdingButtons
            // 
            this.pnlThresholdingButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlThresholdingButtons.Controls.Add(this.btnRight25);
            this.pnlThresholdingButtons.Controls.Add(this.btnRight1);
            this.pnlThresholdingButtons.Controls.Add(this.btnLeft1);
            this.pnlThresholdingButtons.Controls.Add(this.btnLeft25);
            this.pnlThresholdingButtons.Location = new System.Drawing.Point(442, 526);
            this.pnlThresholdingButtons.Name = "pnlThresholdingButtons";
            this.pnlThresholdingButtons.Size = new System.Drawing.Size(84, 30);
            this.pnlThresholdingButtons.TabIndex = 136;
            this.pnlThresholdingButtons.Visible = false;
            // 
            // btnRight25
            // 
            this.btnRight25.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRight25.Location = new System.Drawing.Point(6, 3);
            this.btnRight25.Name = "btnRight25";
            this.btnRight25.Size = new System.Drawing.Size(75, 23);
            this.btnRight25.TabIndex = 112;
            this.btnRight25.Text = ">>";
            this.btnRight25.UseVisualStyleBackColor = true;
            this.btnRight25.Click += new System.EventHandler(this.btnRight25_Click);
            // 
            // btnRight1
            // 
            this.btnRight1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRight1.Location = new System.Drawing.Point(-75, 3);
            this.btnRight1.Name = "btnRight1";
            this.btnRight1.Size = new System.Drawing.Size(75, 23);
            this.btnRight1.TabIndex = 111;
            this.btnRight1.Text = ">";
            this.btnRight1.UseVisualStyleBackColor = true;
            this.btnRight1.Click += new System.EventHandler(this.btnRight1_Click);
            // 
            // btnLeft1
            // 
            this.btnLeft1.Location = new System.Drawing.Point(84, 3);
            this.btnLeft1.Name = "btnLeft1";
            this.btnLeft1.Size = new System.Drawing.Size(75, 23);
            this.btnLeft1.TabIndex = 110;
            this.btnLeft1.Text = "<";
            this.btnLeft1.UseVisualStyleBackColor = true;
            this.btnLeft1.Click += new System.EventHandler(this.btnLeft1_Click);
            // 
            // btnLeft25
            // 
            this.btnLeft25.Location = new System.Drawing.Point(3, 3);
            this.btnLeft25.Name = "btnLeft25";
            this.btnLeft25.Size = new System.Drawing.Size(75, 23);
            this.btnLeft25.TabIndex = 109;
            this.btnLeft25.Text = "<<";
            this.btnLeft25.UseVisualStyleBackColor = true;
            this.btnLeft25.Click += new System.EventHandler(this.btnLeft25_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.lblValidationTitle);
            this.groupBox1.Controls.Add(this.lblSpec);
            this.groupBox1.Controls.Add(this.chartValidation);
            this.groupBox1.Location = new System.Drawing.Point(381, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(224, 565);
            this.groupBox1.TabIndex = 135;
            this.groupBox1.TabStop = false;
            // 
            // lblValidationTitle
            // 
            this.lblValidationTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblValidationTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblValidationTitle.Location = new System.Drawing.Point(3, 16);
            this.lblValidationTitle.Name = "lblValidationTitle";
            this.lblValidationTitle.Size = new System.Drawing.Size(218, 18);
            this.lblValidationTitle.TabIndex = 135;
            this.lblValidationTitle.Text = "Adjust the threshold (vertical line) to balance true positives and true negatives" +
    ":";
            this.lblValidationTitle.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblValidationTitle.Visible = false;
            // 
            // lblSpec
            // 
            this.lblSpec.AutoSize = true;
            this.lblSpec.Location = new System.Drawing.Point(6, 42);
            this.lblSpec.Name = "lblSpec";
            this.lblSpec.Size = new System.Drawing.Size(103, 13);
            this.lblSpec.TabIndex = 130;
            this.lblSpec.Text = "specificity goes here";
            this.lblSpec.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblSpec.Visible = false;
            // 
            // chartValidation
            // 
            this.chartValidation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;
            chartArea1.AxisX.MajorTickMark.TickMarkStyle = System.Windows.Forms.DataVisualization.Charting.TickMarkStyle.AcrossAxis;
            chartArea1.AxisX2.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;
            chartArea1.AxisX2.MajorTickMark.TickMarkStyle = System.Windows.Forms.DataVisualization.Charting.TickMarkStyle.AcrossAxis;
            chartArea1.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;
            chartArea1.AxisY.MajorTickMark.TickMarkStyle = System.Windows.Forms.DataVisualization.Charting.TickMarkStyle.AcrossAxis;
            chartArea1.AxisY2.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;
            chartArea1.AxisY2.MajorTickMark.TickMarkStyle = System.Windows.Forms.DataVisualization.Charting.TickMarkStyle.AcrossAxis;
            chartArea1.Name = "ChartArea1";
            this.chartValidation.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chartValidation.Legends.Add(legend1);
            this.chartValidation.Location = new System.Drawing.Point(3, 46);
            this.chartValidation.Name = "chartValidation";
            series1.BorderWidth = 2;
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine;
            series1.Legend = "Legend1";
            series1.Name = "True positives";
            series2.BorderWidth = 2;
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine;
            series2.Legend = "Legend1";
            series2.Name = "True negatives";
            this.chartValidation.Series.Add(series1);
            this.chartValidation.Series.Add(series2);
            this.chartValidation.Size = new System.Drawing.Size(218, 487);
            this.chartValidation.TabIndex = 129;
            this.chartValidation.Text = "chart1";
            // 
            // cbRandomSeed
            // 
            this.cbRandomSeed.AutoSize = true;
            this.cbRandomSeed.Location = new System.Drawing.Point(203, 186);
            this.cbRandomSeed.Name = "cbRandomSeed";
            this.cbRandomSeed.Size = new System.Drawing.Size(15, 14);
            this.cbRandomSeed.TabIndex = 133;
            this.cbRandomSeed.UseVisualStyleBackColor = true;
            this.cbRandomSeed.CheckedChanged += new System.EventHandler(this.cbRandomSeed_CheckedChanged);
            // 
            // lblRandomSeed
            // 
            this.lblRandomSeed.AutoSize = true;
            this.lblRandomSeed.ForeColor = System.Drawing.Color.Black;
            this.lblRandomSeed.Location = new System.Drawing.Point(224, 187);
            this.lblRandomSeed.Name = "lblRandomSeed";
            this.lblRandomSeed.Size = new System.Drawing.Size(84, 13);
            this.lblRandomSeed.TabIndex = 132;
            this.lblRandomSeed.Text = "Set Seed Value:";
            // 
            // tbRandomSeed
            // 
            this.tbRandomSeed.Location = new System.Drawing.Point(311, 184);
            this.tbRandomSeed.Name = "tbRandomSeed";
            this.tbRandomSeed.Size = new System.Drawing.Size(44, 20);
            this.tbRandomSeed.TabIndex = 131;
            this.tbRandomSeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lblDecisionThreshold
            // 
            this.lblDecisionThreshold.AutoSize = true;
            this.lblDecisionThreshold.Location = new System.Drawing.Point(13, 184);
            this.lblDecisionThreshold.Name = "lblDecisionThreshold";
            this.lblDecisionThreshold.Size = new System.Drawing.Size(0, 13);
            this.lblDecisionThreshold.TabIndex = 130;
            // 
            // groupBox10
            // 
            this.groupBox10.Controls.Add(this.lvValidation);
            this.groupBox10.Location = new System.Drawing.Point(3, 468);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(369, 100);
            this.groupBox10.TabIndex = 119;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "Model Validation";
            // 
            // lvValidation
            // 
            this.lvValidation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lvValidation.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.TruePos,
            this.TrueNeg,
            this.FalsePos,
            this.FalseNeg,
            this.Sensitivity,
            this.Specificity,
            this.Accuracy});
            this.lvValidation.FullRowSelect = true;
            this.lvValidation.GridLines = true;
            this.lvValidation.HideSelection = false;
            this.lvValidation.Location = new System.Drawing.Point(3, 16);
            this.lvValidation.Name = "lvValidation";
            this.lvValidation.Size = new System.Drawing.Size(363, 81);
            this.lvValidation.TabIndex = 52;
            this.lvValidation.UseCompatibleStateImageBehavior = false;
            this.lvValidation.View = System.Windows.Forms.View.Details;
            // 
            // TruePos
            // 
            this.TruePos.Text = "True Positives";
            this.TruePos.Width = 80;
            // 
            // TrueNeg
            // 
            this.TrueNeg.Text = "True Negatives";
            this.TrueNeg.Width = 85;
            // 
            // FalsePos
            // 
            this.FalsePos.Text = "False Positives";
            this.FalsePos.Width = 82;
            // 
            // FalseNeg
            // 
            this.FalseNeg.Text = "False Negatives";
            this.FalseNeg.Width = 88;
            // 
            // Sensitivity
            // 
            this.Sensitivity.Text = "Sensitivity";
            // 
            // Specificity
            // 
            this.Specificity.Text = "Specificity";
            this.Specificity.Width = 62;
            // 
            // Accuracy
            // 
            this.Accuracy.Text = "Accuracy";
            this.Accuracy.Width = 57;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.Blue;
            this.label4.Location = new System.Drawing.Point(66, 184);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 13);
            this.label4.TabIndex = 126;
            this.label4.Text = "Decision Criterion";
            // 
            // groupBox6
            // 
            this.groupBox6.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox6.Controls.Add(this.lvModel);
            this.groupBox6.Location = new System.Drawing.Point(3, 213);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(369, 252);
            this.groupBox6.TabIndex = 125;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Model Summary";
            // 
            // lvModel
            // 
            this.lvModel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lvModel.FullRowSelect = true;
            this.lvModel.GridLines = true;
            this.lvModel.Location = new System.Drawing.Point(3, 16);
            this.lvModel.Name = "lvModel";
            this.lvModel.Size = new System.Drawing.Size(363, 233);
            this.lvModel.TabIndex = 51;
            this.lvModel.UseCompatibleStateImageBehavior = false;
            this.lvModel.View = System.Windows.Forms.View.Details;
            // 
            // groupBox11
            // 
            this.groupBox11.Controls.Add(this.groupBox12);
            this.groupBox11.Controls.Add(this.groupBox13);
            this.groupBox11.Controls.Add(this.label39);
            this.groupBox11.Controls.Add(this.tbThreshold);
            this.groupBox11.Location = new System.Drawing.Point(6, 4);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.Size = new System.Drawing.Size(369, 149);
            this.groupBox11.TabIndex = 121;
            this.groupBox11.TabStop = false;
            this.groupBox11.Text = "Model Evaluation Threshold";
            // 
            // groupBox12
            // 
            this.groupBox12.Controls.Add(this.label1);
            this.groupBox12.Controls.Add(this.tbExponent);
            this.groupBox12.Controls.Add(this.rbPower);
            this.groupBox12.Controls.Add(this.rbLoge);
            this.groupBox12.Controls.Add(this.rbValue);
            this.groupBox12.Controls.Add(this.rbLog10);
            this.groupBox12.Location = new System.Drawing.Point(6, 45);
            this.groupBox12.Name = "groupBox12";
            this.groupBox12.Size = new System.Drawing.Size(173, 96);
            this.groupBox12.TabIndex = 91;
            this.groupBox12.TabStop = false;
            this.groupBox12.Text = "Threshold entry is transformed:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(101, 72);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "exp:";
            // 
            // tbExponent
            // 
            this.tbExponent.Enabled = false;
            this.tbExponent.Location = new System.Drawing.Point(128, 69);
            this.tbExponent.Name = "tbExponent";
            this.tbExponent.Size = new System.Drawing.Size(29, 20);
            this.tbExponent.TabIndex = 19;
            this.tbExponent.Text = "1";
            this.tbExponent.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // rbPower
            // 
            this.rbPower.AutoSize = true;
            this.rbPower.Location = new System.Drawing.Point(9, 70);
            this.rbPower.Name = "rbPower";
            this.rbPower.Size = new System.Drawing.Size(90, 17);
            this.rbPower.TabIndex = 18;
            this.rbPower.Text = "Power (value)";
            this.rbPower.UseVisualStyleBackColor = true;
            // 
            // rbLoge
            // 
            this.rbLoge.AutoSize = true;
            this.rbLoge.Location = new System.Drawing.Point(9, 52);
            this.rbLoge.Name = "rbLoge";
            this.rbLoge.Size = new System.Drawing.Size(84, 17);
            this.rbLoge.TabIndex = 17;
            this.rbLoge.Text = "Loge (value)";
            this.rbLoge.UseVisualStyleBackColor = true;
            this.rbLoge.CheckedChanged += new System.EventHandler(this.rbLogeValue_CheckedChanged);
            // 
            // rbValue
            // 
            this.rbValue.AutoSize = true;
            this.rbValue.Checked = true;
            this.rbValue.Location = new System.Drawing.Point(9, 16);
            this.rbValue.Name = "rbValue";
            this.rbValue.Size = new System.Drawing.Size(52, 17);
            this.rbValue.TabIndex = 15;
            this.rbValue.TabStop = true;
            this.rbValue.Text = "Value";
            this.rbValue.UseVisualStyleBackColor = true;
            this.rbValue.CheckedChanged += new System.EventHandler(this.rbValue_CheckedChanged);
            // 
            // rbLog10
            // 
            this.rbLog10.AutoSize = true;
            this.rbLog10.Location = new System.Drawing.Point(9, 34);
            this.rbLog10.Name = "rbLog10";
            this.rbLog10.Size = new System.Drawing.Size(90, 17);
            this.rbLog10.TabIndex = 16;
            this.rbLog10.Text = "Log10 (value)";
            this.rbLog10.UseVisualStyleBackColor = true;
            this.rbLog10.CheckedChanged += new System.EventHandler(this.rbLog10Value_CheckedChanged);
            // 
            // groupBox13
            // 
            this.groupBox13.Controls.Add(this.label32);
            this.groupBox13.Controls.Add(this.label33);
            this.groupBox13.Controls.Add(this.label34);
            this.groupBox13.Controls.Add(this.label35);
            this.groupBox13.Controls.Add(this.label36);
            this.groupBox13.Controls.Add(this.label37);
            this.groupBox13.Location = new System.Drawing.Point(185, 45);
            this.groupBox13.Name = "groupBox13";
            this.groupBox13.Size = new System.Drawing.Size(178, 96);
            this.groupBox13.TabIndex = 90;
            this.groupBox13.TabStop = false;
            this.groupBox13.Text = "2013 US Regulatory Standards";
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(130, 72);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(25, 13);
            this.label32.TabIndex = 5;
            this.label32.Text = "104";
            this.label32.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(133, 54);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(19, 13);
            this.label33.TabIndex = 4;
            this.label33.Text = "61";
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(133, 36);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(25, 13);
            this.label34.TabIndex = 3;
            this.label34.Text = "235";
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(6, 72);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(117, 13);
            this.label35.TabIndex = 2;
            this.label35.Text = "Enterococci, Saltwater:";
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(6, 54);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(125, 13);
            this.label36.TabIndex = 1;
            this.label36.Text = "Enterococci, Freshwater:";
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(6, 36);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(97, 13);
            this.label37.TabIndex = 0;
            this.label37.Text = "E. coli, Freshwater:";
            // 
            // label39
            // 
            this.label39.AutoSize = true;
            this.label39.ForeColor = System.Drawing.Color.Green;
            this.label39.Location = new System.Drawing.Point(49, 22);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(104, 13);
            this.label39.TabIndex = 82;
            this.label39.Text = "Regulatory Standard";
            // 
            // tbThreshold
            // 
            this.tbThreshold.Location = new System.Drawing.Point(10, 19);
            this.tbThreshold.Name = "tbThreshold";
            this.tbThreshold.Size = new System.Drawing.Size(33, 20);
            this.tbThreshold.TabIndex = 82;
            this.tbThreshold.Text = "235";
            this.tbThreshold.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.tbThreshold.TextChanged += new System.EventHandler(this.tbThreshold_TextChanged);
            // 
            // DiagnosticTab
            // 
            this.DiagnosticTab.BackColor = System.Drawing.SystemColors.Control;
            this.DiagnosticTab.Controls.Add(this.zgcDiagnostic);
            this.DiagnosticTab.Location = new System.Drawing.Point(4, 22);
            this.DiagnosticTab.Name = "DiagnosticTab";
            this.DiagnosticTab.Size = new System.Drawing.Size(623, 584);
            this.DiagnosticTab.TabIndex = 3;
            this.DiagnosticTab.Text = "Diagnostics";
            // 
            // zgcDiagnostic
            // 
            this.zgcDiagnostic.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.zgcDiagnostic.Location = new System.Drawing.Point(3, 3);
            this.zgcDiagnostic.Name = "zgcDiagnostic";
            this.zgcDiagnostic.ScrollGrace = 0D;
            this.zgcDiagnostic.ScrollMaxX = 0D;
            this.zgcDiagnostic.ScrollMaxY = 0D;
            this.zgcDiagnostic.ScrollMaxY2 = 0D;
            this.zgcDiagnostic.ScrollMinX = 0D;
            this.zgcDiagnostic.ScrollMinY = 0D;
            this.zgcDiagnostic.ScrollMinY2 = 0D;
            this.zgcDiagnostic.Size = new System.Drawing.Size(602, 565);
            this.zgcDiagnostic.TabIndex = 64;
            // 
            // RModelingGui
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoScrollMinSize = new System.Drawing.Size(400, 610);
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tabControl1);
            this.Name = "RModelingGui";
            this.Size = new System.Drawing.Size(613, 602);
            this.tabControl1.ResumeLayout(false);
            this.DatasheetTab.ResumeLayout(false);
            this.DatasheetTab.PerformLayout();
            this.VariableSelectionTab.ResumeLayout(false);
            this.VariableSelectionTab.PerformLayout();
            this.ModelingTab.ResumeLayout(false);
            this.ModelingTab.PerformLayout();
            this.pnlThresholdingButtons.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartValidation)).EndInit();
            this.groupBox10.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox11.ResumeLayout(false);
            this.groupBox11.PerformLayout();
            this.groupBox12.ResumeLayout(false);
            this.groupBox12.PerformLayout();
            this.groupBox13.ResumeLayout(false);
            this.groupBox13.PerformLayout();
            this.DiagnosticTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage VariableSelectionTab;
        private System.Windows.Forms.Label lblNumObs;
        private System.Windows.Forms.Button btnRemoveInputVariable;
        private System.Windows.Forms.Button btnAddInputVariable;
        private System.Windows.Forms.Label lbDepVarName;
        private System.Windows.Forms.Label lblDepVariable;
        private System.Windows.Forms.Label label14;
        public System.Windows.Forms.ListBox lbIndVariables;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ListBox lbAvailableVariables;
        private System.Windows.Forms.TabPage ModelingTab;
        protected System.Windows.Forms.GroupBox groupBox11;
        protected System.Windows.Forms.GroupBox groupBox12;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.TextBox tbExponent;
        protected System.Windows.Forms.RadioButton rbPower;
        protected System.Windows.Forms.RadioButton rbLoge;
        protected System.Windows.Forms.RadioButton rbValue;
        protected System.Windows.Forms.RadioButton rbLog10;
        protected System.Windows.Forms.GroupBox groupBox13;
        protected System.Windows.Forms.Label label32;
        protected System.Windows.Forms.Label label33;
        protected System.Windows.Forms.Label label34;
        protected System.Windows.Forms.Label label35;
        protected System.Windows.Forms.Label label36;
        protected System.Windows.Forms.Label label37;
        protected System.Windows.Forms.Label label39;
        protected System.Windows.Forms.TextBox tbThreshold;
        protected System.Windows.Forms.Label label4;
        protected System.Windows.Forms.GroupBox groupBox6;
        protected System.Windows.Forms.ListView lvModel;
        protected System.Windows.Forms.GroupBox groupBox10;
        protected System.Windows.Forms.ListView lvValidation;
        protected System.Windows.Forms.ColumnHeader TruePos;
        protected System.Windows.Forms.ColumnHeader TrueNeg;
        protected System.Windows.Forms.ColumnHeader FalsePos;
        protected System.Windows.Forms.ColumnHeader FalseNeg;
        protected System.Windows.Forms.Label lblDecisionThreshold;
        private System.Windows.Forms.Label lblDepVars;
        private System.Windows.Forms.Label lblAvailVars;
        private System.Windows.Forms.TabPage DiagnosticTab;
        private System.Windows.Forms.TabPage DatasheetTab;
        private VBCommon.Controls.DatasheetControl dsControl1;
        private System.Windows.Forms.Label label3;
        private ZedGraph.ZedGraphControl zgcDiagnostic;
        protected System.Windows.Forms.ColumnHeader Accuracy;
        protected System.Windows.Forms.ColumnHeader Sensitivity;
        protected System.Windows.Forms.ColumnHeader Specificity;
        protected System.Windows.Forms.Label lblRandomSeed;
        public System.Windows.Forms.TextBox tbRandomSeed;
        public System.Windows.Forms.CheckBox cbRandomSeed;
        protected System.Windows.Forms.Panel pnlThresholdingButtons;
        protected System.Windows.Forms.Button btnRight25;
        protected System.Windows.Forms.Button btnRight1;
        protected System.Windows.Forms.Button btnLeft1;
        protected System.Windows.Forms.Button btnLeft25;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblValidationTitle;
        protected System.Windows.Forms.Label lblSpec;
        protected System.Windows.Forms.DataVisualization.Charting.Chart chartValidation;
    }
}
