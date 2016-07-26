namespace StampClient
{
    partial class StampClientMainWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StampClientMainWindow));
            this.tbOutput = new System.Windows.Forms.TextBox();
            this.bwLink = new System.ComponentModel.BackgroundWorker();
            this.bwLink2 = new System.ComponentModel.BackgroundWorker();
            this.button4 = new System.Windows.Forms.Button();
            this.tbXBTBalance = new System.Windows.Forms.TextBox();
            this.tbUSDBalance = new System.Windows.Forms.TextBox();
            this.tbProfit = new System.Windows.Forms.TextBox();
            this.tbMinProfit = new System.Windows.Forms.TextBox();
            this.btnUpdateMinProfit = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tbOutput
            // 
            this.tbOutput.Location = new System.Drawing.Point(6, 57);
            this.tbOutput.Margin = new System.Windows.Forms.Padding(2);
            this.tbOutput.Multiline = true;
            this.tbOutput.Name = "tbOutput";
            this.tbOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbOutput.Size = new System.Drawing.Size(548, 233);
            this.tbOutput.TabIndex = 4;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(6, 4);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 10;
            this.button4.Text = "Start";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // tbXBTBalance
            // 
            this.tbXBTBalance.Location = new System.Drawing.Point(6, 309);
            this.tbXBTBalance.Name = "tbXBTBalance";
            this.tbXBTBalance.Size = new System.Drawing.Size(93, 20);
            this.tbXBTBalance.TabIndex = 11;
            // 
            // tbUSDBalance
            // 
            this.tbUSDBalance.Location = new System.Drawing.Point(203, 309);
            this.tbUSDBalance.Name = "tbUSDBalance";
            this.tbUSDBalance.Size = new System.Drawing.Size(93, 20);
            this.tbUSDBalance.TabIndex = 12;
            // 
            // tbProfit
            // 
            this.tbProfit.Location = new System.Drawing.Point(270, 32);
            this.tbProfit.Name = "tbProfit";
            this.tbProfit.Size = new System.Drawing.Size(178, 20);
            this.tbProfit.TabIndex = 13;
            // 
            // tbMinProfit
            // 
            this.tbMinProfit.Location = new System.Drawing.Point(270, 6);
            this.tbMinProfit.Name = "tbMinProfit";
            this.tbMinProfit.Size = new System.Drawing.Size(100, 20);
            this.tbMinProfit.TabIndex = 14;
            this.tbMinProfit.Text = "1.0";
            // 
            // btnUpdateMinProfit
            // 
            this.btnUpdateMinProfit.Location = new System.Drawing.Point(479, 4);
            this.btnUpdateMinProfit.Name = "btnUpdateMinProfit";
            this.btnUpdateMinProfit.Size = new System.Drawing.Size(75, 23);
            this.btnUpdateMinProfit.TabIndex = 15;
            this.btnUpdateMinProfit.Text = "Update";
            this.btnUpdateMinProfit.UseVisualStyleBackColor = true;
            this.btnUpdateMinProfit.Click += new System.EventHandler(this.btnUpdateMinProfit_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(200, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "Min. profit %";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(198, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 18;
            this.label2.Text = "Curr. profit %";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(376, 6);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 20;
            this.textBox1.Text = "1.0";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(454, 33);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(100, 20);
            this.textBox2.TabIndex = 19;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(203, 335);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(93, 20);
            this.textBox3.TabIndex = 22;
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(6, 335);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(93, 20);
            this.textBox4.TabIndex = 21;
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(375, 308);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(100, 20);
            this.textBox5.TabIndex = 23;
            // 
            // StampClientMainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(561, 394);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnUpdateMinProfit);
            this.Controls.Add(this.tbMinProfit);
            this.Controls.Add(this.tbProfit);
            this.Controls.Add(this.tbUSDBalance);
            this.Controls.Add(this.tbXBTBalance);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.tbOutput);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "StampClientMainWindow";
            this.Text = "Marvin - The Apathic Arbitrage Agent";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbOutput;
        private System.ComponentModel.BackgroundWorker bwLink;
        private System.ComponentModel.BackgroundWorker bwLink2;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox tbXBTBalance;
        private System.Windows.Forms.TextBox tbUSDBalance;
        private System.Windows.Forms.TextBox tbProfit;
        private System.Windows.Forms.TextBox tbMinProfit;
        private System.Windows.Forms.Button btnUpdateMinProfit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox5;
    }
}

