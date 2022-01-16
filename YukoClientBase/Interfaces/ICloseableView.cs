using System;

namespace YukoClientBase.Interfaces
{
    public interface ICloseableView
    {
        Action Close { get; set; }
    }
}