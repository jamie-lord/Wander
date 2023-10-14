using AngleSharp;
using AngleSharp.Io;

namespace WanderUi
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void UriEntry_Completed(object sender, EventArgs e)
        {
            var requester = new DefaultHttpRequester();
            requester.Headers["User-Agent"] = "Wander/0.1";
            var config = Configuration.Default.With(requester).WithDefaultLoader();
            var address = ((Entry)sender).Text;
            if (!address.StartsWith("https://") && !address.StartsWith("http://"))
            {
                address = "http://" + address;
            }
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(address);

            verticalStackLayout.Children.Clear();

            foreach (var child in document.Body.Children)
            {
                CreateComponent(child, verticalStackLayout);
            }
        }

        private void CreateComponent(AngleSharp.Dom.IElement element, Microsoft.Maui.ILayout parent)
        {
            IView view = null;
            if (element.NodeName is "DIV" or
                "HEADER" or
                "MAIN" or
                "SECTION" or
                "ASIDE" or
                "NAV" or
                "UL" or
                "OL" or
                "LI" or
                "FIGURE" or
                "FOOTER")
            {
                if (element.HasChildNodes)
                {
                    view = new VerticalStackLayout
                    {
                        new Label()
                        {
                            Text = element.NodeName
                        }
                    };

                    foreach (var child in element.Children)
                    {
                        CreateComponent(child, (VerticalStackLayout)view);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(element.TextContent))
                {
                    view = new Label()
                    {
                        Text = element.NodeName + "\t" + element.TextContent.Trim()
                    };
                }
            }
            else if (element.NodeName is "H1" or
                "H2" or
                "H3" or
                "H4" or
                "H5" or
                "H6" or
                "P" or
                "A" or
                "SPAN")
            {
                view = new Label()
                {
                    Text = element.NodeName + "\t" + element.TextContent.Trim()
                };
            }
            else if (element.NodeName == "IMG")
            {
                view = new VerticalStackLayout() {
                    new Label()
                    {
                        Text = element.NodeName
                    },
                    new Image()
                    {
                        Source = element.GetAttribute("src")
                    }
                };
            }
            else
            {
                view = new Label()
                {
                    Text = element.NodeName + "\t" + element.NodeType.ToString(),
                    TextColor = Colors.Red
                };
            }

            parent.Add(view);
        }
    }

}
