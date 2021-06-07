
namespace Client
{
    partial class load_room_form
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
            this.join_roon_btn = new System.Windows.Forms.Button();
            this.create_room_btn = new System.Windows.Forms.Button();
            this.list_room_rtb = new System.Windows.Forms.RichTextBox();
            this.enter_room_rtb = new System.Windows.Forms.RichTextBox();
            this.input_name_room_label = new System.Windows.Forms.Label();
            this.request_list_roon_btn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // join_roon_btn
            // 
            this.join_roon_btn.Location = new System.Drawing.Point(662, 212);
            this.join_roon_btn.Name = "join_roon_btn";
            this.join_roon_btn.Size = new System.Drawing.Size(87, 60);
            this.join_roon_btn.TabIndex = 0;
            this.join_roon_btn.Text = "Join phòng";
            this.join_roon_btn.UseVisualStyleBackColor = true;
            this.join_roon_btn.Click += new System.EventHandler(this.join_roon_btn_Click);
            // 
            // create_room_btn
            // 
            this.create_room_btn.Location = new System.Drawing.Point(530, 212);
            this.create_room_btn.Name = "create_room_btn";
            this.create_room_btn.Size = new System.Drawing.Size(86, 60);
            this.create_room_btn.TabIndex = 1;
            this.create_room_btn.Text = "Tạo phòng";
            this.create_room_btn.UseVisualStyleBackColor = true;
            this.create_room_btn.Click += new System.EventHandler(this.create_room_btn_Click);
            // 
            // list_room_rtb
            // 
            this.list_room_rtb.Location = new System.Drawing.Point(87, 30);
            this.list_room_rtb.Name = "list_room_rtb";
            this.list_room_rtb.ReadOnly = true;
            this.list_room_rtb.Size = new System.Drawing.Size(387, 223);
            this.list_room_rtb.TabIndex = 2;
            this.list_room_rtb.Text = "";
            // 
            // enter_room_rtb
            // 
            this.enter_room_rtb.Location = new System.Drawing.Point(530, 132);
            this.enter_room_rtb.Name = "enter_room_rtb";
            this.enter_room_rtb.Size = new System.Drawing.Size(219, 50);
            this.enter_room_rtb.TabIndex = 3;
            this.enter_room_rtb.Text = "";
            // 
            // input_name_room_label
            // 
            this.input_name_room_label.AutoSize = true;
            this.input_name_room_label.Font = new System.Drawing.Font("Ink Free", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.input_name_room_label.ForeColor = System.Drawing.Color.Red;
            this.input_name_room_label.Location = new System.Drawing.Point(530, 87);
            this.input_name_room_label.Name = "input_name_room_label";
            this.input_name_room_label.Size = new System.Drawing.Size(185, 29);
            this.input_name_room_label.TabIndex = 4;
            this.input_name_room_label.Text = "Nhập tên phòng";
            // 
            // request_list_roon_btn
            // 
            this.request_list_roon_btn.Location = new System.Drawing.Point(162, 273);
            this.request_list_roon_btn.Name = "request_list_roon_btn";
            this.request_list_roon_btn.Size = new System.Drawing.Size(215, 33);
            this.request_list_roon_btn.TabIndex = 5;
            this.request_list_roon_btn.Text = "Hiện danh sách phòng";
            this.request_list_roon_btn.UseVisualStyleBackColor = true;
            this.request_list_roon_btn.Click += new System.EventHandler(this.request_list_roon_btn_Click);
            // 
            // load_room_form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::Client.Properties.Resources.load_form_background;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(800, 341);
            this.Controls.Add(this.request_list_roon_btn);
            this.Controls.Add(this.input_name_room_label);
            this.Controls.Add(this.enter_room_rtb);
            this.Controls.Add(this.list_room_rtb);
            this.Controls.Add(this.create_room_btn);
            this.Controls.Add(this.join_roon_btn);
            this.Name = "load_room_form";
            this.Text = "Chọn phòng";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button join_roon_btn;
        private System.Windows.Forms.Button create_room_btn;
        private System.Windows.Forms.RichTextBox list_room_rtb;
        private System.Windows.Forms.RichTextBox enter_room_rtb;
        private System.Windows.Forms.Label input_name_room_label;
        private System.Windows.Forms.Button request_list_roon_btn;
    }
}