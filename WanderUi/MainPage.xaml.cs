using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using System.Text;

namespace WanderUi
{
    public partial class MainPage : ContentPage
    {
        private const bool ShowContainerElements = false;
        private const bool ShowElementNames = false;

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
            // Block level elements.
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
                "I" or
                "B" or
                "EM" or
                "SPAN" or
                "PICTURE" or
                "TIME")
            {
                if (element.Children.Length > 0)
                {
                    view = new VerticalStackLayout();

                    if (ShowContainerElements)
                    {
                        ((VerticalStackLayout)view).Add(new Label()
                        {
                            Text = element.NodeName
                        });
                    }

                    foreach (var childNode in element.ChildNodes)
                    {
                        if (childNode.NodeType == AngleSharp.Dom.NodeType.Element)
                        {
                            CreateComponent((AngleSharp.Dom.IElement)childNode, (VerticalStackLayout)view);
                        }
                        else if (childNode.NodeType == AngleSharp.Dom.NodeType.Text && !string.IsNullOrWhiteSpace(ExceptBlanks(childNode.TextContent).Trim()))
                        {
                            ((VerticalStackLayout)view).Add(new Label()
                            {
                                Text = (ShowElementNames ? childNode.NodeName + "\t" : "") + ExceptBlanks(childNode.TextContent).Trim(),
                                FontSize = GetFontSize(childNode.NodeName),
                            });
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(ExceptBlanks(element.TextContent).Trim()))
                {
                    view = new Label()
                    {
                        Text = (ShowElementNames ? element.NodeName + "\t" : "") + ExceptBlanks(element.TextContent).Trim(),
                        FontSize = GetFontSize(element.NodeName),
                    };
                }
            }
            // Inline elements.
            else if (element.NodeName is "A" or "AREA" or "LINK")
            {
                view = new Label()
                {
                    Text = (ShowElementNames ? element.NodeName + "\t" : "") + ExceptBlanks(element.TextContent).Trim(),
                    TextDecorations = TextDecorations.Underline,
                    TextColor = Colors.Blue,
                    FontSize = GetFontSize(element.NodeName)
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
            // Image and object elements.
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
            // Elements to ignore.
            else if (element.NodeName is "NOSCRIPT" or
                "SCRIPT" or
                "BR")
            {
                // Not currently showing these elements in the UI.
            }
            // Unknown elements.
            else
            {
                view = new Label()
                {
                    Text = element.NodeName + "\t" + element.NodeType.ToString(),
                    TextColor = Colors.Red
                };
            }

            if (view != null)
            {
                parent.Add(view);
            }
        }

        private static double GetFontSize(string nodeName)
        {
            const double standardFontSize = 16;
            return nodeName switch
            {
                "H1" => standardFontSize * 3,
                "H2" => standardFontSize * 2.8,
                "H3" => standardFontSize * 2.6,
                "H4" => standardFontSize * 2.4,
                "H5" => standardFontSize * 2.2,
                "H6" => standardFontSize * 2,
                _ => standardFontSize,
            };
        }

        private static string ExceptBlanks(string str)
        {
            StringBuilder sb = new(str.Length);
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                switch (c)
                {
                    case '\r':
                    case '\n':
                    case '\t':
                        continue;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
