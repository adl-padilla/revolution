using System;

namespace Revolution
{
    internal class SpacesAttribute : Attribute
    {
        public byte Value { get; set; }

        public SpacesAttribute(byte value)
        {
            this.Value = value;
        }
    }
}