using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader
{
    public interface ICustomComponent
    {
        Type SLTemplateModel { get; }
    }
}
