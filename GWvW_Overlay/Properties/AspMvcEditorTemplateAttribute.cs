using System;

namespace GWvW_Overlay.Annotations
{
    /// <summary>
    /// ASP.NET MVC attribute. Indicates that a parameter is an MVC editor template.
    /// Use this attribute for custom wrappers similar to 
    /// <see cref="System.Web.Mvc.Html.EditorExtensions.EditorForModel(HtmlHelper, String)"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class AspMvcEditorTemplateAttribute : Attribute { }
}