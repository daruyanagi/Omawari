using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Omawari.Utilities
{
    public class WebBrowserExtension
    {
        public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
            "Html", typeof(string), typeof(WebBrowserExtension), 
            new UIPropertyMetadata(null, HtmlPropertyChanged));

        public static string GetHtml(DependencyObject obj)
        {
            return (string)obj.GetValue(HtmlProperty);
        }

        public static void SetHtml(DependencyObject obj, string value)
        {
            obj.SetValue(HtmlProperty, value);
        }

        public static void HtmlPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            WebBrowser browser = o as WebBrowser;
            if (browser == null) return;
            string html = e.NewValue as string;
            if (html == null) return;
            html = $@"
<html>
    <head>
        <meta http-equiv='Content-Type' content='text/html;charset=UTF-8'>
        <style>
            ins.diffins {{ background-color: #cfc; text-decoration: none; }} 
            del.diffdel {{ color: #999; background-color:#FEC8C8; }}
        </style>
    </head>
    <body>
        {html}
    </body>
</html>";
            browser.NavigateToString(html);
        }
    }
}
