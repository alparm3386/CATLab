using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CAT.TagHelpers
{
    [HtmlTargetElement("greeting")]
    public class GreetingTagHelper : TagHelper
    {
        public string MyProperty { get; set; } = default!;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";  // Set the tag name
            output.Content.SetContent($"Custom content with property: {MyProperty}");
        }
    }
}
