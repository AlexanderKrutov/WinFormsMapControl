using System.Windows.Forms;

namespace DemoApp
{
    partial class FormMain
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
            this.btnClearCache = new System.Windows.Forms.Button();
            this.cmbTileServers = new System.Windows.Forms.ComboBox();
            this.cmbExample = new System.Windows.Forms.ComboBox();
            this.lblExample = new System.Windows.Forms.Label();
            this.lblTileServer = new System.Windows.Forms.Label();
            this.mapControl = new System.Windows.Forms.MapControl();
            this.lblOverlay = new System.Windows.Forms.Label();
            this.cmbOverlay = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // btnClearCache
            // 
            this.btnClearCache.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearCache.Location = new System.Drawing.Point(699, 5);
            this.btnClearCache.Name = "btnClearCache";
            this.btnClearCache.Size = new System.Drawing.Size(75, 23);
            this.btnClearCache.TabIndex = 1;
            this.btnClearCache.Text = "Clear cache";
            this.btnClearCache.UseVisualStyleBackColor = true;
            this.btnClearCache.Click += new System.EventHandler(this.btnClearCache_Click);
            // 
            // cmbTileServers
            // 
            this.cmbTileServers.AllowDrop = true;
            this.cmbTileServers.DisplayMember = "Name";
            this.cmbTileServers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTileServers.FormattingEnabled = true;
            this.cmbTileServers.Location = new System.Drawing.Point(328, 6);
            this.cmbTileServers.Name = "cmbTileServers";
            this.cmbTileServers.Size = new System.Drawing.Size(138, 21);
            this.cmbTileServers.TabIndex = 2;
            this.cmbTileServers.SelectedIndexChanged += new System.EventHandler(this.cmbTileServers_SelectedIndexChanged);
            // 
            // cmbExample
            // 
            this.cmbExample.AllowDrop = true;
            this.cmbExample.DisplayMember = "Title";
            this.cmbExample.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbExample.FormattingEnabled = true;
            this.cmbExample.Location = new System.Drawing.Point(83, 6);
            this.cmbExample.Name = "cmbExample";
            this.cmbExample.Size = new System.Drawing.Size(159, 21);
            this.cmbExample.TabIndex = 3;
            this.cmbExample.SelectedIndexChanged += new System.EventHandler(this.cmbExample_SelectedIndexChanged);
            // 
            // lblExample
            // 
            this.lblExample.AutoSize = true;
            this.lblExample.Location = new System.Drawing.Point(9, 9);
            this.lblExample.Name = "lblExample";
            this.lblExample.Size = new System.Drawing.Size(68, 13);
            this.lblExample.TabIndex = 4;
            this.lblExample.Text = "Sample map:";
            // 
            // lblTileServer
            // 
            this.lblTileServer.AutoSize = true;
            this.lblTileServer.Location = new System.Drawing.Point(263, 10);
            this.lblTileServer.Name = "lblTileServer";
            this.lblTileServer.Size = new System.Drawing.Size(62, 13);
            this.lblTileServer.TabIndex = 5;
            this.lblTileServer.Text = "Main Layer:";
            // 
            // mapControl
            // 
            this.mapControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mapControl.BackColor = System.Drawing.Color.White;
            this.mapControl.Cursor = System.Windows.Forms.Cursors.Cross;
            this.mapControl.ErrorColor = System.Drawing.Color.Red;
            this.mapControl.FitToBounds = true;
            this.mapControl.ForeColor = System.Drawing.Color.Black;
            this.mapControl.Location = new System.Drawing.Point(0, 34);
            this.mapControl.Name = "mapControl";
            this.mapControl.ShowThumbnails = true;
            this.mapControl.Size = new System.Drawing.Size(784, 528);
            this.mapControl.TabIndex = 0;
            this.mapControl.ThumbnailBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.mapControl.ThumbnailForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(176)))), ((int)(((byte)(176)))));
            this.mapControl.ThumbnailText = "Downloading...";
            this.mapControl.TileImageAttributes = null;
            this.mapControl.ZoomLevel = 0;
            this.mapControl.DrawMarker += new System.EventHandler<System.Windows.Forms.DrawMarkerEventArgs>(this.mapControl_DrawMarker);
            this.mapControl.DoubleClick += new System.EventHandler(this.mapControl_DoubleClick);
            this.mapControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.mapControl_MouseMove);
            this.mapControl.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.mapControl_MouseWheel);
            // 
            // lblOverlay
            // 
            this.lblOverlay.AutoSize = true;
            this.lblOverlay.Location = new System.Drawing.Point(503, 10);
            this.lblOverlay.Name = "lblOverlay";
            this.lblOverlay.Size = new System.Drawing.Size(46, 13);
            this.lblOverlay.TabIndex = 7;
            this.lblOverlay.Text = "Overlay:";
            // 
            // cmbOverlay
            // 
            this.cmbOverlay.AllowDrop = true;
            this.cmbOverlay.DisplayMember = "Name";
            this.cmbOverlay.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOverlay.FormattingEnabled = true;
            this.cmbOverlay.Location = new System.Drawing.Point(555, 6);
            this.cmbOverlay.Name = "cmbOverlay";
            this.cmbOverlay.Size = new System.Drawing.Size(138, 21);
            this.cmbOverlay.TabIndex = 6;
            this.cmbOverlay.SelectedIndexChanged += new System.EventHandler(this.cmbOverlay_SelectedIndexChanged);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.lblOverlay);
            this.Controls.Add(this.cmbOverlay);
            this.Controls.Add(this.lblTileServer);
            this.Controls.Add(this.lblExample);
            this.Controls.Add(this.cmbExample);
            this.Controls.Add(this.cmbTileServers);
            this.Controls.Add(this.btnClearCache);
            this.Controls.Add(this.mapControl);
            this.MinimumSize = new System.Drawing.Size(700, 400);
            this.Name = "FormMain";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MapControl mapControl;
        private System.Windows.Forms.Button btnClearCache;
        private System.Windows.Forms.ComboBox cmbTileServers;
        private ComboBox cmbExample;
        private Label lblExample;
        private Label lblTileServer;
        private Label lblOverlay;
        private ComboBox cmbOverlay;
    }
}

