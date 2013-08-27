using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ImageTools;
using ImageTools.Controls;
using ImageTools.IO;
using ImageTools.IO.Gif;
using System.Windows.Media;
using HtmlAgilityPack;

namespace DevelopersLife
{

    public partial class RandomPage : PhoneApplicationPage
    {
        public RandomPage()
        {
            InitializeComponent();
            Decoders.AddDecoder<GifDecoder>();
            var WebClient = new WebClient(); 
            WebClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(WebClient_DownloadStringCompleted);
            WebClient.DownloadStringAsync(new Uri(Constants.RandomAdress, UriKind.Absolute));
        }

        private void WebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(e.Result);

            HtmlNode BaseNode = doc.DocumentNode.SelectSingleNode(@".//*[@id='site']/div[@class='center']/div[@class='content']");

            var comment = BaseNode.SelectSingleNode(@"div/div[@class='comment']").InnerHtml;
            comment = HttpUtility.HtmlDecode(comment);
            comment = comment.Replace("  ", string.Empty);
            comment = comment.Replace("\n", string.Empty);
            comment = comment.Replace("<br>", "\n");
            Comment.Text = comment;

            Entry.Text = BaseNode.SelectSingleNode(@"div/div/a[@class='entryLink']").InnerText;

            Title.Text = BaseNode.SelectSingleNode(@"div/div[@class='code']/span[@class='value']").InnerText;
            
            var ImageUrl = BaseNode.SelectSingleNode(@"div/div[@class='image']/div[@class='gif']/img").GetAttributeValue("src", "");
            var EI = new ExtendedImage() { UriSource = new Uri(ImageUrl, UriKind.Absolute) };
            EI.LoadingCompleted += new EventHandler(EI_LoadingCompleted);

            Rating.Text = BaseNode.SelectSingleNode(@"div/div[@class='code']/span[@class='value rating bolder']").InnerText;

            CommentsCount.Text = BaseNode.SelectSingleNode(@"div/div[@class='code']/a/span[@class='value bolder']").InnerText;

            MainGrid.Visibility = Visibility.Visible;
        }

        private void EI_LoadingCompleted(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    AnimatedImage.Source = (ExtendedImage)sender;
                }
                catch (OutOfMemoryException)
                {
                    MessageBox.Show("Недостаточно памяти для отображения анимации");
                }
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Visibility = Visibility.Collapsed;
            });            
        }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            base.OnOrientationChanged(e);
            if (e.Orientation == PageOrientation.Landscape || e.Orientation == PageOrientation.LandscapeLeft || e.Orientation == PageOrientation.LandscapeRight)
                TitlePanel.Visibility = Visibility.Collapsed;
            else
                TitlePanel.Visibility = Visibility.Visible;
        }

        private void GestureListener_DragCompleted(object sender, DragCompletedGestureEventArgs e)
        {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal)
            {
                if (e.HorizontalChange < 0)
                {
                    int random = new Random().Next();
                    NavigationService.Navigate(new Uri("/RandomPage.xaml?r=" + random, UriKind.Relative));
                }
            }
        }
    }
}