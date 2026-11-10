using System.Windows.Forms;

namespace CapaPresentacion
{
    public sealed partial class PrincipalForm
    {
        private TableLayoutPanel tlpCalendario;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label lblMesAnio;
        private FlowLayoutPanel flTareas;
        private Label label8;
                private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrincipalForm));
            this.tlpCalendario = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblMesAnio = new System.Windows.Forms.Label();
            this.flTareas = new System.Windows.Forms.FlowLayoutPanel();
            this.label8 = new System.Windows.Forms.Label();
            this.panel3 = new AntdUI.Panel();
            this.panel1 = new AntdUI.Panel();
            this.panel2 = new AntdUI.Panel();
            this.panel4 = new AntdUI.Panel();
            this.panel5 = new AntdUI.Panel();
            this.panel6 = new AntdUI.Panel();
            this.panel7 = new AntdUI.Panel();
            this.btnSiguiente = new AntdUI.Button();
            this.btnAnterior = new AntdUI.Button();
            this.btnAgregarTarea = new AntdUI.Button();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnMinimizar = new AntdUI.Button();
            this.btnConfiguracion = new AntdUI.Button();
            this.btnCerrarSesion = new AntdUI.Button();
            this.label15 = new System.Windows.Forms.Label();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel7.SuspendLayout();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpCalendario
            // 
            this.tlpCalendario.ColumnCount = 7;
            this.tlpCalendario.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tlpCalendario.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28572F));
            this.tlpCalendario.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28572F));
            this.tlpCalendario.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28572F));
            this.tlpCalendario.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28572F));
            this.tlpCalendario.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28572F));
            this.tlpCalendario.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28572F));
            this.tlpCalendario.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tlpCalendario.Location = new System.Drawing.Point(12, 180);
            this.tlpCalendario.Name = "tlpCalendario";
            this.tlpCalendario.RowCount = 6;
            this.tlpCalendario.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpCalendario.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpCalendario.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpCalendario.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpCalendario.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpCalendario.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpCalendario.Size = new System.Drawing.Size(1584, 852);
            this.tlpCalendario.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(204, 36);
            this.label1.TabIndex = 3;
            this.label1.Text = "Lunes";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(204, 36);
            this.label2.TabIndex = 4;
            this.label2.Text = "Martes";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(204, 36);
            this.label3.TabIndex = 5;
            this.label3.Text = "Miércoles";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(204, 36);
            this.label4.TabIndex = 6;
            this.label4.Text = "Jueves";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(0, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(204, 36);
            this.label5.TabIndex = 7;
            this.label5.Text = "Viernes";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(0, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(204, 36);
            this.label6.TabIndex = 8;
            this.label6.Text = "Sábado";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(0, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(204, 36);
            this.label7.TabIndex = 9;
            this.label7.Text = "Domingo";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblMesAnio
            // 
            this.lblMesAnio.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMesAnio.Location = new System.Drawing.Point(12, 60);
            this.lblMesAnio.Name = "lblMesAnio";
            this.lblMesAnio.Size = new System.Drawing.Size(1584, 36);
            this.lblMesAnio.TabIndex = 12;
            this.lblMesAnio.Text = "MesAnio";
            this.lblMesAnio.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // flTareas
            // 
            this.flTareas.AutoScroll = true;
            this.flTareas.BackColor = System.Drawing.Color.LightSteelBlue;
            this.flTareas.Location = new System.Drawing.Point(1608, 180);
            this.flTareas.Name = "flTareas";
            this.flTareas.Size = new System.Drawing.Size(300, 792);
            this.flTareas.TabIndex = 13;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(1608, 144);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(300, 36);
            this.label8.TabIndex = 15;
            this.label8.Text = "Tareas del día seleccionado";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel3
            // 
            this.panel3.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.panel3.BackColor = System.Drawing.Color.Transparent;
            this.panel3.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(15)))), ((int)(((byte)(28)))));
            this.panel3.BorderWidth = 1F;
            this.panel3.Controls.Add(this.label2);
            this.panel3.Location = new System.Drawing.Point(252, 132);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(204, 36);
            this.panel3.TabIndex = 57;
            this.panel3.Text = "panel3";
            // 
            // panel1
            // 
            this.panel1.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(15)))), ((int)(((byte)(28)))));
            this.panel1.BorderWidth = 1F;
            this.panel1.Controls.Add(this.label3);
            this.panel1.Location = new System.Drawing.Point(468, 132);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(204, 36);
            this.panel1.TabIndex = 58;
            this.panel1.Text = "panel1";
            // 
            // panel2
            // 
            this.panel2.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.panel2.BackColor = System.Drawing.Color.Transparent;
            this.panel2.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(15)))), ((int)(((byte)(28)))));
            this.panel2.BorderWidth = 1F;
            this.panel2.Controls.Add(this.label1);
            this.panel2.Location = new System.Drawing.Point(24, 132);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(204, 36);
            this.panel2.TabIndex = 58;
            this.panel2.Text = "panel2";
            // 
            // panel4
            // 
            this.panel4.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.panel4.BackColor = System.Drawing.Color.Transparent;
            this.panel4.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(15)))), ((int)(((byte)(28)))));
            this.panel4.BorderWidth = 1F;
            this.panel4.Controls.Add(this.label4);
            this.panel4.Location = new System.Drawing.Point(696, 132);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(204, 36);
            this.panel4.TabIndex = 59;
            this.panel4.Text = "panel4";
            // 
            // panel5
            // 
            this.panel5.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.panel5.BackColor = System.Drawing.Color.Transparent;
            this.panel5.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(15)))), ((int)(((byte)(28)))));
            this.panel5.BorderWidth = 1F;
            this.panel5.Controls.Add(this.label5);
            this.panel5.Location = new System.Drawing.Point(924, 132);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(204, 36);
            this.panel5.TabIndex = 60;
            this.panel5.Text = "panel5";
            // 
            // panel6
            // 
            this.panel6.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.panel6.BackColor = System.Drawing.Color.Transparent;
            this.panel6.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(15)))), ((int)(((byte)(28)))));
            this.panel6.BorderWidth = 1F;
            this.panel6.Controls.Add(this.label6);
            this.panel6.Location = new System.Drawing.Point(1152, 132);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(204, 36);
            this.panel6.TabIndex = 61;
            this.panel6.Text = "panel6";
            // 
            // panel7
            // 
            this.panel7.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.panel7.BackColor = System.Drawing.Color.Transparent;
            this.panel7.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(15)))), ((int)(((byte)(28)))));
            this.panel7.BorderWidth = 1F;
            this.panel7.Controls.Add(this.label7);
            this.panel7.Location = new System.Drawing.Point(1380, 132);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(204, 36);
            this.panel7.TabIndex = 62;
            this.panel7.Text = "panel7";
            // 
            // btnSiguiente
            // 
            this.btnSiguiente.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnSiguiente.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnSiguiente.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnSiguiente.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(156)))), ((int)(((byte)(195)))));
            this.btnSiguiente.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSiguiente.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnSiguiente.Icon = ((System.Drawing.Image)(resources.GetObject("btnSiguiente.Icon")));
            this.btnSiguiente.IconRatio = 1.2F;
            this.btnSiguiente.Location = new System.Drawing.Point(1536, 84);
            this.btnSiguiente.Name = "btnSiguiente";
            this.btnSiguiente.Size = new System.Drawing.Size(60, 48);
            this.btnSiguiente.TabIndex = 63;
            this.btnSiguiente.Click += new System.EventHandler(this.BtnSiguiente_Click);
            // 
            // btnAnterior
            // 
            this.btnAnterior.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnAnterior.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnAnterior.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnAnterior.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(156)))), ((int)(((byte)(195)))));
            this.btnAnterior.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAnterior.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnAnterior.Icon = ((System.Drawing.Image)(resources.GetObject("btnAnterior.Icon")));
            this.btnAnterior.IconRatio = 1.2F;
            this.btnAnterior.Location = new System.Drawing.Point(12, 84);
            this.btnAnterior.Name = "btnAnterior";
            this.btnAnterior.Size = new System.Drawing.Size(60, 48);
            this.btnAnterior.TabIndex = 64;
            this.btnAnterior.Click += new System.EventHandler(this.BtnAnterior_Click);
            // 
            // btnAgregarTarea
            // 
            this.btnAgregarTarea.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnAgregarTarea.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnAgregarTarea.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnAgregarTarea.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(116)))), ((int)(((byte)(62)))));
            this.btnAgregarTarea.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAgregarTarea.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnAgregarTarea.IconRatio = 1.2F;
            this.btnAgregarTarea.Location = new System.Drawing.Point(1608, 984);
            this.btnAgregarTarea.Name = "btnAgregarTarea";
            this.btnAgregarTarea.Size = new System.Drawing.Size(300, 48);
            this.btnAgregarTarea.TabIndex = 65;
            this.btnAgregarTarea.Text = "Agregar Tarea";
            this.btnAgregarTarea.Click += new System.EventHandler(this.btnAgregarTarea_Click);
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(104)))), ((int)(((byte)(180)))));
            this.pnlTop.Controls.Add(this.btnMinimizar);
            this.pnlTop.Controls.Add(this.btnConfiguracion);
            this.pnlTop.Controls.Add(this.btnCerrarSesion);
            this.pnlTop.Controls.Add(this.label15);
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Padding = new System.Windows.Forms.Padding(10);
            this.pnlTop.Size = new System.Drawing.Size(1932, 60);
            this.pnlTop.TabIndex = 75;
            // 
            // btnMinimizar
            // 
            this.btnMinimizar.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnMinimizar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnMinimizar.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnMinimizar.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(156)))), ((int)(((byte)(195)))));
            this.btnMinimizar.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMinimizar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnMinimizar.Icon = ((System.Drawing.Image)(resources.GetObject("btnMinimizar.Icon")));
            this.btnMinimizar.IconRatio = 1.2F;
            this.btnMinimizar.Location = new System.Drawing.Point(1800, 0);
            this.btnMinimizar.Name = "btnMinimizar";
            this.btnMinimizar.Size = new System.Drawing.Size(60, 60);
            this.btnMinimizar.TabIndex = 78;
            this.btnMinimizar.Click += new System.EventHandler(this.btnMinimizar_Click);
            // 
            // btnConfiguracion
            // 
            this.btnConfiguracion.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnConfiguracion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnConfiguracion.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnConfiguracion.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(156)))), ((int)(((byte)(195)))));
            this.btnConfiguracion.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConfiguracion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnConfiguracion.Icon = ((System.Drawing.Image)(resources.GetObject("btnConfiguracion.Icon")));
            this.btnConfiguracion.IconRatio = 1.2F;
            this.btnConfiguracion.Location = new System.Drawing.Point(1740, 0);
            this.btnConfiguracion.Name = "btnConfiguracion";
            this.btnConfiguracion.Size = new System.Drawing.Size(60, 60);
            this.btnConfiguracion.TabIndex = 77;
            // 
            // btnCerrarSesion
            // 
            this.btnCerrarSesion.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnCerrarSesion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnCerrarSesion.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnCerrarSesion.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(156)))), ((int)(((byte)(195)))));
            this.btnCerrarSesion.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCerrarSesion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnCerrarSesion.Icon = ((System.Drawing.Image)(resources.GetObject("btnCerrarSesion.Icon")));
            this.btnCerrarSesion.IconRatio = 1.2F;
            this.btnCerrarSesion.Location = new System.Drawing.Point(1860, 0);
            this.btnCerrarSesion.Name = "btnCerrarSesion";
            this.btnCerrarSesion.Size = new System.Drawing.Size(60, 60);
            this.btnCerrarSesion.TabIndex = 76;
            this.btnCerrarSesion.Click += new System.EventHandler(this.BtnCerrarSesion_Click);
            // 
            // label15
            // 
            this.label15.Font = new System.Drawing.Font("Inter", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.ForeColor = System.Drawing.Color.White;
            this.label15.Location = new System.Drawing.Point(24, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(264, 60);
            this.label15.TabIndex = 7;
            this.label15.Text = "Ventana principal";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PrincipalForm
            // 
            this.BackColor = System.Drawing.Color.Lavender;
            this.ClientSize = new System.Drawing.Size(1924, 1050);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.btnAgregarTarea);
            this.Controls.Add(this.btnAnterior);
            this.Controls.Add(this.btnSiguiente);
            this.Controls.Add(this.panel7);
            this.Controls.Add(this.panel6);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.flTareas);
            this.Controls.Add(this.tlpCalendario);
            this.Controls.Add(this.lblMesAnio);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "PrincipalForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.panel3.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel7.ResumeLayout(false);
            this.pnlTop.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private AntdUI.Panel panel3;
        private AntdUI.Panel panel1;
        private AntdUI.Panel panel2;
        private AntdUI.Panel panel4;
        private AntdUI.Panel panel5;
        private AntdUI.Panel panel6;
        private AntdUI.Panel panel7;
        private AntdUI.Button btnSiguiente;
        private AntdUI.Button btnAnterior;
        private AntdUI.Button btnAgregarTarea;
        private Panel pnlTop;
        private AntdUI.Button btnCerrarSesion;
        private Label label15;
        private AntdUI.Button btnConfiguracion;
        private AntdUI.Button btnMinimizar;
    }
}