using System.Data;
using System.Windows.Forms;
using System.Data;
using System.Windows.Forms;
using System.Drawing;

namespace OrderbookVisualiser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            var symbols = PublisherUtilities.getList("exchanges");
            listBox1.Items.AddRange(symbols);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string prefix = listBox1.SelectedItem.ToString();
            var exchanges = PublisherUtilities.getList(prefix);

            DataTable table = new DataTable("NewTable");
            Console.WriteLine("Original table name: " + table.TableName);
            DataColumn column = new DataColumn("Name", typeof(System.String));
            table.Columns.Add(column);

            column = new DataColumn("Bid Max", typeof(System.String));
            table.Columns.Add(column);

            column = new DataColumn("Sell Min", typeof(System.String));
            table.Columns.Add(column);

            double max = 0, min = 9999999;
            DataRow row;
            foreach (var item in exchanges)
            {
                string[] data = PublisherUtilities.get(item + prefix).ToString().Split(",");

                row = table.NewRow();
                row.ItemArray = new object[] { item, data[0], data[1] };
                table.Rows.Add(row);
                if (double.Parse(data[0]) > max)
                {
                    max = double.Parse(data[0]);
                }
                if (double.Parse(data[1]) < min)
                {
                    min = double.Parse(data[1]);
                }
            }
            row = table.NewRow();
            row.ItemArray = new object[] { "MAXMIN", max.ToString("0.#########"), min.ToString("0.#########") };
            table.Rows.Add(row);
            dataGridView1.DataSource = table;
            dataGridView1.Update();
        }

        private void splitContainer2_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            Console.WriteLine("rawr");
        }
    }
}
