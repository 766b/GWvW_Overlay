using System;

namespace GWvW_Overlay.Annotations
{
    /// <summary>
    /// This attribute is intended to mark publicly available API which should not be removed and so is treated as used.
    /// </summary>
    [MeansImplicitUse]
    public sealed class PublicAPIAttribute : Attribute
    {
        public PublicAPIAttribute() { }
        public PublicAPIAttribute(string comment) { }
    }
}