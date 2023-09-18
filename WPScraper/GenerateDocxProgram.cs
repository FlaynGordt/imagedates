using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace WPScraper
{
    internal partial class GenerateDocxProgram
    {
        private static HashSet<string> UnknownContent = new HashSet<string>();
        private static long ImageCounter = 0;

        static void Main()
        {
            // CreateXML();
            WriteDoc();
            Console.ReadLine();
        }

        private static void WriteDoc()
        {
            List<Post> posts = new List<Post>();

            try
            {
                // load locations
                Location.Parse(@"locations.json");
                Console.WriteLine("Locations: " + Location.Locations.Count);

                //Load xml
                XDocument xdoc = XDocument.Load(@"posts.xml");
                foreach (XElement element in xdoc.Root.Elements())
                {
                    var title = element.Attribute("post_title").Value;
                    var content = element.FirstNode as XElement;
                    var date = DateTime.Parse(element.Attribute("post_date").Value);
                    var authorNumber = int.Parse(element.Attribute("post_author").Value);
                    var author = authorNumber == 3 ? "Claudia" : "Florian";
                    var dates = Post.ParseDatesFromTitel(title);

                    if (dates.Count ==0)
                    {
                        dates.Add(date);
                    }

                    var locations = Location.FindLocationsForDates(dates);

                    posts.Add(new Post() { Content = content, Dates = dates, Title = title, Author = author, Locations = locations });
                }

                using (var document = DocX.Create(@"test.docx"))
                {
                    document.InsertParagraph("Travel now - Rest later").FontSize(25).Alignment = Alignment.center;
                    document.InsertParagraph("von zwein, die eine Reise tun").FontSize(15).Alignment = Alignment.right;

                    try
                    {
                        foreach (var post in posts.Where(p => p.Dates.First() > DateTime.Parse("05.06.2022") 
                                                         //     && p.DateTime < DateTime.Parse("15.08.2022")
                                                         )
                                                  .OrderBy(p => p.Dates.First())
                                                 // .Take(10)
                                                  )
                        {
                            AppendPost(document, post);

                        }

                        document.InsertParagraph("Statistik").Heading(HeadingType.Heading2).SpacingAfter(10d);
                        document.InsertParagraph("Number of images : " + ImageCounter).FontSize(15d).Alignment = Alignment.both;
                        document.InsertParagraph("Posts : " + posts.Count).FontSize(15d).Alignment = Alignment.both;
                        document.InsertParagraph("Claudia : " + posts.Where(p => p.Author == "Claudia").Count()).FontSize(15d).Alignment = Alignment.both;
                        document.InsertParagraph("Flo : " + posts.Where(p => p.Author != "Claudia").Count()).FontSize(15d).Alignment = Alignment.both;

                    }
                    finally
                    {
                        document.Save();

                    }

                 

                    Process.Start(@"test.docx");
                }
            }
            catch (Exception)
            {

                throw;
            }


            Console.WriteLine(string.Join(",", UnknownContent));
            Console.WriteLine("Number of images : " + ImageCounter);
            Console.WriteLine("Posts : " + posts.Count);
            Console.WriteLine("Claudia : " + posts.Where(p => p.Author == "Claudia").Count());
            Console.WriteLine("Flo : " + posts.Where(p => p.Author != "Claudia").Count());
        }

        private static void AppendPost(DocX doc, Post post)
        {
            doc.InsertParagraph(post.Title).Heading(HeadingType.Heading1);

            doc.InsertParagraph(post.Author).FontSize(10).Alignment = Alignment.right;
            doc.InsertParagraph(string.Join(", ", post.Dates.Select(d => d.ToString("d")))).FontSize(10).Alignment = Alignment.right;
            doc.InsertParagraph("Locations: " + post.Locations.Count).FontSize(10).Alignment = Alignment.right;

            Console.Write(post.Title);

            foreach (var contentItem in post.Content.Elements())
            {
                Console.Write('.');
                try
                {
                    InsertNextElement(contentItem, doc);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString() + " " + contentItem.Value);
                    // das muss rein!!
                  //  throw;
                }

            }

            Console.WriteLine();
        }

    

        private static void InsertNextElement(XElement contentItem, DocX doc)
        {


            switch (contentItem.Name.ToString())
            {
                case "hr":
                    {
                        //skip
                        break;
                    }
                case "h2":
                    {
                        doc.InsertParagraph(contentItem.Value).Heading(HeadingType.Heading2).SpacingAfter(10d);
                        break;
                    }
                case "p":
                    {
                        var fixedText = contentItem.Value.Replace("\\\"", "\"").Replace("\\'", "'");
                        doc.InsertParagraph(fixedText).FontSize(15d).Alignment = Alignment.both;
                        break;
                    }
                case "pre":
                    {
                        doc.InsertParagraph(contentItem.Value).Italic();
                        break;
                    }
                case "blockquote":
                    {
                        foreach (var blockquoteElement in contentItem.Elements())
                        {
                            doc.InsertParagraph(blockquoteElement.Value).SpacingAfter(15d).Italic();
                        }
                        break;
                    }

                case "img":
                    {
                        if (!string.IsNullOrWhiteSpace(contentItem.Attribute("src")?.Value))
                        {
                            ConvertAndInsertImage(contentItem, doc);
                        }
                        break;
                    }
                case "figure":
                case "ul":
                    {
                        // children will get handled later
                        
                        break;
                    }
                default:
                    {
                        UnknownContent.Add(contentItem.Name.LocalName);
                        break;
                    }
            }

            foreach (var childElement in contentItem.Elements())
            {
                InsertNextElement(childElement, doc);

            }
        }

        private static void ConvertAndInsertImage(XElement imgElement, DocX doc)
        {
            ImageCounter++;

            var pathToImage = imgElement.Attribute("src")?.Value;
            string caption = null;

            var tempDebug = doc.InsertParagraph(pathToImage);

            if (imgElement.NextNode is XElement figCaption && figCaption?.Name == "figcaption")
            {
                caption = figCaption.Value;
                tempDebug.InsertCaptionAfterSelf(caption);
            }


            // disabled for now
            return;

            var localFileName = CacheFile(pathToImage);

            var image = doc.AddImage(localFileName);

            System.Drawing.Image img = System.Drawing.Image.FromFile(localFileName);

            // nicht breiter als x% der Seite
            var maxWidth = doc.PageWidth * 0.7;
            var scale = 1.0f;

            if (img.Width > maxWidth)
            {
                scale = (float)maxWidth / img.Width;
            }

            // nicht größer als 1/3 der Seite
            if (img.Height > doc.PageHeight / 3)
            {
                var maxheigt = doc.PageHeight / 3;

                var heightScale = maxheigt / img.Height;

                if (heightScale < scale)
                {
                    scale = heightScale;
                }
            }

            var picture = image.CreatePicture(img.Height * scale, img.Width * scale);

            if(caption != null)
            {
                picture.InsertCaptionAfterSelf(caption);
            }
            
            var p = doc.InsertParagraph().Border(new Border() { Tcbs = BorderStyle.Tcbs_thick, Color = Color.Black, Size= BorderSize.three}).Spacing(5);
            p.Alignment = Alignment.center;

            p.AppendPicture(picture);
        }

        private static string CacheFile(string original)
        {
            var localValue = original.Replace("https://wir-verreisen.de/wp-content/uploads", @"C:\privat\uploads").Replace('/', '\\');

            var macbookValue = original.Replace("https://wir-verreisen.de/wp-content/uploads", "\\\\macbook\\wp-content\\uploads").Replace('/', '\\');

            try
            {

                if (!File.Exists(macbookValue))
                {
                    macbookValue = Directory.GetFiles("\\\\macbook\\wp-content\\uploads", new Uri(macbookValue).Segments.Last(), SearchOption.AllDirectories).SingleOrDefault();

                    if (macbookValue == null)
                    {
                        throw new Exception("Cant find file: " + new Uri(localValue).Segments.Last());
                    }

                    
                }


                var localFile = new FileInfo(localValue);

                if (!localFile.Directory.Exists)
                {
                    localFile.Directory.Create();
                }

                if (!localFile.Exists)
                {
                    File.Copy(macbookValue, localFile.FullName);
                }

                return localFile.FullName;
            }
            catch (Exception ex)
            {
                Console.WriteLine(localValue);
                throw;
            }
           
        }

        private static void CreateXML()
        {
            var fileText = File.ReadAllLines(@"wpgo_posts.sql");
            var xdoc = new XDocument();

            var postsElement = new XElement("Posts");
            xdoc.Add(postsElement);

            fileText = fileText.Where(l => l.StartsWith("INSERT")).ToArray();



            var postTypeHashSet = new HashSet<string>();

            foreach (var line in fileText)
            {

                try
                {


                    //line.Substring(0, 15).Dump();

                    var dings = line.Split(new[] { ") VALUES(" }, System.StringSplitOptions.RemoveEmptyEntries);

                    var header = dings[0];


                    header = header.Substring("INSERT INTO `wpgo_posts` (".Length, header.Length - ("INSERT INTO `wpgo_posts` (".Length)).Trim();
                    header = header.Substring(1, header.Length - 2); // remove leadin ' and trailing ');
                    var headers = header.Split(new[] { "`,`" }, StringSplitOptions.None);


                    var value = dings[1];
                    value = value.Substring(1, value.Length - 4); // remove leadin ' and trailing ');
                    var values = value.Split(new[] { "','" }, StringSplitOptions.None);

                    if (values.All(v => v == "attachment"))
                    {
                        continue;
                    }

                    var postElement = new XElement("Post");
                    postsElement.Add(postElement);

                    for (int i = 0; i < headers.Length; i++)
                    {

                        if (headers[i] == "post_type")
                        {
                            if (values[i] != "post")
                            {
                                postElement.Remove(); // remove again
                                break;
                            }
                        }

                        if (headers[i] != "post_content")
                        {
                            postElement.SetAttributeValue(headers[i], values[i]);
                        }
                        else
                        {

                            HtmlDocument doc = new HtmlDocument();


                            doc.LoadHtml("<body>" + values[i] + "</body>");
                            System.IO.StringWriter sw = new System.IO.StringWriter();
                            System.Xml.XmlTextWriter xw = new System.Xml.XmlTextWriter(sw);
                            doc.Save(xw);
                            string result = sw.ToString();

                            var pimpedText = result.Replace("\\n", "");
                            pimpedText = pimpedText.Replace("\\&quot;", "");
                            //pimpedText = pimpedText.Replace("\\n", "");
                            //pimpedText = pimpedText.Replace("\\\"", "\"");
                            //pimpedText = pimpedText.Replace("&nbsp;", "&#160;");
                            //pimpedText = "<Content>" + pimpedText + "</Content>";
                            try
                            {
                                var contentElement = XElement.Parse(pimpedText);
                                contentElement.Name = "Content";
                                postElement.Add(contentElement);
                            }
                            catch (Exception e)
                            {
                                postElement.Remove(); // remove again
                                Console.WriteLine("Skipping " + values[0] + " Error: " + e.Message);
                                break;
                            }

                        }


                    }

                }
                catch (Exception ex)
                {

                    throw;
                }


            }


            xdoc.Save(@"posts.xml");
        }
    }
}
