namespace JidamVision
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.FileTopMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ModelNewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ModelOpenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ModelSaveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ModelSaveAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ImageLoadMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImageSaveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetupTopMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetupMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileTopMenuItem,
            this.SetupTopMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // FileTopMenuItem
            // 
            this.FileTopMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ModelNewMenuItem,
            this.ModelOpenMenuItem,
            this.ModelSaveMenuItem,
            this.ModelSaveAsMenuItem,
            this.toolStripSeparator1,
            this.ImageLoadMenuItem,
            this.ImageSaveMenuItem});
            this.FileTopMenuItem.Name = "FileTopMenuItem";
            this.FileTopMenuItem.Size = new System.Drawing.Size(37, 20);
            this.FileTopMenuItem.Text = "File";
            // 
            // ModelNewMenuItem
            // 
            this.ModelNewMenuItem.Name = "ModelNewMenuItem";
            this.ModelNewMenuItem.Size = new System.Drawing.Size(180, 22);
            this.ModelNewMenuItem.Text = "Model New";
            this.ModelNewMenuItem.Click += new System.EventHandler(this.ModelNewMenuItem_Click);
            // 
            // ModelOpenMenuItem
            // 
            this.ModelOpenMenuItem.Name = "ModelOpenMenuItem";
            this.ModelOpenMenuItem.Size = new System.Drawing.Size(180, 22);
            this.ModelOpenMenuItem.Text = "Model Open";
            this.ModelOpenMenuItem.Click += new System.EventHandler(this.ModelOpenMenuItem_Click);
            // 
            // ModelSaveMenuItem
            // 
            this.ModelSaveMenuItem.Name = "ModelSaveMenuItem";
            this.ModelSaveMenuItem.Size = new System.Drawing.Size(180, 22);
            this.ModelSaveMenuItem.Text = "Model Save";
            this.ModelSaveMenuItem.Click += new System.EventHandler(this.ModelSaveMenuItem_Click);
            // 
            // ModelSaveAsMenuItem
            // 
            this.ModelSaveAsMenuItem.Name = "ModelSaveAsMenuItem";
            this.ModelSaveAsMenuItem.Size = new System.Drawing.Size(180, 22);
            this.ModelSaveAsMenuItem.Text = "Model SaveAs";
            this.ModelSaveAsMenuItem.Click += new System.EventHandler(this.ModelSaveAsMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // ImageLoadMenuItem
            // 
            this.ImageLoadMenuItem.Name = "ImageLoadMenuItem";
            this.ImageLoadMenuItem.Size = new System.Drawing.Size(180, 22);
            this.ImageLoadMenuItem.Text = "Image Load";
            this.ImageLoadMenuItem.Click += new System.EventHandler(this.ImageLoadMenuItem_Click);
            // 
            // ImageSaveMenuItem
            // 
            this.ImageSaveMenuItem.Name = "ImageSaveMenuItem";
            this.ImageSaveMenuItem.Size = new System.Drawing.Size(180, 22);
            this.ImageSaveMenuItem.Text = "Image Save";
            this.ImageSaveMenuItem.Click += new System.EventHandler(this.ImageSaveMenuItem_Click);
            // 
            // SetupTopMenuItem
            // 
            this.SetupTopMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SetupMenuItem});
            this.SetupTopMenuItem.Name = "SetupTopMenuItem";
            this.SetupTopMenuItem.Size = new System.Drawing.Size(50, 20);
            this.SetupTopMenuItem.Text = "Setup";
            // 
            // SetupMenuItem
            // 
            this.SetupMenuItem.Name = "SetupMenuItem";
            this.SetupMenuItem.Size = new System.Drawing.Size(180, 22);
            this.SetupMenuItem.Text = "Setup";
            this.SetupMenuItem.Click += new System.EventHandler(this.SetupMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem FileTopMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SetupTopMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SetupMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ModelOpenMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ModelSaveMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ModelSaveAsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ImageLoadMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ImageSaveMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem ModelNewMenuItem;
    }
}