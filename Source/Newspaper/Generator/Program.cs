using Libraries;
using Libraries.Outputs;
using Libraries.Renderers;

if (args.Length == 0 || string.IsNullOrEmpty(args[0]) || !File.Exists(args[0]))
{
    Console.WriteLine("Please specify a valid input file");
    return;
}

string paperName;
var content = GenerateNewspaper(args[0], out paperName);

var filename = $"{paperName} {DateTime.Now.ToString("yyyy-MM-dd")}.pdf";
content.Save(filename);

static RenderedContent GenerateNewspaper(string filename, out string paperName)
{
    var newspaper = Newspaper.Load(filename);
    paperName = newspaper.Name;
    var content = newspaper.GenerateContent(DateTime.Now - newspaper.Frequency);

    var renderer = new NewspaperRenderer();
    return renderer.Render(content);
}