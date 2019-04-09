using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FF8
{
    public partial class MakiDebugger : Form
    {
        //Maki debugger is a dynamic form that let's you edit FREAKING EVERY SINGLE field that is static, private, whatever
        //it's like an Cheat Engine that has pre-made table of ALL variables of n-class available for edit and debug
        //so have fun- I'm really proud of this one. Minimalistic code, extreme possibilities
        public MakiDebugger()
        {
            InitializeComponent();
        }

        private System.Reflection.FieldInfo lastObject; 

        public void UpdateWindow()
        {
            foreach(var c in 
            MakiExtended.DebuggerFood)
            {
                //int controller = FindControllerId(c.DeclaringType.FullName);
                //if (controller == -1)
                //{
                //    listBox1.Items.Add(new ListViewItem(c.DeclaringType.FullName));
                //    controller = listBox1.Items.Count - 1;
                //}
                listBox1.Items.Add($"{MakiExtended.DebuggerFood.IndexOf(c)}:{c.ReflectedType.FullName}:{c.Name}");
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0)
                return;
            string[] parentDividor = (listBox1.SelectedItem as string).Split(':');
            var field = MakiExtended.DebuggerFood[int.Parse(parentDividor[0])];
            TypeBeautizer(field);

        }

        private void TypeBeautizer(System.Reflection.FieldInfo field)
        {
            flowLayoutPanel1.Controls.Clear();
            lastObject = field;
            object value = field.GetValue(field);
            Type tp = value.GetType();
            if(tp == typeof(int))
            {
                flowLayoutPanel1.Controls.Add(new NumericUpDown() { Minimum = int.MinValue, Maximum = int.MaxValue, Value=(int)value });
                (flowLayoutPanel1.Controls[0] as NumericUpDown).ValueChanged += @int;
                return;
            }
            if (tp == typeof(uint))
            {
                flowLayoutPanel1.Controls.Add(new NumericUpDown() { Minimum = uint.MinValue, Maximum = uint.MaxValue, Value = (uint)value });
                (flowLayoutPanel1.Controls[0] as NumericUpDown).ValueChanged += @uint;
                return;
            }
            if (tp == typeof(Vector3))
            {
                flowLayoutPanel1.Controls.Add(new TextBox() { Text = ((Vector3)value).X.ToString() });
                flowLayoutPanel1.Controls.Add(new TextBox() { Text = ((Vector3)value).Y.ToString() });
                flowLayoutPanel1.Controls.Add(new TextBox() { Text = ((Vector3)value).Z.ToString() });
                (flowLayoutPanel1.Controls[0] as TextBox).TextChanged += @vectorX;
                (flowLayoutPanel1.Controls[1] as TextBox).TextChanged += @vectorY;
                (flowLayoutPanel1.Controls[2] as TextBox).TextChanged += @vectorZ;
                return;
            }
            if(tp==typeof(Matrix))
            {
                DataGridView dgv = new DataGridView();
                dgv.Columns.Add("M1", "M1");
                dgv.Columns.Add("M2", "M2");
                dgv.Columns.Add("M3", "M3");
                dgv.Columns.Add("M4", "M4");
                Matrix mx = (Matrix)value;
                dgv.Rows.Add(mx.M11, mx.M12, mx.M13, mx.M14);
                dgv.Rows.Add(mx.M21, mx.M22, mx.M23, mx.M24);
                dgv.Rows.Add(mx.M31, mx.M32, mx.M33, mx.M34);
                dgv.Rows.Add(mx.M41, mx.M42, mx.M43, mx.M44);
                dgv.AutoSize = true;
                flowLayoutPanel1.Controls.Add(dgv);
                (flowLayoutPanel1.Controls[0] as DataGridView).CellValueChanged += @Matrix;
                return;
            }
            flowLayoutPanel1.Controls.Add(new Label() { Text = $"UNKNOWN. Type: {tp}", AutoSize=true});
        }

        //Matrix
        private void @Matrix(object sender, DataGridViewCellEventArgs e)
        {
            var dgv = (sender as DataGridView);
            Matrix mm = (Matrix)lastObject.GetValue(lastObject);
            try
            {
                mm = new Matrix(
                    (float)dgv.Rows[0].Cells[0].Value, (float)dgv.Rows[0].Cells[1].Value, (float)dgv.Rows[0].Cells[2].Value, (float)dgv.Rows[0].Cells[3].Value,
                    (float)dgv.Rows[1].Cells[0].Value, (float)dgv.Rows[1].Cells[1].Value, (float)dgv.Rows[1].Cells[2].Value, (float)dgv.Rows[1].Cells[3].Value,
                    (float)dgv.Rows[2].Cells[0].Value, (float)dgv.Rows[2].Cells[1].Value, (float)dgv.Rows[2].Cells[2].Value, (float)dgv.Rows[2].Cells[3].Value,
                (float)dgv.Rows[3].Cells[0].Value, (float)dgv.Rows[3].Cells[1].Value, (float)dgv.Rows[3].Cells[2].Value, (float)dgv.Rows[3].Cells[3].Value);
            }
            catch
            {
                ;
            }
            lastObject.SetValue(lastObject, mm);
        }

        //32 bit integer
        private void @int(object sender, EventArgs e) => lastObject.SetValue(lastObject, (int)(sender as NumericUpDown).Value);
        private void @uint(object sender, EventArgs e) => lastObject.SetValue(lastObject, (uint)(sender as NumericUpDown).Value);

        //vector 3x float
        private void @vectorX(object sender, EventArgs e) => lastObject.SetValue(lastObject,
            new Vector3(float.Parse((sender as TextBox).Text),
                ((Vector3)lastObject.GetValue(lastObject)).Y,
                ((Vector3)lastObject.GetValue(lastObject)).Z));
        private void @vectorY(object sender, EventArgs e) => lastObject.SetValue(lastObject,
            new Vector3(((Vector3)lastObject.GetValue(lastObject)).X,
                float.Parse((sender as TextBox).Text),
                ((Vector3)lastObject.GetValue(lastObject)).Z));
        private void @vectorZ(object sender, EventArgs e) => lastObject.SetValue(lastObject,
            new Vector3(((Vector3)lastObject.GetValue(lastObject)).X,
                ((Vector3)lastObject.GetValue(lastObject)).Y,
                float.Parse((sender as TextBox).Text)));



        //private int FindControllerId(string d)
        //{
        //    if (listView1.Items.Count == 0)
        //        return -1;
        //    for (int i = 0; i < listView1.Items.Count; i++)
        //        if (listView1.Items[i].Name == d)
        //            return i;
        //    return -1;
        //}
    }
}
