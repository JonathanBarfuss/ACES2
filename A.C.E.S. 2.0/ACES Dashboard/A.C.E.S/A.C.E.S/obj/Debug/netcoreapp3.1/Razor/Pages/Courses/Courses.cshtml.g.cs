#pragma checksum "S:\My Stuff\School\CS 4450\A.C.E.S.-2.0\A.C.E.S. 2.0\ACES Dashboard\A.C.E.S\A.C.E.S\Pages\Courses\Courses.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "5e23f9ab509d329a7a39098872053504997d827b"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(A.C.E.S.Pages.Courses.Pages_Courses_Courses), @"mvc.1.0.razor-page", @"/Pages/Courses/Courses.cshtml")]
namespace A.C.E.S.Pages.Courses
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
#line 1 "S:\My Stuff\School\CS 4450\A.C.E.S.-2.0\A.C.E.S. 2.0\ACES Dashboard\A.C.E.S\A.C.E.S\Pages\_ViewImports.cshtml"
using A.C.E.S;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemMetadataAttribute("RouteTemplate", "/Courses")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"5e23f9ab509d329a7a39098872053504997d827b", @"/Pages/Courses/Courses.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"0fe3f33a1bbfb825705ae137818486ffb09d4dab", @"/Pages/_ViewImports.cshtml")]
    public class Pages_Courses_Courses : global::Microsoft.AspNetCore.Mvc.RazorPages.Page
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("asp-page", "Add", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_1 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("class", new global::Microsoft.AspNetCore.Html.HtmlString("button"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_2 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("action", new global::Microsoft.AspNetCore.Html.HtmlString("/Courses/Add"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
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
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper;
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.FormTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper;
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.RenderAtEndOfFormTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#nullable restore
#line 3 "S:\My Stuff\School\CS 4450\A.C.E.S.-2.0\A.C.E.S. 2.0\ACES Dashboard\A.C.E.S\A.C.E.S\Pages\Courses\Courses.cshtml"
  
    ViewData["Title"] = "Courses";
    ViewData["Header"] = "Courses";

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n\r\n<section id=\"top-bar\">\r\n    <div>\r\n        <input id=\"filter-course-name\" type=\"text\" placeholder=\"Search Course\" onkeyup=\"LoadList()\" />\r\n        <select id=\"filter-course-status\" onchange=\"LoadList()\">\r\n            ");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("option", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "5e23f9ab509d329a7a39098872053504997d827b5012", async() => {
                WriteLiteral("Active");
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
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("option", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "5e23f9ab509d329a7a39098872053504997d827b5982", async() => {
                WriteLiteral("Archived");
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
            WriteLiteral("\r\n        </select>\r\n    </div>\r\n    <div>\r\n        ");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("a", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "5e23f9ab509d329a7a39098872053504997d827b6998", async() => {
                WriteLiteral("+ Add Course");
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper);
            __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper.Page = (string)__tagHelperAttribute_0.Value;
            __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_0);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_1);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral(@"
    </div>
</section>
<section>
    <table class=""table"">
        <colgroup>
            <col />
            <col />
            <col />
            <col style=""width:120px;"" />
        </colgroup>
        <thead>
            <tr>
                <td>Name</td>
                <td># Sections</td>
                <td># Assignments</td>
                <td></td>
            </tr>
        </thead>
        <tbody id=""course-list2"">
        </tbody>
    </table>
</section>
<section id=""course-list"">
</section>
<aside class=""pop-up"">
    <h2>Add Course</h2>
    ");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("form", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "5e23f9ab509d329a7a39098872053504997d827b8836", async() => {
                WriteLiteral(@"
        <div><label for=""name"">Name:</label></div>
        <div><input id=""name"" name=""name"" type=""text"" /></div>
        <p>
            <input type=""submit"" value=""Add"" />
            <input type=""button"" value=""Cancel"" onclick=""$('.pop-up').animate({ width: 'toggle' });"" />
        </p>
    ");
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.FormTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper);
            __Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.RenderAtEndOfFormTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_2);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n</aside>\r\n<script>\r\n    courses = JSON.parse(\'");
#nullable restore
#line 55 "S:\My Stuff\School\CS 4450\A.C.E.S.-2.0\A.C.E.S. 2.0\ACES Dashboard\A.C.E.S\A.C.E.S\Pages\Courses\Courses.cshtml"
                     Write(Json.Serialize(Model.Courses));

#line default
#line hidden
#nullable disable
            WriteLiteral(@"');
    LoadList();

    function LoadList() {
        var count = 0;

        var cname = document.getElementById('filter-course-name').value;
        var status = document.getElementById('filter-course-status').value;

        var elem = document.getElementById('course-list2');
        elem.innerHTML = """";

        for (var i = 0; i < courses.length; i++){
            if (courses[i].archived && status == ""Active"") continue;
            if (!courses[i].archived && status == ""Archived"") continue;
            if (!courses[i].name.toLowerCase().includes(cname.toLowerCase())) continue;

            count++;

            var tr = document.createElement(""tr"");
            tr.innerHTML += `<td><a href=""/Courses/${courses[i].id}"">${courses[i].name}</a></td>`;
            tr.innerHTML += `<td>${courses[i].sections.length}</td>`;
            tr.innerHTML += `<td>${courses[i].assignments.length}</td>`;

            var td = document.createElement(""td"");
            td.classList.add(""table-butto");
            WriteLiteral(@"n"");
            td.innerHTML += `<a title=""Edit"" href=""/Courses/${courses[i].id}/Edit""><i class=""fas fa-fw fa-lg fa-pencil-alt""></i></a>`;
            if (status == ""Active"")
                td.innerHTML += `<a title=""Archive"" onclick=""ArchiveCourse(${i}, true)""><i class=""fas fa-fw fa-lg fa-trash""></i></a>`;
            else
                td.innerHTML += `<a title=""Restore"" onclick=""ArchiveCourse(${i}, false)""><i class=""fas fa-fw fa-lg fa-trash-restore""></i></a>`;
            tr.appendChild(td);
            elem.appendChild(tr);
        }
        if (count == 0) {
            elem.innerHTML += '<tr><td colspan=""4""><center>No Result.</center></td></tr>';
        }
    }

    function DisplayError(request, status, error) {
        console.log({ request, status, error });
    }

    function ArchiveCourse(i, archive) {
        courses[i].archived = archive;
        $.ajax({
            type: 'get',
            url: '/Courses',
            data: {
                handler: 'Archive',
  ");
            WriteLiteral("              id: courses[i].id,\r\n                archive: archive\r\n            },\r\n            contenttype: \"application/json\",\r\n            datatype: \"json\"\r\n        }).done(LoadList).fail(DisplayError);\r\n    }\r\n</script>");
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<A.C.E.S.Pages.Courses.CoursesModel> Html { get; private set; }
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<A.C.E.S.Pages.Courses.CoursesModel> ViewData => (global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<A.C.E.S.Pages.Courses.CoursesModel>)PageContext?.ViewData;
        public A.C.E.S.Pages.Courses.CoursesModel Model => ViewData.Model;
    }
}
#pragma warning restore 1591
