#pragma checksum "D:\Documents\_School\2020_1_Spring\CS 4450 Software\A.C.E.S\A.C.E.S. 2.0\ACES Dashboard\A.C.E.S\A.C.E.S\Pages\Sections\Sections.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "0cf41b0bb3e188d626f39ad3c142fe4035fd75b3"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(A.C.E.S.Pages.Sections.Pages_Sections_Sections), @"mvc.1.0.razor-page", @"/Pages/Sections/Sections.cshtml")]
namespace A.C.E.S.Pages.Sections
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "D:\Documents\_School\2020_1_Spring\CS 4450 Software\A.C.E.S\A.C.E.S. 2.0\ACES Dashboard\A.C.E.S\A.C.E.S\Pages\_ViewImports.cshtml"
using A.C.E.S;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemMetadataAttribute("RouteTemplate", "/Sections")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"0cf41b0bb3e188d626f39ad3c142fe4035fd75b3", @"/Pages/Sections/Sections.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"0fe3f33a1bbfb825705ae137818486ffb09d4dab", @"/Pages/_ViewImports.cshtml")]
    public class Pages_Sections_Sections : global::Microsoft.AspNetCore.Mvc.RazorPages.Page
    {
        #line hidden
        #pragma warning disable 0649
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext;
        #pragma warning restore 0649
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
        #pragma warning disable 0169
        private string __tagHelperStringValueBuffer;
        #pragma warning restore 0169
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __backed__tagHelperScopeManager = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __tagHelperScopeManager
        {
            get
            {
                if (__backed__tagHelperScopeManager == null)
                {
                    __backed__tagHelperScopeManager = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
                }
                return __backed__tagHelperScopeManager;
            }
        }
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.OptionTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#nullable restore
#line 3 "D:\Documents\_School\2020_1_Spring\CS 4450 Software\A.C.E.S\A.C.E.S. 2.0\ACES Dashboard\A.C.E.S\A.C.E.S\Pages\Sections\Sections.cshtml"
  
    ViewData["Title"] = "Sections";

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n<section id=\"top-bar\">\r\n    <div>\r\n        <input id=\"filter-section-name\" type=\"text\" placeholder=\"Search Section\" onkeyup=\"LoadList()\" />\r\n        <select id=\"filter-section-status\" onchange=\"LoadList()\">\r\n            ");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("option", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "0cf41b0bb3e188d626f39ad3c142fe4035fd75b33641", async() => {
                WriteLiteral("Current");
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.OptionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n            ");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("option", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "0cf41b0bb3e188d626f39ad3c142fe4035fd75b34612", async() => {
                WriteLiteral("Planned");
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.OptionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n            ");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("option", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "0cf41b0bb3e188d626f39ad3c142fe4035fd75b35583", async() => {
                WriteLiteral("Past");
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.OptionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral(@"
        </select>
    </div>
    <div>
        <a class=""button"" href=""./Section/Add"">+ Add Section</a>
    </div>
</section>
<section id=""course-list"">
</section>
<script>
    function LoadList() {
        var name = document.getElementById('filter-section-name').value;
        var status = document.getElementById('filter-section-status').value;

        var elem = document.getElementById('course-list');
        elem.innerHTML = """";

        var sections = JSON.parse('");
#nullable restore
#line 30 "D:\Documents\_School\2020_1_Spring\CS 4450 Software\A.C.E.S\A.C.E.S. 2.0\ACES Dashboard\A.C.E.S\A.C.E.S\Pages\Sections\Sections.cshtml"
                              Write(Json.Serialize(Model.Sections));

#line default
#line hidden
#nullable disable
            WriteLiteral(@"');
        for (var section of sections) {
            if (section.archived) continue;
            if (!section.name.toLowerCase().includes(name.toLowerCase())) continue;

            var accordion = document.createElement('div');
            accordion.classList.add('accordion');
            accordion.innerHTML += `<div class=""accordion-header"">${section.name}</div>`;

            var content = document.createElement('div');
            content.classList.add('accordion-content');

            console.log(section);
            for (var student of section.students) {
            }
            content.innerHTML += '<div><a href=""#"">Add Student</a></div>';

            accordion.appendChild(content);
            elem.appendChild(accordion);
        }
    }

    LoadList();
</script>");
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<A.C.E.S.Pages.Sections.SectionsModel> Html { get; private set; }
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<A.C.E.S.Pages.Sections.SectionsModel> ViewData => (global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<A.C.E.S.Pages.Sections.SectionsModel>)PageContext?.ViewData;
        public A.C.E.S.Pages.Sections.SectionsModel Model => ViewData.Model;
    }
}
#pragma warning restore 1591
