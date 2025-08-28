using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using OfficeOpenXml;
using System.Drawing;
using OfficeOpenXml.Style;
using System.IO;
using System.Linq;

namespace MistCore.Framework.Export
{
    public class ExcelClient
    {
        private string comments;
        private string author;
        private string declaration;

        public ExcelClient(string comments, string author, string declaration)
        {
            this.comments = comments;
            this.author = author;
            this.declaration = declaration;
        }

        public ExcelInfo Export(DataTable dt)
        {
            var temp = new ExcelExportTemplate();

            var prop = new ExcelProperties();
            prop.Title = dt.TableName;
            prop.Comments = this.comments;
            prop.Author = this.author;
            /** 这几个是文件属性，添加可能导致文件打开时提示错误
            prop.Comments = "描述";
            prop.Author = "作者";
            prop.AppVersion = "1.0";
            prop.Application = "应用名称";
            **/
            prop.Created = DateTime.Now;
            prop.Modified = DateTime.Now;

            prop.Mappings.Add("序号", styleCell =>
            {
                styleCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            });

            return temp.Export(dt, prop, declaration);
        }

        public DataTable GetDataTable(Stream stream, string password)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            stream.Position = 0;
            using (ExcelPackage package = new ExcelPackage(stream, password))
            {
                var dataTable = new DataTable();

                var worksheet = package.Workbook.Worksheets.First();

                foreach (var column in worksheet.Columns)
                {
                    dataTable.Columns.Add();
                }

                for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
                {
                    var dataRow = dataTable.NewRow();
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        var cellValue = worksheet.Cells[row, col].Value?.ToString();
                        dataRow[col - 1] = cellValue;
                    }
                    dataTable.Rows.Add(dataRow);
                }

                return dataTable;
            }
        }

        //public List<DataTable> GetDataTable(Stream stream, string password)
        //{
        //    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        //    stream.Position = 0;
        //    using (ExcelPackage package = new ExcelPackage(stream, password))
        //    {
        //        var worksheet = package.Workbook.Worksheets[0];

        //        var dataTables = new List<DataTable>();
        //        foreach (var table in worksheet.Tables)
        //        {
        //            var dataTable = new DataTable(table.Name);

        //            foreach (var column in table.Columns)
        //            {
        //                dataTable.Columns.Add(column.Name);
        //            }

        //            foreach(var row in table.Range..Rows)
        //            {

        //            }

        //            table.Columns

        //            dataTables.Add(dataTable);
        //        }
        //    }
        //}

    }

    internal class ExcelExportTemplate
    {
        private List<string> styles = new List<string>();

        public ExcelInfo Export(DataTable dt, ExcelProperties properties, string declaration = null)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage())
            {
                #region Excel系统属性
                if (properties != null)
                {
                    if (properties.Title != null) package.Workbook.Properties.Title = properties.Title;
                    if (properties.Subject != null) package.Workbook.Properties.Subject = properties.Subject;
                    if (properties.Keywords != null) package.Workbook.Properties.Keywords = properties.Keywords;
                    if (properties.Category != null) package.Workbook.Properties.Category = properties.Category;
                    if (properties.Comments != null) package.Workbook.Properties.Comments = properties.Comments;
                    if (properties.Author != null) package.Workbook.Properties.Author = properties.Author;
                    if (properties.LastModifiedBy != null) package.Workbook.Properties.LastModifiedBy = properties.LastModifiedBy;
                    if (properties.AppVersion != null) package.Workbook.Properties.AppVersion = properties.AppVersion;
                    if (properties.Application != null) package.Workbook.Properties.Application = properties.Application;
                    if (properties.Company != null) package.Workbook.Properties.Company = properties.Company;
                    if (properties.Manager != null) package.Workbook.Properties.Manager = properties.Manager;
                    if (properties.Created != null) package.Workbook.Properties.Created = properties.Created.Value;
                    if (properties.Modified != null) package.Workbook.Properties.Modified = properties.Modified.Value;
                }
                #endregion

                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(dt.TableName);

                int rowIndex = 1;
                var tableRowIndexStart = 1;

                #region 填写申明
                if (!string.IsNullOrEmpty(declaration))
                {
                    worksheet.Cells[rowIndex, 1].Value = declaration;
                    worksheet.Cells[rowIndex, 1].StyleName = GetNamedStyle(worksheet, "readme");
                    rowIndex++;
                    tableRowIndexStart++;
                }
                #endregion

                #region 填写标题
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    worksheet.Cells[rowIndex, i + 1].Value = dt.Columns[i].ColumnName;

                    //switch (dt.Columns[i].DataType?.Name)
                    //{
                    //    case "DateTime":
                    //        worksheet.Columns[i + 1].StyleName = GetNamedStyle(worksheet, $"default_{i}", styleCell =>
                    //        {
                    //            styleCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    //            styleCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    //            styleCell.Style.Numberformat.Format = "yyyy/MM/dd";
                    //        });
                    //        break;
                    //    default:
                    //        worksheet.Columns[i + 1].StyleName = GetNamedStyle(worksheet, $"default_{i}");
                    //        break;
                    //}

                    worksheet.Cells[rowIndex, i + 1].StyleName = GetNamedStyle(worksheet, "head", styleCell =>
                    {
                        styleCell.Style.Font.Bold = true;
                        styleCell.Style.Font.Color.SetColor(Color.White);
                        styleCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        styleCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(100, 100, 100));
                        styleCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        styleCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    });
                }
                worksheet.View.FreezePanes(3, 1);
                #endregion

                #region 填写数据
                foreach (DataRow row in dt.Rows)
                {
                    rowIndex++;
                    for (var i = 0; i < dt.Columns.Count; i++)
                    {
                        #region 设置样式
                        switch (dt.Columns[i].DataType?.Name)
                        {
                            case "DateTime":
                                worksheet.Cells[rowIndex, i + 1].StyleName = GetNamedStyle(worksheet, $"default_{i}", styleCell =>
                                {
                                    styleCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                    styleCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                    styleCell.Style.Numberformat.Format = "yyyy-MM-dd";

                                    if (properties.Mappings.ContainsKey(dt.Columns[i].ColumnName))
                                    {
                                        properties.Mappings[dt.Columns[i].ColumnName](styleCell);
                                    }
                                });
                                break;
                            default:
                                worksheet.Cells[rowIndex, i + 1].StyleName = GetNamedStyle(worksheet, $"default_{i}", styleCell =>
                                {
                                    if (properties.Mappings.ContainsKey(dt.Columns[i].ColumnName))
                                    {
                                        properties.Mappings[dt.Columns[i].ColumnName](styleCell);
                                    }
                                });
                                break;
                        }
                        #endregion

                        #region 设置值
                        switch (row[i].GetType().Name)
                        {
                            case "String":
                                worksheet.Cells[rowIndex, i + 1].Value = row[i].ToString();
                                break;
                            case "Int32":
                                worksheet.Cells[rowIndex, i + 1].Value = (int)row[i];
                                break;
                            case "Decimal":
                                worksheet.Cells[rowIndex, i + 1].Value = Convert.ToDouble((decimal)row[i]);
                                break;
                            case "DateTime":
                                worksheet.Cells[rowIndex, i + 1].Value = (DateTime)row[i];
                                break;
                            case "DBNull":
                                worksheet.Cells[rowIndex, i + 1].Value = "";
                                break;
                            default:
                                worksheet.Cells[rowIndex, i + 1].Value = row[i].ToString();
                                break;
                        }
                        #endregion
                    }
                    //#region 自适应行高
                    //worksheet.Row(rowIndex + 1).CustomHeight = true;
                    //#endregion
                }

                #endregion

                #region 调整样式

                //自动调整列宽
                const double minWidth = 0.00;
                const double maxWidth = 100.00;
                //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns(minWidth, maxWidth);
                worksheet.Cells[$"A{tableRowIndexStart}:{worksheet.Dimension.End.Address}"].AutoFitColumns(minWidth, maxWidth);

                foreach (var column in worksheet.Columns)
                {
                    column.Width += 2;
                }

                //自动换行
                //worksheet.Cells[worksheet.Dimension.Address].Style.WrapText = true;

                //添加筛选
                //worksheet.Cells[worksheet.Dimension.Address].AutoFilter = true;
                worksheet.Cells[$"A{tableRowIndexStart}:{worksheet.Dimension.End.Address}"].AutoFilter = true;

                #endregion

                var result = new ExcelInfo();
                result.Type = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                result.Name = $"{dt.TableName}_${DateTime.Now.ToString("yyyyMMdd_HHmm")}${Guid.NewGuid().ToString("n").Substring(0, 4).ToLower()}.xlsx";
                result.Extend = "xlsx";
                result.Data = package.GetAsByteArray();
                return result;
            }
        }

        private string GetNamedStyle(ExcelWorksheet worksheet, string name = null, Action<OfficeOpenXml.Style.XmlAccess.ExcelNamedStyleXml> action = null)
        {
            if (styles.Contains(name))
            {
                return name;
            }

            var styleCell = worksheet.Workbook.Styles.CreateNamedStyle(name ?? Guid.NewGuid().ToString("n").ToLower());
            styleCell.Style.Font.Name = "宋体";
            styleCell.Style.Font.Size = 10;
            styleCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            styleCell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            styleCell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            styleCell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            styleCell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            styleCell.Style.Border.Top.Color.SetColor(Color.FromArgb(170, 170, 170));
            styleCell.Style.Border.Right.Color.SetColor(Color.FromArgb(170, 170, 170));
            styleCell.Style.Border.Bottom.Color.SetColor(Color.FromArgb(170, 170, 170));
            styleCell.Style.Border.Left.Color.SetColor(Color.FromArgb(170, 170, 170));
            //styleCell.Style.WrapText = true;

            if (action != null)
            {
                action(styleCell);
            }

            styles.Add(name);
            return name;
        }
    }

    internal class ExcelProperties
    {
        public string Category { get; set; }
        public DateTime? Modified { get; set; }
        public string Manager { get; set; }
        public string Company { get; set; }
        public string AppVersion { get; set; }
        public string Application { get; set; }
        public string LastModifiedBy { get; set; }
        public string Keywords { get; set; }
        public string Comments { get; set; }
        public string Author { get; set; }
        public string Subject { get; set; }
        public string Title { get; set; }
        public DateTime? Created { get; set; }

        public Dictionary<string, Action<OfficeOpenXml.Style.XmlAccess.ExcelNamedStyleXml>> Mappings = new Dictionary<string, Action<OfficeOpenXml.Style.XmlAccess.ExcelNamedStyleXml>>();
    }

    public class ExcelInfo
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public string Extend { get; set; }

        public byte[] Data { get; set; }
    }

}
