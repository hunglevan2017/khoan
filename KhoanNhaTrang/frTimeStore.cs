using Dapper;
using KhoanNhaTrang.Model;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KhoanNhaTrang
{
    public partial class frTimeStore : Form
    {
        Config config = new Config();

        public frTimeStore()
        {
            InitializeComponent();

            string query = @"select * from config order by id desc limit 1";
            config = db.Query<Config>(query, config).Single();
            nUI.Value = config.time_update_ui;
            nDb.Value = config.time_store_db;
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }


        static string connStr = ConfigurationManager.ConnectionStrings["connStr"].ConnectionString;
        public static IDbConnection db = new MySqlConnection(connStr);

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                db.Open();
                var param = new DynamicParameters();
                param.Add("id", config.id);
                param.Add("time_update_ui", nUI.Value);
                param.Add("time_store_db", nDb.Value);
                String query = "update config set time_update_ui = @time_update_ui,time_store_db=@time_store_db where Id = @id";
                db.Execute(query, param);
                

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                try
                {
                    db.Close();
                }
                catch (Exception ex)
                {
                }
            }

        }
    }
}
