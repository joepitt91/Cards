namespace JoePitt.Cards.UI
{
    /// <summary>
    /// Auto generated designer class.
    /// </summary>
    partial class ConnectionDetails
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionDetails));
            this.lstConnections = new System.Windows.Forms.ListView();
            this.colIPAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPort = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colScope = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colConnectivity = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblConnectionDetials = new System.Windows.Forms.Label();
            this.lstPlayers = new System.Windows.Forms.ListView();
            this.btnTestSelected = new System.Windows.Forms.Button();
            this.lblPlayersJoined = new System.Windows.Forms.Label();
            this.btnCopyToClipboard = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lstConnections
            // 
            this.lstConnections.AccessibleDescription = "Provide details of one of the below connections to the other players to allow the" +
    "m to join:";
            this.lstConnections.AccessibleName = "Connection Details";
            this.lstConnections.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colIPAddress,
            this.colPort,
            this.colScope,
            this.colConnectivity});
            this.lstConnections.FullRowSelect = true;
            this.lstConnections.GridLines = true;
            this.lstConnections.HideSelection = false;
            this.lstConnections.Location = new System.Drawing.Point(12, 25);
            this.lstConnections.Name = "lstConnections";
            this.lstConnections.Size = new System.Drawing.Size(610, 114);
            this.lstConnections.TabIndex = 1;
            this.lstConnections.UseCompatibleStateImageBehavior = false;
            this.lstConnections.View = System.Windows.Forms.View.Details;
            // 
            // colIPAddress
            // 
            this.colIPAddress.Text = "IP Address";
            this.colIPAddress.Width = 250;
            // 
            // colPort
            // 
            this.colPort.Text = "Port";
            this.colPort.Width = 50;
            // 
            // colScope
            // 
            this.colScope.Text = "Scope";
            this.colScope.Width = 90;
            // 
            // colConnectivity
            // 
            this.colConnectivity.Text = "Port Forwarding";
            this.colConnectivity.Width = 190;
            // 
            // lblConnectionDetials
            // 
            this.lblConnectionDetials.AutoSize = true;
            this.lblConnectionDetials.Location = new System.Drawing.Point(12, 9);
            this.lblConnectionDetials.Name = "lblConnectionDetials";
            this.lblConnectionDetials.Size = new System.Drawing.Size(423, 13);
            this.lblConnectionDetials.TabIndex = 0;
            this.lblConnectionDetials.Text = "Provide details of one of the below connections to the other players to allow the" +
    "m to join:";
            // 
            // lstPlayers
            // 
            this.lstPlayers.AccessibleDescription = "The following players have joined:";
            this.lstPlayers.AccessibleName = "Players";
            this.lstPlayers.GridLines = true;
            this.lstPlayers.Location = new System.Drawing.Point(12, 196);
            this.lstPlayers.Name = "lstPlayers";
            this.lstPlayers.Size = new System.Drawing.Size(610, 53);
            this.lstPlayers.TabIndex = 5;
            this.lstPlayers.UseCompatibleStateImageBehavior = false;
            this.lstPlayers.View = System.Windows.Forms.View.List;
            // 
            // btnTestSelected
            // 
            this.btnTestSelected.Location = new System.Drawing.Point(12, 145);
            this.btnTestSelected.Name = "btnTestSelected";
            this.btnTestSelected.Size = new System.Drawing.Size(290, 23);
            this.btnTestSelected.TabIndex = 2;
            this.btnTestSelected.Text = "&Test Selected Connection";
            this.btnTestSelected.UseVisualStyleBackColor = true;
            this.btnTestSelected.Click += new System.EventHandler(this.TestSelected_Click);
            // 
            // lblPlayersJoined
            // 
            this.lblPlayersJoined.AutoSize = true;
            this.lblPlayersJoined.Location = new System.Drawing.Point(12, 180);
            this.lblPlayersJoined.Name = "lblPlayersJoined";
            this.lblPlayersJoined.Size = new System.Drawing.Size(167, 13);
            this.lblPlayersJoined.TabIndex = 4;
            this.lblPlayersJoined.Text = "The following players have joined:";
            // 
            // btnCopyToClipboard
            // 
            this.btnCopyToClipboard.Location = new System.Drawing.Point(332, 145);
            this.btnCopyToClipboard.Name = "btnCopyToClipboard";
            this.btnCopyToClipboard.Size = new System.Drawing.Size(290, 23);
            this.btnCopyToClipboard.TabIndex = 3;
            this.btnCopyToClipboard.Text = "Copy Selected Connection to Clipboard";
            this.btnCopyToClipboard.UseVisualStyleBackColor = true;
            this.btnCopyToClipboard.Click += new System.EventHandler(this.CopyToClipboard_Click);
            // 
            // ConnectionDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 261);
            this.Controls.Add(this.btnCopyToClipboard);
            this.Controls.Add(this.lblPlayersJoined);
            this.Controls.Add(this.btnTestSelected);
            this.Controls.Add(this.lstPlayers);
            this.Controls.Add(this.lblConnectionDetials);
            this.Controls.Add(this.lstConnections);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ConnectionDetails";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Connection Details";
            this.Load += new System.EventHandler(this.ConnectionDetails_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lstConnections;
        private System.Windows.Forms.ColumnHeader colIPAddress;
        private System.Windows.Forms.ColumnHeader colPort;
        private System.Windows.Forms.ColumnHeader colScope;
        private System.Windows.Forms.ColumnHeader colConnectivity;
        private System.Windows.Forms.Label lblConnectionDetials;
        private System.Windows.Forms.ListView lstPlayers;
        private System.Windows.Forms.Button btnTestSelected;
        private System.Windows.Forms.Label lblPlayersJoined;
        private System.Windows.Forms.Button btnCopyToClipboard;
    }
}