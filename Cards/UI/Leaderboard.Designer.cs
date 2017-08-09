namespace JoePitt.Cards.UI
{
    partial class Leaderboard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Leaderboard));
            this.lstLeaderboard = new System.Windows.Forms.ListView();
            this.colPosition = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPlayer = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPoints = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // lstLeaderboard
            // 
            this.lstLeaderboard.AccessibleName = "Leaderboard";
            this.lstLeaderboard.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colPosition,
            this.colPlayer,
            this.colPoints});
            this.lstLeaderboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstLeaderboard.FullRowSelect = true;
            this.lstLeaderboard.GridLines = true;
            this.lstLeaderboard.Location = new System.Drawing.Point(0, 0);
            this.lstLeaderboard.MultiSelect = false;
            this.lstLeaderboard.Name = "lstLeaderboard";
            this.lstLeaderboard.Size = new System.Drawing.Size(273, 407);
            this.lstLeaderboard.TabIndex = 3;
            this.lstLeaderboard.UseCompatibleStateImageBehavior = false;
            this.lstLeaderboard.View = System.Windows.Forms.View.Details;
            // 
            // colPosition
            // 
            this.colPosition.Text = "No.";
            this.colPosition.Width = 30;
            // 
            // colPlayer
            // 
            this.colPlayer.Text = "Player";
            this.colPlayer.Width = 188;
            // 
            // colPoints
            // 
            this.colPoints.Text = "Points";
            this.colPoints.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.colPoints.Width = 50;
            // 
            // Leaderboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(273, 407);
            this.Controls.Add(this.lstLeaderboard);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Leaderboard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Leaderboard";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ListView lstLeaderboard;
        private System.Windows.Forms.ColumnHeader colPosition;
        private System.Windows.Forms.ColumnHeader colPlayer;
        private System.Windows.Forms.ColumnHeader colPoints;
    }
}