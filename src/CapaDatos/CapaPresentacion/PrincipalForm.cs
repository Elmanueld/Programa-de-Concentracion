using System;
using System.Drawing;
using System.Windows.Forms;
using CapaEntidades;
using CapaNegocio;

namespace CapaPresentacion
{
    public sealed class PrincipalForm : Form
    {
        private readonly E_Usuario usuario;
        private readonly N_Tarea tareas = new N_Tarea();
        private readonly DataGridView listado = new DataGridView {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
            AllowUserToDeleteRows = false, AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        private readonly TextBox filtro = new TextBox { Width = 180, MaxLength = 70 };

        public PrincipalForm(E_Usuario usuario)
        {
            if (usuario == null || usuario.IdUsuario <= 0) throw new ArgumentException("Se requiere una sesión válida.");
            this.usuario = usuario;
            Text = "Programa de Concentración — " + usuario.Nombre;
            Size = new Size(980, 580); MinimumSize = new Size(850, 450);
            Font = new Font("Segoe UI", 10); StartPosition = FormStartPosition.CenterScreen;
            var barra = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 95, Padding = new Padding(8) };
            barra.Controls.Add(new Label { Text = "Mis tareas", AutoSize = true, Margin = new Padding(4, 8, 10, 0) });
            barra.Controls.Add(filtro);
            Boton(barra, "Buscar", Cargar);
            Boton(barra, "Crear", () => Editar(null));
            Boton(barra, "Editar", () => Editar(Seleccionada()));
            Boton(barra, "Eliminar", Eliminar);
            Boton(barra, "Cerrar sesión", Close);
            Columna("IdTarea", "ID", 35);
            Columna("Nombre", "Tarea", 150);
            Columna("FechaInicio", "Inicio", 100);
            Columna("FechaFin", "Fin", 100);
            Columna("Estado", "Estado (P/A/C)", 75);
            Controls.Add(listado); Controls.Add(barra);
            Shown += (s, e) => OperacionUI.Ejecutar(this, Cargar);
        }

        private void Columna(string propiedad, string titulo, int peso)
        {
            listado.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = propiedad, HeaderText = titulo, FillWeight = peso });
        }

        private void Boton(Control padre, string texto, Action accion)
        {
            var boton = new Button { Text = texto, AutoSize = true };
            boton.Click += (s, e) => OperacionUI.Ejecutar(this, accion);
            padre.Controls.Add(boton);
        }

        private void Cargar() { listado.DataSource = tareas.BuscarTarea(usuario.IdUsuario, filtro.Text); }

        private E_Tarea Seleccionada()
        {
            var tarea = listado.CurrentRow == null ? null : listado.CurrentRow.DataBoundItem as E_Tarea;
            if (tarea == null) throw new ArgumentException("Seleccione una tarea del listado.");
            return tarea;
        }

        private void Editar(E_Tarea tarea)
        {
            if (tarea != null && tarea.Estado == 'A') throw new ArgumentException("Una tarea activa no se puede editar.");
            using (var editor = new TareaForm(usuario.IdUsuario, tarea))
                if (editor.ShowDialog() == DialogResult.OK) Cargar();
        }

        private void Eliminar()
        {
            var tarea = Seleccionada();
            if (MessageBox.Show(this, "¿Eliminar la tarea seleccionada?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            tareas.EliminarTarea(usuario.IdUsuario, tarea.IdTarea);
            Cargar();
        }
    }
}
