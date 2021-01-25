namespace LeaRun.WebSocketService
{
    partial class Websocket
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.txtprot = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.txtIpurl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtprot
            // 
            this.txtprot.Location = new System.Drawing.Point(199, 27);
            this.txtprot.Name = "txtprot";
            this.txtprot.Size = new System.Drawing.Size(100, 21);
            this.txtprot.TabIndex = 16;
            this.txtprot.Text = "8203";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(164, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 15;
            this.label2.Text = "端口";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(320, 27);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 14;
            this.button1.Text = "启动";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtIpurl
            // 
            this.txtIpurl.Location = new System.Drawing.Point(51, 27);
            this.txtIpurl.Name = "txtIpurl";
            this.txtIpurl.Size = new System.Drawing.Size(100, 21);
            this.txtIpurl.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 12;
            this.label1.Text = "ip地址";
            // 
            // txtInfo
            // 
            this.txtInfo.Location = new System.Drawing.Point(5, 56);
            this.txtInfo.Multiline = true;
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.Size = new System.Drawing.Size(390, 287);
            this.txtInfo.TabIndex = 17;
            // 
            // Websocket
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(409, 369);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.txtprot);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtIpurl);
            this.Controls.Add(this.label1);
            this.Name = "Websocket";
            this.Text = "websocket服务";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtprot;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtIpurl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtInfo;
    }
}

