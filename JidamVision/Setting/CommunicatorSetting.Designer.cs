namespace JidamVision.Setting
{
    partial class CommunicatorSetting
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
            this.lbCommType = new System.Windows.Forms.Label();
            this.laIpAddr = new System.Windows.Forms.Label();
            this.cbCommType = new System.Windows.Forms.ComboBox();
            this.txtIpAddr = new System.Windows.Forms.TextBox();
            this.btnApply = new System.Windows.Forms.Button();
            this.txtMachine = new System.Windows.Forms.TextBox();
            this.lbMachine = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbCommType
            // 
            this.lbCommType.AutoSize = true;
            this.lbCommType.Location = new System.Drawing.Point(11, 39);
            this.lbCommType.Name = "lbCommType";
            this.lbCommType.Size = new System.Drawing.Size(53, 12);
            this.lbCommType.TabIndex = 0;
            this.lbCommType.Text = "통신타입";
            // 
            // laIpAddr
            // 
            this.laIpAddr.AutoSize = true;
            this.laIpAddr.Location = new System.Drawing.Point(11, 65);
            this.laIpAddr.Name = "laIpAddr";
            this.laIpAddr.Size = new System.Drawing.Size(44, 12);
            this.laIpAddr.TabIndex = 1;
            this.laIpAddr.Text = "IP 주소";
            // 
            // cbCommType
            // 
            this.cbCommType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCommType.FormattingEnabled = true;
            this.cbCommType.Location = new System.Drawing.Point(81, 36);
            this.cbCommType.Name = "cbCommType";
            this.cbCommType.Size = new System.Drawing.Size(142, 20);
            this.cbCommType.TabIndex = 2;
            // 
            // txtIpAddr
            // 
            this.txtIpAddr.Location = new System.Drawing.Point(81, 62);
            this.txtIpAddr.Name = "txtIpAddr";
            this.txtIpAddr.Size = new System.Drawing.Size(142, 21);
            this.txtIpAddr.TabIndex = 3;
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(148, 89);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(75, 23);
            this.btnApply.TabIndex = 4;
            this.btnApply.Text = "적용";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // txtMachine
            // 
            this.txtMachine.Location = new System.Drawing.Point(81, 9);
            this.txtMachine.Name = "txtMachine";
            this.txtMachine.Size = new System.Drawing.Size(142, 21);
            this.txtMachine.TabIndex = 6;
            // 
            // lbMachine
            // 
            this.lbMachine.AutoSize = true;
            this.lbMachine.Location = new System.Drawing.Point(11, 12);
            this.lbMachine.Name = "lbMachine";
            this.lbMachine.Size = new System.Drawing.Size(41, 12);
            this.lbMachine.TabIndex = 5;
            this.lbMachine.Text = "설비명";
            // 
            // CommunicatorSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtMachine);
            this.Controls.Add(this.lbMachine);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.txtIpAddr);
            this.Controls.Add(this.cbCommType);
            this.Controls.Add(this.laIpAddr);
            this.Controls.Add(this.lbCommType);
            this.Name = "CommunicatorSetting";
            this.Size = new System.Drawing.Size(233, 118);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbCommType;
        private System.Windows.Forms.Label laIpAddr;
        private System.Windows.Forms.ComboBox cbCommType;
        private System.Windows.Forms.TextBox txtIpAddr;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.TextBox txtMachine;
        private System.Windows.Forms.Label lbMachine;
    }
}
