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
            System.Windows.Forms.TrackStyle trackStyle1 = new System.Windows.Forms.TrackStyle();
            this.btnClearCache = new System.Windows.Forms.Button();
            this.cmbTileServers = new System.Windows.Forms.ComboBox();
            this.cmbExample = new System.Windows.Forms.ComboBox();
            this.mapControl = new System.Windows.Forms.MapControl();
            this.SuspendLayout();
            // 
            // btnClearCache
            // 
            this.btnClearCache.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearCache.Location = new System.Drawing.Point(721, 3);
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
            this.cmbTileServers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbTileServers.DisplayMember = "Name";
            this.cmbTileServers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTileServers.FormattingEnabled = true;
            this.cmbTileServers.Location = new System.Drawing.Point(505, 5);
            this.cmbTileServers.Name = "cmbTileServers";
            this.cmbTileServers.Size = new System.Drawing.Size(210, 21);
            this.cmbTileServers.TabIndex = 2;
            this.cmbTileServers.SelectedIndexChanged += new System.EventHandler(this.cmbTileServers_SelectedIndexChanged);
            // 
            // cmbExample
            // 
            this.cmbExample.AllowDrop = true;
            this.cmbExample.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbExample.DisplayMember = "Title";
            this.cmbExample.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbExample.FormattingEnabled = true;
            this.cmbExample.Location = new System.Drawing.Point(12, 12);
            this.cmbExample.Name = "cmbExample";
            this.cmbExample.Size = new System.Drawing.Size(210, 21);
            this.cmbExample.TabIndex = 3;
            this.cmbExample.SelectedIndexChanged += new System.EventHandler(this.cmbExample_SelectedIndexChanged);
            // 
            // mapControl
            // 
            this.mapControl.BackColor = System.Drawing.Color.White;
            this.mapControl.CacheFolder = "C:\\Users\\krutov\\AppData\\Roaming\\MapControl";
            this.mapControl.Cursor = System.Windows.Forms.Cursors.Cross;
            trackStyle1.Pen = null;
            this.mapControl.DefaultTrackStyle = trackStyle1;
            this.mapControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapControl.ForeColor = System.Drawing.Color.Black;
            this.mapControl.Location = new System.Drawing.Point(0, 0);
            this.mapControl.MaxZoomLevel = 19;
            this.mapControl.MinZoomLevel = 0;
            this.mapControl.Name = "mapControl";
            this.mapControl.Size = new System.Drawing.Size(800, 450);
            this.mapControl.TabIndex = 0;
            this.mapControl.Text = "mapControl1";
            this.mapControl.TileServer = null;
            this.mapControl.ZoomLevel = 0;
            this.mapControl.DrawMarker += new System.EventHandler<System.Windows.Forms.DrawMarkerEventArgs>(this.mapControl_DrawMarker);
            this.mapControl.Paint += new System.Windows.Forms.PaintEventHandler(this.mapControl_Paint);
            this.mapControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.mapControl_MouseMove);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.cmbExample);
            this.Controls.Add(this.cmbTileServers);
            this.Controls.Add(this.btnClearCache);
            this.Controls.Add(this.mapControl);
            this.Name = "FormMain";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private MapControl mapControl;
        private System.Windows.Forms.Button btnClearCache;
        private System.Windows.Forms.ComboBox cmbTileServers;
        private ComboBox cmbExample;
    }
}

