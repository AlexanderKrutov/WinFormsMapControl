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
            this.cbxMarkerLayer = new System.Windows.Forms.CheckBox();
            this.cbxTrackLayer = new System.Windows.Forms.CheckBox();
            this.cbxPolygonLayer = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnClearCache
            // 
            this.btnClearCache.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearCache.Location = new System.Drawing.Point(599, 5);
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
            this.cmbTileServers.Location = new System.Drawing.Point(381, 6);
            this.cmbTileServers.Name = "cmbTileServers";
            this.cmbTileServers.Size = new System.Drawing.Size(210, 21);
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
            this.cmbExample.Size = new System.Drawing.Size(210, 21);
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
            this.lblTileServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTileServer.AutoSize = true;
            this.lblTileServer.Location = new System.Drawing.Point(316, 10);
            this.lblTileServer.Name = "lblTileServer";
            this.lblTileServer.Size = new System.Drawing.Size(59, 13);
            this.lblTileServer.TabIndex = 5;
            this.lblTileServer.Text = "Tile server:";
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
            this.mapControl.Location = new System.Drawing.Point(0, 76);
            this.mapControl.Name = "mapControl";
            this.mapControl.ShowThumbnails = true;
            this.mapControl.Size = new System.Drawing.Size(684, 286);
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
            // cbxMarkerLayer
            // 
            this.cbxMarkerLayer.AutoSize = true;
            this.cbxMarkerLayer.Checked = true;
            this.cbxMarkerLayer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxMarkerLayer.Location = new System.Drawing.Point(13, 44);
            this.cbxMarkerLayer.Name = "cbxMarkerLayer";
            this.cbxMarkerLayer.Size = new System.Drawing.Size(115, 17);
            this.cbxMarkerLayer.TabIndex = 6;
            this.cbxMarkerLayer.Text = "Show MarkerLayer";
            this.cbxMarkerLayer.UseVisualStyleBackColor = true;
            this.cbxMarkerLayer.CheckedChanged += new System.EventHandler(this.cbxMarkerLayer_CheckedChanged);
            // 
            // cbxTrackLayer
            // 
            this.cbxTrackLayer.AutoSize = true;
            this.cbxTrackLayer.Checked = true;
            this.cbxTrackLayer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxTrackLayer.Location = new System.Drawing.Point(134, 44);
            this.cbxTrackLayer.Name = "cbxTrackLayer";
            this.cbxTrackLayer.Size = new System.Drawing.Size(110, 17);
            this.cbxTrackLayer.TabIndex = 7;
            this.cbxTrackLayer.Text = "Show TrackLayer";
            this.cbxTrackLayer.UseVisualStyleBackColor = true;
            this.cbxTrackLayer.CheckedChanged += new System.EventHandler(this.cbxTrackLayer_CheckedChanged);
            // 
            // cbxPolygonLayer
            // 
            this.cbxPolygonLayer.AutoSize = true;
            this.cbxPolygonLayer.Checked = true;
            this.cbxPolygonLayer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxPolygonLayer.Location = new System.Drawing.Point(250, 44);
            this.cbxPolygonLayer.Name = "cbxPolygonLayer";
            this.cbxPolygonLayer.Size = new System.Drawing.Size(120, 17);
            this.cbxPolygonLayer.TabIndex = 8;
            this.cbxPolygonLayer.Text = "Show PolygonLayer";
            this.cbxPolygonLayer.UseVisualStyleBackColor = true;
            this.cbxPolygonLayer.CheckedChanged += new System.EventHandler(this.cbxPolygonLayer_CheckedChanged);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 362);
            this.Controls.Add(this.cbxPolygonLayer);
            this.Controls.Add(this.cbxTrackLayer);
            this.Controls.Add(this.cbxMarkerLayer);
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
        private CheckBox cbxMarkerLayer;
        private CheckBox cbxTrackLayer;
        private CheckBox cbxPolygonLayer;
    }
}

