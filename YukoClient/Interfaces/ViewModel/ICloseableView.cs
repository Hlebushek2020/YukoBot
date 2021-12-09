using System;

namespace YukoClient.Interfaces.ViewModel
{
    public interface ICloseableView
    {
        Action Close { get; set; }
    }
}