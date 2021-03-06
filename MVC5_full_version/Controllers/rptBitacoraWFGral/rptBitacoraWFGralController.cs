﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Reporting.WebForms;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
//using ReportViewerForMvc.Example.Reports;

namespace MVC5_full_version.Controllers.reportesBitacoraWF
{
    public class rptBitacoraWFGralController : Controller
    {

      private DataSet.DataSetReportes tds = new DataSet.DataSetReportes();
        public string thisConnectionString = ClickFactura_Entidades.BD.Entidades.AccesoBD.CadenaConexion;//  System.Configuration.ConfigurationManager.ConnectionStrings["Desarrollo_CF"].ConnectionString;
        // GET: rptBitacoraWFGral
        public ActionResult Index()
        {
            SetLocalReport();
            return View();
        }

          public ActionResult AnonymousExample()
        {
            return View();
        }

        /// <summary>
        /// Creates a ReportViewer control and stores it on the ViewBag
        /// </summary>
        /// <returns></returns>
        public ActionResult ServerReportExample()
        {
            ReportViewer reportViewer = new ReportViewer();
            reportViewer.ProcessingMode = ProcessingMode.Remote;
            reportViewer.SizeToReportContent = true;
            reportViewer.Width = Unit.Percentage(100);
            reportViewer.Height = Unit.Percentage(100);

            reportViewer.ServerReport.ReportPath = "/AdventureWorks 2012/Sales_by_Region";
            reportViewer.ServerReport.ReportServerUrl = new Uri("http://localhost/ReportServer/");
            reportViewer.ServerReport.SetParameters(GetParametersServer());

            ViewBag.ReportViewer = reportViewer;

            return View();
        }

        /// <summary>
        /// Creates a ReportViewer control and stores it on the ViewBag
        /// </summary>
        /// <returns></returns>
        public ActionResult LocalReportExample()
        {
            SetLocalReport();

            return View();
        }

        private void SetLocalReport()
        {
            ReportViewer reportViewer = new ReportViewer();
            reportViewer.ProcessingMode = ProcessingMode.Local;
            reportViewer.SizeToReportContent = true;
            reportViewer.Width = Unit.Percentage(100);
            reportViewer.Height = Unit.Percentage(100);

            FillDataSet();
            string RptPath = Server.MapPath("~/DataSet/Reportes/BitacorasWF/rptBitacoraWFGral.rdlc");
            reportViewer.LocalReport.ReportPath = RptPath;// Request.MapPath(Request.ApplicationPath) + @"Reports\LocalReportExample.rdlc";
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("DataSetReportes", tds.Tables[0]));
            //reportViewer.LocalReport.SetParameters(GetParametersLocal());

            ViewBag.ReportViewer = reportViewer;
        }

        private void FillDataSet()
        {
            string connectionString = GetConnectionString();

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                string queryString = GetQueryString();

                SqlDataAdapter sqlDataAapter = new SqlDataAdapter(queryString, sqlConnection);

                sqlDataAapter.Fill(tds, tds.view_todoWorkflow.TableName);
            }
        }

        private string GetConnectionString()
        {
            return thisConnectionString;// "Data Source=localhost;Initial Catalog=AdventureWorks2012;Integrated Security=True";
        }

        private string GetQueryString()
        {
            string query = System.Web.HttpContext.Current.Session["queryrptBitacoraWFGral"] as string;
            return query;// "Select * from view_todoWorkflow";
        }

        private ReportParameter[] GetParametersLocal()
        {
            ReportParameter p1 = new ReportParameter("ReportTitle", "Local Report Example");
            return new ReportParameter[] { p1 };
        }

        private ReportParameter[] GetParametersServer()
        {
            ReportParameter p1 = new ReportParameter("ShowBingMaps", "Visible");
            ReportParameter p2 = new ReportParameter("ShowAll", "True");
            return new ReportParameter[] { p1, p2 };
        }


    }
}