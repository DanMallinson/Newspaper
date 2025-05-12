using Libraries.Outputs;

namespace Libraries.Renderers
{
    public abstract class Renderer
    {
        public abstract RenderedContent Render(NewspaperContent content);
    }
}
