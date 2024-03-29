﻿using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Table;
using System.Data;
using System.IO;


namespace ExportExcelToTemplateEpplus
{
    public class TemplateExcel2
    {
        public static void FillReport(FileStream fs, string templatefilename, int count, string pathImage, DataSet data)
        {
            FillReport(fs, templatefilename, data, count, pathImage, new string[] { "%", "%" });
        }

        public static void FillReport(FileStream fs, string templatefilename, DataSet data, int count, string pathImage, string[] deliminator)
        {
            using (var temp = new FileStream(templatefilename, FileMode.Open))
            {
                using (var xls = new ExcelPackage(fs, temp))
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    foreach (var n in xls.Workbook.Names)
                    {
                        n.Worksheet.InsertRow(19, count);
                        FillWorksheetData(data, n.Worksheet, n, deliminator);
                    }
                       
                    foreach (var ws in xls.Workbook.Worksheets)
                    {
                        foreach (var n in ws.Names)
                        {
                            FillWorksheetData(data, ws, n, deliminator);
                        }
                    }
                     
                    foreach (var ws in xls.Workbook.Worksheets)
                    {
                        foreach (var c in ws.Cells)
                        {
                            var s = "" + c.Value;
                            if (s.StartsWith(deliminator[0]) == false &&
                                s.EndsWith(deliminator[1]) == false)
                                continue;
                            s = s.Replace(deliminator[0], "").Replace(deliminator[1], "");
                            var ss = s.Split('.');
                            try
                            {
                                if (ss[0] == "chart")
                                {
                                    c.Value = data.Tables[ss[0]].Rows[0][ss[1]];
                                    ExcelPicture excelImage = null;
                                    excelImage = ws.Drawings.AddPicture("Debopam Pal", pathImage);
                                    excelImage.From.Column = 0;
                                    excelImage.From.Row = c.Start.Row - 1;
                                    excelImage.SetSize(170, 120);
                                    // 2x2 px space for better alignment
                                    excelImage.From.ColumnOff = 2 * 9525;
                                    excelImage.From.RowOff = 2 * 9525;
                                } else
                                {
                                    c.Value = data.Tables[ss[0]].Rows[0][ss[1]];

                                }
                            }
                            catch { }
                        }
                    }

                    xls.Save();
                }
            }
        }

        private static void FillWorksheetData(DataSet data, ExcelWorksheet ws, ExcelNamedRange n, string[] deliminator)
        {          
            if (data.Tables.Contains(n.Name) == false)
                return;

            var dt = data.Tables[n.Name];

            int row = n.Start.Row;

            var cn = new string[n.Columns];
            var st = new int[n.Columns];
            for (int i = 0; i < n.Columns; i++)
            {
                cn[i] = (n.Value as object[,])[0, i].ToString().Replace(deliminator[0], "").Replace(deliminator[1], "");
                if (cn[i].Contains("."))
                    cn[i] = cn[i].Split('.')[1];
                st[i] = ws.Cells[row, n.Start.Column + i].StyleID;
            }

            foreach (DataRow r in dt.Rows)
            {
                for (int col = 0; col < n.Columns; col++)
                {
                    if (dt.Columns.Contains(cn[col]))
                        ws.Cells[row, n.Start.Column + col].Value = r[cn[col]]; 
                    ws.Cells[row, n.Start.Column + col].StyleID = st[col]; 
                }
                row++;
            }

            // extend table formatting range to all rows
            foreach (var t in ws.Tables)
            {
                var a = t.Address;
                if (n.Start.Row.Between2(a.Start.Row, a.End.Row) &&
                    n.Start.Column.Between2(a.Start.Column, a.End.Column))
                {
                    ExtendRows(t, dt.Rows.Count - 1);
                }
                   
            }
        }
        public static void ExtendRows(ExcelTable excelTable, int count)
        {
            
            var ad = new ExcelAddress(excelTable.Address.Start.Row,
                                      excelTable.Address.Start.Column,
                                      excelTable.Address.End.Row + count,
                                      excelTable.Address.End.Column);
            //Address = ad;
        }
    }

    public static class int_between2
    {
        public static bool Between2(this int v, int a, int b)
        {
            return v >= a && v <= b;
        }
    }
}

