using System;

namespace GenericSQL.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : Attribute
    {
        public string RelatedEntityName { get; set; }
        public ForeignKeyAttribute(string relatedEntityName)
        {
            RelatedEntityName = relatedEntityName;
        }
    }
}