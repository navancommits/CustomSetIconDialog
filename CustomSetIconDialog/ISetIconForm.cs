using Sitecore.Configuration;
using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Resources;
using Sitecore.Web.UI;
using Sitecore.Web.UI.HtmlControls;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web.UI;

namespace CustomSetIconDialog
{
    public abstract class ISetIconForm
    {
        public abstract void RenderIcons();
        public abstract void RenderIcons(Scrollbox scrollbox, string prefix);
        public abstract void DrawIcons(string prefix, string img, string area);

        public string GetFilename(string prefix) => FileUtil.MapPath(FileUtil.MakePath(TempFolder.Folder, "icons_" + prefix + ".png"));

        public string[] GetFiles(string prefix)
        {
            Assert.ArgumentNotNullOrEmpty(prefix, nameof(prefix));
            return !Settings.Icons.UseZippedIcons ? GetFolderFiles(prefix) : GetZippedFiles(prefix);
        }

        /// <summary>Gets the files.</summary>
        /// <param name="prefix">The icon category.</param>
        /// <returns>Returns the files within the category folder in file system.</returns>
        public string[] GetFolderFiles(string prefix)
        {
            string[] files = Directory.GetFiles(FileUtil.MapPath("/sitecore/shell/themes/standard/" + prefix + "/32x32"));
            for (int index = 0; index < files.Length; ++index)
                files[index] = Path.GetFileName(files[index]);

            return files;
        }

        /// <summary>Gets the zipped filed.</summary>
        /// <param name="prefix">The prefix.</param>
        /// <returns>Returns the zipped filed.</returns>
        public string[] GetZippedFiles(string prefix) => ZippedIcon.GetFiles(prefix, "/sitecore/shell/themes/standard/" + prefix + ".zip");

        public void WriteImageTag(string img, string prefix, HtmlTextWriter output)
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

    public class SetSearchedIcon : ISetIconForm
    {
        public Scrollbox CompleteList { get; set; }

        public string HtmlString { get; set; }

        public string SearchValue { get; set; }

        public string MapTagName { get; set; }

        //useful in calculating height of whole bitmap/png stored in file system
        private int CalculateBitmapHeight()
        {
            string[] categories = { "Applications", "Apps", "Business", "Control", "Core", "Core2", "Core3", "Database", "Flags", "Imaging", "Multimedia", "Network", "Office", "OfficeWhite", "Other", "People", "Software", "WordProcessing" };

            int fileLength = 0;
            foreach (var category in categories)
            {
                string[] files = GetFiles(category);
                fileLength += files.Length;
            }

            return fileLength;
        }

        private int GetFileLengthforCategory(string category) => GetFiles(category).Length;

        private void FillOfficeWhiteBgGraphics(int x, int y, Bitmap bitmapparam,Brush brush)
        {
            int filelength = GetFileLengthforCategory("OfficeWhite");
            int height = (filelength / 24 + (filelength % 24 == 0 ? 0 : 1)) * 40;

            Graphics.FromImage((System.Drawing.Image)bitmapparam).FillRectangle(brush, 0, 0, 960, height);
        }


        public override void DrawIcons(string prefix, string img, string area)
        {
            string[] categories = { "Applications", "Apps", "Business", "Control", "Core", "Core2", "Core3", "Database", "Flags", "Imaging", "Multimedia", "Network", "Office", "OfficeWhite", "Other", "People", "Software", "WordProcessing" };

            int num1 = CalculateBitmapHeight();
            int startcoord = 0;//starting coords for the big bitmap - named as num2 in original code

            int height = (num1 / 24 + (num1 % 24 == 0 ? 0 : 1)) * 40;//height of cumulative bitmap calculated here

            //bitmap1 is the large png file system file
            using (Bitmap bitmap1 = new Bitmap(960, height, PixelFormat.Format32bppArgb))//generating the whole bitmap
            {
                HtmlTextWriter htmlTextWriter = new HtmlTextWriter((TextWriter)new StringWriter());
                htmlTextWriter.WriteLine("<map name=\"" + prefix + "\">");

                using (Graphics graphics = Graphics.FromImage((System.Drawing.Image)bitmap1))
                {
                    foreach (var category in categories)
                    {             

                        //if (category == "OfficeWhite")
                        //    FillOfficeWhiteBgGraphics((num3 * 40 + 4), (num3 * 40 + 36), bitmap1, Brushes.DarkGray);

                        string relativeIconPath = category + "/32x32/";

                        string[] files = GetFiles(category);
                        foreach (string path in files)
                        {                            
                            string themedImageSource = Images.GetThemedImageSource(relativeIconPath + path, ImageDimension.id32x32);
                            
                            try
                            {
                                var capSearchValue = StringUtil.Capitalize(SearchValue);//search keyword in action from here

                                //in original code, str2 is alt name for each image file
                                string altName = StringUtil.Capitalize(Path.GetFileNameWithoutExtension(path).Replace("_", " "));

                                if (altName.Contains(capSearchValue))//if condition specific to keyword search
                                {
                                    int num3 = startcoord % 24;
                                    int num4 = startcoord / 24;

                                    //bitmap2 is the smaller icon - rendered in scrollbox
                                    using (Bitmap bitmap2 = Settings.Icons.UseZippedIcons ? new Bitmap(ZippedIcon.GetStream(relativeIconPath + path, ZippedIcon.GetZipFile(relativeIconPath))) : new Bitmap(FileUtil.MapPath(themedImageSource)))
                                        graphics.DrawImage((System.Drawing.Image)bitmap2, num3 * 40 + 4, num4 * 40 + 4, 32, 32);

                                    //in original code, str1 is calculated coords for each file
                                    string coords = string.Format("{0},{1},{2},{3}", (object)(num3 * 40 + 4), (object)(num4 * 40 + 4), (object)(num3 * 40 + 36), (object)(num4 * 40 + 36));

                                    //final area cumulatively written to htmlwriter 
                                    htmlTextWriter.WriteLine("<area shape=\"rect\" coords=\"{0}\" href=\"#\" alt=\"{1}\" sc_path=\"{2}\"/>", (object)coords, (object)altName, (object)(relativeIconPath + path));

                                    ++startcoord;
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Warn("Unable to open icon " + themedImageSource, ex, (object)typeof(SetIconForm));
                            }
                        }
                    }
                }

                htmlTextWriter.WriteLine("</map>");
                FileUtil.WriteToFile(area, htmlTextWriter.InnerWriter.ToString());//all concatenated value part of htmlwriter written to file system html
                bitmap1.Save(img, ImageFormat.Png);//img is image file name and bitmap1 holds the cumulated values of all bitmaps

            }
        }

        public override void RenderIcons()
        {
            RenderIcons(CompleteList, MapTagName);
        }

        public override void RenderIcons(Scrollbox scrollbox,string prefix)
        {
            Assert.ArgumentNotNull((object)scrollbox, nameof(scrollbox));
            Assert.ArgumentNotNullOrEmpty(prefix, nameof(prefix));
            string imgfilename = GetFilename(prefix);
            string htmlfilename = Path.ChangeExtension(imgfilename, ".html");
            //if (!File.Exists(filename) || !File.Exists(str))
            DrawIcons(prefix, imgfilename, htmlfilename);

            HtmlTextWriter output = new HtmlTextWriter((TextWriter)new StringWriter());
            output.Write(FileUtil.ReadFromFile(htmlfilename));
            WriteImageTag(imgfilename, prefix, output);
            scrollbox.InnerHtml = output.InnerWriter.ToString();
        }
    }
}
