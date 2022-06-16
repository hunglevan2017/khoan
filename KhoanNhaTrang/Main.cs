using Dapper;
using KhoanNhaTrang.Model;
using MySqlConnector;
using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace KhoanNhaTrang
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //Dapper 
        //https://hanhtranglaptrinh.net/dapper-c-la-gi-micro-orm-trong-net/
        static string connStr = ConfigurationManager.ConnectionStrings["connStr"].ConnectionString;
        static IDbConnection db = new MySqlConnection(connStr);

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                db.Open();
                var data = new Data();
                data.flow_rate = 30f;
                data.fluid = 25f;
                string query = @"insert into data(flow_rate, fluid) values(@flow_rate, @fluid);
                            SELECT LAST_INSERT_ID()";
                long id = db.Query<int>(query, data).Single();
                data.Id = id;
                textBox1.Text = "ID:" + data.Id;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                try {
                    db.Close();
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
