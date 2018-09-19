using System;

namespace Revolution
{
    internal class PointValueAttribute : Attribute
    {
        public byte Value { get; set; }

        public PointValueAttribute(byte value)
        {
            this.Value = value;
        }
    }
}