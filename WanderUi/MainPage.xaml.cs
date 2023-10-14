using AngleSharp;
using AngleSharp.Html.Dom;
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
            await Browse(((Entry)sender).Text);
        }

        private async Task Browse(string address)
        {
            var requester = new DefaultHttpRequester();
            requester.Headers["User-Agent"] = "Wander/0.1";
            var config = Configuration.Default.With(requester).WithDefaultLoader();
            if (!address.StartsWith("https://") && !address.StartsWith("http://"))
            {
                address = "http://" + address;
            }

            if (UriEntry.Text != address)
            {
                UriEntry.Text = address;
            }

            verticalStackLayout.Children.Clear();

            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(address);

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
                "ARTICLE" or
                "SECTION" or
                "ASIDE" or
                "NAV" or
                "UL" or
                "OL" or
                "LI" or
                "FIGURE" or
                "FOOTER" or
                "H1" or
                "H2" or
                "H3" or
                "H4" or
                "H5" or
                "H6" or
                "P" or
                "SPAN")
            {
                if (element.Children.Length > 0)
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
            else if (element.NodeName is "A" or "AREA" or "LINK")
            {
                view = new Label()
                {
                    Text = element.NodeName + "\t" + element.TextContent.Trim(),
                    TextDecorations = TextDecorations.Underline,
                    TextColor = Colors.Blue
                };
                if (element.HasAttribute("href"))
                {
                    ((Label)view).GestureRecognizers.Add(
                    new TapGestureRecognizer()
                    {
                        Command = new Command(async () => await Browse(((IHtmlAnchorElement)element).Href))
                    });
                }

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
