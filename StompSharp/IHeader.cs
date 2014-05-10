using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stomp2
{
    public interface IHeader
    {

        string Key { get; }

        object Value { get; }

    }
}
