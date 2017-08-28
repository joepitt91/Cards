namespace JoePitt.Cards.UI
{
    partial class Waiting
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Waiting));
            this.picBlackCard = new System.Windows.Forms.PictureBox();
            this.txtMessage = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.picBlackCard)).BeginInit();
            this.SuspendLayout();
            // 
            // picBlackCard
            // 
            this.picBlackCard.BackColor = System.Drawing.Color.Black;
            this.picBlackCard.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.picBlackCard.Image = ((System.Drawing.Image)(resources.GetObject("picBlackCard.Image")));
            this.picBlackCard.Location = new System.Drawing.Point(0, 308);
            this.picBlackCard.Name = "picBlackCard";
            this.picBlackCard.Size = new System.Drawing.Size(327, 54);
            this.picBlackCard.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picBlackCard.TabIndex = 5;
            this.picBlackCard.TabStop = false;
            this.picBlackCard.UseWaitCursor = true;
            // 
            // txtMessage
            // 
            this.txtMessage.BackColor = System.Drawing.Color.Black;
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMessage.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.txtMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMessage.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMessage.ForeColor = System.Drawing.Color.White;
            this.txtMessage.Location = new System.Drawing.Point(0, 0);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ReadOnly = true;
            this.txtMessage.Size = new System.Drawing.Size(327, 362);
            this.txtMessage.TabIndex = 6;
            this.txtMessage.Text = "Waiting for [something]...";
            this.txtMessage.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtMessage.UseWaitCursor = true;
            // 
            // Waiting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(327, 362);
            this.Controls.Add(this.picBlackCard);
            this.Controls.Add(this.txtMessage);
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Waiting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Waiting - Cards";
            this.UseWaitCursor = true;
            this.Load += new System.EventHandler(this.Waiting_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picBlackCard)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picBlackCard;
        private System.Windows.Forms.TextBox txtMessage;
    }
}