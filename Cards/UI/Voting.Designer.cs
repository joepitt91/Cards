namespace JoePitt.Cards.UI
{
    partial class Voting
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Voting));
            this.btnVote = new System.Windows.Forms.Button();
            this.picBlackCard = new System.Windows.Forms.PictureBox();
            this.txtAnswer = new System.Windows.Forms.TextBox();
            this.cmbAnswers = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.picBlackCard)).BeginInit();
            this.SuspendLayout();
            // 
            // btnVote
            // 
            this.btnVote.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVote.Location = new System.Drawing.Point(12, 381);
            this.btnVote.Name = "btnVote";
            this.btnVote.Size = new System.Drawing.Size(326, 44);
            this.btnVote.TabIndex = 2;
            this.btnVote.Text = "&Vote";
            this.btnVote.UseVisualStyleBackColor = true;
            this.btnVote.Click += new System.EventHandler(this.btnVote_Click);
            // 
            // picBlackCard
            // 
            this.picBlackCard.BackColor = System.Drawing.Color.AntiqueWhite;
            this.picBlackCard.Image = ((System.Drawing.Image)(resources.GetObject("picBlackCard.Image")));
            this.picBlackCard.Location = new System.Drawing.Point(13, 320);
            this.picBlackCard.Name = "picBlackCard";
            this.picBlackCard.Size = new System.Drawing.Size(324, 54);
            this.picBlackCard.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picBlackCard.TabIndex = 7;
            this.picBlackCard.TabStop = false;
            // 
            // txtAnswer
            // 
            this.txtAnswer.AccessibleName = "Voting For";
            this.txtAnswer.BackColor = System.Drawing.Color.AntiqueWhite;
            this.txtAnswer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtAnswer.Cursor = System.Windows.Forms.Cursors.Default;
            this.txtAnswer.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAnswer.ForeColor = System.Drawing.Color.Black;
            this.txtAnswer.Location = new System.Drawing.Point(12, 13);
            this.txtAnswer.Multiline = true;
            this.txtAnswer.Name = "txtAnswer";
            this.txtAnswer.ReadOnly = true;
            this.txtAnswer.Size = new System.Drawing.Size(326, 362);
            this.txtAnswer.TabIndex = 1;
            this.txtAnswer.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // cmbAnswers
            // 
            this.cmbAnswers.AccessibleName = "Select Best Answer";
            this.cmbAnswers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAnswers.FormattingEnabled = true;
            this.cmbAnswers.Location = new System.Drawing.Point(12, 13);
            this.cmbAnswers.Name = "cmbAnswers";
            this.cmbAnswers.Size = new System.Drawing.Size(343, 22);
            this.cmbAnswers.TabIndex = 0;
            this.cmbAnswers.SelectedIndexChanged += new System.EventHandler(this.cmbAnswers_SelectedIndexChanged);
            // 
            // Vote
            // 
            this.AcceptButton = this.btnVote;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(366, 438);
            this.Controls.Add(this.picBlackCard);
            this.Controls.Add(this.txtAnswer);
            this.Controls.Add(this.btnVote);
            this.Controls.Add(this.cmbAnswers);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Vote";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = " - Vote - Cards";
            this.Load += new System.EventHandler(this.Vote_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picBlackCard)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnVote;
        private System.Windows.Forms.PictureBox picBlackCard;
        private System.Windows.Forms.TextBox txtAnswer;
        private System.Windows.Forms.ComboBox cmbAnswers;
    }
}