/*
 * Created by SharpDevelop.
 * User: W110
 * Date: 15/12/2013
 * Time: 8:21 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text;
using Spire.Pdf;

namespace Tesseract.WebDemo
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public class DefaultPage : System.Web.UI.Page
	{	
		#region Data

        // input panel controls

        protected Panel inputPanel;
		protected HtmlInputFile imageFile;
		protected HtmlButton submitFile;

        // result panel controls
        protected Panel resultPanel;
        protected HtmlGenericControl meanConfidenceLabel;
        protected HtmlTextArea resultText;
        protected HtmlButton restartButton;


        #endregion

        #region Event Handlers 

        private void OnSubmitFileClicked(object sender, EventArgs args)
        {
            meanConfidenceLabel.InnerText = "";
            resultText.InnerText = "";

            if (imageFile.PostedFile != null && imageFile.PostedFile.ContentLength > 0)
            {
                // for now just fail hard if there's any error however in a propper app I would expect a full demo.
                string filePath = Server.MapPath("~/UploadedFiles/") + imageFile.PostedFile.FileName;
                imageFile.PostedFile.SaveAs(filePath);


                var documentText = new StringBuilder();
                using (var engine = new TesseractEngine(Server.MapPath(@"~/tessdata"), "eng", EngineMode.Default))
                {
                    PdfDocument document = new PdfDocument();
                    document.LoadFromFile(filePath);

                    // Convert each page to TIFF
                    for (int i = 0; i < document.Pages.Count; i++)
                    {
                        string tiffFilePath = Server.MapPath("~/UploadedFiles/") + $"output_page_{i + 1}.tiff";
                        // Save each page as a TIFF file
                        System.Drawing.Image thisPage = document.SaveAsImage(i);
                        thisPage.Save(tiffFilePath);
                        // Perform OCR
                        using (Pix img = Pix.LoadFromFile(tiffFilePath))
                        {
                            using (Page recognizedPage = engine.Process(img))
                            {

                                meanConfidenceLabel.InnerText = meanConfidenceLabel.InnerText +  String.Format("{0:P}", recognizedPage.GetMeanConfidence());
                                resultText.InnerText = resultText.InnerText  +  recognizedPage.GetText();
                            }
                        }

                    }

         

                   
                    inputPanel.Visible = false;
                    resultPanel.Visible = true;
                }
            }
        }

        private void OnRestartClicked(object sender, EventArgs args)
        {
            resultPanel.Visible = false;
            inputPanel.Visible = true;
        }

		#endregion

		#region Page Setup
		protected override void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
		}

		//----------------------------------------------------------------------
		private void InitializeComponent()
		{
            this.restartButton.ServerClick += OnRestartClicked;
			this.submitFile.ServerClick += OnSubmitFileClicked;
		}

		#endregion
	}
}