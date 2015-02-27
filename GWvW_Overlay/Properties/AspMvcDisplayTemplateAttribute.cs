using System;

namespace GWvW_Overlay.Annotations
{
    /// <summary>
    /// ASP.NET MVC attribute. Indicates that a parameter is an MVC display template.
    /// Use this attribute for custom wrappers similar to 
    /// <see cref="System.Web.Mvc.Html.DisplayExtensions.DisplayForModel(HtmlHelper, String)"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class AspMvcDisplayTemplateAttribute : Attribute { }
}