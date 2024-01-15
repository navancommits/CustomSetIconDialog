using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.IO;
using Sitecore.Resources;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using System;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Web.UI;

namespace CustomSetIconDialog
{
    public class SetIconForm : DialogForm
    {
        public Scrollbox CompleteList { get; set; }

        private string MapTagName { get; set; }

        protected  Input SearchText { get; set; }

        protected Button SearchButton { get; set; }
        /// <summary>Gets or sets the applications list.</summary>
        /// <value>The applications list.</value>
        protected Scrollbox ApplicationsList { get; set; }

        /// <summary>Gets or sets the apps list.</summary>
        /// <value>The apps list.</value>
        protected Scrollbox AppsList { get; set; }

        /// <summary>Gets or sets the business list.</summary>
        /// <value>The business list.</value>
        protected Scrollbox BusinessList { get; set; }

        /// <summary>Gets or sets the controls list.</summary>
        /// <value>The controls list.</value>
        protected Scrollbox ControlsList { get; set; }

        /// <summary>Gets or sets the core1 list.</summary>
        /// <value>The core1 list.</value>
        protected Scrollbox Core1List { get; set; }

        /// <summary>Gets or sets the core2 list.</summary>
        /// <value>The core2 list.</value>
        protected Scrollbox Core2List { get; set; }

        /// <summary>Gets or sets the core3 list.</summary>
        /// <value>The core3 list.</value>
        protected Scrollbox Core3List { get; set; }

        /// <summary>Gets or sets the database list.</summary>
        /// <value>The database list.</value>
        protected Scrollbox DatabaseList { get; set; }

        /// <summary>Gets or sets the flags list.</summary>
        /// <value>The flags list.</value>
        protected Scrollbox FlagsList { get; set; }

        /// <summary>Gets or sets the icon file.</summary>
        /// <value>The icon file.</value>
        protected Edit IconFile { get; set; }

        /// <summary>Gets or sets the imaging list.</summary>
        /// <value>The imaging list.</value>
        protected Scrollbox ImagingList { get; set; }

        /// <summary>Gets or sets the launchPad icons list.</summary>
        /// <value>The launchPad icons list.</value>
        protected Scrollbox LaunchPadIconsList { get; set; }

        /// <summary>Gets or sets the multimedia list.</summary>
        /// <value>The multimedia list.</value>
        protected Scrollbox MultimediaList { get; set; }

        /// <summary>Gets or sets the network list.</summary>
        /// <value>The network list.</value>
        protected Scrollbox NetworkList { get; set; }

        /// <summary>Gets or sets the office list.</summary>
        /// <value>The office list.</value>
        protected Scrollbox OfficeList { get; set; }

        /// <summary>Gets or sets the office white list.</summary>
        /// <value>The office white list.</value>
        protected Scrollbox OfficeWhiteList { get; set; }

        /// <summary>Gets or sets the other list.</summary>
        /// <value>The other list.</value>
        protected Scrollbox OtherList { get; set; }

        /// <summary>Gets or sets the people list.</summary>
        /// <value>The people list.</value>
        protected Scrollbox PeopleList { get; set; }

        /// <summary>Gets or sets the recent list.</summary>
        /// <value>The recent list.</value>
        protected Scrollbox RecentList { get; set; }

        /// <summary>
        /// Gets recent icons list from registry /Current_User/RecentIcons.
        /// </summary>
        protected virtual ListString RecentIcons => new ListString(Registry.GetString("/Current_User/RecentIcons"));

        /// <summary>Gets or sets the software list.</summary>
        /// <value>The software list.</value>
        protected Scrollbox SoftwareList { get; set; }

        /// <summary>Gets or sets the tab strip.</summary>
        /// <value>The tab strip.</value>
        protected VerticalTabstrip TabStrip { get; set; }

        /// <summary>Gets or sets the word processing list.</summary>
        /// <value>The word processing list.</value>
        protected Scrollbox WordProcessingList { get; set; }

        protected Radiobutton Search { get; set; }

        protected Radiobutton Select { get; set; }

        /// <summary>Raises the load event.</summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        /// <remarks>This method notifies the server control that it should perform actions common to each HTTP
        /// request for the page it is associated with, such as setting up a database query. At this
        /// stage in the page lifecycle, server controls in the hierarchy are created and initialized,
        /// view state is restored, and form controls reflect client-side data. Use the IsPostBack
        /// property to determine whether the page is being loaded in response to a client postback,
        /// or if it is being loaded and accessed for the first time.</remarks>
        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull((object)e, nameof(e));
            base.OnLoad(e);
            this.TabStrip.OnChange += new EventHandler(this.TabStrip_OnChange);
            this.Select.OnClick += new EventHandler(this.Select_OnClick);
            this.Search.OnClick += new EventHandler(this.Search_OnClick);
            this.SearchButton.OnClick += new EventHandler(this.SearchButton_OnClick);
            if (Context.ClientPage.IsEvent)
                return;
            Item itemFromQueryString = UIUtil.GetItemFromQueryString(Database.GetDatabase(WebUtil.GetQueryString("sc_content", Context.ContentDatabase.Name)));
            Assert.IsNotNull((object)itemFromQueryString, typeof(Item));
            string queryString = WebUtil.GetQueryString("fld_id");
            if (string.IsNullOrEmpty(queryString))
                this.IconFile.Value = itemFromQueryString.Appearance.Icon;
            else
                this.IconFile.Value = itemFromQueryString.Fields[ID.Parse(queryString)].Value;

            //if Complete.zip doesn't exist in themes\standard folder, generate it
            //var filepath = FileUtil.MapPath("/sitecore/shell/themes/standard/Complete.zip");
            //if (!File.Exists(filepath)) this.GenerateCompleteZipFile();

            this.RenderIcons();            
           
            this.RecentList.InnerHtml = this.RenderRecentIcons();
        }

        /// <summary>Handles a click on the OK button.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        /// <remarks>When the user clicks OK, the dialog is closed by calling
        /// the <see cref="M:Sitecore.Web.UI.Sheer.ClientResponse.CloseWindow">CloseWindow</see> method.</remarks>
        protected override void OnOK(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull((object)args, nameof(args));
            SheerResponse.SetDialogValue(this.IconFile.Value);
            base.OnOK(sender, args);
        }

        /// <summary>Handles the OnChange event of the TabStrip control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        protected void TabStrip_OnChange(object sender, EventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull((object)e, nameof(e));
            SheerResponse.Eval("scUpdateControls();");
        }

        protected void Search_OnClick(object sender, EventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull((object)e, nameof(e));
            //SheerResponse.Eval("scUpdateControls();");            
        }

        protected void SearchButton_OnClick(object sender, EventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));            
            Assert.ArgumentNotNull((object)e, nameof(e));

            try
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

                var setIconForm = new SetSearchedIcon
                {
                    MapTagName = "Complete",
                    SearchValue = SearchText.Value,
                    CompleteList = CompleteList
                };

                setIconForm.RenderIcons();
            }
            finally
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            }
            
            //SheerResponse.Eval("scUpdateControls();");
        }

        protected void Select_OnClick(object sender, EventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull((object)e, nameof(e));
            //SheerResponse.Eval("scUpdateControls();");
        }

        /// <summary>Draws the icons.</summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="img">The img.</param>
        /// <param name="area">The area.</param>
        private static void DrawIcons(string prefix, string img, string area)
        {
            string[] files = SetIconForm.GetFiles(prefix);
            int num1 = files.Length;
            if (num1 == 0)
                num1 = 1;
            int height = (num1 / 24 + (num1 % 24 == 0 ? 0 : 1)) * 40;
            using (Bitmap bitmap1 = new Bitmap(960, height, PixelFormat.Format32bppArgb))
            {
                if (prefix == "OfficeWhite")
                    Graphics.FromImage((System.Drawing.Image)bitmap1).FillRectangle(Brushes.DarkGray, 0, 0, 960, height);
                HtmlTextWriter htmlTextWriter = new HtmlTextWriter((TextWriter)new StringWriter());
                htmlTextWriter.WriteLine("<map name=\"" + prefix + "\">");
                string relativeIconPath = prefix + "/32x32/";
                int num2 = 0;
                using (Graphics graphics = Graphics.FromImage((System.Drawing.Image)bitmap1))
                {
                    foreach (string path in files)
                    {
                        int num3 = num2 % 24;
                        int num4 = num2 / 24;
                        string themedImageSource = Images.GetThemedImageSource(relativeIconPath + path, ImageDimension.id32x32);
                        try
                        {
                            using (Bitmap bitmap2 = Settings.Icons.UseZippedIcons ? new Bitmap(ZippedIcon.GetStream(relativeIconPath + path, ZippedIcon.GetZipFile(relativeIconPath))) : new Bitmap(FileUtil.MapPath(themedImageSource)))
                                graphics.DrawImage((System.Drawing.Image)bitmap2, num3 * 40 + 4, num4 * 40 + 4, 32, 32);
                            string str1 = string.Format("{0},{1},{2},{3}", (object)(num3 * 40 + 4), (object)(num4 * 40 + 4), (object)(num3 * 40 + 36), (object)(num4 * 40 + 36));
                            string str2 = StringUtil.Capitalize(Path.GetFileNameWithoutExtension(path).Replace("_", " "));
                            htmlTextWriter.WriteLine("<area shape=\"rect\" coords=\"{0}\" href=\"#\" alt=\"{1}\" sc_path=\"{2}\"/>", (object)str1, (object)str2, (object)(relativeIconPath + path));
                            ++num2;
                        }
                        catch (Exception ex)
                        {
                            Log.Warn("Unable to open icon " + themedImageSource, ex, (object)typeof(SetIconForm));
                        }
                    }
                }
                htmlTextWriter.WriteLine("</map>");
                FileUtil.WriteToFile(area, htmlTextWriter.InnerWriter.ToString());
                bitmap1.Save(img, ImageFormat.Png);
            }
        }

        /// <summary>Gets the filename.</summary>
        /// <param name="prefix">The prefix.</param>
        /// <returns>Returns the filename.</returns>
        private static string GetFilename(string prefix) => FileUtil.MapPath(FileUtil.MakePath(TempFolder.Folder, "icons_" + prefix + ".png"));

        /// <summary>Gets the files.</summary>
        /// <param name="prefix">The prefix.</param>
        /// <returns>The files.</returns>
        private static string[] GetFiles(string prefix)
        {
            Assert.ArgumentNotNullOrEmpty(prefix, nameof(prefix));
            return !Settings.Icons.UseZippedIcons ? SetIconForm.GetFolderFiles(prefix) : SetIconForm.GetZippedFiles(prefix);
        }

        /// <summary>Gets the files.</summary>
        /// <param name="prefix">The prefix.</param>
        /// <returns>Returns the files.</returns>
        private static string[] GetFolderFiles(string prefix)
        {
            string[] files = Directory.GetFiles(FileUtil.MapPath("/sitecore/shell/themes/standard/" + prefix + "/32x32"));
            for (int index = 0; index < files.Length; ++index)
                files[index] = Path.GetFileName(files[index]);
            return files;
        }

        /// <summary>Gets the zipped filed.</summary>
        /// <param name="prefix">The prefix.</param>
        /// <returns>Returns the zipped filed.</returns>
        private static string[] GetZippedFiles(string prefix) => ZippedIcon.GetFiles(prefix, "/sitecore/shell/themes/standard/" + prefix + ".zip");

        /// <summary>Renders the icons.</summary>
        /// <param name="scrollbox">The scrollbox.</param>
        /// <param name="prefix">The prefix.</param>
        private static void RenderIcons(Scrollbox scrollbox, string prefix)
        {
            Assert.ArgumentNotNull((object)scrollbox, nameof(scrollbox));
            Assert.ArgumentNotNullOrEmpty(prefix, nameof(prefix));
            string filename = SetIconForm.GetFilename(prefix);
            string str = Path.ChangeExtension(filename, ".html");
            if (!File.Exists(filename) || !File.Exists(str))
                SetIconForm.DrawIcons(prefix, filename, str);
            HtmlTextWriter output = new HtmlTextWriter((TextWriter)new StringWriter());
            output.Write(FileUtil.ReadFromFile(str));
            SetIconForm.WriteImageTag(filename, prefix, output);
            scrollbox.InnerHtml = output.InnerWriter.ToString();
        }

        /// <summary>Renders the icons.</summary>
        private void RenderIcons()
        {
            SetIconForm.RenderIcons(this.ApplicationsList, "Applications");
            SetIconForm.RenderIcons(this.AppsList, "Apps");
            SetIconForm.RenderIcons(this.BusinessList, "Business");
            SetIconForm.RenderIcons(this.ControlsList, "Control");
            SetIconForm.RenderIcons(this.Core1List, "Core");
            SetIconForm.RenderIcons(this.Core2List, "Core2");
            SetIconForm.RenderIcons(this.Core3List, "Core3");
            SetIconForm.RenderIcons(this.DatabaseList, "Database");
            SetIconForm.RenderIcons(this.FlagsList, "Flags");
            SetIconForm.RenderIcons(this.ImagingList, "Imaging");
            SetIconForm.RenderIcons(this.LaunchPadIconsList, "LaunchPadIcons");
            SetIconForm.RenderIcons(this.MultimediaList, "Multimedia");
            SetIconForm.RenderIcons(this.NetworkList, "Network");
            SetIconForm.RenderIcons(this.OfficeList, "Office");
            SetIconForm.RenderIcons(this.OfficeWhiteList, "OfficeWhite");
            SetIconForm.RenderIcons(this.OtherList, "Other");
            SetIconForm.RenderIcons(this.PeopleList, "People");
            SetIconForm.RenderIcons(this.SoftwareList, "Software");
            SetIconForm.RenderIcons(this.WordProcessingList, "WordProcessing");
        }

        private void RenderAllIcons(string searchValue="")
        {
            //SetIconForm.RenderIcons(this.CompleteList, "Complete", searchValue);
        }

        /// <summary>Renders the recent icons.</summary>
        protected string RenderRecentIcons()
        {
            int num = 0;
            ListString recentIcons = this.RecentIcons;
            HtmlTextWriter htmlTextWriter = new HtmlTextWriter((TextWriter)new StringWriter());
            ImageBuilder imageBuilder = new ImageBuilder()
            {
                Width = 32,
                Height = 32
            };
            foreach (string str in recentIcons)
            {
                imageBuilder.Src = Images.GetThemedImageSource(str, ImageDimension.id32x32);
                imageBuilder.Alt = StringUtil.Capitalize(FileUtil.GetFileNameWithoutExtension(str).Replace("_", " "));
                imageBuilder.Attributes.Set("sc_path", str);
                imageBuilder.Class = "scRecentIcon";
                htmlTextWriter.Write(imageBuilder.ToString());
                ++num;
            }
            if (num == 0)
            {
                htmlTextWriter.Write("<div align=\"center\" style=\"padding:32px 0px 0px 0px\"><i>");
                htmlTextWriter.Write(Translate.Text("There are no icons to display."));
                htmlTextWriter.Write("</i></div>");
            }
            return htmlTextWriter.InnerWriter.ToString();
        }

        /// <summary>Writes the image tag.</summary>
        /// <param name="img">The img.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="output">The output.</param>
        private static void WriteImageTag(string img, string prefix, HtmlTextWriter output)
        {
            Assert.ArgumentNotNull((object)img, nameof(img));
            Assert.ArgumentNotNull((object)prefix, nameof(prefix));
            Assert.ArgumentNotNull((object)output, nameof(output));
            ImageBuilder imageBuilder;
            using (Bitmap bitmap = new Bitmap(img))
                imageBuilder = new ImageBuilder()
                {
                    Src = FileUtil.UnmapPath(img),
                    Width = bitmap.Width,
                    Height = bitmap.Height,
                    Usemap = "#" + prefix
                };
            output.WriteLine(imageBuilder.ToString());
        }
    }
}
